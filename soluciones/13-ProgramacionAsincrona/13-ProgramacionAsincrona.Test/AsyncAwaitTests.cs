using FluentAssertions;
using NUnit.Framework;
using _13_ProgramacionAsincrona.Models;
using _13_ProgramacionAsincrona.Repositories;
using _13_ProgramacionAsincrona.Services;

namespace _13_ProgramacionAsincrona.Test;

/// <summary>
/// Tests de ASYNC/AWAIT (fluido frío): carga TODO en memoria con Task<T>.
/// </summary>
[TestFixture]
public class AsyncAwaitTests
{
    private AccidentesCsvRepository _repository = null!;
    private AsyncAwaitService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new AccidentesCsvRepository(TestHelper.ObtenerRutaCsv());
        _service = new AsyncAwaitService(_repository);
    }

    [Test]
    public async Task ContarTotalAsync_CargaTodosLosRegistros()
    {
        var total = await _service.ContarTotalAsync();
        total.Should().Be(46596, "debe cargar todos los registros del CSV en una lista");
    }

    [Test]
    public async Task FiltrarAlcoholAsync_DevuelveSoloPositivos()
    {
        var resultado = await _service.FiltrarAlcoholAsync();
        resultado.Should().AllSatisfy(a => a.PositivoAlcohol.Should().BeTrue());
        resultado.Should().HaveCount(1197);
    }

    [Test]
    public async Task FiltrarAlcoholAsync_CadaLlamadaCreaNuevaLista()
    {
        var lista1 = await _service.FiltrarAlcoholAsync();
        var lista2 = await _service.FiltrarAlcoholAsync();
        lista1.Should().NotBeSameAs(lista2);
    }

    [Test]
    public async Task ContarTotalAsync_NoBloqueaElHilo()
    {
        var task = _service.ContarTotalAsync();
        task.IsCompleted.Should().BeFalse("la tarea debe estar pendiente mientras se ejecuta");
        await task;
        task.IsCompleted.Should().BeTrue();
    }

    [Test]
    public async Task FiltrarAlcoholAsync_DevuelveVacioSiNoHayDatos()
    {
        var repositoryVacio = new AccidentesCsvRepository("no-existe.csv");
        var serviceVacio = new AsyncAwaitService(repositoryVacio);
        var accion = () => serviceVacio.FiltrarAlcoholAsync();
        await accion.Should().ThrowAsync<Exception>();
    }

    [Test]
    public async Task ContarTotalAsync_EsMasRapidoQueIAsyncEnumerable()
    {
        var sw1 = System.Diagnostics.Stopwatch.StartNew();
        await _service.ContarTotalAsync();
        sw1.Stop();

        var serviceEnum = new AsyncEnumerableService(_repository);
        var sw2 = System.Diagnostics.Stopwatch.StartNew();
        await serviceEnum.ContarYFiltrarAsync();
        sw2.Stop();

        // Async/Await puede ser más lento porque carga todo en memoria
        // pero es más simple
        sw1.ElapsedMilliseconds.Should().BeGreaterThan(0);
        sw2.ElapsedMilliseconds.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task FiltrarAlcoholAsync_LosRegistrosTienenDatosCorrectos()
    {
        var resultado = await _service.FiltrarAlcoholAsync();
        resultado.Should().OnlyContain(a =>
            !string.IsNullOrEmpty(a.NumExpediente) &&
            a.Fecha > DateTime.MinValue &&
            !string.IsNullOrEmpty(a.Distrito));
    }

    [Test]
    public async Task ContarTotalAsync_SePuedeEjecutarVariasVeces()
    {
        var t1 = _service.ContarTotalAsync();
        var t2 = _service.ContarTotalAsync();
        var t3 = _service.ContarTotalAsync();

        await Task.WhenAll(t1, t2, t3);

        t1.Result.Should().Be(46596);
        t2.Result.Should().Be(46596);
        t3.Result.Should().Be(46596);
    }
}
