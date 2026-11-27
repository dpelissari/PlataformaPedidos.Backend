using PlataformaPedidos.Api;
using PlataformaPedidos.Infraestrutura;
using PlataformaPedidos.Infraestrutura.Observabilidade;
using PlataformaPedidos.Infraestrutura.Saude;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ManipuladorExcecaoGlobal>();
builder.Services.AddCors(opcoes =>
    opcoes.AddPolicy("Angular", politica =>
        politica.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()));

builder.Services.AdicionarCasosDeUso();
builder.Services.AdicionarInfraestrutura(builder.Configuration);
builder.Services.AdicionarObservabilidade(
    builder.Configuration,
    "PlataformaPedidos.Api",
    incluirAspNetCore: true);
builder.Services.AdicionarVerificacoesSaude();

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors("Angular");

if (!app.Environment.IsEnvironment("Testing"))
    app.UseHttpsRedirection();

app.MapOpenApi();
app.MapScalarApiReference();
app.MapHealthChecks("/health");
app.MapearPedidos();
app.MapearEnderecos();

app.Run();

/// <summary>
/// Ponto de entrada exposto para os testes de integração.
/// </summary>
public partial class Program;
