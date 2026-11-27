using PlataformaPedidos.Aplicacao.Pedidos;

namespace PlataformaPedidos.Api;

/// <summary>
/// Endpoints de pedido.
/// </summary>
internal static class EndpointsPedidos
{
    public static IEndpointRouteBuilder MapearPedidos(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/pedidos").WithTags("Pedidos");

        grupo.MapPost(string.Empty, Criar)
            .WithName("CriarPedido")
            .Produces<PedidoResposta>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status502BadGateway);

        grupo.MapGet(string.Empty, Listar)
            .WithName("ListarPedidos")
            .Produces<IReadOnlyList<PedidoResposta>>();

        grupo.MapGet("{id:guid}", Obter)
            .WithName("ObterPedido")
            .Produces<PedidoResposta>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        grupo.MapPost("{id:guid}/solicitar-pagamento", SolicitarPagamento)
            .WithName("SolicitarPagamentoPedido")
            .Produces<PedidoResposta>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        grupo.MapPost("{id:guid}/cancelar", Cancelar)
            .WithName("CancelarPedido")
            .Produces<PedidoResposta>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }

    private static async Task<IResult> Criar(
        CriarPedidoComando comando,
        CriarPedidoManipulador manipulador,
        CancellationToken cancellationToken)
    {
        var resultado = await manipulador.ManipularAsync(comando, cancellationToken);
        return ResultadoHttp.Para(resultado, pedido => Results.Created($"/api/pedidos/{pedido.Id}", pedido));
    }

    private static async Task<IResult> Listar(
        ListarPedidosManipulador manipulador,
        CancellationToken cancellationToken)
    {
        var resultado = await manipulador.ManipularAsync(new ListarPedidosConsulta(), cancellationToken);
        return ResultadoHttp.Para(resultado, Results.Ok);
    }

    private static async Task<IResult> Obter(
        Guid id,
        ObterPedidoPorIdManipulador manipulador,
        CancellationToken cancellationToken)
    {
        var resultado = await manipulador.ManipularAsync(new ObterPedidoPorIdConsulta(id), cancellationToken);
        return ResultadoHttp.Para(resultado, Results.Ok);
    }

    private static async Task<IResult> SolicitarPagamento(
        Guid id,
        SolicitarPagamentoCorpo corpo,
        SolicitarPagamentoPedidoManipulador manipulador,
        CancellationToken cancellationToken)
    {
        var resultado = await manipulador.ManipularAsync(
            new SolicitarPagamentoPedidoComando(
                id,
                corpo.Token ?? string.Empty,
                corpo.Bandeira ?? string.Empty,
                corpo.EmissorId,
                corpo.Parcelas,
                corpo.EmailPagador ?? string.Empty,
                corpo.TipoDocumento ?? string.Empty,
                corpo.NumeroDocumento ?? string.Empty),
            cancellationToken);
        return ResultadoHttp.Para(resultado, Results.Ok);
    }

    private static async Task<IResult> Cancelar(
        Guid id,
        CancelarPedidoCorpo corpo,
        CancelarPedidoManipulador manipulador,
        CancellationToken cancellationToken)
    {
        var resultado = await manipulador.ManipularAsync(
            new CancelarPedidoComando(id, corpo.Motivo ?? string.Empty),
            cancellationToken);
        return ResultadoHttp.Para(resultado, Results.Ok);
    }
}

/// <summary>
/// Corpo do cancelamento. O identificador do pedido vem da rota.
/// </summary>
/// <param name="Motivo">Motivo obrigatório do cancelamento.</param>
internal sealed record CancelarPedidoCorpo(string? Motivo);

/// <summary>
/// Cartão já tokenizado. O número do cartão não chega na API.
/// </summary>
/// <param name="Token">Token de uso único.</param>
/// <param name="Bandeira">Bandeira informada pelo Mercado Pago.</param>
/// <param name="EmissorId">Banco emissor, quando houver.</param>
/// <param name="Parcelas">Quantidade de parcelas.</param>
/// <param name="EmailPagador">E-mail do pagador.</param>
/// <param name="TipoDocumento">Tipo do documento.</param>
/// <param name="NumeroDocumento">Número do documento.</param>
internal sealed record SolicitarPagamentoCorpo(
    string? Token,
    string? Bandeira,
    string? EmissorId,
    int Parcelas,
    string? EmailPagador,
    string? TipoDocumento,
    string? NumeroDocumento);
