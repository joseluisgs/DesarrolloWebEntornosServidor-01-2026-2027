using AccidentesMadrid.Models;

namespace AccidentesMadrid.Services;

/// <summary>
/// Analizador de las 30 consultas sobre los accidentes.
/// Permite tres formas de ejecución: secuencial, totalmente paralela
/// y por lotes (N tareas con varias consultas cada una).
/// </summary>
public interface IAccidentesAnalyzer
{
    /// <summary>
    /// Ejecuta las 30 consultas una detrás de otra (1 sola tarea).
    /// </summary>
    void EjecutarConsultas(IReadOnlyList<Accidente> accidentes);

    /// <summary>
    /// Ejecuta las 30 consultas en paralelo: una tarea por consulta (Task.WhenAll).
    /// </summary>
    Task EjecutarConsultasParalelasAsync(IReadOnlyList<Accidente> accidentes);

    /// <summary>
    /// Ejecuta las 30 consultas repartidas en <paramref name="numeroTareas"/> tareas.
    /// Cada tarea ejecuta su bloque de consultas de forma secuencial y las
    /// <paramref name="numeroTareas"/> tareas corren en paralelo (Task.WhenAll).
    /// </summary>
    /// <param name="accidentes">Datos combinados de los 3 CSV.</param>
    /// <param name="numeroTareas">Número de tareas (lotes). Debe dividir 30:
    /// 2, 3, 5, 6, 10, 15 o 30.</param>
    Task EjecutarConsultasEnLotesAsync(IReadOnlyList<Accidente> accidentes, int numeroTareas);

    /// <summary>
    /// Ejecuta las 30 consultas con el motor Parallel.For (una iteración por
    /// consulta). Alternativa "data parallel" de la BCL al Task.WhenAll.
    /// </summary>
    Task EjecutarConsultasParallelForAsync(IReadOnlyList<Accidente> accidentes);

    /// <summary>
    /// Ejecuta las 30 consultas con Parallel.ForEach + Partitioner.Create:
    /// paquetes EXACTOS de 30/<paramref name="numeroTareas"/> consultas,
    /// con MaxDegreeOfParallelism limitado al número de tareas.
    /// </summary>
    /// <param name="accidentes">Datos combinados de los 3 CSV.</param>
    /// <param name="numeroTareas">Número de paquetes. Debe dividir 30:
    /// 2, 3, 5, 6, 10, 15 o 30.</param>
    Task EjecutarConsultasPorLotesParallelForAsync(IReadOnlyList<Accidente> accidentes, int numeroTareas);
}
