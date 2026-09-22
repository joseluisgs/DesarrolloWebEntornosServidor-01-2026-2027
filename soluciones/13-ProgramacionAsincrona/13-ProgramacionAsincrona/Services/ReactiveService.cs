using System.Reactive.Linq;
using System.Reactive.Subjects;
using _13_ProgramacionAsincrona.Models;

namespace _13_ProgramacionAsincrona.Services;

/// <summary>
/// Opción 3: Rx.NET con Observable.
/// Lee línea a línea con StreamReader → OnNext cada registro → suscriptores filtran en tiempo real.
/// 
/// Ventaja: múltiples suscriptores, filtrado en tiempo real, composición de flujos.
/// Desventaja: más complejo, overhead de Rx.
/// Cuándo usar: datos en tiempo real, sensores, logs, cuando necesitas múltiples consumidores.
/// </summary>
public sealed class ReactiveService(Repositories.AccidentesCsvRepository repository)
{
    public async Task<(int Total, int ConAlcohol)> ProcesarObservableAsync()
    {
        // Observable que lee el CSV línea a línea y emite cada registro
        var observable = repository.CrearObservable();

        int total = 0;
        int conAlcohol = 0;

        // Suscriptor 1: cuenta total
        Console.WriteLine("  Suscriptor 1: cuenta total de registros");
        var subTotal = observable.Subscribe(_ => total++);

        // Suscriptor 2: filtra alcohol en tiempo real
        Console.WriteLine("  Suscriptor 2: filtra registros con alcohol");
        var subAlcohol = observable
            .Where(a => a.PositivoAlcohol)
            .Subscribe(a =>
            {
                conAlcohol++;
                Console.WriteLine($"    [ALCOHOL] {a.NumExpediente} — {a.Distrito}");
            });

        // Suscriptor 3: muestra los 5 primeros
        Console.WriteLine("  Suscriptor 3: muestra los 5 primeros registros");
        int mostrados = 0;
        var subPrimeros = observable
            .Take(5)
            .Subscribe(a =>
            {
                mostrados++;
                Console.WriteLine($"    [PRIMERO #{mostrados}] {a.NumExpediente} — {a.Fecha:dd/MM/yyyy}");
            });

        // Esperar a que termine el observable
        Console.WriteLine("  Esperando a que termine el observable...");
        var tcs = new TaskCompletionSource();
        observable.Subscribe(_ => { }, () => tcs.TrySetResult());
        await tcs.Task;

        subTotal.Dispose();
        subAlcohol.Dispose();
        subPrimeros.Dispose();

        return (total, conAlcohol);
    }
}
