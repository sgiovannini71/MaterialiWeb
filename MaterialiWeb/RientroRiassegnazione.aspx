<%@ Page Title="Rientro o riassegnazione" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" EnableEventValidation="false" CodeBehind="RientroRiassegnazione.aspx.cs" Inherits="MaterialiGestioneWeb.RientroRiassegnazionePage" %>

<asp:Content ID="Title3" ContentPlaceHolderID="TitleContent" runat="server">Rientro o riassegnazione</asp:Content>
<asp:Content ID="Main3" ContentPlaceHolderID="MainContent" runat="server">
    <section class="page-title"><h1>Rientro o riassegnazione</h1><p>Opera solo su beni attualmente assegnati: puoi chiudere l'assegnazione corrente oppure trasferire il bene a un nuovo assegnatario.</p></section>
    <asp:Panel ID="ErrorPanel" runat="server" CssClass="alert error" Visible="false"><asp:Literal ID="ErrorMessage" runat="server" /></asp:Panel>
    <asp:Panel ID="SuccessPanel" runat="server" CssClass="alert success" Visible="false"><asp:Literal ID="SuccessMessage" runat="server" /></asp:Panel>
    <section class="form-card">
        <div class="field-grid">
            <label>Categorico<asp:TextBox ID="CategoricoText" runat="server" CssClass="input" AutoPostBack="true" OnTextChanged="CategoricoText_TextChanged" placeholder="Inserisci categorico..." /></label>
            <label>Filtro<asp:Button ID="FiltraProdottoButton" runat="server" Text="Filtra" CssClass="button" OnClick="FiltraProdottoButton_Click" /></label>
            <label>Prodotto assegnato<asp:DropDownList ID="ProdottoDropDown" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ProdottoDropDown_SelectedIndexChanged" /></label>
            <label>Data operazione<asp:TextBox ID="DataOperazioneText" runat="server" CssClass="input" TextMode="Date" /></label>
            <label>Operazione
                <asp:RadioButtonList ID="TipoOperazioneRadio" runat="server" CssClass="radio-list" RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged="TipoOperazioneRadio_SelectedIndexChanged">
                    <asp:ListItem Value="R" Selected="True">Rientro</asp:ListItem>
                    <asp:ListItem Value="T">Riassegnazione</asp:ListItem>
                </asp:RadioButtonList>
            </label>
            <label>Tipo personale
                <asp:RadioButtonList ID="TipoPersonaleRadio" runat="server" CssClass="radio-list" RepeatDirection="Horizontal" AutoPostBack="true" EnableViewState="false" OnSelectedIndexChanged="TipoPersonaleRadio_SelectedIndexChanged">
                    <asp:ListItem Value="I" Selected="True">Interno</asp:ListItem>
                    <asp:ListItem Value="E">Esterno</asp:ListItem>
                </asp:RadioButtonList>
            </label>
            <label class="checkbox-field"><asp:CheckBox ID="MostraNonAttiviCheck" runat="server" Text="Includi personale non attivo" AutoPostBack="true" EnableViewState="false" OnCheckedChanged="MostraNonAttiviCheck_CheckedChanged" /></label>
            <label>Nuovo assegnatario<asp:DropDownList ID="PersonaleDropDown" runat="server" CssClass="input" EnableViewState="false" /></label>
            <label class="field-span-2">Note<asp:TextBox ID="NoteText" runat="server" CssClass="input" TextMode="MultiLine" Rows="3" /></label>
        </div>
        <asp:Panel ID="ProdottoContextPanel" runat="server" CssClass="detail-card" Visible="false">
            <h2>Contesto prodotto</h2>
            <dl class="detail-list">
                <dt>Descrizione</dt><dd><asp:Literal ID="ProdottoDescrizione" runat="server" /></dd>
                <dt>Stato corrente</dt><dd><asp:Literal ID="ProdottoStato" runat="server" /></dd>
                <dt>Assegnatario corrente</dt><dd><asp:Literal ID="ProdottoAssegnatario" runat="server" /></dd>
                <dt>Stanza corrente</dt><dd><asp:Literal ID="ProdottoStanza" runat="server" /></dd>
                <dt>Nome macchina</dt><dd><asp:Literal ID="ProdottoNomeMacchina" runat="server" /></dd>
                <dt>MAC address</dt><dd><asp:Literal ID="ProdottoMacAddress" runat="server" /></dd>
            </dl>
        </asp:Panel>
    </section>
    <div class="page-actions">
        <asp:Button ID="SaveButton" runat="server" Text="Registra operazione" CssClass="button primary" OnClick="SaveButton_Click" />
        <asp:Button ID="PrintReturnSheetButton" runat="server" Text="Stampa scheda restituzione" CssClass="button" OnClick="PrintReturnSheetButton_Click" Enabled="false" />
    </div>
</asp:Content>
