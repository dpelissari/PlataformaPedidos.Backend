using FluentValidation;
using PlataformaPedidos.Aplicacao.Cache;
using PlataformaPedidos.Aplicacao.Comum;
using PlataformaPedidos.Aplicacao.Contratos;

namespace PlataformaPedidos.Aplicacao.Enderecos;

/// <summary>
/// Resolve o endereço de um CEP. O cache é consultado antes do serviço externo.
/// </summary>
public sealed class ConsultarEnderecoPorCepManipulador(
    IServicoConsultaCep servicoConsultaCep,
    IServicoCache cache,
    IValidator<ConsultarEnderecoPorCepConsulta> validador)
{
    /// <summary>
    /// Executa a consulta.
    /// </summary>
    /// <param name="consulta">CEP informado.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    public async Task<Resultado<EnderecoPorCepResposta>> ManipularAsync(
        ConsultarEnderecoPorCepConsulta consulta,
        CancellationToken cancellationToken)
    {
        var erroValidacao = await ExecutorValidacao.ValidarAsync(validador, consulta, cancellationToken);
        if (erroValidacao is not null)
            return Resultado<EnderecoPorCepResposta>.Falha(erroValidacao);

        var cep = CepNormalizador.Normalizar(consulta.Cep);
        var chave = ChavesCache.Cep(cep);
        var emCache = await cache.ObterAsync<EnderecoPorCepResposta>(chave, cancellationToken);
        if (emCache is not null)
            return Resultado<EnderecoPorCepResposta>.Ok(emCache);

        var consultaExterna = await servicoConsultaCep.ConsultarAsync(cep, cancellationToken);
        if (!consultaExterna.Sucesso || consultaExterna.Valor is null)
        {
            return Resultado<EnderecoPorCepResposta>.Falha(
                consultaExterna.Erro
                ?? ErroAplicacao.FalhaExterna("cep.indisponivel", "A consulta de CEP não retornou endereço."));
        }

        var resposta = new EnderecoPorCepResposta(
            consultaExterna.Valor.Cep,
            consultaExterna.Valor.Logradouro,
            consultaExterna.Valor.Bairro,
            consultaExterna.Valor.Cidade,
            consultaExterna.Valor.Estado);

        await cache.DefinirAsync(chave, resposta, ChavesCache.ExpiracaoCep, cancellationToken);
        return Resultado<EnderecoPorCepResposta>.Ok(resposta);
    }
}
