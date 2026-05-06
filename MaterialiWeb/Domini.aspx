<%@ Page Title="Domini" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Domini.aspx.cs" Inherits="MaterialiGestioneWeb.Domini" %>

<asp:Content ID="Title" ContentPlaceHolderID="TitleContent" runat="server">Domini</asp:Content>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <section id="domini-top" class="page-title">
        <h1>Tabelle di dominio</h1>
        <p>Domini e anagrafiche principali del database Materiali.</p>
    </section>

    <asp:Panel ID="ErrorPanel" runat="server" CssClass="alert error" Visible="false">
        <asp:Literal ID="ErrorMessage" runat="server" />
    </asp:Panel>
    <asp:Panel ID="SuccessPanel" runat="server" CssClass="alert success" Visible="false">
        <asp:Literal ID="SuccessMessage" runat="server" />
    </asp:Panel>

    <nav class="domain-jump-nav" aria-label="Accesso rapido ai domini">
        <a href="#dominio-categorie">Categorie prodotti</a>
        <a href="#dominio-livelli">Livelli efficienza</a>
        <a href="#dominio-stanze">Stanze</a>
        <a href="#dominio-ditte">Ditte</a>
        <a href="#dominio-tipi-oggetto">Tipi oggetto ordinativo</a>
    </nav>

    <section class="split-grid">
        <div id="dominio-categorie" class="domain-section">
            <div class="domain-title">
                <h2>Categorie prodotti</h2>
                <a href="#domini-top">Torna su</a>
            </div>
            <div class="form-card">
                <div class="field-grid">
                    <label>Descrizione<asp:TextBox ID="CategoriaNomeText" runat="server" CssClass="input" /></label>
                    <label>Tipo oggetto<asp:DropDownList ID="CategoriaTipoDropDown" runat="server" CssClass="input" /></label>
                </div>
                <div class="page-actions">
                    <asp:Button ID="AddCategoriaButton" runat="server" Text="Aggiungi categoria" CssClass="button primary" OnClick="AddCategoriaButton_Click" />
                </div>
            </div>
            <asp:GridView ID="TipologieGrid" runat="server" AutoGenerateColumns="False" CssClass="data-grid compact" GridLines="None" DataKeyNames="Id"
                OnRowEditing="TipologieGrid_RowEditing" OnRowCancelingEdit="TipologieGrid_RowCancelingEdit" OnRowUpdating="TipologieGrid_RowUpdating"
                OnRowDeleting="TipologieGrid_RowDeleting" OnRowDataBound="TipologieGrid_RowDataBound">
                <Columns>
                    <asp:TemplateField HeaderText="Nome">
                        <ItemTemplate><%# Eval("Nome") %></ItemTemplate>
                        <EditItemTemplate><asp:TextBox ID="EditCategoriaNomeText" runat="server" CssClass="input" Text='<%# Bind("Nome") %>' /></EditItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Tipo oggetto">
                        <ItemTemplate><%# Eval("Extra") %></ItemTemplate>
                        <EditItemTemplate><asp:DropDownList ID="EditCategoriaTipoDropDown" runat="server" CssClass="input" /></EditItemTemplate>
                    </asp:TemplateField>
                    <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" />
                </Columns>
            </asp:GridView>
        </div>
        <div id="dominio-livelli" class="domain-section">
            <div class="domain-title">
                <h2>Livelli efficienza</h2>
                <a href="#domini-top">Torna su</a>
            </div>
            <div class="form-card">
                <div class="field-grid">
                    <label>Codice<asp:TextBox ID="LivelloCodiceText" runat="server" CssClass="input" /></label>
                    <label>Descrizione<asp:TextBox ID="LivelloNomeText" runat="server" CssClass="input" /></label>
                </div>
                <div class="page-actions">
                    <asp:Button ID="AddLivelloButton" runat="server" Text="Aggiungi livello" CssClass="button primary" OnClick="AddLivelloButton_Click" />
                </div>
            </div>
            <asp:GridView ID="LivelliGrid" runat="server" AutoGenerateColumns="False" CssClass="data-grid compact" GridLines="None" DataKeyNames="Id"
                OnRowEditing="LivelliGrid_RowEditing" OnRowCancelingEdit="LivelliGrid_RowCancelingEdit" OnRowUpdating="LivelliGrid_RowUpdating"
                OnRowDeleting="LivelliGrid_RowDeleting">
                <Columns>
                    <asp:BoundField DataField="Id" HeaderText="Id" ReadOnly="True" />
                    <asp:TemplateField HeaderText="Codice">
                        <ItemTemplate><%# Eval("Codice") %></ItemTemplate>
                        <EditItemTemplate><asp:TextBox ID="EditLivelloCodiceText" runat="server" CssClass="input" Text='<%# Bind("Codice") %>' /></EditItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Nome">
                        <ItemTemplate><%# Eval("Nome") %></ItemTemplate>
                        <EditItemTemplate><asp:TextBox ID="EditLivelloNomeText" runat="server" CssClass="input" Text='<%# Bind("Nome") %>' /></EditItemTemplate>
                    </asp:TemplateField>
                    <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" />
                </Columns>
            </asp:GridView>
        </div>
        <div id="dominio-stanze" class="domain-section">
            <div class="domain-title">
                <h2>Stanze</h2>
                <a href="#domini-top">Torna su</a>
            </div>
            <div class="form-card">
                <div class="field-grid">
                    <label>Numero stanza<asp:TextBox ID="StanzaNumeroText" runat="server" CssClass="input" /></label>
                </div>
                <div class="page-actions">
                    <asp:Button ID="AddStanzaButton" runat="server" Text="Aggiungi stanza" CssClass="button primary" OnClick="AddStanzaButton_Click" />
                </div>
            </div>
            <asp:GridView ID="StanzeGrid" runat="server" AutoGenerateColumns="False" CssClass="data-grid compact" GridLines="None" DataKeyNames="Id"
                OnRowEditing="StanzeGrid_RowEditing" OnRowCancelingEdit="StanzeGrid_RowCancelingEdit" OnRowUpdating="StanzeGrid_RowUpdating"
                OnRowDeleting="StanzeGrid_RowDeleting">
                <Columns>
                    <asp:BoundField DataField="Id" HeaderText="Id" ReadOnly="True" />
                    <asp:TemplateField HeaderText="Numero">
                        <ItemTemplate><%# Eval("Nome") %></ItemTemplate>
                        <EditItemTemplate><asp:TextBox ID="EditStanzaNomeText" runat="server" CssClass="input" Text='<%# Bind("Nome") %>' /></EditItemTemplate>
                    </asp:TemplateField>
                    <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" />
                </Columns>
            </asp:GridView>
        </div>
        <div id="dominio-ditte" class="domain-section">
            <div class="domain-title">
                <h2>Ditte</h2>
                <a href="#domini-top">Torna su</a>
            </div>
            <div class="form-card">
                <div class="field-grid">
                    <label>Nome<asp:TextBox ID="DittaNomeText" runat="server" CssClass="input" /></label>
                    <label>Citta<asp:TextBox ID="DittaCittaText" runat="server" CssClass="input" /></label>
                    <label>Mail<asp:TextBox ID="DittaMailText" runat="server" CssClass="input" /></label>
                    <label>Tipologia<asp:TextBox ID="DittaTipologiaText" runat="server" CssClass="input" MaxLength="1" /></label>
                </div>
                <div class="page-actions">
                    <asp:Button ID="AddDittaButton" runat="server" Text="Aggiungi ditta" CssClass="button primary" OnClick="AddDittaButton_Click" />
                </div>
            </div>
            <asp:GridView ID="DitteGrid" runat="server" AutoGenerateColumns="False" CssClass="data-grid compact" GridLines="None" DataKeyNames="Id"
                OnRowEditing="DitteGrid_RowEditing" OnRowCancelingEdit="DitteGrid_RowCancelingEdit" OnRowUpdating="DitteGrid_RowUpdating"
                OnRowDeleting="DitteGrid_RowDeleting">
                <Columns>
                    <asp:BoundField DataField="Id" HeaderText="Id" ReadOnly="True" />
                    <asp:TemplateField HeaderText="Nome">
                        <ItemTemplate><%# Eval("Nome") %></ItemTemplate>
                        <EditItemTemplate><asp:TextBox ID="EditDittaNomeText" runat="server" CssClass="input" Text='<%# Bind("Nome") %>' /></EditItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Citta">
                        <ItemTemplate><%# Eval("Extra") %></ItemTemplate>
                        <EditItemTemplate><asp:TextBox ID="EditDittaCittaText" runat="server" CssClass="input" Text='<%# Bind("Extra") %>' /></EditItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Mail">
                        <ItemTemplate><%# Eval("Extra2") %></ItemTemplate>
                        <EditItemTemplate><asp:TextBox ID="EditDittaMailText" runat="server" CssClass="input" Text='<%# Bind("Extra2") %>' /></EditItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Tipologia">
                        <ItemTemplate><%# Eval("Codice") %></ItemTemplate>
                        <EditItemTemplate><asp:TextBox ID="EditDittaTipologiaText" runat="server" CssClass="input" Text='<%# Bind("Codice") %>' MaxLength="1" /></EditItemTemplate>
                    </asp:TemplateField>
                    <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" />
                </Columns>
            </asp:GridView>
        </div>
        <div id="dominio-tipi-oggetto" class="field-span-2 domain-section">
            <div class="domain-title">
                <h2>Tipi oggetto ordinativo</h2>
                <a href="#domini-top">Torna su</a>
            </div>
            <div class="form-card">
                <div class="field-grid">
                    <label>Id tipo<asp:TextBox ID="TipoOggettoIdText" runat="server" CssClass="input" /></label>
                    <label>Descrizione<asp:TextBox ID="TipoOggettoNomeText" runat="server" CssClass="input" /></label>
                </div>
                <div class="page-actions">
                    <asp:Button ID="AddTipoOggettoButton" runat="server" Text="Aggiungi tipo oggetto" CssClass="button primary" OnClick="AddTipoOggettoButton_Click" />
                </div>
            </div>
            <asp:GridView ID="TipiOggettoGrid" runat="server" AutoGenerateColumns="False" CssClass="data-grid compact" GridLines="None" DataKeyNames="Id"
                OnRowEditing="TipiOggettoGrid_RowEditing" OnRowCancelingEdit="TipiOggettoGrid_RowCancelingEdit" OnRowUpdating="TipiOggettoGrid_RowUpdating"
                OnRowDeleting="TipiOggettoGrid_RowDeleting">
                <Columns>
                    <asp:BoundField DataField="Id" HeaderText="Id" ReadOnly="True" />
                    <asp:TemplateField HeaderText="Descrizione">
                        <ItemTemplate><%# Eval("Nome") %></ItemTemplate>
                        <EditItemTemplate><asp:TextBox ID="EditTipoOggettoNomeText" runat="server" CssClass="input" Text='<%# Bind("Nome") %>' /></EditItemTemplate>
                    </asp:TemplateField>
                    <asp:CommandField ShowEditButton="True" ShowDeleteButton="True" />
                </Columns>
            </asp:GridView>
        </div>
    </section>
</asp:Content>
