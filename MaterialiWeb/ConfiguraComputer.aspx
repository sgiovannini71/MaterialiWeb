<%@ Page Title="Configura rete e postazione" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ConfiguraComputer.aspx.cs" Inherits="MaterialiGestioneWeb.ConfiguraComputerPage" %>

<asp:Content ID="Title6" ContentPlaceHolderID="TitleContent" runat="server">Configura rete e postazione</asp:Content>
<asp:Content ID="Main6" ContentPlaceHolderID="MainContent" runat="server">
    <section class="page-title"><h1>Aggiorna rete e postazione</h1><p>Gestisce MAC address in `NetworkData` e nome macchina in `Postazione/NomeMacchina`.</p></section>
    <asp:Panel ID="ErrorPanel" runat="server" CssClass="alert error" Visible="false"><asp:Literal ID="ErrorMessage" runat="server" /></asp:Panel>
    <asp:Panel ID="SuccessPanel" runat="server" CssClass="alert success" Visible="false"><asp:Literal ID="SuccessMessage" runat="server" /></asp:Panel>
    <section class="form-card">
        <div class="field-grid">
            <label class="field-span-2">Materiale<asp:DropDownList ID="ProdottoDropDown" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ProdottoDropDown_SelectedIndexChanged" /></label>
            <label>Nome macchina<asp:TextBox ID="HostNameText" runat="server" CssClass="input" /></label>
            <label>MAC address<asp:TextBox ID="MacAddressText" runat="server" CssClass="input" /></label>
            <label class="field-span-2">Note rete/postazione<asp:TextBox ID="TipoRamText" runat="server" CssClass="input" TextMode="MultiLine" Rows="3" /></label>
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
    <div class="page-actions"><asp:Button ID="SaveButton" runat="server" Text="Salva configurazione" CssClass="button primary" OnClick="SaveButton_Click" /></div>
</asp:Content>
