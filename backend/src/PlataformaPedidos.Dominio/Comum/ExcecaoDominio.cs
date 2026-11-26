namespace PlataformaPedidos.Dominio.Comum;

/// <summary>
/// Falha de uma invariante do domínio. A operação foi recusada e o estado anterior permanece válido.
/// </summary>
public sealed class ExcecaoDominio : Exception
{
    /// <summary>
    /// Cria a exceção com a mensagem que descreve a regra violada.
    /// </summary>
    /// <param name="mensagem">Descrição da invariante recusada.</param>
    public ExcecaoDominio(string mensagem)
        : base(mensagem)
    {
    }
}
