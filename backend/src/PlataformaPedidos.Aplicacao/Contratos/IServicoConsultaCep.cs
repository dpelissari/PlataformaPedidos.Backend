using PlataformaPedidos.Aplicacao.Comum;

namespace PlataformaPedidos.Aplicacao.Contratos;

/// <summary>
/// Dados de endereço obtidos de um serviço externo de CEP, sem número nem complemento.
/// </summary>
/// <param name="Cep">CEP com oito dígitos.</param>
/// <param name="Logradouro">Logradouro retornado pelo serviço.</param>
/// <param name="Bairro">Bairro retornado pelo serviço.</param>
/// <param name="Cidade">Cidade retornada pelo serviço.</param>
/// <param name="Estado">Sigla da UF.</param>
public sealed record EnderecoExterno(string Cep, string Logradouro, string Bairro, string Cidade, string Estado);

/// <summary>
/// Consulta um CEP em um serviço externo.
/// </summary>
public interface IServicoConsultaCep
{
    /// <summary>
    /// Busca o endereço do CEP. CEP inexistente volta como não encontrado; falha de comunicação, como falha externa.
    /// </summary>
    /// <param name="cep">CEP já normalizado, com oito dígitos.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    Task<Resultado<EnderecoExterno>> ConsultarAsync(string cep, CancellationToken cancellationToken = default);
}
