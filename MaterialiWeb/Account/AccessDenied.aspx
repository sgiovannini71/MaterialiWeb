<%@ Page Title="Accesso negato" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AccessDenied.aspx.cs" Inherits="MaterialiGestioneWeb.Account.AccessDenied" %>
<asp:Content ID="Title" ContentPlaceHolderID="TitleContent" runat="server">Accesso negato</asp:Content>
<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">
    <section class="panel">
        <h1>Accesso negato</h1>
        <p>L'utente Windows connesso non è autorizzato ad accedere a Gestione Materiali.</p>
        <p>Contattare l'amministratore per verificare la registrazione nella tabella Autorizzazioni.</p>
    </section>
</asp:Content>
