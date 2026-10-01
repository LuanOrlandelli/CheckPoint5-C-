using System.IO;
using System.Text;

namespace CadastroProdutos.Services;

public static class ArquivoLogger
{
    private static readonly object LockObject = new();

    public static void Registrar(string mensagem)
    {
        try
        {
            var pastaLogs = Path.Combine(AppContext.BaseDirectory, "Logs");
            Directory.CreateDirectory(pastaLogs);

            var caminhoArquivo = Path.Combine(pastaLogs, "operacoes.log");
            var linha = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {mensagem}{Environment.NewLine}";

            lock (LockObject)
            {
                File.AppendAllText(caminhoArquivo, linha, Encoding.UTF8);
            }
        }
        catch
        {
            // Falha de log não deve interromper a aplicação.
        }
    }
}
