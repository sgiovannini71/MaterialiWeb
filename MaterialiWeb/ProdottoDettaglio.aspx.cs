using System;
using System.Linq;
using MaterialiGestioneWeb.Infrastructure;
using MaterialiGestioneWeb.Services;

namespace MaterialiGestioneWeb
{
    public partial class ProdottoDettaglioPage : Auth.BaseAuthenticatedPage
    {
        private readonly InventarioRepository _repository = new InventarioRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadDetail();
            }
        }

        private void LoadDetail()
        {
            int idProdotto;
            if (!int.TryParse(Request.QueryString["id"], out idProdotto))
            {
                ShowError("Parametro id mancante o non valido.");
                return;
            }

            try
            {
                var detail = _repository.GetProdottoDettaglio(idProdotto);
                if (detail == null || detail.Prodotto == null)
                {
                    ShowError("Prodotto non trovato.");
                    return;
                }

                var prodotto = detail.Prodotto;
                DetailPanel.Visible = true;
                NomeProdotto.Text = Server.HtmlEncode(prodotto.DescrizioneProdotto);
                Categorico.Text = prodotto.Categorico.HasValue ? prodotto.Categorico.Value.ToString() : "-";
                Seriale.Text = Server.HtmlEncode(prodotto.Matricola);
                Tipologia.Text = Server.HtmlEncode(prodotto.Categoria);
                Modello.Text = Server.HtmlEncode(prodotto.Modello);
                Stato.Text = Server.HtmlEncode(prodotto.LivelloEfficienza);
                Assegnatario.Text = string.IsNullOrWhiteSpace(prodotto.AssegnatarioDisplay) ? "-" : Server.HtmlEncode(prodotto.AssegnatarioDisplay);
                Ubicazione.Text = string.IsNullOrWhiteSpace(prodotto.NumeroStanza) ? "-" : Server.HtmlEncode(prodotto.NumeroStanza);
                Fornitore.Text = string.IsNullOrWhiteSpace(prodotto.DittaCostruttrice) ? "-" : Server.HtmlEncode(prodotto.DittaCostruttrice);
                Fattura.Text = string.IsNullOrWhiteSpace(prodotto.Versamento) ? "-" : Server.HtmlEncode(prodotto.Versamento);

                var assegnazioneCorrente = detail.StoricoAssegnazioni.FirstOrDefault(item => !item.DataFine.HasValue);
                DischiGrid.DataSource = assegnazioneCorrente == null ? null : new[] { assegnazioneCorrente };
                DischiGrid.DataBind();
                DocumentiGrid.DataSource = detail.Ordinativo != null && detail.Ordinativo.IdOrdinativo.HasValue ? new[] { detail.Ordinativo } : null;
                DocumentiGrid.DataBind();
                StoricoAssegnazioniGrid.DataSource = detail.StoricoAssegnazioni;
                StoricoAssegnazioniGrid.DataBind();

                AssegnaLink.NavigateUrl = "AssegnaBene.aspx?id=" + idProdotto;
                RientroLink.NavigateUrl = "RientroRiassegnazione.aspx?id=" + idProdotto;
                StatoLink.NavigateUrl = "CambiaStato.aspx?id=" + idProdotto;
                UbicazioneLink.NavigateUrl = "CambiaUbicazione.aspx?id=" + idProdotto;
                ConfiguraLink.NavigateUrl = "ConfiguraComputer.aspx?id=" + idProdotto;
                DismettiLink.NavigateUrl = "DismettiBene.aspx?id=" + idProdotto;

                if (!string.IsNullOrWhiteSpace(prodotto.NomeMacchina) || !string.IsNullOrWhiteSpace(prodotto.MacAddress))
                {
                    ComputerPanel.Visible = true;
                    NonComputerPanel.Visible = false;
                    ComputerProcessore.Text = Server.HtmlEncode(prodotto.TipoOggetto);
                    ComputerRam.Text = prodotto.DataUltimaMov.HasValue ? prodotto.DataUltimaMov.Value.ToString("dd/MM/yyyy HH:mm") : "-";
                    ComputerTipoRam.Text = Server.HtmlEncode(prodotto.NomeMacchina);
                    ComputerImpiego.Text = Server.HtmlEncode(prodotto.MacAddress);
                    ComputerMac.Text = Server.HtmlEncode(prodotto.MacAddress);
                    ComputerHostName.Text = Server.HtmlEncode(prodotto.Note);
                }
                else
                {
                    ComputerPanel.Visible = false;
                    NonComputerPanel.Visible = true;
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("ProdottoDettaglioPage.LoadDetail", "Errore durante il caricamento del dettaglio materiale. QueryStringId=" + Request.QueryString["id"], ex);
                ShowError(ex.Message);
            }
        }

        private void ShowError(string message)
        {
            ErrorPanel.Visible = true;
            ErrorMessage.Text = Server.HtmlEncode(message);
        }
    }
}
