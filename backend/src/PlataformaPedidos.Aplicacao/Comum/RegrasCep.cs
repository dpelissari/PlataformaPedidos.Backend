using FluentValidation;

namespace PlataformaPedidos.Aplicacao.Comum;

/// <summary>
/// Regras reutilizáveis de CEP nos validadores.
/// </summary>
internal static class RegrasCep
{
    /// <summary>
    /// Exige um CEP com oito dígitos numéricos, aceitando máscara.
    /// </summary>
    public static IRuleBuilderOptions<T, string> CepObrigatorio<T>(this IRuleBuilder<T, string> regra) =>
        regra
            .NotEmpty()
            .WithMessage("O CEP é obrigatório.")
            .Must(CepNormalizador.TemOitoDigitos)
            .WithMessage("O CEP deve conter 8 dígitos numéricos.");
}
