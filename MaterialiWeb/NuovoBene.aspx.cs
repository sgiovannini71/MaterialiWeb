using System;
using System.Globalization;
using System.Web.UI.WebControls;
using MaterialiGestioneWeb.Infrastructure;
using MaterialiGestioneWeb.Models;
using MaterialiGestioneWeb.Services;

namespace MaterialiGestioneWeb
{
    public partial class NuovoBenePage : System.Web.UI.Page
    {
        private readonly InventarioRepository _repository = new InventarioRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindLookups();
                CategoricoText.Text = _repository.GetNextCategorico().ToString(CultureInfo.InvariantCulture);
                CategoricoText.Attributes["readonly"] = "readonly";
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            try
            {
                ErrorPanel.Visible = false;
                SuccessPanel.Visible = false;

                AppLogger.Info("NuovoBenePage.SaveButton_Click", "Richiesta registrazione nuovo materiale.");
                var input = new NuovoProdottoInput
                {
                    Categorico = null,
                    Matricola = SerialeText.Text,
                    IdStanza = ParseOptionalInt(UbicazioneDropDown.SelectedValue),
                    IdOggettoOrdinativo = ParseOptionalInt(ModelloHardwareDropDown.SelectedValue),
                    IdEfficienza = ParseOptionalInt(StatoDropDown.SelectedValue),
                    Note = NoteText.Text,
                    Versamento = NumeroFatturaText.Text,
                    DescrizioneProdotto = NomeText.Text,
                    Modello = MarcaNuovoModelloText.Text,
                    IdCategoria = ParseOptionalInt(TipologiaDropDown.SelectedValue),
                    IdDittaCostruttrice = ParseOptionalInt(ImpiegoDropDown.SelectedValue),
                    PrezzoInventario = ParseOptionalDecimal(ValoreAcquistoText.Text),
                    PrezzoUnitarioNetto = ParseOptionalDecimal(ValoreAcquistoText.Text)
                };

                var newId = _repository.CreateProdotto(input);
                SuccessPanel.Visible = true;
                SuccessMessage.Text = "Materiale registrato con successo. <a href='ProdottoDettaglio.aspx?id=" + newId.ToString(CultureInfo.InvariantCulture) + "'>Apri il dettaglio</a>.";
                CategoricoText.Text = _repository.GetNextCategorico().ToString(CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                AppLogger.Error("NuovoBenePage.SaveButton_Click", "Errore durante la registrazione del nuovo materiale.", ex);
                ErrorPanel.Visible = true;
                ErrorMessage.Text = Server.HtmlEncode(ex.Message);
            }
        }

        private void BindLookups()
        {
            BindDropDown(TipologiaDropDown, _repository.GetCategorieLookup(), "-- nessuna --");
            BindDropDown(StatoDropDown, _repository.GetLivelliEfficienzaLookup(), "-- nessuno --");
            BindDropDown(UbicazioneDropDown, _repository.GetStanzeLookup(), "-- nessuna --");
            BindDropDown(ModelloHardwareDropDown, _repository.GetOggettiOrdinativoLookup(), "-- nessuno --");
            BindDropDown(ImpiegoDropDown, _repository.GetDitteLookup(), "-- nessuna --");
        }

        private static void BindDropDown(ListControl control, object dataSource, string emptyText)
        {
            control.Items.Clear();
            control.Items.Add(new ListItem(emptyText, string.Empty));
            control.DataSource = dataSource;
            control.DataTextField = "DisplayName";
            control.DataValueField = "Id";
            control.DataBind();
        }

        private static int? ParseOptionalInt(string value)
        {
            int parsed;
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) ? (int?)parsed : null;
        }

        private static decimal? ParseOptionalDecimal(string value)
        {
            decimal parsed;
            return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out parsed) ? (decimal?)parsed : null;
        }
    }
}
