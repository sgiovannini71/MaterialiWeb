using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web.UI.WebControls;
using MaterialiGestioneWeb.Infrastructure;
using MaterialiGestioneWeb.Models;
using MaterialiGestioneWeb.Services;

namespace MaterialiGestioneWeb
{
    public partial class GestioneCrudPage : Auth.BaseAuthenticatedPage
    {
        private readonly InventarioRepository _repository = new InventarioRepository();
        private readonly PersonaleRepository _personaleRepository = new PersonaleRepository();

        protected override int[] LivelliConsentiti
        {
            get { return new[] { (int)Auth.LivelliUtente.Amministratore }; }
        }

        protected void Page_Init(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                BindAllPersonaleDropDown(ProdPersPersonaleDropDown, null);
                BindAllPersonaleDropDown(StoricoPersonaleDropDown, null);
            }
        }

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
                OrdinativiGrid.DataSource = _repository.GetOrdinativiAdmin();
                OrdinativiGrid.DataBind();
            }
            OggettiGrid.DataSource = _repository.GetOggettiOrdinativoAdmin(selectedOggettoOrdinativoId);
            OggettiGrid.DataBind();
            if (!isContextMode)
            {
                NetworkGrid.DataSource = _repository.GetNetworkDataAdmin();
                NetworkGrid.DataBind();
                PostazioniGrid.DataSource = _repository.GetPostazioniAdmin();
                PostazioniGrid.DataBind();
                ProdPersGrid.DataSource = _repository.GetProdPersAdmin();
                ProdPersGrid.DataBind();
                StoricoGrid.DataSource = _repository.GetProdPersStoricoAdmin();
                StoricoGrid.DataBind();
            }
            BindLookupDropDown(OggettoOrdinativoDropDown, _repository.GetOrdinativiLookup(), selectedOggettoOrdinativoId);
            BindLookupDropDown(OggettoDittaDropDown, _repository.GetDitteLookup(), null);
            BindLookupDropDown(OggettoCategoriaDropDown, _repository.GetCategorieLookup(), null);
            if (!isContextMode)
            {
                BindLookupDropDown(NetworkProdottoDropDown, _repository.GetProdottiNetworkLookup(), null);
                BindLookupDropDown(PostazioneProdottoDropDown, _repository.GetProdottiLookup(), null);
                BindLookupDropDown(PostazioneNomeDropDown, _repository.GetNomiMacchinaLookup(), null);
                BindLookupDropDown(ProdPersProdottoDropDown, _repository.GetProdottiLookup(), null);
                BindLookupDropDown(StoricoProdottoDropDown, _repository.GetProdottiLookup(), null);
                BindPersonaleDropDown(ProdPersPersonaleDropDown, IsProdPersPersonaleEsternoSelected(), ProdPersMostraNonAttiviCheck.Checked, null);
                BindPersonaleDropDown(StoricoPersonaleDropDown, IsStoricoPersonaleEsternoSelected(), StoricoMostraNonAttiviCheck.Checked, null);
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

        protected void AddNetworkButton_Click(object sender, EventArgs e)
        {
            ExecuteAction(() =>
            {
                _repository.CreateNetworkDataAdmin(new NetworkDataAdminItem
                {
                    IdProdotto = ParseRequiredInt(NetworkProdottoDropDown.SelectedValue, "Prodotto rete"),
                    MacAddress = RequireText(NetworkMacText.Text, "MAC address"),
                    Note = NetworkNoteText.Text
                });
                ClearText(NetworkMacText, NetworkNoteText);
                ResetDropDown(NetworkProdottoDropDown);
            }, "Record rete inserito.");
        }

        protected void AddPostazioneButton_Click(object sender, EventArgs e)
        {
            ExecuteAction(() =>
            {
                _repository.CreatePostazioneAdmin(new PostazioneAdminItem
                {
                    IdProdotto = ParseRequiredInt(PostazioneProdottoDropDown.SelectedValue, "Prodotto postazione"),
                    IdNomeMacchina = ParseOptionalInt(PostazioneNomeDropDown.SelectedValue)
                });
                ResetDropDown(PostazioneProdottoDropDown);
                ResetDropDown(PostazioneNomeDropDown);
            }, "Postazione inserita.");
        }

        protected void AddProdPersButton_Click(object sender, EventArgs e)
        {
            ExecuteAction(() =>
            {
                _repository.CreateProdPersAdmin(new ProdPersAdminItem
                {
                    IdProdotto = ParseOptionalInt(ProdPersProdottoDropDown.SelectedValue),
                    IdPersonale = ParseOptionalInt(ProdPersPersonaleDropDown.SelectedValue),
                    DataAssegnazione = ParseOptionalDate(ProdPersDataText.Text)
                });
                ClearText(ProdPersDataText);
                ResetDropDown(ProdPersProdottoDropDown);
                ResetDropDown(ProdPersPersonaleDropDown);
            }, "Assegnazione corrente inserita.");
        }

        protected void AddStoricoButton_Click(object sender, EventArgs e)
        {
            ExecuteAction(() =>
            {
                _repository.CreateProdPersStoricoAdmin(new ProdPersStoricoAdminItem
                {
                    IdProdPers = ParseOptionalInt(StoricoProdPersIdText.Text),
                    IdProdotto = ParseOptionalInt(StoricoProdottoDropDown.SelectedValue),
                    IdPersonale = ParseOptionalInt(StoricoPersonaleDropDown.SelectedValue),
                    DataAssegnazione = ParseOptionalDate(StoricoDataAssText.Text),
                    DataRestituzione = ParseOptionalDate(StoricoDataResText.Text),
                    NumeroStanza = StoricoStanzaText.Text,
                    LivelloEfficienza = StoricoLivelloText.Text,
                    NomeMacchina = StoricoMacchinaText.Text,
                    SerialNumber = StoricoSerialeText.Text,
                    NoteProdotto = StoricoNoteText.Text
                });
                ClearText(StoricoProdPersIdText, StoricoDataAssText, StoricoDataResText, StoricoStanzaText, StoricoLivelloText, StoricoMacchinaText, StoricoSerialeText, StoricoNoteText);
                ResetDropDown(StoricoProdottoDropDown);
                ResetDropDown(StoricoPersonaleDropDown);
            }, "Storico assegnazione inserito.");
        }

        protected void ProdPersTipoPersonaleRadio_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindPersonaleDropDown(ProdPersPersonaleDropDown, IsProdPersPersonaleEsternoSelected(), ProdPersMostraNonAttiviCheck.Checked, null);
        }

        protected void ProdPersMostraNonAttiviCheck_CheckedChanged(object sender, EventArgs e)
        {
            BindPersonaleDropDown(ProdPersPersonaleDropDown, IsProdPersPersonaleEsternoSelected(), ProdPersMostraNonAttiviCheck.Checked, null);
        }

        protected void StoricoTipoPersonaleRadio_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindPersonaleDropDown(StoricoPersonaleDropDown, IsStoricoPersonaleEsternoSelected(), StoricoMostraNonAttiviCheck.Checked, null);
        }

        protected void StoricoMostraNonAttiviCheck_CheckedChanged(object sender, EventArgs e)
        {
            BindPersonaleDropDown(StoricoPersonaleDropDown, IsStoricoPersonaleEsternoSelected(), StoricoMostraNonAttiviCheck.Checked, null);
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

        protected void NetworkGrid_RowEditing(object sender, GridViewEditEventArgs e) { SetEdit(NetworkGrid, e.NewEditIndex); }
        protected void NetworkGrid_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e) { CancelEdit(NetworkGrid); }
        protected void NetworkGrid_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            ExecuteAction(() =>
            {
                var row = NetworkGrid.Rows[e.RowIndex];
                _repository.UpdateNetworkDataAdmin(new NetworkDataAdminItem
                {
                    IdNetworkData = GetGridKey(NetworkGrid, e.RowIndex),
                    IdProdotto = GetDropDownRequiredValue(row, "EditNetworkProdottoDropDown", "Prodotto rete"),
                    MacAddress = RequireText(GetText(row, "EditNetworkMacText"), "MAC address"),
                    Note = GetText(row, "EditNetworkNoteText")
                });
                NetworkGrid.EditIndex = -1;
            }, "Record rete aggiornato.");
        }
        protected void NetworkGrid_RowDeleting(object sender, GridViewDeleteEventArgs e) { ExecuteAction(() => _repository.DeleteNetworkDataAdmin(GetGridKey(NetworkGrid, e.RowIndex)), "Record rete eliminato."); }

        protected void NetworkGrid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (!IsEditRow(e.Row))
            {
                return;
            }

            var item = (NetworkDataAdminItem)e.Row.DataItem;
            BindLookupDropDown((DropDownList)e.Row.FindControl("EditNetworkProdottoDropDown"), _repository.GetProdottiNetworkLookup(), item.IdProdotto);
        }

        protected void PostazioniGrid_RowEditing(object sender, GridViewEditEventArgs e) { SetEdit(PostazioniGrid, e.NewEditIndex); }
        protected void PostazioniGrid_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e) { CancelEdit(PostazioniGrid); }
        protected void PostazioniGrid_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            ExecuteAction(() =>
            {
                var row = PostazioniGrid.Rows[e.RowIndex];
                _repository.UpdatePostazioneAdmin(new PostazioneAdminItem
                {
                    IdPostazione = GetGridKey(PostazioniGrid, e.RowIndex),
                    IdProdotto = GetDropDownRequiredValue(row, "EditPostazioneProdottoDropDown", "Prodotto postazione"),
                    IdNomeMacchina = GetDropDownSelectedValue(row, "EditPostazioneNomeDropDown")
                });
                PostazioniGrid.EditIndex = -1;
            }, "Postazione aggiornata.");
        }
        protected void PostazioniGrid_RowDeleting(object sender, GridViewDeleteEventArgs e) { ExecuteAction(() => _repository.DeletePostazioneAdmin(GetGridKey(PostazioniGrid, e.RowIndex)), "Postazione eliminata."); }

        protected void PostazioniGrid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (!IsEditRow(e.Row))
            {
                return;
            }

            var item = (PostazioneAdminItem)e.Row.DataItem;
            BindLookupDropDown((DropDownList)e.Row.FindControl("EditPostazioneProdottoDropDown"), _repository.GetProdottiLookup(), item.IdProdotto);
            BindLookupDropDown((DropDownList)e.Row.FindControl("EditPostazioneNomeDropDown"), _repository.GetNomiMacchinaLookup(), item.IdNomeMacchina);
        }

        protected void ProdPersGrid_RowEditing(object sender, GridViewEditEventArgs e) { SetEdit(ProdPersGrid, e.NewEditIndex); }
        protected void ProdPersGrid_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e) { CancelEdit(ProdPersGrid); }
        protected void ProdPersGrid_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            ExecuteAction(() =>
            {
                var row = ProdPersGrid.Rows[e.RowIndex];
                _repository.UpdateProdPersAdmin(new ProdPersAdminItem
                {
                    IdProdPers = GetGridKey(ProdPersGrid, e.RowIndex),
                    IdProdotto = GetDropDownSelectedValue(row, "EditProdPersProdottoDropDown"),
                    IdPersonale = GetDropDownSelectedValue(row, "EditProdPersPersonaleDropDown"),
                    DataAssegnazione = ParseOptionalDate(GetText(row, "EditProdPersDataText"))
                });
                ProdPersGrid.EditIndex = -1;
            }, "Assegnazione corrente aggiornata.");
        }
        protected void ProdPersGrid_RowDeleting(object sender, GridViewDeleteEventArgs e) { ExecuteAction(() => _repository.DeleteProdPersAdmin(GetGridKey(ProdPersGrid, e.RowIndex)), "Assegnazione corrente eliminata."); }

        protected void ProdPersGrid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (!IsEditRow(e.Row))
            {
                return;
            }

            var item = (ProdPersAdminItem)e.Row.DataItem;
            BindLookupDropDown((DropDownList)e.Row.FindControl("EditProdPersProdottoDropDown"), _repository.GetProdottiLookup(), item.IdProdotto);
            BindAllPersonaleDropDown((DropDownList)e.Row.FindControl("EditProdPersPersonaleDropDown"), item.IdPersonale);
        }

        protected void StoricoGrid_RowEditing(object sender, GridViewEditEventArgs e) { SetEdit(StoricoGrid, e.NewEditIndex); }
        protected void StoricoGrid_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e) { CancelEdit(StoricoGrid); }
        protected void StoricoGrid_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            ExecuteAction(() =>
            {
                var row = StoricoGrid.Rows[e.RowIndex];
                _repository.UpdateProdPersStoricoAdmin(new ProdPersStoricoAdminItem
                {
                    Id = GetGridKey(StoricoGrid, e.RowIndex),
                    IdProdPers = ParseOptionalInt(GetText(row, "EditStoricoProdPersIdText")),
                    IdProdotto = GetDropDownSelectedValue(row, "EditStoricoProdottoDropDown"),
                    IdPersonale = GetDropDownSelectedValue(row, "EditStoricoPersonaleDropDown"),
                    DataAssegnazione = ParseOptionalDate(GetText(row, "EditStoricoDataAssText")),
                    DataRestituzione = ParseOptionalDate(GetText(row, "EditStoricoDataResText")),
                    NumeroStanza = GetText(row, "EditStoricoStanzaText"),
                    LivelloEfficienza = GetText(row, "EditStoricoLivelloText"),
                    NomeMacchina = GetText(row, "EditStoricoMacchinaText"),
                    SerialNumber = GetText(row, "EditStoricoSerialeText"),
                    NoteProdotto = GetText(row, "EditStoricoNoteText")
                });
                StoricoGrid.EditIndex = -1;
            }, "Storico assegnazione aggiornato.");
        }
        protected void StoricoGrid_RowDeleting(object sender, GridViewDeleteEventArgs e) { ExecuteAction(() => _repository.DeleteProdPersStoricoAdmin(GetGridKey(StoricoGrid, e.RowIndex)), "Storico assegnazione eliminato."); }

        protected void StoricoGrid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (!IsEditRow(e.Row))
            {
                return;
            }

            var item = (ProdPersStoricoAdminItem)e.Row.DataItem;
            BindLookupDropDown((DropDownList)e.Row.FindControl("EditStoricoProdottoDropDown"), _repository.GetProdottiLookup(), item.IdProdotto);
            BindAllPersonaleDropDown((DropDownList)e.Row.FindControl("EditStoricoPersonaleDropDown"), item.IdPersonale);
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
                PageIntroText.Text = "Gestione completa di prodotti, ordinativi, oggetti ordinativo, rete, postazioni, assegnazioni correnti e storico assegnazioni.";
                return;
            }

            ContextCssLiteral.Text = @"
<style>
    .domain-jump-nav,
    #crud-prodotti,
    #crud-ordinativi,
    #crud-network,
    #crud-postazioni,
    #crud-prodpers,
    #crud-storico {
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

        private void BindPersonaleDropDown(DropDownList control, bool isEsterno, bool includeNonAttivi, int? selectedId)
        {
            if (control == null)
            {
                return;
            }

            control.Items.Clear();
            control.Items.Add(new ListItem("-- seleziona personale --", string.Empty));
            control.AppendDataBoundItems = true;
            control.DataSource = _personaleRepository.GetPersonale(isEsterno, includeNonAttivi);
            control.DataTextField = "DisplayName";
            control.DataValueField = "Id";
            control.DataBind();
            control.AppendDataBoundItems = false;
            SelectDropDownValue(control, selectedId);
        }

        private void BindAllPersonaleDropDown(DropDownList control, int? selectedId)
        {
            if (control == null)
            {
                return;
            }

            var items = new List<LookupItem>();
            foreach (var item in _personaleRepository.GetPersonale(false, true))
            {
                items.Add(new LookupItem { Id = item.Id, Nome = "Interno - " + item.DisplayName });
            }

            foreach (var item in _personaleRepository.GetPersonale(true, true))
            {
                items.Add(new LookupItem { Id = item.Id, Nome = "Esterno - " + item.DisplayName });
            }

            BindLookupDropDown(control, items, selectedId);
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

        private bool IsProdPersPersonaleEsternoSelected()
        {
            return ProdPersTipoPersonaleRadio.SelectedValue == "E";
        }

        private bool IsStoricoPersonaleEsternoSelected()
        {
            return StoricoTipoPersonaleRadio.SelectedValue == "E";
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
