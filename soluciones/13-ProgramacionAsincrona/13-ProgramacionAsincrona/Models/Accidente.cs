namespace _13_ProgramacionAsincrona.Models;

public class Accidente
{
    public string NumExpediente { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public TimeSpan Hora { get; set; }
    public string Distrito { get; set; } = string.Empty;
    public string TipoAccidente { get; set; } = string.Empty;
    public string Sexo { get; set; } = string.Empty;
    public bool PositivoAlcohol { get; set; }
    public bool PositivoDroga { get; set; }
}
