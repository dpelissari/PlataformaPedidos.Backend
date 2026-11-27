using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PlataformaPedidos.Aplicacao.Comum;
using PlataformaPedidos.Aplicacao.Contratos;
using Polly.Timeout;

namespace PlataformaPedidos.Infraestrutura.Integracoes;

/// <summary>
/// Consulta CEP por HTTP. Aceita o JSON do ViaCEP e o da BrasilAPI, além do WireMock.
/// </summary>
public sealed class ServicoConsultaCepHttp(
    HttpClient httpClient,
    IOptions<OpcoesConsultaCep> opcoes) : IServicoConsultaCep
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    /// <inheritdoc />
    public async Task<Resultado<EnderecoExterno>> ConsultarAsync(string cep, CancellationToken cancellationToken = default)
    {
        var caminho = opcoes.Value.Caminho.Replace("{cep}", Uri.EscapeDataString(cep), StringComparison.Ordinal);

        try
        {
            var resposta = await httpClient.GetAsync(caminho, cancellationToken);
            if (resposta.StatusCode == HttpStatusCode.NotFound)
                return NaoEncontrado();

            if (!resposta.IsSuccessStatusCode)
                return FalhaHttp((int)resposta.StatusCode);

            var corpo = await resposta.Content.ReadFromJsonAsync<RespostaCep>(Json, cancellationToken);
            if (corpo is null || corpo.IndicaErro)
                return NaoEncontrado();

            var cidade = PrimeiroPreenchido(corpo.Localidade, corpo.City);
            var estado = PrimeiroPreenchido(corpo.Uf, corpo.State);
            if (cidade is null || estado is null)
                return NaoEncontrado();

            return Resultado<EnderecoExterno>.Ok(new EnderecoExterno(
                CepNormalizador.Normalizar(PrimeiroPreenchido(corpo.Cep, cep)!),
                PrimeiroPreenchido(corpo.Logradouro, corpo.Street) ?? string.Empty,
                PrimeiroPreenchido(corpo.Bairro, corpo.Neighborhood) ?? string.Empty,
                cidade,
                estado));
        }
        catch (Exception excecao) when (EhFalhaDeComunicacao(excecao))
        {
            return FalhaDeComunicacao();
        }
    }

    private static bool EhFalhaDeComunicacao(Exception excecao) =>
        excecao is HttpRequestException or TaskCanceledException or TimeoutRejectedException;

    private static Resultado<EnderecoExterno> NaoEncontrado() =>
        Resultado<EnderecoExterno>.Falha(
            ErroAplicacao.NaoEncontrado("cep.nao_encontrado", "O CEP não foi encontrado."));

    private static Resultado<EnderecoExterno> FalhaHttp(int codigo) =>
        Resultado<EnderecoExterno>.Falha(
            ErroAplicacao.FalhaExterna("cep.indisponivel", $"A consulta de CEP respondeu HTTP {codigo}."));

    private static Resultado<EnderecoExterno> FalhaDeComunicacao() =>
        Resultado<EnderecoExterno>.Falha(
            ErroAplicacao.FalhaExterna("cep.indisponivel", "A consulta de CEP não respondeu."));

    private static string? PrimeiroPreenchido(params string?[] valores) =>
        valores.FirstOrDefault(valor => !string.IsNullOrWhiteSpace(valor));

    private sealed class RespostaCep
    {
        public string? Cep { get; set; }
        public string? Logradouro { get; set; }
        public string? Bairro { get; set; }
        public string? Localidade { get; set; }
        public string? Uf { get; set; }

        /// <summary>
        /// ViaCEP envia "true" como texto. Um booleano também indica CEP inexistente.
        /// </summary>
        public JsonElement Erro { get; set; }

        public bool IndicaErro => Erro.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.String => string.Equals(Erro.GetString(), "true", StringComparison.OrdinalIgnoreCase),
            _ => false
        };
        public string? Street { get; set; }
        public string? Neighborhood { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
    }
}
