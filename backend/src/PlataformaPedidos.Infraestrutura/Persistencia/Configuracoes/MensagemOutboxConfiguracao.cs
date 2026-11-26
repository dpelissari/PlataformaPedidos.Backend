using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PlataformaPedidos.Infraestrutura.Persistencia.Configuracoes;

/// <summary>
/// Mapeia a tabela de outbox. A publicação consulta as linhas ainda sem data de processamento.
/// </summary>
public sealed class MensagemOutboxConfiguracao : IEntityTypeConfiguration<MensagemOutbox>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<MensagemOutbox> builder)
    {
        builder.ToTable("mensagens_outbox");

        builder.HasKey(mensagem => mensagem.Id);
        builder.Property(mensagem => mensagem.Id).HasColumnName("id");
        builder.Property(mensagem => mensagem.Tipo).HasColumnName("tipo").HasMaxLength(120).IsRequired();
        builder.Property(mensagem => mensagem.Topico).HasColumnName("topico").HasMaxLength(120).IsRequired();
        builder.Property(mensagem => mensagem.Conteudo).HasColumnName("conteudo").IsRequired();
        builder.Property(mensagem => mensagem.TraceId).HasColumnName("trace_id").HasMaxLength(64);
        builder.Property(mensagem => mensagem.OcorridoEmUtc).HasColumnName("ocorrido_em_utc").IsRequired();
        builder.Property(mensagem => mensagem.ProcessadaEmUtc).HasColumnName("processada_em_utc");

        builder.HasIndex(mensagem => mensagem.ProcessadaEmUtc);
    }
}
