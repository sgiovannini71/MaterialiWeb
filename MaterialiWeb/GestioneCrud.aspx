<%@ Page Title="CRUD amministrativi" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="GestioneCrud.aspx.cs" Inherits="MaterialiGestioneWeb.GestioneCrudPage" %>

<asp:Content ID="TitleCrud" ContentPlaceHolderID="TitleContent" runat="server">CRUD amministrativi</asp:Content>
<asp:Content ID="MainCrud" ContentPlaceHolderID="MainContent" runat="server">
    <section id="crud-top" class="page-title">
        <h1>CRUD amministrativi</h1>
        <p>Gestione completa di prodotti, ordinativi, oggetti ordinativo, rete, postazioni, assegnazioni correnti e storico assegnazioni.</p>
    </section>

    <asp:Panel ID="ErrorPanel" runat="server" CssClass="alert error" Visible="false"><asp:Literal ID="ErrorMessage" runat="server" /></asp:Panel>
    <asp:Panel ID="SuccessPanel" runat="server" CssClass="alert success" Visible="false"><asp:Literal ID="SuccessMessage" runat="server" /></asp:Panel>

    <nav class="domain-jump-nav" aria-label="Accesso rapido ai CRUD amministrativi">
        <a href="#crud-prodotti">Prodotti</a>
        <a href="#crud-ordinativi">Ordinativi</a>
        <a href="#crud-oggetti">Oggetti ordinativo</a>
        <a href="#crud-network">NetworkData</a>
        <a href="#crud-postazioni">Postazioni</a>
        <a href="#crud-prodpers">ProdPers</a>
        <a href="#crud-storico">ProdPersStorico</a>
    </nav>

    <section id="crud-prodotti" class="form-card domain-section">
        <div class="domain-title">
            <h2>Prodotti</h2>
            <a href="#crud-top">Torna su</a>
        </div>
        <div class="field-grid">
            <label>Categorico<asp:TextBox ID="ProdottoCategoricoText" runat="server" CssClass="input" /></label>
            <label>Matricola<asp:TextBox ID="ProdottoMatricolaText" runat="server" CssClass="input" /></label>
            <label>Stanza<asp:DropDownList ID="ProdottoStanzaDropDown" runat="server" CssClass="input" /></label>
            <label>Ordinativo<asp:DropDownList ID="ProdottoOrdinativoDropDown" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ProdottoOrdinativoDropDown_SelectedIndexChanged" /></label>
            <label>Oggetto ordinativo<asp:DropDownList ID="ProdottoOggettoDropDown" runat="server" CssClass="input" /></label>
            <label>Efficienza<asp:DropDownList ID="ProdottoEfficienzaDropDown" runat="server" CssClass="input" /></label>
            <label>Versamento<asp:TextBox ID="ProdottoVersamentoText" runat="server" CssClass="input" /></label>
            <label class="field-span-2">Note<asp:TextBox ID="ProdottoNoteText" runat="server" CssClass="input" TextMode="MultiLine" Rows="2" /></label>
        </div>
        <div class="page-actions"><asp:Button ID="AddProdottoButton" runat="server" Text="Aggiungi prodotto" CssClass="button primary" OnClick="AddProdottoButton_Click" /></div>
        <asp:GridView ID="ProdottiAdminGrid" runat="server" AutoGenerateColumns="False" CssClass="data-grid compact" GridLines="None" DataKeyNames="IdProdotto"
            OnRowEditing="ProdottiAdminGrid_RowEditing" OnRowCancelingEdit="ProdottiAdminGrid_RowCancelingEdit" OnRowUpdating="ProdottiAdminGrid_RowUpdating" OnRowDeleting="ProdottiAdminGrid_RowDeleting" OnRowDataBound="ProdottiAdminGrid_RowDataBound">
            <Columns>
                <asp:BoundField DataField="IdProdotto" HeaderText="Id" ReadOnly="True" />
                <asp:TemplateField HeaderText="Categorico"><ItemTemplate><%# Eval("Categorico") %></ItemTemplate><EditItemTemplate><asp:TextBox ID="EditProdottoCategoricoText" runat="server" CssClass="input" Text='<%# Bind("Categorico") %>' /></EditItemTemplate></asp:TemplateField>
                <asp:TemplateField HeaderText="Matricola"><ItemTemplate><%# Eval("Matricola") %></ItemTemplate><EditItemTemplate><asp:TextBox ID="EditProdottoMatricolaText" runat="server" CssClass="input" Text='<%# Bind("Matricola") %>' /></EditItemTemplate></asp:TemplateField>
                <asp:TemplateField HeaderText="Stanza"><ItemTemplate><%# Eval("StanzaDescrizione") %></ItemTemplate><EditItemTemplate><asp:DropDownList ID="EditProdottoStanzaDropDown" runat="server" CssClass="input" /></EditItemTemplate></asp:TemplateField>
                <asp:TemplateField HeaderText="Ordinativo"><ItemTemplate><%# Eval("OrdinativoDescrizione") %></ItemTemplate><EditItemTemplate><asp:DropDownList ID="EditProdottoOrdinativoDropDown" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="EditProdottoOrdinativoDropDown_SelectedIndexChanged" /></EditItemTemplate></asp:TemplateField>
                <asp:TemplateField HeaderText="Oggetto"><ItemTemplate><%# Eval("OggettoDescrizione") %></ItemTemplate><EditItemTemplate><asp:DropDownList ID="EditProdottoOggettoDropDown" runat="server" CssClass="input" /></EditItemTemplate></asp:TemplateField>
                <asp:TemplateField HeaderText="Efficienza"><ItemTemplate><%# Eval("EfficienzaDescrizione") %></ItemTemplate><EditItemTemplate><asp:DropDownList ID="EditProdottoEfficienzaDropDown" runat="server" CssClass="input" /></EditItemTemplate></asp:TemplateField>
                <asp:TemplateField HeaderText="Versamento"><ItemTemplate><%# Eval("Versamento") %></ItemTemplate><EditItemTemplate><asp:TextBox ID="EditProdottoVersamentoText" runat="server" CssClass="input" Text='<%# Bind("Versamento") %>' /></EditItemTemplate></asp:TemplateField>
                <asp:TemplateField HeaderText="Note"><ItemTemplate><%# Eval("Note") %></ItemTemplate><EditItemTemplate><asp:TextBox ID="EditProdottoNoteText" runat="server" CssClass="input" TextMode="MultiLine" Rows="2" Text='<%# Bind("Note") %>' /></EditItemTemplate></asp:TemplateField>
                <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" />
            </Columns>
        </asp:GridView>
    </section>

    <section id="crud-ordinativi" class="form-card domain-section">
        <div class="domain-title">
            <h2>Ordinativi</h2>
            <a href="#crud-top">Torna su</a>
        </div>
        <div class="field-grid">
            <label>Codice<asp:TextBox ID="OrdinativoCodiceText" runat="server" CssClass="input" ReadOnly="true" /></label>
            <label>Denominazione<asp:TextBox ID="OrdinativoDenominazioneText" runat="server" CssClass="input" /></label>
        </div>
        <div class="page-actions"><asp:Button ID="AddOrdinativoButton" runat="server" Text="Aggiungi ordinativo" CssClass="button primary" OnClick="AddOrdinativoButton_Click" /></div>
        <asp:GridView ID="OrdinativiGrid" runat="server" AutoGenerateColumns="False" CssClass="data-grid compact" GridLines="None" DataKeyNames="IdOrdinativo"
            OnRowEditing="OrdinativiGrid_RowEditing" OnRowCancelingEdit="OrdinativiGrid_RowCancelingEdit" OnRowUpdating="OrdinativiGrid_RowUpdating" OnRowDeleting="OrdinativiGrid_RowDeleting">
            <Columns>
                <asp:BoundField DataField="IdOrdinativo" HeaderText="Id" ReadOnly="True" />
                <asp:TemplateField HeaderText="Codice"><ItemTemplate><%# Eval("CodiceOrdinativo") %></ItemTemplate><EditItemTemplate><asp:TextBox ID="EditOrdinativoCodiceText" runat="server" CssClass="input" Text='<%# Bind("CodiceOrdinativo") %>' /></EditItemTemplate></asp:TemplateField>
                <asp:TemplateField HeaderText="Denominazione"><ItemTemplate><%# Eval("DenominazioneOrdinativo") %></ItemTemplate><EditItemTemplate><asp:TextBox ID="EditOrdinativoDenominazioneText" runat="server" CssClass="input" Text='<%# Bind("DenominazioneOrdinativo") %>' /></EditItemTemplate></asp:TemplateField>
                <asp:HyperLinkField Text="Composizione" DataNavigateUrlFields="IdOrdinativo" DataNavigateUrlFormatString="DettaglioOrdinativo.aspx?id={0}" />
                <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" />
            </Columns>
        </asp:GridView>
    </section>

    <section id="crud-oggetti" class="form-card domain-section">
        <div class="domain-title">
            <h2>Oggetti ordinativo</h2>
            <a href="#crud-top">Torna su</a>
        </div>
        <div class="field-grid">
            <label>Ordinativo<asp:DropDownList ID="OggettoOrdinativoDropDown" runat="server" CssClass="input" /></label>
            <label>Descrizione<asp:TextBox ID="OggettoDescrizioneText" runat="server" CssClass="input" /></label>
            <label>Ditta costruttrice<asp:DropDownList ID="OggettoDittaDropDown" runat="server" CssClass="input" /></label>
            <label>Modello<asp:TextBox ID="OggettoModelloText" runat="server" CssClass="input" /></label>
            <label>NUC<asp:TextBox ID="OggettoNucText" runat="server" CssClass="input" /></label>
            <label>Quantita<asp:TextBox ID="OggettoQuantitaText" runat="server" CssClass="input" TextMode="Number" /></label>
            <label>Categoria<asp:DropDownList ID="OggettoCategoriaDropDown" runat="server" CssClass="input" /></label>
        </div>
        <div class="page-actions"><asp:Button ID="AddOggettoButton" runat="server" Text="Aggiungi oggetto ordinativo" CssClass="button primary" OnClick="AddOggettoButton_Click" /></div>
        <asp:GridView ID="OggettiGrid" runat="server" AutoGenerateColumns="False" CssClass="data-grid compact" GridLines="None" DataKeyNames="IdOggOrdinativo"
            OnRowEditing="OggettiGrid_RowEditing" OnRowCancelingEdit="OggettiGrid_RowCancelingEdit" OnRowUpdating="OggettiGrid_RowUpdating" OnRowDeleting="OggettiGrid_RowDeleting" OnRowDataBound="OggettiGrid_RowDataBound">
            <Columns>
                <asp:BoundField DataField="IdOggOrdinativo" HeaderText="Id" ReadOnly="True" />
                <asp:TemplateField HeaderText="Ordinativo"><ItemTemplate><%# Eval("OrdinativoDescrizione") %></ItemTemplate><EditItemTemplate><asp:DropDownList ID="EditOggettoOrdinativoDropDown" runat="server" CssClass="input" /></EditItemTemplate></asp:TemplateField>
                <asp:TemplateField HeaderText="Descrizione"><ItemTemplate><%# Eval("DescrizioneProdotto") %></ItemTemplate><EditItemTemplate><asp:TextBox ID="EditOggettoDescrizioneText" runat="server" CssClass="input" Text='<%# Bind("DescrizioneProdotto") %>' /></EditItemTemplate></asp:TemplateField>
                <asp:TemplateField HeaderText="Ditta"><ItemTemplate><%# Eval("DittaDescrizione") %></ItemTemplate><EditItemTemplate><asp:DropDownList ID="EditOggettoDittaDropDown" runat="server" CssClass="input" /></EditItemTemplate></asp:TemplateField>
                <asp:TemplateField HeaderText="Modello"><ItemTemplate><%# Eval("Modello") %></ItemTemplate><EditItemTemplate><asp:TextBox ID="EditOggettoModelloText" runat="server" CssClass="input" Text='<%# Bind("Modello") %>' /></EditItemTemplate></asp:TemplateField>
                <asp:TemplateField HeaderText="NUC"><ItemTemplate><%# Eval("NUC") %></ItemTemplate><EditItemTemplate><asp:TextBox ID="EditOggettoNucText" runat="server" CssClass="input" Text='<%# Bind("NUC") %>' /></EditItemTemplate></asp:TemplateField>
                <asp:BoundField DataField="Quantita" HeaderText="Quantita" ReadOnly="True" />
                <asp:TemplateField HeaderText="Categoria"><ItemTemplate><%# Eval("CategoriaDescrizione") %></ItemTemplate><EditItemTemplate><asp:DropDownList ID="EditOggettoCategoriaDropDown" runat="server" CssClass="input" /></EditItemTemplate></asp:TemplateField>
                <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" />
            </Columns>
        </asp:GridView>
    </section>

    <section class="split-grid">
        <div id="crud-network" class="form-card domain-section">
            <div class="domain-title">
                <h2>NetworkData</h2>
                <a href="#crud-top">Torna su</a>
            </div>
            <div class="field-grid">
                <label>Prodotto<asp:DropDownList ID="NetworkProdottoDropDown" runat="server" CssClass="input" /></label>
                <label>MAC address<asp:TextBox ID="NetworkMacText" runat="server" CssClass="input" /></label>
                <label class="field-span-2">Note<asp:TextBox ID="NetworkNoteText" runat="server" CssClass="input" /></label>
            </div>
            <div class="page-actions"><asp:Button ID="AddNetworkButton" runat="server" Text="Aggiungi rete" CssClass="button primary" OnClick="AddNetworkButton_Click" /></div>
            <asp:GridView ID="NetworkGrid" runat="server" AutoGenerateColumns="False" CssClass="data-grid compact" GridLines="None" DataKeyNames="IdNetworkData"
                OnRowEditing="NetworkGrid_RowEditing" OnRowCancelingEdit="NetworkGrid_RowCancelingEdit" OnRowUpdating="NetworkGrid_RowUpdating" OnRowDeleting="NetworkGrid_RowDeleting" OnRowDataBound="NetworkGrid_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="IdNetworkData" HeaderText="Id" ReadOnly="True" />
                    <asp:TemplateField HeaderText="Prodotto"><ItemTemplate><%# Eval("ProdottoDescrizione") %></ItemTemplate><EditItemTemplate><asp:DropDownList ID="EditNetworkProdottoDropDown" runat="server" CssClass="input" /></EditItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="MAC"><ItemTemplate><%# Eval("MacAddress") %></ItemTemplate><EditItemTemplate><asp:TextBox ID="EditNetworkMacText" runat="server" CssClass="input" Text='<%# Bind("MacAddress") %>' /></EditItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="Note"><ItemTemplate><%# Eval("Note") %></ItemTemplate><EditItemTemplate><asp:TextBox ID="EditNetworkNoteText" runat="server" CssClass="input" Text='<%# Bind("Note") %>' /></EditItemTemplate></asp:TemplateField>
                    <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" />
                </Columns>
            </asp:GridView>
        </div>
        <div id="crud-postazioni" class="form-card domain-section">
            <div class="domain-title">
                <h2>Postazioni</h2>
                <a href="#crud-top">Torna su</a>
            </div>
            <div class="field-grid">
                <label>Prodotto<asp:DropDownList ID="PostazioneProdottoDropDown" runat="server" CssClass="input" /></label>
                <label>Nome macchina<asp:DropDownList ID="PostazioneNomeDropDown" runat="server" CssClass="input" /></label>
            </div>
            <div class="page-actions"><asp:Button ID="AddPostazioneButton" runat="server" Text="Aggiungi postazione" CssClass="button primary" OnClick="AddPostazioneButton_Click" /></div>
            <asp:GridView ID="PostazioniGrid" runat="server" AutoGenerateColumns="False" CssClass="data-grid compact" GridLines="None" DataKeyNames="IdPostazione"
                OnRowEditing="PostazioniGrid_RowEditing" OnRowCancelingEdit="PostazioniGrid_RowCancelingEdit" OnRowUpdating="PostazioniGrid_RowUpdating" OnRowDeleting="PostazioniGrid_RowDeleting" OnRowDataBound="PostazioniGrid_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="IdPostazione" HeaderText="Id" ReadOnly="True" />
                    <asp:TemplateField HeaderText="Prodotto"><ItemTemplate><%# Eval("ProdottoDescrizione") %></ItemTemplate><EditItemTemplate><asp:DropDownList ID="EditPostazioneProdottoDropDown" runat="server" CssClass="input" /></EditItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="Nome macchina"><ItemTemplate><%# Eval("NomeMacchina") %></ItemTemplate><EditItemTemplate><asp:DropDownList ID="EditPostazioneNomeDropDown" runat="server" CssClass="input" /></EditItemTemplate></asp:TemplateField>
                    <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" />
                </Columns>
            </asp:GridView>
        </div>
    </section>

    <section class="split-grid">
        <div id="crud-prodpers" class="form-card domain-section">
            <div class="domain-title">
                <h2>ProdPers</h2>
                <a href="#crud-top">Torna su</a>
            </div>
            <div class="field-grid">
                <label>Prodotto<asp:DropDownList ID="ProdPersProdottoDropDown" runat="server" CssClass="input" /></label>
                <label>Tipo personale
                    <asp:RadioButtonList ID="ProdPersTipoPersonaleRadio" runat="server" CssClass="radio-list" RepeatDirection="Horizontal" AutoPostBack="true" EnableViewState="false" OnSelectedIndexChanged="ProdPersTipoPersonaleRadio_SelectedIndexChanged">
                        <asp:ListItem Value="I" Selected="True">Interno</asp:ListItem>
                        <asp:ListItem Value="E">Esterno</asp:ListItem>
                    </asp:RadioButtonList>
                </label>
                <label class="checkbox-field"><asp:CheckBox ID="ProdPersMostraNonAttiviCheck" runat="server" Text="Includi personale non attivo" AutoPostBack="true" EnableViewState="false" OnCheckedChanged="ProdPersMostraNonAttiviCheck_CheckedChanged" /></label>
                <label>Personale<asp:DropDownList ID="ProdPersPersonaleDropDown" runat="server" CssClass="input" EnableViewState="false" /></label>
                <label>Data assegnazione<asp:TextBox ID="ProdPersDataText" runat="server" CssClass="input" TextMode="Date" /></label>
            </div>
            <div class="page-actions"><asp:Button ID="AddProdPersButton" runat="server" Text="Aggiungi assegnazione corrente" CssClass="button primary" OnClick="AddProdPersButton_Click" /></div>
            <asp:GridView ID="ProdPersGrid" runat="server" AutoGenerateColumns="False" CssClass="data-grid compact" GridLines="None" DataKeyNames="IdProdPers"
                OnRowEditing="ProdPersGrid_RowEditing" OnRowCancelingEdit="ProdPersGrid_RowCancelingEdit" OnRowUpdating="ProdPersGrid_RowUpdating" OnRowDeleting="ProdPersGrid_RowDeleting" OnRowDataBound="ProdPersGrid_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="IdProdPers" HeaderText="Id" ReadOnly="True" />
                    <asp:TemplateField HeaderText="Prodotto"><ItemTemplate><%# Eval("ProdottoDescrizione") %></ItemTemplate><EditItemTemplate><asp:DropDownList ID="EditProdPersProdottoDropDown" runat="server" CssClass="input" /></EditItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="Personale"><ItemTemplate><%# Eval("PersonaleDescrizione") %></ItemTemplate><EditItemTemplate><asp:DropDownList ID="EditProdPersPersonaleDropDown" runat="server" CssClass="input" /></EditItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="Data"><ItemTemplate><%# Eval("DataAssegnazione", "{0:yyyy-MM-dd}") %></ItemTemplate><EditItemTemplate><asp:TextBox ID="EditProdPersDataText" runat="server" CssClass="input" TextMode="Date" Text='<%# Bind("DataAssegnazione", "{0:yyyy-MM-dd}") %>' /></EditItemTemplate></asp:TemplateField>
                    <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" />
                </Columns>
            </asp:GridView>
        </div>
        <div id="crud-storico" class="form-card domain-section">
            <div class="domain-title">
                <h2>ProdPersStorico</h2>
                <a href="#crud-top">Torna su</a>
            </div>
            <div class="field-grid">
                <label>Id ProdPers<asp:TextBox ID="StoricoProdPersIdText" runat="server" CssClass="input" /></label>
                <label>Prodotto<asp:DropDownList ID="StoricoProdottoDropDown" runat="server" CssClass="input" /></label>
                <label>Tipo personale
                    <asp:RadioButtonList ID="StoricoTipoPersonaleRadio" runat="server" CssClass="radio-list" RepeatDirection="Horizontal" AutoPostBack="true" EnableViewState="false" OnSelectedIndexChanged="StoricoTipoPersonaleRadio_SelectedIndexChanged">
                        <asp:ListItem Value="I" Selected="True">Interno</asp:ListItem>
                        <asp:ListItem Value="E">Esterno</asp:ListItem>
                    </asp:RadioButtonList>
                </label>
                <label class="checkbox-field"><asp:CheckBox ID="StoricoMostraNonAttiviCheck" runat="server" Text="Includi personale non attivo" AutoPostBack="true" EnableViewState="false" OnCheckedChanged="StoricoMostraNonAttiviCheck_CheckedChanged" /></label>
                <label>Personale<asp:DropDownList ID="StoricoPersonaleDropDown" runat="server" CssClass="input" EnableViewState="false" /></label>
                <label>Data assegnazione<asp:TextBox ID="StoricoDataAssText" runat="server" CssClass="input" TextMode="Date" /></label>
                <label>Data restituzione<asp:TextBox ID="StoricoDataResText" runat="server" CssClass="input" TextMode="Date" /></label>
                <label>Stanza<asp:TextBox ID="StoricoStanzaText" runat="server" CssClass="input" /></label>
                <label>Livello efficienza<asp:TextBox ID="StoricoLivelloText" runat="server" CssClass="input" /></label>
                <label>Nome macchina<asp:TextBox ID="StoricoMacchinaText" runat="server" CssClass="input" /></label>
                <label>Seriale<asp:TextBox ID="StoricoSerialeText" runat="server" CssClass="input" /></label>
                <label class="field-span-2">Note prodotto<asp:TextBox ID="StoricoNoteText" runat="server" CssClass="input" TextMode="MultiLine" Rows="2" /></label>
            </div>
            <div class="page-actions"><asp:Button ID="AddStoricoButton" runat="server" Text="Aggiungi storico assegnazione" CssClass="button primary" OnClick="AddStoricoButton_Click" /></div>
            <asp:GridView ID="StoricoGrid" runat="server" AutoGenerateColumns="False" CssClass="data-grid compact" GridLines="None" DataKeyNames="Id"
                OnRowEditing="StoricoGrid_RowEditing" OnRowCancelingEdit="StoricoGrid_RowCancelingEdit" OnRowUpdating="StoricoGrid_RowUpdating" OnRowDeleting="StoricoGrid_RowDeleting" OnRowDataBound="StoricoGrid_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="Id" HeaderText="Id" ReadOnly="True" />
                    <asp:TemplateField HeaderText="IdProdPers"><ItemTemplate><%# Eval("IdProdPers") %></ItemTemplate><EditItemTemplate><asp:TextBox ID="EditStoricoProdPersIdText" runat="server" CssClass="input" Text='<%# Bind("IdProdPers") %>' /></EditItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="Prodotto"><ItemTemplate><%# Eval("ProdottoDescrizione") %></ItemTemplate><EditItemTemplate><asp:DropDownList ID="EditStoricoProdottoDropDown" runat="server" CssClass="input" /></EditItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="Personale"><ItemTemplate><%# Eval("PersonaleDescrizione") %></ItemTemplate><EditItemTemplate><asp:DropDownList ID="EditStoricoPersonaleDropDown" runat="server" CssClass="input" /></EditItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="Data ass."><ItemTemplate><%# Eval("DataAssegnazione", "{0:yyyy-MM-dd}") %></ItemTemplate><EditItemTemplate><asp:TextBox ID="EditStoricoDataAssText" runat="server" CssClass="input" TextMode="Date" Text='<%# Bind("DataAssegnazione", "{0:yyyy-MM-dd}") %>' /></EditItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="Data rest."><ItemTemplate><%# Eval("DataRestituzione", "{0:yyyy-MM-dd}") %></ItemTemplate><EditItemTemplate><asp:TextBox ID="EditStoricoDataResText" runat="server" CssClass="input" TextMode="Date" Text='<%# Bind("DataRestituzione", "{0:yyyy-MM-dd}") %>' /></EditItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="Stanza"><ItemTemplate><%# Eval("NumeroStanza") %></ItemTemplate><EditItemTemplate><asp:TextBox ID="EditStoricoStanzaText" runat="server" CssClass="input" Text='<%# Bind("NumeroStanza") %>' /></EditItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="Livello"><ItemTemplate><%# Eval("LivelloEfficienza") %></ItemTemplate><EditItemTemplate><asp:TextBox ID="EditStoricoLivelloText" runat="server" CssClass="input" Text='<%# Bind("LivelloEfficienza") %>' /></EditItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="Macchina"><ItemTemplate><%# Eval("NomeMacchina") %></ItemTemplate><EditItemTemplate><asp:TextBox ID="EditStoricoMacchinaText" runat="server" CssClass="input" Text='<%# Bind("NomeMacchina") %>' /></EditItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="Seriale"><ItemTemplate><%# Eval("SerialNumber") %></ItemTemplate><EditItemTemplate><asp:TextBox ID="EditStoricoSerialeText" runat="server" CssClass="input" Text='<%# Bind("SerialNumber") %>' /></EditItemTemplate></asp:TemplateField>
                    <asp:TemplateField HeaderText="Note"><ItemTemplate><%# Eval("NoteProdotto") %></ItemTemplate><EditItemTemplate><asp:TextBox ID="EditStoricoNoteText" runat="server" CssClass="input" TextMode="MultiLine" Rows="2" Text='<%# Bind("NoteProdotto") %>' /></EditItemTemplate></asp:TemplateField>
                    <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" />
                </Columns>
            </asp:GridView>
        </div>
    </section>
</asp:Content>
