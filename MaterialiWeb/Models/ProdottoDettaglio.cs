using System.Collections.Generic;

namespace MaterialiGestioneWeb.Models
{
    public class ProdottoDettaglio
    {
        public ProdottoCorrente Prodotto { get; set; }
        public OrdinativoInfo Ordinativo { get; set; }
        public IList<StoricoItem> StoricoAssegnazioni { get; set; }

        public ProdottoDettaglio()
        {
            StoricoAssegnazioni = new List<StoricoItem>();
        }
    }

    public class OrdinativoInfo
    {
        public int? IdOrdinativo { get; set; }
        public string CodiceOrdinativo { get; set; }
        public string DenominazioneOrdinativo { get; set; }
        public string EnteStipulante { get; set; }
        public string EstremiOrdinativo { get; set; }
        public string DittaOrdinativo { get; set; }
    }
}
