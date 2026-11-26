using PlataformaPedidos.Dominio.Comum;

namespace PlataformaPedidos.Aplicacao.Contratos;

/// <summary>
/// Publica um evento de domínio já confirmado.
/// Os casos de uso não chamam este contrato: o evento permanece no agregado
/// para a infraestrutura gravá-lo na mesma transação da alteração.
/// </summary>
public interface IBarramentoEventos
{
    /// <summary>
    /// Publica o evento no barramento.
    /// </summary>
    /// <param name="evento">Evento já ocorrido no agregado.</param>
    /// <param name="cancellationToken">Token de cancelamento da operação.</param>
    Task PublicarAsync(IDominioEvento evento, CancellationToken cancellationToken = default);
}
