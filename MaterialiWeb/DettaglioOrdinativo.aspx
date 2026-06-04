<%@ Page Title="Dettaglio ordinativo" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="DettaglioOrdinativo.aspx.cs" Inherits="MaterialiGestioneWeb.DettaglioOrdinativoPage" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">Dettaglio ordinativo</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <section class="page-title">
        <div>
            <h1>Dettaglio ordinativi</h1>
            <p>Vista gerarchica di testata ordinativo, righe oggetto ordinativo e prodotti inventariali generati.</p>
        </div>
    </section>

    <asp:Panel ID="ErrorPanel" runat="server" CssClass="alert error" Visible="false">
        <asp:Literal ID="ErrorMessage" runat="server" />
    </asp:Panel>

    <asp:Panel ID="DetailPanel" runat="server" Visible="false">
        <section class="page-title">
            <div>
                <h1><asp:Literal ID="NomeOrdinativo" runat="server" /></h1>
                <p><asp:Literal ID="DescrizioneOrdinativo" runat="server" /></p>
            </div>
            <div class="hero-actions">
                <asp:HyperLink ID="CrudOrdinativoLink" runat="server" CssClass="button">Modifica testata</asp:HyperLink>
                <asp:HyperLink ID="CrudOggettiLink" runat="server" CssClass="button primary">Gestisci righe oggetto</asp:HyperLink>
            </div>
        </section>

        <section class="stats-grid">
            <article class="stat-card">
                <span>Oggetti ordinativo</span>
                <strong><asp:Literal ID="OggettiCount" runat="server" /></strong>
            </article>
            <article class="stat-card">
                <span>Quantita dichiarata</span>
                <strong><asp:Literal ID="QuantitaCount" runat="server" /></strong>
            </article>
            <article class="stat-card">
                <span>Prodotti generati</span>
                <strong><asp:Literal ID="ProdottiCount" runat="server" /></strong>
            </article>
            <article class="stat-card">
                <span>Stato prodotti</span>
                <div class="status-summary"><asp:Literal ID="StatiProdottiText" runat="server" /></div>
            </article>
        </section>

        <div class="detail-card" style="margin-top: 1rem;">
            <dl>
                <dt>Id ordinativo</dt><dd><asp:Literal ID="IdOrdinativoText" runat="server" /></dd>
                <dt>Codice</dt><dd><asp:Literal ID="CodiceText" runat="server" /></dd>
                <dt>Denominazione</dt><dd><asp:Literal ID="DenominazioneText" runat="server" /></dd>
                <dt>Ditta ordinativo</dt><dd><asp:Literal ID="DittaText" runat="server" /></dd>
                <dt>Ente stipulante</dt><dd><asp:Literal ID="EnteText" runat="server" /></dd>
                <dt>Estremi</dt><dd><asp:Literal ID="EstremiText" runat="server" /></dd>
                <dt>EF</dt><dd><asp:Literal ID="EfText" runat="server" /></dd>
                <dt>Tipo ordinativo</dt><dd><asp:Literal ID="TipoText" runat="server" /></dd>
            </dl>
        </div>

        <section class="form-card" style="margin-top: 1rem;">
            <div class="domain-title">
                <h2>Composizione dell'ordinativo</h2>
                <asp:HyperLink ID="AddOggettoLink" runat="server">Aggiungi oggetto ordinativo</asp:HyperLink>
            </div>
            <asp:GridView ID="OggettiGrid" runat="server"
                AutoGenerateColumns="False"
                CssClass="data-grid"
                GridLines="None"
                EmptyDataText="Nessun oggetto ordinativo collegato."
                OnRowDataBound="OggettiGrid_RowDataBound">
                <Columns>
                    <asp:BoundField DataField="DescrizioneProdotto" HeaderText="Descrizione" />
                    <asp:BoundField DataField="Modello" HeaderText="Modello" />
                    <asp:BoundField DataField="NUC" HeaderText="NUC" />
                    <asp:BoundField DataField="Quantita" HeaderText="Quantita" />
                    <asp:BoundField DataField="CategoriaDescrizione" HeaderText="Categoria" />
                    <asp:BoundField DataField="DittaDescrizione" HeaderText="Ditta costruttrice" />
                    <asp:TemplateField HeaderText="Stato prodotti">
                        <ItemTemplate>
                            <div class="status-summary status-summary-compact"><%# FormatOggettoStati(Container.DataItem) %></div>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Prodotti">
                        <ItemTemplate>
                            <details class="products-disclosure">
                                <summary>Vedi prodotti (<%# Eval("ProdottiGenerati") %>)</summary>
                                <asp:Repeater ID="ProdottiOggettoRepeater" runat="server">
                                    <HeaderTemplate><div class="product-list"></HeaderTemplate>
                                    <ItemTemplate>
                                        <article class="product-list-item">
                                            <div>
                                                <strong>Categorico <%# Eval("Categorico") %></strong>
                                                <span><%# FormatProductMeta(Container.DataItem) %></span>
                                            </div>
                                            <a class="button" href='<%# "ProdottoDettaglio.aspx?id=" + Eval("IdProdotto") %>'>Dettaglio</a>
                                        </article>
                                    </ItemTemplate>
                                    <FooterTemplate></div></FooterTemplate>
                                </asp:Repeater>
                                <asp:PlaceHolder ID="NoProdottiPlaceholder" runat="server" Visible="false">
                                    <p class="muted-text">Nessun prodotto generato per questa riga.</p>
                                </asp:PlaceHolder>
                            </details>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </section>
    </asp:Panel>

    <asp:Panel ID="OrdinativiListPanel" runat="server" CssClass="form-card ordinativo-search-section">
        <div class="section-heading">
            <div>
                <h2>Cerca o seleziona ordinativo</h2>
                <p>Usa la ricerca per filtrare l'elenco o il menu per aprire rapidamente un altro dettaglio.</p>
            </div>
            <asp:HyperLink ID="CrudAdminLink" runat="server" CssClass="button" NavigateUrl="GestioneCrud.aspx#crud-ordinativi">Apri CRUD ordinativi</asp:HyperLink>
        </div>
        <div class="toolbar ordinativo-toolbar">
            <asp:TextBox ID="SearchText" runat="server" CssClass="input" placeholder="Cerca per codice, denominazione, ditta, ente o estremi..." />
            <asp:Button ID="SearchButton" runat="server" Text="Cerca" CssClass="button primary" OnClick="SearchButton_Click" />
            <asp:Button ID="ResetButton" runat="server" Text="Reset" CssClass="button" OnClick="ResetButton_Click" />
            <asp:DropDownList ID="OrdinativoDropDown" runat="server" CssClass="input" AutoPostBack="true" OnSelectedIndexChanged="OrdinativoDropDown_SelectedIndexChanged" />
        </div>
        <asp:GridView ID="OrdinativiListGrid" runat="server"
            AutoGenerateColumns="False"
            CssClass="data-grid compact"
            GridLines="None"
            EmptyDataText="Nessun ordinativo disponibile.">
            <Columns>
                <asp:BoundField DataField="IdOrdinativo" HeaderText="Id" />
                <asp:BoundField DataField="CodiceOrdinativo" HeaderText="Codice" />
                <asp:BoundField DataField="DenominazioneOrdinativo" HeaderText="Denominazione" />
                <asp:BoundField DataField="DittaDescrizione" HeaderText="Ditta" />
                <asp:HyperLinkField Text="Dettaglio" DataNavigateUrlFields="IdOrdinativo" DataNavigateUrlFormatString="DettaglioOrdinativo.aspx?id={0}" />
            </Columns>
        </asp:GridView>
    </asp:Panel>
</asp:Content>
