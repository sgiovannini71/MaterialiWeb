<%@ Page Title="Dashboard" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="MaterialiGestioneWeb.Default" %>

<asp:Content ID="Title" ContentPlaceHolderID="TitleContent" runat="server">Dashboard</asp:Content>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <section class="hero">
        <div>
            <h1>Dashboard inventario</h1>
            <p>Vista sintetica del materiale informatico con assegnazioni correnti, rete e postazioni.</p>
        </div>
        <div class="hero-actions">
            <a class="button primary" href="Prodotti.aspx">Apri elenco prodotti</a>
            <a class="button" href="ProdottiAssegnati.aspx">Prodotti per assegnatario</a>
            <a class="button" href="StoricoAssegnazioni.aspx">Storico assegnazioni</a>
            <a class="button" href="ImportaRete.aspx">Import rete TXT</a>
            <a class="button" href="html/guida_materiali_gestione.html" target="_blank" rel="noopener">Help</a>
            <a class="button" href="NuovoBene.aspx">Registra nuovo bene</a>
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

    <section class="quick-links">
        <a class="action-tile" href="NuovoBene.aspx">Nuovo bene</a>
        <a class="action-tile" href="ProdottiAssegnati.aspx">Prodotti per assegnatario</a>
        <a class="action-tile" href="AssegnaBene.aspx">Assegna bene</a>
        <a class="action-tile" href="RientroRiassegnazione.aspx">Rientro o riassegnazione</a>
        <a class="action-tile" href="StoricoAssegnazioni.aspx">Storico assegnazioni</a>
        <a class="action-tile" href="CambiaStato.aspx">Cambia stato</a>
        <a class="action-tile" href="CambiaUbicazione.aspx">Cambia ubicazione</a>
        <a class="action-tile" href="ConfiguraComputer.aspx">Aggiorna rete/postazione</a>
        <a class="action-tile" href="ImportaRete.aspx">Import rete TXT</a>
        <a class="action-tile" href="html/guida_materiali_gestione.html" target="_blank" rel="noopener">Help applicazione</a>
        <a class="action-tile" href="DismettiBene.aspx">Versa o dismetti</a>
    </section>
</asp:Content>
