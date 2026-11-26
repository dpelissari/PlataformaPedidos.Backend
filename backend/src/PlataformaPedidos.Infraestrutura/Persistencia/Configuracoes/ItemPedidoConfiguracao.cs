using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaPedidos.Dominio.Pedidos;

namespace PlataformaPedidos.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// Mapeia o item do pedido e o preço unitário.
/// </summary>
public sealed class ItemPedidoConfiguracao : IEntityTypeConfiguration<ItemPedido>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ItemPedido> builder)
    {
        builder.ToTable("itens_pedido");

        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).HasColumnName("id");
        builder.Property(item => item.ProdutoId).HasColumnName("produto_id").IsRequired();
        builder.Property(item => item.NomeProduto).HasColumnName("nome_produto").HasMaxLength(200).IsRequired();
        builder.Property(item => item.Quantidade).HasColumnName("quantidade").IsRequired();
        builder.Ignore(item => item.Subtotal);

        builder.Property<Guid>("PedidoId").HasColumnName("pedido_id");

        builder.OwnsOne(item => item.PrecoUnitario, preco =>
        {
            preco.Property(dinheiro => dinheiro.Valor)
                .HasColumnName("preco_unitario_valor")
                .HasPrecision(18, 2)
                .IsRequired();
            preco.Property(dinheiro => dinheiro.Moeda)
                .HasColumnName("preco_unitario_moeda")
                .HasMaxLength(3)
                .IsRequired();
        });

        builder.Navigation(item => item.PrecoUnitario).IsRequired();
    }
}
