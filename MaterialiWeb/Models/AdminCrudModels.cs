using System;

namespace MaterialiGestioneWeb.Models
{
    public class ProdottoAdminItem
    {
        public int IdProdotto { get; set; }
        public int? IdStanza { get; set; }
        public int? IdOrdinativo { get; set; }
        public int? IdOggOrdinativo { get; set; }
        public int? Categorico { get; set; }
        public string Matricola { get; set; }
        public int? IdEfficienza { get; set; }
        public DateTime? DataUltimaMov { get; set; }
        public string Note { get; set; }
        public string Versamento { get; set; }
        public string StanzaDescrizione { get; set; }
        public string OrdinativoDescrizione { get; set; }
        public string OggettoDescrizione { get; set; }
        public string EfficienzaDescrizione { get; set; }
    }

    public class OrdinativoAdminItem
    {
        public int IdOrdinativo { get; set; }
        public string CodiceOrdinativo { get; set; }
        public string DenominazioneOrdinativo { get; set; }
        public string EF { get; set; }
        public string TipoOrdinativo { get; set; }
        public int? IdDittaOrdinativo { get; set; }
        public string EnteStipulante { get; set; }
        public string EstremiOrdinativo { get; set; }
        public string DittaDescrizione { get; set; }
    }

    public class OggettoOrdinativoAdminItem
    {
        public int IdOggOrdinativo { get; set; }
        public int? IdOrdinativo { get; set; }
        public string DescrizioneProdotto { get; set; }
        public int? IdDittaCostruttrice { get; set; }
        public string Modello { get; set; }
        public string NUC { get; set; }
        public decimal PrezzoUnitarioNetto { get; set; }
        public decimal PrezzoInventario { get; set; }
        public int? IdCategProdotti { get; set; }
        public string OrdinativoDescrizione { get; set; }
        public string DittaDescrizione { get; set; }
        public string CategoriaDescrizione { get; set; }
    }

    public class NetworkDataAdminItem
    {
        public int IdNetworkData { get; set; }
        public int IdProdotto { get; set; }
        public string MacAddress { get; set; }
        public string Note { get; set; }
        public string ProdottoDescrizione { get; set; }
    }

    public class PostazioneAdminItem
    {
        public int IdPostazione { get; set; }
        public int IdProdotto { get; set; }
        public int? IdNomeMacchina { get; set; }
        public string NomeMacchina { get; set; }
        public string ProdottoDescrizione { get; set; }
    }

    public class ProdPersAdminItem
    {
        public int IdProdPers { get; set; }
        public int? IdProdotto { get; set; }
        public int? IdPersonale { get; set; }
        public DateTime? DataAssegnazione { get; set; }
        public string ProdottoDescrizione { get; set; }
        public string PersonaleDescrizione { get; set; }
    }

    public class ProdPersStoricoAdminItem
    {
        public int Id { get; set; }
        public int? IdProdPers { get; set; }
        public int? IdProdotto { get; set; }
        public int? IdPersonale { get; set; }
        public string ProdottoDescrizione { get; set; }
        public string PersonaleDescrizione { get; set; }
        public DateTime? DataAssegnazione { get; set; }
        public DateTime? DataRestituzione { get; set; }
        public string NumeroStanza { get; set; }
        public string LivelloEfficienza { get; set; }
        public string NoteProdotto { get; set; }
        public string NomeMacchina { get; set; }
        public string SerialNumber { get; set; }
    }
}
