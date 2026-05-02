using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Models;

namespace Receiver2.Usuarios;

class Program
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        Console.WriteLine("=== Receiver 2: Usuários Validados ===");
        Console.WriteLine(" [*] Aguardando mensagens...");

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = JsonSerializer.Deserialize<UsuarioMessage>(Encoding.UTF8.GetString(body));

            if (message != null)
            {
                Console.WriteLine("\n[Novo Usuário Recebido]");
                Console.WriteLine($"- Nome: {message.NomeCompleto}");
                Console.WriteLine($"- Endereço: {message.Endereco}");
                Console.WriteLine($"- RG: {message.RG}");
                Console.WriteLine($"- CPF: {message.CPF}");
                Console.WriteLine($"- Data/Hora Registro: {message.DataHoraRegistro}");
                Console.WriteLine($"- Status de Validação: {(message.IsValidated ? "VALIDADO" : "PENDENTE")}");

                channel.BasicAck(ea.DeliveryTag, false);
            }
        };

        channel.BasicConsume(queue: RabbitMqConstants.QueueUsuariosValidated,
                             autoAck: false,
                             consumer: consumer);

        Console.ReadLine();
    }
}