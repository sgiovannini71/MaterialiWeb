<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="MaterialiGestioneWeb.Default" %>

<asp:Content ID="Title" ContentPlaceHolderID="TitleContent" runat="server">Dashboard</asp:Content>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <section class="hero">
        <div>
            <h1>Dashboard inventario</h1>
            <p>Accesso organizzato per acquisizione, gestione operativa dei beni, area tecnica e consultazione.</p>
        </div>
        <div class="hero-actions">
            <a class="button primary" href="DettaglioOrdinativo.aspx">Apri ordinativi</a>
            <a class="button" href="Prodotti.aspx">Apri prodotti</a>
            <a class="button" href="AssegnaBene.aspx">Assegna bene</a>
            <a class="button" href="ConfiguraComputer.aspx">Aggiorna rete/postazione</a>
            <a class="button" href="StoricoAssegnazioni.aspx">Storico assegnazioni</a>
            <a class="button" href="html/guida_materiali_gestione.html" target="_blank" rel="noopener">Help</a>
        </div>
    </section>

    <asp:Panel ID="ErrorPanel" runat="server" CssClass="alert error" Visible="false">
        <asp:Literal ID="ErrorMessage" runat="server" />
    </asp:Panel>

    <section class="stats-grid">
        <article class="stat-card">
            <span>Prodotti</span>
            <strong><asp:Literal ID="ProdottiCount" runat="server" /></strong>
        </article>
        <article class="stat-card">
            <span>Con rete</span>
            <strong><asp:Literal ID="ComputerCount" runat="server" /></strong>
        </article>
        <article class="stat-card">
            <span>Assegnati</span>
            <strong><asp:Literal ID="AssegnatiCount" runat="server" /></strong>
        </article>
        <article class="stat-card">
            <span>Da versare</span>
            <strong><asp:Literal ID="DaVersareCount" runat="server" /></strong>
        </article>
        <article class="stat-card">
            <span>Con postazione</span>
            <strong><asp:Literal ID="AttesaDistribuzioneCount" runat="server" /></strong>
        </article>
        <article class="stat-card">
            <span>Senza stanza</span>
            <strong><asp:Literal ID="AlienatiCount" runat="server" /></strong>
        </article>
        <article class="stat-card">
            <span>Senza assegnatario</span>
            <strong><asp:Literal ID="SenzaAssegnatarioCount" runat="server" /></strong>
        </article>
    </section>

    <section class="dashboard-section">
        <div class="section-heading">
            <div>
                <h2>Acquisizione</h2>
                <p>Gestione testate ordinativo, righe di acquisto e beni generati automaticamente.</p>
            </div>
        </div>
        <div class="quick-links">
            <a class="action-tile" href="DettaglioOrdinativo.aspx">Dettaglio ordinativi</a>
            <a class="action-tile" href="GestioneCrud.aspx#crud-ordinativi">CRUD ordinativi</a>
            <a class="action-tile" href="GestioneCrud.aspx#crud-oggetti">CRUD oggetti ordinativo</a>
            <a class="action-tile" href="NuovoBene.aspx">Nuovo bene</a>
        </div>
    </section>

    <section class="dashboard-section">
        <div class="section-heading">
            <div>
                <h2>Gestione Beni</h2>
                <p>Processi operativi sul singolo bene inventariato e sulle sue movimentazioni.</p>
            </div>
        </div>
        <div class="quick-links">
            <a class="action-tile" href="Prodotti.aspx">Prodotti</a>
            <a class="action-tile" href="AssegnaBene.aspx">Assegna bene</a>
            <a class="action-tile" href="RientroRiassegnazione.aspx">Rientro o riassegnazione</a>
            <a class="action-tile" href="CambiaStato.aspx">Cambia stato</a>
            <a class="action-tile" href="CambiaUbicazione.aspx">Cambia ubicazione</a>
            <a class="action-tile" href="DismettiBene.aspx">Versa o dismetti</a>
        </div>
    </section>

    <section class="dashboard-section">
        <div class="section-heading">
            <div>
                <h2>Area Tecnica</h2>
                <p>Configurazione di rete, postazioni e dati tecnici dei dispositivi.</p>
            </div>
        </div>
        <div class="quick-links">
            <a class="action-tile" href="ConfiguraComputer.aspx">Aggiorna rete/postazione</a>
            <a class="action-tile" href="Computer.aspx">Rete e postazioni</a>
            <a class="action-tile" href="ImportaRete.aspx">Import rete TXT</a>
            <a class="action-tile" href="Domini.aspx">Domini</a>
        </div>
    </section>

    <section class="dashboard-section">
        <div class="section-heading">
            <div>
                <h2>Consultazione</h2>
                <p>Viste trasversali per analisi, controllo e supporto operativo.</p>
            </div>
        </div>
        <div class="quick-links">
            <a class="action-tile" href="ProdottiAssegnati.aspx">Prodotti per assegnatario</a>
            <a class="action-tile" href="StoricoAssegnazioni.aspx">Storico assegnazioni</a>
            <a class="action-tile" href="GestioneCrud.aspx">CRUD amministrativi</a>
            <a class="action-tile" href="html/guida_materiali_gestione.html" target="_blank" rel="noopener">Help applicazione</a>
        </div>
    </section>
</asp:Content>
