using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using MaterialiGestioneWeb.Data;
using MaterialiGestioneWeb.Models;

namespace MaterialiGestioneWeb.Services
{
    public class PersonaleRepository
    {
        public IList<PersonaleLookupItem> GetPersonale(bool isEsterno, bool includeNonAttivi)
        {
            var sql = isEsterno
                ? "SELECT id_pe AS Id, nome, cognome, attivo FROM dbo.pe_elencopersonaleesterno"
                    + (includeNonAttivi ? string.Empty : " WHERE attivo = 1")
                    + " ORDER BY cognome, nome;"
                : "SELECT idpersonale AS Id, nome, cognome, stato_servizio FROM dbo.elencopersonale"
                    + (includeNonAttivi ? string.Empty : " WHERE stato_servizio = 'attivo'")
                    + " ORDER BY cognome, nome;";

            var results = new List<PersonaleLookupItem>();
            using (var connection = DipendentiDb.OpenConnection())
            using (var command = new SqlCommand(sql, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    results.Add(new PersonaleLookupItem
                    {
                        Id = Convert.ToInt32(reader["Id"], CultureInfo.InvariantCulture),
                        Nome = reader["nome"] == DBNull.Value ? string.Empty : Convert.ToString(reader["nome"], CultureInfo.InvariantCulture),
                        Cognome = reader["cognome"] == DBNull.Value ? string.Empty : Convert.ToString(reader["cognome"], CultureInfo.InvariantCulture),
                        IsEsterno = isEsterno,
                        IsAttivo = isEsterno
                            ? reader["attivo"] != DBNull.Value && Convert.ToInt32(reader["attivo"], CultureInfo.InvariantCulture) == 1
                            : string.Equals(
                                reader["stato_servizio"] == DBNull.Value ? string.Empty : Convert.ToString(reader["stato_servizio"], CultureInfo.InvariantCulture),
                                "attivo",
                                System.StringComparison.OrdinalIgnoreCase)
                    });
                }
            }

            return results;
        }

        public IDictionary<int, string> GetDisplayNames(IEnumerable<int?> ids)
        {
            var result = new Dictionary<int, string>();
            var idList = ids == null ? new List<int>() : ids.Where(i => i.HasValue).Select(i => i.Value).Distinct().ToList();
            if (idList.Count == 0)
            {
                return result;
            }

            FillNames(result, idList, false);
            var missing = idList.Where(i => !result.ContainsKey(i)).ToList();
            if (missing.Count > 0)
            {
                FillNames(result, missing, true);
            }
            return result;
        }

        private static void FillNames(IDictionary<int, string> target, IList<int> ids, bool isEsterno)
        {
            using (var connection = DipendentiDb.OpenConnection())
            using (var command = connection.CreateCommand())
            {
                var parameterNames = new List<string>();
                for (var index = 0; index < ids.Count; index++)
                {
                    var parameterName = "@p" + index.ToString(CultureInfo.InvariantCulture);
                    parameterNames.Add(parameterName);
                    command.Parameters.Add(parameterName, SqlDbType.Int).Value = ids[index];
                }

                command.CommandText = isEsterno
                    ? "SELECT id_pe AS Id, nome, cognome FROM dbo.pe_elencopersonaleesterno WHERE id_pe IN (" + string.Join(",", parameterNames.ToArray()) + ");"
                    : "SELECT idpersonale AS Id, nome, cognome FROM dbo.elencopersonale WHERE idpersonale IN (" + string.Join(",", parameterNames.ToArray()) + ");";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var id = Convert.ToInt32(reader["Id"], CultureInfo.InvariantCulture);
                        var nome = reader["nome"] == DBNull.Value ? string.Empty : Convert.ToString(reader["nome"], CultureInfo.InvariantCulture);
                        var cognome = reader["cognome"] == DBNull.Value ? string.Empty : Convert.ToString(reader["cognome"], CultureInfo.InvariantCulture);
                        target[id] = BuildDisplayName(cognome, nome, isEsterno, true);
                    }
                }
            }
        }

        private static string BuildDisplayName(string cognome, string nome, bool isEsterno, bool isAttivo)
        {
            var displayName = (cognome + " " + nome).Trim();
            if (string.IsNullOrWhiteSpace(displayName))
            {
                displayName = isEsterno ? "Esterno senza nominativo" : "Interno senza nominativo";
            }

            return isAttivo ? displayName : displayName + " [non attivo]";
        }
    }
}
