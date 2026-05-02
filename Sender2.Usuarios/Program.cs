using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Shared.Models;

namespace Sender2.Usuarios;

class Program
{
    static void Main(string[] args)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        Console.WriteLine("=== Sender 2: Registro de Usuários Hortifruti ===");
        Console.WriteLine("Pressione [Enter] para simular o registro de um usuário ou [Ctrl+C] para sair.");

        var usuarios = new[]
        {
            new { Nome = "João da Silva", Endereco = "Rua das Laranjeiras, 123", RG = "123456789", CPF = "11122233344" },
            new { Nome = "Maria Souza", Endereco = "Av. Paulista, 1000", RG = "987654321", CPF = "55566677788" },
            new { Nome = "Carlos Ferreira", Endereco = "Rua do Pomar, 45", RG = "456123789", CPF = "99988877766" }
        };

        var random = new Random();

        while (true)
        {
            Console.ReadLine();

            var usuarioEscolhido = usuarios[random.Next(usuarios.Length)];

            var message = new UsuarioMessage
            {
                NomeCompleto = usuarioEscolhido.Nome,
                Endereco = usuarioEscolhido.Endereco,
                RG = usuarioEscolhido.RG,
                CPF = usuarioEscolhido.CPF,
                DataHoraRegistro = DateTime.Now,
                IsValidated = false
            };

            var json = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(json);

            channel.BasicPublish(exchange: RabbitMqConstants.ExchangeName,
                                 routingKey: RabbitMqConstants.RoutingKeyUsuarioRequest,
                                 basicProperties: null,
                                 body: body);

            Console.WriteLine($" [x] Enviado Registro de: {message.NomeCompleto} às {message.DataHoraRegistro}");
        }
    }
}