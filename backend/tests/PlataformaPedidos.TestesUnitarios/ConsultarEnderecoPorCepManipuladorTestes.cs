using FluentAssertions;
using NSubstitute;
using PlataformaPedidos.Aplicacao.Cache;
using PlataformaPedidos.Aplicacao.Comum;
using PlataformaPedidos.Aplicacao.Contratos;
using PlataformaPedidos.Aplicacao.Enderecos;

namespace PlataformaPedidos.TestesUnitarios;

public class ConsultarEnderecoPorCepManipuladorTestes
{
    private readonly IServicoConsultaCep _consultaCep = Substitute.For<IServicoConsultaCep>();
    private readonly IServicoCache _cache = Substitute.For<IServicoCache>();
    private readonly ConsultarEnderecoPorCepManipulador _manipulador;

    public ConsultarEnderecoPorCepManipuladorTestes()
    {
        _manipulador = new ConsultarEnderecoPorCepManipulador(
            _consultaCep,
            _cache,
            new ConsultarEnderecoPorCepConsultaValidador());
    }

    [Fact]
    public async Task ManipularAsync_ComCacheHit_DevolveEnderecoSemChamarServico()
    {
        var resposta = new EnderecoPorCepResposta("01310100", "Avenida Paulista", "Bela Vista", "São Paulo", "SP");
        _cache.ObterAsync<EnderecoPorCepResposta>(ChavesCache.Cep("01310100"), Arg.Any<CancellationToken>())
            .Returns(resposta);

        var resultado = await _manipulador.ManipularAsync(
            new ConsultarEnderecoPorCepConsulta("01310-100"),
            CancellationToken.None);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor.Should().BeSameAs(resposta);
        await _consultaCep.DidNotReceive().ConsultarAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ManipularAsync_ComCacheMiss_ConsultaServicoEGravaCache()
    {
        _consultaCep.ConsultarAsync("01310100", Arg.Any<CancellationToken>())
            .Returns(Resultado<EnderecoExterno>.Ok(
                new EnderecoExterno("01310100", "Avenida Paulista", "Bela Vista", "São Paulo", "SP")));

        var resultado = await _manipulador.ManipularAsync(
            new ConsultarEnderecoPorCepConsulta("01310-100"),
            CancellationToken.None);

        resultado.Sucesso.Should().BeTrue();
        resultado.Valor!.Logradouro.Should().Be("Avenida Paulista");
        resultado.Valor.Estado.Should().Be("SP");
        await _cache.Received(1).DefinirAsync(
            ChavesCache.Cep("01310100"),
            Arg.Is<EnderecoPorCepResposta>(endereco => endereco.Cep == "01310100"),
            ChavesCache.ExpiracaoCep,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ManipularAsync_ComCepInvalido_DevolveValidacao()
    {
        var resultado = await _manipulador.ManipularAsync(
            new ConsultarEnderecoPorCepConsulta("123"),
            CancellationToken.None);

        resultado.Erro!.Tipo.Should().Be(TipoErroAplicacao.Validacao);
        await _cache.DidNotReceive().ObterAsync<EnderecoPorCepResposta>(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _consultaCep.DidNotReceive().ConsultarAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ManipularAsync_ComFalhaExterna_DevolveFalhaSemGravarCache()
    {
        _consultaCep.ConsultarAsync("01310100", Arg.Any<CancellationToken>())
            .Returns(Resultado<EnderecoExterno>.Falha(
                ErroAplicacao.FalhaExterna("cep.indisponivel", "Serviço de CEP indisponível.")));

        var resultado = await _manipulador.ManipularAsync(
            new ConsultarEnderecoPorCepConsulta("01310100"),
            CancellationToken.None);

        resultado.Erro!.Tipo.Should().Be(TipoErroAplicacao.FalhaExterna);
        await _cache.DidNotReceive().DefinirAsync(
            Arg.Any<string>(),
            Arg.Any<EnderecoPorCepResposta>(),
            Arg.Any<TimeSpan>(),
            Arg.Any<CancellationToken>());
    }
}
