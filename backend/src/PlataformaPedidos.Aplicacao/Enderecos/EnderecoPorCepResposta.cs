namespace PlataformaPedidos.Aplicacao.Enderecos;

/// <summary>
/// Endereço devolvido pela consulta de CEP, sem número nem complemento.
/// </summary>
/// <param name="Cep">CEP com oito dígitos.</param>
/// <param name="Logradouro">Logradouro encontrado.</param>
/// <param name="Bairro">Bairro encontrado.</param>
/// <param name="Cidade">Cidade encontrada.</param>
/// <param name="Estado">UF encontrada.</param>
public sealed record EnderecoPorCepResposta(string Cep, string Logradouro, string Bairro, string Cidade, string Estado);
