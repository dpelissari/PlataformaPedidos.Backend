using Microsoft.Extensions.Configuration;

namespace PlataformaPedidos.Infraestrutura;

/// <summary>
/// Lê o arquivo mercadopago.local.json na raiz do repositório, se existir.
/// </summary>
public static class ConfiguracaoMercadoPagoLocal
{
    /// <summary>
    /// Acrescenta o arquivo local, que não entra no Git. Ausente, a configuração segue sem ele.
    /// </summary>
    /// <param name="configuracao">Configuração da API ou do worker.</param>
    /// <param name="contentRoot">Pasta do projeto em execução.</param>
    public static void Adicionar(IConfigurationBuilder configuracao, string contentRoot)
    {
        var caminho = Path.GetFullPath(Path.Combine(contentRoot, "..", "..", "..", "mercadopago.local.json"));
        configuracao.AddJsonFile(caminho, optional: true, reloadOnChange: false);
    }
}
