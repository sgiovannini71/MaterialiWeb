/*
    Seed demo per database Materiali
    Lotto: DEMO_MATERIALI_WEB_20260430

    Inserisce:
    - tipi oggetto ordinativo
    - categorie prodotti
    - ditte
    - livelli efficienza
    - stanze
    - ordinativi
    - oggetti ordinativo
    - prodotti
    - assegnazioni correnti e storico
    - network data
    - nome macchina / postazione

    Gli ID inseriti vengono registrati in dbo.ScriptSeedLotto e dbo.ScriptSeedItem
    per consentire il cleanup puntuale.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

IF OBJECT_ID(N'dbo.ScriptSeedLotto', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ScriptSeedLotto
    (
        IdSeedLotto INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CodiceLotto VARCHAR(100) NOT NULL UNIQUE,
        Descrizione VARCHAR(300) NULL,
        DataCreazione DATETIME2(0) NOT NULL CONSTRAINT DF_ScriptSeedLotto_DataCreazione DEFAULT SYSUTCDATETIME()
    );
END;
GO

IF OBJECT_ID(N'dbo.ScriptSeedItem', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ScriptSeedItem
    (
        IdSeedItem INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        IdSeedLotto INT NOT NULL,
        NomeTabella VARCHAR(128) NOT NULL,
        IdRecord INT NOT NULL,
        Note VARCHAR(200) NULL,
        CONSTRAINT FK_ScriptSeedItem_ScriptSeedLotto
            FOREIGN KEY (IdSeedLotto) REFERENCES dbo.ScriptSeedLotto(IdSeedLotto)
    );

    CREATE INDEX IX_ScriptSeedItem_Lotto_Tabella
        ON dbo.ScriptSeedItem(IdSeedLotto, NomeTabella);
END;
GO

BEGIN TRANSACTION;
BEGIN TRY
    DECLARE @CodiceLotto VARCHAR(100) = 'DEMO_MATERIALI_WEB_20260430';
    DECLARE @IdSeedLotto INT;

    IF EXISTS (SELECT 1 FROM dbo.ScriptSeedLotto WHERE CodiceLotto = @CodiceLotto)
    BEGIN
        THROW 51000, 'Il lotto demo DEMO_MATERIALI_WEB_20260430 esiste gia. Eseguire prima lo script di cleanup.', 1;
    END;

    INSERT INTO dbo.ScriptSeedLotto (CodiceLotto, Descrizione)
    VALUES (@CodiceLotto, 'Dataset demo per MaterialiGestioneWeb');

    SET @IdSeedLotto = SCOPE_IDENTITY();

    DECLARE @IdTipoOO_PC INT;
    DECLARE @IdTipoOO_PERIF INT;
    DECLARE @IdCategoriaNotebook INT;
    DECLARE @IdCategoriaDesktop INT;
    DECLARE @IdCategoriaMonitor INT;
    DECLARE @IdCategoriaStampante INT;
    DECLARE @IdDittaDell INT;
    DECLARE @IdDittaHP INT;
    DECLARE @IdDittaBrother INT;
    DECLARE @IdEffMagazzino INT;
    DECLARE @IdEffUso INT;
    DECLARE @IdEffVersare INT;
    DECLARE @IdStanzaLab INT;
    DECLARE @IdStanzaUff101 INT;
    DECLARE @IdStanzaMagazzino INT;
    DECLARE @IdOrdNotebook INT;
    DECLARE @IdOrdDesktop INT;
    DECLARE @IdOrdMonitor INT;
    DECLARE @IdOggNotebook INT;
    DECLARE @IdOggDesktop INT;
    DECLARE @IdOggMonitor INT;
    DECLARE @IdOggStampante INT;
    DECLARE @IdProdNotebook1 INT;
    DECLARE @IdProdNotebook2 INT;
    DECLARE @IdProdDesktop1 INT;
    DECLARE @IdProdMonitor1 INT;
    DECLARE @IdProdStampante1 INT;
    DECLARE @IdProdPersNotebook1 INT;
    DECLARE @IdProdPersDesktop1 INT;
    DECLARE @IdNomeMacchinaNotebook1 INT;
    DECLARE @IdNomeMacchinaDesktop1 INT;

    INSERT INTO dbo.TipoOggettoOrdinativo (idTipoOggOrdinativo, Descrizione)
    SELECT v.idTipoOggOrdinativo, v.Descrizione
    FROM (VALUES
        (9101, 'Postazione informatica'),
        (9102, 'Periferica')
    ) v(idTipoOggOrdinativo, Descrizione)
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.TipoOggettoOrdinativo t
        WHERE t.idTipoOggOrdinativo = v.idTipoOggOrdinativo
    );

    SET @IdTipoOO_PC = 9101;
    SET @IdTipoOO_PERIF = 9102;

    INSERT INTO dbo.CategoriaProdotti (Descrizione, idTipoOO, ethernet)
    VALUES ('Notebook demo', @IdTipoOO_PC, 1);
    SET @IdCategoriaNotebook = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'CategoriaProdotti', @IdCategoriaNotebook, 'Notebook demo');

    INSERT INTO dbo.CategoriaProdotti (Descrizione, idTipoOO, ethernet)
    VALUES ('Desktop demo', @IdTipoOO_PC, 1);
    SET @IdCategoriaDesktop = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'CategoriaProdotti', @IdCategoriaDesktop, 'Desktop demo');

    INSERT INTO dbo.CategoriaProdotti (Descrizione, idTipoOO, ethernet)
    VALUES ('Monitor demo', @IdTipoOO_PERIF, 0);
    SET @IdCategoriaMonitor = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'CategoriaProdotti', @IdCategoriaMonitor, 'Monitor demo');

    INSERT INTO dbo.CategoriaProdotti (Descrizione, idTipoOO, ethernet)
    VALUES ('Stampante demo', @IdTipoOO_PERIF, 1);
    SET @IdCategoriaStampante = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'CategoriaProdotti', @IdCategoriaStampante, 'Stampante demo');

    INSERT INTO dbo.Ditte (Nome, Citta, nazione, tipologia, Mail)
    VALUES ('Dell Demo Srl', 'Roma', 'Italia', 'F', 'supporto@dell-demo.local');
    SET @IdDittaDell = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'Ditte', @IdDittaDell, 'Dell demo');

    INSERT INTO dbo.Ditte (Nome, Citta, nazione, tipologia, Mail)
    VALUES ('HP Demo Srl', 'Milano', 'Italia', 'F', 'supporto@hp-demo.local');
    SET @IdDittaHP = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'Ditte', @IdDittaHP, 'HP demo');

    INSERT INTO dbo.Ditte (Nome, Citta, nazione, tipologia, Mail)
    VALUES ('Brother Demo Srl', 'Torino', 'Italia', 'F', 'supporto@brother-demo.local');
    SET @IdDittaBrother = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'Ditte', @IdDittaBrother, 'Brother demo');

    INSERT INTO dbo.LivelliEfficenza (Livello_efficienza, Codice)
    VALUES ('Magazzino demo', 'MAGD');
    SET @IdEffMagazzino = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'LivelliEfficenza', @IdEffMagazzino, 'Magazzino demo');

    INSERT INTO dbo.LivelliEfficenza (Livello_efficienza, Codice)
    VALUES ('In uso demo', 'USOD');
    SET @IdEffUso = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'LivelliEfficenza', @IdEffUso, 'In uso demo');

    INSERT INTO dbo.LivelliEfficenza (Livello_efficienza, Codice)
    VALUES ('Da versare demo', 'VERD');
    SET @IdEffVersare = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'LivelliEfficenza', @IdEffVersare, 'Da versare demo');

    INSERT INTO dbo.Stanze (numero) VALUES ('LAB-DEMO');
    SET @IdStanzaLab = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'Stanze', @IdStanzaLab, 'LAB-DEMO');

    INSERT INTO dbo.Stanze (numero) VALUES ('UFF-101');
    SET @IdStanzaUff101 = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'Stanze', @IdStanzaUff101, 'UFF-101');

    INSERT INTO dbo.Stanze (numero) VALUES ('MAG-01');
    SET @IdStanzaMagazzino = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'Stanze', @IdStanzaMagazzino, 'MAG-01');

    INSERT INTO dbo.Ordinativo
    (
        CodiceOrdinativo, denominazioneOrdinativo, EF, tipoOrdinativo,
        idDittaOrdinativo, enteStipulante, estremiOrdinativo, DataRepertorio,
        dataDecorrenza, importoTotaleNetto, importoTotaleLordo
    )
    VALUES
    (
        'ORD-DEMO-NB', 'Fornitura notebook demo', '2026', 'ODA',
        @IdDittaDell, 'Servizio ICT', 'REP/DEMO/001', '2026-01-15',
        '2026-01-20', 3200.00, 3904.00
    );
    SET @IdOrdNotebook = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'Ordinativo', @IdOrdNotebook, 'Notebook');

    INSERT INTO dbo.Ordinativo
    (
        CodiceOrdinativo, denominazioneOrdinativo, EF, tipoOrdinativo,
        idDittaOrdinativo, enteStipulante, estremiOrdinativo, DataRepertorio,
        dataDecorrenza, importoTotaleNetto, importoTotaleLordo
    )
    VALUES
    (
        'ORD-DEMO-PC', 'Fornitura desktop demo', '2026', 'ODA',
        @IdDittaHP, 'Servizio ICT', 'REP/DEMO/002', '2026-02-10',
        '2026-02-14', 2400.00, 2928.00
    );
    SET @IdOrdDesktop = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'Ordinativo', @IdOrdDesktop, 'Desktop');

    INSERT INTO dbo.Ordinativo
    (
        CodiceOrdinativo, denominazioneOrdinativo, EF, tipoOrdinativo,
        idDittaOrdinativo, enteStipulante, estremiOrdinativo, DataRepertorio,
        dataDecorrenza, importoTotaleNetto, importoTotaleLordo
    )
    VALUES
    (
        'ORD-DEMO-MON', 'Fornitura monitor demo', '2026', 'ODA',
        @IdDittaHP, 'Servizio ICT', 'REP/DEMO/003', '2026-03-02',
        '2026-03-04', 900.00, 1098.00
    );
    SET @IdOrdMonitor = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'Ordinativo', @IdOrdMonitor, 'Monitor');

    INSERT INTO dbo.OggettoOrdinativo
    (
        idOrdinativo, descrizioneProdotto, specificheTecniche, idDittaCostruttrice, modello,
        partNumberDitta, NUC, unitaDiMisura, quantita, prezzoUnitarioNetto, prezzoInventario,
        note, durataGaranzia, inizioGaranzia, enteCaricomateriale, iva, tempCodOggOrdinativo, idCategProdotti
    )
    VALUES
    (
        @IdOrdNotebook, 'Notebook Dell Latitude 7440 demo', 'Core i7, 16GB RAM, SSD 512GB',
        @IdDittaDell, 'Latitude 7440', 'DL-7440-DEMO', 'NUC-NB-001', 'PZ', 2, 1600.00, 1600.00,
        'Batch demo notebook', 36, '2026-01-20', 'ICT', 22, 'TMP-NB-001', @IdCategoriaNotebook
    );
    SET @IdOggNotebook = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'OggettoOrdinativo', @IdOggNotebook, 'Notebook');

    INSERT INTO dbo.OggettoOrdinativo
    (
        idOrdinativo, descrizioneProdotto, specificheTecniche, idDittaCostruttrice, modello,
        partNumberDitta, NUC, unitaDiMisura, quantita, prezzoUnitarioNetto, prezzoInventario,
        note, durataGaranzia, inizioGaranzia, enteCaricomateriale, iva, tempCodOggOrdinativo, idCategProdotti
    )
    VALUES
    (
        @IdOrdDesktop, 'Desktop HP EliteDesk 800 G6 demo', 'Core i5, 16GB RAM, SSD 256GB',
        @IdDittaHP, 'EliteDesk 800 G6', 'HP-800G6-DEMO', 'NUC-PC-001', 'PZ', 1, 2400.00, 2400.00,
        'Postazione demo desktop', 24, '2026-02-14', 'ICT', 22, 'TMP-PC-001', @IdCategoriaDesktop
    );
    SET @IdOggDesktop = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'OggettoOrdinativo', @IdOggDesktop, 'Desktop');

    INSERT INTO dbo.OggettoOrdinativo
    (
        idOrdinativo, descrizioneProdotto, specificheTecniche, idDittaCostruttrice, modello,
        partNumberDitta, NUC, unitaDiMisura, quantita, prezzoUnitarioNetto, prezzoInventario,
        note, durataGaranzia, inizioGaranzia, enteCaricomateriale, iva, tempCodOggOrdinativo, idCategProdotti
    )
    VALUES
    (
        @IdOrdMonitor, 'Monitor HP 24 demo', 'Monitor 24 pollici Full HD',
        @IdDittaHP, 'E24 G5', 'HP-MON-DEMO', 'NUC-MON-001', 'PZ', 1, 900.00, 900.00,
        'Monitor demo', 24, '2026-03-04', 'ICT', 22, 'TMP-MON-001', @IdCategoriaMonitor
    );
    SET @IdOggMonitor = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'OggettoOrdinativo', @IdOggMonitor, 'Monitor');

    INSERT INTO dbo.OggettoOrdinativo
    (
        idOrdinativo, descrizioneProdotto, specificheTecniche, idDittaCostruttrice, modello,
        partNumberDitta, NUC, unitaDiMisura, quantita, prezzoUnitarioNetto, prezzoInventario,
        note, durataGaranzia, inizioGaranzia, enteCaricomateriale, iva, tempCodOggOrdinativo, idCategProdotti
    )
    VALUES
    (
        NULL, 'Stampante Brother demo', 'Laser monocromatica con rete',
        @IdDittaBrother, 'HL-L6400DW', 'BR-6400-DEMO', 'NUC-PRN-001', 'PZ', 1, 420.00, 420.00,
        'Stampante demo di reparto', 12, '2026-03-20', 'ICT', 22, 'TMP-PRN-001', @IdCategoriaStampante
    );
    SET @IdOggStampante = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'OggettoOrdinativo', @IdOggStampante, 'Stampante');

    INSERT INTO dbo.Prodotti (idStanza, idOggOrdinativo, Categorico, Matricola, IdEfficienza, DataUltimaMov, Note, Versamento)
    VALUES (@IdStanzaUff101, @IdOggNotebook, 700001, 'SN-NB-DEMO-001', @IdEffUso, GETDATE(), N'Notebook assegnato a utente interno', NULL);
    SET @IdProdNotebook1 = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'Prodotti', @IdProdNotebook1, 'Notebook 1');

    INSERT INTO dbo.Prodotti (idStanza, idOggOrdinativo, Categorico, Matricola, IdEfficienza, DataUltimaMov, Note, Versamento)
    VALUES (@IdStanzaMagazzino, @IdOggNotebook, 700002, 'SN-NB-DEMO-002', @IdEffMagazzino, GETDATE(), N'Notebook disponibile a magazzino', NULL);
    SET @IdProdNotebook2 = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'Prodotti', @IdProdNotebook2, 'Notebook 2');

    INSERT INTO dbo.Prodotti (idStanza, idOggOrdinativo, Categorico, Matricola, IdEfficienza, DataUltimaMov, Note, Versamento)
    VALUES (@IdStanzaLab, @IdOggDesktop, 700003, 'SN-PC-DEMO-001', @IdEffUso, GETDATE(), N'Desktop di laboratorio', NULL);
    SET @IdProdDesktop1 = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'Prodotti', @IdProdDesktop1, 'Desktop 1');

    INSERT INTO dbo.Prodotti (idStanza, idOggOrdinativo, Categorico, Matricola, IdEfficienza, DataUltimaMov, Note, Versamento)
    VALUES (@IdStanzaUff101, @IdOggMonitor, 700004, 'SN-MON-DEMO-001', @IdEffUso, GETDATE(), N'Monitor collegato a notebook demo', NULL);
    SET @IdProdMonitor1 = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'Prodotti', @IdProdMonitor1, 'Monitor 1');

    INSERT INTO dbo.Prodotti (idStanza, idOggOrdinativo, Categorico, Matricola, IdEfficienza, DataUltimaMov, Note, Versamento)
    VALUES (@IdStanzaLab, @IdOggStampante, 700005, 'SN-PRN-DEMO-001', @IdEffVersare, GETDATE(), N'Stampante da versare', N'VERB-2026-PRN');
    SET @IdProdStampante1 = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'Prodotti', @IdProdStampante1, 'Stampante 1');

    INSERT INTO dbo.ProdPers (IdProdotto, IdPersonale, DataAssegnazione)
    VALUES (@IdProdNotebook1, 101, '2026-03-18');
    SET @IdProdPersNotebook1 = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'ProdPers', @IdProdPersNotebook1, 'Assegnazione notebook 1');

    INSERT INTO dbo.ProdPers (IdProdotto, IdPersonale, DataAssegnazione)
    VALUES (@IdProdDesktop1, 205, '2026-03-25');
    SET @IdProdPersDesktop1 = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'ProdPers', @IdProdPersDesktop1, 'Assegnazione desktop 1');

    INSERT INTO dbo.ProdPersStorico
    (
        idProdPers, idProdotto, idPersonale, dataAssegnazione, dataRestituzione,
        numeroStanza, livelloEfficienza, noteProdotto, nomeMacchina, serialNumber
    )
    VALUES
    (
        @IdProdPersNotebook1, @IdProdNotebook1, 101, '2026-03-18', NULL,
        'UFF-101', 'In uso demo', N'Assegnazione corrente demo notebook', N'NB-DEMO-001', N'SN-NB-DEMO-001'
    );
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note)
    VALUES (@IdSeedLotto, 'ProdPersStorico', SCOPE_IDENTITY(), 'Storico notebook 1');

    INSERT INTO dbo.ProdPersStorico
    (
        idProdPers, idProdotto, idPersonale, dataAssegnazione, dataRestituzione,
        numeroStanza, livelloEfficienza, noteProdotto, nomeMacchina, serialNumber
    )
    VALUES
    (
        @IdProdPersDesktop1, @IdProdDesktop1, 205, '2026-03-25', NULL,
        'LAB-DEMO', 'In uso demo', N'Assegnazione corrente demo desktop', N'PC-LAB-01', N'SN-PC-DEMO-001'
    );
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note)
    VALUES (@IdSeedLotto, 'ProdPersStorico', SCOPE_IDENTITY(), 'Storico desktop 1');

    INSERT INTO dbo.ProdPersStorico
    (
        idProdPers, idProdotto, idPersonale, dataAssegnazione, dataRestituzione,
        numeroStanza, livelloEfficienza, noteProdotto, nomeMacchina, serialNumber
    )
    VALUES
    (
        NULL, @IdProdStampante1, 9999, '2026-01-10', '2026-04-01',
        'LAB-DEMO', 'Da versare demo', N'Assegnazione storica chiusa stampante', N'PRN-DEMO-01', N'SN-PRN-DEMO-001'
    );
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note)
    VALUES (@IdSeedLotto, 'ProdPersStorico', SCOPE_IDENTITY(), 'Storico stampante chiuso');

    INSERT INTO dbo.NetworkData (idProdotto, macaddress, note)
    VALUES (@IdProdNotebook1, 'AA:11:22:33:44:01', 'Dock utente ufficio');
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note)
    VALUES (@IdSeedLotto, 'NetworkData', SCOPE_IDENTITY(), 'Rete notebook 1');

    INSERT INTO dbo.NetworkData (idProdotto, macaddress, note)
    VALUES (@IdProdDesktop1, 'AA:11:22:33:44:02', 'Desktop laboratorio');
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note)
    VALUES (@IdSeedLotto, 'NetworkData', SCOPE_IDENTITY(), 'Rete desktop 1');

    INSERT INTO dbo.NetworkData (idProdotto, macaddress, note)
    VALUES (@IdProdStampante1, 'AA:11:22:33:44:03', 'Stampante da reparto');
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note)
    VALUES (@IdSeedLotto, 'NetworkData', SCOPE_IDENTITY(), 'Rete stampante 1');

    INSERT INTO dbo.NomeMacchina (NomeMacchina) VALUES ('NB-DEMO-001');
    SET @IdNomeMacchinaNotebook1 = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'NomeMacchina', @IdNomeMacchinaNotebook1, 'Nome macchina notebook');

    INSERT INTO dbo.NomeMacchina (NomeMacchina) VALUES ('PC-LAB-01');
    SET @IdNomeMacchinaDesktop1 = SCOPE_IDENTITY();
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note) VALUES (@IdSeedLotto, 'NomeMacchina', @IdNomeMacchinaDesktop1, 'Nome macchina desktop');

    INSERT INTO dbo.Postazione (idNomeMacchina, idProdotto)
    VALUES (@IdNomeMacchinaNotebook1, @IdProdNotebook1);
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note)
    VALUES (@IdSeedLotto, 'Postazione', SCOPE_IDENTITY(), 'Postazione notebook 1');

    INSERT INTO dbo.Postazione (idNomeMacchina, idProdotto)
    VALUES (@IdNomeMacchinaDesktop1, @IdProdDesktop1);
    INSERT INTO dbo.ScriptSeedItem (IdSeedLotto, NomeTabella, IdRecord, Note)
    VALUES (@IdSeedLotto, 'Postazione', SCOPE_IDENTITY(), 'Postazione desktop 1');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
