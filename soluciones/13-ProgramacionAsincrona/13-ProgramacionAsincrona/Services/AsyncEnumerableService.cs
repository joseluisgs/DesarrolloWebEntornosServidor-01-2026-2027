using _13_ProgramacionAsincrona.Models;

namespace _13_ProgramacionAsincrona.Services;

/// <summary>
/// Opción 2: IAsyncEnumerable con yield return.
/// Lee línea a línea → yield cada registro → procesa uno a uno.
/// 
/// Ventaja: no carga todo en memoria, procesa bajo demanda (pull).
/// Desventaja: más complejo, solo hay un consumidor a la vez.
/// Cuándo usar: CSV grandes, cuando no necesitas toda la lista.
/// </summary>
public sealed class AsyncEnumerableService(Repositories.AccidentesCsvRepository repository)
{
    public async Task<(int Total, int ConAlcohol)> ContarYFiltrarAsync()
    {
        int total = 0;
        int conAlcohol = 0;

        await foreach (var accidente in repository.GetAllAsyncEnumerable()
                           .Where(a => a.PositivoAlcohol || !a.PositivoAlcohol))
        {
            total++;

            if (accidente.PositivoAlcohol)
                conAlcohol++;

            // Mostrar progreso cada 10.000 registros
            if (total % 10000 == 0)
                Console.WriteLine($"    Procesadas: {total:N0}...");
        }

        return (total, conAlcohol);
    }
}
