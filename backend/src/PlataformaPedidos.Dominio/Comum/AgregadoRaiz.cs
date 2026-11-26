namespace PlataformaPedidos.Dominio.Comum;

/// <summary>
/// Raiz de agregado. Concentra as mudanças consistentes e os eventos de domínio gerados por elas.
/// </summary>
public abstract class AgregadoRaiz : EntidadeBase
{
    private readonly List<IDominioEvento> _eventos = [];

    /// <summary>
    /// Inicializa o agregado com o identificador informado.
    /// </summary>
    /// <param name="id">Identificador do agregado.</param>
    protected AgregadoRaiz(Guid id)
        : base(id)
    {
    }

    /// <summary>
    /// Construtor para materialização. Os eventos não são recarregados do banco.
    /// </summary>
    protected AgregadoRaiz()
    {
    }

    /// <summary>
    /// Eventos registrados desde a última limpeza.
    /// </summary>
    public IReadOnlyCollection<IDominioEvento> Eventos => _eventos.AsReadOnly();

    /// <summary>
    /// Registra um fato ocorrido neste agregado.
    /// </summary>
    /// <param name="evento">Evento produzido pela operação.</param>
    protected void AdicionarEvento(IDominioEvento evento)
    {
        if (evento is null)
            throw new ExcecaoDominio("O evento de domínio é obrigatório.");

        _eventos.Add(evento);
    }

    /// <summary>
    /// Descarta os eventos já coletados por um fluxo externo, como a publicação na mesma transação.
    /// </summary>
    public void LimparEventos() => _eventos.Clear();
}
