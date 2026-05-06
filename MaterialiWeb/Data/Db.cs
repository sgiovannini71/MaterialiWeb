using System.Configuration;
using System.Data.SqlClient;
using MaterialiGestioneWeb.Infrastructure;

namespace MaterialiGestioneWeb.Data
{
    public static class Db
    {
        public static SqlConnection OpenConnection()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["MaterialiDb"].ConnectionString;
            var connection = new SqlConnection(connectionString);
            connection.Open();
            AppLogger.Info("Db.OpenConnection", "Connessione SQL aperta verso MaterialiDb.");
            return connection;
        }
    }
}
