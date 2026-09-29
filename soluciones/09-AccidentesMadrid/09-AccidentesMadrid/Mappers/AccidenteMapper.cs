using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using AccidentesMadrid.Models;

namespace AccidentesMadrid.Mappers;

public sealed class AccidenteMapper : ClassMap<Accidente>
{
    public AccidenteMapper()
    {
        Map(a => a.NumExpediente).Name("num_expediente");
        Map(a => a.Fecha).Name("fecha").TypeConverterOption.Format("dd/MM/yyyy");
        Map(a => a.Hora).Name("hora").TypeConverter(new HoraTimeSpanConverter());
        Map(a => a.Localizacion).Name("localizacion");
        Map(a => a.Numero).Name("numero");
        Map(a => a.CodDistrito).Name("cod_distrito");
        Map(a => a.Distrito).Name("distrito");
        Map(a => a.TipoAccidente).Name("tipo_accidente");
        Map(a => a.EstadoMeteorologico).Name("estado_meteorológico");
        Map(a => a.TipoVehiculo).Name("tipo_vehiculo");
        Map(a => a.TipoPersona).Name("tipo_persona").TypeConverter(new TipoPersonaConverter());
        Map(a => a.RangoEdad).Name("rango_edad");
        Map(a => a.Sexo).Name("sexo").TypeConverter(new SexoEnumConverter());
        Map(a => a.CodLesividad).Name("cod_lesividad");
        Map(a => a.Lesividad).Name("lesividad");
        Map(a => a.CoordenadaXUtm).Name("coordenada_x_utm");
        Map(a => a.CoordenadaYUtm).Name("coordenada_y_utm");
        Map(a => a.PositivoAlcohol).Name("positiva_alcohol").TypeConverter(new SiNoConverter());
        Map(a => a.PositivoDroga).Name("positiva_droga").TypeConverter(new SiNoConverter());
    }
}

public sealed class SexoEnumConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        return text?.Trim().ToUpperInvariant() switch
        {
            "HOMBRE" or "H" => Sexo.Hombre,
            "MUJER" or "M" => Sexo.Mujer,
            _ => Sexo.NoAsignado
        };
    }
}

public sealed class HoraTimeSpanConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        if (TimeSpan.TryParse(text, out var resultado))
            return resultado;

        if (TimeSpan.TryParseExact(text, @"hh\:mm", null, out resultado))
            return resultado;

        return TimeSpan.Zero;
    }
}

public sealed class TipoPersonaConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        return text?.Trim().ToUpperInvariant() switch
        {
            "CONDUCTOR" => TipoPersona.Conductor,
            "PASAJERO" => TipoPersona.Pasajero,
            "PEATON" or "PEATÓN" => TipoPersona.Peaton,
            _ => TipoPersona.Conductor
        };
    }
}

public sealed class SiNoConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        var value = text?.Trim().ToUpperInvariant();
        return value is "S" or "SI" or "1";
    }
}
