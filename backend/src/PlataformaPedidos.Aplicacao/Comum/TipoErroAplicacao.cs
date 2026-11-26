namespace PlataformaPedidos.Aplicacao.Comum;

/// <summary>
/// Classificação do erro devolvido por um caso de uso.
/// </summary>
public enum TipoErroAplicacao
{
    /// <summary>
    /// A entrada do caso de uso não passou na validação.
    /// </summary>
    Validacao,

    /// <summary>
    /// O recurso pedido não existe.
    /// </summary>
    NaoEncontrado,

    /// <summary>
    /// A operação conflita com o estado atual do agregado.
    /// </summary>
    Conflito,

    /// <summary>
    /// Uma dependência externa falhou ou ficou indisponível.
    /// </summary>
    FalhaExterna
}
