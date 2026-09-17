// ============================================================
// SOLID - L: Liskov Substitution Principle (LSP)
// Los subtipos deben ser sustituibles por sus tipos base
// ============================================================

namespace _11_PatronesDiseno.SOLID;

// ❌ MALO: Cuadrado hereda de Rectangulo pero ROMPE el comportamiento
public class RectanguloMal
{
    public virtual int Ancho { get; set; }
    public virtual int Alto { get; set; }
    public int Area => Ancho * Alto;
}

public class CuadradoMal : RectanguloMal
{
    // ¡ROMPE LSP! Al cambiar Ancho, cambia Alto también
    public override int Ancho
    {
        set { base.Ancho = value; base.Alto = value; }
    }
    public override int Alto
    {
        set { base.Ancho = value; base.Alto = value; }
    }
}

// Si usas Cuadrado donde se espera Rectangulo, el comportamiento es inesperado:
// var r = new CuadradoMal();
// r.Ancho = 5;  // Alto también cambia a 5 (¿quién lo esperaba?)

// ✅ BUENO: Usar composición en vez de herencia
public interface IFigura
{
    int Area();
}

public record Rectangulo(int Ancho, int Alto) : IFigura
{
    public int Area() => Ancho * Alto;
}

public record Cuadrado(int Lado) : IFigura
{
    public int Area() => Lado * Lado;
}

// Ambas implementan IFigura correctamente, sin sorpresas
