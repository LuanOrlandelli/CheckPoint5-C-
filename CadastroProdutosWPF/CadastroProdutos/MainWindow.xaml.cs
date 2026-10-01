using CadastroProdutos.Helpers;
using CadastroProdutos.Models;
using CadastroProdutos.Repositories;
using CadastroProdutos.Services;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace CadastroProdutos;

public partial class MainWindow : Window
{
    private readonly ProdutoRepository _repository;

    public MainWindow()
    {
        InitializeComponent();

        try
        {
            _repository = new ProdutoRepository(ConfiguracaoBanco.ObterConnectionString());
            ArquivoLogger.Registrar("APLICAÇÃO | Sistema iniciado.");
            CarregarProdutos();
        }
        catch (Exception ex)
        {
            ArquivoLogger.Registrar($"ERRO INICIALIZAÇÃO | {ex.Message}");
            MessageBox.Show(
                ex.Message,
                "Erro de configuração",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            _repository = null!;
        }
    }

    private void BtnInserir_Click(object sender, RoutedEventArgs e)
    {
        if (!TryLerProdutoDoFormulario(exigirId: false, out var produto))
            return;

        try
        {
            _repository.Inserir(produto!);
            MessageBox.Show("Produto cadastrado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            LimparCampos();
            CarregarProdutos();
        }
        catch (Exception ex)
        {
            ExibirErro(ex);
        }
    }

    private void BtnListar_Click(object sender, RoutedEventArgs e)
    {
        CarregarProdutos();
    }

    private void BtnBuscar_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(TxtId.Text, out var id) || id <= 0)
        {
            MessageBox.Show("Informe um ID válido para buscar.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var produto = _repository.BuscarPorId(id);

            if (produto is null)
            {
                MessageBox.Show("Produto não encontrado.", "Busca", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            PreencherCampos(produto);
            DgProdutos.ItemsSource = new List<Produto> { produto };
        }
        catch (Exception ex)
        {
            ExibirErro(ex);
        }
    }

    private void BtnAtualizar_Click(object sender, RoutedEventArgs e)
    {
        if (!TryLerProdutoDoFormulario(exigirId: true, out var produto))
            return;

        try
        {
            var atualizado = _repository.Atualizar(produto!);

            MessageBox.Show(
                atualizado ? "Produto atualizado com sucesso!" : "Produto não encontrado.",
                atualizado ? "Sucesso" : "Aviso",
                MessageBoxButton.OK,
                atualizado ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (atualizado)
            {
                LimparCampos();
                CarregarProdutos();
            }
        }
        catch (Exception ex)
        {
            ExibirErro(ex);
        }
    }

    private void BtnExcluir_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(TxtId.Text, out var id) || id <= 0)
        {
            MessageBox.Show("Informe um ID válido para excluir.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var confirmacao = MessageBox.Show(
            $"Deseja realmente excluir o produto de ID {id}?",
            "Confirmar exclusão",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirmacao != MessageBoxResult.Yes)
            return;

        try
        {
            var excluido = _repository.Excluir(id);

            MessageBox.Show(
                excluido ? "Produto excluído com sucesso!" : "Produto não encontrado.",
                excluido ? "Sucesso" : "Aviso",
                MessageBoxButton.OK,
                excluido ? MessageBoxImage.Information : MessageBoxImage.Warning);

            if (excluido)
            {
                LimparCampos();
                CarregarProdutos();
            }
        }
        catch (Exception ex)
        {
            ExibirErro(ex);
        }
    }

    private void BtnSair_Click(object sender, RoutedEventArgs e)
    {
        ArquivoLogger.Registrar("APLICAÇÃO | Sistema encerrado pelo usuário.");
        Close();
    }

    private void DgProdutos_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DgProdutos.SelectedItem is Produto produto)
        {
            PreencherCampos(produto);
        }
    }

    private void CarregarProdutos()
    {
        if (_repository is null)
            return;

        try
        {
            DgProdutos.ItemsSource = _repository.Listar();
        }
        catch (Exception ex)
        {
            ExibirErro(ex);
        }
    }

    private bool TryLerProdutoDoFormulario(bool exigirId, out Produto? produto)
    {
        produto = null;

        var nome = TxtNome.Text.Trim();
        var categoria = TxtCategoria.Text.Trim();

        if (string.IsNullOrWhiteSpace(nome))
        {
            MessageBox.Show("Informe o nome do produto.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (!TryParseDecimal(TxtPreco.Text, out var preco) || preco < 0)
        {
            MessageBox.Show("Informe um preço válido e não negativo.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (!int.TryParse(TxtEstoque.Text, out var estoque) || estoque < 0)
        {
            MessageBox.Show("Informe um estoque inteiro e não negativo.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(categoria))
        {
            MessageBox.Show("Informe a categoria do produto.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        var id = 0;
        if (exigirId && (!int.TryParse(TxtId.Text, out id) || id <= 0))
        {
            MessageBox.Show("Informe um ID válido.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        produto = new Produto
        {
            Id = id,
            Nome = nome,
            Preco = preco,
            Estoque = estoque,
            Categoria = categoria
        };

        return true;
    }

    private static bool TryParseDecimal(string texto, out decimal valor)
    {
        return decimal.TryParse(texto, NumberStyles.Number, CultureInfo.CurrentCulture, out valor)
               || decimal.TryParse(texto.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out valor);
    }

    private void PreencherCampos(Produto produto)
    {
        TxtId.Text = produto.Id.ToString();
        TxtNome.Text = produto.Nome;
        TxtPreco.Text = produto.Preco.ToString("0.00");
        TxtEstoque.Text = produto.Estoque.ToString();
        TxtCategoria.Text = produto.Categoria;
    }

    private void LimparCampos()
    {
        TxtId.Clear();
        TxtNome.Clear();
        TxtPreco.Clear();
        TxtEstoque.Clear();
        TxtCategoria.Clear();
        DgProdutos.SelectedItem = null;
    }

    private static void ExibirErro(Exception ex)
    {
        ArquivoLogger.Registrar($"ERRO INTERFACE | {ex.Message}");
        MessageBox.Show(
            $"Ocorreu um erro: {ex.Message}",
            "Erro",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}
