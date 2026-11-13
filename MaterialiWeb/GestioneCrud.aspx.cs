using System;
using System.Globalization;
using System.Web.UI.WebControls;
using MaterialiGestioneWeb.Infrastructure;
using MaterialiGestioneWeb.Models;
using MaterialiGestioneWeb.Services;

namespace MaterialiGestioneWeb
{
    public partial class GestioneCrudPage : System.Web.UI.Page
    {
        private readonly InventarioRepository _repository = new InventarioRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindAll();
            }
        }

        private void BindAll()
        {
            var selectedOggettoOrdinativoId = GetRequestedOggettoOrdinativoId();
            var isContextMode = selectedOggettoOrdinativoId.HasValue;
            ConfigureContextUi(selectedOggettoOrdinativoId);
            ErrorPanel.Visible = false;
            if (!isContextMode)
            {
                ProdottiAdminGrid.DataSource = _repository.GetProdottiAdmin();
                ProdottiAdminGrid.DataBind();
                OrdinativiGrid.DataSource = _repository.GetOrdinativiAdmin(applyRowLimit: true);
                OrdinativiGrid.DataBind();
            }
            OggettiGrid.DataSource = _repository.GetOggettiOrdinativoAdmin(selectedOggettoOrdinativoId);
            OggettiGrid.DataBind();
            BindLookupDropDown(OggettoOrdinativoDropDown, _repository.GetOrdinativiLookup(), selectedOggettoOrdinativoId);
            BindLookupDropDown(OggettoDittaDropDown, _repository.GetDitteLookup(), null);
            BindLookupDropDown(OggettoCategoriaDropDown, _repository.GetCategorieLookup(), null);
            if (!isContextMode)
            {
                OrdinativoCodiceText.Text = _repository.GetNextCodiceOrdinativo();
            }
        }

        protected void AddOrdinativoButton_Click(object sender, EventArgs e)
        {
            ExecuteAction(() =>
            {
                _repository.CreateOrdinativoAdmin(new OrdinativoAdminItem
                {
                    CodiceOrdinativo = _repository.GetNextCodiceOrdinativo(),
                    DenominazioneOrdinativo = RequireText(OrdinativoDenominazioneText.Text, "Denominazione ordinativo")
                });
                ClearText(OrdinativoDenominazioneText);
            }, "Ordinativo inserito.");
        }

        protected void AddOggettoButton_Click(object sender, EventArgs e)
        {
            ExecuteAction(() =>
            {
                _repository.CreateOggettoOrdinativoAdmin(new OggettoOrdinativoAdminItem
                {
                    IdOrdinativo = ParseRequiredInt(OggettoOrdinativoDropDown.SelectedValue, "Ordinativo"),
                    DescrizioneProdotto = RequireText(OggettoDescrizioneText.Text, "Descrizione oggetto ordinativo"),
                    IdDittaCostruttrice = ParseOptionalInt(OggettoDittaDropDown.SelectedValue),
                    Modello = OggettoModelloText.Text,
                    NUC = OggettoNucText.Text,
                    Quantita = ParseRequiredInt(OggettoQuantitaText.Text, "Quantita"),
                    IdCategProdotti = ParseOptionalInt(OggettoCategoriaDropDown.SelectedValue)
                });
                ClearText(OggettoDescrizioneText, OggettoModelloText, OggettoNucText, OggettoQuantitaText);
                var selectedOggettoOrdinativoId = GetRequestedOggettoOrdinativoId();
                if (selectedOggettoOrdinativoId.HasValue)
                {
                    SelectDropDownValue(OggettoOrdinativoDropDown, selectedOggettoOrdinativoId);
                }
                else
                {
                    ResetDropDown(OggettoOrdinativoDropDown);
                }
                ResetDropDown(OggettoDittaDropDown);
                ResetDropDown(OggettoCategoriaDropDown);
            }, "Oggetto ordinativo inserito e prodotti generati.");
        }

        protected void ProdottiAdminGrid_RowEditing(object sender, GridViewEditEventArgs e) { SetEdit(ProdottiAdminGrid, e.NewEditIndex); }
        protected void ProdottiAdminGrid_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e) { CancelEdit(ProdottiAdminGrid); }
        protected void ProdottiAdminGrid_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            ExecuteAction(() =>
            {
                var row = ProdottiAdminGrid.Rows[e.RowIndex];
                _repository.UpdateProdottoAdmin(new ProdottoAdminItem
                {
                    IdProdotto = GetGridKey(ProdottiAdminGrid, e.RowIndex),
                    Categorico = ParseOptionalInt(GetText(row, "EditProdottoCategoricoText")),
                    Matricola = GetText(row, "EditProdottoMatricolaText"),
                    IdStanza = GetDropDownSelectedValue(row, "EditProdottoStanzaDropDown"),
                    IdOggOrdinativo = GetDropDownSelectedValue(row, "EditProdottoOggettoDropDown"),
                    IdEfficienza = GetDropDownSelectedValue(row, "EditProdottoEfficienzaDropDown"),
                    Versamento = GetText(row, "EditProdottoVersamentoText"),
                    Note = GetText(row, "EditProdottoNoteText")
                });
                ProdottiAdminGrid.EditIndex = -1;
            }, "Prodotto aggiornato.");
        }
        protected void ProdottiAdminGrid_RowDeleting(object sender, GridViewDeleteEventArgs e) { ExecuteAction(() => _repository.DeleteProdottoAdmin(GetGridKey(ProdottiAdminGrid, e.RowIndex)), "Prodotto eliminato."); }

        protected void ProdottiAdminGrid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (!IsEditRow(e.Row))
            {
                return;
            }

            var item = (ProdottoAdminItem)e.Row.DataItem;
            BindLookupDropDown((DropDownList)e.Row.FindControl("EditProdottoStanzaDropDown"), _repository.GetStanzeLookup(), item.IdStanza);
            BindLookupDropDown((DropDownList)e.Row.FindControl("EditProdottoOrdinativoDropDown"), _repository.GetOrdinativiLookup(), item.IdOrdinativo);
            BindLookupDropDown((DropDownList)e.Row.FindControl("EditProdottoOggettoDropDown"), _repository.GetOggettiOrdinativoLookup(item.IdOrdinativo), item.IdOggOrdinativo);
            BindLookupDropDown((DropDownList)e.Row.FindControl("EditProdottoEfficienzaDropDown"), _repository.GetLivelliEfficienzaLookup(), item.IdEfficienza);
        }

        protected void EditProdottoOrdinativoDropDown_SelectedIndexChanged(object sender, EventArgs e)
        {
            ErrorPanel.Visible = false;
            SuccessPanel.Visible = false;

            var dropDown = (DropDownList)sender;
            var row = (GridViewRow)dropDown.NamingContainer;
            var idOrdinativo = ParseOptionalInt(dropDown.SelectedValue);
            BindLookupDropDown(
                (DropDownList)row.FindControl("EditProdottoOggettoDropDown"),
                idOrdinativo.HasValue ? _repository.GetOggettiOrdinativoLookup(idOrdinativo) : new LookupItem[0],
                null);
        }

        protected void OrdinativiGrid_RowEditing(object sender, GridViewEditEventArgs e) { SetEdit(OrdinativiGrid, e.NewEditIndex); }
        protected void OrdinativiGrid_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e) { CancelEdit(OrdinativiGrid); }
        protected void OrdinativiGrid_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            ExecuteAction(() =>
            {
                var row = OrdinativiGrid.Rows[e.RowIndex];
                _repository.UpdateOrdinativoAdmin(new OrdinativoAdminItem
                {
                    IdOrdinativo = GetGridKey(OrdinativiGrid, e.RowIndex),
                    CodiceOrdinativo = GetText(row, "EditOrdinativoCodiceText"),
                    DenominazioneOrdinativo = RequireText(GetText(row, "EditOrdinativoDenominazioneText"), "Denominazione ordinativo")
                });
                OrdinativiGrid.EditIndex = -1;
            }, "Ordinativo aggiornato.");
        }
        protected void OrdinativiGrid_RowDeleting(object sender, GridViewDeleteEventArgs e) { ExecuteAction(() => _repository.DeleteOrdinativoAdmin(GetGridKey(OrdinativiGrid, e.RowIndex)), "Ordinativo eliminato."); }

        protected void OggettiGrid_RowEditing(object sender, GridViewEditEventArgs e) { SetEdit(OggettiGrid, e.NewEditIndex); }
        protected void OggettiGrid_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e) { CancelEdit(OggettiGrid); }
        protected void OggettiGrid_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            ExecuteAction(() =>
            {
                var row = OggettiGrid.Rows[e.RowIndex];
                _repository.UpdateOggettoOrdinativoAdmin(new OggettoOrdinativoAdminItem
                {
                    IdOggOrdinativo = GetGridKey(OggettiGrid, e.RowIndex),
                    IdOrdinativo = GetDropDownSelectedValue(row, "EditOggettoOrdinativoDropDown"),
                    DescrizioneProdotto = RequireText(GetText(row, "EditOggettoDescrizioneText"), "Descrizione oggetto ordinativo"),
                    IdDittaCostruttrice = GetDropDownSelectedValue(row, "EditOggettoDittaDropDown"),
                    Modello = GetText(row, "EditOggettoModelloText"),
                    NUC = GetText(row, "EditOggettoNucText"),
                    IdCategProdotti = GetDropDownSelectedValue(row, "EditOggettoCategoriaDropDown")
                });
                OggettiGrid.EditIndex = -1;
            }, "Oggetto ordinativo aggiornato.");
        }
        protected void OggettiGrid_RowDeleting(object sender, GridViewDeleteEventArgs e) { ExecuteAction(() => _repository.DeleteOggettoOrdinativoAdmin(GetGridKey(OggettiGrid, e.RowIndex)), "Oggetto ordinativo eliminato."); }

        protected void OggettiGrid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (!IsEditRow(e.Row))
            {
                return;
            }

            e.Row.CssClass = string.IsNullOrWhiteSpace(e.Row.CssClass) ? "editing-row" : e.Row.CssClass + " editing-row";
            e.Row.Attributes["id"] = "editing-oggetto-row";
            ClientScript.RegisterStartupScript(
                GetType(),
                "focus-editing-oggetto-row",
                "var row=document.getElementById('editing-oggetto-row'); if(row){row.scrollIntoView({block:'center'});}",
                true);
            var item = (OggettoOrdinativoAdminItem)e.Row.DataItem;
            BindLookupDropDown((DropDownList)e.Row.FindControl("EditOggettoOrdinativoDropDown"), _repository.GetOrdinativiLookup(), item.IdOrdinativo);
            BindLookupDropDown((DropDownList)e.Row.FindControl("EditOggettoDittaDropDown"), _repository.GetDitteLookup(), item.IdDittaCostruttrice);
            BindLookupDropDown((DropDownList)e.Row.FindControl("EditOggettoCategoriaDropDown"), _repository.GetCategorieLookup(), item.IdCategProdotti);
        }

        private void ExecuteAction(Action action, string successMessage)
        {
            try
            {
                ErrorPanel.Visible = false;
                SuccessPanel.Visible = false;
                action();
                BindAll();
                SuccessPanel.Visible = true;
                SuccessMessage.Text = Server.HtmlEncode(successMessage);
            }
            catch (Exception ex)
            {
                AppLogger.Error("GestioneCrudPage.ExecuteAction", "Errore CRUD amministrativi.", ex);
                SuccessPanel.Visible = false;
                ErrorPanel.Visible = true;
                ErrorMessage.Text = Server.HtmlEncode(ex.Message);
            }
        }

        private void SetEdit(GridView grid, int index)
        {
            grid.EditIndex = index;
            BindAll();
        }

        private void CancelEdit(GridView grid)
        {
            grid.EditIndex = -1;
            BindAll();
        }

        private void ConfigureContextUi(int? idOrdinativo)
        {
            if (!idOrdinativo.HasValue)
            {
                ContextCssLiteral.Text = string.Empty;
                ContextActionsPanel.Visible = false;
                PageHeadingText.Text = "CRUD amministrativi";
                PageIntroText.Text = "Gestione di prodotti, ordinativi e oggetti ordinativo.";
                return;
            }

            ContextCssLiteral.Text = @"
<style>
    .domain-jump-nav,
    #crud-prodotti,
    #crud-ordinativi {
        display: none;
    }
</style>";
            ContextActionsPanel.Visible = true;
            BackToDettaglioLink.NavigateUrl = "DettaglioOrdinativo.aspx?id=" + idOrdinativo.Value.ToString(CultureInfo.InvariantCulture);
            PageHeadingText.Text = "Gestione righe oggetto";
            PageIntroText.Text = "Vista filtrata sull'ordinativo selezionato. La pagina mostra solo le righe oggetto collegate e il form per aggiungerne di nuove.";
        }

        private static int GetGridKey(GridView grid, int rowIndex)
        {
            return Convert.ToInt32(grid.DataKeys[rowIndex].Value, CultureInfo.InvariantCulture);
        }

        private static string GetText(GridViewRow row, string controlId)
        {
            var textBox = (TextBox)row.FindControl(controlId);
            return textBox == null ? string.Empty : textBox.Text;
        }

        private static int? GetDropDownSelectedValue(GridViewRow row, string controlId)
        {
            var dropDown = (DropDownList)row.FindControl(controlId);
            return dropDown == null ? null : ParseOptionalInt(dropDown.SelectedValue);
        }

        private static int GetDropDownRequiredValue(GridViewRow row, string controlId, string fieldName)
        {
            var dropDown = (DropDownList)row.FindControl(controlId);
            return ParseRequiredInt(dropDown == null ? string.Empty : dropDown.SelectedValue, fieldName);
        }

        private static bool IsEditRow(GridViewRow row)
        {
            return row.RowType == DataControlRowType.DataRow && (row.RowState & DataControlRowState.Edit) == DataControlRowState.Edit;
        }

        private static void BindLookupDropDown(DropDownList control, System.Collections.Generic.IEnumerable<LookupItem> items, int? selectedId)
        {
            if (control == null)
            {
                return;
            }

            control.Items.Clear();
            control.Items.Add(new ListItem("-- seleziona --", string.Empty));
            control.AppendDataBoundItems = true;
            control.DataSource = items;
            control.DataTextField = "DisplayName";
            control.DataValueField = "Id";
            control.DataBind();
            control.AppendDataBoundItems = false;
            if (selectedId.HasValue)
            {
                var selectedValue = selectedId.Value.ToString(CultureInfo.InvariantCulture);
                if (control.Items.FindByValue(selectedValue) != null)
                {
                    control.SelectedValue = selectedValue;
                }
            }
        }

        private static void SelectDropDownValue(DropDownList control, int? selectedId)
        {
            if (selectedId.HasValue)
            {
                var selectedValue = selectedId.Value.ToString(CultureInfo.InvariantCulture);
                if (control.Items.FindByValue(selectedValue) != null)
                {
                    control.SelectedValue = selectedValue;
                }
            }
        }

        private static int? ParseOptionalInt(string value)
        {
            int parsed;
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed) ? (int?)parsed : null;
        }

        private int? GetRequestedOggettoOrdinativoId()
        {
            return ParseOptionalInt(Request.QueryString["idOrdinativo"]);
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

        private static decimal ParseRequiredDecimal(string value, string fieldName)
        {
            decimal parsed;
            if (!decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out parsed))
            {
                throw new InvalidOperationException(fieldName + " non valido.");
            }

            return parsed;
        }

        private static DateTime? ParseOptionalDate(string value)
        {
            DateTime parsed;
            return DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out parsed) ? (DateTime?)parsed : null;
        }

        private static string RequireText(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(fieldName + " obbligatorio.");
            }

            return value.Trim();
        }

        private static void ClearText(params TextBox[] controls)
        {
            foreach (var control in controls)
            {
                control.Text = string.Empty;
            }
        }

        private static void ResetDropDown(DropDownList control)
        {
            if (control != null && control.Items.Count > 0)
            {
                control.SelectedIndex = 0;
            }
        }
    }
}
