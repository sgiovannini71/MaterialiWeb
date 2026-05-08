using System;
using System.Collections.Generic;
using System.Globalization;
using MaterialiGestioneWeb.Infrastructure;
using MaterialiGestioneWeb.Models;
using MaterialiGestioneWeb.Services;

namespace MaterialiGestioneWeb
{
    public partial class Computer : System.Web.UI.Page
    {
        private readonly InventarioRepository _repository = new InventarioRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindGrid(null);
            }
        }

        protected void SearchButton_Click(object sender, EventArgs e)
        {
            BindGrid(SearchText.Text);
        }

        protected void ResetButton_Click(object sender, EventArgs e)
        {
            SearchText.Text = string.Empty;
            BindGrid(null);
        }

        protected void ExportCsvButton_Click(object sender, EventArgs e)
        {
            try
            {
                ErrorPanel.Visible = false;
                ExportCsv(_repository.GetComputer(SearchText.Text));
            }
            catch (Exception ex)
            {
                ErrorPanel.Visible = true;
                ErrorMessage.Text = Server.HtmlEncode(ex.Message);
            }
        }

        private void BindGrid(string search)
        {
            try
            {
                ErrorPanel.Visible = false;
                ComputerGrid.DataSource = _repository.GetComputer(search);
                ComputerGrid.DataBind();
            }
            catch (Exception ex)
            {
                ErrorPanel.Visible = true;
                ErrorMessage.Text = Server.HtmlEncode(ex.Message);
            }
        }

        private void ExportCsv(IList<ComputerCorrente> computers)
        {
            CsvExport.Write(this, "RetePostazioni", new[]
            {
                "Categorico",
                "Nome macchina",
                "Descrizione",
                "Matricola",
                "Categoria",
                "Modello",
                "MAC",
                "Stanza",
                "Stato",
                "Assegnatario"
            }, BuildCsvRows(computers));
        }

        private static IEnumerable<IEnumerable<string>> BuildCsvRows(IEnumerable<ComputerCorrente> computers)
        {
            foreach (var item in computers)
            {
                yield return new[]
                {
                    item.Categorico.HasValue ? item.Categorico.Value.ToString(CultureInfo.InvariantCulture) : string.Empty,
                    item.NomeMacchina,
                    item.DescrizioneProdotto,
                    item.Matricola,
                    item.Categoria,
                    item.Modello,
                    item.MacAddress,
                    item.NumeroStanza,
                    item.LivelloEfficienza,
                    item.AssegnatarioDisplay
                };
            }
        }
    }
}
