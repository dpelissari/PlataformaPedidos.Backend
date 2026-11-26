using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlataformaPedidos.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// Mapeia o controle de idempotência dos consumidores. A chave é o identificador da mensagem.
/// </summary>
public sealed class MensagemProcessadaConfiguracao : IEntityTypeConfiguration<MensagemProcessada>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MensagemProcessada> builder)
    {
        builder.ToTable("mensagens_processadas");

        builder.HasKey(mensagem => mensagem.MensagemId);
        builder.Property(mensagem => mensagem.MensagemId).HasColumnName("mensagem_id");
        builder.Property(mensagem => mensagem.Consumidor).HasColumnName("consumidor").HasMaxLength(80).IsRequired();
        builder.Property(mensagem => mensagem.ProcessadaEmUtc).HasColumnName("processada_em_utc").IsRequired();
    }
}
