using FluentAssertions;
using System.Reactive.Linq;
using NUnit.Framework;
using _13_ProgramacionAsincrona.Models;
using _13_ProgramacionAsincrona.Repositories;
using _13_ProgramacionAsincrona.Services;

namespace _13_ProgramacionAsincrona.Test;

/// <summary>
/// Tests de FLUJOS CALIENTES (Rx.NET): datos que fluyen, múltiples suscriptores.
/// </summary>
[TestFixture]
public class FlujosCalientesTests
{
    private AccidentesCsvRepository _repository = null!;
    private ReactiveService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new AccidentesCsvRepository(TestHelper.ObtenerRutaCsv());
        _service = new ReactiveService(_repository);
    }

    [Test]
    public async Task ProcesarObservableAsync_DevuelveMismosTotales()
    {
        var (total, conAlcohol) = await _service.ProcesarObservableAsync();
        total.Should().Be(46596);
        conAlcohol.Should().Be(1197);
    }

    [Test]
    public async Task MultiplesSuscriptoresRecibenTodosLosDatos()
    {
        var observable = _repository.CrearObservable();
        int count1 = 0, count2 = 0, count3 = 0;

        observable.Subscribe(_ => count1++);
        observable.Subscribe(_ => count2++);
        observable.Subscribe(_ => count3++);

        var tcs = new TaskCompletionSource();
        observable.Subscribe(_ => { }, () => tcs.TrySetResult());
        await tcs.Task;

        count1.Should().Be(46596);
        count2.Should().Be(46596);
        count3.Should().Be(46596);
    }

    [Test]
    public async Task FiltraEnTiempoReal()
    {
        var observable = _repository.CrearObservable();
        var alcoholCount = 0;

        observable
            .Where(a => a.PositivoAlcohol)
            .Subscribe(_ => alcoholCount++);

        var tcs = new TaskCompletionSource();
        observable.Subscribe(_ => { }, () => tcs.TrySetResult());
        await tcs.Task;

        alcoholCount.Should().Be(1197);
    }

    [Test]
    public async Task TakeSoloDaLosPrimerosN()
    {
        var observable = _repository.CrearObservable();
        var primeros = new List<Accidente>();

        observable
            .Take(5)
            .Subscribe(a => primeros.Add(a));

        var tcs = new TaskCompletionSource();
        observable.Subscribe(_ => { }, () => tcs.TrySetResult());
        await tcs.Task;

        primeros.Should().HaveCount(5);
    }

    [Test]
    public async Task BufferAgrupaEnLotes()
    {
        var observable = _repository.CrearObservable();
        var lotes = new List<IList<Accidente>>();

        observable
            .Buffer(1000)
            .Subscribe(lote => lotes.Add(lote));

        var tcs = new TaskCompletionSource();
        observable.Subscribe(_ => { }, () => tcs.TrySetResult());
        await tcs.Task;

        lotes.Should().NotBeEmpty();
        lotes.Sum(l => l.Count).Should().Be(46596);
    }

    [Test]
    public async Task LosTresMetodosDanElMismoResultado()
    {
        var serviceAsync = new AsyncAwaitService(_repository);
        var serviceEnum = new AsyncEnumerableService(_repository);

        var (totalRx, alcoholRx) = await _service.ProcesarObservableAsync();
        var totalAwait = await serviceAsync.ContarTotalAsync();
        var alcoholAwait = (await serviceAsync.FiltrarAlcoholAsync()).Count;
        var (totalEnum, alcoholEnum) = await serviceEnum.ContarYFiltrarAsync();

        totalRx.Should().Be(totalAwait).And.Be(totalEnum);
        alcoholRx.Should().Be(alcoholAwait).And.Be(alcoholEnum);
    }

    [Test]
    public async Task LosRegistrosTienenDatosValidos()
    {
        var observable = _repository.CrearObservable();
        var registros = new List<Accidente>();

        observable
            .Take(100)
            .Subscribe(a => registros.Add(a));

        var tcs = new TaskCompletionSource();
        observable.Subscribe(_ => { }, () => tcs.TrySetResult());
        await tcs.Task;

        registros.Should().OnlyContain(a =>
            !string.IsNullOrEmpty(a.NumExpediente) &&
            a.Fecha > DateTime.MinValue);
    }

    [Test]
    public async Task DistinctPorDistrito()
    {
        var observable = _repository.CrearObservable();
        var distritos = new HashSet<string>();

        observable
            .Select(a => a.Distrito)
            .Distinct()
            .Subscribe(d => distritos.Add(d));

        var tcs = new TaskCompletionSource();
        observable.Subscribe(_ => { }, () => tcs.TrySetResult());
        await tcs.Task;

        distritos.Should().NotBeEmpty();
        distritos.Count.Should().BeGreaterThan(1);
    }

    [Test]
    public async Task CountAsincrono()
    {
        var observable = _repository.CrearObservable();
        var count = 0;

        observable.Subscribe(_ => count++);

        var tcs = new TaskCompletionSource();
        observable.Subscribe(_ => { }, () => tcs.TrySetResult());
        await tcs.Task;

        count.Should().Be(46596);
    }

    [Test]
    public async Task PrimerosYUltimos()
    {
        var observable = _repository.CrearObservable();
        var primeros = new List<Accidente>();

        observable
            .Take(3)
            .Subscribe(a => primeros.Add(a));

        var tcs = new TaskCompletionSource();
        observable.Subscribe(_ => { }, () => tcs.TrySetResult());
        await tcs.Task;

        primeros.Should().HaveCount(3);
        primeros.Should().OnlyContain(a => !string.IsNullOrEmpty(a.NumExpediente));
    }

    [Test]
    public async Task whereYTakeCombinados()
    {
        var observable = _repository.CrearObservable();
        var alcoholPrimeros = new List<Accidente>();

        observable
            .Where(a => a.PositivoAlcohol)
            .Take(10)
            .Subscribe(a => alcoholPrimeros.Add(a));

        var tcs = new TaskCompletionSource();
        observable.Subscribe(_ => { }, () => tcs.TrySetResult());
        await tcs.Task;

        alcoholPrimeros.Should().HaveCount(10);
        alcoholPrimeros.Should().AllSatisfy(a => a.PositivoAlcohol.Should().BeTrue());
    }

    [Test]
    public async Task SuscriptorIndependienteNoAfectaAOtros()
    {
        var observable = _repository.CrearObservable();
        var count1 = 0;
        var count2 = 0;

        observable.Take(5).Subscribe(_ => count1++);
        observable.Subscribe(_ => count2++);

        var tcs = new TaskCompletionSource();
        observable.Subscribe(_ => { }, () => tcs.TrySetResult());
        await tcs.Task;

        count1.Should().Be(5);
        count2.Should().Be(46596);
    }

    [Test]
    public async Task SiSeConectaDespues_PierdeLosPrimeros()
    {
        // Fluido caliente: como un canal de TV en directo
        // Si te conectas después, pierdes lo que ya se emitió
        var observable = _repository.CrearObservable();

        var primeros = new List<Accidente>();
        var segundos = new List<Accidente>();

        // Suscriptor 1: recibe desde el principio
        observable.Take(5).Subscribe(a => primeros.Add(a));

        // Suscriptor 2: se suscribe después de los primeros 5
        observable.Skip(5).Take(5).Subscribe(a => segundos.Add(a));

        var tcs = new TaskCompletionSource();
        observable.Subscribe(_ => { }, () => tcs.TrySetResult());
        await tcs.Task;

        primeros.Should().HaveCount(5);
        segundos.Should().HaveCount(5);
        primeros[0].NumExpediente.Should().NotBe(segundos[0].NumExpediente);
    }

    [Test]
    public async Task DosSuscriptoresIndependientes_CadaUnoProcesaLoSuyo()
    {
        // Un suscriptor filtra alcohol, otro filtra drogas
        // No se afectan entre sí
        var observable = _repository.CrearObservable();
        var alcoholCount = 0;
        var drogasCount = 0;

        observable.Where(a => a.PositivoAlcohol).Subscribe(_ => alcoholCount++);
        observable.Where(a => a.PositivoDroga).Subscribe(_ => drogasCount++);

        var tcs = new TaskCompletionSource();
        observable.Subscribe(_ => { }, () => tcs.TrySetResult());
        await tcs.Task;

        alcoholCount.Should().Be(1197);
        drogasCount.Should().BeGreaterThan(0);
        alcoholCount.Should().NotBe(drogasCount);
    }

    [Test]
    public async Task Merge_DosObservablesEnUno()
    {
        // Combinar dos flujos en uno
        var observable1 = _repository.CrearObservable().Take(100);
        var observable2 = _repository.CrearObservable().Skip(100).Take(100);

        var combinado = observable1.Merge(observable2);
        var total = 0;

        combinado.Subscribe(_ => total++);

        var tcs = new TaskCompletionSource();
        combinado.Subscribe(_ => { }, () => tcs.TrySetResult());
        await tcs.Task;

        total.Should().Be(200);
    }
}
