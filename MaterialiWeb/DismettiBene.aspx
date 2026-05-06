<%@ Page Title="Dismetti bene" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DismettiBene.aspx.cs" Inherits="MaterialiGestioneWeb.DismettiBenePage" %>

<asp:Content ID="Title7" ContentPlaceHolderID="TitleContent" runat="server">Dismetti bene</asp:Content>
<asp:Content ID="Main7" ContentPlaceHolderID="MainContent" runat="server">
    <section class="page-title"><h1>Versamento o dismissione</h1><p>Chiude l'assegnazione attiva se richiesto, aggiorna il versamento e può cambiare il livello di efficienza.</p></section>
    <asp:Panel ID="ErrorPanel" runat="server" CssClass="alert error" Visible="false"><asp:Literal ID="ErrorMessage" runat="server" /></asp:Panel>
    <asp:Panel ID="SuccessPanel" runat="server" CssClass="alert success" Visible="false"><asp:Literal ID="SuccessMessage" runat="server" /></asp:Panel>
    <section class="form-card">
        <div class="field-grid">
            <label class="field-span-2">Prodotto<asp:DropDownList ID="ProdottoDropDown" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ProdottoDropDown_SelectedIndexChanged" /></label>
            <label class="checkbox-field"><asp:CheckBox ID="ChiudiAssegnazioneCheck" runat="server" Text="Chiudi eventuale assegnazione attiva" Checked="true" /></label>
            <label>Nuovo livello efficienza<asp:DropDownList ID="UbicazioneDropDown" runat="server" CssClass="input" /></label>
            <label class="field-span-2">Note<asp:TextBox ID="NoteText" runat="server" CssClass="input" TextMode="MultiLine" Rows="3" /></label>
        </div>
        <asp:Panel ID="ProdottoContextPanel" runat="server" CssClass="detail-card" Visible="false">
            <h2>Contesto prodotto</h2>
            <dl class="detail-list">
                <dt>Descrizione</dt><dd><asp:Literal ID="ProdottoDescrizione" runat="server" /></dd>
                <dt>Stato corrente</dt><dd><asp:Literal ID="ProdottoStato" runat="server" /></dd>
                <dt>Assegnatario corrente</dt><dd><asp:Literal ID="ProdottoAssegnatario" runat="server" /></dd>
                <dt>Stanza corrente</dt><dd><asp:Literal ID="ProdottoStanza" runat="server" /></dd>
                <dt>Versamento corrente</dt><dd><asp:Literal ID="ProdottoVersamento" runat="server" /></dd>
            </dl>
        </asp:Panel>
    </section>
    <section class="form-card">
        <h2>Riferimento versamento</h2>
        <div class="field-grid">
            <label class="field-span-2">Valore da salvare in `Versamento`<asp:TextBox ID="DocumentoNomeFileText" runat="server" CssClass="input" /></label>
        </div>
    </section>
    <div class="page-actions"><asp:Button ID="SaveButton" runat="server" Text="Conferma operazione" CssClass="button primary danger" OnClick="SaveButton_Click" /></div>
</asp:Content>
