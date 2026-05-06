<%@ Page Title="Cambia stato" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CambiaStato.aspx.cs" Inherits="MaterialiGestioneWeb.CambiaStatoPage" %>

<asp:Content ID="Title4" ContentPlaceHolderID="TitleContent" runat="server">Cambia stato</asp:Content>
<asp:Content ID="Main4" ContentPlaceHolderID="MainContent" runat="server">
    <section class="page-title"><h1>Cambia stato</h1><p>Aggiorna il livello di efficienza corrente del materiale.</p></section>
    <asp:Panel ID="ErrorPanel" runat="server" CssClass="alert error" Visible="false"><asp:Literal ID="ErrorMessage" runat="server" /></asp:Panel>
    <asp:Panel ID="SuccessPanel" runat="server" CssClass="alert success" Visible="false"><asp:Literal ID="SuccessMessage" runat="server" /></asp:Panel>
    <section class="form-card">
        <div class="field-grid">
            <label class="field-span-2">Prodotto<asp:DropDownList ID="ProdottoDropDown" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ProdottoDropDown_SelectedIndexChanged" /></label>
            <label>Nuovo stato<asp:DropDownList ID="StatoDropDown" runat="server" CssClass="input" /></label>
            <label class="field-span-2">Note<asp:TextBox ID="NoteText" runat="server" CssClass="input" TextMode="MultiLine" Rows="3" /></label>
        </div>
        <asp:Panel ID="ProdottoContextPanel" runat="server" CssClass="detail-card" Visible="false">
            <h2>Contesto prodotto</h2>
            <dl class="detail-list">
                <dt>Descrizione</dt><dd><asp:Literal ID="ProdottoDescrizione" runat="server" /></dd>
                <dt>Stato corrente</dt><dd><asp:Literal ID="ProdottoStato" runat="server" /></dd>
                <dt>Assegnatario corrente</dt><dd><asp:Literal ID="ProdottoAssegnatario" runat="server" /></dd>
                <dt>Stanza corrente</dt><dd><asp:Literal ID="ProdottoStanza" runat="server" /></dd>
            </dl>
        </asp:Panel>
    </section>
    <div class="page-actions"><asp:Button ID="SaveButton" runat="server" Text="Registra stato" CssClass="button primary" OnClick="SaveButton_Click" /></div>
</asp:Content>
