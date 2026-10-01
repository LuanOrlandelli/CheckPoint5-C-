using Microsoft.Extensions.Configuration;

namespace CadastroProdutos.Helpers;

public static class ConfiguracaoBanco
{
    public static string ObterConnectionString()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "A connection string 'DefaultConnection' não foi encontrada no appsettings.json.");
        }

        return connectionString;
    }
}
