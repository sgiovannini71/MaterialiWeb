using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using MaterialiGestioneWeb.Infrastructure;
using MaterialiGestioneWeb.Models;
using MaterialiGestioneWeb.Services;

namespace MaterialiGestioneWeb
{
    public partial class RientroRiassegnazionePage : Auth.BaseAuthenticatedPage
    {
        private readonly InventarioRepository _repository = new InventarioRepository();
        private readonly PersonaleRepository _personaleRepository = new PersonaleRepository();

        protected override int[] LivelliConsentiti
        {
            get { return new[] { (int)Auth.LivelliUtente.Operatore, (int)Auth.LivelliUtente.Amministratore }; }
        }

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
                var idProdotto = ParseRequiredInt(ProdottoDropDown.SelectedValue, "Prodotto");
                var detail = RequireAssignedProduct(idProdotto);
                var isRiassegnazione = IsRiassegnazioneMode();
                var nuovoIdPersonale = ParseOptionalInt(PersonaleDropDown.SelectedValue);

                if (isRiassegnazione && !nuovoIdPersonale.HasValue)
                {
                    throw new InvalidOperationException("Selezionare il nuovo assegnatario per la riassegnazione.");
                }

                if (!isRiassegnazione)
                {
                    nuovoIdPersonale = null;
                }

                if (isRiassegnazione && IsSameAsCurrentAssignee(detail.Prodotto.AssegnatarioDisplay, PersonaleDropDown.SelectedItem))
                {
                    throw new InvalidOperationException("Il nuovo assegnatario coincide con quello corrente.");
                }

                _repository.RegistraRientroORiassegnazione(new RientroRiassegnazioneInput
                {
                    IdProdotto = idProdotto,
                    DataOperazione = ParseRequiredDate(DataOperazioneText.Text, "Data operazione"),
                    CreaNuovaAssegnazione = isRiassegnazione,
                    NuovoIdPersonale = nuovoIdPersonale,
                    Note = NoteText.Text
                });
                SuccessPanel.Visible = true;
                ErrorPanel.Visible = false;
                SuccessMessage.Text = isRiassegnazione
                    ? "Riassegnazione registrata correttamente."
                    : "Rientro registrato correttamente.";
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
            UpdateOperationModeUi();
        }

        protected void MostraNonAttiviCheck_CheckedChanged(object sender, EventArgs e)
        {
            BindPersonale();
            UpdateOperationModeUi();
        }

        protected void TipoOperazioneRadio_SelectedIndexChanged(object sender, EventArgs e)
        {
            ErrorPanel.Visible = false;
            SuccessPanel.Visible = false;
            PersonaleDropDown.ClearSelection();
            UpdateOperationModeUi();
        }

        protected void ProdottoDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            ErrorPanel.Visible = false;
            SuccessPanel.Visible = false;
            PersonaleDropDown.ClearSelection();
            LoadSelectedProductContext();
            UpdateOperationModeUi();
        }

        protected void CategoricoText_TextChanged(object sender, EventArgs e)
        {
            FilterByCategorico();
        }

        protected void PrintReturnSheetButton_Click(object sender, EventArgs e)
        {
            try
            {
                ErrorPanel.Visible = false;
                if (!PrintReturnSheetButton.Enabled)
                {
                    throw new InvalidOperationException("Caricare prima un materiale da stampare.");
                }

                var idProdotto = ParseRequiredInt(ProdottoDropDown.SelectedValue, "Prodotto");
                var detail = RequireAssignedProduct(idProdotto);
                ExportMovementSheet(
                    "Scheda restituzione bene",
                    "Assegnatario",
                    detail.Prodotto.AssegnatarioDisplay,
                    detail.Prodotto,
                    "SchedaRestituzioneSingola.pdf");
            }
            catch (Exception ex)
            {
                AppLogger.Error("RientroRiassegnazionePage.PrintReturnSheetButton_Click", "Errore durante stampa scheda restituzione.", ex);
                ErrorPanel.Visible = true;
                SuccessPanel.Visible = false;
                ErrorMessage.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void FiltraProdottoButton_Click(object sender, EventArgs e)
        {
            FilterByCategorico();
        }

        private void BindLookups()
        {
            BindProdottiByCategorico(CategoricoText.Text, null);
            BindPersonale();
            UpdateOperationModeUi();
        }

        private void FilterByCategorico()
        {
            ErrorPanel.Visible = false;
            SuccessPanel.Visible = false;
            BindProdottiByCategorico(CategoricoText.Text, null);
            LoadSelectedProductContext();
            UpdateOperationModeUi();
        }

        private void BindProdottiByCategorico(string categoricoFilter, int? selectedId)
        {
            var items = string.IsNullOrWhiteSpace(categoricoFilter)
                ? new LookupItem[0]
                : FilterAssignedProducts(_repository.GetProdottiLookupByCategorico(categoricoFilter));
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
            PrintReturnSheetButton.Enabled = false;

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
            PrintReturnSheetButton.Enabled = true;
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

        private IList<LookupItem> FilterAssignedProducts(IEnumerable<LookupItem> items)
        {
            var results = new List<LookupItem>();
            foreach (var item in items ?? Enumerable.Empty<LookupItem>())
            {
                var detail = _repository.GetProdottoDettaglio(item.Id);
                if (detail != null && detail.Prodotto != null && HasCurrentAssignee(detail.Prodotto.AssegnatarioDisplay))
                {
                    results.Add(item);
                }
            }

            return results;
        }

        private ProdottoDettaglio RequireAssignedProduct(int idProdotto)
        {
            var detail = _repository.GetProdottoDettaglio(idProdotto);
            if (detail == null || detail.Prodotto == null)
            {
                throw new InvalidOperationException("Prodotto non trovato.");
            }

            if (!HasCurrentAssignee(detail.Prodotto.AssegnatarioDisplay))
            {
                throw new InvalidOperationException("Il prodotto selezionato non ha un'assegnazione attiva.");
            }

            return detail;
        }

        private void UpdateOperationModeUi()
        {
            var enableNewAssignee = IsRiassegnazioneMode();
            PersonaleDropDown.Enabled = enableNewAssignee;
            TipoPersonaleRadio.Enabled = enableNewAssignee;
            MostraNonAttiviCheck.Enabled = enableNewAssignee;
            if (!enableNewAssignee)
            {
                PersonaleDropDown.ClearSelection();
            }
        }

        private bool IsRiassegnazioneMode()
        {
            return string.Equals(TipoOperazioneRadio.SelectedValue, "T", StringComparison.OrdinalIgnoreCase);
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

        private static bool HasCurrentAssignee(string assegnatarioDisplay)
        {
            return !string.IsNullOrWhiteSpace(assegnatarioDisplay) && assegnatarioDisplay.Trim() != "-";
        }

        private static bool IsSameAsCurrentAssignee(string currentDisplay, ListItem selectedItem)
        {
            if (selectedItem == null || string.IsNullOrWhiteSpace(selectedItem.Value))
            {
                return false;
            }

            return string.Equals(
                NormalizeAssigneeDisplay(currentDisplay),
                NormalizeAssigneeDisplay(selectedItem.Text),
                StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeAssigneeDisplay(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return value.Replace(" [non attivo]", string.Empty).Trim();
        }

        private void ExportMovementSheet(string title, string personLabel, string personValue, ProdottoCorrente prodotto, string fileName)
        {
            var templatePath = MovementSheetSupport.GetMovementTemplatePath(this);
            if (!System.IO.File.Exists(templatePath))
            {
                throw new InvalidOperationException("Template scheda movimentazione non trovato.");
            }

            var model = new MovementSheetModel
            {
                Ente = MovementSheetSupport.GetConfiguredHeaderText(),
                Titolo = title,
                PersonaLabel = personLabel,
                PersonaValore = NormalizeAssigneeDisplay(personValue),
                Data = DateTime.Today,
                Prodotti = new[] { prodotto },
                Logo = MovementSheetSupport.LoadLogo(this)
            };

            var bytes = MovementSheetPdfExporter.Create(model);
            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment; filename=\"" + fileName + "\"");
            Response.BinaryWrite(bytes);
            Response.Flush();
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
