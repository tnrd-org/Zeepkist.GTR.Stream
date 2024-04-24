using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using TNRD.Zeepkist.GTR.Stream;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .Enrich.FromLogContext()
        .MinimumLevel.Information()
        .WriteTo.Console();
});

builder.Services.AddControllers();

builder.Services.AddSingleton<SocketRepository>();
builder.Services.AddSingleton<SocketWriter>();

builder.Services.Configure<RabbitOptions>(builder.Configuration.GetSection("Rabbit"));
builder.Services.AddHostedService<RabbitWorker>();

WebApplication app = builder.Build();

WebSocketOptions options = new()
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};

app.UseWebSockets(options);

app.UseForwardedHeaders(new ForwardedHeadersOptions()
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
