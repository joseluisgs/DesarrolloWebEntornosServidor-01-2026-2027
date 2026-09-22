using _13_ProgramacionAsincrona.Models;

namespace _13_ProgramacionAsincrona.Services;

/// <summary>
/// Opción 1: Async/Await clásico.
/// Lee TODO el CSV de golpe → List<T> → procesa.
/// 
/// Ventaja: simple, código legible.
/// Desventaja: carga TODO en memoria (puede ser mucho RAM).
/// Cuándo usar: datos pequeños/medianos, cuando necesitas toda la lista.
/// </summary>
public sealed class AsyncAwaitService(Repositories.AccidentesCsvRepository repository)
{
    public async Task<int> ContarTotalAsync()
    {
        var accidentes = await repository.GetAllAsync();
        return accidentes.Count;
    }

    public async Task<List<Accidente>> FiltrarAlcoholAsync()
    {
        var accidentes = await repository.GetAllAsync();
        return accidentes.Where(a => a.PositivoAlcohol).ToList();
    }
}
