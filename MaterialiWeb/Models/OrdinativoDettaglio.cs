using System.Collections.Generic;

namespace MaterialiGestioneWeb.Models
{
    public class OrdinativoDettaglio
    {
        public OrdinativoAdminItem Ordinativo { get; set; }
        public IList<OggettoOrdinativoDettaglioItem> Oggetti { get; set; }

        public OrdinativoDettaglio()
        {
            Oggetti = new List<OggettoOrdinativoDettaglioItem>();
        }
    }

    public class OggettoOrdinativoDettaglioItem
    {
        public int IdOggOrdinativo { get; set; }
        public int? IdOrdinativo { get; set; }
        public string DescrizioneProdotto { get; set; }
        public string Modello { get; set; }
        public string NUC { get; set; }
        public int Quantita { get; set; }
        public decimal PrezzoUnitarioNetto { get; set; }
        public decimal PrezzoInventario { get; set; }
        public string DittaDescrizione { get; set; }
        public string CategoriaDescrizione { get; set; }
        public int ProdottiGenerati { get; set; }
        public string CategoriciRiepilogo { get; set; }
        public IList<ProdottoOrdinativoItem> Prodotti { get; set; }

        public OggettoOrdinativoDettaglioItem()
        {
            Prodotti = new List<ProdottoOrdinativoItem>();
        }
    }

    public class ProdottoOrdinativoItem
    {
        public int IdProdotto { get; set; }
        public int IdOggOrdinativo { get; set; }
        public int? Categorico { get; set; }
        public string Matricola { get; set; }
        public string LivelloEfficienza { get; set; }
        public string NumeroStanza { get; set; }
        public string Versamento { get; set; }
    }
}
