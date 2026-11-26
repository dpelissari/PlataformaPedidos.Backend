namespace PlataformaPedidos.Infraestrutura.Persistencia;

/// <summary>
/// Registro de uma mensagem já tratada por um consumidor, para não processá-la de novo.
/// </summary>
public sealed class MensagemProcessada
{
    /// <summary>
    /// Identificador da mensagem já consumida.
    /// </summary>
    public Guid MensagemId { get; set; }

    /// <summary>
    /// Nome do consumidor que tratou a mensagem.
    /// </summary>
    public string Consumidor { get; set; } = string.Empty;

    /// <summary>
    /// Instante UTC em que o consumidor concluiu o tratamento.
    /// </summary>
    public DateTime ProcessadaEmUtc { get; set; }
}
