using System;
using System.Globalization;
using System.Web.UI.WebControls;
using MaterialiGestioneWeb.Infrastructure;
using MaterialiGestioneWeb.Models;
using MaterialiGestioneWeb.Services;

namespace MaterialiGestioneWeb
{
    public partial class ConfiguraComputerPage : Auth.BaseAuthenticatedPage
    {
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
                if (!string.IsNullOrWhiteSpace(Request.QueryString["id"]))
                {
                    ProdottoDropDown.SelectedValue = Request.QueryString["id"];
                }

                LoadSelectedProductContext();
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                AppLogger.Info("ConfiguraComputerPage.SaveButton_Click", "Richiesta aggiornamento rete/postazione.");
                _repository.AggiornaComputer(new AggiornamentoComputerInput
                {
                    IdProdotto = ParseRequiredInt(ProdottoDropDown.SelectedValue, "Materiale"),
                    NumeroSerie = SerialNumberText.Text,
                    NomeMacchina = HostNameText.Text,
                    MacAddress = MacAddressText.Text,
                    NoteRete = TipoRamText.Text
                });

                SuccessPanel.Visible = true;
                ErrorPanel.Visible = false;
                SuccessMessage.Text = "Configurazione e numero di serie aggiornati.";
            }
            catch (Exception ex)
            {
                AppLogger.Error("ConfiguraComputerPage.SaveButton_Click", "Errore durante l'aggiornamento rete/postazione.", ex);
                ErrorPanel.Visible = true;
                SuccessPanel.Visible = false;
                ErrorMessage.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void ProdottoDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            ErrorPanel.Visible = false;
            SuccessPanel.Visible = false;
            LoadSelectedProductContext();
        }

        private void BindLookups()
        {
            BindDropDown(ProdottoDropDown, _repository.GetProdottiLookup(), "-- seleziona materiale --", "Nome");
        }

        private void LoadSelectedProductContext()
        {
            ProdottoContextPanel.Visible = false;

            var idProdotto = ParseOptionalInt(ProdottoDropDown.SelectedValue);
            if (!idProdotto.HasValue)
            {
                SerialNumberText.Text = string.Empty;
                HostNameText.Text = string.Empty;
                MacAddressText.Text = string.Empty;
                TipoRamText.Text = string.Empty;
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
            ProdottoSeriale.Text = Server.HtmlEncode(Fallback(prodotto.Matricola));

            SerialNumberText.Text = prodotto.Matricola;
            HostNameText.Text = prodotto.NomeMacchina;
            MacAddressText.Text = prodotto.MacAddress;
            TipoRamText.Text = prodotto.Note;
        }

        private static void BindDropDown(ListControl control, object dataSource, string emptyText, string textField)
        {
            control.Items.Clear();
            control.Items.Add(new ListItem(emptyText, string.Empty));
            control.DataSource = dataSource;
            control.DataTextField = textField;
            control.DataValueField = "Id";
            control.DataBind();
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

        private static string Fallback(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value;
        }
    }
}
