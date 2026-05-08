<%@ Page Title="Storico assegnazioni" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="StoricoAssegnazioni.aspx.cs" Inherits="MaterialiGestioneWeb.StoricoAssegnazioniPage" %>

<asp:Content ID="TitleStoricoAssegnazioni" ContentPlaceHolderID="TitleContent" runat="server">Storico assegnazioni</asp:Content>
<asp:Content ID="MainStoricoAssegnazioni" ContentPlaceHolderID="MainContent" runat="server">
    <section class="page-title">
        <h1>Storico assegnazioni</h1>
        <p>Consulta lo storico filtrando per categorico, prodotto e assegnatario.</p>
    </section>

    <asp:Panel ID="ErrorPanel" runat="server" CssClass="alert error" Visible="false">
        <asp:Literal ID="ErrorMessage" runat="server" />
    </asp:Panel>

    <section class="form-card">
        <div class="field-grid">
            <label>Categorico<asp:TextBox ID="CategoricoText" runat="server" CssClass="input" AutoPostBack="true" OnTextChanged="CategoricoText_TextChanged" placeholder="Inserisci categorico..." /></label>
            <label>Filtro<asp:Button ID="FiltraProdottoButton" runat="server" Text="Filtra prodotti" CssClass="button" OnClick="FiltraProdottoButton_Click" /></label>
            <label class="field-span-2">Prodotto<asp:DropDownList ID="ProdottoDropDown" runat="server" CssClass="input" /></label>
            <label>Tipo personale
                <asp:RadioButtonList ID="TipoPersonaleRadio" runat="server" CssClass="radio-list" RepeatDirection="Horizontal" AutoPostBack="true" EnableViewState="false" OnSelectedIndexChanged="TipoPersonaleRadio_SelectedIndexChanged">
                    <asp:ListItem Value="I" Selected="True">Interno</asp:ListItem>
                    <asp:ListItem Value="E">Esterno</asp:ListItem>
                </asp:RadioButtonList>
            </label>
            <label class="checkbox-field"><asp:CheckBox ID="MostraNonAttiviCheck" runat="server" Text="Includi personale non attivo" AutoPostBack="true" EnableViewState="false" OnCheckedChanged="MostraNonAttiviCheck_CheckedChanged" /></label>
            <label class="field-span-2">Assegnatario<asp:DropDownList ID="PersonaleDropDown" runat="server" CssClass="input" EnableViewState="false" /></label>
        </div>
        <div class="page-actions">
            <asp:Button ID="SearchButton" runat="server" Text="Cerca storico" CssClass="button primary" OnClick="SearchButton_Click" />
            <asp:Button ID="ResetButton" runat="server" Text="Reset" CssClass="button" OnClick="ResetButton_Click" />
            <asp:Button ID="ExportCsvButton" runat="server" Text="Esporta CSV" CssClass="button" OnClick="ExportCsvButton_Click" />
        </div>
    </section>

    <asp:Panel ID="ResultPanel" runat="server" CssClass="form-card" Visible="false">
        <div class="domain-title">
            <h2><asp:Literal ID="ResultTitle" runat="server" /></h2>
        </div>
        <asp:GridView ID="StoricoGrid" runat="server"
            AutoGenerateColumns="False"
            CssClass="data-grid"
            GridLines="None"
            EmptyDataText="Nessuna assegnazione storica trovata.">
            <Columns>
                <asp:BoundField DataField="Categorico" HeaderText="Categorico" />
                <asp:BoundField DataField="DescrizioneProdotto" HeaderText="Descrizione" />
                <asp:BoundField DataField="Matricola" HeaderText="Matricola" />
                <asp:BoundField DataField="AssegnatarioDisplay" HeaderText="Assegnatario" />
                <asp:BoundField DataField="DataAssegnazione" HeaderText="Data ass." DataFormatString="{0:yyyy-MM-dd}" />
                <asp:BoundField DataField="DataRestituzione" HeaderText="Data rest." DataFormatString="{0:yyyy-MM-dd}" />
                <asp:BoundField DataField="NumeroStanza" HeaderText="Stanza" />
                <asp:BoundField DataField="LivelloEfficienza" HeaderText="Stato" />
                <asp:BoundField DataField="NomeMacchina" HeaderText="Macchina" />
                <asp:BoundField DataField="SerialNumber" HeaderText="Seriale" />
                <asp:BoundField DataField="NoteProdotto" HeaderText="Note" />
                <asp:HyperLinkField Text="Dettaglio" DataNavigateUrlFields="IdProdotto" DataNavigateUrlFormatString="ProdottoDettaglio.aspx?id={0}" />
            </Columns>
        </asp:GridView>
    </asp:Panel>
</asp:Content>
