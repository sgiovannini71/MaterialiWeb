<%@ Page Title="Nuovo bene" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="NuovoBene.aspx.cs" Inherits="MaterialiGestioneWeb.NuovoBenePage" %>

<asp:Content ID="Title" ContentPlaceHolderID="TitleContent" runat="server">Nuovo bene</asp:Content>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <section class="page-title">
        <h1>Nuovo bene</h1>
        <p>Registrazione del materiale su `Prodotti` con eventuale legame a un oggetto ordinativo esistente o con dati minimi per crearne uno nuovo.</p>
        <p>Il categorico viene proposto automaticamente prendendo il prossimo valore disponibile.</p>
    </section>

    <asp:Panel ID="ErrorPanel" runat="server" CssClass="alert error" Visible="false"><asp:Literal ID="ErrorMessage" runat="server" /></asp:Panel>
    <asp:Panel ID="SuccessPanel" runat="server" CssClass="alert success" Visible="false"><asp:Literal ID="SuccessMessage" runat="server" /></asp:Panel>

    <div class="form-grid">
        <section class="form-card">
            <h2>Dati generali del materiale</h2>
            <div class="field-grid">
                <label>Categorico assegnato automaticamente<asp:TextBox ID="CategoricoText" runat="server" CssClass="input" /></label>
                <label>Descrizione prodotto<asp:TextBox ID="NomeText" runat="server" CssClass="input" /></label>
                <label>Matricola<asp:TextBox ID="SerialeText" runat="server" CssClass="input" /></label>
                <label>Categoria<asp:DropDownList ID="TipologiaDropDown" runat="server" CssClass="input" /></label>
                <label>Oggetto ordinativo esistente<asp:DropDownList ID="ModelloHardwareDropDown" runat="server" CssClass="input" /></label>
                <label>Modello<asp:TextBox ID="MarcaNuovoModelloText" runat="server" CssClass="input" /></label>
                <label>Ditta costruttrice<asp:DropDownList ID="ImpiegoDropDown" runat="server" CssClass="input" /></label>
                <label>Prezzo inventario<asp:TextBox ID="ValoreAcquistoText" runat="server" CssClass="input" /></label>
                <label>Versamento / riferimento<asp:TextBox ID="NumeroFatturaText" runat="server" CssClass="input" /></label>
                <label class="field-span-2">Note<asp:TextBox ID="NoteText" runat="server" CssClass="input" TextMode="MultiLine" Rows="3" /></label>
            </div>
        </section>

        <section class="form-card">
            <h2>Stato e collocazione correnti</h2>
            <div class="field-grid">
                <label>Livello efficienza<asp:DropDownList ID="StatoDropDown" runat="server" CssClass="input" /></label>
                <label>Stanza<asp:DropDownList ID="UbicazioneDropDown" runat="server" CssClass="input" /></label>
                <label class="field-span-2">Note aggiuntive<asp:TextBox ID="NoteStatoText" runat="server" CssClass="input" TextMode="MultiLine" Rows="2" /></label>
            </div>
        </section>
    </div>

    <div class="page-actions">
        <asp:Button ID="SaveButton" runat="server" Text="Registra bene" CssClass="button primary" OnClick="SaveButton_Click" />
    </div>
</asp:Content>
