<%@ Page Title="Prodotti" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Prodotti.aspx.cs" Inherits="MaterialiGestioneWeb.Prodotti" %>

<asp:Content ID="Title" ContentPlaceHolderID="TitleContent" runat="server">Prodotti</asp:Content>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <section class="page-title">
        <h1>Prodotti</h1>
        <p>Elenco del materiale censito con stato corrente, stanza, assegnatario e riferimenti tecnici.</p>
    </section>

    <asp:Panel ID="ErrorPanel" runat="server" CssClass="alert error" Visible="false">
        <asp:Literal ID="ErrorMessage" runat="server" />
    </asp:Panel>

    <div class="toolbar">
        <asp:TextBox ID="SearchText" runat="server" CssClass="input" placeholder="Cerca per categorico, matricola, descrizione, modello, macchina o MAC..." />
        <asp:Button ID="SearchButton" runat="server" Text="Cerca" CssClass="button primary" OnClick="SearchButton_Click" />
        <asp:Button ID="ResetButton" runat="server" Text="Reset" CssClass="button" OnClick="ResetButton_Click" />
    </div>

    <asp:GridView ID="ProdottiGrid" runat="server"
        AutoGenerateColumns="False"
        CssClass="data-grid"
        GridLines="None"
        EmptyDataText="Nessun prodotto trovato.">
        <Columns>
            <asp:BoundField DataField="Categorico" HeaderText="Categorico" />
            <asp:BoundField DataField="DescrizioneProdotto" HeaderText="Descrizione" />
            <asp:BoundField DataField="Matricola" HeaderText="Matricola" />
            <asp:BoundField DataField="Categoria" HeaderText="Categoria" />
            <asp:BoundField DataField="Modello" HeaderText="Modello" />
            <asp:BoundField DataField="LivelloEfficienza" HeaderText="Stato" />
            <asp:BoundField DataField="AssegnatarioDisplay" HeaderText="Assegnatario" />
            <asp:BoundField DataField="NumeroStanza" HeaderText="Stanza" />
            <asp:HyperLinkField Text="Dettaglio" DataNavigateUrlFields="IdProdotto" DataNavigateUrlFormatString="ProdottoDettaglio.aspx?id={0}" />
        </Columns>
    </asp:GridView>
</asp:Content>
