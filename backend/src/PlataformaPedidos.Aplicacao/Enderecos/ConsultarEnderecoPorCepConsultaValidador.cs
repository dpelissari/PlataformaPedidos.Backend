using FluentValidation;
using PlataformaPedidos.Aplicacao.Comum;

namespace PlataformaPedidos.Aplicacao.Enderecos;

/// <summary>
/// Valida a consulta de endereço por CEP.
/// </summary>
public sealed class ConsultarEnderecoPorCepConsultaValidador : AbstractValidator<ConsultarEnderecoPorCepConsulta>
{
    /// <summary>
    /// Define as regras da consulta.
    /// </summary>
    public ConsultarEnderecoPorCepConsultaValidador()
    {
        RuleFor(consulta => consulta.Cep).CepObrigatorio();
    }
}
