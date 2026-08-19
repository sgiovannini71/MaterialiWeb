using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;

namespace MaterialiGestioneWeb.Infrastructure
{
    public class BaseAuthenticatedPage : Page
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var identity = HttpContext.Current.User == null
                ? null
                : HttpContext.Current.User.Identity;

            if (identity == null || !identity.IsAuthenticated || string.IsNullOrWhiteSpace(identity.Name))
            {
                DenyAccess("Utente Windows non autenticato.");
                return;
            }

            var windowsUser = identity.Name.Trim();
            AuthorizedUser authorizedUser;
            try
            {
                authorizedUser = FindAuthorizedUser(windowsUser);
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    "BaseAuthenticatedPage.OnInit",
                    "Errore durante la verifica dell'utente Windows " + windowsUser + ".",
                    ex);
                Response.Redirect("~/Account/AccessDenied.aspx?reason=error", true);
                return;
            }

            if (authorizedUser == null)
            {
                DenyAccess("Utente Windows non presente in Autorizzazioni: " + windowsUser);
                return;
            }

            Session["UserId"] = authorizedUser.Id;
            Session["Username"] = authorizedUser.Username;
            Session["Livello"] = authorizedUser.Livello;
            Session["WindowsUser"] = windowsUser;

            AppLogger.Info(
                "BaseAuthenticatedPage.OnInit",
                "Utente autorizzato: " + authorizedUser.Username
                + "; livello: " + authorizedUser.Livello
                + "; identità Windows: " + windowsUser + ".");
        }

        private static AuthorizedUser FindAuthorizedUser(string windowsUser)
        {
            var separatorIndex = windowsUser.LastIndexOf('\\');
            var usernameOnly = separatorIndex >= 0
                ? windowsUser.Substring(separatorIndex + 1)
                : windowsUser;

            var connectionSetting = ConfigurationManager.ConnectionStrings["MaterialiDb"];
            if (connectionSetting == null || string.IsNullOrWhiteSpace(connectionSetting.ConnectionString))
                throw new ConfigurationErrorsException("Connection string MaterialiDb non configurata.");

            using (var connection = new SqlConnection(connectionSetting.ConnectionString))
            using (var command = new SqlCommand(
                @"SELECT TOP (1) id, username, livello
                  FROM dbo.Autorizzazioni
                  WHERE username = @UsernameCompleto
                     OR username = @UsernameSolo",
                connection))
            {
                command.Parameters.Add("@UsernameCompleto", SqlDbType.NVarChar, 256).Value = windowsUser;
                command.Parameters.Add("@UsernameSolo", SqlDbType.NVarChar, 256).Value = usernameOnly;
                connection.Open();

                using (var reader = command.ExecuteReader(CommandBehavior.SingleRow))
                {
                    if (!reader.Read())
                        return null;

                    return new AuthorizedUser
                    {
                        Id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt32(reader["id"]),
                        Username = reader["username"] == DBNull.Value ? string.Empty : Convert.ToString(reader["username"]),
                        Livello = reader["livello"] == DBNull.Value ? 0 : Convert.ToInt32(reader["livello"])
                    };
                }
            }
        }

        private void DenyAccess(string logMessage)
        {
            AppLogger.Info("BaseAuthenticatedPage.OnInit", logMessage);
            Response.Redirect("~/Account/AccessDenied.aspx", true);
        }

        private sealed class AuthorizedUser
        {
            public int Id { get; set; }
            public string Username { get; set; }
            public int Livello { get; set; }
        }
    }
}
