namespace Shared.Models;

public class FrutaMessage
{
    public string Nome { get; set; } = string.Empty;
    public string Resumo { get; set; } = string.Empty;
    public DateTime DataHoraSolicitacao { get; set; }
    public bool IsValidated { get; set; }
}