<%@ Page Title="CRUD amministrativi" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="GestioneCrud.aspx.cs" Inherits="MaterialiGestioneWeb.GestioneCrudPage" %>

<asp:Content ID="TitleCrud" ContentPlaceHolderID="TitleContent" runat="server">CRUD amministrativi</asp:Content>
<asp:Content ID="MainCrud" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Literal ID="ContextCssLiteral" runat="server" />

    <section id="crud-top" class="page-title">
        <div>
            <h1><asp:Literal ID="PageHeadingText" runat="server" /></h1>
            <p><asp:Literal ID="PageIntroText" runat="server" /></p>
        </div>
        <asp:Panel ID="ContextActionsPanel" runat="server" CssClass="hero-actions" Visible="false">
            <asp:HyperLink ID="BackToDettaglioLink" runat="server" CssClass="button primary">Torna al dettaglio ordinativo</asp:HyperLink>
            <asp:HyperLink ID="FullCrudLink" runat="server" CssClass="button" NavigateUrl="GestioneCrud.aspx">Apri CRUD completo</asp:HyperLink>
        </asp:Panel>
    </section>

    <asp:Panel ID="ErrorPanel" runat="server" CssClass="alert error" Visible="false"><asp:Literal ID="ErrorMessage" runat="server" /></asp:Panel>
    <asp:Panel ID="SuccessPanel" runat="server" CssClass="alert success" Visible="false"><asp:Literal ID="SuccessMessage" runat="server" /></asp:Panel>

    <nav class="domain-jump-nav" data-crud-navigation="true" aria-label="Accesso rapido ai CRUD amministrativi">
        <a href="#crud-prodotti">Prodotti</a>
        <a href="#crud-ordinativi">Ordinativi</a>
        <a href="#crud-oggetti">Oggetti ordinativo</a>
    </nav>

    <section id="crud-prodotti" class="form-card domain-section">
        <div class="domain-title">
            <h2>Prodotti</h2>
            <a href="#crud-top">Torna su</a>
        </div>
        <p class="muted-text">I prodotti vengono generati automaticamente dalla quantita indicata negli oggetti ordinativo. Usa questa sezione solo per correggere o manutenere prodotti gia generati.</p>
        <div class="page-actions">
            <a class="button primary" href="#crud-oggetti">Aggiungi oggetto ordinativo</a>
            <a class="button" href="NuovoBene.aspx">Completa prodotti generati</a>
        </div>
        <div class="crud-list-heading">
            <h3>Lista prodotti generati</h3>
        </div>
        <asp:GridView ID="ProdottiAdminGrid" runat="server" AutoGenerateColumns="False" CssClass="data-grid compact" GridLines="None" DataKeyNames="IdProdotto"
            OnRowEditing="ProdottiAdminGrid_RowEditing" OnRowCancelingEdit="ProdottiAdminGrid_RowCancelingEdit" OnRowUpdating="ProdottiAdminGrid_RowUpdating" OnRowDeleting="ProdottiAdminGrid_RowDeleting" OnRowDataBound="ProdottiAdminGrid_RowDataBound">
            <Columns>
                <asp:BoundField DataField="IdProdotto" HeaderText="Id" ReadOnly="True" />
                <asp:TemplateField HeaderText="Categorico"><ItemTemplate><%# Eval("Categorico") %></ItemTemplate><EditItemTemplate><asp:TextBox ID="EditProdottoCategoricoText" runat="server" CssClass="input" ReadOnly="true" Text='<%# Bind("Categorico") %>' /></EditItemTemplate></asp:TemplateField>
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
        <div class="crud-entry-panel">
            <h3>Inserimento</h3>
            <div class="field-grid">
                <label>Codice<asp:TextBox ID="OrdinativoCodiceText" runat="server" CssClass="input" ReadOnly="true" /></label>
                <label>Denominazione<asp:TextBox ID="OrdinativoDenominazioneText" runat="server" CssClass="input" /></label>
            </div>
            <div class="page-actions"><asp:Button ID="AddOrdinativoButton" runat="server" Text="Aggiungi ordinativo" CssClass="button primary" OnClick="AddOrdinativoButton_Click" /></div>
        </div>
        <div class="crud-list-heading">
            <h3>Lista ordinativi</h3>
        </div>
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
        <p class="muted-text">Se arrivi dal dettaglio di un ordinativo, il campo Ordinativo viene preselezionato e la griglia mostra solo le righe di quell'ordinativo. La quantita inserita genera automaticamente i prodotti collegati.</p>
        <div class="crud-entry-panel">
            <h3>Inserimento</h3>
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
        </div>
        <div class="crud-list-heading">
            <h3>Lista oggetti ordinativo</h3>
        </div>
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

</asp:Content>
