using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using MaterialiGestioneWeb.Infrastructure;

namespace MaterialiGestioneWeb.Auth
{
    public class BaseAuthenticatedPage : Page
    {
        protected virtual int[] LivelliConsentiti
        {
            get { return new int[0]; }
        }

        protected override void OnInit(EventArgs e)
        {
            if (HttpContext.Current == null ||
                HttpContext.Current.User == null ||
                HttpContext.Current.User.Identity == null ||
                !HttpContext.Current.User.Identity.IsAuthenticated)
            {
                AppLogger.Info("BaseAuthenticatedPage.OnLoad", "Richiesta non autenticata. Url=" + Request.RawUrl);
                Response.Redirect("~/AccessDenied.aspx", true);
                return;
            }

            string windowsUser = HttpContext.Current.User.Identity.Name;
            if (string.IsNullOrWhiteSpace(windowsUser))
            {
                AppLogger.Info("BaseAuthenticatedPage.OnInit", "Identity.Name vuoto. Url=" + Request.RawUrl);
                Response.Redirect("~/AccessDenied.aspx", true);
                return;
            }

            UtenteAutorizzato previousSessionUser = GetUtenteFromSession();
            UtenteAutorizzato utente = GetUtenteAutorizzato(windowsUser);
            if (utente == null)
            {
                AppLogger.Info("BaseAuthenticatedPage.OnInit", "Utente Windows non autorizzato. WindowsUser=" + windowsUser + "; Url=" + Request.RawUrl);
                ClearAuthSession();
                Response.Redirect("~/AccessDenied.aspx", true);
                return;
            }

            Session["UserId"] = utente.Id;
            Session["Username"] = utente.Username;
            Session["Livello"] = utente.Livello;
            Session["WindowsUser"] = windowsUser;

            if (previousSessionUser == null)
            {
                AppLogger.Info(
                    "BaseAuthenticatedPage.OnInit",
                    "Utente autorizzato. Username=" + utente.Username + "; WindowsUser=" + windowsUser + "; Livello=" + utente.Livello + "; Url=" + Request.RawUrl);
            }
            else if (previousSessionUser.Livello != utente.Livello || previousSessionUser.Id != utente.Id || previousSessionUser.Username != utente.Username)
            {
                AppLogger.Info(
                    "BaseAuthenticatedPage.OnInit",
                    "Autorizzazione aggiornata da tabella. Username=" + utente.Username + "; WindowsUser=" + windowsUser + "; LivelloPrecedente=" + previousSessionUser.Livello + "; Livello=" + utente.Livello + "; Url=" + Request.RawUrl);
            }

            if (LivelliConsentiti.Length > 0 && !LivelliConsentiti.Contains(utente.Livello))
            {
                AppLogger.Info(
                    "BaseAuthenticatedPage.OnInit",
                    "Accesso negato per livello. Username=" + utente.Username + "; Livello=" + utente.Livello + "; Url=" + Request.RawUrl);
                Response.Redirect("~/AccessDenied.aspx", true);
                return;
            }

            base.OnInit(e);
        }

        private UtenteAutorizzato GetUtenteFromSession()
        {
            if (Session["UserId"] == null || Session["Username"] == null || Session["Livello"] == null)
                return null;

            int id;
            int livello;
            if (!int.TryParse(Session["UserId"].ToString(), out id) ||
                !int.TryParse(Session["Livello"].ToString(), out livello))
                return null;

            return new UtenteAutorizzato
            {
                Id = id,
                Username = Session["Username"].ToString(),
                Livello = livello
            };
        }

        private void ClearAuthSession()
        {
            Session.Remove("UserId");
            Session.Remove("Username");
            Session.Remove("Livello");
            Session.Remove("WindowsUser");
        }

        private UtenteAutorizzato GetUtenteAutorizzato(string windowsUser)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MaterialiDb"].ConnectionString;
            string usernameSolo = windowsUser;

            if (windowsUser.Contains("\\"))
                usernameSolo = windowsUser.Split('\\')[1];

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(@"
SELECT TOP 1 Id, Username, Livello
FROM dbo.AutorizzazioniMateriali
WHERE Attivo = 1
  AND (Username = @UsernameCompleto OR Username = @UsernameSolo)", conn))
            {
                cmd.Parameters.AddWithValue("@UsernameCompleto", windowsUser);
                cmd.Parameters.AddWithValue("@UsernameSolo", usernameSolo);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.Read())
                        return null;

                    return new UtenteAutorizzato
                    {
                        Id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0,
                        Username = reader["Username"] != DBNull.Value ? reader["Username"].ToString() : "",
                        Livello = reader["Livello"] != DBNull.Value ? Convert.ToInt32(reader["Livello"]) : 0
                    };
                }
            }
        }
    }
}
