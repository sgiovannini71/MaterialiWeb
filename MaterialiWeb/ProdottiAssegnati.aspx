<%@ Page Title="Prodotti per assegnatario" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="ProdottiAssegnati.aspx.cs" Inherits="MaterialiGestioneWeb.ProdottiAssegnatiPage" %>

<asp:Content ID="TitleAssegnati" ContentPlaceHolderID="TitleContent" runat="server">Prodotti per assegnatario</asp:Content>
<asp:Content ID="MainAssegnati" ContentPlaceHolderID="MainContent" runat="server">
    <section class="page-title">
        <h1>Prodotti per assegnatario</h1>
        <p>Seleziona una persona e visualizza il materiale attualmente assegnato.</p>
    </section>

    <asp:Panel ID="ErrorPanel" runat="server" CssClass="alert error" Visible="false">
        <asp:Literal ID="ErrorMessage" runat="server" />
    </asp:Panel>

    <section class="form-card">
        <div class="field-grid">
            <label>Tipo personale
                <asp:RadioButtonList ID="TipoPersonaleRadio" runat="server" CssClass="radio-list" RepeatDirection="Horizontal" AutoPostBack="true" EnableViewState="false" OnSelectedIndexChanged="TipoPersonaleRadio_SelectedIndexChanged">
                    <asp:ListItem Value="I" Selected="True">Interno</asp:ListItem>
                    <asp:ListItem Value="E">Esterno</asp:ListItem>
                </asp:RadioButtonList>
            </label>
            <label class="checkbox-field"><asp:CheckBox ID="MostraNonAttiviCheck" runat="server" Text="Includi personale non attivo" AutoPostBack="true" EnableViewState="false" OnCheckedChanged="MostraNonAttiviCheck_CheckedChanged" /></label>
            <label class="field-span-2">Personale<asp:DropDownList ID="PersonaleDropDown" runat="server" CssClass="input" EnableViewState="false" /></label>
        </div>
        <div class="page-actions">
            <asp:Button ID="SearchButton" runat="server" Text="Visualizza prodotti" CssClass="button primary" OnClick="SearchButton_Click" />
            <asp:Button ID="ResetButton" runat="server" Text="Reset" CssClass="button" OnClick="ResetButton_Click" />
            <asp:Button ID="PreviewSheetButton" runat="server" Text="Anteprima scheda" CssClass="button" OnClick="PreviewSheetButton_Click" />
            <asp:Button ID="ExportCsvButton" runat="server" Text="Esporta CSV" CssClass="button" OnClick="ExportCsvButton_Click" />
            <asp:Button ID="ExportPdfButton" runat="server" Text="Esporta scheda PDF" CssClass="button" OnClick="ExportPdfButton_Click" />
        </div>
    </section>

    <asp:Panel ID="ResultPanel" runat="server" CssClass="form-card" Visible="false">
        <div class="domain-title">
            <h2><asp:Literal ID="ResultTitle" runat="server" /></h2>
        </div>
        <asp:GridView ID="ProdottiGrid" runat="server"
            AutoGenerateColumns="False"
            CssClass="data-grid"
            GridLines="None"
            EmptyDataText="Nessun prodotto assegnato.">
            <Columns>
                <asp:BoundField DataField="Categorico" HeaderText="Categorico" />
                <asp:BoundField DataField="DescrizioneProdotto" HeaderText="Descrizione" />
                <asp:BoundField DataField="Matricola" HeaderText="Matricola" />
                <asp:BoundField DataField="Categoria" HeaderText="Categoria" />
                <asp:BoundField DataField="Modello" HeaderText="Modello" />
                <asp:BoundField DataField="LivelloEfficienza" HeaderText="Stato" />
                <asp:BoundField DataField="NumeroStanza" HeaderText="Stanza" />
                <asp:BoundField DataField="NomeMacchina" HeaderText="Nome macchina" />
                <asp:BoundField DataField="MacAddress" HeaderText="MAC" />
                <asp:HyperLinkField Text="Dettaglio" DataNavigateUrlFields="IdProdotto" DataNavigateUrlFormatString="ProdottoDettaglio.aspx?id={0}" />
            </Columns>
        </asp:GridView>
    </asp:Panel>

    <asp:Panel ID="PreviewPanel" runat="server" CssClass="form-card" Visible="false">
        <div class="domain-title">
            <h2>Anteprima scheda A4</h2>
        </div>
        <asp:Literal ID="PreviewHtmlLiteral" runat="server" />
    </asp:Panel>
</asp:Content>
