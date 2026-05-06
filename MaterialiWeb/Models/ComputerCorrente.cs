namespace MaterialiGestioneWeb.Models
{
    public class ComputerCorrente
    {
        public int IdProdotto { get; set; }
        public int? Categorico { get; set; }
        public string Matricola { get; set; }
        public string Categoria { get; set; }
        public string DescrizioneProdotto { get; set; }
        public string Modello { get; set; }
        public string NomeMacchina { get; set; }
        public string MacAddress { get; set; }
        public string NumeroStanza { get; set; }
        public string LivelloEfficienza { get; set; }
        public int? IdPersonale { get; set; }
        public string AssegnatarioDisplay { get; set; }
    }
}
