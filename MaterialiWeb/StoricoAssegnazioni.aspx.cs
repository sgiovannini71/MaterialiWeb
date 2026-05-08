using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI.WebControls;
using MaterialiGestioneWeb.Infrastructure;
using MaterialiGestioneWeb.Models;
using MaterialiGestioneWeb.Services;

namespace MaterialiGestioneWeb
{
    public partial class StoricoAssegnazioniPage : System.Web.UI.Page
    {
        private readonly InventarioRepository _repository = new InventarioRepository();
        private readonly PersonaleRepository _personaleRepository = new PersonaleRepository();

        protected void Page_Init(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                BindPersonale(GetPostedIsPersonaleEsternoSelected(), GetPostedIncludeNonAttivi());
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindProdottiByCategorico(null, null);
                BindPersonale();
            }
        }

        protected void CategoricoText_TextChanged(object sender, EventArgs e)
        {
            FilterProducts();
        }

        protected void FiltraProdottoButton_Click(object sender, EventArgs e)
        {
            FilterProducts();
        }

        protected void TipoPersonaleRadio_SelectedIndexChanged(object sender, EventArgs e)
        {
            ResultPanel.Visible = false;
            BindPersonale();
        }

        protected void MostraNonAttiviCheck_CheckedChanged(object sender, EventArgs e)
        {
            ResultPanel.Visible = false;
            BindPersonale();
        }

        protected void SearchButton_Click(object sender, EventArgs e)
        {
            try
            {
                ErrorPanel.Visible = false;
                var idProdotto = ParseOptionalInt(ProdottoDropDown.SelectedValue);
                var idPersonale = ParseOptionalInt(PersonaleDropDown.SelectedValue);
                StoricoGrid.DataSource = _repository.GetStoricoAssegnazioni(CategoricoText.Text, idProdotto, idPersonale);
                StoricoGrid.DataBind();
                ResultTitle.Text = Server.HtmlEncode("Risultati storico assegnazioni");
                ResultPanel.Visible = true;
            }
            catch (Exception ex)
            {
                AppLogger.Error("StoricoAssegnazioniPage.SearchButton_Click", "Errore durante la ricerca storico assegnazioni.", ex);
                ResultPanel.Visible = false;
                ErrorPanel.Visible = true;
                ErrorMessage.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void ResetButton_Click(object sender, EventArgs e)
        {
            ErrorPanel.Visible = false;
            ResultPanel.Visible = false;
            CategoricoText.Text = string.Empty;
            TipoPersonaleRadio.SelectedValue = "I";
            MostraNonAttiviCheck.Checked = false;
            BindProdottiByCategorico(null, null);
            BindPersonale();
        }

        protected void ExportCsvButton_Click(object sender, EventArgs e)
        {
            try
            {
                ErrorPanel.Visible = false;
                var idProdotto = ParseOptionalInt(ProdottoDropDown.SelectedValue);
                var idPersonale = ParseOptionalInt(PersonaleDropDown.SelectedValue);
                ExportCsv(_repository.GetStoricoAssegnazioni(CategoricoText.Text, idProdotto, idPersonale));
            }
            catch (Exception ex)
            {
                AppLogger.Error("StoricoAssegnazioniPage.ExportCsvButton_Click", "Errore durante export storico assegnazioni.", ex);
                ErrorPanel.Visible = true;
                ErrorMessage.Text = Server.HtmlEncode(ex.Message);
            }
        }

        private void FilterProducts()
        {
            ErrorPanel.Visible = false;
            ResultPanel.Visible = false;
            BindProdottiByCategorico(CategoricoText.Text, null);
        }

        private void BindProdottiByCategorico(string categoricoFilter, int? selectedId)
        {
            var items = string.IsNullOrWhiteSpace(categoricoFilter)
                ? new LookupItem[0]
                : _repository.GetProdottiLookupByCategorico(categoricoFilter);
            BindDropDown(ProdottoDropDown, items, "-- tutti i prodotti filtrati --", "DisplayName");
            SelectDropDownValue(ProdottoDropDown, selectedId);
        }

        private void BindPersonale()
        {
            BindPersonale(IsPersonaleEsternoSelected(), MostraNonAttiviCheck.Checked);
        }

        private void BindPersonale(bool isEsterno, bool includeNonAttivi)
        {
            PersonaleDropDown.Items.Clear();
            PersonaleDropDown.Items.Add(new ListItem("-- tutti gli assegnatari --", string.Empty));
            PersonaleDropDown.AppendDataBoundItems = true;
            PersonaleDropDown.DataSource = _personaleRepository.GetPersonale(isEsterno, includeNonAttivi);
            PersonaleDropDown.DataTextField = "DisplayName";
            PersonaleDropDown.DataValueField = "Id";
            PersonaleDropDown.DataBind();
            PersonaleDropDown.AppendDataBoundItems = false;
        }

        private bool IsPersonaleEsternoSelected()
        {
            return TipoPersonaleRadio.SelectedValue == "E";
        }

        private bool GetPostedIsPersonaleEsternoSelected()
        {
            return string.Equals(Request.Form[TipoPersonaleRadio.UniqueID], "E", StringComparison.OrdinalIgnoreCase);
        }

        private bool GetPostedIncludeNonAttivi()
        {
            return !string.IsNullOrEmpty(Request.Form[MostraNonAttiviCheck.UniqueID]);
        }

        private static void BindDropDown(ListControl control, object dataSource, string emptyText, string textField)
        {
            control.Items.Clear();
            control.Items.Add(new ListItem(emptyText, string.Empty));
            control.AppendDataBoundItems = true;
            control.DataSource = dataSource;
            control.DataTextField = textField;
            control.DataValueField = "Id";
            control.DataBind();
            control.AppendDataBoundItems = false;
        }

        private static void SelectDropDownValue(ListControl control, int? selectedId)
        {
            if (!selectedId.HasValue)
            {
                return;
            }

            var selectedValue = selectedId.Value.ToString(CultureInfo.InvariantCulture);
            if (control.Items.FindByValue(selectedValue) != null)
            {
                control.SelectedValue = selectedValue;
            }
        }

        private static int? ParseOptionalInt(string value)
        {
            int parsed;
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) ? (int?)parsed : null;
        }

        private void ExportCsv(IList<StoricoAssegnazioneConsultazioneItem> storico)
        {
            CsvExport.Write(this, "StoricoAssegnazioni", new[]
            {
                "Categorico",
                "Descrizione",
                "Matricola",
                "Assegnatario",
                "Data assegnazione",
                "Data restituzione",
                "Stanza",
                "Stato",
                "Nome macchina",
                "Seriale",
                "Note"
            }, BuildCsvRows(storico));
        }

        private static IEnumerable<IEnumerable<string>> BuildCsvRows(IEnumerable<StoricoAssegnazioneConsultazioneItem> storico)
        {
            foreach (var item in storico)
            {
                yield return new[]
                {
                    item.Categorico.HasValue ? item.Categorico.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                    item.DescrizioneProdotto,
                    item.Matricola,
                    item.AssegnatarioDisplay,
                    item.DataAssegnazione.HasValue ? item.DataAssegnazione.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty,
                    item.DataRestituzione.HasValue ? item.DataRestituzione.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty,
                    item.NumeroStanza,
                    item.LivelloEfficienza,
                    item.NomeMacchina,
                    item.SerialNumber,
                    item.NoteProdotto
                };
            }
        }
    }
}
