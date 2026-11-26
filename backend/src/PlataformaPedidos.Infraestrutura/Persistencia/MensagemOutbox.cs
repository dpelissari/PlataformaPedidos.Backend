namespace PlataformaPedidos.Infraestrutura.Persistencia;

/// <summary>
/// Evento de domínio ainda não publicado. Nasce na mesma transação que alterou o pedido.
/// </summary>
public sealed class MensagemOutbox
{
    /// <summary>
    /// Identificador do evento de domínio.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nome do tipo do evento.
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Tópico em que a mensagem será publicada.
    /// </summary>
    public string Topico { get; set; } = string.Empty;

    /// <summary>
    /// Conteúdo JSON do evento.
    /// </summary>
    public string Conteudo { get; set; } = string.Empty;

    /// <summary>
    /// Trace da operação que gravou o evento, quando houver.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Instante UTC em que o evento ocorreu.
    /// </summary>
    public DateTime OcorridoEmUtc { get; set; }

    /// <summary>
    /// Instante UTC em que o worker publicou a mensagem. Nulo enquanto ela espera.
    /// </summary>
    public DateTime? ProcessadaEmUtc { get; set; }
}
