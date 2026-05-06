<%@ Page Title="Assegna bene" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AssegnaBene.aspx.cs" Inherits="MaterialiGestioneWeb.AssegnaBenePage" %>

<asp:Content ID="Title2" ContentPlaceHolderID="TitleContent" runat="server">Assegna bene</asp:Content>
<asp:Content ID="Main2" ContentPlaceHolderID="MainContent" runat="server">
    <section class="page-title"><h1>Assegna bene</h1><p>Crea l'assegnazione corrente in `ProdPers` e registra lo snapshot iniziale in `ProdPersStorico`.</p></section>
    <asp:Panel ID="ErrorPanel" runat="server" CssClass="alert error" Visible="false"><asp:Literal ID="ErrorMessage" runat="server" /></asp:Panel>
    <asp:Panel ID="SuccessPanel" runat="server" CssClass="alert success" Visible="false"><asp:Literal ID="SuccessMessage" runat="server" /></asp:Panel>
    <section class="form-card">
        <div class="field-grid">
            <label class="field-span-2">Prodotto<asp:DropDownList ID="ProdottoDropDown" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ProdottoDropDown_SelectedIndexChanged" /></label>
            <label>Tipo personale
                <asp:RadioButtonList ID="TipoPersonaleRadio" runat="server" CssClass="radio-list" RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged="TipoPersonaleRadio_SelectedIndexChanged">
                    <asp:ListItem Value="I" Selected="True">Interno</asp:ListItem>
                    <asp:ListItem Value="E">Esterno</asp:ListItem>
                </asp:RadioButtonList>
            </label>
            <label class="checkbox-field"><asp:CheckBox ID="MostraNonAttiviCheck" runat="server" Text="Includi personale non attivo" AutoPostBack="true" OnCheckedChanged="MostraNonAttiviCheck_CheckedChanged" /></label>
            <label>Assegnatario<asp:DropDownList ID="PersonaleDropDown" runat="server" CssClass="input" /></label>
            <label>Data assegnazione<asp:TextBox ID="DataInizioText" runat="server" CssClass="input" TextMode="Date" /></label>
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
    <div class="page-actions"><asp:Button ID="SaveButton" runat="server" Text="Conferma assegnazione" CssClass="button primary" OnClick="SaveButton_Click" /></div>
</asp:Content>
