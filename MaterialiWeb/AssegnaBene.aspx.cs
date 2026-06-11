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
    public partial class AssegnaBenePage : Auth.BaseAuthenticatedPage
    {
        private readonly InventarioRepository _repository = new InventarioRepository();
        private readonly PersonaleRepository _personaleRepository = new PersonaleRepository();

        protected override int[] LivelliConsentiti
        {
            get { return new[] { (int)Auth.LivelliUtente.Operatore, (int)Auth.LivelliUtente.Amministratore }; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                try
                {
                    BindLookups();
                    DataInizioText.Text = DateTime.Today.ToString("yyyy-MM-dd");
                    if (!string.IsNullOrWhiteSpace(Request.QueryString["id"]))
                    {
                        TrySelectProductFromQueryString(Request.QueryString["id"]);
                    }

                    LoadSelectedProductContext();
                }
                catch (Exception ex)
                {
                    AppLogger.Error("AssegnaBenePage.Page_Load", "Errore durante il caricamento della pagina di assegnazione.", ex);
                    ErrorPanel.Visible = true;
                    ErrorMessage.Text = Server.HtmlEncode(ex.Message);
                }
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                AppLogger.Info("AssegnaBenePage.SaveButton_Click", "Richiesta assegnazione materiale.");
                var idProdotto = ParseRequiredInt(ProdottoDropDown.SelectedValue, "Prodotto");
                RequireAvailableProduct(idProdotto);
                _repository.AssegnaProdotto(new AssegnazioneInput
                {
                    IdProdotto = idProdotto,
                    IdPersonale = ParseRequiredInt(PersonaleDropDown.SelectedValue, "Assegnatario"),
                    DataAssegnazione = ParseRequiredDate(DataInizioText.Text, "Data assegnazione"),
                    Note = NoteText.Text
                });

                SuccessPanel.Visible = true;
                ErrorPanel.Visible = false;
                SuccessMessage.Text = "Assegnazione registrata correttamente.";
            }
            catch (Exception ex)
            {
                AppLogger.Error("AssegnaBenePage.SaveButton_Click", "Errore durante l'assegnazione del materiale.", ex);
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

        protected void PrintAssignmentSheetButton_Click(object sender, EventArgs e)
        {
            try
            {
                ErrorPanel.Visible = false;
                if (!PrintAssignmentSheetButton.Enabled)
                {
                    throw new InvalidOperationException("Caricare prima un materiale da stampare.");
                }

                var idProdotto = ParseRequiredInt(ProdottoDropDown.SelectedValue, "Prodotto");
                var detail = RequireAvailableProduct(idProdotto);
                var assegnatario = GetSelectedPersonaleDisplay();
                if (string.IsNullOrWhiteSpace(assegnatario))
                {
                    throw new InvalidOperationException("Selezionare l'assegnatario prima della stampa.");
                }

                ExportMovementSheet(
                    "Scheda assegnazione bene",
                    "Assegnatario",
                    assegnatario,
                    detail.Prodotto,
                    "SchedaAssegnazioneSingola.pdf");
            }
            catch (Exception ex)
            {
                AppLogger.Error("AssegnaBenePage.PrintAssignmentSheetButton_Click", "Errore durante stampa scheda assegnazione.", ex);
                ErrorPanel.Visible = true;
                SuccessPanel.Visible = false;
                ErrorMessage.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void ProdottoDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            ErrorPanel.Visible = false;
            SuccessPanel.Visible = false;
            PersonaleDropDown.ClearSelection();
            LoadSelectedProductContext();
        }

        protected void CategoricoFiltroText_TextChanged(object sender, EventArgs e)
        {
            FilterByCategorico();
        }

        protected void FiltraProdottoButton_Click(object sender, EventArgs e)
        {
            FilterByCategorico();
        }

        protected void RicercaProdottoText_TextChanged(object sender, EventArgs e)
        {
            FilterByCategorico();
        }

        private void BindLookups()
        {
            BindProdottiByCategorico(CategoricoFiltroText.Text, null);
            BindPersonale();
        }

        private void FilterByCategorico()
        {
            ErrorPanel.Visible = false;
            SuccessPanel.Visible = false;
            BindProdottiByCategorico(CategoricoFiltroText.Text, null);
            LoadSelectedProductContext();
        }

        private void BindProdottiByCategorico(string categoricoFilter, int? selectedId)
        {
            var items = string.IsNullOrWhiteSpace(categoricoFilter)
                ? new LookupItem[0]
                : _repository.GetProdottiLookupByCategorico(categoricoFilter);
            items = FilterAvailableProducts(items);
            items = ApplyDescriptionFilter(items);
            BindDropDown(ProdottoDropDown, items, "-- inserisci un categorico --", "DisplayName");

            if (!selectedId.HasValue)
            {
                return;
            }

            var selectedValue = selectedId.Value.ToString(CultureInfo.InvariantCulture);
            var selectedItem = ProdottoDropDown.Items.FindByValue(selectedValue);
            if (selectedItem != null)
            {
                ProdottoDropDown.ClearSelection();
                selectedItem.Selected = true;
            }
        }

        private void LoadSelectedProductContext()
        {
            ProdottoContextPanel.Visible = false;
            PrintAssignmentSheetButton.Enabled = false;

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
            PrintAssignmentSheetButton.Enabled = true;
        }

        private void BindPersonale()
        {
            PersonaleDropDown.Items.Clear();
            PersonaleDropDown.Items.Add(new ListItem("-- seleziona assegnatario --", string.Empty));
            PersonaleDropDown.AppendDataBoundItems = true;
            PersonaleDropDown.DataSource = _personaleRepository.GetPersonale(IsPersonaleEsternoSelected(), MostraNonAttiviCheck.Checked);
            PersonaleDropDown.DataTextField = "DisplayName";
            PersonaleDropDown.DataValueField = "Id";
            PersonaleDropDown.DataBind();
            PersonaleDropDown.AppendDataBoundItems = false;
        }

        private void TrySelectProductFromQueryString(string idValue)
        {
            if (string.IsNullOrWhiteSpace(idValue))
            {
                return;
            }

            int idProdotto;
            if (!int.TryParse(idValue.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out idProdotto))
            {
                return;
            }

            var detail = _repository.GetProdottoDettaglio(idProdotto);
            if (detail != null && detail.Prodotto != null && detail.Prodotto.Categorico.HasValue)
            {
                CategoricoFiltroText.Text = detail.Prodotto.Categorico.Value.ToString(CultureInfo.InvariantCulture);
                BindProdottiByCategorico(CategoricoFiltroText.Text, idProdotto);
            }

            var item = ProdottoDropDown.Items.FindByValue(idValue.Trim());
            if (item != null)
            {
                ProdottoDropDown.ClearSelection();
                item.Selected = true;
                return;
            }

            ErrorPanel.Visible = true;
            ErrorMessage.Text = "Il prodotto richiesto non e' disponibile per una nuova assegnazione. Usa la pagina di rientro o riassegnazione se il bene e' gia' assegnato.";
        }

        private ProdottoDettaglio RequireAvailableProduct(int idProdotto)
        {
            var detail = _repository.GetProdottoDettaglio(idProdotto);
            if (detail == null || detail.Prodotto == null)
            {
                throw new InvalidOperationException("Prodotto non trovato.");
            }

            if (HasCurrentAssignee(detail.Prodotto.AssegnatarioDisplay))
            {
                throw new InvalidOperationException("Il prodotto selezionato risulta gia' assegnato. Usa la pagina di rientro o riassegnazione.");
            }

            return detail;
        }

        private bool IsPersonaleEsternoSelected()
        {
            return TipoPersonaleRadio.SelectedValue == "E";
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

        private LookupItem[] FilterAvailableProducts(IList<LookupItem> items)
        {
            return (items ?? new LookupItem[0])
                .Where(item =>
                {
                    if (item == null)
                    {
                        return false;
                    }

                    var detail = _repository.GetProdottoDettaglio(item.Id);
                    return detail != null
                        && detail.Prodotto != null
                        && !HasCurrentAssignee(detail.Prodotto.AssegnatarioDisplay);
                })
                .ToArray();
        }

        private LookupItem[] ApplyDescriptionFilter(IList<LookupItem> items)
        {
            var filter = string.IsNullOrWhiteSpace(RicercaProdottoText.Text) ? string.Empty : RicercaProdottoText.Text.Trim();
            if (string.IsNullOrWhiteSpace(filter))
            {
                return (items ?? new LookupItem[0]).ToArray();
            }

            return (items ?? new LookupItem[0])
                .Where(item =>
                {
                    var haystack = ((item == null ? string.Empty : item.Nome) + " " + (item == null ? string.Empty : item.Codice)).Trim();
                    return haystack.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
                })
                .ToArray();
        }

        private static bool HasCurrentAssignee(string assegnatarioDisplay)
        {
            return !string.IsNullOrWhiteSpace(assegnatarioDisplay) && assegnatarioDisplay.Trim() != "-";
        }

        private string GetSelectedPersonaleDisplay()
        {
            return PersonaleDropDown.SelectedItem == null || string.IsNullOrWhiteSpace(PersonaleDropDown.SelectedValue)
                ? string.Empty
                : PersonaleDropDown.SelectedItem.Text.Replace(" [non attivo]", string.Empty).Trim();
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
                PersonaValore = personValue,
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
