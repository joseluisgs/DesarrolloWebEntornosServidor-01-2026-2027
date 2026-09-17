using System.Globalization;
using System.Text.Json;
using FluentAssertions;
using CsvHelper;
using CsvHelper.Configuration;
using NUnit.Framework;
using _03_FicherosJsonCsv.Models;

namespace _03_FicherosJsonCsv.Tests;

/// <summary>
/// Tests de lectura y escritura de ficheros JSON y CSV.
/// </summary>
[TestFixture]
public class JsonCsvTests
{
    private string _dataDir = null!;
    private string _outputDir = null!;

    [SetUp]
    public void SetUp()
    {
        _dataDir = Path.Combine(AppContext.BaseDirectory, "data");
        _outputDir = Path.Combine(Path.GetTempPath(), $"test_output_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_outputDir);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_outputDir))
            Directory.Delete(_outputDir, true);
    }

    [TestFixture]
    public class CasosPositivos : JsonCsvTests
    {
        [Test]
        public void LeerJson_ProductosNoVacios_DeberiaLeerCorrectamente()
        {
            // Arrange
            var jsonPath = Path.Combine(_dataDir, "productos.json");

            // Act
            var jsonContent = File.ReadAllText(jsonPath);
            var productos = JsonSerializer.Deserialize<List<Producto>>(jsonContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            // Assert
            productos.Should().NotBeNullOrEmpty();
            productos!.Count.Should().BeGreaterThan(0);
            productos[0].Nombre.Should().NotBeNullOrEmpty();
        }

        [Test]
        public void LeerCsv_ProductosNoVacios_DeberiaLeerCorrectamente()
        {
            // Arrange
            var csvPath = Path.Combine(_dataDir, "productos.csv");
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null
            };

            // Act
            List<Producto> productos;
            using (var reader = new StreamReader(csvPath))
            using (var csv = new CsvReader(reader, config))
            {
                productos = csv.GetRecords<Producto>().ToList();
            }

            // Assert
            productos.Should().NotBeNullOrEmpty();
            productos.Count.Should().BeGreaterThan(0);
        }

        [Test]
        public async Task ExportarJson_DeberiaCrearFichero()
        {
            // Arrange
            var productos = new List<Producto>
            {
                new(1, "Test Product", 9.99m, "Test Category")
            };
            var opciones = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            // Act
            var json = JsonSerializer.Serialize(productos, opciones);
            var path = Path.Combine(_outputDir, "test_export.json");
            await File.WriteAllTextAsync(path, json);

            // Assert
            File.Exists(path).Should().BeTrue();
            var contenido = await File.ReadAllTextAsync(path);
            contenido.Should().Contain("Test Product");
        }

        [Test]
        public async Task ExportarCsv_DeberiaCrearFichero()
        {
            // Arrange
            var productos = new List<Producto>
            {
                new(1, "Test Product", 9.99m, "Test Category")
            };
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true
            };
            var path = Path.Combine(_outputDir, "test_export.csv");

            // Act
            using (var writer = new StreamWriter(path))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecords(productos);
            }

            // Assert
            File.Exists(path).Should().BeTrue();
            var contenido = await File.ReadAllTextAsync(path);
            contenido.Should().Contain("Test Product");
        }
    }

    [TestFixture]
    public class CasosNegativos : JsonCsvTests
    {
        [Test]
        public void LeerJson_FicheroNoExiste_DeberiaLanzarExcepcion()
        {
            // Arrange
            var path = Path.Combine(_dataDir, "no_existe.json");

            // Act & Assert
            var accion = () => File.ReadAllText(path);
            accion.Should().Throw<FileNotFoundException>();
        }

        [Test]
        public void LeerCsv_FicheroNoExiste_DeberiaLanzarExcepcion()
        {
            // Arrange
            var path = Path.Combine(_dataDir, "no_existe.csv");
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                BadDataFound = null
            };

            // Act & Assert
            var accion = () =>
            {
                using var reader = new StreamReader(path);
                using var csv = new CsvReader(reader, config);
                csv.GetRecords<Producto>().ToList();
            };
            accion.Should().Throw<Exception>();
        }
    }
}
