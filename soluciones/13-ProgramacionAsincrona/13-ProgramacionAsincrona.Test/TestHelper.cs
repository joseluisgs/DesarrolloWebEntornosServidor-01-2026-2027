using NUnit.Framework;

namespace _13_ProgramacionAsincrona.Test;

internal static class TestHelper
{
    internal static string ObtenerRutaCsv()
    {
        var rutaCsv = Path.Combine(TestContext.CurrentContext.TestDirectory, "data", "2025_Accidentalidad.csv");
        if (!File.Exists(rutaCsv))
            rutaCsv = Path.Combine(AppContext.BaseDirectory, "data", "2025_Accidentalidad.csv");
        return rutaCsv;
    }
}
