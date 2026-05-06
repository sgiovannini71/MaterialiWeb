namespace MaterialiGestioneWeb.Models
{
    public class LookupItem
    {
        public int Id { get; set; }
        public string Codice { get; set; }
        public string Nome { get; set; }
        public bool Flag { get; set; }

        public string DisplayName
        {
            get
            {
                return string.IsNullOrWhiteSpace(Codice) ? Nome : Codice + " - " + Nome;
            }
        }
    }
}
