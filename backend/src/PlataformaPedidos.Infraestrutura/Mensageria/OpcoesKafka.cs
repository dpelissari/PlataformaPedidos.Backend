namespace PlataformaPedidos.Infraestrutura.Mensageria;

/// <summary>
/// Servidores do Kafka usados pelo produtor.
/// </summary>
public sealed class OpcoesKafka
{
    /// <summary>
    /// Nome da seção de configuração.
    /// </summary>
    public const string Secao = "Kafka";

    /// <summary>
    /// Lista de brokers. No ambiente local é o listener do host.
    /// </summary>
    public string Servidores { get; set; } = ConexoesLocais.Kafka;
}
