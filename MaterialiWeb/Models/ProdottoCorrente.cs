using System;

namespace MaterialiGestioneWeb.Models
{
    public class ProdottoCorrente
    {
        public int IdProdotto { get; set; }
        public int? Categorico { get; set; }
        public string Matricola { get; set; }
        public string Categoria { get; set; }
        public string TipoOggetto { get; set; }
        public string DescrizioneProdotto { get; set; }
        public string Modello { get; set; }
        public string DittaCostruttrice { get; set; }
        public string LivelloEfficienza { get; set; }
        public int? IdPersonale { get; set; }
        public string AssegnatarioDisplay { get; set; }
        public string NumeroStanza { get; set; }
        public DateTime? DataUltimaMov { get; set; }
        public string Note { get; set; }
        public string Versamento { get; set; }
        public string NomeMacchina { get; set; }
        public string MacAddress { get; set; }

        public string DescrizioneSintetica
        {
            get
            {
                return string.Join(" - ", new[] { ToText(Categorico), DescrizioneProdotto, Matricola }).Trim(' ', '-');
            }
        }

        private static string ToText(int? value)
        {
            return value.HasValue ? value.Value.ToString() : string.Empty;
        }
    }
}
