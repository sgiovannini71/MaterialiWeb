using System;
using System.Globalization;
using System.Web.UI.WebControls;
using MaterialiGestioneWeb.Infrastructure;
using MaterialiGestioneWeb.Models;
using MaterialiGestioneWeb.Services;

namespace MaterialiGestioneWeb
{
    public partial class AssegnaBenePage : System.Web.UI.Page
    {
        private readonly InventarioRepository _repository = new InventarioRepository();
        private readonly PersonaleRepository _personaleRepository = new PersonaleRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindLookups();
                DataInizioText.Text = DateTime.Today.ToString("yyyy-MM-dd");
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
                AppLogger.Info("AssegnaBenePage.SaveButton_Click", "Richiesta assegnazione materiale.");
                _repository.AssegnaProdotto(new AssegnazioneInput
                {
                    IdProdotto = ParseRequiredInt(ProdottoDropDown.SelectedValue, "Prodotto"),
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

        protected void ProdottoDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            ErrorPanel.Visible = false;
            SuccessPanel.Visible = false;
            PersonaleDropDown.ClearSelection();
            LoadSelectedProductContext();
        }

        private void BindLookups()
        {
            BindDropDown(ProdottoDropDown, _repository.GetProdottiLookup(), "-- seleziona materiale --", "Nome");
            BindPersonale();
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
            PersonaleDropDown.Items.Clear();
            PersonaleDropDown.Items.Add(new ListItem("-- seleziona assegnatario --", string.Empty));
            PersonaleDropDown.DataSource = _personaleRepository.GetPersonale(IsPersonaleEsternoSelected(), MostraNonAttiviCheck.Checked);
            PersonaleDropDown.DataTextField = "DisplayName";
            PersonaleDropDown.DataValueField = "Id";
            PersonaleDropDown.DataBind();
        }

        private bool IsPersonaleEsternoSelected()
        {
            return TipoPersonaleRadio.SelectedValue == "E";
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
