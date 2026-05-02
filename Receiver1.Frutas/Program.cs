using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Models;

namespace Receiver1.Frutas;

class Program
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        Console.WriteLine("=== Receiver 1: Frutas Validadas ===");
        Console.WriteLine(" [*] Aguardando mensagens...");

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = JsonSerializer.Deserialize<FrutaMessage>(Encoding.UTF8.GetString(body));

            if (message != null)
            {
                Console.WriteLine("\n[Nova Fruta Recebida]");
                Console.WriteLine($"- Nome: {message.Nome}");
                Console.WriteLine($"- Resumo: {message.Resumo}");
                Console.WriteLine($"- Data/Hora Solicitação: {message.DataHoraSolicitacao}");
                Console.WriteLine($"- Status de Validação: {(message.IsValidated ? "VALIDADO" : "PENDENTE")}");

                channel.BasicAck(ea.DeliveryTag, false);
            }
        };

        channel.BasicConsume(queue: RabbitMqConstants.QueueFrutasValidated,
                             autoAck: false,
                             consumer: consumer);

        Console.ReadLine();
    }
}