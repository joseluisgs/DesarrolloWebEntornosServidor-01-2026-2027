using FluentAssertions;
using Moq;
using NUnit.Framework;
using ProductoApp.Models;
using ProductoApp.Repositories.Base;
using ProductoApp.Services;

namespace ProductoApp.Tests.Services;

[TestFixture]
public class ProductoServiceTests
{
    private Mock<IProductoRepository> _mockRepository = null!;
    private ProductoService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<IProductoRepository>();
        _service = new ProductoService(_mockRepository.Object);
    }

    [TestFixture]
    public class CasosValidos : ProductoServiceTests
    {
        [Test]
        public void GetAll_ConProductos_RetornaTodos()
        {
            // Arrange
            var productos = new List<Producto>
            {
                new Producto(1, "Portátil", 999.99m),
                new Producto(2, "Ratón", 29.99m)
            };
            _mockRepository.Setup(r => r.GetAll()).Returns(productos);

            // Act
            var resultado = _service.GetAll();

            // Assert
            resultado.Should().HaveCount(2);
            _mockRepository.Verify(r => r.GetAll(), Times.Once);
        }

        [Test]
        public void GetById_Existente_RetornaProducto()
        {
            // Arrange
            var producto = new Producto(1, "Portátil", 999.99m);
            _mockRepository.Setup(r => r.GetById(1)).Returns(producto);

            // Act
            var resultado = _service.GetById(1);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Nombre.Should().Be("Portátil");
            resultado.Precio.Should().Be(999.99m);
        }

        [Test]
        public void Create_ProductoValido_RetornaProducto()
        {
            // Arrange
            var producto = new Producto(0, "Monitor", 349.99m);
            var productoCreado = new Producto(4, "Monitor", 349.99m);
            _mockRepository.Setup(r => r.Create(producto)).Returns(productoCreado);

            // Act
            var resultado = _service.Create(producto);

            // Assert
            resultado.Should().NotBeNull();
            resultado.Id.Should().Be(4);
            resultado.Nombre.Should().Be("Monitor");
        }
    }

    [TestFixture]
    public class CasosInvalidos : ProductoServiceTests
    {
        [Test]
        public void GetById_Inexistente_RetornaNull()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetById(999)).Returns((Producto?)null);

            // Act
            var resultado = _service.GetById(999);

            // Assert
            resultado.Should().BeNull();
        }
    }
}
