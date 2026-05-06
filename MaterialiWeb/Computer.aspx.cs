using System;
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
    }
}
