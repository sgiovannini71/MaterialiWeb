using System;
using System.Linq;
using System.Web.UI.WebControls;
using MaterialiGestioneWeb.Infrastructure;
using MaterialiGestioneWeb.Services;

namespace MaterialiGestioneWeb
{
    public partial class NuovoBenePage : Auth.BaseAuthenticatedPage
    {
        private const int DefaultFiltroEfficienza = 1;
        private readonly InventarioRepository _repository = new InventarioRepository();

        protected override int[] LivelliConsentiti
        {
            get { return new[] { (int)Auth.LivelliUtente.Operatore, (int)Auth.LivelliUtente.Amministratore }; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindLookups();
                LoadProductFromQueryString();
            }
        }

        protected void ProdottoDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSelectedProduct();
        }

        protected void FiltroEfficienzaDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            ErrorPanel.Visible = false;
            SuccessPanel.Visible = false;
            DetailPanel.Visible = false;
            BindProdottiDaCompletare();
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                ErrorPanel.Visible = false;
                SuccessPanel.Visible = false;

                var idProdotto = ParseRequiredInt(ProdottoDropDown.SelectedValue, "Prodotto");
                AppLogger.Info("NuovoBenePage.SaveButton_Click", "Richiesta completamento materiale " + idProdotto + ".");

                var note = MergeNotes(NoteText.Text, NoteStatoText.Text);
                _repository.CompletaProdottoGenerato(
                    idProdotto,
                    SerialeText.Text,
                    ParseOptionalInt(UbicazioneDropDown.SelectedValue),
                    ParseOptionalInt(StatoDropDown.SelectedValue),
                    NumeroFatturaText.Text,
                    note);

                var detail = _repository.GetProdottoDettaglio(idProdotto);
                SuccessPanel.Visible = true;
                SuccessMessage.Text = "Materiale completato con successo. <a href='ProdottoDettaglio.aspx?id=" + idProdotto + "'>Apri il dettaglio</a>.";
                BindCurrentSelections(detail);
            }
            catch (Exception ex)
            {
                AppLogger.Error("NuovoBenePage.SaveButton_Click", "Errore durante il completamento del materiale.", ex);
                ErrorPanel.Visible = true;
                ErrorMessage.Text = Server.HtmlEncode(ex.Message);
            }
        }

        private void BindLookups()
        {
            BindDropDown(StatoDropDown, _repository.GetLivelliEfficienzaLookup(), "-- seleziona stato --");
            BindDropDown(UbicazioneDropDown, _repository.GetStanzeLookup(), "-- seleziona stanza --");
            BindDropDown(FiltroEfficienzaDropDown, _repository.GetLivelliEfficienzaLookup(), "-- tutti gli stati --");
            SelectDropDownValue(FiltroEfficienzaDropDown, DefaultFiltroEfficienza);
            BindProdottiDaCompletare();
        }

        private void LoadProductFromQueryString()
        {
            int idProdotto;
            if (!int.TryParse(Request.QueryString["id"], out idProdotto))
            {
                DetailPanel.Visible = false;
                return;
            }

            SelectProduct(idProdotto);
            if (ProdottoDropDown.SelectedIndex <= 0)
            {
                FiltroEfficienzaDropDown.ClearSelection();
                BindProdottiDaCompletare();
                SelectProduct(idProdotto);
            }

            LoadSelectedProduct();
        }

        private void BindProdottiDaCompletare()
        {
            BindDropDown(
                ProdottoDropDown,
                _repository.GetProdottiDaCompletareLookup(ParseOptionalInt(FiltroEfficienzaDropDown.SelectedValue)),
                "-- seleziona prodotto generato --");
        }

        private void LoadSelectedProduct()
        {
            SuccessPanel.Visible = false;

            var idProdotto = ParseOptionalInt(ProdottoDropDown.SelectedValue);
            if (!idProdotto.HasValue)
            {
                DetailPanel.Visible = false;
                return;
            }

            var detail = _repository.GetProdottoDettaglio(idProdotto.Value);
            if (detail == null || detail.Prodotto == null)
            {
                throw new InvalidOperationException("Prodotto non trovato.");
            }

            DetailPanel.Visible = true;
            CategoricoText.Text = detail.Prodotto.Categorico.HasValue ? detail.Prodotto.Categorico.Value.ToString() : string.Empty;
            NomeText.Text = detail.Prodotto.DescrizioneProdotto;
            TipologiaText.Text = detail.Prodotto.Categoria;
            ModelloHardwareText.Text = detail.Prodotto.TipoOggetto;
            MarcaNuovoModelloText.Text = detail.Prodotto.Modello;
            ImpiegoText.Text = detail.Prodotto.DittaCostruttrice;
            OrdinativoText.Text = detail.Ordinativo == null ? string.Empty : string.Join(" - ", new[]
            {
                detail.Ordinativo.DenominazioneOrdinativo,
                detail.Ordinativo.CodiceOrdinativo
            }.Where(value => !string.IsNullOrWhiteSpace(value)));
            BindCurrentSelections(detail);
        }

        private void BindCurrentSelections(Models.ProdottoDettaglio detail)
        {
            SerialeText.Text = detail.Prodotto.Matricola;
            NumeroFatturaText.Text = detail.Prodotto.Versamento;
            NoteText.Text = detail.Prodotto.Note;
            NoteStatoText.Text = string.Empty;
            SelectByText(StatoDropDown, detail.Prodotto.LivelloEfficienza);
            SelectByText(UbicazioneDropDown, detail.Prodotto.NumeroStanza);
        }

        private static void SelectByText(ListControl control, string text)
        {
            control.ClearSelection();
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var item = control.Items.Cast<ListItem>().FirstOrDefault(entry => string.Equals(entry.Text, text, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                item.Selected = true;
            }
        }

        private static void SelectDropDownValue(ListControl control, int value)
        {
            var item = control.Items.FindByValue(value.ToString());
            if (item != null)
            {
                control.ClearSelection();
                item.Selected = true;
            }
        }

        private void SelectProduct(int idProdotto)
        {
            var item = ProdottoDropDown.Items.FindByValue(idProdotto.ToString());
            if (item != null)
            {
                ProdottoDropDown.ClearSelection();
                item.Selected = true;
            }
        }

        private static void BindDropDown(ListControl control, object dataSource, string emptyText)
        {
            control.Items.Clear();
            control.DataSource = dataSource;
            control.DataTextField = "Nome";
            control.DataValueField = "Id";
            control.DataBind();
            control.Items.Insert(0, new ListItem(emptyText, string.Empty));
        }

        private static int ParseRequiredInt(string value, string fieldName)
        {
            int parsed;
            if (int.TryParse(value, out parsed))
            {
                return parsed;
            }

            throw new InvalidOperationException(fieldName + " obbligatorio.");
        }

        private static string MergeNotes(string noteGenerali, string noteOperative)
        {
            if (string.IsNullOrWhiteSpace(noteGenerali))
            {
                return noteOperative;
            }

            if (string.IsNullOrWhiteSpace(noteOperative))
            {
                return noteGenerali;
            }

            return noteGenerali.Trim() + " | " + noteOperative.Trim();
        }

        private static int? ParseOptionalInt(string value)
        {
            int parsed;
            return int.TryParse(value, out parsed) ? (int?)parsed : null;
        }
    }
}
