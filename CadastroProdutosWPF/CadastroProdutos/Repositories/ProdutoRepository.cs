using CadastroProdutos.Models;
using CadastroProdutos.Services;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CadastroProdutos.Repositories;

public class ProdutoRepository
{
    private readonly string _connectionString;

    public ProdutoRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void Inserir(Produto produto)
    {
        const string sql = @"
            INSERT INTO Produtos (Nome, Preco, Estoque, Categoria)
            VALUES (@Nome, @Preco, @Estoque, @Categoria);";

        try
        {
            using var conexao = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(sql, conexao);

            comando.Parameters.Add("@Nome", SqlDbType.NVarChar, 150).Value = produto.Nome;
            comando.Parameters.Add("@Preco", SqlDbType.Decimal).Value = produto.Preco;
            comando.Parameters["@Preco"].Precision = 18;
            comando.Parameters["@Preco"].Scale = 2;
            comando.Parameters.Add("@Estoque", SqlDbType.Int).Value = produto.Estoque;
            comando.Parameters.Add("@Categoria", SqlDbType.NVarChar, 100).Value = produto.Categoria;

            conexao.Open();
            comando.ExecuteNonQuery();

            ArquivoLogger.Registrar($"INSERT | Produto '{produto.Nome}' cadastrado.");
        }
        catch (SqlException ex)
        {
            ArquivoLogger.Registrar($"ERRO SQL INSERT | {ex.Message}");
            throw new InvalidOperationException("Erro ao inserir o produto no banco de dados.", ex);
        }
    }

    public List<Produto> Listar()
    {
        const string sql = @"
            SELECT Id, Nome, Preco, Estoque, Categoria
            FROM Produtos
            ORDER BY Id;";

        var produtos = new List<Produto>();

        try
        {
            using var conexao = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(sql, conexao);

            conexao.Open();

            using var reader = comando.ExecuteReader();
            while (reader.Read())
            {
                produtos.Add(MapearProduto(reader));
            }

            ArquivoLogger.Registrar($"SELECT LISTAR | {produtos.Count} produto(s) retornado(s).");
            return produtos;
        }
        catch (SqlException ex)
        {
            ArquivoLogger.Registrar($"ERRO SQL LISTAR | {ex.Message}");
            throw new InvalidOperationException("Erro ao listar os produtos do banco de dados.", ex);
        }
    }

    public Produto? BuscarPorId(int id)
    {
        const string sql = @"
            SELECT Id, Nome, Preco, Estoque, Categoria
            FROM Produtos
            WHERE Id = @Id;";

        try
        {
            using var conexao = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(sql, conexao);

            comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;

            conexao.Open();

            using var reader = comando.ExecuteReader();
            Produto? produto = null;

            if (reader.Read())
            {
                produto = MapearProduto(reader);
            }

            ArquivoLogger.Registrar(
                produto is null
                    ? $"SELECT ID | Produto {id} não encontrado."
                    : $"SELECT ID | Produto {id} encontrado.");

            return produto;
        }
        catch (SqlException ex)
        {
            ArquivoLogger.Registrar($"ERRO SQL BUSCAR ID {id} | {ex.Message}");
            throw new InvalidOperationException("Erro ao buscar o produto no banco de dados.", ex);
        }
    }

    public bool Atualizar(Produto produto)
    {
        const string sql = @"
            UPDATE Produtos
            SET Nome = @Nome,
                Preco = @Preco,
                Estoque = @Estoque,
                Categoria = @Categoria
            WHERE Id = @Id;";

        try
        {
            using var conexao = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(sql, conexao);

            comando.Parameters.Add("@Id", SqlDbType.Int).Value = produto.Id;
            comando.Parameters.Add("@Nome", SqlDbType.NVarChar, 150).Value = produto.Nome;
            comando.Parameters.Add("@Preco", SqlDbType.Decimal).Value = produto.Preco;
            comando.Parameters["@Preco"].Precision = 18;
            comando.Parameters["@Preco"].Scale = 2;
            comando.Parameters.Add("@Estoque", SqlDbType.Int).Value = produto.Estoque;
            comando.Parameters.Add("@Categoria", SqlDbType.NVarChar, 100).Value = produto.Categoria;

            conexao.Open();
            var linhasAfetadas = comando.ExecuteNonQuery();

            ArquivoLogger.Registrar($"UPDATE | Produto {produto.Id} | Linhas afetadas: {linhasAfetadas}.");
            return linhasAfetadas > 0;
        }
        catch (SqlException ex)
        {
            ArquivoLogger.Registrar($"ERRO SQL UPDATE ID {produto.Id} | {ex.Message}");
            throw new InvalidOperationException("Erro ao atualizar o produto no banco de dados.", ex);
        }
    }

    public bool Excluir(int id)
    {
        const string sql = "DELETE FROM Produtos WHERE Id = @Id;";

        try
        {
            using var conexao = new SqlConnection(_connectionString);
            using var comando = new SqlCommand(sql, conexao);

            comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;

            conexao.Open();
            var linhasAfetadas = comando.ExecuteNonQuery();

            ArquivoLogger.Registrar($"DELETE | Produto {id} | Linhas afetadas: {linhasAfetadas}.");
            return linhasAfetadas > 0;
        }
        catch (SqlException ex)
        {
            ArquivoLogger.Registrar($"ERRO SQL DELETE ID {id} | {ex.Message}");
            throw new InvalidOperationException("Erro ao excluir o produto do banco de dados.", ex);
        }
    }

    private static Produto MapearProduto(SqlDataReader reader)
    {
        return new Produto
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Nome = reader.GetString(reader.GetOrdinal("Nome")),
            Preco = reader.GetDecimal(reader.GetOrdinal("Preco")),
            Estoque = reader.GetInt32(reader.GetOrdinal("Estoque")),
            Categoria = reader.GetString(reader.GetOrdinal("Categoria"))
        };
    }
}
