# MaterialiGestioneWeb

Web Application ASP.NET Web Forms (`.NET Framework 4.7.2`) per Visual Studio 2019, allineata al database `Materiali`.

## Ambito dati

Il progetto usa direttamente queste tabelle principali:

- `Prodotti`
- `OggettoOrdinativo`
- `Ordinativo`
- `CategoriaProdotti`
- `LivelliEfficenza`
- `Stanze`
- `ProdPers`
- `ProdPersStorico`
- `NetworkData`
- `Postazione`
- `NomeMacchina`
- `Ditte`
- `TipoOggettoOrdinativo`

Per l'assegnazione utenti usa il database esterno `DipendentiDB`:

- interni: `dbo.elencopersonale` (`idpersonale`)
- esterni: `dbo.pe_elencopersonaleesterno` (`id_pe`)

Default applicativo:

- personale `Interno`
- solo personale `attivo`

## Pagine principali

- `Default.aspx`: dashboard
- `Prodotti.aspx`: elenco materiali
- `Computer.aspx`: rete e postazioni
- `ProdottoDettaglio.aspx`: scheda materiale
- `NuovoBene.aspx`: inserimento materiale
- `AssegnaBene.aspx`: assegnazione
- `RientroRiassegnazione.aspx`: chiusura/nuova assegnazione
- `CambiaStato.aspx`: cambio livello efficienza
- `CambiaUbicazione.aspx`: cambio stanza
- `ConfiguraComputer.aspx`: MAC e nome macchina
- `DismettiBene.aspx`: versamento/dismissione
- `Domini.aspx`: categorie, livelli, stanze e ditte

## Configurazione

Aggiornare in `Web.config`:

- `MaterialiDb`
- `DipendentiDBConnectionString`

I log applicativi vengono scritti in `App_Data/Logs`.
