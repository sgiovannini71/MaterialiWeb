using System;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using MaterialiGestioneWeb.Infrastructure;
using MaterialiGestioneWeb.Models;
using MaterialiGestioneWeb.Services;

namespace MaterialiGestioneWeb
{
    public partial class DettaglioOrdinativoPage : Auth.BaseAuthenticatedPage
    {
        private readonly InventarioRepository _repository = new InventarioRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                return;
            }

            try
            {
                BindLookups();
                LoadDetailFromQueryString();
                if (OrdinativiListPanel.Visible)
                {
                    BindOrdinativiGrid();
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("DettaglioOrdinativoPage.Page_Load", "Errore durante il caricamento iniziale del dettaglio ordinativo.", ex);
                ShowError(ex.Message);
            }
        }

        protected void OrdinativoDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedId = ParseSelectedId(OrdinativoDropDown);
            if (!selectedId.HasValue)
            {
                Response.Redirect("DettaglioOrdinativo.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            Response.Redirect("DettaglioOrdinativo.aspx?id=" + selectedId.Value, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void SearchButton_Click(object sender, EventArgs e)
        {
            try
            {
                BindOrdinativiGrid(SearchText.Text);
            }
            catch (Exception ex)
            {
                AppLogger.Error("DettaglioOrdinativoPage.SearchButton_Click", "Errore durante la ricerca ordinativi.", ex);
                ShowError(ex.Message);
            }
        }

        protected void ResetButton_Click(object sender, EventArgs e)
        {
            SearchText.Text = string.Empty;
            ErrorPanel.Visible = false;
            BindOrdinativiGrid();
        }

        protected void OggettiGrid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
            {
                return;
            }

            var item = e.Row.DataItem as OggettoOrdinativoDettaglioItem;
            var prodottiRepeater = e.Row.FindControl("ProdottiOggettoRepeater") as Repeater;
            var emptyPlaceholder = e.Row.FindControl("NoProdottiPlaceholder") as PlaceHolder;
            if (item == null || prodottiRepeater == null)
            {
                return;
            }

            var prodotti = item.Prodotti ?? new System.Collections.Generic.List<ProdottoOrdinativoItem>();
            prodottiRepeater.DataSource = prodotti;
            prodottiRepeater.DataBind();
            if (emptyPlaceholder != null)
            {
                emptyPlaceholder.Visible = prodotti.Count == 0;
            }
        }

        private void BindLookups()
        {
            OrdinativoDropDown.DataSource = _repository.GetOrdinativiLookup();
            OrdinativoDropDown.DataTextField = "Nome";
            OrdinativoDropDown.DataValueField = "Id";
            OrdinativoDropDown.DataBind();
            OrdinativoDropDown.Items.Insert(0, new ListItem("-- seleziona ordinativo --", string.Empty));
        }

        private void BindOrdinativiGrid(string search = null)
        {
            var items = _repository.GetOrdinativiAdmin();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                items = items.Where(item =>
                    ContainsInsensitive(item.CodiceOrdinativo, term) ||
                    ContainsInsensitive(item.DenominazioneOrdinativo, term) ||
                    ContainsInsensitive(item.DittaDescrizione, term) ||
                    ContainsInsensitive(item.EnteStipulante, term) ||
                    ContainsInsensitive(item.EstremiOrdinativo, term)).ToList();
            }

            OrdinativiListGrid.DataSource = items;
            OrdinativiListGrid.DataBind();
        }

        private void LoadDetailFromQueryString()
        {
            int idOrdinativo;
            if (!int.TryParse(Request.QueryString["id"], out idOrdinativo))
            {
                DetailPanel.Visible = false;
                OrdinativiListPanel.Visible = true;
                return;
            }

            var listItem = OrdinativoDropDown.Items.FindByValue(idOrdinativo.ToString());
            if (listItem != null)
            {
                OrdinativoDropDown.ClearSelection();
                listItem.Selected = true;
            }

            LoadDetail(idOrdinativo);
        }

        private void LoadDetail(int idOrdinativo)
        {
            var detail = _repository.GetOrdinativoDettaglio(idOrdinativo);
            if (detail == null || detail.Ordinativo == null)
            {
                ShowError("Ordinativo non trovato.");
                DetailPanel.Visible = false;
                OrdinativiListPanel.Visible = true;
                return;
            }

            var ordinativo = detail.Ordinativo;
            var totaleQuantita = detail.Oggetti.Sum(item => item.Quantita);
            var totaleProdotti = detail.Oggetti.Sum(item => item.ProdottiGenerati);

            DetailPanel.Visible = true;
            OrdinativiListPanel.Visible = false;
            NomeOrdinativo.Text = Server.HtmlEncode(string.IsNullOrWhiteSpace(ordinativo.DenominazioneOrdinativo) ? "Ordinativo" : ordinativo.DenominazioneOrdinativo);
            DescrizioneOrdinativo.Text = Server.HtmlEncode(string.Join(" - ", new[]
            {
                ordinativo.CodiceOrdinativo,
                ordinativo.DittaDescrizione,
                ordinativo.EstremiOrdinativo
            }).Trim(' ', '-'));
            IdOrdinativoText.Text = ordinativo.IdOrdinativo.ToString();
            CodiceText.Text = SafeText(ordinativo.CodiceOrdinativo);
            DenominazioneText.Text = SafeText(ordinativo.DenominazioneOrdinativo);
            DittaText.Text = SafeText(ordinativo.DittaDescrizione);
            EnteText.Text = SafeText(ordinativo.EnteStipulante);
            EstremiText.Text = SafeText(ordinativo.EstremiOrdinativo);
            EfText.Text = SafeText(ordinativo.EF);
            TipoText.Text = SafeText(ordinativo.TipoOrdinativo);

            OggettiCount.Text = detail.Oggetti.Count.ToString();
            QuantitaCount.Text = totaleQuantita.ToString();
            ProdottiCount.Text = totaleProdotti.ToString();
            StatiProdottiText.Text = BuildStatiProdottiSummary(detail);

            CrudOrdinativoLink.NavigateUrl = "GestioneCrud.aspx#crud-ordinativi";
            var oggettiUrl = "GestioneCrud.aspx?idOrdinativo=" + idOrdinativo.ToString(CultureInfo.InvariantCulture) + "#crud-oggetti";
            CrudOggettiLink.NavigateUrl = oggettiUrl;
            AddOggettoLink.NavigateUrl = oggettiUrl;

            OggettiGrid.DataSource = detail.Oggetti;
            OggettiGrid.DataBind();
        }

        private string BuildStatiProdottiSummary(OrdinativoDettaglio detail)
        {
            return BuildStatiProdottiSummary(detail.Oggetti
                .SelectMany(item => item.Prodotti == null
                    ? Enumerable.Empty<ProdottoOrdinativoItem>()
                    : item.Prodotti));
        }

        protected string FormatOggettoStati(object dataItem)
        {
            var oggetto = dataItem as OggettoOrdinativoDettaglioItem;
            return BuildStatiProdottiSummary(oggetto == null
                ? Enumerable.Empty<ProdottoOrdinativoItem>()
                : oggetto.Prodotti == null
                    ? Enumerable.Empty<ProdottoOrdinativoItem>()
                    : oggetto.Prodotti);
        }

        private string BuildStatiProdottiSummary(System.Collections.Generic.IEnumerable<ProdottoOrdinativoItem> prodotti)
        {
            var summary = prodotti
                .GroupBy(item => string.IsNullOrWhiteSpace(item.LivelloEfficienza) ? "Non indicato" : item.LivelloEfficienza)
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key)
                .Select(group => "<span>" + Server.HtmlEncode(group.Key) + "<strong>" + group.Count() + "</strong></span>")
                .ToList();

            return summary.Count == 0
                ? "<span>Nessun prodotto generato</span>"
                : string.Join(string.Empty, summary);
        }

        private int? ParseSelectedId(ListControl control)
        {
            int id;
            return int.TryParse(control.SelectedValue, out id) ? id : (int?)null;
        }

        private static bool ContainsInsensitive(string value, string search)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                value.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private string SafeText(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : Server.HtmlEncode(value);
        }

        protected string FormatProductMeta(object dataItem)
        {
            var prodotto = dataItem as ProdottoOrdinativoItem;
            if (prodotto == null)
            {
                return string.Empty;
            }

            var parts = new[]
            {
                string.IsNullOrWhiteSpace(prodotto.Matricola) ? null : "Matricola " + prodotto.Matricola,
                string.IsNullOrWhiteSpace(prodotto.LivelloEfficienza) ? null : prodotto.LivelloEfficienza,
                string.IsNullOrWhiteSpace(prodotto.NumeroStanza) ? null : "Stanza " + prodotto.NumeroStanza
            }
            .Where(value => !string.IsNullOrWhiteSpace(value));

            return Server.HtmlEncode(string.Join(" | ", parts));
        }

        private void ShowError(string message)
        {
            ErrorPanel.Visible = true;
            ErrorMessage.Text = Server.HtmlEncode(message);
        }
    }
}
