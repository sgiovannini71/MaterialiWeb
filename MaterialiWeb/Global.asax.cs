using System;
using System.Web;
using MaterialiGestioneWeb.Infrastructure;

namespace MaterialiGestioneWeb
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            AppLogger.Info("Global.Application_Start", "Applicazione avviata.");
        }
    }
}
