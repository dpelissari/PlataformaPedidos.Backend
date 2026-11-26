namespace PlataformaPedidos.Aplicacao.Pedidos;

/// <summary>
/// Item devolvido nas consultas de pedido.
/// </summary>
/// <param name="Id">Identificador do item.</param>
/// <param name="ProdutoId">Identificador do produto.</param>
/// <param name="NomeProduto">Nome do produto.</param>
/// <param name="Quantidade">Quantidade pedida.</param>
/// <param name="PrecoUnitario">Preço de uma unidade.</param>
/// <param name="Subtotal">Preço unitário multiplicado pela quantidade.</param>
public sealed record ItemPedidoResposta(
    Guid Id,
    Guid ProdutoId,
    string NomeProduto,
    int Quantidade,
    decimal PrecoUnitario,
    decimal Subtotal);

/// <summary>
/// Pedido devolvido pelos casos de uso, sem expor o agregado.
/// </summary>
/// <param name="Id">Identificador do pedido.</param>
/// <param name="ClienteId">Identificador do cliente.</param>
/// <param name="Cep">CEP da entrega, com oito dígitos.</param>
/// <param name="Logradouro">Logradouro da entrega.</param>
/// <param name="Numero">Número da entrega.</param>
/// <param name="Complemento">Complemento da entrega, quando houver.</param>
/// <param name="Bairro">Bairro da entrega.</param>
/// <param name="Cidade">Cidade da entrega.</param>
/// <param name="Estado">UF da entrega.</param>
/// <param name="Status">Situação atual do pedido.</param>
/// <param name="DataCriacaoUtc">Instante UTC da criação.</param>
/// <param name="ValorTotal">Soma dos subtotais.</param>
/// <param name="Moeda">Código da moeda do total.</param>
/// <param name="MotivoCancelamento">Motivo preenchido quando o pedido foi cancelado.</param>
/// <param name="Itens">Itens do pedido.</param>
public sealed record PedidoResposta(
    Guid Id,
    Guid ClienteId,
    string Cep,
    string Logradouro,
    string Numero,
    string? Complemento,
    string Bairro,
    string Cidade,
    string Estado,
    string Status,
    DateTime DataCriacaoUtc,
    decimal ValorTotal,
    string Moeda,
    string? MotivoCancelamento,
    IReadOnlyList<ItemPedidoResposta> Itens);
