using PlataformaPedidos.Dominio.Comum;
using PlataformaPedidos.Dominio.ObjetosValor;

namespace PlataformaPedidos.Dominio.Pedidos;

/// <summary>
/// Item pertencente a um pedido. Só é criado e alterado pela raiz do agregado.
/// </summary>
public sealed class ItemPedido : EntidadeBase
{
    /// <summary>
    /// Produto vendido neste item.
    /// </summary>
    public Guid ProdutoId { get; private set; }

    /// <summary>
    /// Nome do produto no momento da inclusão.
    /// </summary>
    public string NomeProduto { get; private set; } = string.Empty;

    /// <summary>
    /// Quantidade pedida, sempre maior que zero.
    /// </summary>
    public int Quantidade { get; private set; }

    /// <summary>
    /// Preço de uma unidade.
    /// </summary>
    public Dinheiro PrecoUnitario { get; private set; } = null!;

    /// <summary>
    /// Preço unitário multiplicado pela quantidade.
    /// </summary>
    public Dinheiro Subtotal => PrecoUnitario.Multiplicar(Quantidade);

    // Reservado à materialização da persistência. Não valida invariantes.
    private ItemPedido()
    {
    }

    private ItemPedido(Guid id, Guid produtoId, string nomeProduto, int quantidade, Dinheiro precoUnitario)
        : base(id)
    {
        ProdutoId = produtoId;
        NomeProduto = nomeProduto;
        Quantidade = quantidade;
        PrecoUnitario = precoUnitario;
    }

    /// <summary>
    /// Cria um item válido.
    /// </summary>
    /// <param name="produtoId">Identificador do produto.</param>
    /// <param name="nomeProduto">Nome não vazio.</param>
    /// <param name="quantidade">Quantidade maior que zero.</param>
    /// <param name="precoUnitario">Preço de uma unidade.</param>
    /// <returns>Item com subtotal calculado.</returns>
    public static ItemPedido Criar(Guid produtoId, string nomeProduto, int quantidade, Dinheiro precoUnitario)
    {
        if (produtoId == Guid.Empty)
            throw new ExcecaoDominio("O produto do item é obrigatório.");

        if (string.IsNullOrWhiteSpace(nomeProduto))
            throw new ExcecaoDominio("O nome do produto é obrigatório.");

        if (quantidade <= 0)
            throw new ExcecaoDominio("A quantidade do item deve ser maior que zero.");

        if (precoUnitario is null)
            throw new ExcecaoDominio("O preço unitário do item é obrigatório.");

        return new ItemPedido(Guid.CreateVersion7(), produtoId, nomeProduto.Trim(), quantidade, precoUnitario);
    }

    /// <summary>
    /// Aumenta a quantidade quando o pedido agrupa outra inclusão do mesmo produto.
    /// </summary>
    /// <param name="quantidade">Quantidade adicional, maior que zero.</param>
    internal void AumentarQuantidade(int quantidade)
    {
        if (quantidade <= 0)
            throw new ExcecaoDominio("A quantidade adicional deve ser maior que zero.");

        Quantidade += quantidade;
    }
}
