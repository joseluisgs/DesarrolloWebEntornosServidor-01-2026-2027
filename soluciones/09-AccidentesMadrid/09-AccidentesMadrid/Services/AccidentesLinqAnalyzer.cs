using System.Collections.Concurrent;
using AccidentesMadrid.Models;

namespace AccidentesMadrid.Services;

/// <summary>
/// Analiza accidentes usando LINQ, PLINQ y ejecución paralela de consultas.
///
/// PLINQ se usa en GroupBy PESADOS (muchos registros + muchos grupos).
/// Las consultas simples (Count, Where) son tan rápidas que PLINQ las haría más lentas.
///
/// Ejecución paralela: Las 30 consultas son INDEPENDIENTES entre sí.
/// Ninguna depende del resultado de otra, por eso se pueden ejecutar todas
/// a la vez con Task.WhenAll.
/// </summary>
public sealed class AccidentesLinqAnalyzer : IAccidentesAnalyzer
{
    // ══════════════════════════════════════════════════════════════
    // VERSIÓN SECUENCIAL — Las consultas se ejecutan una detrás de otra
    // ══════════════════════════════════════════════════════════════
    public void EjecutarConsultas(IReadOnlyList<Accidente> accidentes)
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("  CONSULTAS LINQ / PLINQ — SECUENCIAL");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine();

        EjecutarConsultasBase(accidentes);
        EjecutarConsultasTemporales(accidentes);
    }

    // ══════════════════════════════════════════════════════════════
    // VERSIÓN PARALELA — Las 30 consultas se ejecutan a la vez
    // ══════════════════════════════════════════════════════════════
    public async Task EjecutarConsultasParalelasAsync(IReadOnlyList<Accidente> accidentes)
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("  CONSULTAS LINQ / PLINQ — PARALELAS (Task.WhenAll)");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine();
        Console.WriteLine("  Las 30 consultas son INDEPENDIENTES: ninguna depende del");
        Console.WriteLine("  resultado de otra. Por eso se pueden ejecutar todas a la vez.");
        Console.WriteLine("  Cada consulta se ejecuta en un HILO DEL POOL distinto.");
        Console.WriteLine();

        // Lanzamos cada consulta en un Task independiente (30 tareas = 1 por consulta).
        //
        // ¿POR QUÉ funciona?
        // Porque las consultas solo LEESEN la lista (no la modifican).
        // Múltiples hilos pueden LEER la misma lista en paralelo sin problemas.
        // Si alguna consulta ESCRIBIERA en la lista, necesitaríamos sincronización.

        var tareas = ObtenerConsultas()
            .Select(consulta => Task.Run(() => consulta(accidentes)))
            .ToList();

        // Esperar a que terminen las 30 consultas
        await Task.WhenAll(tareas);

        Console.WriteLine();
        Console.WriteLine("  ✅ Las 30 consultas terminaron.");
        Console.WriteLine($"  🧵 Se usaron múltiples hilos del ThreadPool en paralelo.");
        Console.WriteLine();
    }

    // ══════════════════════════════════════════════════════════════
    // VERSIÓN POR LOTES — N tareas; cada tarea ejecuta su bloque de
    // consultas en secuencia. Ni 1 sola tarea, ni 30: el equilibrio.
    // ══════════════════════════════════════════════════════════════
    public async Task EjecutarConsultasEnLotesAsync(IReadOnlyList<Accidente> accidentes, int numeroTareas)
    {
        if (numeroTareas < 1 || numeroTareas > 30)
            throw new ArgumentOutOfRangeException(nameof(numeroTareas), numeroTareas, "Debe estar entre 1 y 30.");
        if (30 % numeroTareas != 0)
            throw new ArgumentOutOfRangeException(nameof(numeroTareas), numeroTareas,
                "30 consultas deben repartirse en partes iguales: usa 1, 2, 3, 5, 6, 10, 15 o 30.");

        var consultas = ObtenerConsultas();
        var consultasPorTarea = consultas.Length / numeroTareas;

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine($"  CONSULTAS LINQ / PLINQ — POR LOTES: {numeroTareas} tareas × {consultasPorTarea} consultas");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine();
        Console.WriteLine($"  {numeroTareas} tareas en paralelo; cada una ejecuta {consultasPorTarea} consultas seguidas.");
        Console.WriteLine("  Menos tareas = menos overhead, pero menos paralelismo.");
        Console.WriteLine("  Buscamos el EQUILIBRIO entre tareas, hilos y recursos libres.");
        Console.WriteLine();

        var tareas = new List<Task>(numeroTareas);
        for (var t = 0; t < numeroTareas; t++)
        {
            var inicio = t * consultasPorTarea;
            tareas.Add(Task.Run(() =>
            {
                // Cada tarea ejecuta su bloque de consultas en secuencia
                for (var i = inicio; i < inicio + consultasPorTarea; i++)
                    consultas[i](accidentes);
            }));
        }

        await Task.WhenAll(tareas);

        Console.WriteLine();
        Console.WriteLine($"  ✅ {numeroTareas} tareas terminaron ({consultas.Length} consultas en lotes de {consultasPorTarea}).");
        Console.WriteLine();
    }

    // ══════════════════════════════════════════════════════════════
    // MOTOR PARALLEL.FOR — Una iteración por consulta.
    // Alternativa "data parallel" de la BCL al Task.WhenAll.
    // ══════════════════════════════════════════════════════════════
    public async Task EjecutarConsultasParallelForAsync(IReadOnlyList<Accidente> accidentes)
    {
        var consultas = ObtenerConsultas();

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("  CONSULTAS LINQ / PLINQ — MOTOR PARALLEL.FOR (una iteración/consulta)");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine();
        Console.WriteLine("  Parallel.For reparte las 30 iteraciones entre el ThreadPool.");
        Console.WriteLine("  Es la alternativa \"data parallel\" de la BCL: menos código que");
        Console.WriteLine("  gestionar 30 Task.Run, pero BLOQUEANTE (no hay await interno).");
        Console.WriteLine();

        await Task.Run(() =>
            Parallel.For(0, consultas.Length, i => consultas[i](accidentes)));

        Console.WriteLine();
        Console.WriteLine("  ✅ Las 30 iteraciones de Parallel.For terminaron.");
        Console.WriteLine();
    }

    // ══════════════════════════════════════════════════════════════
    // MOTOR PARALLEL.FOR + PARTITIONER — Paquetes EXACTOS de
    // 30/N consultas, con MaxDegreeOfParallelism = N.
    // ══════════════════════════════════════════════════════════════
    public async Task EjecutarConsultasPorLotesParallelForAsync(IReadOnlyList<Accidente> accidentes, int numeroTareas)
    {
        if (numeroTareas < 1 || numeroTareas > 30)
            throw new ArgumentOutOfRangeException(nameof(numeroTareas), numeroTareas, "Debe estar entre 1 y 30.");
        if (30 % numeroTareas != 0)
            throw new ArgumentOutOfRangeException(nameof(numeroTareas), numeroTareas,
                "30 consultas deben repartirse en partes iguales: usa 1, 2, 3, 5, 6, 10, 15 o 30.");

        var consultas = ObtenerConsultas();
        var consultasPorTarea = consultas.Length / numeroTareas;

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine($"  CONSULTAS LINQ / PLINQ — PARALLEL.FOR POR LOTES: {numeroTareas} paquetes × {consultasPorTarea} consultas");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine();
        Console.WriteLine($"  Partitioner.Create(0, 30, {consultasPorTarea}) genera {numeroTareas} rangos EXACTOS;");
        Console.WriteLine($"  MaxDegreeOfParallelism = {numeroTareas} los ejecuta como máximo a la vez.");
        Console.WriteLine("  Cada rango ejecuta su bloque de consultas de forma secuencial.");
        Console.WriteLine();

        var opciones = new ParallelOptions { MaxDegreeOfParallelism = numeroTareas };
        var paquetes = Partitioner.Create(0, consultas.Length, consultasPorTarea);

        await Task.Run(() => Parallel.ForEach(paquetes, opciones, rango =>
        {
            for (var i = rango.Item1; i < rango.Item2; i++)
                consultas[i](accidentes);
        }));

        Console.WriteLine();
        Console.WriteLine($"  ✅ {numeroTareas} paquetes terminaron ({consultas.Length} consultas en bloques de {consultasPorTarea}).");
        Console.WriteLine();
    }

    /// <summary>
    /// Las 30 consultas en orden, como acciones reutilizables.
    /// Fuente única para los modos paralelo y por lotes.
    /// </summary>
    private Action<IReadOnlyList<Accidente>>[] ObtenerConsultas() =>
    [
        // ── Consultas base (1-22) ──
        EjecutarConsulta1,
        EjecutarConsulta2,
        EjecutarConsulta3,
        EjecutarConsulta4,
        EjecutarConsulta5,
        EjecutarConsulta6,
        EjecutarConsulta7,
        EjecutarConsulta8,
        EjecutarConsulta9,
        EjecutarConsulta10,
        EjecutarConsulta11,
        EjecutarConsulta12,
        EjecutarConsulta13,
        EjecutarConsulta14,
        EjecutarConsulta15,
        EjecutarConsulta16,
        EjecutarConsulta17,
        EjecutarConsulta18,
        EjecutarConsulta19,
        EjecutarConsulta20,
        EjecutarConsulta21,
        EjecutarConsulta22,
        // ── Consultas temporales (23-30) ──
        EjecutarConsulta23,
        EjecutarConsulta24,
        EjecutarConsulta25,
        EjecutarConsulta26,
        EjecutarConsulta27,
        EjecutarConsulta28,
        EjecutarConsulta29,
        EjecutarConsulta30
    ];

    // ══════════════════════════════════════════════════════════════
    // MÉTODOS AUXILIARES — Cada consulta es un método independiente
    // para poder ejecutarse en paralelo con Task.Run
    // ══════════════════════════════════════════════════════════════

    private void EjecutarConsultasBase(IReadOnlyList<Accidente> accidentes)
    {
        // 1. Total de accidentes
        EjecutarConsulta1(accidentes);

        // 2. Accidentes por distrito
        EjecutarConsulta2(accidentes);

        // 3. Accidentes por tipo
        EjecutarConsulta3(accidentes);

        // 4. Accidentes por estado meteorológico
        EjecutarConsulta4(accidentes);

        // 5. Accidentes por sexo
        EjecutarConsulta5(accidentes);

        // 6. Accidentes por rango de edad
        EjecutarConsulta6(accidentes);

        // 7. Positivos en alcohol
        EjecutarConsulta7(accidentes);

        // 8. Positivos en drogas
        EjecutarConsulta8(accidentes);

        // 9. Accidentes por día de la semana
        EjecutarConsulta9(accidentes);

        // 10. Accidentes por mes
        EjecutarConsulta10(accidentes);

        // 11. Hora con más accidentes
        EjecutarConsulta11(accidentes);

        // 12. Lesiones más frecuentes
        EjecutarConsulta12(accidentes);

        // 13. Tipo de vehículo más implicado
        EjecutarConsulta13(accidentes);

        // 14. Accidentes con peatones
        EjecutarConsulta14(accidentes);

        // 15. Proporción hombre/mujer
        EjecutarConsulta15(accidentes);

        // 16. Distrito con más peatones
        EjecutarConsulta16(accidentes);

        // 17. Fin de semana vs entre semana
        EjecutarConsulta17(accidentes);

        // 18. Media de accidentes por día
        EjecutarConsulta18(accidentes);

        // 19. Alcohol + droga
        EjecutarConsulta19(accidentes);

        // 20. Edad vulnerable (peatones)
        EjecutarConsulta20(accidentes);

        // 21. Distrito con más alcohol
        EjecutarConsulta21(accidentes);

        // 22. Accidentes por código de distrito
        EjecutarConsulta22(accidentes);
    }

    private void EjecutarConsultasTemporales(IReadOnlyList<Accidente> accidentes)
    {
        EjecutarConsulta23(accidentes);
        EjecutarConsulta24(accidentes);
        EjecutarConsulta25(accidentes);
        EjecutarConsulta26(accidentes);
        EjecutarConsulta27(accidentes);
        EjecutarConsulta28(accidentes);
        EjecutarConsulta29(accidentes);
        EjecutarConsulta30(accidentes);
    }

    // ══════════════════════════════════════════════════════════════
    // CONSULTAS 1-22: Base
    // ══════════════════════════════════════════════════════════════

    private void EjecutarConsulta1(IReadOnlyList<Accidente> accidentes)
    {
        var total = accidentes.Count;
        Console.WriteLine($"1. Total de accidentes: {total}");
    }

    private void EjecutarConsulta2(IReadOnlyList<Accidente> accidentes)
    {
        var porDistrito = accidentes
            .GroupBy(a => a.Distrito)
            .OrderByDescending(g => g.Count())
            .Select(g => new { Distrito = g.Key, Cantidad = g.Count() })
            .ToList();
        Console.WriteLine("\n2. Accidentes por distrito (top 5):");
        foreach (var d in porDistrito.Take(5))
            Console.WriteLine($"   {d.Distrito}: {d.Cantidad}");
    }

    private void EjecutarConsulta3(IReadOnlyList<Accidente> accidentes)
    {
        var porTipo = accidentes
            .GroupBy(a => a.TipoAccidente)
            .OrderByDescending(g => g.Count())
            .Select(g => new { Tipo = g.Key, Cantidad = g.Count() })
            .ToList();
        Console.WriteLine("\n3. Accidentes por tipo:");
        foreach (var t in porTipo)
            Console.WriteLine($"   {t.Tipo}: {t.Cantidad}");
    }

    private void EjecutarConsulta4(IReadOnlyList<Accidente> accidentes)
    {
        var porMeteo = accidentes
            .GroupBy(a => a.EstadoMeteorologico)
            .OrderByDescending(g => g.Count())
            .Select(g => new { Meteorologia = g.Key, Cantidad = g.Count() })
            .ToList();
        Console.WriteLine("\n4. Accidentes por estado meteorológico:");
        foreach (var m in porMeteo)
            Console.WriteLine($"   {m.Meteorologia}: {m.Cantidad}");
    }

    private void EjecutarConsulta5(IReadOnlyList<Accidente> accidentes)
    {
        var porSexo = accidentes
            .GroupBy(a => a.Sexo)
            .OrderByDescending(g => g.Count())
            .Select(g => new { Sexo = g.Key, Cantidad = g.Count() })
            .ToList();
        Console.WriteLine("\n5. Accidentes por sexo:");
        foreach (var s in porSexo)
            Console.WriteLine($"   {s.Sexo}: {s.Cantidad}");
    }

    private void EjecutarConsulta6(IReadOnlyList<Accidente> accidentes)
    {
        var porEdad = accidentes
            .GroupBy(a => a.RangoEdad)
            .OrderByDescending(g => g.Count())
            .Select(g => new { Rango = g.Key, Cantidad = g.Count() })
            .ToList();
        Console.WriteLine("\n6. Accidentes por rango de edad:");
        foreach (var e in porEdad)
            Console.WriteLine($"   {e.Rango}: {e.Cantidad}");
    }

    private void EjecutarConsulta7(IReadOnlyList<Accidente> accidentes)
    {
        var alcohol = accidentes.Count(a => a.PositivoAlcohol);
        Console.WriteLine($"\n7. Positivos en alcohol: {alcohol}");
    }

    private void EjecutarConsulta8(IReadOnlyList<Accidente> accidentes)
    {
        var drogas = accidentes.Count(a => a.PositivoDroga);
        Console.WriteLine($"8. Positivos en drogas: {drogas}");
    }

    private void EjecutarConsulta9(IReadOnlyList<Accidente> accidentes)
    {
        var porDiaSemana = accidentes
            .GroupBy(a => a.Fecha.DayOfWeek)
            .OrderByDescending(g => g.Count())
            .Select(g => new { Dia = g.Key, Cantidad = g.Count() })
            .ToList();
        Console.WriteLine("\n9. Accidentes por día de la semana:");
        foreach (var d in porDiaSemana)
            Console.WriteLine($"   {d.Dia}: {d.Cantidad}");
    }

    private void EjecutarConsulta10(IReadOnlyList<Accidente> accidentes)
    {
        var porMes = accidentes
            .GroupBy(a => a.Fecha.Month)
            .OrderBy(g => g.Key)
            .Select(g => new { Mes = g.Key, Cantidad = g.Count() })
            .ToList();
        Console.WriteLine("\n10. Accidentes por mes:");
        foreach (var m in porMes)
            Console.WriteLine($"   Mes {m.Mes}: {m.Cantidad}");
    }

    private void EjecutarConsulta11(IReadOnlyList<Accidente> accidentes)
    {
        var horaPico = accidentes
            .GroupBy(a => a.Hora.Hours)
            .OrderByDescending(g => g.Count())
            .First();
        Console.WriteLine($"\n11. Hora con más accidentes: {horaPico.Key}:00 ({horaPico.Count()} accidentes)");
    }

    private void EjecutarConsulta12(IReadOnlyList<Accidente> accidentes)
    {
        var porLesion = accidentes
            .GroupBy(a => a.Lesividad)
            .OrderByDescending(g => g.Count())
            .Select(g => new { Lesion = g.Key, Cantidad = g.Count() })
            .ToList();
        Console.WriteLine("\n12. Lesiones más frecuentes:");
        foreach (var l in porLesion.Take(5))
            Console.WriteLine($"   {l.Lesion}: {l.Cantidad}");
    }

    private void EjecutarConsulta13(IReadOnlyList<Accidente> accidentes)
    {
        var porVehiculo = accidentes
            .GroupBy(a => a.TipoVehiculo)
            .OrderByDescending(g => g.Count())
            .Select(g => new { Vehiculo = g.Key, Cantidad = g.Count() })
            .ToList();
        Console.WriteLine("\n13. Vehículos más implicados:");
        foreach (var v in porVehiculo)
            Console.WriteLine($"   {v.Vehiculo}: {v.Cantidad}");
    }

    private void EjecutarConsulta14(IReadOnlyList<Accidente> accidentes)
    {
        var peatones = accidentes.Count(a => a.TipoPersona == TipoPersona.Peaton);
        Console.WriteLine($"\n14. Accidentes con peatones: {peatones}");
    }

    private void EjecutarConsulta15(IReadOnlyList<Accidente> accidentes)
    {
        var hombres = accidentes.Count(a => a.Sexo == Sexo.Hombre);
        var mujeres = accidentes.Count(a => a.Sexo == Sexo.Mujer);
        Console.WriteLine($"\n15. Proporción H/M: {hombres}/{mujeres} ({(mujeres > 0 ? (double)hombres / mujeres : 0):F2})");
    }

    private void EjecutarConsulta16(IReadOnlyList<Accidente> accidentes)
    {
        var distritoPeatones = accidentes
            .Where(a => a.TipoPersona == TipoPersona.Peaton)
            .GroupBy(a => a.Distrito)
            .OrderByDescending(g => g.Count())
            .Select(g => new { Distrito = g.Key, Cantidad = g.Count() })
            .ToList();
        Console.WriteLine("\n16. Distritos con más peatones:");
        foreach (var d in distritoPeatones.Take(3))
            Console.WriteLine($"   {d.Distrito}: {d.Cantidad}");
    }

    private void EjecutarConsulta17(IReadOnlyList<Accidente> accidentes)
    {
        var total = accidentes.Count;
        var finDeSemana = accidentes.Count(a => a.Fecha.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday);
        var entreSemana = total - finDeSemana;
        Console.WriteLine($"\n17. Fin de semana: {finDeSemana} | Entre semana: {entreSemana}");
    }

    private void EjecutarConsulta18(IReadOnlyList<Accidente> accidentes)
    {
        var total = accidentes.Count;
        var diasDistintos = accidentes.Select(a => a.Fecha.Date).Distinct().Count();
        var mediaPorDia = diasDistintos > 0 ? (double)total / diasDistintos : 0;
        Console.WriteLine($"\n18. Media de accidentes por día: {mediaPorDia:F2}");
    }

    private void EjecutarConsulta19(IReadOnlyList<Accidente> accidentes)
    {
        var multiFactor = accidentes.Count(a => a.PositivoAlcohol && a.PositivoDroga);
        Console.WriteLine($"\n19. Accidentes con alcohol + droga: {multiFactor}");
    }

    private void EjecutarConsulta20(IReadOnlyList<Accidente> accidentes)
    {
        var edadVulnerable = accidentes
            .Where(a => a.TipoPersona == TipoPersona.Peaton)
            .GroupBy(a => a.RangoEdad)
            .OrderByDescending(g => g.Count())
            .Select(g => new { Rango = g.Key, Cantidad = g.Count() })
            .ToList();
        Console.WriteLine("\n20. Rangos de edad más vulnerables (peatones):");
        foreach (var e in edadVulnerable.Take(3))
            Console.WriteLine($"   {e.Rango}: {e.Cantidad}");
    }

    private void EjecutarConsulta21(IReadOnlyList<Accidente> accidentes)
    {
        var distritoAlcohol = accidentes
            .Where(a => a.PositivoAlcohol)
            .GroupBy(a => a.Distrito)
            .OrderByDescending(g => g.Count())
            .Select(g => new { Distrito = g.Key, Cantidad = g.Count() })
            .ToList();
        Console.WriteLine("\n21. Distritos con más positivos en alcohol:");
        foreach (var d in distritoAlcohol.Take(3))
            Console.WriteLine($"   {d.Distrito}: {d.Cantidad}");
    }

    private void EjecutarConsulta22(IReadOnlyList<Accidente> accidentes)
    {
        var porCodDistrito = accidentes
            .GroupBy(a => a.CodDistrito)
            .OrderBy(g => g.Key)
            .Select(g => new { Codigo = g.Key, Distrito = g.First().Distrito, Cantidad = g.Count() })
            .ToList();
        Console.WriteLine("\n22. Accidentes por código de distrito:");
        foreach (var d in porCodDistrito)
            Console.WriteLine($"   [{d.Codigo:D2}] {d.Distrito}: {d.Cantidad}");
    }

    // ══════════════════════════════════════════════════════════════
    // CONSULTAS 23-30: Temporales (análisis por año)
    // ══════════════════════════════════════════════════════════════

    private void EjecutarConsulta23(IReadOnlyList<Accidente> accidentes)
    {
        var porAnio = accidentes
            .GroupBy(a => a.Fecha.Year)
            .OrderBy(g => g.Key)
            .Select(g => new { Anio = g.Key, Cantidad = g.Count() })
            .ToList();
        Console.WriteLine("\n23. Accidentes por año:");
        foreach (var a in porAnio)
            Console.WriteLine($"   {a.Anio}: {a.Cantidad:N0}");
    }

    private void EjecutarConsulta24(IReadOnlyList<Accidente> accidentes)
    {
        // PLINQ: GroupBy compuesto pesado (~100K registros → ~36 grupos)
        var evolucionMensual = accidentes
            .AsParallel()
            .GroupBy(a => new { a.Fecha.Year, a.Fecha.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new { Anio = g.Key.Year, Mes = g.Key.Month, Cantidad = g.Count() })
            .ToList();
        Console.WriteLine("\n24. Evolución mensual por año (PLINQ) — primeros 12:");
        foreach (var e in evolucionMensual.Take(12))
            Console.WriteLine($"   {e.Anio}/{e.Mes:D2}: {e.Cantidad:N0}");
    }

    private void EjecutarConsulta25(IReadOnlyList<Accidente> accidentes)
    {
        // PLINQ: GroupBy anidado muy pesado
        var distritoPorAnio = accidentes
            .AsParallel()
            .GroupBy(a => a.Fecha.Year)
            .Select(g => new
            {
                Anio = g.Key,
                Distrito = g.GroupBy(a => a.Distrito)
                            .OrderByDescending(d => d.Count())
                            .First().Key,
                Cantidad = g.GroupBy(a => a.Distrito)
                            .OrderByDescending(d => d.Count())
                            .First().Count()
            })
            .OrderBy(x => x.Anio)
            .ToList();
        Console.WriteLine("\n25. Distrito más peligroso por año (PLINQ):");
        foreach (var d in distritoPorAnio)
            Console.WriteLine($"   {d.Anio}: {d.Distrito} ({d.Cantidad} accidentes)");
    }

    private void EjecutarConsulta26(IReadOnlyList<Accidente> accidentes)
    {
        var alcoholPorAnio = accidentes
            .GroupBy(a => a.Fecha.Year)
            .Select(g => new
            {
                Anio = g.Key,
                Total = g.Count(),
                ConAlcohol = g.Count(a => a.PositivoAlcohol),
                Porcentaje = g.Count() > 0 ? (double)g.Count(a => a.PositivoAlcohol) / g.Count() * 100 : 0
            })
            .OrderBy(x => x.Anio)
            .ToList();
        Console.WriteLine("\n26. Tendencia de alcohol por año:");
        foreach (var a in alcoholPorAnio)
            Console.WriteLine($"   {a.Anio}: {a.ConAlcohol} positivos de {a.Total} ({a.Porcentaje:F1}%)");
    }

    private void EjecutarConsulta27(IReadOnlyList<Accidente> accidentes)
    {
        var finDeSemanaPorAnio = accidentes
            .GroupBy(a => a.Fecha.Year)
            .Select(g => new
            {
                Anio = g.Key,
                EntreSemana = g.Count(a => a.Fecha.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday),
                FinDeSemana = g.Count(a => a.Fecha.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            })
            .OrderBy(x => x.Anio)
            .ToList();
        Console.WriteLine("\n27. Fin de semana vs entre semana por año:");
        foreach (var f in finDeSemanaPorAnio)
            Console.WriteLine($"   {f.Anio}: Entre semana {f.EntreSemana:N0} | Fin de semana {f.FinDeSemana:N0}");
    }

    private void EjecutarConsulta28(IReadOnlyList<Accidente> accidentes)
    {
        var horaPicoPorAnio = accidentes
            .GroupBy(a => a.Fecha.Year)
            .Select(g => new
            {
                Anio = g.Key,
                Hora = g.GroupBy(a => a.Hora.Hours)
                        .OrderByDescending(h => h.Count())
                        .First().Key,
                Cantidad = g.GroupBy(a => a.Hora.Hours)
                            .OrderByDescending(h => h.Count())
                            .First().Count()
            })
            .OrderBy(x => x.Anio)
            .ToList();
        Console.WriteLine("\n28. Hora pico por año:");
        foreach (var h in horaPicoPorAnio)
            Console.WriteLine($"   {h.Anio}: {h.Hora:D2}:00 ({h.Cantidad} accidentes)");
    }

    private void EjecutarConsulta29(IReadOnlyList<Accidente> accidentes)
    {
        var lesionesPorAnio = accidentes
            .GroupBy(a => a.Fecha.Year)
            .Select(g => new
            {
                Anio = g.Key,
                TopLesion = g.GroupBy(a => a.Lesividad)
                             .OrderByDescending(l => l.Count())
                             .First().Key,
                Cantidad = g.GroupBy(a => a.Lesividad)
                            .OrderByDescending(l => l.Count())
                            .First().Count()
            })
            .OrderBy(x => x.Anio)
            .ToList();
        Console.WriteLine("\n29. Lesión más frecuente por año:");
        foreach (var l in lesionesPorAnio)
            Console.WriteLine($"   {l.Anio}: {l.TopLesion} ({l.Cantidad} casos)");
    }

    private void EjecutarConsulta30(IReadOnlyList<Accidente> accidentes)
    {
        var evolucionPeatones = accidentes
            .GroupBy(a => a.Fecha.Year)
            .OrderBy(g => g.Key)
            .Select(g => new
            {
                Anio = g.Key,
                Total = g.Count(),
                Peatones = g.Count(a => a.TipoPersona == TipoPersona.Peaton)
            })
            .ToList();
        Console.WriteLine("\n30. Evolución de peatones por año:");
        foreach (var p in evolucionPeatones)
        {
            var porcentaje = p.Total > 0 ? (double)p.Peatones / p.Total * 100 : 0;
            Console.WriteLine($"   {p.Anio}: {p.Peatones:N0} peatones de {p.Total:N0} ({porcentaje:F1}%)");
        }
    }
}
