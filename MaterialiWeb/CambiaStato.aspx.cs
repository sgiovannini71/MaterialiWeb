using System;
using System.Globalization;
using System.Web.UI.WebControls;
using MaterialiGestioneWeb.Infrastructure;
using MaterialiGestioneWeb.Models;
using MaterialiGestioneWeb.Services;

namespace MaterialiGestioneWeb
{
    public partial class CambiaStatoPage : Auth.BaseAuthenticatedPage
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
                AppLogger.Info("CambiaStatoPage.SaveButton_Click", "Richiesta cambio stato materiale.");
                _repository.CambiaStato(new CambioStatoInput
                {
                    IdProdotto = ParseRequiredInt(ProdottoDropDown.SelectedValue, "Prodotto"),
                    IdEfficienza = ParseRequiredInt(StatoDropDown.SelectedValue, "Stato"),
                    Note = NoteText.Text
                });
                SuccessPanel.Visible = true;
                ErrorPanel.Visible = false;
                SuccessMessage.Text = "Stato aggiornato.";
            }
            catch (Exception ex)
            {
                AppLogger.Error("CambiaStatoPage.SaveButton_Click", "Errore durante il cambio stato.", ex);
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
            BindDropDown(StatoDropDown, _repository.GetLivelliEfficienzaLookup(), "-- seleziona stato --", "DisplayName");
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

            SelectByText(StatoDropDown, prodotto.LivelloEfficienza);
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

        private static void SelectByText(ListControl control, string text)
        {
            control.ClearSelection();
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var item = control.Items.FindByText(text);
            if (item != null)
            {
                item.Selected = true;
            }
        }

        private static string Fallback(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "-" : value;
        }
    }
}
