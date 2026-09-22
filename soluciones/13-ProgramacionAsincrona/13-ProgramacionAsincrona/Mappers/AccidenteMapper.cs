using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using _13_ProgramacionAsincrona.Models;

namespace _13_ProgramacionAsincrona.Mappers;

public sealed class AccidenteMapper : ClassMap<Accidente>
{
    public AccidenteMapper()
    {
        Map(a => a.NumExpediente).Name("num_expediente");
        Map(a => a.Fecha).Name("fecha").TypeConverterOption.Format("dd/MM/yyyy");
        Map(a => a.Hora).Name("hora");
        Map(a => a.Distrito).Name("distrito");
        Map(a => a.TipoAccidente).Name("tipo_accidente");
        Map(a => a.Sexo).Name("sexo");
        Map(a => a.PositivoAlcohol).Name("positiva_alcohol").TypeConverter(new SiNoConverter());
        Map(a => a.PositivoDroga).Name("positiva_droga").TypeConverter(new SiNoConverter());
    }
}

public sealed class SiNoConverter : DefaultTypeConverter
{
    public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
    {
        return text?.Trim().ToUpperInvariant() is "S" or "SI" or "1";
    }
}
