using System;
using System.Collections.Generic;

namespace MaterialiGestioneWeb.Models
{
    public class NuovoProdottoInput
    {
        public int? Categorico { get; set; }
        public string Matricola { get; set; }
        public int? IdStanza { get; set; }
        public int? IdOggettoOrdinativo { get; set; }
        public int? IdEfficienza { get; set; }
        public string Note { get; set; }
        public string Versamento { get; set; }
        public string DescrizioneProdotto { get; set; }
        public string Modello { get; set; }
        public int? IdCategoria { get; set; }
        public int? IdDittaCostruttrice { get; set; }
        public decimal? PrezzoUnitarioNetto { get; set; }
        public decimal? PrezzoInventario { get; set; }
    }

    public class AssegnazioneInput
    {
        public int IdProdotto { get; set; }
        public int IdPersonale { get; set; }
        public DateTime DataAssegnazione { get; set; }
        public string Note { get; set; }
    }

    public class RientroRiassegnazioneInput
    {
        public int IdProdotto { get; set; }
        public DateTime DataOperazione { get; set; }
        public bool CreaNuovaAssegnazione { get; set; }
        public int? NuovoIdPersonale { get; set; }
        public string Note { get; set; }
    }

    public class CambioStatoInput
    {
        public int IdProdotto { get; set; }
        public int IdEfficienza { get; set; }
        public string Note { get; set; }
    }

    public class CambioUbicazioneInput
    {
        public int IdProdotto { get; set; }
        public int IdStanza { get; set; }
        public string Note { get; set; }
    }

    public class AggiornamentoComputerInput
    {
        public int IdProdotto { get; set; }
        public string NomeMacchina { get; set; }
        public string MacAddress { get; set; }
        public string NoteRete { get; set; }
    }

    public class DismissioneInput
    {
        public int IdProdotto { get; set; }
        public int? IdEfficienza { get; set; }
        public string Versamento { get; set; }
        public string Note { get; set; }
        public bool ChiudiAssegnazioneAttiva { get; set; }
    }

    public class StoricoItem
    {
        public DateTime DataInizio { get; set; }
        public DateTime? DataFine { get; set; }
        public string Valore { get; set; }
        public string Note { get; set; }
    }

    public class StoricoAssegnazioneConsultazioneItem
    {
        public int Id { get; set; }
        public int? IdProdotto { get; set; }
        public int? Categorico { get; set; }
        public string DescrizioneProdotto { get; set; }
        public string Matricola { get; set; }
        public int? IdPersonale { get; set; }
        public string AssegnatarioDisplay { get; set; }
        public DateTime? DataAssegnazione { get; set; }
        public DateTime? DataRestituzione { get; set; }
        public string NumeroStanza { get; set; }
        public string LivelloEfficienza { get; set; }
        public string NomeMacchina { get; set; }
        public string SerialNumber { get; set; }
        public string NoteProdotto { get; set; }

        public string ProdottoDisplay
        {
            get
            {
                return string.Join(" - ", new[]
                {
                    Categorico.HasValue ? Categorico.Value.ToString() : string.Empty,
                    DescrizioneProdotto,
                    Matricola
                }).Trim(' ', '-');
            }
        }
    }

    public class PersonaleLookupItem
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Cognome { get; set; }
        public bool IsEsterno { get; set; }
        public bool IsAttivo { get; set; }

        public string DisplayName
        {
            get
            {
                var displayName = string.Format("{0} {1}", Cognome, Nome).Trim();
                return IsAttivo ? displayName : displayName + " [non attivo]";
            }
        }
    }

    public class ProdottoSelezioneItem
    {
        public int IdProdotto { get; set; }
        public int? Categorico { get; set; }
        public string DescrizioneProdotto { get; set; }
        public string Matricola { get; set; }
        public string Categoria { get; set; }
        public string StatoCorrente { get; set; }

        public string DescrizioneSelezione
        {
            get
            {
                return string.Format("{0} - {1} - {2}", Categorico.HasValue ? Categorico.Value.ToString() : "-", DescrizioneProdotto, Matricola).Trim();
            }
        }
    }

    public class DominiViewModel
    {
        public IList<DominioItem> Categorie { get; set; }
        public IList<DominioItem> LivelliEfficienza { get; set; }
        public IList<DominioItem> Stanze { get; set; }
        public IList<DominioItem> Ditte { get; set; }
        public IList<DominioItem> TipiOggettoOrdinativo { get; set; }

        public DominiViewModel()
        {
            Categorie = new List<DominioItem>();
            LivelliEfficienza = new List<DominioItem>();
            Stanze = new List<DominioItem>();
            Ditte = new List<DominioItem>();
            TipiOggettoOrdinativo = new List<DominioItem>();
        }
    }
}
