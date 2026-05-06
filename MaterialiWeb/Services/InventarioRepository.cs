using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using MaterialiGestioneWeb.Data;
using MaterialiGestioneWeb.Infrastructure;
using MaterialiGestioneWeb.Models;

namespace MaterialiGestioneWeb.Services
{
    public class InventarioRepository
    {
        private readonly PersonaleRepository _personaleRepository = new PersonaleRepository();

        public DashboardStats GetDashboardStats()
        {
            const string sql = @"
SELECT
    (SELECT COUNT(*) FROM dbo.Prodotti) AS TotaleProdotti,
    (SELECT COUNT(*) FROM dbo.ProdPers) AS TotaleAssegnati,
    (SELECT COUNT(*) FROM dbo.Prodotti p WHERE NOT EXISTS (SELECT 1 FROM dbo.ProdPers pp WHERE pp.IdProdotto = p.IdProdotto)) AS TotaleSenzaAssegnazione,
    (SELECT COUNT(DISTINCT idProdotto) FROM dbo.NetworkData) AS TotaleConRete,
    (SELECT COUNT(DISTINCT idProdotto) FROM dbo.Postazione) AS TotaleConPostazione,
    (SELECT COUNT(*) FROM dbo.Prodotti WHERE Versamento IS NOT NULL AND LTRIM(RTRIM(Versamento)) <> '') AS TotaleDaVersare,
    (SELECT COUNT(*) FROM dbo.Prodotti WHERE idStanza IS NULL) AS TotaleFuoriStanza;";

            using (var connection = Db.OpenConnection())
            using (var command = new SqlCommand(sql, connection))
            using (var reader = command.ExecuteReader())
            {
                reader.Read();
                return new DashboardStats
                {
                    TotaleProdotti = reader.GetInt32Value("TotaleProdotti"),
                    TotaleAssegnati = reader.GetInt32Value("TotaleAssegnati"),
                    TotaleSenzaAssegnazione = reader.GetInt32Value("TotaleSenzaAssegnazione"),
                    TotaleConRete = reader.GetInt32Value("TotaleConRete"),
                    TotaleConPostazione = reader.GetInt32Value("TotaleConPostazione"),
                    TotaleDaVersare = reader.GetInt32Value("TotaleDaVersare"),
                    TotaleFuoriStanza = reader.GetInt32Value("TotaleFuoriStanza")
                };
            }
        }

        public IList<ProdottoCorrente> GetProdotti(string search)
        {
            var results = new List<ProdottoCorrente>();
            using (var connection = Db.OpenConnection())
            using (var command = new SqlCommand(BuildProdottiQuery(includeOnlyNetworkItems: false), connection))
            {
                command.Parameters.Add("@Search", SqlDbType.NVarChar, 200).Value = string.IsNullOrWhiteSpace(search) ? (object)DBNull.Value : search.Trim();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(MapProdotto(reader));
                    }
                }
            }

            ResolveAssegnatari(results);
            return results;
        }

        public IList<ProdottoCorrente> GetProdottiAssegnati(int idPersonale)
        {
            var results = new List<ProdottoCorrente>();
            using (var connection = Db.OpenConnection())
            using (var command = new SqlCommand(BuildProdottiQuery(includeOnlyNetworkItems: false, filterByPersonale: true), connection))
            {
                command.Parameters.Add("@Search", SqlDbType.NVarChar, 200).Value = DBNull.Value;
                command.Parameters.Add("@IdPersonale", SqlDbType.Int).Value = idPersonale;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(MapProdotto(reader));
                    }
                }
            }

            ResolveAssegnatari(results);
            return results;
        }

        public IList<StoricoAssegnazioneConsultazioneItem> GetStoricoAssegnazioni(string categoricoFilter, int? idProdotto, int? idPersonale)
        {
            var results = new List<StoricoAssegnazioneConsultazioneItem>();
            using (var connection = Db.OpenConnection())
            using (var command = new SqlCommand(@"
SELECT
    s.id,
    s.idProdotto,
    p.Categorico,
    oo.descrizioneProdotto,
    p.Matricola,
    s.idPersonale,
    s.dataAssegnazione,
    s.dataRestituzione,
    s.numeroStanza,
    s.livelloEfficienza,
    s.nomeMacchina,
    s.serialNumber,
    s.noteProdotto
FROM dbo.ProdPersStorico s
LEFT JOIN dbo.Prodotti p ON s.idProdotto = p.IdProdotto
LEFT JOIN dbo.OggettoOrdinativo oo ON p.idOggOrdinativo = oo.idOggOrdinativo
WHERE
    (@Categorico IS NULL OR CONVERT(nvarchar(50), p.Categorico) LIKE @Categorico + '%')
    AND (@IdProdotto IS NULL OR s.idProdotto = @IdProdotto)
    AND (@IdPersonale IS NULL OR s.idPersonale = @IdPersonale)
ORDER BY s.dataAssegnazione DESC, s.id DESC;", connection))
            {
                command.Parameters.Add("@Categorico", SqlDbType.NVarChar, 50).Value = string.IsNullOrWhiteSpace(categoricoFilter) ? (object)DBNull.Value : categoricoFilter.Trim();
                command.Parameters.Add("@IdProdotto", SqlDbType.Int).Value = NullableDb(idProdotto);
                command.Parameters.Add("@IdPersonale", SqlDbType.Int).Value = NullableDb(idPersonale);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new StoricoAssegnazioneConsultazioneItem
                        {
                            Id = reader.GetInt32Value("id"),
                            IdProdotto = reader.GetNullableInt32("idProdotto"),
                            Categorico = reader.GetNullableInt32("Categorico"),
                            DescrizioneProdotto = reader.GetStringOrEmpty("descrizioneProdotto"),
                            Matricola = reader.GetStringOrEmpty("Matricola"),
                            IdPersonale = reader.GetNullableInt32("idPersonale"),
                            DataAssegnazione = reader["dataAssegnazione"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["dataAssegnazione"], CultureInfo.InvariantCulture),
                            DataRestituzione = reader["dataRestituzione"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["dataRestituzione"], CultureInfo.InvariantCulture),
                            NumeroStanza = reader.GetStringOrEmpty("numeroStanza"),
                            LivelloEfficienza = reader.GetStringOrEmpty("livelloEfficienza"),
                            NomeMacchina = reader.GetStringOrEmpty("nomeMacchina"),
                            SerialNumber = reader.GetStringOrEmpty("serialNumber"),
                            NoteProdotto = reader.GetStringOrEmpty("noteProdotto")
                        });
                    }
                }
            }

            var names = _personaleRepository.GetDisplayNames(results.Select(item => item.IdPersonale));
            foreach (var item in results)
            {
                if (item.IdPersonale.HasValue && names.ContainsKey(item.IdPersonale.Value))
                {
                    item.AssegnatarioDisplay = names[item.IdPersonale.Value];
                }
            }

            return results;
        }

        public IList<ComputerCorrente> GetComputer(string search)
        {
            var results = new List<ComputerCorrente>();
            using (var connection = Db.OpenConnection())
            using (var command = new SqlCommand(BuildProdottiQuery(includeOnlyNetworkItems: true), connection))
            {
                command.Parameters.Add("@Search", SqlDbType.NVarChar, 200).Value = string.IsNullOrWhiteSpace(search) ? (object)DBNull.Value : search.Trim();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        results.Add(new ComputerCorrente
                        {
                            IdProdotto = reader.GetInt32Value("IdProdotto"),
                            Categorico = reader.GetNullableInt32("Categorico"),
                            Matricola = reader.GetStringOrEmpty("Matricola"),
                            Categoria = reader.GetStringOrEmpty("Categoria"),
                            DescrizioneProdotto = reader.GetStringOrEmpty("DescrizioneProdotto"),
                            Modello = reader.GetStringOrEmpty("Modello"),
                            NomeMacchina = reader.GetStringOrEmpty("NomeMacchina"),
                            MacAddress = reader.GetStringOrEmpty("MacAddress"),
                            NumeroStanza = reader.GetStringOrEmpty("NumeroStanza"),
                            LivelloEfficienza = reader.GetStringOrEmpty("LivelloEfficienza"),
                            IdPersonale = reader.GetNullableInt32("IdPersonale")
                        });
                    }
                }
            }

            var names = _personaleRepository.GetDisplayNames(results.Select(item => item.IdPersonale));
            foreach (var item in results)
            {
                if (item.IdPersonale.HasValue && names.ContainsKey(item.IdPersonale.Value))
                {
                    item.AssegnatarioDisplay = names[item.IdPersonale.Value];
                }
            }

            return results;
        }

        public ProdottoDettaglio GetProdottoDettaglio(int idProdotto)
        {
            var dettaglio = new ProdottoDettaglio();
            using (var connection = Db.OpenConnection())
            {
                using (var command = new SqlCommand(BuildDettaglioQuery(), connection))
                {
                    command.Parameters.Add("@IdProdotto", SqlDbType.Int).Value = idProdotto;
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            dettaglio.Prodotto = MapProdotto(reader);
                            dettaglio.Ordinativo = new OrdinativoInfo
                            {
                                IdOrdinativo = reader.GetNullableInt32("IdOrdinativo"),
                                CodiceOrdinativo = reader.GetStringOrEmpty("CodiceOrdinativo"),
                                DenominazioneOrdinativo = reader.GetStringOrEmpty("DenominazioneOrdinativo"),
                                EnteStipulante = reader.GetStringOrEmpty("EnteStipulante"),
                                EstremiOrdinativo = reader.GetStringOrEmpty("EstremiOrdinativo"),
                                DittaOrdinativo = reader.GetStringOrEmpty("DittaOrdinativo")
                            };
                        }
                    }
                }

                using (var command = new SqlCommand(@"
SELECT dataAssegnazione, dataRestituzione, idPersonale, numeroStanza, livelloEfficienza, nomeMacchina, serialNumber
FROM dbo.ProdPersStorico
WHERE idProdotto = @IdProdotto
ORDER BY dataAssegnazione DESC, id DESC;", connection))
                {
                    command.Parameters.Add("@IdProdotto", SqlDbType.Int).Value = idProdotto;
                    using (var reader = command.ExecuteReader())
                    {
                        var items = new List<(StoricoItem Item, int? IdPersonale)>();
                        while (reader.Read())
                        {
                            var idPersonale = reader["idPersonale"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["idPersonale"], CultureInfo.InvariantCulture);
                            items.Add((new StoricoItem
                            {
                                DataInizio = reader["dataAssegnazione"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["dataAssegnazione"], CultureInfo.InvariantCulture),
                                DataFine = reader["dataRestituzione"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["dataRestituzione"], CultureInfo.InvariantCulture),
                                Valore = BuildStoricoValue(
                                    reader["numeroStanza"] == DBNull.Value ? string.Empty : Convert.ToString(reader["numeroStanza"], CultureInfo.InvariantCulture),
                                    reader["livelloEfficienza"] == DBNull.Value ? string.Empty : Convert.ToString(reader["livelloEfficienza"], CultureInfo.InvariantCulture),
                                    reader["nomeMacchina"] == DBNull.Value ? string.Empty : Convert.ToString(reader["nomeMacchina"], CultureInfo.InvariantCulture),
                                    reader["serialNumber"] == DBNull.Value ? string.Empty : Convert.ToString(reader["serialNumber"], CultureInfo.InvariantCulture)),
                                Note = string.Empty
                            }, idPersonale));
                        }

                        var names = _personaleRepository.GetDisplayNames(items.Select(i => i.IdPersonale));
                        foreach (var item in items)
                        {
                            if (item.IdPersonale.HasValue && names.ContainsKey(item.IdPersonale.Value))
                            {
                                item.Item.Note = names[item.IdPersonale.Value];
                            }

                            dettaglio.StoricoAssegnazioni.Add(item.Item);
                        }
                    }
                }

            }

            if (dettaglio.Prodotto != null)
            {
                ResolveAssegnatari(new[] { dettaglio.Prodotto });
            }

            return dettaglio;
        }

        public DominiViewModel GetDomini()
        {
            return new DominiViewModel
            {
                Categorie = GetCategoriaItems(),
                LivelliEfficienza = GetLivelloItems(),
                Stanze = GetStanzaItems(),
                Ditte = GetDittaItems(),
                TipiOggettoOrdinativo = GetTipoOggettoItems()
            };
        }

        public IList<LookupItem> GetProdottiLookup()
        {
            const string sql = @"
SELECT p.IdProdotto, p.Categorico, oo.descrizioneProdotto, p.Matricola
FROM dbo.Prodotti p
LEFT JOIN dbo.OggettoOrdinativo oo ON p.idOggOrdinativo = oo.idOggOrdinativo
ORDER BY p.Categorico, oo.descrizioneProdotto, p.Matricola;";

            return ReadLookup(sql, reader => new LookupItem
            {
                Id = reader.GetInt32Value("IdProdotto"),
                Codice = reader["Categorico"] == DBNull.Value ? string.Empty : Convert.ToString(reader["Categorico"], CultureInfo.InvariantCulture),
                Nome = string.Join(" - ", new[]
                {
                    reader.GetStringOrEmpty("descrizioneProdotto"),
                    reader.GetStringOrEmpty("Matricola")
                }).Trim(' ', '-')
            });
        }

        public IList<LookupItem> GetProdottiLookupByCategorico(string categoricoFilter)
        {
            const string sql = @"
SELECT p.IdProdotto, p.Categorico, oo.descrizioneProdotto, p.Matricola
FROM dbo.Prodotti p
LEFT JOIN dbo.OggettoOrdinativo oo ON p.idOggOrdinativo = oo.idOggOrdinativo
WHERE @Categorico IS NULL OR CONVERT(nvarchar(50), p.Categorico) LIKE @Categorico + '%'
ORDER BY p.Categorico, oo.descrizioneProdotto, p.Matricola;";

            using (var connection = Db.OpenConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@Categorico", SqlDbType.NVarChar, 50).Value = string.IsNullOrWhiteSpace(categoricoFilter) ? (object)DBNull.Value : categoricoFilter.Trim();
                using (var reader = command.ExecuteReader())
                {
                    var results = new List<LookupItem>();
                    while (reader.Read())
                    {
                        results.Add(new LookupItem
                        {
                            Id = reader.GetInt32Value("IdProdotto"),
                            Codice = reader["Categorico"] == DBNull.Value ? string.Empty : Convert.ToString(reader["Categorico"], CultureInfo.InvariantCulture),
                            Nome = string.Join(" - ", new[]
                            {
                                reader.GetStringOrEmpty("descrizioneProdotto"),
                                reader.GetStringOrEmpty("Matricola")
                            }).Trim(' ', '-')
                        });
                    }

                    return results;
                }
            }
        }

        public IList<LookupItem> GetProdottiNetworkLookup()
        {
            const string sql = @"
SELECT p.IdProdotto, p.Categorico, oo.descrizioneProdotto, p.Matricola
FROM dbo.Prodotti p
LEFT JOIN dbo.OggettoOrdinativo oo ON p.idOggOrdinativo = oo.idOggOrdinativo
LEFT JOIN dbo.CategoriaProdotti c ON oo.idCategProdotti = c.IdCategoria
WHERE ISNULL(c.ethernet, 0) = 1
ORDER BY p.Categorico, oo.descrizioneProdotto, p.Matricola;";

            return ReadLookup(sql, reader => new LookupItem
            {
                Id = reader.GetInt32Value("IdProdotto"),
                Codice = reader["Categorico"] == DBNull.Value ? string.Empty : Convert.ToString(reader["Categorico"], CultureInfo.InvariantCulture),
                Nome = string.Join(" - ", new[]
                {
                    reader.GetStringOrEmpty("descrizioneProdotto"),
                    reader.GetStringOrEmpty("Matricola")
                }).Trim(' ', '-')
            });
        }

        public IList<LookupItem> GetNomiMacchinaLookup()
        {
            return ReadLookup(@"
SELECT idnomemacchina, NomeMacchina
FROM dbo.NomeMacchina
ORDER BY NomeMacchina;", reader => new LookupItem
            {
                Id = reader.GetInt32Value("idnomemacchina"),
                Nome = reader.GetStringOrEmpty("NomeMacchina")
            });
        }

        public int GetNextCategorico()
        {
            using (var connection = Db.OpenConnection())
            using (var command = new SqlCommand("SELECT ISNULL(MAX(Categorico), 0) + 1 FROM dbo.Prodotti;", connection))
            {
                return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
        }

        public IList<LookupItem> GetStanzeLookup()
        {
            return ReadLookup("SELECT idstanza, numero FROM dbo.Stanze ORDER BY numero;", reader => new LookupItem
            {
                Id = reader.GetInt32Value("idstanza"),
                Nome = reader.GetStringOrEmpty("numero")
            });
        }

        public IList<LookupItem> GetLivelliEfficienzaLookup()
        {
            return ReadLookup("SELECT IdEfficienza, Codice, Livello_efficienza FROM dbo.LivelliEfficenza ORDER BY Livello_efficienza;", reader => new LookupItem
            {
                Id = reader.GetInt32Value("IdEfficienza"),
                Codice = reader.GetStringOrEmpty("Codice"),
                Nome = reader.GetStringOrEmpty("Livello_efficienza")
            });
        }

        public IList<LookupItem> GetOggettiOrdinativoLookup()
        {
            return GetOggettiOrdinativoLookup(null);
        }

        public IList<LookupItem> GetOggettiOrdinativoLookup(int? idOrdinativo)
        {
            const string sql = @"
SELECT oo.idOggOrdinativo, oo.descrizioneProdotto, oo.modello, o.denominazioneOrdinativo, d.Nome AS DittaDescrizione, c.Descrizione AS CategoriaDescrizione
FROM dbo.OggettoOrdinativo oo
LEFT JOIN dbo.Ordinativo o ON oo.idOrdinativo = o.idOrdinativo
LEFT JOIN dbo.Ditte d ON oo.idDittaCostruttrice = d.IdDitta
LEFT JOIN dbo.CategoriaProdotti c ON oo.idCategProdotti = c.IdCategoria
WHERE @IdOrdinativo IS NULL OR oo.idOrdinativo = @IdOrdinativo
ORDER BY o.denominazioneOrdinativo, oo.descrizioneProdotto, oo.modello;";

            using (var connection = Db.OpenConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@IdOrdinativo", SqlDbType.Int).Value = NullableDb(idOrdinativo);
                using (var reader = command.ExecuteReader())
                {
                    var results = new List<LookupItem>();
                    while (reader.Read())
                    {
                        results.Add(new LookupItem
                        {
                            Id = reader.GetInt32Value("idOggOrdinativo"),
                            Nome = string.Join(" - ", new[]
                            {
                                reader.GetStringOrEmpty("denominazioneOrdinativo"),
                                reader.GetStringOrEmpty("descrizioneProdotto"),
                                reader.GetStringOrEmpty("modello"),
                                reader.GetStringOrEmpty("DittaDescrizione"),
                                reader.GetStringOrEmpty("CategoriaDescrizione")
                            }).Trim(' ', '-')
                        });
                    }

                    return results;
                }
            }
        }

        public IList<LookupItem> GetOrdinativiLookup()
        {
            return ReadLookup(@"
SELECT idOrdinativo, CodiceOrdinativo, denominazioneOrdinativo
FROM dbo.Ordinativo
ORDER BY denominazioneOrdinativo, CodiceOrdinativo;", reader => new LookupItem
            {
                Id = reader.GetInt32Value("idOrdinativo"),
                Nome = string.Join(" - ", new[] { reader.GetStringOrEmpty("denominazioneOrdinativo"), reader.GetStringOrEmpty("CodiceOrdinativo") }).Trim(' ', '-')
            });
        }

        public string GetNextCodiceOrdinativo()
        {
            var year = DateTime.Today.Year.ToString(CultureInfo.InvariantCulture);
            using (var connection = Db.OpenConnection())
            using (var command = new SqlCommand(@"
SELECT CodiceOrdinativo
FROM dbo.Ordinativo
WHERE CodiceOrdinativo LIKE @Prefix + '/%';", connection))
            {
                command.Parameters.Add("@Prefix", SqlDbType.VarChar, 4).Value = year;
                var max = 0;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var code = reader.GetStringOrEmpty("CodiceOrdinativo");
                        var slashIndex = code.IndexOf('/');
                        int parsed;
                        if (slashIndex >= 0 && int.TryParse(code.Substring(slashIndex + 1), NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) && parsed > max)
                        {
                            max = parsed;
                        }
                    }
                }

                var next = max + 1;
                return year + "/" + next.ToString(CultureInfo.InvariantCulture);
            }
        }

        public IList<LookupItem> GetCategorieLookup()
        {
            return ReadLookup(@"
SELECT IdCategoria, Descrizione, ethernet
FROM dbo.CategoriaProdotti
ORDER BY Descrizione;", reader => new LookupItem
            {
                Id = reader.GetInt32Value("IdCategoria"),
                Nome = reader.GetStringOrEmpty("Descrizione"),
                Flag = reader["ethernet"] != DBNull.Value && Convert.ToInt32(reader["ethernet"], CultureInfo.InvariantCulture) == 1
            });
        }

        public IList<LookupItem> GetDitteLookup()
        {
            return ReadLookup("SELECT IdDitta, Nome FROM dbo.Ditte ORDER BY Nome;", reader => new LookupItem
            {
                Id = reader.GetInt32Value("IdDitta"),
                Nome = reader.GetStringOrEmpty("Nome")
            });
        }

        public IList<LookupItem> GetTipiOggettoOrdinativoLookup()
        {
            return ReadLookup("SELECT idTipoOggOrdinativo, Descrizione FROM dbo.TipoOggettoOrdinativo ORDER BY Descrizione;", reader => new LookupItem
            {
                Id = reader.GetInt32Value("idTipoOggOrdinativo"),
                Nome = reader.GetStringOrEmpty("Descrizione")
            });
        }

        public IList<ProdottoAdminItem> GetProdottiAdmin()
        {
            using (var connection = Db.OpenConnection())
            using (var command = new SqlCommand(@"
SELECT
    p.IdProdotto,
    p.idStanza,
    oo.idOrdinativo,
    p.idOggOrdinativo,
    p.Categorico,
    p.Matricola,
    p.IdEfficienza,
    p.DataUltimaMov,
    CAST(p.Note AS nvarchar(max)) AS Note,
    p.Versamento,
    s.numero AS StanzaDescrizione,
    o.denominazioneOrdinativo AS OrdinativoDescrizione,
    oo.descrizioneProdotto AS OggettoDescrizione,
    le.Livello_efficienza AS EfficienzaDescrizione
FROM dbo.Prodotti p
LEFT JOIN dbo.Stanze s ON p.idStanza = s.idstanza
LEFT JOIN dbo.OggettoOrdinativo oo ON p.idOggOrdinativo = oo.idOggOrdinativo
LEFT JOIN dbo.Ordinativo o ON oo.idOrdinativo = o.idOrdinativo
LEFT JOIN dbo.LivelliEfficenza le ON p.IdEfficienza = le.IdEfficienza
ORDER BY p.IdProdotto DESC;", connection))
            using (var reader = command.ExecuteReader())
            {
                var results = new List<ProdottoAdminItem>();
                while (reader.Read())
                {
                    results.Add(new ProdottoAdminItem
                    {
                        IdProdotto = reader.GetInt32Value("IdProdotto"),
                        IdStanza = reader["idStanza"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["idStanza"], CultureInfo.InvariantCulture),
                        IdOrdinativo = reader["idOrdinativo"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["idOrdinativo"], CultureInfo.InvariantCulture),
                        IdOggOrdinativo = reader["idOggOrdinativo"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["idOggOrdinativo"], CultureInfo.InvariantCulture),
                        Categorico = reader["Categorico"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["Categorico"], CultureInfo.InvariantCulture),
                        Matricola = reader.GetStringOrEmpty("Matricola"),
                        IdEfficienza = reader["IdEfficienza"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["IdEfficienza"], CultureInfo.InvariantCulture),
                        DataUltimaMov = reader["DataUltimaMov"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DataUltimaMov"], CultureInfo.InvariantCulture),
                        Note = reader.GetStringOrEmpty("Note"),
                        Versamento = reader.GetStringOrEmpty("Versamento"),
                        StanzaDescrizione = reader.GetStringOrEmpty("StanzaDescrizione"),
                        OrdinativoDescrizione = reader.GetStringOrEmpty("OrdinativoDescrizione"),
                        OggettoDescrizione = reader.GetStringOrEmpty("OggettoDescrizione"),
                        EfficienzaDescrizione = reader.GetStringOrEmpty("EfficienzaDescrizione")
                    });
                }

                return results;
            }
        }

        public IList<OrdinativoAdminItem> GetOrdinativiAdmin()
        {
            using (var connection = Db.OpenConnection())
            using (var command = new SqlCommand(@"
SELECT
    o.idOrdinativo,
    o.CodiceOrdinativo,
    o.denominazioneOrdinativo,
    o.EF,
    o.tipoOrdinativo,
    o.idDittaOrdinativo,
    o.enteStipulante,
    o.estremiOrdinativo,
    d.Nome AS DittaDescrizione
FROM dbo.Ordinativo o
LEFT JOIN dbo.Ditte d ON o.idDittaOrdinativo = d.IdDitta
ORDER BY o.idOrdinativo DESC;", connection))
            using (var reader = command.ExecuteReader())
            {
                var results = new List<OrdinativoAdminItem>();
                while (reader.Read())
                {
                    results.Add(new OrdinativoAdminItem
                    {
                        IdOrdinativo = reader.GetInt32Value("idOrdinativo"),
                        CodiceOrdinativo = reader.GetStringOrEmpty("CodiceOrdinativo"),
                        DenominazioneOrdinativo = reader.GetStringOrEmpty("denominazioneOrdinativo"),
                        EF = reader.GetStringOrEmpty("EF"),
                        TipoOrdinativo = reader.GetStringOrEmpty("tipoOrdinativo"),
                        IdDittaOrdinativo = reader["idDittaOrdinativo"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["idDittaOrdinativo"], CultureInfo.InvariantCulture),
                        EnteStipulante = reader.GetStringOrEmpty("enteStipulante"),
                        EstremiOrdinativo = reader.GetStringOrEmpty("estremiOrdinativo"),
                        DittaDescrizione = reader.GetStringOrEmpty("DittaDescrizione")
                    });
                }

                return results;
            }
        }

        public IList<OggettoOrdinativoAdminItem> GetOggettiOrdinativoAdmin()
        {
            using (var connection = Db.OpenConnection())
            using (var command = new SqlCommand(@"
SELECT
    oo.idOggOrdinativo,
    oo.idOrdinativo,
    oo.descrizioneProdotto,
    oo.idDittaCostruttrice,
    oo.modello,
    oo.NUC,
    oo.prezzoUnitarioNetto,
    oo.prezzoInventario,
    oo.idCategProdotti,
    o.denominazioneOrdinativo AS OrdinativoDescrizione,
    d.Nome AS DittaDescrizione,
    c.Descrizione AS CategoriaDescrizione
FROM dbo.OggettoOrdinativo oo
LEFT JOIN dbo.Ordinativo o ON oo.idOrdinativo = o.idOrdinativo
LEFT JOIN dbo.Ditte d ON oo.idDittaCostruttrice = d.IdDitta
LEFT JOIN dbo.CategoriaProdotti c ON oo.idCategProdotti = c.IdCategoria
ORDER BY oo.idOggOrdinativo DESC;", connection))
            using (var reader = command.ExecuteReader())
            {
                var results = new List<OggettoOrdinativoAdminItem>();
                while (reader.Read())
                {
                    results.Add(new OggettoOrdinativoAdminItem
                    {
                        IdOggOrdinativo = reader.GetInt32Value("idOggOrdinativo"),
                        IdOrdinativo = reader["idOrdinativo"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["idOrdinativo"], CultureInfo.InvariantCulture),
                        DescrizioneProdotto = reader.GetStringOrEmpty("descrizioneProdotto"),
                        IdDittaCostruttrice = reader["idDittaCostruttrice"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["idDittaCostruttrice"], CultureInfo.InvariantCulture),
                        Modello = reader.GetStringOrEmpty("modello"),
                        NUC = reader.GetStringOrEmpty("NUC"),
                        PrezzoUnitarioNetto = reader["prezzoUnitarioNetto"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["prezzoUnitarioNetto"], CultureInfo.InvariantCulture),
                        PrezzoInventario = reader["prezzoInventario"] == DBNull.Value ? 0m : Convert.ToDecimal(reader["prezzoInventario"], CultureInfo.InvariantCulture),
                        IdCategProdotti = reader["idCategProdotti"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["idCategProdotti"], CultureInfo.InvariantCulture),
                        OrdinativoDescrizione = reader.GetStringOrEmpty("OrdinativoDescrizione"),
                        DittaDescrizione = reader.GetStringOrEmpty("DittaDescrizione"),
                        CategoriaDescrizione = reader.GetStringOrEmpty("CategoriaDescrizione")
                    });
                }

                return results;
            }
        }

        public IList<NetworkDataAdminItem> GetNetworkDataAdmin()
        {
            using (var connection = Db.OpenConnection())
            using (var command = new SqlCommand(@"
SELECT
    nd.idNetworkData,
    nd.idProdotto,
    nd.macaddress,
    nd.note,
    p.Categorico,
    oo.descrizioneProdotto,
    p.Matricola
FROM dbo.NetworkData nd
INNER JOIN dbo.Prodotti p ON nd.idProdotto = p.IdProdotto
LEFT JOIN dbo.OggettoOrdinativo oo ON p.idOggOrdinativo = oo.idOggOrdinativo
ORDER BY nd.idNetworkData DESC;", connection))
            using (var reader = command.ExecuteReader())
            {
                var results = new List<NetworkDataAdminItem>();
                while (reader.Read())
                {
                    results.Add(new NetworkDataAdminItem
                    {
                        IdNetworkData = reader.GetInt32Value("idNetworkData"),
                        IdProdotto = reader.GetInt32Value("idProdotto"),
                        MacAddress = reader.GetStringOrEmpty("macaddress"),
                        Note = reader.GetStringOrEmpty("note"),
                        ProdottoDescrizione = string.Join(" - ", new[]
                        {
                            reader["Categorico"] == DBNull.Value ? string.Empty : Convert.ToString(reader["Categorico"], CultureInfo.InvariantCulture),
                            reader.GetStringOrEmpty("descrizioneProdotto"),
                            reader.GetStringOrEmpty("Matricola")
                        }).Trim(' ', '-')
                    });
                }

                return results;
            }
        }

        public IList<PostazioneAdminItem> GetPostazioniAdmin()
        {
            using (var connection = Db.OpenConnection())
            using (var command = new SqlCommand(@"
SELECT p.idPostazione, p.idProdotto, p.idNomeMacchina, n.NomeMacchina, pr.Categorico, oo.descrizioneProdotto, pr.Matricola
FROM dbo.Postazione p
LEFT JOIN dbo.NomeMacchina n ON p.idNomeMacchina = n.idnomemacchina
INNER JOIN dbo.Prodotti pr ON p.idProdotto = pr.IdProdotto
LEFT JOIN dbo.OggettoOrdinativo oo ON pr.idOggOrdinativo = oo.idOggOrdinativo
ORDER BY p.idPostazione DESC;", connection))
            using (var reader = command.ExecuteReader())
            {
                var results = new List<PostazioneAdminItem>();
                while (reader.Read())
                {
                    results.Add(new PostazioneAdminItem
                    {
                        IdPostazione = reader.GetInt32Value("idPostazione"),
                        IdProdotto = reader.GetInt32Value("idProdotto"),
                        IdNomeMacchina = reader["idNomeMacchina"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["idNomeMacchina"], CultureInfo.InvariantCulture),
                        NomeMacchina = reader.GetStringOrEmpty("NomeMacchina"),
                        ProdottoDescrizione = string.Join(" - ", new[]
                        {
                            reader["Categorico"] == DBNull.Value ? string.Empty : Convert.ToString(reader["Categorico"], CultureInfo.InvariantCulture),
                            reader.GetStringOrEmpty("descrizioneProdotto"),
                            reader.GetStringOrEmpty("Matricola")
                        }).Trim(' ', '-')
                    });
                }

                return results;
            }
        }

        public IList<ProdPersAdminItem> GetProdPersAdmin()
        {
            var results = new List<ProdPersAdminItem>();
            using (var connection = Db.OpenConnection())
            using (var command = new SqlCommand(@"
SELECT pp.IdProdPers, pp.IdProdotto, pp.IdPersonale, pp.DataAssegnazione, p.Categorico, oo.descrizioneProdotto, p.Matricola
FROM dbo.ProdPers pp
LEFT JOIN dbo.Prodotti p ON pp.IdProdotto = p.IdProdotto
LEFT JOIN dbo.OggettoOrdinativo oo ON p.idOggOrdinativo = oo.idOggOrdinativo
ORDER BY pp.IdProdPers DESC;", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    results.Add(new ProdPersAdminItem
                    {
                        IdProdPers = reader.GetInt32Value("IdProdPers"),
                        IdProdotto = reader["IdProdotto"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["IdProdotto"], CultureInfo.InvariantCulture),
                        IdPersonale = reader["IdPersonale"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["IdPersonale"], CultureInfo.InvariantCulture),
                        DataAssegnazione = reader["DataAssegnazione"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DataAssegnazione"], CultureInfo.InvariantCulture),
                        ProdottoDescrizione = string.Join(" - ", new[]
                        {
                            reader["Categorico"] == DBNull.Value ? string.Empty : Convert.ToString(reader["Categorico"], CultureInfo.InvariantCulture),
                            reader.GetStringOrEmpty("descrizioneProdotto"),
                            reader.GetStringOrEmpty("Matricola")
                        }).Trim(' ', '-')
                    });
                }
            }

            var names = _personaleRepository.GetDisplayNames(results.Select(item => item.IdPersonale));
            foreach (var item in results)
            {
                if (item.IdPersonale.HasValue && names.ContainsKey(item.IdPersonale.Value))
                {
                    item.PersonaleDescrizione = names[item.IdPersonale.Value];
                }
            }

            return results;
        }

        public IList<ProdPersStoricoAdminItem> GetProdPersStoricoAdmin()
        {
            var results = new List<ProdPersStoricoAdminItem>();
            using (var connection = Db.OpenConnection())
            using (var command = new SqlCommand(@"
SELECT s.id, s.idProdPers, s.idProdotto, s.idPersonale, s.dataAssegnazione, s.dataRestituzione, s.numeroStanza, s.livelloEfficienza, s.noteProdotto, s.nomeMacchina, s.serialNumber,
       p.Categorico, oo.descrizioneProdotto, p.Matricola
FROM dbo.ProdPersStorico s
LEFT JOIN dbo.Prodotti p ON s.idProdotto = p.IdProdotto
LEFT JOIN dbo.OggettoOrdinativo oo ON p.idOggOrdinativo = oo.idOggOrdinativo
ORDER BY s.id DESC;", connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    results.Add(new ProdPersStoricoAdminItem
                    {
                        Id = reader.GetInt32Value("id"),
                        IdProdPers = reader["idProdPers"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["idProdPers"], CultureInfo.InvariantCulture),
                        IdProdotto = reader["idProdotto"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["idProdotto"], CultureInfo.InvariantCulture),
                        IdPersonale = reader["idPersonale"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["idPersonale"], CultureInfo.InvariantCulture),
                        DataAssegnazione = reader["dataAssegnazione"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["dataAssegnazione"], CultureInfo.InvariantCulture),
                        DataRestituzione = reader["dataRestituzione"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["dataRestituzione"], CultureInfo.InvariantCulture),
                        ProdottoDescrizione = string.Join(" - ", new[]
                        {
                            reader["Categorico"] == DBNull.Value ? string.Empty : Convert.ToString(reader["Categorico"], CultureInfo.InvariantCulture),
                            reader.GetStringOrEmpty("descrizioneProdotto"),
                            reader.GetStringOrEmpty("Matricola")
                        }).Trim(' ', '-'),
                        NumeroStanza = reader.GetStringOrEmpty("numeroStanza"),
                        LivelloEfficienza = reader.GetStringOrEmpty("livelloEfficienza"),
                        NoteProdotto = reader.GetStringOrEmpty("noteProdotto"),
                        NomeMacchina = reader.GetStringOrEmpty("nomeMacchina"),
                        SerialNumber = reader.GetStringOrEmpty("serialNumber")
                    });
                }
            }

            var names = _personaleRepository.GetDisplayNames(results.Select(item => item.IdPersonale));
            foreach (var item in results)
            {
                if (item.IdPersonale.HasValue && names.ContainsKey(item.IdPersonale.Value))
                {
                    item.PersonaleDescrizione = names[item.IdPersonale.Value];
                }
            }

            return results;
        }

        public int CreateProdottoAdmin(ProdottoAdminItem item)
        {
            using (var connection = Db.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            using (var command = new SqlCommand(@"
INSERT INTO dbo.Prodotti (idStanza, idOggOrdinativo, Categorico, Matricola, IdEfficienza, DataUltimaMov, Note, Versamento)
VALUES (@IdStanza, @IdOggOrdinativo, @Categorico, @Matricola, @IdEfficienza, GETDATE(), @Note, @Versamento);
SELECT CAST(SCOPE_IDENTITY() AS int);", connection, transaction))
            {
                try
                {
                    var categorico = item.Categorico ?? GetNextCategorico(connection, transaction);
                    command.Parameters.Add("@IdStanza", SqlDbType.Int).Value = NullableDb(item.IdStanza);
                    command.Parameters.Add("@IdOggOrdinativo", SqlDbType.Int).Value = NullableDb(item.IdOggOrdinativo);
                    command.Parameters.Add("@Categorico", SqlDbType.Int).Value = categorico;
                    command.Parameters.Add("@Matricola", SqlDbType.NVarChar, 50).Value = NullableDb(item.Matricola);
                    command.Parameters.Add("@IdEfficienza", SqlDbType.Int).Value = NullableDb(item.IdEfficienza);
                    command.Parameters.Add("@Note", SqlDbType.NVarChar, -1).Value = NullableDb(item.Note);
                    command.Parameters.Add("@Versamento", SqlDbType.NVarChar, 256).Value = NullableDb(item.Versamento);
                    var id = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
                    transaction.Commit();
                    return id;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void UpdateProdottoAdmin(ProdottoAdminItem item)
        {
            ExecuteDomainNonQuery("InventarioRepository.UpdateProdottoAdmin", @"
UPDATE dbo.Prodotti
SET idStanza = @IdStanza,
    idOggOrdinativo = @IdOggOrdinativo,
    Categorico = @Categorico,
    Matricola = @Matricola,
    IdEfficienza = @IdEfficienza,
    DataUltimaMov = GETDATE(),
    Note = @Note,
    Versamento = @Versamento
WHERE IdProdotto = @Id;", command =>
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = item.IdProdotto;
                command.Parameters.Add("@IdStanza", SqlDbType.Int).Value = NullableDb(item.IdStanza);
                command.Parameters.Add("@IdOggOrdinativo", SqlDbType.Int).Value = NullableDb(item.IdOggOrdinativo);
                command.Parameters.Add("@Categorico", SqlDbType.Int).Value = NullableDb(item.Categorico);
                command.Parameters.Add("@Matricola", SqlDbType.NVarChar, 50).Value = NullableDb(item.Matricola);
                command.Parameters.Add("@IdEfficienza", SqlDbType.Int).Value = NullableDb(item.IdEfficienza);
                command.Parameters.Add("@Note", SqlDbType.NVarChar, -1).Value = NullableDb(item.Note);
                command.Parameters.Add("@Versamento", SqlDbType.NVarChar, 256).Value = NullableDb(item.Versamento);
            });
        }

        public void DeleteProdottoAdmin(int id)
        {
            AppLogger.Info("InventarioRepository.DeleteProdottoAdmin", "Eliminazione prodotto " + id.ToString(CultureInfo.InvariantCulture) + ".");
            using (var connection = Db.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using (var cmd = new SqlCommand("DELETE FROM dbo.ProdPersStorico WHERE idProdotto = @IdProdotto;", connection, transaction))
                    {
                        cmd.Parameters.Add("@IdProdotto", SqlDbType.Int).Value = id;
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new SqlCommand("DELETE FROM dbo.NetworkData WHERE idProdotto = @IdProdotto;", connection, transaction))
                    {
                        cmd.Parameters.Add("@IdProdotto", SqlDbType.Int).Value = id;
                        cmd.ExecuteNonQuery();
                    }

                    using (var cmd = new SqlCommand("DELETE FROM dbo.Prodotti WHERE IdProdotto = @IdProdotto;", connection, transaction))
                    {
                        cmd.Parameters.Add("@IdProdotto", SqlDbType.Int).Value = id;
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public int CreateOrdinativoAdmin(OrdinativoAdminItem item)
        {
            return ExecuteInsertScalar("InventarioRepository.CreateOrdinativoAdmin", @"
INSERT INTO dbo.Ordinativo (CodiceOrdinativo, denominazioneOrdinativo)
VALUES (@Codice, @Denominazione);
SELECT CAST(SCOPE_IDENTITY() AS int);", command =>
            {
                command.Parameters.Add("@Codice", SqlDbType.VarChar, 50).Value = NullableDb(item.CodiceOrdinativo);
                command.Parameters.Add("@Denominazione", SqlDbType.VarChar, 255).Value = item.DenominazioneOrdinativo.Trim();
            });
        }

        public void UpdateOrdinativoAdmin(OrdinativoAdminItem item)
        {
            ExecuteDomainNonQuery("InventarioRepository.UpdateOrdinativoAdmin", @"
UPDATE dbo.Ordinativo
SET CodiceOrdinativo = @Codice,
    denominazioneOrdinativo = @Denominazione
WHERE idOrdinativo = @Id;", command =>
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = item.IdOrdinativo;
                command.Parameters.Add("@Codice", SqlDbType.VarChar, 50).Value = NullableDb(item.CodiceOrdinativo);
                command.Parameters.Add("@Denominazione", SqlDbType.VarChar, 255).Value = item.DenominazioneOrdinativo.Trim();
            });
        }

        public void DeleteOrdinativoAdmin(int id)
        {
            ExecuteDelete("InventarioRepository.DeleteOrdinativoAdmin", "DELETE FROM dbo.Ordinativo WHERE idOrdinativo = @Id;", id);
        }

        public int CreateOggettoOrdinativoAdmin(OggettoOrdinativoAdminItem item)
        {
            return ExecuteInsertScalar("InventarioRepository.CreateOggettoOrdinativoAdmin", @"
INSERT INTO dbo.OggettoOrdinativo (idOrdinativo, descrizioneProdotto, idDittaCostruttrice, modello, NUC, prezzoUnitarioNetto, prezzoInventario, idCategProdotti)
VALUES (@IdOrdinativo, @Descrizione, @IdDitta, @Modello, @NUC, @PrezzoUnitario, @PrezzoInventario, @IdCategoria);
SELECT CAST(SCOPE_IDENTITY() AS int);", command =>
            {
                command.Parameters.Add("@IdOrdinativo", SqlDbType.Int).Value = NullableDb(item.IdOrdinativo);
                command.Parameters.Add("@Descrizione", SqlDbType.VarChar, 256).Value = item.DescrizioneProdotto.Trim();
                command.Parameters.Add("@IdDitta", SqlDbType.Int).Value = NullableDb(item.IdDittaCostruttrice);
                command.Parameters.Add("@Modello", SqlDbType.VarChar, 64).Value = NullableDb(item.Modello);
                command.Parameters.Add("@NUC", SqlDbType.VarChar, 50).Value = NullableDb(item.NUC);
                command.Parameters.Add("@PrezzoUnitario", SqlDbType.Money).Value = 0m;
                command.Parameters.Add("@PrezzoInventario", SqlDbType.Money).Value = 0m;
                command.Parameters.Add("@IdCategoria", SqlDbType.Int).Value = NullableDb(item.IdCategProdotti);
            });
        }

        public void UpdateOggettoOrdinativoAdmin(OggettoOrdinativoAdminItem item)
        {
            ExecuteDomainNonQuery("InventarioRepository.UpdateOggettoOrdinativoAdmin", @"
UPDATE dbo.OggettoOrdinativo
SET idOrdinativo = @IdOrdinativo,
    descrizioneProdotto = @Descrizione,
    idDittaCostruttrice = @IdDitta,
    modello = @Modello,
    NUC = @NUC,
    idCategProdotti = @IdCategoria
WHERE idOggOrdinativo = @Id;", command =>
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = item.IdOggOrdinativo;
                command.Parameters.Add("@IdOrdinativo", SqlDbType.Int).Value = NullableDb(item.IdOrdinativo);
                command.Parameters.Add("@Descrizione", SqlDbType.VarChar, 256).Value = item.DescrizioneProdotto.Trim();
                command.Parameters.Add("@IdDitta", SqlDbType.Int).Value = NullableDb(item.IdDittaCostruttrice);
                command.Parameters.Add("@Modello", SqlDbType.VarChar, 64).Value = NullableDb(item.Modello);
                command.Parameters.Add("@NUC", SqlDbType.VarChar, 50).Value = NullableDb(item.NUC);
                command.Parameters.Add("@IdCategoria", SqlDbType.Int).Value = NullableDb(item.IdCategProdotti);
            });
        }

        public void DeleteOggettoOrdinativoAdmin(int id)
        {
            ExecuteDelete("InventarioRepository.DeleteOggettoOrdinativoAdmin", "DELETE FROM dbo.OggettoOrdinativo WHERE idOggOrdinativo = @Id;", id);
        }

        public int CreateNetworkDataAdmin(NetworkDataAdminItem item)
        {
            return ExecuteInsertScalar("InventarioRepository.CreateNetworkDataAdmin", "INSERT INTO dbo.NetworkData (idProdotto, macaddress, note) VALUES (@IdProdotto, @MacAddress, @Note); SELECT CAST(SCOPE_IDENTITY() AS int);", command =>
            {
                command.Parameters.Add("@IdProdotto", SqlDbType.Int).Value = item.IdProdotto;
                command.Parameters.Add("@MacAddress", SqlDbType.NChar, 17).Value = NormalizeMacAddress(item.MacAddress);
                command.Parameters.Add("@Note", SqlDbType.VarChar, 50).Value = NullableDb(item.Note);
            });
        }

        public void UpdateNetworkDataAdmin(NetworkDataAdminItem item)
        {
            ExecuteDomainNonQuery("InventarioRepository.UpdateNetworkDataAdmin", "UPDATE dbo.NetworkData SET idProdotto = @IdProdotto, macaddress = @MacAddress, note = @Note WHERE idNetworkData = @Id;", command =>
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = item.IdNetworkData;
                command.Parameters.Add("@IdProdotto", SqlDbType.Int).Value = item.IdProdotto;
                command.Parameters.Add("@MacAddress", SqlDbType.NChar, 17).Value = NormalizeMacAddress(item.MacAddress);
                command.Parameters.Add("@Note", SqlDbType.VarChar, 50).Value = NullableDb(item.Note);
            });
        }

        public void DeleteNetworkDataAdmin(int id)
        {
            ExecuteDelete("InventarioRepository.DeleteNetworkDataAdmin", "DELETE FROM dbo.NetworkData WHERE idNetworkData = @Id;", id);
        }

        public int CreatePostazioneAdmin(PostazioneAdminItem item)
        {
            AppLogger.Info("InventarioRepository.CreatePostazioneAdmin", "Creazione postazione.");
            using (var connection = Db.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    var idNomeMacchina = EnsureNomeMacchina(connection, transaction, item.NomeMacchina, item.IdNomeMacchina);
                    using (var command = new SqlCommand("INSERT INTO dbo.Postazione (idNomeMacchina, idProdotto) VALUES (@IdNomeMacchina, @IdProdotto); SELECT CAST(SCOPE_IDENTITY() AS int);", connection, transaction))
                    {
                        command.Parameters.Add("@IdNomeMacchina", SqlDbType.Int).Value = NullableDb(idNomeMacchina);
                        command.Parameters.Add("@IdProdotto", SqlDbType.Int).Value = item.IdProdotto;
                        var id = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
                        transaction.Commit();
                        return id;
                    }
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void UpdatePostazioneAdmin(PostazioneAdminItem item)
        {
            AppLogger.Info("InventarioRepository.UpdatePostazioneAdmin", "Aggiornamento postazione.");
            using (var connection = Db.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    var idNomeMacchina = EnsureNomeMacchina(connection, transaction, item.NomeMacchina, item.IdNomeMacchina);
                    int? oldIdNomeMacchina = null;
                    using (var select = new SqlCommand("SELECT idNomeMacchina FROM dbo.Postazione WHERE idPostazione = @Id;", connection, transaction))
                    {
                        select.Parameters.Add("@Id", SqlDbType.Int).Value = item.IdPostazione;
                        var scalar = select.ExecuteScalar();
                        if (scalar != null && scalar != DBNull.Value)
                        {
                            oldIdNomeMacchina = Convert.ToInt32(scalar, CultureInfo.InvariantCulture);
                        }
                    }

                    using (var command = new SqlCommand("UPDATE dbo.Postazione SET idNomeMacchina = @IdNomeMacchina, idProdotto = @IdProdotto WHERE idPostazione = @Id;", connection, transaction))
                    {
                        command.Parameters.Add("@Id", SqlDbType.Int).Value = item.IdPostazione;
                        command.Parameters.Add("@IdNomeMacchina", SqlDbType.Int).Value = NullableDb(idNomeMacchina);
                        command.Parameters.Add("@IdProdotto", SqlDbType.Int).Value = item.IdProdotto;
                        command.ExecuteNonQuery();
                    }

                    DeleteNomeMacchinaIfOrphan(connection, transaction, oldIdNomeMacchina);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public void DeletePostazioneAdmin(int id)
        {
            AppLogger.Info("InventarioRepository.DeletePostazioneAdmin", "Eliminazione postazione.");
            using (var connection = Db.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    int? idNomeMacchina = null;
                    using (var select = new SqlCommand("SELECT idNomeMacchina FROM dbo.Postazione WHERE idPostazione = @Id;", connection, transaction))
                    {
                        select.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                        var scalar = select.ExecuteScalar();
                        if (scalar != null && scalar != DBNull.Value)
                        {
                            idNomeMacchina = Convert.ToInt32(scalar, CultureInfo.InvariantCulture);
                        }
                    }

                    using (var command = new SqlCommand("DELETE FROM dbo.Postazione WHERE idPostazione = @Id;", connection, transaction))
                    {
                        command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                        command.ExecuteNonQuery();
                    }

                    DeleteNomeMacchinaIfOrphan(connection, transaction, idNomeMacchina);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        public int CreateProdPersAdmin(ProdPersAdminItem item)
        {
            return ExecuteInsertScalar("InventarioRepository.CreateProdPersAdmin", "INSERT INTO dbo.ProdPers (IdProdotto, IdPersonale, DataAssegnazione) VALUES (@IdProdotto, @IdPersonale, @DataAssegnazione); SELECT CAST(SCOPE_IDENTITY() AS int);", command =>
            {
                command.Parameters.Add("@IdProdotto", SqlDbType.Int).Value = NullableDb(item.IdProdotto);
                command.Parameters.Add("@IdPersonale", SqlDbType.Int).Value = NullableDb(item.IdPersonale);
                command.Parameters.Add("@DataAssegnazione", SqlDbType.Date).Value = NullableDb(item.DataAssegnazione);
            });
        }

        public void UpdateProdPersAdmin(ProdPersAdminItem item)
        {
            ExecuteDomainNonQuery("InventarioRepository.UpdateProdPersAdmin", "UPDATE dbo.ProdPers SET IdProdotto = @IdProdotto, IdPersonale = @IdPersonale, DataAssegnazione = @DataAssegnazione WHERE IdProdPers = @Id;", command =>
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = item.IdProdPers;
                command.Parameters.Add("@IdProdotto", SqlDbType.Int).Value = NullableDb(item.IdProdotto);
                command.Parameters.Add("@IdPersonale", SqlDbType.Int).Value = NullableDb(item.IdPersonale);
                command.Parameters.Add("@DataAssegnazione", SqlDbType.Date).Value = NullableDb(item.DataAssegnazione);
            });
        }

        public void DeleteProdPersAdmin(int id)
        {
            ExecuteDelete("InventarioRepository.DeleteProdPersAdmin", "DELETE FROM dbo.ProdPers WHERE IdProdPers = @Id;", id);
        }

        public int CreateProdPersStoricoAdmin(ProdPersStoricoAdminItem item)
        {
            return ExecuteInsertScalar("InventarioRepository.CreateProdPersStoricoAdmin", @"
INSERT INTO dbo.ProdPersStorico (idProdPers, idProdotto, idPersonale, dataAssegnazione, dataRestituzione, numeroStanza, livelloEfficienza, noteProdotto, nomeMacchina, serialNumber)
VALUES (@IdProdPers, @IdProdotto, @IdPersonale, @DataAssegnazione, @DataRestituzione, @NumeroStanza, @LivelloEfficienza, @NoteProdotto, @NomeMacchina, @SerialNumber);
SELECT CAST(SCOPE_IDENTITY() AS int);", command =>
            {
                command.Parameters.Add("@IdProdPers", SqlDbType.Int).Value = NullableDb(item.IdProdPers);
                command.Parameters.Add("@IdProdotto", SqlDbType.Int).Value = NullableDb(item.IdProdotto);
                command.Parameters.Add("@IdPersonale", SqlDbType.Int).Value = NullableDb(item.IdPersonale);
                command.Parameters.Add("@DataAssegnazione", SqlDbType.Date).Value = NullableDb(item.DataAssegnazione);
                command.Parameters.Add("@DataRestituzione", SqlDbType.Date).Value = NullableDb(item.DataRestituzione);
                command.Parameters.Add("@NumeroStanza", SqlDbType.NVarChar, 50).Value = NullableDb(item.NumeroStanza);
                command.Parameters.Add("@LivelloEfficienza", SqlDbType.NVarChar, 50).Value = NullableDb(item.LivelloEfficienza);
                command.Parameters.Add("@NoteProdotto", SqlDbType.NVarChar, 500).Value = NullableDb(item.NoteProdotto);
                command.Parameters.Add("@NomeMacchina", SqlDbType.NVarChar, 50).Value = NullableDb(item.NomeMacchina);
                command.Parameters.Add("@SerialNumber", SqlDbType.NVarChar, 50).Value = NullableDb(item.SerialNumber);
            });
        }

        public void UpdateProdPersStoricoAdmin(ProdPersStoricoAdminItem item)
        {
            ExecuteDomainNonQuery("InventarioRepository.UpdateProdPersStoricoAdmin", @"
UPDATE dbo.ProdPersStorico
SET idProdPers = @IdProdPers,
    idProdotto = @IdProdotto,
    idPersonale = @IdPersonale,
    dataAssegnazione = @DataAssegnazione,
    dataRestituzione = @DataRestituzione,
    numeroStanza = @NumeroStanza,
    livelloEfficienza = @LivelloEfficienza,
    noteProdotto = @NoteProdotto,
    nomeMacchina = @NomeMacchina,
    serialNumber = @SerialNumber
WHERE id = @Id;", command =>
            {
                command.Parameters.Add("@Id", SqlDbType.Int).Value = item.Id;
                command.Parameters.Add("@IdProdPers", SqlDbType.Int).Value = NullableDb(item.IdProdPers);
                command.Parameters.Add("@IdProdotto", SqlDbType.Int).Value = NullableDb(item.IdProdotto);
                command.Parameters.Add("@IdPersonale", SqlDbType.Int).Value = NullableDb(item.IdPersonale);
                command.Parameters.Add("@DataAssegnazione", SqlDbType.Date).Value = NullableDb(item.DataAssegnazione);
                command.Parameters.Add("@DataRestituzione", SqlDbType.Date).Value = NullableDb(item.DataRestituzione);
                command.Parameters.Add("@NumeroStanza", SqlDbType.NVarChar, 50).Value = NullableDb(item.NumeroStanza);
                command.Parameters.Add("@LivelloEfficienza", SqlDbType.NVarChar, 50).Value = NullableDb(item.LivelloEfficienza);
                command.Parameters.Add("@NoteProdotto", SqlDbType.NVarChar, 500).Value = NullableDb(item.NoteProdotto);
                command.Parameters.Add("@NomeMacchina", SqlDbType.NVarChar, 50).Value = NullableDb(item.NomeMacchina);
                command.Parameters.Add("@SerialNumber", SqlDbType.NVarChar, 50).Value = NullableDb(item.SerialNumber);
            });
        }

        public void DeleteProdPersStoricoAdmin(int id)
        {
            ExecuteDelete("InventarioRepository.DeleteProdPersStoricoAdmin", "DELETE FROM dbo.ProdPersStorico WHERE id = @Id;", id);
        }

        public void CreateCategoria(string nome, int idTipoOggetto)
        {
            ExecuteDomainNonQuery(
                "InventarioRepository.CreateCategoria",
                "INSERT INTO dbo.CategoriaProdotti (Descrizione, idTipoOO, ethernet) VALUES (@Nome, @IdTipoOO, 0);",
                command =>
                {
                    command.Parameters.Add("@Nome", SqlDbType.NVarChar, 50).Value = nome.Trim();
                    command.Parameters.Add("@IdTipoOO", SqlDbType.Int).Value = idTipoOggetto;
                });
        }

        public void UpdateCategoria(int id, string nome, int idTipoOggetto)
        {
            ExecuteDomainNonQuery(
                "InventarioRepository.UpdateCategoria",
                "UPDATE dbo.CategoriaProdotti SET Descrizione = @Nome, idTipoOO = @IdTipoOO WHERE IdCategoria = @Id;",
                command =>
                {
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                    command.Parameters.Add("@Nome", SqlDbType.NVarChar, 50).Value = nome.Trim();
                    command.Parameters.Add("@IdTipoOO", SqlDbType.Int).Value = idTipoOggetto;
                });
        }

        public void DeleteCategoria(int id)
        {
            ExecuteDelete("InventarioRepository.DeleteCategoria", "DELETE FROM dbo.CategoriaProdotti WHERE IdCategoria = @Id;", id);
        }

        public void CreateLivelloEfficienza(string codice, string nome)
        {
            ExecuteDomainNonQuery(
                "InventarioRepository.CreateLivelloEfficienza",
                "INSERT INTO dbo.LivelliEfficenza (Codice, Livello_efficienza) VALUES (@Codice, @Nome);",
                command =>
                {
                    command.Parameters.Add("@Codice", SqlDbType.VarChar, 4).Value = NullableDb(codice == null ? null : codice.Trim());
                    command.Parameters.Add("@Nome", SqlDbType.NVarChar, 50).Value = nome.Trim();
                });
        }

        public void UpdateLivelloEfficienza(int id, string codice, string nome)
        {
            ExecuteDomainNonQuery(
                "InventarioRepository.UpdateLivelloEfficienza",
                "UPDATE dbo.LivelliEfficenza SET Codice = @Codice, Livello_efficienza = @Nome WHERE IdEfficienza = @Id;",
                command =>
                {
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                    command.Parameters.Add("@Codice", SqlDbType.VarChar, 4).Value = NullableDb(codice == null ? null : codice.Trim());
                    command.Parameters.Add("@Nome", SqlDbType.NVarChar, 50).Value = nome.Trim();
                });
        }

        public void DeleteLivelloEfficienza(int id)
        {
            ExecuteDelete("InventarioRepository.DeleteLivelloEfficienza", "DELETE FROM dbo.LivelliEfficenza WHERE IdEfficienza = @Id;", id);
        }

        public void CreateStanza(string numero)
        {
            ExecuteDomainNonQuery(
                "InventarioRepository.CreateStanza",
                "INSERT INTO dbo.Stanze (numero) VALUES (@Numero);",
                command => command.Parameters.Add("@Numero", SqlDbType.VarChar, 10).Value = numero.Trim());
        }

        public void UpdateStanza(int id, string numero)
        {
            ExecuteDomainNonQuery(
                "InventarioRepository.UpdateStanza",
                "UPDATE dbo.Stanze SET numero = @Numero WHERE idstanza = @Id;",
                command =>
                {
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                    command.Parameters.Add("@Numero", SqlDbType.VarChar, 10).Value = numero.Trim();
                });
        }

        public void DeleteStanza(int id)
        {
            ExecuteDelete("InventarioRepository.DeleteStanza", "DELETE FROM dbo.Stanze WHERE idstanza = @Id;", id);
        }

        public void CreateDitta(string nome, string citta, string mail, string tipologia)
        {
            ExecuteDomainNonQuery(
                "InventarioRepository.CreateDitta",
                "INSERT INTO dbo.Ditte (Nome, Citta, Mail, tipologia) VALUES (@Nome, @Citta, @Mail, @Tipologia);",
                command =>
                {
                    command.Parameters.Add("@Nome", SqlDbType.VarChar, 50).Value = nome.Trim();
                    command.Parameters.Add("@Citta", SqlDbType.NVarChar, 30).Value = NullableDb(citta == null ? null : citta.Trim());
                    command.Parameters.Add("@Mail", SqlDbType.NVarChar, 50).Value = NullableDb(mail == null ? null : mail.Trim());
                    command.Parameters.Add("@Tipologia", SqlDbType.Char, 1).Value = NullableDb(NormalizeSingleChar(tipologia));
                });
        }

        public void UpdateDitta(int id, string nome, string citta, string mail, string tipologia)
        {
            ExecuteDomainNonQuery(
                "InventarioRepository.UpdateDitta",
                "UPDATE dbo.Ditte SET Nome = @Nome, Citta = @Citta, Mail = @Mail, tipologia = @Tipologia WHERE IdDitta = @Id;",
                command =>
                {
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                    command.Parameters.Add("@Nome", SqlDbType.VarChar, 50).Value = nome.Trim();
                    command.Parameters.Add("@Citta", SqlDbType.NVarChar, 30).Value = NullableDb(citta == null ? null : citta.Trim());
                    command.Parameters.Add("@Mail", SqlDbType.NVarChar, 50).Value = NullableDb(mail == null ? null : mail.Trim());
                    command.Parameters.Add("@Tipologia", SqlDbType.Char, 1).Value = NullableDb(NormalizeSingleChar(tipologia));
                });
        }

        public void DeleteDitta(int id)
        {
            ExecuteDelete("InventarioRepository.DeleteDitta", "DELETE FROM dbo.Ditte WHERE IdDitta = @Id;", id);
        }

        public void CreateTipoOggetto(int id, string nome)
        {
            ExecuteDomainNonQuery(
                "InventarioRepository.CreateTipoOggetto",
                "INSERT INTO dbo.TipoOggettoOrdinativo (idTipoOggOrdinativo, Descrizione) VALUES (@Id, @Nome);",
                command =>
                {
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                    command.Parameters.Add("@Nome", SqlDbType.VarChar, 50).Value = NullableDb(nome == null ? null : nome.Trim());
                });
        }

        public void UpdateTipoOggetto(int id, string nome)
        {
            ExecuteDomainNonQuery(
                "InventarioRepository.UpdateTipoOggetto",
                "UPDATE dbo.TipoOggettoOrdinativo SET Descrizione = @Nome WHERE idTipoOggOrdinativo = @Id;",
                command =>
                {
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                    command.Parameters.Add("@Nome", SqlDbType.VarChar, 50).Value = NullableDb(nome == null ? null : nome.Trim());
                });
        }

        public void DeleteTipoOggetto(int id)
        {
            ExecuteDelete("InventarioRepository.DeleteTipoOggetto", "DELETE FROM dbo.TipoOggettoOrdinativo WHERE idTipoOggOrdinativo = @Id;", id);
        }

        public int CreateProdotto(NuovoProdottoInput input)
        {
            AppLogger.Info("InventarioRepository.CreateProdotto", "Creazione nuovo materiale.");
            using (var connection = Db.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    var categorico = input.Categorico ?? GetNextCategorico(connection, transaction);
                    var idOggetto = EnsureOggettoOrdinativo(connection, transaction, input);
                    using (var command = new SqlCommand(@"
INSERT INTO dbo.Prodotti (idStanza, idOggOrdinativo, Categorico, Matricola, IdEfficienza, DataUltimaMov, Note, Versamento)
VALUES (@IdStanza, @IdOggOrdinativo, @Categorico, @Matricola, @IdEfficienza, GETDATE(), @Note, @Versamento);
SELECT CAST(SCOPE_IDENTITY() AS int);", connection, transaction))
                    {
                        command.Parameters.Add("@IdStanza", SqlDbType.Int).Value = NullableDb(input.IdStanza);
                        command.Parameters.Add("@IdOggOrdinativo", SqlDbType.Int).Value = NullableDb(idOggetto);
                        command.Parameters.Add("@Categorico", SqlDbType.Int).Value = categorico;
                        command.Parameters.Add("@Matricola", SqlDbType.NVarChar, 50).Value = NullableDb(input.Matricola);
                        command.Parameters.Add("@IdEfficienza", SqlDbType.Int).Value = NullableDb(input.IdEfficienza);
                        command.Parameters.Add("@Note", SqlDbType.NVarChar, -1).Value = NullableDb(input.Note);
                        command.Parameters.Add("@Versamento", SqlDbType.NVarChar, 256).Value = NullableDb(input.Versamento);
                        var idProdotto = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
                        transaction.Commit();
                        return idProdotto;
                    }
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    AppLogger.Error("InventarioRepository.CreateProdotto", "Errore nella creazione del materiale.", ex);
                    throw;
                }
            }
        }

        public void AssegnaProdotto(AssegnazioneInput input)
        {
            AppLogger.Info("InventarioRepository.AssegnaProdotto", "Assegnazione materiale " + input.IdProdotto.ToString(CultureInfo.InvariantCulture) + ".");
            using (var connection = Db.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    CloseCurrentAssignment(connection, transaction, input.IdProdotto, input.DataAssegnazione);

                    int idProdPers;
                    using (var command = new SqlCommand(@"
INSERT INTO dbo.ProdPers (IdProdotto, IdPersonale, DataAssegnazione)
VALUES (@IdProdotto, @IdPersonale, @DataAssegnazione);
SELECT CAST(SCOPE_IDENTITY() AS int);", connection, transaction))
                    {
                        command.Parameters.Add("@IdProdotto", SqlDbType.Int).Value = input.IdProdotto;
                        command.Parameters.Add("@IdPersonale", SqlDbType.Int).Value = input.IdPersonale;
                        command.Parameters.Add("@DataAssegnazione", SqlDbType.Date).Value = input.DataAssegnazione.Date;
                        idProdPers = Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
                    }

                    InsertStoricoSnapshot(connection, transaction, idProdPers, input.IdProdotto, input.IdPersonale, input.DataAssegnazione.Date, null, input.Note);
                    UpdateDataUltimaMov(connection, transaction, input.IdProdotto, input.Note);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    AppLogger.Error("InventarioRepository.AssegnaProdotto", "Errore in assegnazione.", ex);
                    throw;
                }
            }
        }

        public void RegistraRientroORiassegnazione(RientroRiassegnazioneInput input)
        {
            AppLogger.Info("InventarioRepository.RegistraRientroORiassegnazione", "Rientro o riassegnazione materiale " + input.IdProdotto.ToString(CultureInfo.InvariantCulture) + ".");
            using (var connection = Db.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    CloseCurrentAssignment(connection, transaction, input.IdProdotto, input.DataOperazione);

                    if (input.CreaNuovaAssegnazione && input.NuovoIdPersonale.HasValue)
                    {
                        int idProdPers;
                        using (var insert = new SqlCommand(@"
INSERT INTO dbo.ProdPers (IdProdotto, IdPersonale, DataAssegnazione)
VALUES (@IdProdotto, @IdPersonale, @DataAssegnazione);
SELECT CAST(SCOPE_IDENTITY() AS int);", connection, transaction))
                        {
                            insert.Parameters.Add("@IdProdotto", SqlDbType.Int).Value = input.IdProdotto;
                            insert.Parameters.Add("@IdPersonale", SqlDbType.Int).Value = input.NuovoIdPersonale.Value;
                            insert.Parameters.Add("@DataAssegnazione", SqlDbType.Date).Value = input.DataOperazione.Date;
                            idProdPers = Convert.ToInt32(insert.ExecuteScalar(), CultureInfo.InvariantCulture);
                        }

                        InsertStoricoSnapshot(connection, transaction, idProdPers, input.IdProdotto, input.NuovoIdPersonale.Value, input.DataOperazione.Date, null, input.Note);
                    }

                    UpdateDataUltimaMov(connection, transaction, input.IdProdotto, input.Note);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    AppLogger.Error("InventarioRepository.RegistraRientroORiassegnazione", "Errore in rientro/riassegnazione.", ex);
                    throw;
                }
            }
        }

        public void CambiaStato(CambioStatoInput input)
        {
            ExecuteProdottoUpdateWithTransaction(@"
UPDATE dbo.Prodotti
SET IdEfficienza = @IdEfficienza,
    DataUltimaMov = GETDATE(),
    Note = CASE WHEN @Note IS NULL THEN Note ELSE CONVERT(ntext, @Note) END
WHERE IdProdotto = @IdProdotto;", input.IdProdotto, (connection, transaction, command) =>
            {
                command.Parameters.Add("@IdEfficienza", SqlDbType.Int).Value = input.IdEfficienza;
                command.Parameters.Add("@Note", SqlDbType.NVarChar, -1).Value = NullableDb(input.Note);
            }, "InventarioRepository.CambiaStato");
        }

        public void CambiaUbicazione(CambioUbicazioneInput input)
        {
            ExecuteProdottoUpdateWithTransaction(@"
UPDATE dbo.Prodotti
SET idStanza = @IdStanza,
    DataUltimaMov = GETDATE(),
    Note = CASE WHEN @Note IS NULL THEN Note ELSE CONVERT(ntext, @Note) END
WHERE IdProdotto = @IdProdotto;", input.IdProdotto, (connection, transaction, command) =>
            {
                command.Parameters.Add("@IdStanza", SqlDbType.Int).Value = input.IdStanza;
                command.Parameters.Add("@Note", SqlDbType.NVarChar, -1).Value = NullableDb(input.Note);
            }, "InventarioRepository.CambiaUbicazione");
        }

        public void AggiornaComputer(AggiornamentoComputerInput input)
        {
            AppLogger.Info("InventarioRepository.AggiornaComputer", "Aggiornamento rete/postazione materiale " + input.IdProdotto.ToString(CultureInfo.InvariantCulture) + ".");
            using (var connection = Db.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    var normalizedMac = NormalizeMacAddress(input.MacAddress);
                    UpsertNetworkData(connection, transaction, input.IdProdotto, normalizedMac, input.NoteRete);
                    UpsertPostazione(connection, transaction, input.IdProdotto, input.NomeMacchina);
                    UpdateDataUltimaMov(connection, transaction, input.IdProdotto, input.NoteRete);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    AppLogger.Error("InventarioRepository.AggiornaComputer", "Errore in aggiornamento rete/postazione.", ex);
                    throw;
                }
            }
        }

        public IList<ImportReteResultItem> ImportaReteDaNomiMacchina(IEnumerable<ImportReteInputItem> items)
        {
            var results = new List<ImportReteResultItem>();
            using (var connection = Db.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    foreach (var item in items)
                    {
                        var result = new ImportReteResultItem
                        {
                            Riga = item.Riga,
                            NomeMacchina = item.NomeMacchina,
                            MacAddress = item.MacAddress
                        };

                        var normalizedMac = NormalizeMacAddress(item.MacAddress);
                        if (string.IsNullOrWhiteSpace(item.NomeMacchina))
                        {
                            result.Esito = "Scartata";
                            result.Messaggio = "Nome macchina mancante.";
                            results.Add(result);
                            continue;
                        }

                        if (string.IsNullOrWhiteSpace(normalizedMac) || !IsNormalizedMacAddress(normalizedMac))
                        {
                            result.Esito = "Scartata";
                            result.Messaggio = "MAC address mancante o non valido.";
                            results.Add(result);
                            continue;
                        }

                        var idProdotto = FindProdottoByNomeMacchina(connection, transaction, item.NomeMacchina);
                        if (!idProdotto.HasValue)
                        {
                            result.Esito = "Scartata";
                            result.Messaggio = "Nome macchina non trovato o non collegato a un prodotto.";
                            results.Add(result);
                            continue;
                        }

                        UpsertNetworkData(connection, transaction, idProdotto.Value, normalizedMac, "Import TXT");
                        UpdateDataUltimaMov(connection, transaction, idProdotto.Value, "Aggiornamento MAC da import TXT");
                        result.IdProdotto = idProdotto;
                        result.MacAddress = normalizedMac;
                        result.Esito = "Aggiornata";
                        result.Messaggio = "NetworkData aggiornata.";
                        results.Add(result);
                    }

                    transaction.Commit();
                    return results;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    AppLogger.Error("InventarioRepository.ImportaReteDaNomiMacchina", "Errore durante import rete da TXT.", ex);
                    throw;
                }
            }
        }

        public void DismettiProdotto(DismissioneInput input)
        {
            AppLogger.Info("InventarioRepository.DismettiProdotto", "Dismissione materiale " + input.IdProdotto.ToString(CultureInfo.InvariantCulture) + ".");
            using (var connection = Db.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    if (input.ChiudiAssegnazioneAttiva)
                    {
                        CloseCurrentAssignment(connection, transaction, input.IdProdotto, DateTime.Today);
                    }

                    using (var command = new SqlCommand(@"
UPDATE dbo.Prodotti
SET IdEfficienza = COALESCE(@IdEfficienza, IdEfficienza),
    Versamento = @Versamento,
    DataUltimaMov = GETDATE(),
    Note = CASE WHEN @Note IS NULL THEN Note ELSE CONVERT(ntext, @Note) END
WHERE IdProdotto = @IdProdotto;", connection, transaction))
                    {
                        command.Parameters.Add("@IdProdotto", SqlDbType.Int).Value = input.IdProdotto;
                        command.Parameters.Add("@IdEfficienza", SqlDbType.Int).Value = NullableDb(input.IdEfficienza);
                        command.Parameters.Add("@Versamento", SqlDbType.NVarChar, 256).Value = NullableDb(input.Versamento);
                        command.Parameters.Add("@Note", SqlDbType.NVarChar, -1).Value = NullableDb(input.Note);
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    AppLogger.Error("InventarioRepository.DismettiProdotto", "Errore in dismissione.", ex);
                    throw;
                }
            }
        }

        private static string BuildProdottiQuery(bool includeOnlyNetworkItems)
        {
            return BuildProdottiQuery(includeOnlyNetworkItems, false);
        }

        private static string BuildProdottiQuery(bool includeOnlyNetworkItems, bool filterByPersonale)
        {
            return @"
SELECT
    p.IdProdotto,
    p.Categorico,
    p.Matricola,
    p.DataUltimaMov,
    CAST(p.Note AS nvarchar(max)) AS Note,
    p.Versamento,
    s.numero AS NumeroStanza,
    le.Livello_efficienza AS LivelloEfficienza,
    pp.IdPersonale,
    cp.Descrizione AS Categoria,
    too.Descrizione AS TipoOggetto,
    oo.descrizioneProdotto AS DescrizioneProdotto,
    oo.modello AS Modello,
    d.Nome AS DittaCostruttrice,
    nm.NomeMacchina,
    nd.macaddress AS MacAddress
FROM dbo.Prodotti p
LEFT JOIN dbo.Stanze s ON p.idStanza = s.idstanza
LEFT JOIN dbo.LivelliEfficenza le ON p.IdEfficienza = le.IdEfficienza
LEFT JOIN dbo.OggettoOrdinativo oo ON p.idOggOrdinativo = oo.idOggOrdinativo
LEFT JOIN dbo.CategoriaProdotti cp ON oo.idCategProdotti = cp.IdCategoria
LEFT JOIN dbo.TipoOggettoOrdinativo too ON cp.idTipoOO = too.idTipoOggOrdinativo
LEFT JOIN dbo.Ditte d ON oo.idDittaCostruttrice = d.IdDitta
OUTER APPLY (SELECT TOP (1) IdPersonale, DataAssegnazione FROM dbo.ProdPers pp WHERE pp.IdProdotto = p.IdProdotto ORDER BY pp.DataAssegnazione DESC, pp.IdProdPers DESC) pp
OUTER APPLY (SELECT TOP (1) nd.macaddress FROM dbo.NetworkData nd WHERE nd.idProdotto = p.IdProdotto ORDER BY nd.idNetworkData DESC) nd
OUTER APPLY (
    SELECT TOP (1) nm.NomeMacchina
    FROM dbo.Postazione po
    LEFT JOIN dbo.NomeMacchina nm ON po.idNomeMacchina = nm.idnomemacchina
    WHERE po.idProdotto = p.IdProdotto
    ORDER BY po.idPostazione DESC
) nm
WHERE
    (
        @Search IS NULL
        OR CONVERT(nvarchar(50), p.Categorico) LIKE '%' + @Search + '%'
        OR p.Matricola LIKE '%' + @Search + '%'
        OR oo.descrizioneProdotto LIKE '%' + @Search + '%'
        OR oo.modello LIKE '%' + @Search + '%'
        OR cp.Descrizione LIKE '%' + @Search + '%'
        OR nm.NomeMacchina LIKE '%' + @Search + '%'
        OR nd.macaddress LIKE '%' + @Search + '%'
    )
    AND (
        " + (includeOnlyNetworkItems
                ? "nd.macaddress IS NOT NULL OR nm.NomeMacchina IS NOT NULL OR cp.ethernet = 1"
                : "1 = 1") + @"
    )
    AND (
        " + (filterByPersonale ? "pp.IdPersonale = @IdPersonale" : "1 = 1") + @"
    )
ORDER BY p.Categorico, oo.descrizioneProdotto, p.Matricola;";
        }

        private static string BuildDettaglioQuery()
        {
            return @"
SELECT
    p.IdProdotto,
    p.Categorico,
    p.Matricola,
    p.DataUltimaMov,
    CAST(p.Note AS nvarchar(max)) AS Note,
    p.Versamento,
    s.numero AS NumeroStanza,
    le.Livello_efficienza AS LivelloEfficienza,
    pp.IdPersonale,
    cp.Descrizione AS Categoria,
    too.Descrizione AS TipoOggetto,
    oo.descrizioneProdotto AS DescrizioneProdotto,
    oo.modello AS Modello,
    d.Nome AS DittaCostruttrice,
    nm.NomeMacchina,
    nd.macaddress AS MacAddress,
    o.idOrdinativo AS IdOrdinativo,
    o.CodiceOrdinativo,
    o.denominazioneOrdinativo AS DenominazioneOrdinativo,
    o.enteStipulante AS EnteStipulante,
    o.estremiOrdinativo AS EstremiOrdinativo,
    doo.Nome AS DittaOrdinativo
FROM dbo.Prodotti p
LEFT JOIN dbo.Stanze s ON p.idStanza = s.idstanza
LEFT JOIN dbo.LivelliEfficenza le ON p.IdEfficienza = le.IdEfficienza
LEFT JOIN dbo.OggettoOrdinativo oo ON p.idOggOrdinativo = oo.idOggOrdinativo
LEFT JOIN dbo.CategoriaProdotti cp ON oo.idCategProdotti = cp.IdCategoria
LEFT JOIN dbo.TipoOggettoOrdinativo too ON cp.idTipoOO = too.idTipoOggOrdinativo
LEFT JOIN dbo.Ditte d ON oo.idDittaCostruttrice = d.IdDitta
LEFT JOIN dbo.Ordinativo o ON oo.idOrdinativo = o.idOrdinativo
LEFT JOIN dbo.Ditte doo ON o.idDittaOrdinativo = doo.IdDitta
OUTER APPLY (SELECT TOP (1) IdPersonale, DataAssegnazione FROM dbo.ProdPers pp WHERE pp.IdProdotto = p.IdProdotto ORDER BY pp.DataAssegnazione DESC, pp.IdProdPers DESC) pp
OUTER APPLY (SELECT TOP (1) nd.macaddress FROM dbo.NetworkData nd WHERE nd.idProdotto = p.IdProdotto ORDER BY nd.idNetworkData DESC) nd
OUTER APPLY (
    SELECT TOP (1) nm.NomeMacchina
    FROM dbo.Postazione po
    LEFT JOIN dbo.NomeMacchina nm ON po.idNomeMacchina = nm.idnomemacchina
    WHERE po.idProdotto = p.IdProdotto
    ORDER BY po.idPostazione DESC
) nm
WHERE p.IdProdotto = @IdProdotto;";
        }

        private static ProdottoCorrente MapProdotto(IDataRecord reader)
        {
            return new ProdottoCorrente
            {
                IdProdotto = reader.GetInt32Value("IdProdotto"),
                Categorico = reader.GetNullableInt32("Categorico"),
                Matricola = reader.GetStringOrEmpty("Matricola"),
                Categoria = reader.GetStringOrEmpty("Categoria"),
                TipoOggetto = reader.GetStringOrEmpty("TipoOggetto"),
                DescrizioneProdotto = reader.GetStringOrEmpty("DescrizioneProdotto"),
                Modello = reader.GetStringOrEmpty("Modello"),
                DittaCostruttrice = reader.GetStringOrEmpty("DittaCostruttrice"),
                LivelloEfficienza = reader.GetStringOrEmpty("LivelloEfficienza"),
                IdPersonale = reader.GetNullableInt32("IdPersonale"),
                NumeroStanza = reader.GetStringOrEmpty("NumeroStanza"),
                DataUltimaMov = reader["DataUltimaMov"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["DataUltimaMov"], CultureInfo.InvariantCulture),
                Note = reader.GetStringOrEmpty("Note"),
                Versamento = reader.GetStringOrEmpty("Versamento"),
                NomeMacchina = reader.GetStringOrEmpty("NomeMacchina"),
                MacAddress = reader.GetStringOrEmpty("MacAddress")
            };
        }

        private void ResolveAssegnatari(IEnumerable<ProdottoCorrente> prodotti)
        {
            var names = _personaleRepository.GetDisplayNames(prodotti.Select(item => item.IdPersonale));
            foreach (var item in prodotti)
            {
                if (item.IdPersonale.HasValue && names.ContainsKey(item.IdPersonale.Value))
                {
                    item.AssegnatarioDisplay = names[item.IdPersonale.Value];
                }
            }
        }

        private static string BuildStoricoValue(string stanza, string livello, string macchina, string seriale)
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(stanza))
            {
                parts.Add("Stanza " + stanza);
            }

            if (!string.IsNullOrWhiteSpace(livello))
            {
                parts.Add(livello);
            }

            if (!string.IsNullOrWhiteSpace(macchina))
            {
                parts.Add("Macchina " + macchina);
            }

            if (!string.IsNullOrWhiteSpace(seriale))
            {
                parts.Add("Matricola " + seriale);
            }

            return string.Join(" | ", parts);
        }

        private static int GetNextCategorico(SqlConnection connection, SqlTransaction transaction)
        {
            using (var command = new SqlCommand("SELECT ISNULL(MAX(Categorico), 0) + 1 FROM dbo.Prodotti WITH (UPDLOCK, HOLDLOCK);", connection, transaction))
            {
                return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
        }

        private int? EnsureOggettoOrdinativo(SqlConnection connection, SqlTransaction transaction, NuovoProdottoInput input)
        {
            if (input.IdOggettoOrdinativo.HasValue)
            {
                return input.IdOggettoOrdinativo.Value;
            }

            if (string.IsNullOrWhiteSpace(input.DescrizioneProdotto))
            {
                return null;
            }

            using (var command = new SqlCommand(@"
INSERT INTO dbo.OggettoOrdinativo
(
    descrizioneProdotto,
    idDittaCostruttrice,
    modello,
    prezzoUnitarioNetto,
    prezzoInventario,
    idCategProdotti
)
VALUES
(
    @DescrizioneProdotto,
    @IdDittaCostruttrice,
    @Modello,
    @PrezzoUnitarioNetto,
    @PrezzoInventario,
    @IdCategoria
);
SELECT CAST(SCOPE_IDENTITY() AS int);", connection, transaction))
            {
                command.Parameters.Add("@DescrizioneProdotto", SqlDbType.VarChar, 256).Value = input.DescrizioneProdotto.Trim();
                command.Parameters.Add("@IdDittaCostruttrice", SqlDbType.Int).Value = NullableDb(input.IdDittaCostruttrice);
                command.Parameters.Add("@Modello", SqlDbType.VarChar, 64).Value = NullableDb(input.Modello);
                command.Parameters.Add("@PrezzoUnitarioNetto", SqlDbType.Money).Value = input.PrezzoUnitarioNetto ?? 0m;
                command.Parameters.Add("@PrezzoInventario", SqlDbType.Money).Value = input.PrezzoInventario ?? 0m;
                command.Parameters.Add("@IdCategoria", SqlDbType.Int).Value = NullableDb(input.IdCategoria);
                return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
        }

        private void CloseCurrentAssignment(SqlConnection connection, SqlTransaction transaction, int idProdotto, DateTime dataRestituzione)
        {
            int? idProdPers = null;
            using (var command = new SqlCommand(@"
SELECT TOP (1) IdProdPers
FROM dbo.ProdPers
WHERE IdProdotto = @IdProdotto
ORDER BY DataAssegnazione DESC, IdProdPers DESC;", connection, transaction))
            {
                command.Parameters.Add("@IdProdotto", SqlDbType.Int).Value = idProdotto;
                var scalar = command.ExecuteScalar();
                if (scalar != null && scalar != DBNull.Value)
                {
                    idProdPers = Convert.ToInt32(scalar, CultureInfo.InvariantCulture);
                }
            }

            if (!idProdPers.HasValue)
            {
                return;
            }

            using (var command = new SqlCommand(@"
UPDATE dbo.ProdPersStorico
SET dataRestituzione = @DataRestituzione
WHERE idProdPers = @IdProdPers AND dataRestituzione IS NULL;", connection, transaction))
            {
                command.Parameters.Add("@DataRestituzione", SqlDbType.Date).Value = dataRestituzione.Date;
                command.Parameters.Add("@IdProdPers", SqlDbType.Int).Value = idProdPers.Value;
                command.ExecuteNonQuery();
            }

            using (var command = new SqlCommand("DELETE FROM dbo.ProdPers WHERE IdProdPers = @IdProdPers;", connection, transaction))
            {
                command.Parameters.Add("@IdProdPers", SqlDbType.Int).Value = idProdPers.Value;
                command.ExecuteNonQuery();
            }
        }

        private void InsertStoricoSnapshot(SqlConnection connection, SqlTransaction transaction, int idProdPers, int idProdotto, int idPersonale, DateTime dataAssegnazione, DateTime? dataRestituzione, string noteOperazione)
        {
            var snapshot = ReadSnapshot(connection, transaction, idProdotto);
            using (var command = new SqlCommand(@"
INSERT INTO dbo.ProdPersStorico
(
    idProdPers,
    idProdotto,
    idPersonale,
    dataAssegnazione,
    dataRestituzione,
    numeroStanza,
    livelloEfficienza,
    noteProdotto,
    nomeMacchina,
    serialNumber
)
VALUES
(
    @IdProdPers,
    @IdProdotto,
    @IdPersonale,
    @DataAssegnazione,
    @DataRestituzione,
    @NumeroStanza,
    @LivelloEfficienza,
    @NoteProdotto,
    @NomeMacchina,
    @SerialNumber
);", connection, transaction))
            {
                command.Parameters.Add("@IdProdPers", SqlDbType.Int).Value = idProdPers;
                command.Parameters.Add("@IdProdotto", SqlDbType.Int).Value = idProdotto;
                command.Parameters.Add("@IdPersonale", SqlDbType.Int).Value = idPersonale;
                command.Parameters.Add("@DataAssegnazione", SqlDbType.Date).Value = dataAssegnazione.Date;
                command.Parameters.Add("@DataRestituzione", SqlDbType.Date).Value = NullableDb(dataRestituzione);
                command.Parameters.Add("@NumeroStanza", SqlDbType.NVarChar, 50).Value = NullableDb(snapshot.NumeroStanza);
                command.Parameters.Add("@LivelloEfficienza", SqlDbType.NVarChar, 50).Value = NullableDb(snapshot.LivelloEfficienza);
                command.Parameters.Add("@NoteProdotto", SqlDbType.NVarChar, 500).Value = NullableDb(ComposeStoricoNote(snapshot.NoteProdotto, noteOperazione));
                command.Parameters.Add("@NomeMacchina", SqlDbType.NVarChar, 50).Value = NullableDb(snapshot.NomeMacchina);
                command.Parameters.Add("@SerialNumber", SqlDbType.NVarChar, 50).Value = NullableDb(snapshot.SerialNumber);
                command.ExecuteNonQuery();
            }
        }

        private static SnapshotInfo ReadSnapshot(SqlConnection connection, SqlTransaction transaction, int idProdotto)
        {
            using (var command = new SqlCommand(@"
SELECT
    s.numero AS NumeroStanza,
    le.Livello_efficienza AS LivelloEfficienza,
    CAST(p.Note AS nvarchar(max)) AS NoteProdotto,
    p.Matricola AS SerialNumber,
    nm.NomeMacchina
FROM dbo.Prodotti p
LEFT JOIN dbo.Stanze s ON p.idStanza = s.idstanza
LEFT JOIN dbo.LivelliEfficenza le ON p.IdEfficienza = le.IdEfficienza
OUTER APPLY (
    SELECT TOP (1) nm.NomeMacchina
    FROM dbo.Postazione po
    LEFT JOIN dbo.NomeMacchina nm ON po.idNomeMacchina = nm.idnomemacchina
    WHERE po.idProdotto = p.IdProdotto
    ORDER BY po.idPostazione DESC
) nm
WHERE p.IdProdotto = @IdProdotto;", connection, transaction))
            {
                command.Parameters.Add("@IdProdotto", SqlDbType.Int).Value = idProdotto;
                using (var reader = command.ExecuteReader())
                {
                    if (!reader.Read())
                    {
                        return new SnapshotInfo();
                    }

                    return new SnapshotInfo
                    {
                        NumeroStanza = reader.GetStringOrEmpty("NumeroStanza"),
                        LivelloEfficienza = reader.GetStringOrEmpty("LivelloEfficienza"),
                        NoteProdotto = reader.GetStringOrEmpty("NoteProdotto"),
                        NomeMacchina = reader.GetStringOrEmpty("NomeMacchina"),
                        SerialNumber = reader.GetStringOrEmpty("SerialNumber")
                    };
                }
            }
        }

        private static string ComposeStoricoNote(string noteProdotto, string noteOperazione)
        {
            if (string.IsNullOrWhiteSpace(noteOperazione))
            {
                return noteProdotto;
            }

            if (string.IsNullOrWhiteSpace(noteProdotto))
            {
                return noteOperazione;
            }

            return noteProdotto + " | " + noteOperazione;
        }

        private static void UpdateDataUltimaMov(SqlConnection connection, SqlTransaction transaction, int idProdotto, string note)
        {
            using (var command = new SqlCommand(@"
UPDATE dbo.Prodotti
SET DataUltimaMov = GETDATE(),
    Note = CASE WHEN @Note IS NULL THEN Note ELSE CONVERT(ntext, @Note) END
WHERE IdProdotto = @IdProdotto;", connection, transaction))
            {
                command.Parameters.Add("@IdProdotto", SqlDbType.Int).Value = idProdotto;
                command.Parameters.Add("@Note", SqlDbType.NVarChar, -1).Value = NullableDb(note);
                command.ExecuteNonQuery();
            }
        }

        private static void UpsertNetworkData(SqlConnection connection, SqlTransaction transaction, int idProdotto, string macAddress, string note)
        {
            using (var delete = new SqlCommand("DELETE FROM dbo.NetworkData WHERE idProdotto = @IdProdotto;", connection, transaction))
            {
                delete.Parameters.Add("@IdProdotto", SqlDbType.Int).Value = idProdotto;
                delete.ExecuteNonQuery();
            }

            if (string.IsNullOrWhiteSpace(macAddress))
            {
                return;
            }

            using (var insert = new SqlCommand(@"
INSERT INTO dbo.NetworkData (idProdotto, macaddress, note)
VALUES (@IdProdotto, @MacAddress, @Note);", connection, transaction))
            {
                insert.Parameters.Add("@IdProdotto", SqlDbType.Int).Value = idProdotto;
                insert.Parameters.Add("@MacAddress", SqlDbType.NChar, 17).Value = macAddress;
                insert.Parameters.Add("@Note", SqlDbType.VarChar, 50).Value = NullableDb(note);
                insert.ExecuteNonQuery();
            }
        }

        private static void UpsertPostazione(SqlConnection connection, SqlTransaction transaction, int idProdotto, string nomeMacchina)
        {
            using (var delete = new SqlCommand("DELETE FROM dbo.Postazione WHERE idProdotto = @IdProdotto;", connection, transaction))
            {
                delete.Parameters.Add("@IdProdotto", SqlDbType.Int).Value = idProdotto;
                delete.ExecuteNonQuery();
            }

            if (string.IsNullOrWhiteSpace(nomeMacchina))
            {
                return;
            }

            int idNomeMacchina;
            using (var select = new SqlCommand("SELECT TOP (1) idnomemacchina FROM dbo.NomeMacchina WHERE NomeMacchina = @NomeMacchina ORDER BY idnomemacchina DESC;", connection, transaction))
            {
                select.Parameters.Add("@NomeMacchina", SqlDbType.VarChar, 25).Value = nomeMacchina.Trim();
                var existing = select.ExecuteScalar();
                if (existing != null && existing != DBNull.Value)
                {
                    idNomeMacchina = Convert.ToInt32(existing, CultureInfo.InvariantCulture);
                }
                else
                {
                    using (var insert = new SqlCommand("INSERT INTO dbo.NomeMacchina (NomeMacchina) VALUES (@NomeMacchina); SELECT CAST(SCOPE_IDENTITY() AS int);", connection, transaction))
                    {
                        insert.Parameters.Add("@NomeMacchina", SqlDbType.VarChar, 25).Value = nomeMacchina.Trim();
                        idNomeMacchina = Convert.ToInt32(insert.ExecuteScalar(), CultureInfo.InvariantCulture);
                    }
                }
            }

            using (var insert = new SqlCommand("INSERT INTO dbo.Postazione (idNomeMacchina, idProdotto) VALUES (@IdNomeMacchina, @IdProdotto);", connection, transaction))
            {
                insert.Parameters.Add("@IdNomeMacchina", SqlDbType.Int).Value = idNomeMacchina;
                insert.Parameters.Add("@IdProdotto", SqlDbType.Int).Value = idProdotto;
                insert.ExecuteNonQuery();
            }
        }

        private static int? FindProdottoByNomeMacchina(SqlConnection connection, SqlTransaction transaction, string nomeMacchina)
        {
            using (var command = new SqlCommand(@"
SELECT TOP (1) po.idProdotto
FROM dbo.NomeMacchina nm
INNER JOIN dbo.Postazione po ON po.idNomeMacchina = nm.idnomemacchina
WHERE LTRIM(RTRIM(LOWER(nm.NomeMacchina))) = LTRIM(RTRIM(LOWER(@NomeMacchina)))
ORDER BY po.idPostazione DESC;", connection, transaction))
            {
                command.Parameters.Add("@NomeMacchina", SqlDbType.VarChar, 25).Value = nomeMacchina.Trim();
                var scalar = command.ExecuteScalar();
                return scalar == null || scalar == DBNull.Value ? (int?)null : Convert.ToInt32(scalar, CultureInfo.InvariantCulture);
            }
        }

        private static string NormalizeMacAddress(string macAddress)
        {
            if (string.IsNullOrWhiteSpace(macAddress))
            {
                return null;
            }

            var normalized = macAddress.Trim().Replace("-", ":").Replace(".", string.Empty).ToUpperInvariant();
            if (normalized.Length == 12)
            {
                normalized = string.Join(":", Enumerable.Range(0, 6).Select(i => normalized.Substring(i * 2, 2)).ToArray());
            }

            return normalized;
        }

        private static bool IsNormalizedMacAddress(string macAddress)
        {
            if (string.IsNullOrWhiteSpace(macAddress) || macAddress.Length != 17)
            {
                return false;
            }

            for (var index = 0; index < macAddress.Length; index++)
            {
                if ((index + 1) % 3 == 0)
                {
                    if (macAddress[index] != ':')
                    {
                        return false;
                    }

                    continue;
                }

                var c = macAddress[index];
                if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F')))
                {
                    return false;
                }
            }

            return true;
        }

        private static object NullableDb(object value)
        {
            if (value == null)
            {
                return DBNull.Value;
            }

            if (value is string && string.IsNullOrWhiteSpace((string)value))
            {
                return DBNull.Value;
            }

            return value;
        }

        private IList<DominioItem> GetCategoriaItems()
        {
            using (var connection = Db.OpenConnection())
            using (var command = new SqlCommand(@"
SELECT c.IdCategoria, c.Descrizione, c.idTipoOO, c.ethernet, t.Descrizione AS TipoOggetto
FROM dbo.CategoriaProdotti c
INNER JOIN dbo.TipoOggettoOrdinativo t ON c.idTipoOO = t.idTipoOggOrdinativo
ORDER BY c.Descrizione;", connection))
            using (var reader = command.ExecuteReader())
            {
                var results = new List<DominioItem>();
                while (reader.Read())
                {
                    results.Add(new DominioItem
                    {
                        Id = reader.GetInt32Value("IdCategoria"),
                        Nome = reader.GetStringOrEmpty("Descrizione"),
                        ParentId = reader["idTipoOO"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["idTipoOO"], CultureInfo.InvariantCulture),
                        Extra = reader.GetStringOrEmpty("TipoOggetto"),
                        Flag = reader["ethernet"] != DBNull.Value && Convert.ToInt32(reader["ethernet"], CultureInfo.InvariantCulture) == 1
                    });
                }

                return results;
            }
        }

        private IList<DominioItem> GetLivelloItems()
        {
            using (var connection = Db.OpenConnection())
            using (var command = new SqlCommand("SELECT IdEfficienza, Codice, Livello_efficienza FROM dbo.LivelliEfficenza ORDER BY Livello_efficienza;", connection))
            using (var reader = command.ExecuteReader())
            {
                var results = new List<DominioItem>();
                while (reader.Read())
                {
                    results.Add(new DominioItem
                    {
                        Id = reader.GetInt32Value("IdEfficienza"),
                        Codice = reader.GetStringOrEmpty("Codice"),
                        Nome = reader.GetStringOrEmpty("Livello_efficienza")
                    });
                }

                return results;
            }
        }

        private IList<DominioItem> GetStanzaItems()
        {
            using (var connection = Db.OpenConnection())
            using (var command = new SqlCommand("SELECT idstanza, numero FROM dbo.Stanze ORDER BY numero;", connection))
            using (var reader = command.ExecuteReader())
            {
                var results = new List<DominioItem>();
                while (reader.Read())
                {
                    results.Add(new DominioItem
                    {
                        Id = reader.GetInt32Value("idstanza"),
                        Nome = reader.GetStringOrEmpty("numero")
                    });
                }

                return results;
            }
        }

        private IList<DominioItem> GetDittaItems()
        {
            using (var connection = Db.OpenConnection())
            using (var command = new SqlCommand("SELECT IdDitta, Nome, Citta, Mail, tipologia FROM dbo.Ditte ORDER BY Nome;", connection))
            using (var reader = command.ExecuteReader())
            {
                var results = new List<DominioItem>();
                while (reader.Read())
                {
                    results.Add(new DominioItem
                    {
                        Id = reader.GetInt32Value("IdDitta"),
                        Nome = reader.GetStringOrEmpty("Nome"),
                        Extra = reader.GetStringOrEmpty("Citta"),
                        Extra2 = reader.GetStringOrEmpty("Mail"),
                        Codice = reader.GetStringOrEmpty("tipologia")
                    });
                }

                return results;
            }
        }

        private IList<DominioItem> GetTipoOggettoItems()
        {
            using (var connection = Db.OpenConnection())
            using (var command = new SqlCommand("SELECT idTipoOggOrdinativo, Descrizione FROM dbo.TipoOggettoOrdinativo ORDER BY Descrizione;", connection))
            using (var reader = command.ExecuteReader())
            {
                var results = new List<DominioItem>();
                while (reader.Read())
                {
                    results.Add(new DominioItem
                    {
                        Id = reader.GetInt32Value("idTipoOggOrdinativo"),
                        Nome = reader.GetStringOrEmpty("Descrizione")
                    });
                }

                return results;
            }
        }

        private void ExecuteDomainNonQuery(string source, string sql, Action<SqlCommand> parameterizer)
        {
            AppLogger.Info(source, "Aggiornamento anagrafica dominio.");
            using (var connection = Db.OpenConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                parameterizer(command);
                command.ExecuteNonQuery();
            }
        }

        private int ExecuteInsertScalar(string source, string sql, Action<SqlCommand> parameterizer)
        {
            AppLogger.Info(source, "Inserimento amministrativo.");
            using (var connection = Db.OpenConnection())
            using (var command = new SqlCommand(sql, connection))
            {
                parameterizer(command);
                return Convert.ToInt32(command.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
        }

        private void ExecuteDelete(string source, string sql, int id)
        {
            ExecuteDomainNonQuery(source, sql, command => command.Parameters.Add("@Id", SqlDbType.Int).Value = id);
        }

        private static string NormalizeSingleChar(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            return value.Trim().Substring(0, 1);
        }

        private static int? EnsureNomeMacchina(SqlConnection connection, SqlTransaction transaction, string nomeMacchina, int? idNomeMacchina)
        {
            if (idNomeMacchina.HasValue)
            {
                if (!string.IsNullOrWhiteSpace(nomeMacchina))
                {
                    using (var update = new SqlCommand("UPDATE dbo.NomeMacchina SET NomeMacchina = @Nome WHERE idnomemacchina = @Id;", connection, transaction))
                    {
                        update.Parameters.Add("@Id", SqlDbType.Int).Value = idNomeMacchina.Value;
                        update.Parameters.Add("@Nome", SqlDbType.VarChar, 25).Value = nomeMacchina.Trim();
                        update.ExecuteNonQuery();
                    }
                }

                return idNomeMacchina.Value;
            }

            if (string.IsNullOrWhiteSpace(nomeMacchina))
            {
                return null;
            }

            using (var select = new SqlCommand("SELECT TOP (1) idnomemacchina FROM dbo.NomeMacchina WHERE NomeMacchina = @Nome ORDER BY idnomemacchina DESC;", connection, transaction))
            {
                select.Parameters.Add("@Nome", SqlDbType.VarChar, 25).Value = nomeMacchina.Trim();
                var existing = select.ExecuteScalar();
                if (existing != null && existing != DBNull.Value)
                {
                    return Convert.ToInt32(existing, CultureInfo.InvariantCulture);
                }
            }

            using (var insert = new SqlCommand("INSERT INTO dbo.NomeMacchina (NomeMacchina) VALUES (@Nome); SELECT CAST(SCOPE_IDENTITY() AS int);", connection, transaction))
            {
                insert.Parameters.Add("@Nome", SqlDbType.VarChar, 25).Value = nomeMacchina.Trim();
                return Convert.ToInt32(insert.ExecuteScalar(), CultureInfo.InvariantCulture);
            }
        }

        private static void DeleteNomeMacchinaIfOrphan(SqlConnection connection, SqlTransaction transaction, int? idNomeMacchina)
        {
            if (!idNomeMacchina.HasValue)
            {
                return;
            }

            using (var check = new SqlCommand("SELECT COUNT(*) FROM dbo.Postazione WHERE idNomeMacchina = @Id;", connection, transaction))
            {
                check.Parameters.Add("@Id", SqlDbType.Int).Value = idNomeMacchina.Value;
                var count = Convert.ToInt32(check.ExecuteScalar(), CultureInfo.InvariantCulture);
                if (count > 0)
                {
                    return;
                }
            }

            using (var delete = new SqlCommand("DELETE FROM dbo.NomeMacchina WHERE idnomemacchina = @Id;", connection, transaction))
            {
                delete.Parameters.Add("@Id", SqlDbType.Int).Value = idNomeMacchina.Value;
                delete.ExecuteNonQuery();
            }
        }

        private static IList<LookupItem> ReadLookup(string sql, Func<IDataRecord, LookupItem> mapper)
        {
            using (var connection = Db.OpenConnection())
            using (var command = new SqlCommand(sql, connection))
            using (var reader = command.ExecuteReader())
            {
                var results = new List<LookupItem>();
                while (reader.Read())
                {
                    results.Add(mapper(reader));
                }

                return results;
            }
        }

        private void ExecuteProdottoUpdateWithTransaction(string sql, int idProdotto, Action<SqlConnection, SqlTransaction, SqlCommand> parameterizer, string source)
        {
            AppLogger.Info(source, "Aggiornamento materiale " + idProdotto.ToString(CultureInfo.InvariantCulture) + ".");
            using (var connection = Db.OpenConnection())
            using (var transaction = connection.BeginTransaction())
            {
                try
                {
                    using (var command = new SqlCommand(sql, connection, transaction))
                    {
                        command.Parameters.Add("@IdProdotto", SqlDbType.Int).Value = idProdotto;
                        parameterizer(connection, transaction, command);
                        command.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }

        private sealed class SnapshotInfo
        {
            public string NumeroStanza { get; set; }
            public string LivelloEfficienza { get; set; }
            public string NoteProdotto { get; set; }
            public string NomeMacchina { get; set; }
            public string SerialNumber { get; set; }
        }

        public class ImportReteInputItem
        {
            public int Riga { get; set; }
            public string NomeMacchina { get; set; }
            public string MacAddress { get; set; }
        }

        public class ImportReteResultItem
        {
            public int Riga { get; set; }
            public int? IdProdotto { get; set; }
            public string NomeMacchina { get; set; }
            public string MacAddress { get; set; }
            public string Esito { get; set; }
            public string Messaggio { get; set; }
        }
    }
}
