using FluentAssertions;
using Moq;
using NUnit.Framework;
using _06_Redis_Cache.Models;
using _06_Redis_Cache.Services;

namespace _06_Redis_Cache.Tests;

/// <summary>
/// Tests unitarios de ProductoService con Moq para ICacheService.
/// </summary>
[TestFixture]
public class ProductoServiceTests
{
    private Mock<ICacheService> _mockCache = null!;
    private ProductoService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _mockCache = new Mock<ICacheService>();
        _service = new ProductoService(_mockCache.Object);
    }

    [TestFixture]
    public class CasosPositivos : ProductoServiceTests
    {
        [Test]
        public async Task GetByIdAsync_CacheHit_DeberiaDevolverDeCache()
        {
            // Arrange
            var producto = new Producto(1, "Portátil Dell", 1299.99m, "Informática");
            _mockCache.Setup(c => c.GetAsync<Producto>("producto:1"))
                .ReturnsAsync(producto);

            // Act
            var resultado = await _service.GetByIdAsync(1);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Nombre.Should().Be("Portátil Dell");
            _mockCache.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<Producto>(), It.IsAny<TimeSpan?>()), Times.Never);
        }

        [Test]
        public async Task GetByIdAsync_CacheMiss_DeberiaBuscarEnDbYGuardarEnCache()
        {
            // Arrange
            _mockCache.Setup(c => c.GetAsync<Producto>("producto:1"))
                .ReturnsAsync((Producto?)null);
            _mockCache.Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<Producto>(), It.IsAny<TimeSpan?>()))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _service.GetByIdAsync(1);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Id.Should().Be(1);
            _mockCache.Verify(c => c.SetAsync("producto:1", It.IsAny<Producto>(), It.IsAny<TimeSpan?>()), Times.Once);
        }

        [Test]
        public async Task GetAllAsync_CacheHit_DeberiaDevolverDeCache()
        {
            // Arrange
            var productos = new List<Producto>
            {
                new(1, "Producto A", 10m, "Cat1"),
                new(2, "Producto B", 20m, "Cat2")
            };
            _mockCache.Setup(c => c.GetAsync<List<Producto>>("producto:all"))
                .ReturnsAsync(productos);

            // Act
            var resultado = await _service.GetAllAsync();

            // Assert
            resultado.Should().HaveCount(2);
        }

        [Test]
        public async Task UpdateAsync_DeberiaInvalidarCache()
        {
            // Arrange
            _mockCache.Setup(c => c.RemoveAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var resultado = await _service.UpdateAsync(1, 1199.99m);

            // Assert
            resultado.Should().NotBeNull();
            resultado!.Precio.Should().Be(1199.99m);
            _mockCache.Verify(c => c.RemoveAsync("producto:1"), Times.Once);
            _mockCache.Verify(c => c.RemoveAsync("producto:all"), Times.Once);
        }
    }

    [TestFixture]
    public class CasosNegativos : ProductoServiceTests
    {
        [Test]
        public async Task GetByIdAsync_Inexistente_DeberiaRetornarNull()
        {
            // Arrange
            _mockCache.Setup(c => c.GetAsync<Producto>("producto:999"))
                .ReturnsAsync((Producto?)null);

            // Act
            var resultado = await _service.GetByIdAsync(999);

            // Assert
            resultado.Should().BeNull();
        }

        [Test]
        public async Task UpdateAsync_Inexistente_DeberiaRetornarNull()
        {
            // Arrange
            _mockCache.Setup(c => c.RemoveAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            // Act
            var resultado = await _service.UpdateAsync(999, 10m);

            // Assert
            resultado.Should().BeNull();
        }
    }
}
