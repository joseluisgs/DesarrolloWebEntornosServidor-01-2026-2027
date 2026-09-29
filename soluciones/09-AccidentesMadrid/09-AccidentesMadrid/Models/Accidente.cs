namespace AccidentesMadrid.Models;

public class Accidente
{
    public string NumExpediente { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public TimeSpan Hora { get; set; }
    public string Localizacion { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public int CodDistrito { get; set; }
    public string Distrito { get; set; } = string.Empty;
    public string TipoAccidente { get; set; } = string.Empty;
    public string EstadoMeteorologico { get; set; } = string.Empty;
    public string TipoVehiculo { get; set; } = string.Empty;
    public TipoPersona TipoPersona { get; set; }
    public string RangoEdad { get; set; } = string.Empty;
    public Sexo Sexo { get; set; }
    public string CodLesividad { get; set; } = string.Empty;
    public string Lesividad { get; set; } = string.Empty;
    public string CoordenadaXUtm { get; set; } = string.Empty;
    public string CoordenadaYUtm { get; set; } = string.Empty;
    public bool PositivoAlcohol { get; set; }
    public bool PositivoDroga { get; set; }
}
