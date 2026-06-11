using System;
using System.Globalization;
using System.Web.UI.WebControls;
using MaterialiGestioneWeb.Infrastructure;
using MaterialiGestioneWeb.Services;

namespace MaterialiGestioneWeb
{
    public partial class Domini : Auth.BaseAuthenticatedPage
    {
        private readonly InventarioRepository _repository = new InventarioRepository();

        protected override int[] LivelliConsentiti
        {
            get { return new[] { (int)Auth.LivelliUtente.Amministratore }; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadDomains();
            }
        }

        private void LoadDomains()
        {
            try
            {
                ErrorPanel.Visible = false;
                var domini = _repository.GetDomini();
                TipologieGrid.DataSource = domini.Categorie;
                TipologieGrid.DataBind();
                LivelliGrid.DataSource = domini.LivelliEfficienza;
                LivelliGrid.DataBind();
                StanzeGrid.DataSource = domini.Stanze;
                StanzeGrid.DataBind();
                DitteGrid.DataSource = domini.Ditte;
                DitteGrid.DataBind();
                TipiOggettoGrid.DataSource = domini.TipiOggettoOrdinativo;
                TipiOggettoGrid.DataBind();
                BindTipoOggettoDropDown(CategoriaTipoDropDown, null);
            }
            catch (Exception ex)
            {
                ShowError(ex);
            }
        }

        protected void AddCategoriaButton_Click(object sender, EventArgs e)
        {
            ExecuteAction(() =>
            {
                _repository.CreateCategoria(RequireText(CategoriaNomeText.Text, "Descrizione categoria"), ParseRequiredInt(CategoriaTipoDropDown.SelectedValue, "Tipo oggetto"));
                CategoriaNomeText.Text = string.Empty;
                CategoriaTipoDropDown.SelectedIndex = 0;
            }, "Categoria inserita.");
        }

        protected void AddLivelloButton_Click(object sender, EventArgs e)
        {
            ExecuteAction(() =>
            {
                _repository.CreateLivelloEfficienza(LivelloCodiceText.Text, RequireText(LivelloNomeText.Text, "Descrizione livello"));
                LivelloCodiceText.Text = string.Empty;
                LivelloNomeText.Text = string.Empty;
            }, "Livello inserito.");
        }

        protected void AddStanzaButton_Click(object sender, EventArgs e)
        {
            ExecuteAction(() =>
            {
                _repository.CreateStanza(RequireText(StanzaNumeroText.Text, "Numero stanza"));
                StanzaNumeroText.Text = string.Empty;
            }, "Stanza inserita.");
        }

        protected void AddDittaButton_Click(object sender, EventArgs e)
        {
            ExecuteAction(() =>
            {
                _repository.CreateDitta(RequireText(DittaNomeText.Text, "Nome ditta"), DittaCittaText.Text, DittaMailText.Text, DittaTipologiaText.Text);
                DittaNomeText.Text = string.Empty;
                DittaCittaText.Text = string.Empty;
                DittaMailText.Text = string.Empty;
                DittaTipologiaText.Text = string.Empty;
            }, "Ditta inserita.");
        }

        protected void AddTipoOggettoButton_Click(object sender, EventArgs e)
        {
            ExecuteAction(() =>
            {
                _repository.CreateTipoOggetto(ParseRequiredInt(TipoOggettoIdText.Text, "Id tipo oggetto"), RequireText(TipoOggettoNomeText.Text, "Descrizione tipo oggetto"));
                TipoOggettoIdText.Text = string.Empty;
                TipoOggettoNomeText.Text = string.Empty;
            }, "Tipo oggetto inserito.");
        }

        protected void TipologieGrid_RowEditing(object sender, GridViewEditEventArgs e)
        {
            TipologieGrid.EditIndex = e.NewEditIndex;
            LoadDomains();
        }

        protected void TipologieGrid_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            TipologieGrid.EditIndex = -1;
            LoadDomains();
        }

        protected void TipologieGrid_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            ExecuteAction(() =>
            {
                var row = TipologieGrid.Rows[e.RowIndex];
                var nome = RequireText(((TextBox)row.FindControl("EditCategoriaNomeText")).Text, "Descrizione categoria");
                var tipo = ParseRequiredInt(((DropDownList)row.FindControl("EditCategoriaTipoDropDown")).SelectedValue, "Tipo oggetto");
                _repository.UpdateCategoria(GetGridKey(TipologieGrid, e.RowIndex), nome, tipo);
                TipologieGrid.EditIndex = -1;
            }, "Categoria aggiornata.");
        }

        protected void TipologieGrid_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            ExecuteAction(() => _repository.DeleteCategoria(GetGridKey(TipologieGrid, e.RowIndex)), "Categoria rimossa.");
        }

        protected void TipologieGrid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow || (e.Row.RowState & DataControlRowState.Edit) != DataControlRowState.Edit)
            {
                return;
            }

            var item = (Models.DominioItem)e.Row.DataItem;
            var dropDown = (DropDownList)e.Row.FindControl("EditCategoriaTipoDropDown");
            BindTipoOggettoDropDown(dropDown, item.ParentId);
        }

        protected void LivelliGrid_RowEditing(object sender, GridViewEditEventArgs e)
        {
            LivelliGrid.EditIndex = e.NewEditIndex;
            LoadDomains();
        }

        protected void LivelliGrid_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            LivelliGrid.EditIndex = -1;
            LoadDomains();
        }

        protected void LivelliGrid_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            ExecuteAction(() =>
            {
                var row = LivelliGrid.Rows[e.RowIndex];
                _repository.UpdateLivelloEfficienza(
                    GetGridKey(LivelliGrid, e.RowIndex),
                    ((TextBox)row.FindControl("EditLivelloCodiceText")).Text,
                    RequireText(((TextBox)row.FindControl("EditLivelloNomeText")).Text, "Descrizione livello"));
                LivelliGrid.EditIndex = -1;
            }, "Livello aggiornato.");
        }

        protected void LivelliGrid_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            ExecuteAction(() => _repository.DeleteLivelloEfficienza(GetGridKey(LivelliGrid, e.RowIndex)), "Livello rimosso.");
        }

        protected void StanzeGrid_RowEditing(object sender, GridViewEditEventArgs e)
        {
            StanzeGrid.EditIndex = e.NewEditIndex;
            LoadDomains();
        }

        protected void StanzeGrid_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            StanzeGrid.EditIndex = -1;
            LoadDomains();
        }

        protected void StanzeGrid_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            ExecuteAction(() =>
            {
                var row = StanzeGrid.Rows[e.RowIndex];
                _repository.UpdateStanza(GetGridKey(StanzeGrid, e.RowIndex), RequireText(((TextBox)row.FindControl("EditStanzaNomeText")).Text, "Numero stanza"));
                StanzeGrid.EditIndex = -1;
            }, "Stanza aggiornata.");
        }

        protected void StanzeGrid_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            ExecuteAction(() => _repository.DeleteStanza(GetGridKey(StanzeGrid, e.RowIndex)), "Stanza rimossa.");
        }

        protected void DitteGrid_RowEditing(object sender, GridViewEditEventArgs e)
        {
            DitteGrid.EditIndex = e.NewEditIndex;
            LoadDomains();
        }

        protected void DitteGrid_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            DitteGrid.EditIndex = -1;
            LoadDomains();
        }

        protected void DitteGrid_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            ExecuteAction(() =>
            {
                var row = DitteGrid.Rows[e.RowIndex];
                _repository.UpdateDitta(
                    GetGridKey(DitteGrid, e.RowIndex),
                    RequireText(((TextBox)row.FindControl("EditDittaNomeText")).Text, "Nome ditta"),
                    ((TextBox)row.FindControl("EditDittaCittaText")).Text,
                    ((TextBox)row.FindControl("EditDittaMailText")).Text,
                    ((TextBox)row.FindControl("EditDittaTipologiaText")).Text);
                DitteGrid.EditIndex = -1;
            }, "Ditta aggiornata.");
        }

        protected void DitteGrid_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            ExecuteAction(() => _repository.DeleteDitta(GetGridKey(DitteGrid, e.RowIndex)), "Ditta rimossa.");
        }

        protected void TipiOggettoGrid_RowEditing(object sender, GridViewEditEventArgs e)
        {
            TipiOggettoGrid.EditIndex = e.NewEditIndex;
            LoadDomains();
        }

        protected void TipiOggettoGrid_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            TipiOggettoGrid.EditIndex = -1;
            LoadDomains();
        }

        protected void TipiOggettoGrid_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            ExecuteAction(() =>
            {
                var row = TipiOggettoGrid.Rows[e.RowIndex];
                _repository.UpdateTipoOggetto(GetGridKey(TipiOggettoGrid, e.RowIndex), RequireText(((TextBox)row.FindControl("EditTipoOggettoNomeText")).Text, "Descrizione tipo oggetto"));
                TipiOggettoGrid.EditIndex = -1;
            }, "Tipo oggetto aggiornato.");
        }

        protected void TipiOggettoGrid_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            ExecuteAction(() => _repository.DeleteTipoOggetto(GetGridKey(TipiOggettoGrid, e.RowIndex)), "Tipo oggetto rimosso.");
        }

        private void ExecuteAction(Action action, string successMessage)
        {
            try
            {
                ErrorPanel.Visible = false;
                SuccessPanel.Visible = false;
                action();
                LoadDomains();
                SuccessPanel.Visible = true;
                SuccessMessage.Text = Server.HtmlEncode(successMessage);
            }
            catch (Exception ex)
            {
                AppLogger.Error("Domini.ExecuteAction", "Errore gestione CRUD domini.", ex);
                ShowError(ex);
            }
        }

        private void ShowError(Exception ex)
        {
            SuccessPanel.Visible = false;
            ErrorPanel.Visible = true;
            ErrorMessage.Text = Server.HtmlEncode(ex.Message);
        }

        private void BindTipoOggettoDropDown(DropDownList control, int? selectedId)
        {
            control.Items.Clear();
            control.Items.Add(new ListItem("-- seleziona --", string.Empty));
            control.DataSource = _repository.GetTipiOggettoOrdinativoLookup();
            control.DataTextField = "DisplayName";
            control.DataValueField = "Id";
            control.DataBind();

            if (selectedId.HasValue)
            {
                var selected = control.Items.FindByValue(selectedId.Value.ToString(CultureInfo.InvariantCulture));
                if (selected != null)
                {
                    control.ClearSelection();
                    selected.Selected = true;
                }
            }
        }

        private static int GetGridKey(GridView grid, int rowIndex)
        {
            return Convert.ToInt32(grid.DataKeys[rowIndex].Value, CultureInfo.InvariantCulture);
        }

        private static string RequireText(string value, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(fieldName + " obbligatorio.");
            }

            return value.Trim();
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
