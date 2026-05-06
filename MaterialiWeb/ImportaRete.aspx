<%@ Page Title="Importa rete da TXT" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ImportaRete.aspx.cs" Inherits="MaterialiGestioneWeb.ImportaRetePage" %>

<asp:Content ID="TitleImportaRete" ContentPlaceHolderID="TitleContent" runat="server">Importa rete da TXT</asp:Content>
<asp:Content ID="MainImportaRete" ContentPlaceHolderID="MainContent" runat="server">
    <section class="page-title">
        <h1>Importa rete da TXT</h1>
        <p>Carica un file con due colonne: nome macchina e MAC address.</p>
    </section>

    <asp:Panel ID="ErrorPanel" runat="server" CssClass="alert error" Visible="false">
        <asp:Literal ID="ErrorMessage" runat="server" />
    </asp:Panel>
    <asp:Panel ID="SuccessPanel" runat="server" CssClass="alert success" Visible="false">
        <asp:Literal ID="SuccessMessage" runat="server" />
    </asp:Panel>

    <section class="form-card">
        <div class="field-grid">
            <label class="field-span-2">File TXT<asp:FileUpload ID="ReteFileUpload" runat="server" CssClass="input" /></label>
        </div>
        <div class="page-actions">
            <asp:Button ID="ImportButton" runat="server" Text="Importa e aggiorna" CssClass="button primary" OnClick="ImportButton_Click" />
        </div>
    </section>

    <asp:Panel ID="ResultPanel" runat="server" CssClass="form-card" Visible="false">
        <div class="domain-title">
            <h2>Esito import</h2>
        </div>
        <asp:GridView ID="ResultGrid" runat="server"
            AutoGenerateColumns="False"
            CssClass="data-grid"
            GridLines="None"
            EmptyDataText="Nessuna riga elaborata.">
            <Columns>
                <asp:BoundField DataField="Riga" HeaderText="Riga" />
                <asp:BoundField DataField="NomeMacchina" HeaderText="Nome macchina" />
                <asp:BoundField DataField="MacAddress" HeaderText="MAC address" />
                <asp:BoundField DataField="IdProdotto" HeaderText="Id prodotto" />
                <asp:BoundField DataField="Esito" HeaderText="Esito" />
                <asp:BoundField DataField="Messaggio" HeaderText="Messaggio" />
            </Columns>
        </asp:GridView>
    </asp:Panel>
</asp:Content>
