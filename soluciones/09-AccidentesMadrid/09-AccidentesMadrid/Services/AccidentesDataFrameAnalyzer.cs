using System.Collections.Concurrent;
using AccidentesMadrid.Models;
using Microsoft.Data.Analysis;

namespace AccidentesMadrid.Services;

/// <summary>
/// Analiza accidentes usando DataFrames (Microsoft.Data.Analysis).
///
/// El DataFrame se crea una vez y las consultas lo LEEN en paralelo.
/// Como las consultas solo leen (no modifican), se pueden ejecutar todas
/// a la vez con Task.WhenAll sin problemas de concurrencia.
/// </summary>
public sealed class AccidentesDataFrameAnalyzer : IAccidentesAnalyzer
{
    private DataFrame? _df;

    // ══════════════════════════════════════════════════════════════
    // VERSIÓN SECUENCIAL
    // ══════════════════════════════════════════════════════════════
    public void EjecutarConsultas(IReadOnlyList<Accidente> accidentes)
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("  CONSULTAS CON DATAFRAMES — SECUENCIAL");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine();

        _df = CrearDataFrame(accidentes);
        Console.WriteLine($"DataFrame creado: {_df.Rows.Count} filas, {_df.Columns.Count} columnas");
        Console.WriteLine();

        EjecutarConsultasBase(_df);
        EjecutarConsultasTemporales(_df);
    }

    // ══════════════════════════════════════════════════════════════
    // VERSIÓN PARALELA — Las 30 consultas se ejecutan a la vez
    // ══════════════════════════════════════════════════════════════
    public async Task EjecutarConsultasParalelasAsync(IReadOnlyList<Accidente> accidentes)
    {
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("  CONSULTAS CON DATAFRAMES — PARALELAS (Task.WhenAll)");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine();
        Console.WriteLine("  Las 30 consultas son INDEPENDIENTES: ninguna depende del");
        Console.WriteLine("  resultado de otra. Se ejecutan todas a la vez.");
        Console.WriteLine();

        var df = _df ?? CrearDataFrame(accidentes);

        var tareas = ObtenerConsultas()
            .Select(consulta => Task.Run(() => consulta(df)))
            .ToList();

        await Task.WhenAll(tareas);

        Console.WriteLine();
        Console.WriteLine("  ✅ Las 30 consultas DataFrame terminaron en paralelo.");
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

        var df = _df ?? CrearDataFrame(accidentes);
        var consultas = ObtenerConsultas();
        var consultasPorTarea = consultas.Length / numeroTareas;

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine($"  CONSULTAS CON DATAFRAMES — POR LOTES: {numeroTareas} tareas × {consultasPorTarea} consultas");
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
                    consultas[i](df);
            }));
        }

        await Task.WhenAll(tareas);

        Console.WriteLine();
        Console.WriteLine($"  ✅ {numeroTareas} tareas terminaron ({consultas.Length} consultas en lotes de {consultasPorTarea}).");
        Console.WriteLine();
    }

    // ══════════════════════════════════════════════════════════════
    // MOTOR PARALLEL.FOR — Una iteración por consulta
    // ══════════════════════════════════════════════════════════════
    public async Task EjecutarConsultasParallelForAsync(IReadOnlyList<Accidente> accidentes)
    {
        var df = _df ?? CrearDataFrame(accidentes);
        var consultas = ObtenerConsultas();

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("  CONSULTAS CON DATAFRAMES — MOTOR PARALLEL.FOR (una iteración/consulta)");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine();
        Console.WriteLine("  Parallel.For reparte las 30 iteraciones entre el ThreadPool.");
        Console.WriteLine("  Una sola pasada por el DataFrame, compartido y solo lectura.");
        Console.WriteLine();

        await Task.Run(() =>
            Parallel.For(0, consultas.Length, i => consultas[i](df)));

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

        var df = _df ?? CrearDataFrame(accidentes);
        var consultas = ObtenerConsultas();
        var consultasPorTarea = consultas.Length / numeroTareas;

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine($"  CONSULTAS CON DATAFRAMES — PARALLEL.FOR POR LOTES: {numeroTareas} paquetes × {consultasPorTarea} consultas");
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
                consultas[i](df);
        }));

        Console.WriteLine();
        Console.WriteLine($"  ✅ {numeroTareas} paquetes terminaron ({consultas.Length} consultas en bloques de {consultasPorTarea}).");
        Console.WriteLine();
    }

    /// <summary>
    /// Las 30 consultas en orden, como acciones reutilizables.
    /// Fuente única para los modos paralelo y por lotes.
    /// </summary>
    private static Action<DataFrame>[] ObtenerConsultas() =>
    [
        // ── Consultas base (1-22) ──
        DFConsulta1,
        DFConsulta2,
        DFConsulta3,
        DFConsulta4,
        DFConsulta5,
        DFConsulta6,
        DFConsulta7,
        DFConsulta8,
        DFConsulta9,
        DFConsulta10,
        DFConsulta11,
        DFConsulta12,
        DFConsulta13,
        DFConsulta14,
        DFConsulta15,
        DFConsulta16,
        DFConsulta17,
        DFConsulta18,
        DFConsulta19,
        DFConsulta20,
        DFConsulta21,
        DFConsulta22,
        // ── Consultas temporales (23-30) ──
        DFConsulta23,
        DFConsulta24,
        DFConsulta25,
        DFConsulta26,
        DFConsulta27,
        DFConsulta28,
        DFConsulta29,
        DFConsulta30
    ];

    // ══════════════════════════════════════════════════════════════
    // MÉTODOS AUXILIARES
    // ══════════════════════════════════════════════════════════════

    private void EjecutarConsultasBase(DataFrame df)
    {
        DFConsulta1(df); DFConsulta2(df); DFConsulta3(df);
        DFConsulta4(df); DFConsulta5(df); DFConsulta6(df);
        DFConsulta7(df); DFConsulta8(df); DFConsulta9(df);
        DFConsulta10(df); DFConsulta11(df); DFConsulta12(df);
        DFConsulta13(df); DFConsulta14(df); DFConsulta15(df);
        DFConsulta16(df); DFConsulta17(df); DFConsulta18(df);
        DFConsulta19(df); DFConsulta20(df); DFConsulta21(df);
        DFConsulta22(df);
    }

    private void EjecutarConsultasTemporales(DataFrame df)
    {
        // Microsoft.Data.Analysis no tiene GroupBy compuesto, así que las
        // consultas temporales hacen un recorrido único de la columna Fecha
        // y acumulan los conteos en diccionarios (una sola pasada por consulta).
        DFConsulta23(df); DFConsulta24(df); DFConsulta25(df);
        DFConsulta26(df); DFConsulta27(df); DFConsulta28(df);
        DFConsulta29(df); DFConsulta30(df);
    }

    // ══════════════════════════════════════════════════════════════
    // CONSULTAS INDIVIDUALES (1-22)
    // ══════════════════════════════════════════════════════════════

    private static void DFConsulta1(DataFrame df)
        => Console.WriteLine($"1. Total de accidentes: {df.Rows.Count}");

    private static void DFConsulta2(DataFrame df)
    {
        var porDistrito = AgruparPorColumna(df, "Distrito");
        Console.WriteLine("\n2. Accidentes por distrito (top 5):");
        MostrarTop(porDistrito, 5);
    }

    private static void DFConsulta3(DataFrame df)
    {
        var porTipo = AgruparPorColumna(df, "TipoAccidente");
        Console.WriteLine("\n3. Accidentes por tipo:");
        MostrarTodos(porTipo);
    }

    private static void DFConsulta4(DataFrame df)
    {
        var porMeteo = AgruparPorColumna(df, "EstadoMeteorologico");
        Console.WriteLine("\n4. Accidentes por estado meteorológico:");
        MostrarTodos(porMeteo);
    }

    private static void DFConsulta5(DataFrame df)
    {
        var porSexo = AgruparPorColumna(df, "Sexo");
        Console.WriteLine("\n5. Accidentes por sexo:");
        MostrarTodos(porSexo);
    }

    private static void DFConsulta6(DataFrame df)
    {
        var porEdad = AgruparPorColumna(df, "RangoEdad");
        Console.WriteLine("\n6. Accidentes por rango de edad:");
        MostrarTodos(porEdad);
    }

    private static void DFConsulta7(DataFrame df)
    {
        var alcohol = ContarBool(df, "PositivoAlcohol");
        Console.WriteLine($"\n7. Positivos en alcohol: {alcohol}");
    }

    private static void DFConsulta8(DataFrame df)
    {
        var drogas = ContarBool(df, "PositivoDroga");
        Console.WriteLine($"8. Positivos en drogas: {drogas}");
    }

    private static void DFConsulta9(DataFrame df)
    {
        var porDiaSemana = AgruparPorFecha(df, "Fecha", f => f.DayOfWeek.ToString());
        Console.WriteLine("\n9. Accidentes por día de la semana:");
        MostrarTodos(porDiaSemana);
    }

    private static void DFConsulta10(DataFrame df)
    {
        var porMes = AgruparPorFecha(df, "Fecha", f => f.Month.ToString());
        Console.WriteLine("\n10. Accidentes por mes:");
        MostrarTodos(porMes);
    }

    private static void DFConsulta11(DataFrame df)
    {
        var porHora = AgruparPorColumna(df, "Hora");
        var horaPico = porHora.OrderByDescending(kv => kv.Value).First();
        Console.WriteLine($"\n11. Hora con más accidentes: {horaPico.Key} ({horaPico.Value} accidentes)");
    }

    private static void DFConsulta12(DataFrame df)
    {
        var porLesion = AgruparPorColumna(df, "Lesividad");
        Console.WriteLine("\n12. Lesiones más frecuentes:");
        MostrarTop(porLesion, 5);
    }

    private static void DFConsulta13(DataFrame df)
    {
        var porVehiculo = AgruparPorColumna(df, "TipoVehiculo");
        Console.WriteLine("\n13. Vehículos más implicados:");
        MostrarTodos(porVehiculo);
    }

    private static void DFConsulta14(DataFrame df)
    {
        var peatones = ContarPorValor(df, "TipoPersona", "Peaton");
        Console.WriteLine($"\n14. Accidentes con peatones: {peatones}");
    }

    private static void DFConsulta15(DataFrame df)
    {
        var hombres = ContarPorValor(df, "Sexo", "Hombre");
        var mujeres = ContarPorValor(df, "Sexo", "Mujer");
        Console.WriteLine($"\n15. Proporción H/M: {hombres}/{mujeres} ({(mujeres > 0 ? (double)hombres / mujeres : 0):F2})");
    }

    private static void DFConsulta16(DataFrame df)
    {
        var dfPeatones = FiltrarPorValor(df, "TipoPersona", "Peaton");
        var distritoPeatones = AgruparPorColumna(dfPeatones, "Distrito");
        Console.WriteLine("\n16. Distritos con más peatones:");
        MostrarTop(distritoPeatones, 3);
    }

    private static void DFConsulta17(DataFrame df)
    {
        var finsDeSemana = ContarFinesDeSemana(df);
        Console.WriteLine($"\n17. Fin de semana: {finsDeSemana} | Entre semana: {df.Rows.Count - finsDeSemana}");
    }

    private static void DFConsulta18(DataFrame df)
    {
        var fechaCol = df.Columns["Fecha"] as DateTimeDataFrameColumn;
        var diasDistintos = fechaCol?.Distinct().Count() ?? 0;
        var mediaPorDia = diasDistintos > 0 ? (double)df.Rows.Count / diasDistintos : 0;
        Console.WriteLine($"\n18. Media de accidentes por día: {mediaPorDia:F2}");
    }

    private static void DFConsulta19(DataFrame df)
    {
        var multiFactor = ContarAmbos(df, "PositivoAlcohol", "PositivoDroga");
        Console.WriteLine($"\n19. Accidentes con alcohol + droga: {multiFactor}");
    }

    private static void DFConsulta20(DataFrame df)
    {
        var dfPeatones = FiltrarPorValor(df, "TipoPersona", "Peaton");
        var edadVulnerable = AgruparPorColumna(dfPeatones, "RangoEdad");
        Console.WriteLine("\n20. Rangos de edad más vulnerables (peatones):");
        MostrarTop(edadVulnerable, 3);
    }

    private static void DFConsulta21(DataFrame df)
    {
        var dfAlcohol = FiltrarPorValor(df, "PositivoAlcohol", "True");
        var distritoAlcohol = AgruparPorColumna(dfAlcohol, "Distrito");
        Console.WriteLine("\n21. Distritos con más positivos en alcohol:");
        MostrarTop(distritoAlcohol, 3);
    }

    private static void DFConsulta22(DataFrame df)
    {
        var codCol = df.Columns["CodDistrito"] as Int32DataFrameColumn;
        var distritoCol = df.Columns["Distrito"] as StringDataFrameColumn;
        if (codCol is null) return;

        var conteos = new Dictionary<int, int>();
        var nombres = new Dictionary<int, string>();
        for (int i = 0; i < codCol.Length; i++)
        {
            var codigo = codCol[i] ?? 0;
            conteos[codigo] = conteos.GetValueOrDefault(codigo) + 1;
            if (distritoCol is not null && distritoCol[i] is { } nombre)
                nombres[codigo] = nombre;
        }

        Console.WriteLine("\n22. Accidentes por código de distrito (DataFrame):");
        foreach (var kv in conteos.OrderBy(x => x.Key))
            Console.WriteLine($"   [{kv.Key:D2}] {nombres.GetValueOrDefault(kv.Key, "(sin nombre)")}: {kv.Value}");
    }

    // ══════════════════════════════════════════════════════════════
    // CONSULTAS TEMPORALES (23-30) — una sola pasada por la columna Fecha
    // ══════════════════════════════════════════════════════════════

    private static void DFConsulta23(DataFrame df)
    {
        var fechaCol = df.Columns["Fecha"] as DateTimeDataFrameColumn;
        if (fechaCol is null) return;

        var porAnio = new Dictionary<int, int>();
        for (int i = 0; i < fechaCol.Length; i++)
        {
            if (fechaCol[i] is { } fecha)
                porAnio[fecha.Year] = porAnio.GetValueOrDefault(fecha.Year) + 1;
        }

        Console.WriteLine("\n23. Accidentes por año (DataFrame):");
        foreach (var kv in porAnio.OrderBy(x => x.Key))
            Console.WriteLine($"   {kv.Key}: {kv.Value:N0}");
    }

    private static void DFConsulta24(DataFrame df)
    {
        var fechaCol = df.Columns["Fecha"] as DateTimeDataFrameColumn;
        if (fechaCol is null) return;

        var porMesAnio = new Dictionary<int, int>();
        for (int i = 0; i < fechaCol.Length; i++)
        {
            if (fechaCol[i] is { } fecha)
            {
                var clave = fecha.Year * 100 + fecha.Month;
                porMesAnio[clave] = porMesAnio.GetValueOrDefault(clave) + 1;
            }
        }

        Console.WriteLine("\n24. Evolución mensual por año (DataFrame) — primeros 12:");
        foreach (var kv in porMesAnio.OrderBy(x => x.Key).Take(12))
            Console.WriteLine($"   {kv.Key / 100}/{kv.Key % 100:D2}: {kv.Value:N0}");
    }

    private static void DFConsulta25(DataFrame df)
    {
        var fechaCol = df.Columns["Fecha"] as DateTimeDataFrameColumn;
        var distritoCol = df.Columns["Distrito"] as StringDataFrameColumn;
        if (fechaCol is null || distritoCol is null) return;

        var porAnio = new Dictionary<int, Dictionary<string, int>>();
        for (int i = 0; i < fechaCol.Length; i++)
        {
            if (fechaCol[i] is not { } fecha || distritoCol[i] is not { } distrito)
                continue;

            if (!porAnio.TryGetValue(fecha.Year, out var conteo))
                porAnio[fecha.Year] = conteo = new Dictionary<string, int>();

            conteo[distrito] = conteo.GetValueOrDefault(distrito) + 1;
        }

        Console.WriteLine("\n25. Distrito más peligroso por año (DataFrame):");
        foreach (var kv in porAnio.OrderBy(x => x.Key))
        {
            var top = kv.Value.OrderByDescending(d => d.Value).First();
            Console.WriteLine($"   {kv.Key}: {top.Key} ({top.Value} accidentes)");
        }
    }

    private static void DFConsulta26(DataFrame df)
    {
        var fechaCol = df.Columns["Fecha"] as DateTimeDataFrameColumn;
        var alcoholCol = df.Columns["PositivoAlcohol"] as BooleanDataFrameColumn;
        if (fechaCol is null) return;

        var porAnio = new Dictionary<int, (int Total, int ConAlcohol)>();
        for (int i = 0; i < fechaCol.Length; i++)
        {
            if (fechaCol[i] is not { } fecha) continue;

            var v = porAnio.GetValueOrDefault(fecha.Year);
            var conAlcohol = alcoholCol is not null && alcoholCol[i] == true ? 1 : 0;
            porAnio[fecha.Year] = (v.Total + 1, v.ConAlcohol + conAlcohol);
        }

        Console.WriteLine("\n26. Tendencia de alcohol por año (DataFrame):");
        foreach (var kv in porAnio.OrderBy(x => x.Key))
        {
            var porcentaje = kv.Value.Total > 0
                ? (double)kv.Value.ConAlcohol / kv.Value.Total * 100
                : 0;
            Console.WriteLine($"   {kv.Key}: {kv.Value.ConAlcohol} positivos de {kv.Value.Total} ({porcentaje:F1}%)");
        }
    }

    private static void DFConsulta27(DataFrame df)
    {
        var fechaCol = df.Columns["Fecha"] as DateTimeDataFrameColumn;
        if (fechaCol is null) return;

        var porAnio = new Dictionary<int, (int EntreSemana, int FinDeSemana)>();
        for (int i = 0; i < fechaCol.Length; i++)
        {
            if (fechaCol[i] is not { } fecha) continue;

            var v = porAnio.GetValueOrDefault(fecha.Year);
            porAnio[fecha.Year] = fecha.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday
                ? (v.EntreSemana, v.FinDeSemana + 1)
                : (v.EntreSemana + 1, v.FinDeSemana);
        }

        Console.WriteLine("\n27. Fin de semana vs entre semana por año (DataFrame):");
        foreach (var kv in porAnio.OrderBy(x => x.Key))
            Console.WriteLine($"   {kv.Key}: Entre semana {kv.Value.EntreSemana:N0} | Fin de semana {kv.Value.FinDeSemana:N0}");
    }

    private static void DFConsulta28(DataFrame df)
    {
        var fechaCol = df.Columns["Fecha"] as DateTimeDataFrameColumn;
        var horaCol = df.Columns["Hora"] as StringDataFrameColumn;
        if (fechaCol is null || horaCol is null) return;

        var horasPorAnio = new Dictionary<int, Dictionary<int, int>>();
        for (int i = 0; i < fechaCol.Length; i++)
        {
            if (fechaCol[i] is not { } fecha || horaCol[i] is not { } hora || hora.Length < 2)
                continue;

            if (!horasPorAnio.TryGetValue(fecha.Year, out var conteo))
                horasPorAnio[fecha.Year] = conteo = new Dictionary<int, int>();

            if (int.TryParse(hora[..2], out var horaDia))
                conteo[horaDia] = conteo.GetValueOrDefault(horaDia) + 1;
        }

        Console.WriteLine("\n28. Hora pico por año (DataFrame):");
        foreach (var kv in horasPorAnio.OrderBy(x => x.Key))
        {
            var pico = kv.Value.OrderByDescending(h => h.Value).First();
            Console.WriteLine($"   {kv.Key}: {pico.Key:D2}:00 ({pico.Value} accidentes)");
        }
    }

    private static void DFConsulta29(DataFrame df)
    {
        var fechaCol = df.Columns["Fecha"] as DateTimeDataFrameColumn;
        var lesividadCol = df.Columns["Lesividad"] as StringDataFrameColumn;
        if (fechaCol is null || lesividadCol is null) return;

        var lesionesPorAnio = new Dictionary<int, Dictionary<string, int>>();
        for (int i = 0; i < fechaCol.Length; i++)
        {
            if (fechaCol[i] is not { } fecha || lesividadCol[i] is not { } lesividad)
                continue;

            if (!lesionesPorAnio.TryGetValue(fecha.Year, out var conteo))
                lesionesPorAnio[fecha.Year] = conteo = new Dictionary<string, int>();

            conteo[lesividad] = conteo.GetValueOrDefault(lesividad) + 1;
        }

        Console.WriteLine("\n29. Lesión más frecuente por año (DataFrame):");
        foreach (var kv in lesionesPorAnio.OrderBy(x => x.Key))
        {
            var top = kv.Value.OrderByDescending(l => l.Value).First();
            Console.WriteLine($"   {kv.Key}: {top.Key} ({top.Value} casos)");
        }
    }

    private static void DFConsulta30(DataFrame df)
    {
        var fechaCol = df.Columns["Fecha"] as DateTimeDataFrameColumn;
        var tipoPersonaCol = df.Columns["TipoPersona"] as StringDataFrameColumn;
        if (fechaCol is null) return;

        var porAnio = new Dictionary<int, (int Total, int Peatones)>();
        for (int i = 0; i < fechaCol.Length; i++)
        {
            if (fechaCol[i] is not { } fecha) continue;

            var v = porAnio.GetValueOrDefault(fecha.Year);
            var esPeaton = tipoPersonaCol is not null && tipoPersonaCol[i] == "Peaton" ? 1 : 0;
            porAnio[fecha.Year] = (v.Total + 1, v.Peatones + esPeaton);
        }

        Console.WriteLine("\n30. Evolución de peatones por año (DataFrame):");
        foreach (var kv in porAnio.OrderBy(x => x.Key))
        {
            var porcentaje = kv.Value.Total > 0
                ? (double)kv.Value.Peatones / kv.Value.Total * 100
                : 0;
            Console.WriteLine($"   {kv.Key}: {kv.Value.Peatones:N0} peatones de {kv.Value.Total:N0} ({porcentaje:F1}%)");
        }
    }

    // ══════════════════════════════════════════════════════════════
    // MÉTODOS DE AYUDA
    // ══════════════════════════════════════════════════════════════

    private static DataFrame CrearDataFrame(IReadOnlyList<Accidente> accidentes)
    {
        var numExpediente = new StringDataFrameColumn("NumExpediente", accidentes.Select(a => a.NumExpediente));
        var fecha = new DateTimeDataFrameColumn("Fecha", accidentes.Select(a => a.Fecha));
        var hora = new StringDataFrameColumn("Hora", accidentes.Select(a => a.Hora.ToString(@"hh\:mm")));
        var localizacion = new StringDataFrameColumn("Localizacion", accidentes.Select(a => a.Localizacion));
        var numero = new StringDataFrameColumn("Numero", accidentes.Select(a => a.Numero));
        var codDistrito = new Int32DataFrameColumn("CodDistrito", accidentes.Select(a => a.CodDistrito));
        var distrito = new StringDataFrameColumn("Distrito", accidentes.Select(a => a.Distrito));
        var tipoAccidente = new StringDataFrameColumn("TipoAccidente", accidentes.Select(a => a.TipoAccidente));
        var estadoMeteo = new StringDataFrameColumn("EstadoMeteorologico", accidentes.Select(a => a.EstadoMeteorologico));
        var tipoVehiculo = new StringDataFrameColumn("TipoVehiculo", accidentes.Select(a => a.TipoVehiculo));
        var tipoPersona = new StringDataFrameColumn("TipoPersona", accidentes.Select(a => a.TipoPersona.ToString()));
        var rangoEdad = new StringDataFrameColumn("RangoEdad", accidentes.Select(a => a.RangoEdad));
        var sexo = new StringDataFrameColumn("Sexo", accidentes.Select(a => a.Sexo.ToString()));
        var codLesividad = new StringDataFrameColumn("CodLesividad", accidentes.Select(a => a.CodLesividad));
        var lesividad = new StringDataFrameColumn("Lesividad", accidentes.Select(a => a.Lesividad));
        var positivoAlcohol = new BooleanDataFrameColumn("PositivoAlcohol", accidentes.Select(a => a.PositivoAlcohol));
        var positivoDroga = new BooleanDataFrameColumn("PositivoDroga", accidentes.Select(a => a.PositivoDroga));

        return new DataFrame(
            numExpediente, fecha, hora, localizacion, numero, codDistrito,
            distrito, tipoAccidente, estadoMeteo, tipoVehiculo, tipoPersona,
            rangoEdad, sexo, codLesividad, lesividad, positivoAlcohol, positivoDroga
        );
    }

    private static Dictionary<string, int> AgruparPorColumna(DataFrame df, string nombreColumna)
    {
        var columna = df.Columns[nombreColumna] as StringDataFrameColumn
            ?? throw new ArgumentException($"Columna '{nombreColumna}' no encontrada");

        var resultado = new Dictionary<string, int>();
        for (int i = 0; i < columna.Length; i++)
        {
            var valor = columna[i]?.ToString() ?? "(vacío)";
            resultado[valor] = resultado.GetValueOrDefault(valor) + 1;
        }
        return resultado;
    }

    private static Dictionary<string, int> AgruparPorFecha(DataFrame df, string nombreColumna, Func<DateTime, string> keySelector)
    {
        var columna = df.Columns[nombreColumna] as DateTimeDataFrameColumn
            ?? throw new ArgumentException($"Columna '{nombreColumna}' no encontrada");

        var resultado = new Dictionary<string, int>();
        for (int i = 0; i < columna.Length; i++)
        {
            var fecha = columna[i];
            if (fecha is null) continue;
            var valor = keySelector(fecha.Value);
            resultado[valor] = resultado.GetValueOrDefault(valor) + 1;
        }
        return resultado;
    }

    private static int ContarBool(DataFrame df, string nombreColumna)
    {
        var columna = df.Columns[nombreColumna] as BooleanDataFrameColumn;
        if (columna is null) return 0;
        int count = 0;
        for (int i = 0; i < columna.Length; i++)
            if (columna[i] == true) count++;
        return count;
    }

    private static int ContarAmbos(DataFrame df, string col1, string col2)
    {
        var c1 = df.Columns[col1] as BooleanDataFrameColumn;
        var c2 = df.Columns[col2] as BooleanDataFrameColumn;
        if (c1 is null || c2 is null) return 0;
        int count = 0;
        for (int i = 0; i < c1.Length; i++)
            if (c1[i] == true && c2[i] == true) count++;
        return count;
    }

    private static int ContarPorValor(DataFrame df, string nombreColumna, string valor)
    {
        var columna = df.Columns[nombreColumna] as StringDataFrameColumn;
        if (columna is null) return 0;
        int count = 0;
        for (int i = 0; i < columna.Length; i++)
            if (columna[i]?.ToString() == valor) count++;
        return count;
    }

    private static DataFrame FiltrarPorValor(DataFrame df, string nombreColumna, string valor)
    {
        var columna = df.Columns[nombreColumna] as StringDataFrameColumn;
        if (columna is null) return df;
        var mask = new PrimitiveDataFrameColumn<bool>("mask", columna.Length);
        for (int i = 0; i < columna.Length; i++)
            mask[i] = columna[i]?.ToString() == valor;
        return df.Filter(mask);
    }

    private static int ContarFinesDeSemana(DataFrame df)
    {
        var columna = df.Columns["Fecha"] as DateTimeDataFrameColumn;
        if (columna is null) return 0;
        int count = 0;
        for (int i = 0; i < columna.Length; i++)
            if (columna[i]?.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) count++;
        return count;
    }

    private static void MostrarTodos(Dictionary<string, int> datos)
    {
        foreach (var kv in datos.OrderByDescending(x => x.Value))
            Console.WriteLine($"   {kv.Key}: {kv.Value}");
    }

    private static void MostrarTop(Dictionary<string, int> datos, int top)
    {
        foreach (var kv in datos.OrderByDescending(x => x.Value).Take(top))
            Console.WriteLine($"   {kv.Key}: {kv.Value}");
    }
}
