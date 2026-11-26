namespace PlataformaPedidos.Aplicacao.Enderecos;

/// <summary>
/// Consulta o endereço de um CEP, primeiro no cache e depois no serviço externo.
/// </summary>
/// <param name="Cep">CEP com ou sem máscara.</param>
public sealed record ConsultarEnderecoPorCepConsulta(string Cep);
