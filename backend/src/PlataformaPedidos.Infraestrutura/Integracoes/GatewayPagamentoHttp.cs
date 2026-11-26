using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PlataformaPedidos.Aplicacao.Comum;
using PlataformaPedidos.Aplicacao.Contratos;
using Polly.Timeout;

namespace PlataformaPedidos.Infraestrutura.Integracoes;

/// <summary>
/// Autoriza pagamento no gateway HTTP. No ambiente local o destino é o WireMock.
/// </summary>
public sealed class GatewayPagamentoHttp(
    HttpClient httpClient,
    IOptions<OpcoesGatewayPagamento> opcoes) : IGatewayPagamento
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    public async Task<Resultado<AutorizacaoPagamento>> AutorizarAsync(
        SolicitacaoAutorizacaoPagamento solicitacao,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var resposta = await httpClient.PostAsJsonAsync(opcoes.Value.Caminho, solicitacao, Json, cancellationToken);
            if (!resposta.IsSuccessStatusCode)
            {
                return Resultado<AutorizacaoPagamento>.Falha(
                    ErroAplicacao.FalhaExterna(
                        "pagamento.indisponivel",
                        $"O gateway de pagamento respondeu HTTP {(int)resposta.StatusCode}."));
            }

            var corpo = await resposta.Content.ReadFromJsonAsync<RespostaPagamento>(Json, cancellationToken);
            if (corpo is null)
            {
                return Resultado<AutorizacaoPagamento>.Falha(
                    ErroAplicacao.FalhaExterna(
                        "pagamento.indisponivel",
                        "O gateway de pagamento não devolveu autorização."));
            }

            return Resultado<AutorizacaoPagamento>.Ok(
                new AutorizacaoPagamento(corpo.Autorizado, corpo.MotivoRecusa));
        }
        catch (Exception excecao) when (excecao is HttpRequestException or TaskCanceledException or TimeoutRejectedException)
        {
            return Resultado<AutorizacaoPagamento>.Falha(
                ErroAplicacao.FalhaExterna("pagamento.indisponivel", "O gateway de pagamento não respondeu."));
        }
    }

    private sealed class RespostaPagamento
    {
        public bool Autorizado { get; set; }
        public string? MotivoRecusa { get; set; }
    }
}
