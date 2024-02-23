using System.Text;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace TNRD.Zeepkist.GTR.Stream;

internal class RabbitWorker : IHostedService
{
    private readonly RabbitOptions options;
    private readonly SocketWriter writer;

    private IConnection connection = null!;
    private IModel channel = null!;

    public RabbitWorker(IOptions<RabbitOptions> options, SocketWriter writer)
    {
        this.writer = writer;
        this.options = options.Value;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        ConnectionFactory factory = new()
        {
            HostName = options.Host,
            Port = options.Port,
            UserName = options.Username,
            Password = options.Password
        };

        connection = factory.CreateConnection();
        channel = connection.CreateModel();

        channel.ExchangeDeclare("stream", ExchangeType.Fanout);

        string? queueName = channel.QueueDeclare().QueueName;
        channel.QueueBind(queueName,
            "stream",
            string.Empty);

        EventingBasicConsumer consumer = new(channel);
        consumer.Received += OnReceived;
        channel.BasicConsume(queueName,
            true,
            consumer);

        return Task.CompletedTask;
    }

    private void OnReceived(object? sender, BasicDeliverEventArgs e)
    {
        byte[] body = e.Body.ToArray();
        string message = Encoding.UTF8.GetString(body);
        writer.Write(message);
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        channel.Close();
        connection.Close();
        return Task.CompletedTask;
    }
}
