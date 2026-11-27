using PlataformaPedidos.Infraestrutura;
using PlataformaPedidos.Infraestrutura.Observabilidade;
using PlataformaPedidos.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AdicionarInfraestrutura(builder.Configuration);
builder.Services.AdicionarObservabilidade(
    builder.Configuration,
    "PlataformaPedidos.Worker",
    incluirAspNetCore: false);
builder.Services.AddHostedService<ProcessadorOutboxWorker>();
builder.Services.AddHostedService<ConsumidorPagamentoPedidoWorker>();

var host = builder.Build();
host.Run();
