namespace PlataformaPedidos.Dominio.Comum;

/// <summary>
/// Fato relevante ocorrido dentro de um agregado e que outros fluxos podem reagir depois.
/// </summary>
public interface IDominioEvento
{
    /// <summary>
    /// Identificador único desta ocorrência.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Instante UTC em que o fato ocorreu.
    /// </summary>
    DateTime OcorridoEmUtc { get; }
}
