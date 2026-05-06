using System;
using System.Globalization;
using System.Web.UI.WebControls;
using MaterialiGestioneWeb.Infrastructure;
using MaterialiGestioneWeb.Models;
using MaterialiGestioneWeb.Services;

namespace MaterialiGestioneWeb
{
    public partial class RientroRiassegnazionePage : System.Web.UI.Page
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
                BindLookups();
                DataOperazioneText.Text = DateTime.Today.ToString("yyyy-MM-dd");
                if (!string.IsNullOrWhiteSpace(Request.QueryString["id"]))
                {
                    LoadProductFromQueryString(Request.QueryString["id"]);
                }

                LoadSelectedProductContext();
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                AppLogger.Info("RientroRiassegnazionePage.SaveButton_Click", "Richiesta rientro/riassegnazione materiale.");
                _repository.RegistraRientroORiassegnazione(new RientroRiassegnazioneInput
                {
                    IdProdotto = ParseRequiredInt(ProdottoDropDown.SelectedValue, "Prodotto"),
                    DataOperazione = ParseRequiredDate(DataOperazioneText.Text, "Data operazione"),
                    CreaNuovaAssegnazione = NuovaAssegnazioneCheck.Checked,
                    NuovoIdPersonale = ParseOptionalInt(PersonaleDropDown.SelectedValue),
                    Note = NoteText.Text
                });
                SuccessPanel.Visible = true;
                ErrorPanel.Visible = false;
                SuccessMessage.Text = "Operazione registrata correttamente.";
            }
            catch (Exception ex)
            {
                AppLogger.Error("RientroRiassegnazionePage.SaveButton_Click", "Errore durante il rientro o la riassegnazione.", ex);
                ErrorPanel.Visible = true;
                SuccessPanel.Visible = false;
                ErrorMessage.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void TipoPersonaleRadio_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindPersonale();
        }

        protected void MostraNonAttiviCheck_CheckedChanged(object sender, EventArgs e)
        {
            BindPersonale();
        }

        protected void ProdottoDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            ErrorPanel.Visible = false;
            SuccessPanel.Visible = false;
            PersonaleDropDown.ClearSelection();
            LoadSelectedProductContext();
        }

        protected void CategoricoText_TextChanged(object sender, EventArgs e)
        {
            FilterByCategorico();
        }

        protected void FiltraProdottoButton_Click(object sender, EventArgs e)
        {
            FilterByCategorico();
        }

        private void BindLookups()
        {
            BindProdottiByCategorico(CategoricoText.Text, null);
            BindPersonale();
        }

        private void FilterByCategorico()
        {
            ErrorPanel.Visible = false;
            SuccessPanel.Visible = false;
            BindProdottiByCategorico(CategoricoText.Text, null);
            LoadSelectedProductContext();
        }

        private void BindProdottiByCategorico(string categoricoFilter, int? selectedId)
        {
            var items = string.IsNullOrWhiteSpace(categoricoFilter)
                ? new LookupItem[0]
                : _repository.GetProdottiLookupByCategorico(categoricoFilter);
            BindDropDown(ProdottoDropDown, items, "-- inserisci un categorico --", "DisplayName");
            if (selectedId.HasValue)
            {
                var selectedValue = selectedId.Value.ToString(CultureInfo.InvariantCulture);
                if (ProdottoDropDown.Items.FindByValue(selectedValue) != null)
                {
                    ProdottoDropDown.SelectedValue = selectedValue;
                }
            }
        }

        private void LoadProductFromQueryString(string idValue)
        {
            var idProdotto = ParseOptionalInt(idValue);
            if (!idProdotto.HasValue)
            {
                return;
            }

            var detail = _repository.GetProdottoDettaglio(idProdotto.Value);
            if (detail == null || detail.Prodotto == null)
            {
                return;
            }

            CategoricoText.Text = detail.Prodotto.Categorico.HasValue
                ? detail.Prodotto.Categorico.Value.ToString(CultureInfo.InvariantCulture)
                : string.Empty;
            BindProdottiByCategorico(CategoricoText.Text, idProdotto);
        }

        private void LoadSelectedProductContext()
        {
            ProdottoContextPanel.Visible = false;

            var idProdotto = ParseOptionalInt(ProdottoDropDown.SelectedValue);
            if (!idProdotto.HasValue)
            {
                return;
            }

            var detail = _repository.GetProdottoDettaglio(idProdotto.Value);
            if (detail == null || detail.Prodotto == null)
            {
                return;
            }

            var prodotto = detail.Prodotto;
            ProdottoContextPanel.Visible = true;
            ProdottoDescrizione.Text = Server.HtmlEncode(Fallback(prodotto.DescrizioneSintetica));
            ProdottoStato.Text = Server.HtmlEncode(Fallback(prodotto.LivelloEfficienza));
            ProdottoAssegnatario.Text = Server.HtmlEncode(Fallback(prodotto.AssegnatarioDisplay));
            ProdottoStanza.Text = Server.HtmlEncode(Fallback(prodotto.NumeroStanza));
            ProdottoNomeMacchina.Text = Server.HtmlEncode(Fallback(prodotto.NomeMacchina));
            ProdottoMacAddress.Text = Server.HtmlEncode(Fallback(prodotto.MacAddress));
        }

        private void BindPersonale()
        {
            BindPersonale(IsPersonaleEsternoSelected(), MostraNonAttiviCheck.Checked);
        }

        private void BindPersonale(bool isEsterno, bool includeNonAttivi)
        {
            PersonaleDropDown.Items.Clear();
            PersonaleDropDown.Items.Add(new ListItem("-- seleziona assegnatario --", string.Empty));
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

        private static int ParseRequiredInt(string value, string fieldName)
        {
            int parsed;
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
            {
                throw new InvalidOperationException(fieldName + " non valido.");
            }

            return parsed;
        }

        private static int? ParseOptionalInt(string value)
        {
            int parsed;
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) ? (int?)parsed : null;
        }

        private static DateTime ParseRequiredDate(string value, string fieldName)
        {
            DateTime parsed;
            if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed))
            {
                throw new InvalidOperationException(fieldName + " non valida.");
            }

            return parsed;
        }

        private static string Fallback(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value;
        }
    }
}
