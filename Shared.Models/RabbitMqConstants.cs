namespace Shared.Models;

public static class RabbitMqConstants
{
    public const string ExchangeName = "fiap.checkpoint.topic";

    public const string QueueFrutasUnvalidated = "queue.frutas.unvalidated";
    public const string QueueFrutasValidated = "queue.frutas.validated";

    public const string QueueUsuariosUnvalidated = "queue.usuarios.unvalidated";
    public const string QueueUsuariosValidated = "queue.usuarios.validated";

    public const string RoutingKeyFrutaRequest = "fruta.request";
    public const string RoutingKeyFrutaValidated = "fruta.validated";

    public const string RoutingKeyUsuarioRequest = "usuario.request";
    public const string RoutingKeyUsuarioValidated = "usuario.validated";
}