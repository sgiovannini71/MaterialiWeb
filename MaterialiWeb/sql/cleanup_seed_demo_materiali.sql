/*
    Cleanup dataset demo MaterialiGestioneWeb
    Lotto: DEMO_MATERIALI_WEB_20260430

    Rimuove solo i record registrati dal seed demo.
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

BEGIN TRANSACTION;
BEGIN TRY
    DECLARE @CodiceLotto VARCHAR(100) = 'DEMO_MATERIALI_WEB_20260430';
    DECLARE @IdSeedLotto INT;

    SELECT @IdSeedLotto = IdSeedLotto
    FROM dbo.ScriptSeedLotto
    WHERE CodiceLotto = @CodiceLotto;

    IF @IdSeedLotto IS NULL
    BEGIN
        PRINT 'Nessun lotto demo da rimuovere.';
        COMMIT TRANSACTION;
        RETURN;
    END;

    DECLARE @Prodotti TABLE (Id INT PRIMARY KEY);
    DECLARE @ProdPers TABLE (Id INT PRIMARY KEY);
    DECLARE @Storico TABLE (Id INT PRIMARY KEY);
    DECLARE @Postazioni TABLE (Id INT PRIMARY KEY);
    DECLARE @Network TABLE (Id INT PRIMARY KEY);
    DECLARE @NomiMacchina TABLE (Id INT PRIMARY KEY);
    DECLARE @Oggetti TABLE (Id INT PRIMARY KEY);
    DECLARE @Ordinativi TABLE (Id INT PRIMARY KEY);
    DECLARE @Stanze TABLE (Id INT PRIMARY KEY);
    DECLARE @Livelli TABLE (Id INT PRIMARY KEY);
    DECLARE @Ditte TABLE (Id INT PRIMARY KEY);
    DECLARE @Categorie TABLE (Id INT PRIMARY KEY);

    INSERT INTO @Prodotti (Id)
    SELECT IdRecord FROM dbo.ScriptSeedItem WHERE IdSeedLotto = @IdSeedLotto AND NomeTabella = 'Prodotti';

    INSERT INTO @ProdPers (Id)
    SELECT IdRecord FROM dbo.ScriptSeedItem WHERE IdSeedLotto = @IdSeedLotto AND NomeTabella = 'ProdPers';

    INSERT INTO @Storico (Id)
    SELECT IdRecord FROM dbo.ScriptSeedItem WHERE IdSeedLotto = @IdSeedLotto AND NomeTabella = 'ProdPersStorico';

    INSERT INTO @Postazioni (Id)
    SELECT IdRecord FROM dbo.ScriptSeedItem WHERE IdSeedLotto = @IdSeedLotto AND NomeTabella = 'Postazione';

    INSERT INTO @Network (Id)
    SELECT IdRecord FROM dbo.ScriptSeedItem WHERE IdSeedLotto = @IdSeedLotto AND NomeTabella = 'NetworkData';

    INSERT INTO @NomiMacchina (Id)
    SELECT IdRecord FROM dbo.ScriptSeedItem WHERE IdSeedLotto = @IdSeedLotto AND NomeTabella = 'NomeMacchina';

    INSERT INTO @Oggetti (Id)
    SELECT IdRecord FROM dbo.ScriptSeedItem WHERE IdSeedLotto = @IdSeedLotto AND NomeTabella = 'OggettoOrdinativo';

    INSERT INTO @Ordinativi (Id)
    SELECT IdRecord FROM dbo.ScriptSeedItem WHERE IdSeedLotto = @IdSeedLotto AND NomeTabella = 'Ordinativo';

    INSERT INTO @Stanze (Id)
    SELECT IdRecord FROM dbo.ScriptSeedItem WHERE IdSeedLotto = @IdSeedLotto AND NomeTabella = 'Stanze';

    INSERT INTO @Livelli (Id)
    SELECT IdRecord FROM dbo.ScriptSeedItem WHERE IdSeedLotto = @IdSeedLotto AND NomeTabella = 'LivelliEfficenza';

    INSERT INTO @Ditte (Id)
    SELECT IdRecord FROM dbo.ScriptSeedItem WHERE IdSeedLotto = @IdSeedLotto AND NomeTabella = 'Ditte';

    INSERT INTO @Categorie (Id)
    SELECT IdRecord FROM dbo.ScriptSeedItem WHERE IdSeedLotto = @IdSeedLotto AND NomeTabella = 'CategoriaProdotti';

    DELETE psh
    FROM dbo.ProdPersStorico psh
    INNER JOIN @Storico s ON s.Id = psh.id;

    DELETE pp
    FROM dbo.ProdPers pp
    INNER JOIN @ProdPers p ON p.Id = pp.IdProdPers;

    DELETE po
    FROM dbo.Postazione po
    INNER JOIN @Postazioni p ON p.Id = po.idPostazione;

    DELETE nd
    FROM dbo.NetworkData nd
    INNER JOIN @Network n ON n.Id = nd.idNetworkData;

    DELETE p
    FROM dbo.Prodotti p
    INNER JOIN @Prodotti x ON x.Id = p.IdProdotto;

    DELETE nm
    FROM dbo.NomeMacchina nm
    INNER JOIN @NomiMacchina x ON x.Id = nm.idnomemacchina;

    DELETE oo
    FROM dbo.OggettoOrdinativo oo
    INNER JOIN @Oggetti x ON x.Id = oo.idOggOrdinativo;

    DELETE o
    FROM dbo.Ordinativo o
    INNER JOIN @Ordinativi x ON x.Id = o.idOrdinativo;

    DELETE s
    FROM dbo.Stanze s
    INNER JOIN @Stanze x ON x.Id = s.idstanza;

    DELETE le
    FROM dbo.LivelliEfficenza le
    INNER JOIN @Livelli x ON x.Id = le.IdEfficienza;

    DELETE d
    FROM dbo.Ditte d
    INNER JOIN @Ditte x ON x.Id = d.IdDitta;

    DELETE c
    FROM dbo.CategoriaProdotti c
    INNER JOIN @Categorie x ON x.Id = c.IdCategoria;

    DELETE FROM dbo.ScriptSeedItem WHERE IdSeedLotto = @IdSeedLotto;
    DELETE FROM dbo.ScriptSeedLotto WHERE IdSeedLotto = @IdSeedLotto;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;
GO
