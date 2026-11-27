using System.Diagnostics;

namespace PlataformaPedidos.Infraestrutura.Observabilidade;

/// <summary>
/// Fonte de atividades da plataforma, compartilhada pela API e pelo worker.
/// </summary>
public static class Telemetria
{
    /// <summary>
    /// Nome registrado no OpenTelemetry.
    /// </summary>
    public const string Nome = "PlataformaPedidos";

    /// <summary>
    /// Fonte usada pelos fluxos de outbox e de pagamento.
    /// </summary>
    public static readonly ActivitySource Fonte = new(Nome);
}
