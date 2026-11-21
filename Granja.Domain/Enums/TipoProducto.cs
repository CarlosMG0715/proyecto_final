namespace Granja.Domain.Enums;

public enum TipoProducto
{
    Queso = 1,
    Yogurt = 2
}

public static class TipoProductoExtensions
{
    /// <summary>Unidad en la que se mide el producto terminado (queso en kg, yogurt en litros).</summary>
    public static string Unidad(this TipoProducto tipo) => tipo switch
    {
        TipoProducto.Queso => "kg",
        TipoProducto.Yogurt => "L",
        _ => string.Empty
    };
}
