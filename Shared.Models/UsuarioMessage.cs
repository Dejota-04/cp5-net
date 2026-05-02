namespace Shared.Models;

public class UsuarioMessage
{
    public string NomeCompleto { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string RG { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;

    public DateTime DataHoraRegistro { get; set; }

    public bool IsValidated { get; set; }
}