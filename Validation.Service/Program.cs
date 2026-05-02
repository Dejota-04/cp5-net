using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Models;

namespace Validation.Service;

class Program
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        SetupTopology(channel);

        Console.WriteLine(" [*] Validation Service aguardando mensagens...");

        var consumerFrutas = new EventingBasicConsumer(channel);
        consumerFrutas.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = JsonSerializer.Deserialize<FrutaMessage>(Encoding.UTF8.GetString(body));

            if (message != null)
            {
                Console.WriteLine($" [Frutas] Validando: {message.Nome}...");
                message.IsValidated = true;

                PublishValidatedMessage(channel, RabbitMqConstants.RoutingKeyFrutaValidated, message);

                channel.BasicAck(ea.DeliveryTag, false);
            }
        };

       var consumerUsuarios = new EventingBasicConsumer(channel);
        consumerUsuarios.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = JsonSerializer.Deserialize<UsuarioMessage>(Encoding.UTF8.GetString(body));

            if (message != null)
            {
                Console.WriteLine($" [Usuários] Validando dados de: {message.NomeCompleto}...");

                message.IsValidated = true;

                PublishValidatedMessage(channel, RabbitMqConstants.RoutingKeyUsuarioValidated, message);

                channel.BasicAck(ea.DeliveryTag, false);
            }
        };


        channel.BasicConsume(queue: RabbitMqConstants.QueueFrutasUnvalidated, autoAck: false, consumer: consumerFrutas);
        channel.BasicConsume(queue: RabbitMqConstants.QueueUsuariosUnvalidated, autoAck: false, consumer: consumerUsuarios);

        Console.ReadLine();
    }

    static void SetupTopology(IModel channel)
    {
        channel.ExchangeDeclare(exchange: RabbitMqConstants.ExchangeName, type: ExchangeType.Topic);

        channel.QueueDeclare(queue: RabbitMqConstants.QueueFrutasUnvalidated, durable: true, exclusive: false, autoDelete: false);
        channel.QueueDeclare(queue: RabbitMqConstants.QueueFrutasValidated, durable: true, exclusive: false, autoDelete: false);
        channel.QueueDeclare(queue: RabbitMqConstants.QueueUsuariosUnvalidated, durable: true, exclusive: false, autoDelete: false);
        channel.QueueDeclare(queue: RabbitMqConstants.QueueUsuariosValidated, durable: true, exclusive: false, autoDelete: false);

        channel.QueueBind(queue: RabbitMqConstants.QueueFrutasUnvalidated, exchange: RabbitMqConstants.ExchangeName, routingKey: RabbitMqConstants.RoutingKeyFrutaRequest);
        channel.QueueBind(queue: RabbitMqConstants.QueueFrutasValidated, exchange: RabbitMqConstants.ExchangeName, routingKey: RabbitMqConstants.RoutingKeyFrutaValidated);
        channel.QueueBind(queue: RabbitMqConstants.QueueUsuariosUnvalidated, exchange: RabbitMqConstants.ExchangeName, routingKey: RabbitMqConstants.RoutingKeyUsuarioRequest);
        channel.QueueBind(queue: RabbitMqConstants.QueueUsuariosValidated, exchange: RabbitMqConstants.ExchangeName, routingKey: RabbitMqConstants.RoutingKeyUsuarioValidated);
    }

    static void PublishValidatedMessage<T>(IModel channel, string routingKey, T message)
    {
        var json = JsonSerializer.Serialize(message);
        var body = Encoding.UTF8.GetBytes(json);

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;

        channel.BasicPublish(exchange: RabbitMqConstants.ExchangeName,
                             routingKey: routingKey,
                             basicProperties: properties,
                             body: body);
    }

}