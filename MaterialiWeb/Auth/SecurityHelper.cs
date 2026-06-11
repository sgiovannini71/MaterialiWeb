using System;
using System.Linq;
using System.Web.UI;

namespace MaterialiGestioneWeb.Auth
{
    public static class SecurityHelper
    {
        public static bool CheckLevel(Page page, params LivelliUtente[] allowedLevels)
        {
            if (page == null)
                throw new ArgumentNullException("page");

            int livello;
            if (!TryGetSessionLevel(page, out livello))
            {
                RedirectUnauthorized(page);
                return false;
            }

            if (allowedLevels == null || allowedLevels.Length == 0)
            {
                RedirectUnauthorized(page);
                return false;
            }

            if (!allowedLevels.Select(x => (int)x).Contains(livello))
            {
                RedirectUnauthorized(page);
                return false;
            }

            return true;
        }

        public static bool CheckMinLevel(Page page, int minLevel)
        {
            if (page == null)
                throw new ArgumentNullException("page");

            int livello;
            if (!TryGetSessionLevel(page, out livello) || livello < minLevel)
            {
                RedirectUnauthorized(page);
                return false;
            }

            return true;
        }

        private static bool TryGetSessionLevel(Page page, out int livello)
        {
            livello = 0;
            object sessionValue = page.Session["Livello"];
            return sessionValue != null && int.TryParse(sessionValue.ToString(), out livello);
        }

        private static void RedirectUnauthorized(Page page)
        {
            page.Response.Redirect("~/AccessDenied.aspx", true);
        }
    }
}
