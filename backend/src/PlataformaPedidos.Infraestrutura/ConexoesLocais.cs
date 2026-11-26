namespace PlataformaPedidos.Infraestrutura;

/// <summary>
/// Conexões do ambiente local descrito no docker-compose.
/// </summary>
internal static class ConexoesLocais
{
    public const string Postgres =
        "Host=localhost;Port=5432;Database=plataforma_pedidos;Username=plataforma;Password=plataforma";

    public const string Redis = "localhost:6379";

    public const string Kafka = "localhost:9092";

    public const string WireMock = "http://localhost:8080";
}
