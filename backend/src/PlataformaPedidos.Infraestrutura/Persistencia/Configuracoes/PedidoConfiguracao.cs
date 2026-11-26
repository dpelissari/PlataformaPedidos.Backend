using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlataformaPedidos.Dominio.Pedidos;

namespace PlataformaPedidos.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// Mapeia o agregado de pedido e o endereço de entrega.
/// </summary>
public sealed class PedidoConfiguracao : IEntityTypeConfiguration<Pedido>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Pedido> builder)
    {
        builder.ToTable("pedidos");

        builder.HasKey(pedido => pedido.Id);
        builder.Property(pedido => pedido.Id).HasColumnName("id");
        builder.Property(pedido => pedido.ClienteId).HasColumnName("cliente_id").IsRequired();
        builder.Property(pedido => pedido.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(pedido => pedido.DataCriacaoUtc).HasColumnName("data_criacao_utc").IsRequired();
        builder.Property(pedido => pedido.MotivoCancelamento)
            .HasColumnName("motivo_cancelamento")
            .HasMaxLength(500);

        builder.Ignore(pedido => pedido.Eventos);
        builder.Ignore(pedido => pedido.ValorTotal);

        builder.OwnsOne(pedido => pedido.EnderecoEntrega, endereco =>
        {
            endereco.Property(item => item.Cep).HasColumnName("endereco_cep").HasMaxLength(8).IsRequired();
            endereco.Property(item => item.Logradouro).HasColumnName("endereco_logradouro").HasMaxLength(200).IsRequired();
            endereco.Property(item => item.Numero).HasColumnName("endereco_numero").HasMaxLength(30).IsRequired();
            endereco.Property(item => item.Complemento).HasColumnName("endereco_complemento").HasMaxLength(120);
            endereco.Property(item => item.Bairro).HasColumnName("endereco_bairro").HasMaxLength(120).IsRequired();
            endereco.Property(item => item.Cidade).HasColumnName("endereco_cidade").HasMaxLength(120).IsRequired();
            endereco.Property(item => item.Estado).HasColumnName("endereco_estado").HasMaxLength(2).IsRequired();
        });

        builder.Navigation(pedido => pedido.EnderecoEntrega).IsRequired();

        builder.HasMany(pedido => pedido.Itens)
            .WithOne()
            .HasForeignKey("PedidoId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(pedido => pedido.Itens)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
