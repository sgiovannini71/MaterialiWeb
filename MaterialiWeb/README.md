# MaterialiGestioneWeb

Web Application ASP.NET Web Forms (`.NET Framework 4.7.2`) per Visual Studio 2019, allineata al database `Materiale`.

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
- `DettaglioOrdinativo.aspx`: vista gerarchica `Ordinativo -> OggettiOrdinativo -> Prodotti`
- `NuovoBene.aspx`: completamento di un prodotto gia' generato da oggetto ordinativo
- `AssegnaBene.aspx`: assegnazione
- `RientroRiassegnazione.aspx`: chiusura/nuova assegnazione
- `CambiaStato.aspx`: cambio livello efficienza
- `CambiaUbicazione.aspx`: cambio stanza
- `ConfiguraComputer.aspx`: MAC e nome macchina
- `DismettiBene.aspx`: versamento/dismissione
- `ProdottiAssegnati.aspx`: prodotti correnti per assegnatario, export CSV, anteprima A4 e scheda PDF
- `StoricoAssegnazioni.aspx`: consultazione storico con export CSV
- `Domini.aspx`: categorie, livelli, stanze e ditte

## Configurazione

Aggiornare in `Web.config`:

- `MaterialiDb`
- `DipendentiDBConnectionString`
- `AssignmentSheetHeaderText`
- `AssignmentSheetLogoPath`
- `AssignmentSheetTemplatePath`

I log applicativi vengono scritti in `App_Data/Logs`.

## Regole funzionali rilevanti

- I `Prodotti` nascono dalla creazione di `OggettoOrdinativo` tramite `Quantita`.
- Il `Categorico` viene assegnato in quella fase e non piu' tramite `NuovoBene`.
- `NuovoBene.aspx` completa solo i dati mancanti di prodotti gia' generati.
- In `AssegnaBene.aspx` sono selezionabili solo beni non assegnati.
- I pulsanti di export/preview si abilitano solo dopo un caricamento dati con risultati.
