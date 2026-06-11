using System;
using MaterialiGestioneWeb.Infrastructure;
using MaterialiGestioneWeb.Services;

namespace MaterialiGestioneWeb
{
    public partial class Default : Auth.BaseAuthenticatedPage
    {
        private readonly InventarioRepository _repository = new InventarioRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadDashboard();
            }
        }

        private void LoadDashboard()
        {
            try
            {
                var stats = _repository.GetDashboardStats();
                ProdottiCount.Text = stats.TotaleProdotti.ToString("N0");
                ComputerCount.Text = stats.TotaleConRete.ToString("N0");
                AssegnatiCount.Text = stats.TotaleAssegnati.ToString("N0");
                DaVersareCount.Text = stats.TotaleDaVersare.ToString("N0");
                AttesaDistribuzioneCount.Text = stats.TotaleConPostazione.ToString("N0");
                AlienatiCount.Text = stats.TotaleFuoriStanza.ToString("N0");
                SenzaAssegnatarioCount.Text = stats.TotaleSenzaAssegnazione.ToString("N0");
            }
            catch (Exception ex)
            {
                AppLogger.Error("Default.LoadDashboard", "Errore durante il caricamento della dashboard.", ex);
                ErrorPanel.Visible = true;
                ErrorMessage.Text = Server.HtmlEncode(ex.Message);
            }
        }
    }
}
