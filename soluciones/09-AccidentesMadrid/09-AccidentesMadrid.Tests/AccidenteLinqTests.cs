using AccidentesMadrid.Models;
using FluentAssertions;
using NUnit.Framework;

namespace _09_AccidentesMadrid.Tests;

/// <summary>
/// Tests de consultas LINQ sobre datos de accidentes.
/// </summary>
[TestFixture]
public class AccidenteLinqTests
{
    private List<Accidente> _accidentes = null!;

    [SetUp]
    public void SetUp()
    {
        _accidentes =
        [
            new Accidente
            {
                NumExpediente = "001",
                Fecha = new DateTime(2025, 1, 15),
                Hora = new TimeSpan(8, 0, 0),
                Distrito = "Centro",
                TipoAccidente = "Colisión fronto-lateral",
                EstadoMeteorologico = "Despejado",
                TipoVehiculo = "Turismo",
                TipoPersona = TipoPersona.Conductor,
                RangoEdad = "25-34",
                Sexo = Sexo.Hombre,
                Lesividad = "Herido leve",
                PositivoAlcohol = false,
                PositivoDroga = false
            },
            new Accidente
            {
                NumExpediente = "002",
                Fecha = new DateTime(2025, 1, 20),
                Hora = new TimeSpan(14, 0, 0),
                Distrito = "Retiro",
                TipoAccidente = "Atropello",
                EstadoMeteorologico = "Lluvia",
                TipoVehiculo = "Motocicleta",
                TipoPersona = TipoPersona.Peaton,
                RangoEdad = "45-54",
                Sexo = Sexo.Mujer,
                Lesividad = "Herido grave",
                PositivoAlcohol = true,
                PositivoDroga = false
            },
            new Accidente
            {
                NumExpediente = "003",
                Fecha = new DateTime(2025, 2, 10),
                Hora = new TimeSpan(22, 0, 0),
                Distrito = "Centro",
                TipoAccidente = "Caída",
                EstadoMeteorologico = "Despejado",
                TipoVehiculo = "Turismo",
                TipoPersona = TipoPersona.Conductor,
                RangoEdad = "18-24",
                Sexo = Sexo.Hombre,
                Lesividad = "Herido leve",
                PositivoAlcohol = true,
                PositivoDroga = true
            },
            new Accidente
            {
                NumExpediente = "004",
                Fecha = new DateTime(2025, 2, 15),
                Hora = new TimeSpan(10, 0, 0),
                Distrito = "Salamanca",
                TipoAccidente = "Colisión fronto-lateral",
                EstadoMeteorologico = "Despejado",
                TipoVehiculo = "Turismo",
                TipoPersona = TipoPersona.Pasajero,
                RangoEdad = "35-44",
                Sexo = Sexo.Mujer,
                Lesividad = "Herido leve",
                PositivoAlcohol = false,
                PositivoDroga = false
            }
        ];
    }

    [TestFixture]
    public class CasosPositivos : AccidenteLinqTests
    {
        [Test]
        public void TotalAccidentes_DeberiaContarTodos()
        {
            // Arrange & Act
            var total = _accidentes.Count;

            // Assert
            total.Should().Be(4);
        }

        [Test]
        public void AccidentesPorDistrito_DeberiaAgruparCorrectamente()
        {
            // Arrange & Act
            var porDistrito = _accidentes
                .GroupBy(a => a.Distrito)
                .OrderByDescending(g => g.Count())
                .ToList();

            // Assert
            porDistrito.Should().HaveCount(3);
            porDistrito[0].Key.Should().Be("Centro");
            porDistrito[0].Count().Should().Be(2);
        }

        [Test]
        public void AccidentesPorTipo_DeberiaAgruparCorrectamente()
        {
            // Arrange & Act
            var porTipo = _accidentes
                .GroupBy(a => a.TipoAccidente)
                .ToList();

            // Assert
            porTipo.Should().HaveCount(3);
        }

        [Test]
        public void PositivosAlcohol_DeberiaFiltrarCorrectamente()
        {
            // Arrange & Act
            var conAlcohol = _accidentes.Count(a => a.PositivoAlcohol);

            // Assert
            conAlcohol.Should().Be(2);
        }

        [Test]
        public void AccidentesPorSexo_DeberiaAgruparCorrectamente()
        {
            // Arrange & Act
            var porSexo = _accidentes
                .GroupBy(a => a.Sexo)
                .ToList();

            // Assert
            porSexo.Should().HaveCount(2);
            var hombres = porSexo.First(g => g.Key == Sexo.Hombre);
            var mujeres = porSexo.First(g => g.Key == Sexo.Mujer);
            hombres.Count().Should().Be(2);
            mujeres.Count().Should().Be(2);
        }

        [Test]
        public void Peatones_DeberiaFiltrarCorrectamente()
        {
            // Arrange & Act
            var peatones = _accidentes.Count(a => a.TipoPersona == TipoPersona.Peaton);

            // Assert
            peatones.Should().Be(1);
        }

        [Test]
        public void AccidentesPorDiaSemana_DeberiaAgruparCorrectamente()
        {
            // Arrange & Act
            var porDia = _accidentes
                .GroupBy(a => a.Fecha.DayOfWeek)
                .ToList();

            // Assert
            porDia.Should().NotBeEmpty();
        }

        [Test]
        public void AccidentesPorMes_DeberiaAgruparCorrectamente()
        {
            // Arrange & Act
            var porMes = _accidentes
                .GroupBy(a => a.Fecha.Month)
                .OrderBy(g => g.Key)
                .ToList();

            // Assert
            porMes.Should().HaveCount(2);
            porMes[0].Key.Should().Be(1);
            porMes[0].Count().Should().Be(2);
        }

        [Test]
        public void HoraPico_DeberiaEncontrarHoraConMasAccidentes()
        {
            // Arrange & Act
            var horaPico = _accidentes
                .GroupBy(a => a.Hora.Hours)
                .OrderByDescending(g => g.Count())
                .First();

            // Assert
            horaPico.Key.Should().Be(8);
        }

        [Test]
        public void PLINQ_AsParallel_DeberiaFuncionar()
        {
            // Arrange & Act
            var porHora = _accidentes
                .AsParallel()
                .GroupBy(a => a.Hora.Hours)
                .OrderBy(g => g.Key)
                .Select(g => new { Hora = g.Key, Cantidad = g.Count() })
                .ToList();

            // Assert
            porHora.Should().NotBeEmpty();
        }
    }

    [TestFixture]
    public class CasosNegativos : AccidenteLinqTests
    {
        [Test]
        public void ListaVacia_TotalDeberiaSerCero()
        {
            // Arrange
            var vacia = new List<Accidente>();

            // Act
            var total = vacia.Count;

            // Assert
            total.Should().Be(0);
        }

        [Test]
        public void ListaVacia_AgrupacionesDeberianEstarVacias()
        {
            // Arrange
            var vacia = new List<Accidente>();

            // Act
            var porDistrito = vacia.GroupBy(a => a.Distrito).ToList();

            // Assert
            porDistrito.Should().BeEmpty();
        }
    }
}
