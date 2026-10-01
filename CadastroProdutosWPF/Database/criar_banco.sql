-- ============================================================
-- Cadastro de Produtos - SQL Server / LocalDB
-- ============================================================

IF DB_ID('CadastroProdutosDb') IS NULL
BEGIN
    CREATE DATABASE CadastroProdutosDb;
END
GO

USE CadastroProdutosDb;
GO

IF OBJECT_ID('dbo.Produtos', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Produtos;
END
GO

CREATE TABLE dbo.Produtos
(
    Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Nome NVARCHAR(150) NOT NULL,
    Preco DECIMAL(18,2) NOT NULL CHECK (Preco >= 0),
    Estoque INT NOT NULL CHECK (Estoque >= 0),
    Categoria NVARCHAR(100) NOT NULL
);
GO

-- Dados opcionais para facilitar a demonstração.
INSERT INTO dbo.Produtos (Nome, Preco, Estoque, Categoria)
VALUES
    ('Mouse Gamer', 129.90, 15, 'Periféricos'),
    ('Teclado Mecânico', 299.90, 8, 'Periféricos'),
    ('Monitor 24 Polegadas', 899.00, 5, 'Monitores');
GO

SELECT Id, Nome, Preco, Estoque, Categoria
FROM dbo.Produtos
ORDER BY Id;
GO
