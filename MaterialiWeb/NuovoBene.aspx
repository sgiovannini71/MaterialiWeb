<%@ Page Title="Nuovo bene" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="NuovoBene.aspx.cs" Inherits="MaterialiGestioneWeb.NuovoBenePage" %>

<asp:Content ID="Title" ContentPlaceHolderID="TitleContent" runat="server">Nuovo bene</asp:Content>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <section class="page-title">
        <h1>Completa bene generato</h1>
        <p>Questa pagina non crea nuovi categorici. Serve a completare un prodotto gia generato da un oggetto ordinativo con quantita. La lista dei prodotti viene filtrata per livello di efficienza; il valore iniziale e IdEfficienza=1, "Efficiente in Uso".</p>
    </section>

    <asp:Panel ID="ErrorPanel" runat="server" CssClass="alert error" Visible="false"><asp:Literal ID="ErrorMessage" runat="server" /></asp:Panel>
    <asp:Panel ID="SuccessPanel" runat="server" CssClass="alert success" Visible="false"><asp:Literal ID="SuccessMessage" runat="server" /></asp:Panel>

    <section class="form-card">
        <h2>Selezione prodotto</h2>
        <div class="field-grid">
            <label>Filtro efficienza<asp:DropDownList ID="FiltroEfficienzaDropDown" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="FiltroEfficienzaDropDown_SelectedIndexChanged" /></label>
            <label class="field-span-2">Prodotto<asp:DropDownList ID="ProdottoDropDown" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="ProdottoDropDown_SelectedIndexChanged" /></label>
        </div>
    </section>

    <asp:Panel ID="DetailPanel" runat="server" Visible="false">
    <div class="form-grid">
        <section class="form-card">
            <h2>Dati derivati dall'ordinativo</h2>
            <div class="field-grid">
                <label>Categorico<asp:TextBox ID="CategoricoText" runat="server" CssClass="input" ReadOnly="true" /></label>
                <label>Descrizione prodotto<asp:TextBox ID="NomeText" runat="server" CssClass="input" ReadOnly="true" /></label>
                <label>Categoria<asp:TextBox ID="TipologiaText" runat="server" CssClass="input" ReadOnly="true" /></label>
                <label>Oggetto ordinativo<asp:TextBox ID="ModelloHardwareText" runat="server" CssClass="input" ReadOnly="true" /></label>
                <label>Modello<asp:TextBox ID="MarcaNuovoModelloText" runat="server" CssClass="input" ReadOnly="true" /></label>
                <label>Ditta costruttrice<asp:TextBox ID="ImpiegoText" runat="server" CssClass="input" ReadOnly="true" /></label>
                <label>Ordinativo<asp:TextBox ID="OrdinativoText" runat="server" CssClass="input" ReadOnly="true" /></label>
            </div>
        </section>

        <section class="form-card">
            <h2>Completamento anagrafica</h2>
            <div class="field-grid">
                <label>Matricola<asp:TextBox ID="SerialeText" runat="server" CssClass="input" /></label>
                <label>Versamento / riferimento<asp:TextBox ID="NumeroFatturaText" runat="server" CssClass="input" /></label>
                <label class="field-span-2">Note<asp:TextBox ID="NoteText" runat="server" CssClass="input" TextMode="MultiLine" Rows="3" /></label>
            </div>
        </section>

        <section class="form-card">
            <h2>Stato e collocazione correnti</h2>
            <div class="field-grid">
                <label>Livello efficienza<asp:DropDownList ID="StatoDropDown" runat="server" CssClass="input" /></label>
                <label>Stanza<asp:DropDownList ID="UbicazioneDropDown" runat="server" CssClass="input" /></label>
                <label class="field-span-2">Note operative<asp:TextBox ID="NoteStatoText" runat="server" CssClass="input" TextMode="MultiLine" Rows="2" /></label>
            </div>
        </section>
    </div>

    <div class="page-actions">
        <asp:Button ID="SaveButton" runat="server" Text="Completa bene" CssClass="button primary" OnClick="SaveButton_Click" />
    </div>
    </asp:Panel>
</asp:Content>
