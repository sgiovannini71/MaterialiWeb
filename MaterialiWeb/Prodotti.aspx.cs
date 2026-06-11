using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI.WebControls;
using MaterialiGestioneWeb.Infrastructure;
using MaterialiGestioneWeb.Models;
using MaterialiGestioneWeb.Services;

namespace MaterialiGestioneWeb
{
    public partial class Prodotti : Auth.BaseAuthenticatedPage
    {
        private readonly InventarioRepository _repository = new InventarioRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindEfficienze();
                BindGrid(null);
            }
        }

        protected void EfficienzaDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindGrid(SearchText.Text);
        }

        protected void SearchButton_Click(object sender, EventArgs e)
        {
            BindGrid(SearchText.Text);
        }

        protected void ResetButton_Click(object sender, EventArgs e)
        {
            SearchText.Text = string.Empty;
            EfficienzaDropDown.ClearSelection();
            BindGrid(null);
        }

        protected void ExportCsvButton_Click(object sender, EventArgs e)
        {
            try
            {
                ErrorPanel.Visible = false;
                if (!ExportCsvButton.Enabled)
                {
                    throw new InvalidOperationException("Caricare prima dei dati da esportare.");
                }

                var prodotti = _repository.GetProdotti(SearchText.Text, ParseOptionalInt(EfficienzaDropDown.SelectedValue));
                ExportCsv(prodotti);
            }
            catch (Exception ex)
            {
                ErrorPanel.Visible = true;
                ErrorMessage.Text = Server.HtmlEncode(ex.Message);
            }
        }

        private void BindGrid(string search)
        {
            try
            {
                ErrorPanel.Visible = false;
                var prodotti = _repository.GetProdotti(search, ParseOptionalInt(EfficienzaDropDown.SelectedValue));
                ProdottiGrid.DataSource = prodotti;
                ProdottiGrid.DataBind();
                SetExportAvailability(prodotti != null && prodotti.Count > 0);
            }
            catch (Exception ex)
            {
                SetExportAvailability(false);
                ErrorPanel.Visible = true;
                ErrorMessage.Text = Server.HtmlEncode(ex.Message);
            }
        }

        private void BindEfficienze()
        {
            EfficienzaDropDown.Items.Clear();
            EfficienzaDropDown.DataSource = _repository.GetLivelliEfficienzaLookup();
            EfficienzaDropDown.DataTextField = "Nome";
            EfficienzaDropDown.DataValueField = "Id";
            EfficienzaDropDown.DataBind();
            EfficienzaDropDown.Items.Insert(0, new ListItem("-- tutti gli stati --", string.Empty));
        }

        private static int? ParseOptionalInt(string value)
        {
            int parsed;
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) ? (int?)parsed : null;
        }

        private void SetExportAvailability(bool enabled)
        {
            ExportCsvButton.Enabled = enabled;
        }

        private void ExportCsv(IList<ProdottoCorrente> prodotti)
        {
            CsvExport.Write(this, "Prodotti", new[]
            {
                "Categorico",
                "Descrizione",
                "Matricola",
                "Categoria",
                "Tipo oggetto",
                "Modello",
                "Ditta costruttrice",
                "Stato",
                "Assegnatario",
                "Stanza",
                "Nome macchina",
                "MAC",
                "Data ultima movimentazione",
                "Versamento",
                "Note"
            }, BuildCsvRows(prodotti));
        }

        private static IEnumerable<IEnumerable<string>> BuildCsvRows(IEnumerable<ProdottoCorrente> prodotti)
        {
            foreach (var prodotto in prodotti)
            {
                yield return new[]
                {
                    prodotto.Categorico.HasValue ? prodotto.Categorico.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                    prodotto.DescrizioneProdotto,
                    prodotto.Matricola,
                    prodotto.Categoria,
                    prodotto.TipoOggetto,
                    prodotto.Modello,
                    prodotto.DittaCostruttrice,
                    prodotto.LivelloEfficienza,
                    prodotto.AssegnatarioDisplay,
                    prodotto.NumeroStanza,
                    prodotto.NomeMacchina,
                    prodotto.MacAddress,
                    prodotto.DataUltimaMov.HasValue ? prodotto.DataUltimaMov.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty,
                    prodotto.Versamento,
                    prodotto.Note
                };
            }
        }
    }
}
