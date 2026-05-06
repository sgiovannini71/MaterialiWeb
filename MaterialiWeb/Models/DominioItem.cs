namespace MaterialiGestioneWeb.Models
{
    public class DominioItem
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Codice { get; set; }
        public string Nome { get; set; }
        public string Extra { get; set; }
        public string Extra2 { get; set; }
        public bool Flag { get; set; }
    }
}
