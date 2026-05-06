using System.Configuration;
using System.Data.SqlClient;
using MaterialiGestioneWeb.Infrastructure;

namespace MaterialiGestioneWeb.Data
{
    public static class DipendentiDb
    {
        public static SqlConnection OpenConnection()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["DipendentiDBConnectionString"].ConnectionString;
            var connection = new SqlConnection(connectionString);
            connection.Open();
            AppLogger.Info("DipendentiDb.OpenConnection", "Connessione SQL aperta verso DipendentiDB.");
            return connection;
        }
    }
}
