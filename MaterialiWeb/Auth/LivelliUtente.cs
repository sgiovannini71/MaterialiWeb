using System.ComponentModel;

namespace MaterialiGestioneWeb.Auth
{
    public enum LivelliUtente
    {
        [Description("visualizzazione dati materiali")]
        Visualizzatore = 100,

        [Description("gestione operativa materiali")]
        Operatore = 150,

        [Description("controllo completo")]
        Amministratore = 200
    }
}
