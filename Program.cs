using TNRD.Zeepkist.GTR.Stream;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSingleton<SocketRepository>();
builder.Services.AddSingleton<SocketWriter>();

builder.Services.Configure<RabbitOptions>(builder.Configuration.GetSection("Rabbit"));
builder.Services.AddHostedService<RabbitWorker>();

WebApplication app = builder.Build();

WebSocketOptions options = new WebSocketOptions()
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};

app.UseWebSockets(options);

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();
