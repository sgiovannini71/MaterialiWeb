IF OBJECT_ID(N'dbo.AutorizzazioniMateriali', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.AutorizzazioniMateriali
    (
        Id INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_AutorizzazioniMateriali PRIMARY KEY,
        Username NVARCHAR(256) NOT NULL,
        Livello INT NOT NULL,
        Attivo BIT NOT NULL
            CONSTRAINT DF_AutorizzazioniMateriali_Attivo DEFAULT (1),
        Note NVARCHAR(500) NULL,
        CreatedAt DATETIME2(0) NOT NULL
            CONSTRAINT DF_AutorizzazioniMateriali_CreatedAt DEFAULT (SYSUTCDATETIME()),
        UpdatedAt DATETIME2(0) NULL
    );

    CREATE UNIQUE INDEX UX_AutorizzazioniMateriali_Username
        ON dbo.AutorizzazioniMateriali (Username);

    CREATE INDEX IX_AutorizzazioniMateriali_Attivo_Livello
        ON dbo.AutorizzazioniMateriali (Attivo, Livello);
END;
GO

/*
Livelli applicativi:
100 = Visualizzatore
150 = Operatore
200 = Amministratore

Esempi:
INSERT INTO dbo.AutorizzazioniMateriali (Username, Livello, Note)
VALUES (N'DOMINIO\utente', 200, N'Amministratore applicazione Materiali');

INSERT INTO dbo.AutorizzazioniMateriali (Username, Livello, Note)
VALUES (N'utente.senza.dominio', 150, N'Operatore materiali');
*/
