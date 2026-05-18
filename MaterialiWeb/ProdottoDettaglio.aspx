<%@ Page Title="Dettaglio prodotto" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ProdottoDettaglio.aspx.cs" Inherits="MaterialiGestioneWeb.ProdottoDettaglioPage" %>

<asp:Content ID="Title" ContentPlaceHolderID="TitleContent" runat="server">Dettaglio prodotto</asp:Content>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <asp:Panel ID="ErrorPanel" runat="server" CssClass="alert error" Visible="false">
        <asp:Literal ID="ErrorMessage" runat="server" />
    </asp:Panel>

    <asp:Panel ID="DetailPanel" runat="server" Visible="false">
        <section class="page-title">
            <h1><asp:Literal ID="NomeProdotto" runat="server" /></h1>
            <p>Dettaglio del materiale, assegnazione corrente, stanza, rete e riferimenti ordinativo.</p>
        </section>

        <div class="quick-links">
            <asp:HyperLink ID="AssegnaLink" runat="server" CssClass="action-tile">Assegna</asp:HyperLink>
            <asp:HyperLink ID="RientroLink" runat="server" CssClass="action-tile">Rientro/Riassegna</asp:HyperLink>
            <asp:HyperLink ID="StatoLink" runat="server" CssClass="action-tile">Cambia stato</asp:HyperLink>
            <asp:HyperLink ID="UbicazioneLink" runat="server" CssClass="action-tile">Cambia stanza</asp:HyperLink>
            <asp:HyperLink ID="ConfiguraLink" runat="server" CssClass="action-tile">Configura rete/postazione</asp:HyperLink>
            <asp:HyperLink ID="DismettiLink" runat="server" CssClass="action-tile">Versa o dismetti</asp:HyperLink>
        </div>

        <div class="detail-card">
            <dl>
                <dt>Categorico</dt><dd><asp:Literal ID="Categorico" runat="server" /></dd>
                <dt>Numero di serie</dt><dd><asp:Literal ID="Seriale" runat="server" /></dd>
                <dt>Categoria</dt><dd><asp:Literal ID="Tipologia" runat="server" /></dd>
                <dt>Modello</dt><dd><asp:Literal ID="Modello" runat="server" /></dd>
                <dt>Stato corrente</dt><dd><asp:Literal ID="Stato" runat="server" /></dd>
                <dt>Assegnatario</dt><dd><asp:Literal ID="Assegnatario" runat="server" /></dd>
                <dt>Stanza</dt><dd><asp:Literal ID="Ubicazione" runat="server" /></dd>
                <dt>Ditta costruttrice</dt><dd><asp:Literal ID="Fornitore" runat="server" /></dd>
                <dt>Versamento</dt><dd><asp:Literal ID="Fattura" runat="server" /></dd>
            </dl>
        </div>

        <section class="split-grid">
            <div>
                <h2>Rete e postazione</h2>
                <asp:Panel ID="ComputerPanel" runat="server" Visible="false">
                    <dl class="detail-list">
                        <dt>Tipo oggetto</dt><dd><asp:Literal ID="ComputerProcessore" runat="server" /></dd>
                        <dt>Ultimo movimento</dt><dd><asp:Literal ID="ComputerRam" runat="server" /></dd>
                        <dt>Nome macchina</dt><dd><asp:Literal ID="ComputerTipoRam" runat="server" /></dd>
                        <dt>MAC address</dt><dd><asp:Literal ID="ComputerImpiego" runat="server" /></dd>
                        <dt>MAC</dt><dd><asp:Literal ID="ComputerMac" runat="server" /></dd>
                        <dt>Note</dt><dd><asp:Literal ID="ComputerHostName" runat="server" /></dd>
                    </dl>
                </asp:Panel>
                <asp:Panel ID="NonComputerPanel" runat="server" Visible="false">
                    <p class="muted-text">Nessuna configurazione rete o postazione registrata.</p>
                </asp:Panel>
            </div>
            <div>
                <h2>Assegnazione corrente</h2>
                <asp:GridView ID="DischiGrid" runat="server" AutoGenerateColumns="False" CssClass="data-grid compact" GridLines="None" EmptyDataText="Nessuna assegnazione attiva.">
                    <Columns>
                        <asp:BoundField DataField="DataInizio" HeaderText="Dal" DataFormatString="{0:dd/MM/yyyy}" />
                        <asp:BoundField DataField="DataFine" HeaderText="Al" DataFormatString="{0:dd/MM/yyyy}" />
                        <asp:BoundField DataField="Valore" HeaderText="Snapshot" />
                        <asp:BoundField DataField="Note" HeaderText="Assegnatario" />
                    </Columns>
                </asp:GridView>
            </div>
            <div class="field-span-2">
                <h2>Ordinativo</h2>
                <asp:GridView ID="DocumentiGrid" runat="server" AutoGenerateColumns="False" CssClass="data-grid compact" GridLines="None" EmptyDataText="Nessun ordinativo collegato.">
                    <Columns>
                        <asp:BoundField DataField="CodiceOrdinativo" HeaderText="Codice" />
                        <asp:BoundField DataField="DenominazioneOrdinativo" HeaderText="Denominazione" />
                        <asp:BoundField DataField="EnteStipulante" HeaderText="Ente" />
                        <asp:BoundField DataField="DittaOrdinativo" HeaderText="Ditta" />
                    </Columns>
                </asp:GridView>
            </div>
            <div>
                <h2>Storico assegnazioni</h2>
                <asp:GridView ID="StoricoAssegnazioniGrid" runat="server" AutoGenerateColumns="False" CssClass="data-grid compact" GridLines="None" EmptyDataText="Nessuna assegnazione storica.">
                    <Columns>
                        <asp:BoundField DataField="DataInizio" HeaderText="Dal" DataFormatString="{0:dd/MM/yyyy}" />
                        <asp:BoundField DataField="DataFine" HeaderText="Al" DataFormatString="{0:dd/MM/yyyy}" />
                        <asp:BoundField DataField="Valore" HeaderText="Contesto" />
                        <asp:BoundField DataField="Note" HeaderText="Assegnatario" />
                    </Columns>
                </asp:GridView>
            </div>
        </section>
    </asp:Panel>
</asp:Content>
