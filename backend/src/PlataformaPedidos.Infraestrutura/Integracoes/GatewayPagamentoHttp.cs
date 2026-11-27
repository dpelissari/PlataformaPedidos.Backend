using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using PlataformaPedidos.Aplicacao.Comum;
using PlataformaPedidos.Aplicacao.Contratos;
using Polly.Timeout;

namespace PlataformaPedidos.Infraestrutura.Integracoes;

/// <summary>
/// Cria o pagamento no Mercado Pago com o token gerado no navegador.
/// </summary>
public sealed class GatewayPagamentoHttp(
    HttpClient httpClient,
    IOptions<OpcoesMercadoPago> opcoes) : IGatewayPagamento
{
    private static readonly JsonSerializerOptions Json = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <inheritdoc />
    public async Task<Resultado<AutorizacaoPagamento>> AutorizarAsync(
        SolicitacaoAutorizacaoPagamento solicitacao,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(opcoes.Value.AccessToken))
        {
            return Falha(
                "Configure o Access Token de teste do Mercado Pago em mercadopago.local.json.");
        }

        try
        {
            var valor = solicitacao.Valor.ToString("0.00", CultureInfo.InvariantCulture);
            using var requisicao = new HttpRequestMessage(HttpMethod.Post, opcoes.Value.Caminho)
            {
                Content = JsonContent.Create(new
                {
                    type = "online",
                    processing_mode = "automatic",
                    total_amount = valor,
                    external_reference = solicitacao.PedidoId.ToString(),
                    payer = new
                    {
                        email = solicitacao.EmailPagador,
                        identification = new
                        {
                            type = solicitacao.TipoDocumento,
                            number = solicitacao.NumeroDocumento
                        }
                    },
                    transactions = new
                    {
                        payments = new[]
                        {
                            new
                            {
                                amount = valor,
                                payment_method = new
                                {
                                    id = solicitacao.Bandeira,
                                    type = TipoCartao(solicitacao.Bandeira),
                                    token = solicitacao.Token,
                                    installments = solicitacao.Parcelas
                                }
                            }
                        }
                    }
                }, options: Json)
            };
            requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", opcoes.Value.AccessToken);
            requisicao.Headers.TryAddWithoutValidation("X-Idempotency-Key", solicitacao.ChaveIdempotencia);

            var resposta = await httpClient.SendAsync(requisicao, cancellationToken);
            using var documento = await JsonDocument.ParseAsync(
                await resposta.Content.ReadAsStreamAsync(cancellationToken),
                cancellationToken: cancellationToken);
            var raiz = documento.RootElement;
            var pagamento = PrimeiroPagamento(raiz);
            var detalhe = Texto(pagamento, "status_detail") ?? Texto(raiz, "status_detail");
            var mensagem = Texto(raiz, "message") ?? PrimeiraMensagemErro(raiz);

            if (!resposta.IsSuccessStatusCode)
            {
                var codigo = (int)resposta.StatusCode;
                if (codigo is 401 or 403 || codigo >= 500)
                    return Falha($"O Mercado Pago respondeu HTTP {codigo}.");

                return Resultado<AutorizacaoPagamento>.Ok(
                    new AutorizacaoPagamento(false, Descrever(detalhe, mensagem)));
            }

            var status = Texto(pagamento, "status") ?? Texto(raiz, "status");
            var aprovado = string.Equals(status, "processed", StringComparison.OrdinalIgnoreCase);
            return Resultado<AutorizacaoPagamento>.Ok(
                new AutorizacaoPagamento(aprovado, aprovado ? null : Descrever(detalhe, mensagem)));
        }
        catch (Exception excecao) when (excecao is HttpRequestException or TaskCanceledException or TimeoutRejectedException)
        {
            return Falha("O Mercado Pago não respondeu.");
        }
        catch (JsonException)
        {
            return Falha("O Mercado Pago devolveu uma resposta inválida.");
        }
    }

    private static string? Texto(JsonElement? elemento, string propriedade) =>
        elemento is { } raiz ? Texto(raiz, propriedade) : null;

    private static string? Texto(JsonElement raiz, string propriedade) =>
        raiz.TryGetProperty(propriedade, out var valor) && valor.ValueKind == JsonValueKind.String
            ? valor.GetString()
            : null;

    private static JsonElement? PrimeiroPagamento(JsonElement raiz)
    {
        if (!raiz.TryGetProperty("transactions", out var transacoes)
            || !transacoes.TryGetProperty("payments", out var pagamentos)
            || pagamentos.ValueKind != JsonValueKind.Array
            || pagamentos.GetArrayLength() == 0)
        {
            return null;
        }

        return pagamentos[0];
    }

    private static string? PrimeiraMensagemErro(JsonElement raiz)
    {
        if (!raiz.TryGetProperty("errors", out var erros)
            || erros.ValueKind != JsonValueKind.Array
            || erros.GetArrayLength() == 0)
        {
            return null;
        }

        return Texto(erros[0], "message");
    }

    private static string TipoCartao(string bandeira) =>
        string.Equals(bandeira, "elo", StringComparison.OrdinalIgnoreCase) ? "debit_card" : "credit_card";

    private static string Descrever(string? detalhe, string? mensagem)
    {
        var texto = detalhe switch
        {
            "cc_rejected_insufficient_amount" => "Cartão sem limite suficiente.",
            "cc_rejected_bad_filled_security_code" => "Código de segurança inválido.",
            "cc_rejected_bad_filled_date" => "Data de vencimento inválida.",
            "cc_rejected_bad_filled_card_number" => "Número do cartão inválido.",
            "cc_rejected_call_for_authorize" => "O banco pediu autorização para este cartão.",
            "cc_rejected_other_reason" => "O cartão foi recusado.",
            "cc_rejected_high_risk" => "O Mercado Pago recusou o pagamento por risco.",
            _ => string.IsNullOrWhiteSpace(mensagem) ? "Pagamento recusado." : mensagem.Trim()
        };

        return texto.Length <= 500 ? texto : texto[..500];
    }

    private static Resultado<AutorizacaoPagamento> Falha(string descricao) =>
        Resultado<AutorizacaoPagamento>.Falha(
            ErroAplicacao.FalhaExterna("pagamento.indisponivel", descricao));
}
