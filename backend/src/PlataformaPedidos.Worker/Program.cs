using PlataformaPedidos.Infraestrutura;
using PlataformaPedidos.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AdicionarInfraestrutura(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
