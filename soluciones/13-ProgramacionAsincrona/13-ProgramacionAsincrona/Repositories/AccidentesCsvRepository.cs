using System.Globalization;
using System.Text;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CsvHelper;
using CsvHelper.Configuration;
using _13_ProgramacionAsincrona.Mappers;
using _13_ProgramacionAsincrona.Models;

namespace _13_ProgramacionAsincrona.Repositories;

/// <summary>
/// Lee el CSV de accidentes de Madrid.
/// </summary>
public sealed class AccidentesCsvRepository(string rutaCsv)
{
    private static CsvConfiguration CreateConfig() => new(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = true,
        HeaderValidated = null,
        MissingFieldFound = null,
        BadDataFound = null,
        TrimOptions = TrimOptions.Trim,
        Delimiter = ";"
    };

    /// <summary>
    /// Lee TODO el CSV de golpe y devuelve una lista completa.
    /// Usado por Async/Await.
    /// </summary>
    public async Task<List<Accidente>> GetAllAsync()
    {
        return await Task.Run(() =>
        {
            using var reader = new StreamReader(rutaCsv, Encoding.UTF8);
            using var csv = new CsvReader(reader, CreateConfig());
            csv.Context.RegisterClassMap<AccidenteMapper>();
            return csv.GetRecords<Accidente>().ToList();
        });
    }

    /// <summary>
    /// Lee el CSV línea a línea con yield return (IAsyncEnumerable).
    /// Cada registro se devuelve cuando el consumidor lo pide (pull).
    /// </summary>
    public async IAsyncEnumerable<Accidente> GetAllAsyncEnumerable(
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(rutaCsv, Encoding.UTF8);
        using var csv = new CsvReader(reader, CreateConfig());
        csv.Context.RegisterClassMap<AccidenteMapper>();

        await foreach (var accidente in csv.GetRecordsAsync<Accidente>(cancellationToken))
        {
            yield return accidente;
        }
    }

    /// <summary>
    /// Crea un Observable que lee el CSV línea a línea y emite cada registro.
    /// Rx.NET lee con StreamReader normal (síncrono) y publica con OnNext.
    /// </summary>
    public IObservable<Accidente> CrearObservable()
    {
        return Observable.Create<Accidente>(observer =>
        {
            using var reader = new StreamReader(rutaCsv, Encoding.UTF8);
            using var csv = new CsvReader(reader, CreateConfig());
            csv.Context.RegisterClassMap<AccidenteMapper>();

            foreach (var accidente in csv.GetRecords<Accidente>())
            {
                observer.OnNext(accidente);
            }

            observer.OnCompleted();
            return System.Reactive.Disposables.Disposable.Empty;
        });
    }
}
