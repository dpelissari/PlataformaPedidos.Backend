namespace PlataformaPedidos.Dominio.Comum;

/// <summary>
/// Entidade com identidade estável. Duas instâncias do mesmo tipo e do mesmo identificador representam a mesma coisa.
/// </summary>
public abstract class EntidadeBase : IEquatable<EntidadeBase>
{
    /// <summary>
    /// Identificador estável da entidade.
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Inicializa a entidade com um identificador já definido.
    /// </summary>
    /// <param name="id">Identificador não vazio.</param>
    protected EntidadeBase(Guid id)
    {
        if (id == Guid.Empty)
            throw new ExcecaoDominio("O identificador da entidade é obrigatório.");

        Id = id;
    }

    /// <summary>
    /// Construtor para materialização. O identificador é atribuído pela persistência.
    /// </summary>
    protected EntidadeBase()
    {
    }

    /// <summary>
    /// Indica se a outra entidade é a mesma ocorrência.
    /// </summary>
    /// <param name="other">Entidade comparada.</param>
    /// <returns><see langword="true"/> quando o tipo e o identificador coincidem.</returns>
    public bool Equals(EntidadeBase? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return GetType() == other.GetType()
            && Id != Guid.Empty
            && Id == other.Id;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is EntidadeBase outra && Equals(outra);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    /// <summary>
    /// Compara duas entidades pela identidade.
    /// </summary>
    public static bool operator ==(EntidadeBase? esquerda, EntidadeBase? direita) =>
        esquerda is null ? direita is null : esquerda.Equals(direita);

    /// <summary>
    /// Indica se duas entidades não são a mesma ocorrência.
    /// </summary>
    public static bool operator !=(EntidadeBase? esquerda, EntidadeBase? direita) =>
        !(esquerda == direita);
}
