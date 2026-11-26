using PlataformaPedidos.Dominio.Comum;
using PlataformaPedidos.Dominio.ObjetosValor;

namespace PlataformaPedidos.Dominio.Pedidos;

/// <summary>
/// Pedido de um cliente. Controla itens, total e as transições até a confirmação ou o cancelamento.
/// </summary>
public sealed class Pedido : AgregadoRaiz
{
    private readonly List<ItemPedido> _itens = [];

    /// <summary>
    /// Cliente dono do pedido.
    /// </summary>
    public Guid ClienteId { get; private set; }

    /// <summary>
    /// Endereço para onde a entrega será feita.
    /// </summary>
    public Endereco EnderecoEntrega { get; private set; } = null!;

    /// <summary>
    /// Situação atual do pedido.
    /// </summary>
    public StatusPedido Status { get; private set; }

    /// <summary>
    /// Instante UTC em que o pedido foi criado.
    /// </summary>
    public DateTime DataCriacaoUtc { get; private set; }

    /// <summary>
    /// Itens do pedido. A coleção só muda pelos métodos do agregado.
    /// </summary>
    public IReadOnlyList<ItemPedido> Itens => _itens;

    /// <summary>
    /// Soma dos subtotais. Um pedido sem itens vale zero reais.
    /// </summary>
    public Dinheiro ValorTotal => CalcularValorTotal();

    /// <summary>
    /// Motivo preenchido quando o pedido é cancelado.
    /// </summary>
    public string? MotivoCancelamento { get; private set; }

    // Reservado à materialização da persistência. Não valida invariantes.
    private Pedido()
    {
    }

    private Pedido(Guid id, Guid clienteId, Endereco enderecoEntrega, DateTime dataCriacaoUtc)
        : base(id)
    {
        ClienteId = clienteId;
        EnderecoEntrega = enderecoEntrega;
        DataCriacaoUtc = dataCriacaoUtc;
        Status = StatusPedido.Rascunho;
    }

    /// <summary>
    /// Cria um pedido em rascunho e registra <see cref="PedidoCriadoEvento"/>.
    /// </summary>
    /// <param name="clienteId">Identificador do cliente.</param>
    /// <param name="enderecoEntrega">Endereço de entrega já validado.</param>
    /// <param name="dataCriacaoUtc">
    /// Instante da criação. Quando omitido, usa o UTC atual.
    /// Data com fuso local é convertida para UTC. Data sem fuso é tratada como UTC.
    /// </param>
    /// <returns>Pedido sem itens.</returns>
    public static Pedido Criar(Guid clienteId, Endereco enderecoEntrega, DateTime? dataCriacaoUtc = null)
    {
        if (clienteId == Guid.Empty)
            throw new ExcecaoDominio("O cliente do pedido é obrigatório.");

        if (enderecoEntrega is null)
            throw new ExcecaoDominio("O endereço de entrega é obrigatório.");

        var data = NormalizarUtc(dataCriacaoUtc ?? DateTime.UtcNow);
        var pedido = new Pedido(Guid.CreateVersion7(), clienteId, enderecoEntrega, data);
        pedido.AdicionarEvento(new PedidoCriadoEvento(pedido.Id, clienteId, data));
        return pedido;
    }

    /// <summary>
    /// Inclui um item ou soma a quantidade quando o mesmo produto já está no pedido com o mesmo preço.
    /// </summary>
    /// <param name="produtoId">Identificador do produto.</param>
    /// <param name="nomeProduto">Nome do produto.</param>
    /// <param name="quantidade">Quantidade maior que zero.</param>
    /// <param name="precoUnitario">Preço de uma unidade.</param>
    public void AdicionarItem(Guid produtoId, string nomeProduto, int quantidade, Dinheiro precoUnitario)
    {
        GarantirRascunho("Só um pedido em rascunho aceita itens.");

        if (precoUnitario is null)
            throw new ExcecaoDominio("O preço unitário do item é obrigatório.");

        if (quantidade <= 0)
            throw new ExcecaoDominio("A quantidade do item deve ser maior que zero.");

        var existente = _itens.FirstOrDefault(item => item.ProdutoId == produtoId);
        if (existente is not null)
        {
            if (existente.PrecoUnitario != precoUnitario)
                throw new ExcecaoDominio("O mesmo produto não pode entrar no pedido com preços diferentes.");

            existente.AumentarQuantidade(quantidade);
            return;
        }

        _itens.Add(ItemPedido.Criar(produtoId, nomeProduto, quantidade, precoUnitario));
    }

    /// <summary>
    /// Remove um item do rascunho.
    /// </summary>
    /// <param name="itemId">Identificador do item.</param>
    public void RemoverItem(Guid itemId)
    {
        GarantirRascunho("Só um pedido em rascunho permite remover itens.");

        var item = _itens.FirstOrDefault(candidato => candidato.Id == itemId);
        if (item is null)
            throw new ExcecaoDominio("O item informado não pertence ao pedido.");

        _itens.Remove(item);
    }

    /// <summary>
    /// Envia o rascunho para pagamento quando há ao menos um item.
    /// </summary>
    public void SolicitarPagamento()
    {
        if (Status != StatusPedido.Rascunho)
            throw new ExcecaoDominio("Só um pedido em rascunho pode solicitar pagamento.");

        if (_itens.Count == 0)
            throw new ExcecaoDominio("O pedido precisa ter ao menos um item para solicitar pagamento.");

        Status = StatusPedido.AguardandoPagamento;
        AdicionarEvento(new PagamentoPedidoSolicitadoEvento(Id, ValorTotal, DateTime.UtcNow));
    }

    /// <summary>
    /// Confirma o pagamento de um pedido que já está aguardando.
    /// </summary>
    public void ConfirmarPagamento()
    {
        if (Status != StatusPedido.AguardandoPagamento)
            throw new ExcecaoDominio("Só um pedido aguardando pagamento pode ser confirmado.");

        Status = StatusPedido.Confirmado;
        AdicionarEvento(new PedidoConfirmadoEvento(Id, DateTime.UtcNow));
    }

    /// <summary>
    /// Cancela um rascunho ou um pedido que ainda aguarda pagamento.
    /// </summary>
    /// <param name="motivo">Motivo obrigatório do cancelamento.</param>
    public void Cancelar(string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ExcecaoDominio("O motivo do cancelamento é obrigatório.");

        if (Status is not (StatusPedido.Rascunho or StatusPedido.AguardandoPagamento))
            throw new ExcecaoDominio("O pedido não pode ser cancelado neste status.");

        MotivoCancelamento = motivo.Trim();
        Status = StatusPedido.Cancelado;
        AdicionarEvento(new PedidoCanceladoEvento(Id, MotivoCancelamento, DateTime.UtcNow));
    }

    private Dinheiro CalcularValorTotal()
    {
        if (_itens.Count == 0)
            return Dinheiro.Criar(0);

        return _itens
            .Select(item => item.Subtotal)
            .Aggregate((atual, proximo) => atual.Somar(proximo));
    }

    private void GarantirRascunho(string mensagem)
    {
        if (Status != StatusPedido.Rascunho)
            throw new ExcecaoDominio(mensagem);
    }

    private static DateTime NormalizarUtc(DateTime data)
    {
        return data.Kind switch
        {
            DateTimeKind.Utc => data,
            DateTimeKind.Local => data.ToUniversalTime(),
            _ => DateTime.SpecifyKind(data, DateTimeKind.Utc)
        };
    }
}
