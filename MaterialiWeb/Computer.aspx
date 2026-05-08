<%@ Page Title="Rete e postazioni" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Computer.aspx.cs" Inherits="MaterialiGestioneWeb.Computer" %>

<asp:Content ID="Title" ContentPlaceHolderID="TitleContent" runat="server">Rete e postazioni</asp:Content>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <section class="page-title">
        <h1>Rete e postazioni</h1>
        <p>Materiali con MAC address, nome macchina o categoria marcata come ethernet.</p>
    </section>

    <asp:Panel ID="ErrorPanel" runat="server" CssClass="alert error" Visible="false">
        <asp:Literal ID="ErrorMessage" runat="server" />
    </asp:Panel>

    <div class="toolbar">
        <asp:TextBox ID="SearchText" runat="server" CssClass="input" placeholder="Cerca per macchina, matricola, descrizione o MAC..." />
        <asp:Button ID="SearchButton" runat="server" Text="Cerca" CssClass="button primary" OnClick="SearchButton_Click" />
        <asp:Button ID="ResetButton" runat="server" Text="Reset" CssClass="button" OnClick="ResetButton_Click" />
        <asp:Button ID="ExportCsvButton" runat="server" Text="Esporta CSV" CssClass="button" OnClick="ExportCsvButton_Click" />
    </div>

    <asp:GridView ID="ComputerGrid" runat="server"
        AutoGenerateColumns="False"
        CssClass="data-grid"
        GridLines="None"
        EmptyDataText="Nessun materiale di rete trovato.">
        <Columns>
            <asp:BoundField DataField="Categorico" HeaderText="Categorico" />
            <asp:BoundField DataField="NomeMacchina" HeaderText="Nome macchina" />
            <asp:BoundField DataField="DescrizioneProdotto" HeaderText="Descrizione" />
            <asp:BoundField DataField="Matricola" HeaderText="Matricola" />
            <asp:BoundField DataField="Categoria" HeaderText="Categoria" />
            <asp:BoundField DataField="Modello" HeaderText="Modello" />
            <asp:BoundField DataField="MacAddress" HeaderText="MAC" />
            <asp:BoundField DataField="NumeroStanza" HeaderText="Stanza" />
            <asp:BoundField DataField="LivelloEfficienza" HeaderText="Stato" />
            <asp:HyperLinkField Text="Dettaglio" DataNavigateUrlFields="IdProdotto" DataNavigateUrlFormatString="ProdottoDettaglio.aspx?id={0}" />
        </Columns>
    </asp:GridView>
</asp:Content>
