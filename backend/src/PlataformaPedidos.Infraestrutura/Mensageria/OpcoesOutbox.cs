namespace PlataformaPedidos.Infraestrutura.Mensageria;

/// <summary>
/// Ritmo em que o worker procura mensagens pendentes na outbox.
/// </summary>
public sealed class OpcoesOutbox
{
    /// <summary>
    /// Nome da seção de configuração.
    /// </summary>
    public const string Secao = "Outbox";

    /// <summary>
    /// Intervalo entre as leituras. Um segundo no ambiente local.
    /// </summary>
    public TimeSpan Intervalo { get; set; } = TimeSpan.FromSeconds(1);
}
