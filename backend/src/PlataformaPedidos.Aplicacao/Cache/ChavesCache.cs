namespace PlataformaPedidos.Aplicacao.Cache;

/// <summary>
/// Chaves e tempos de expiração das consultas em cache.
/// </summary>
public static class ChavesCache
{
    /// <summary>
    /// Tempo de vida da consulta de um pedido. A alteração do pedido remove a chave antes disso.
    /// </summary>
    public static readonly TimeSpan ExpiracaoPedido = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Tempo de vida da consulta de CEP. O endereço do CEP muda raramente.
    /// </summary>
    public static readonly TimeSpan ExpiracaoCep = TimeSpan.FromHours(24);

    /// <summary>
    /// Chave da consulta de um pedido.
    /// </summary>
    /// <param name="pedidoId">Identificador do pedido.</param>
    public static string Pedido(Guid pedidoId) => $"pedidos:{pedidoId:N}";

    /// <summary>
    /// Chave da consulta de um CEP já normalizado.
    /// </summary>
    /// <param name="cep">CEP com oito dígitos.</param>
    public static string Cep(string cep) => $"ceps:{cep}";
}
