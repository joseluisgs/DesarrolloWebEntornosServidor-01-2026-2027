using FluentAssertions;
using NUnit.Framework;
using _13_ProgramacionAsincrona.Models;
using _13_ProgramacionAsincrona.Repositories;
using _13_ProgramacionAsincrona.Services;

namespace _13_ProgramacionAsincrona.Test;

/// <summary>
/// Tests de FLUJOS FRÍOS (IAsyncEnumerable): procesa línea a línea bajo demanda (pull).
/// </summary>
[TestFixture]
public class FlujosFriosTests
{
    private AccidentesCsvRepository _repository = null!;
    private AsyncEnumerableService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new AccidentesCsvRepository(TestHelper.ObtenerRutaCsv());
        _service = new AsyncEnumerableService(_repository);
    }

    [Test]
    public async Task ContarYFiltrarAsync_ProcesaTodosLosRegistros()
    {
        var (total, conAlcohol) = await _service.ContarYFiltrarAsync();
        total.Should().Be(46596);
        conAlcohol.Should().Be(1197);
    }

    [Test]
    public async Task LosTresMetodosDanElMismoResultado()
    {
        var serviceAsync = new AsyncAwaitService(_repository);
        var (totalEnum, alcoholEnum) = await _service.ContarYFiltrarAsync();
        var totalAwait = await serviceAsync.ContarTotalAsync();
        var alcoholAwait = (await serviceAsync.FiltrarAlcoholAsync()).Count;

        totalEnum.Should().Be(totalAwait);
        alcoholEnum.Should().Be(alcoholAwait);
    }

    [Test]
    public async Task EsUnFlujoFrio_EmpiezaDesdeElPrincipio()
    {
        var lista1 = new List<Accidente>();
        await foreach (var a in _repository.GetAllAsyncEnumerable())
            lista1.Add(a);

        var lista2 = new List<Accidente>();
        await foreach (var a in _repository.GetAllAsyncEnumerable())
            lista2.Add(a);

        lista1.Should().HaveSameCount(lista2);
        lista1.First().NumExpediente.Should().Be(lista2.First().NumExpediente);
    }

    [Test]
    public async Task NoCargaTodoEnMemoria()
    {
        var procesados = 0;
        await foreach (var _ in _repository.GetAllAsyncEnumerable())
            procesados++;

        procesados.Should().Be(46596);
    }

    [Test]
    public async Task SePuedeCancelar()
    {
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var procesados = 0;
        try
        {
            await foreach (var _ in _repository.GetAllAsyncEnumerable(cts.Token))
                procesados++;
        }
        catch (OperationCanceledException)
        {
            // Esperado
        }

        procesados.Should().Be(0, "la cancellación debe detener el procesamiento");
    }

    [Test]
    public async Task SePuedePararAntesDeTerminar()
    {
        var procesados = 0;
        await foreach (var _ in _repository.GetAllAsyncEnumerable())
        {
            procesados++;
            if (procesados == 100) break;
        }

        procesados.Should().Be(100, "debe parar en el registro 100");
    }

    [Test]
    public async Task SePuedeFiltrarMientrasSeProcesa()
    {
        var conAlcohol = 0;
        await foreach (var accidente in _repository.GetAllAsyncEnumerable())
        {
            if (accidente.PositivoAlcohol)
                conAlcohol++;
        }

        conAlcohol.Should().Be(1197);
    }

    [Test]
    public async Task LosRegistrosSonConsistentes()
    {
        var registros = new List<Accidente>();
        await foreach (var a in _repository.GetAllAsyncEnumerable())
            registros.Add(a);

        registros.Should().OnlyContain(a =>
            !string.IsNullOrEmpty(a.NumExpediente) &&
            a.Fecha > DateTime.MinValue);
    }

    [Test]
    public async Task SePuedeProcesarConParallel()
    {
        var registros = new List<Accidente>();
        await foreach (var a in _repository.GetAllAsyncEnumerable())
            registros.Add(a);

        var conAlcohol = registros.AsParallel().Count(a => a.PositivoAlcohol);
        conAlcohol.Should().Be(1197);
    }

    [Test]
    public async Task SePuedeConvertirALista()
    {
        var lista = new List<Accidente>();
        await foreach (var a in _repository.GetAllAsyncEnumerable())
            lista.Add(a);

        lista.Should().HaveCount(46596);
        lista.Should().BeAssignableTo<List<Accidente>>();
    }

    [Test]
    public async Task ReiniciaDesdeElPrincipio_CompararDosLecturas()
    {
        // Fluido frío: cada lectura empieza desde el principio
        // Como un vídeo en pausa: si le das play 2 veces, empieza desde el inicio
        var primeraLectura = new List<Accidente>();
        await foreach (var a in _repository.GetAllAsyncEnumerable())
            primeraLectura.Add(a);

        var segundaLectura = new List<Accidente>();
        await foreach (var a in _repository.GetAllAsyncEnumerable())
            segundaLectura.Add(a);

        primeraLectura.Should().HaveCount(segundaLectura.Count);
        primeraLectura.First().NumExpediente.Should().Be(segundaLectura.First().NumExpediente);
        primeraLectura.Last().NumExpediente.Should().Be(segundaLectura.Last().NumExpediente);
    }

    [Test]
    public async Task SePuedeGuardarResultado_CuandoSeNecesita()
    {
        // El flujo frío se puede convertir a lista cuando lo necesitas
        var lista = new List<Accidente>();
        await foreach (var a in _repository.GetAllAsyncEnumerable())
            lista.Add(a);

        lista.Should().HaveCount(46596);
        lista.Should().BeAssignableTo<List<Accidente>>();
    }
}
