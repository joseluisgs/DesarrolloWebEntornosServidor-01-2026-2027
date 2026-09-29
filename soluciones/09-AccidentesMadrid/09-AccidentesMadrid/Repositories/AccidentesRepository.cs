using System.Globalization;
using System.Text;
using AccidentesMadrid.Mappers;
using AccidentesMadrid.Models;
using CsvHelper;
using CsvHelper.Configuration;

namespace AccidentesMadrid.Repositories;

/// <summary>
/// Repositorio de accidentes: lectura de los CSV descargados de datos.madrid.es.
/// </summary>
/// <remarks>
/// La lectura de un fichero es E/S REAL: mientras el disco entrega bytes,
/// el hilo no debería estar ocupado esperando. Por eso usamos las versiones
/// async de StreamReader y de CsvHelper: cada espera devuelve el hilo al
/// ThreadPool y continúa cuando llegan los datos.
/// <para>
/// ❌ MALO: <c>Task.Run(() =&gt; LecturaSincrona())</c> — ocupa un hilo del
/// pool esperando al disco durante toda la lectura (falso async).
/// ✅ BUENO: <c>await</c> sobre E/S async — libera el hilo mientras espera.
/// </para>
/// </remarks>
public sealed class AccidentesRepository(string rutaCsv) : IAccidentesRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<Accidente>> GetAllAsync()
    {
        // FileStream con useAsync: las lecturas del disco se completan sin
        // bloquear el hilo (devolución de llamada en vez de espera activa).
        await using var stream = new FileStream(
            rutaCsv, FileMode.Open, FileAccess.Read, FileShare.Read,
            bufferSize: 4096, useAsync: true);

        using var reader = new StreamReader(stream, Encoding.UTF8);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            HeaderValidated = null,
            MissingFieldFound = null,
            BadDataFound = null,
            TrimOptions = TrimOptions.Trim,
            Delimiter = ";"              // El CSV real usa punto y coma
        });

        csv.Context.RegisterClassMap<AccidenteMapper>();

        // GetRecordsAsync: CsvHelper consume el stream con ReadAsync.
        // Cada espera de E/S libera el hilo: 3 lecturas a la vez NO
        // ocupan 3 hilos del pool mientras esperan al disco.
        var accidentes = new List<Accidente>();
        await foreach (var accidente in csv.GetRecordsAsync<Accidente>())
        {
            accidentes.Add(accidente);
        }

        return accidentes;
    }
}
