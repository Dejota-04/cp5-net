using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Shared.Models;

namespace Sender1.Frutas;

class Program
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        Console.WriteLine("=== Sender 1: Frutas de Época ===");
        Console.WriteLine("Pressione [Enter] para enviar uma nova fruta ou [Ctrl+C] para sair.");

        var frutas = new[]
        {
            new { Nome = "Caqui", Resumo = "Rico em vitaminas A e C, ideal para o outono." },
            new { Nome = "Tangerina", Resumo = "Cítrica e doce, excelente para a imunidade." },
            new { Nome = "Abacate", Resumo = "Rico em gorduras boas, ótimo para o coração." }
        };

        var random = new Random();

        while (true)
        {
            Console.ReadLine();

            var frutaEscolhida = frutas[random.Next(frutas.Length)];

            var message = new FrutaMessage
            {
                Nome = frutaEscolhida.Nome,
                Resumo = frutaEscolhida.Resumo,
                DataHoraSolicitacao = DateTime.Now,
                IsValidated = false
            };

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            channel.BasicPublish(exchange: RabbitMqConstants.ExchangeName,
                                 routingKey: RabbitMqConstants.RoutingKeyFrutaRequest,
                                 basicProperties: null,
                                 body: body);

            Console.WriteLine($" [x] Enviado: {message.Nome} às {message.DataHoraSolicitacao}");
        }
    }
}