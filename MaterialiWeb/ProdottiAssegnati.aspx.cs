using System;
using System.Globalization;
using System.Web.UI.WebControls;
using MaterialiGestioneWeb.Infrastructure;
using MaterialiGestioneWeb.Services;

namespace MaterialiGestioneWeb
{
    public partial class ProdottiAssegnatiPage : System.Web.UI.Page
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
                BindPersonale();
            }
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
                var idPersonale = ParseRequiredInt(PersonaleDropDown.SelectedValue, "Personale");
                ProdottiGrid.DataSource = _repository.GetProdottiAssegnati(idPersonale);
                ProdottiGrid.DataBind();
                ResultTitle.Text = Server.HtmlEncode("Materiale assegnato a " + PersonaleDropDown.SelectedItem.Text);
                ResultPanel.Visible = true;
            }
            catch (Exception ex)
            {
                AppLogger.Error("ProdottiAssegnatiPage.SearchButton_Click", "Errore durante il caricamento prodotti assegnati.", ex);
                ResultPanel.Visible = false;
                ErrorPanel.Visible = true;
                ErrorMessage.Text = Server.HtmlEncode(ex.Message);
            }
        }

        protected void ResetButton_Click(object sender, EventArgs e)
        {
            ErrorPanel.Visible = false;
            ResultPanel.Visible = false;
            TipoPersonaleRadio.SelectedValue = "I";
            MostraNonAttiviCheck.Checked = false;
            BindPersonale();
        }

        private void BindPersonale()
        {
            BindPersonale(IsPersonaleEsternoSelected(), MostraNonAttiviCheck.Checked);
        }

        private void BindPersonale(bool isEsterno, bool includeNonAttivi)
        {
            PersonaleDropDown.Items.Clear();
            PersonaleDropDown.Items.Add(new ListItem("-- seleziona personale --", string.Empty));
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

        private static int ParseRequiredInt(string value, string fieldName)
        {
            int parsed;
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed))
            {
                throw new InvalidOperationException(fieldName + " non valido.");
            }

            return parsed;
        }
    }
}
