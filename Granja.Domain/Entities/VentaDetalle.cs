namespace Granja.Domain.Entities;

public class VentaDetalle
{
    private VentaDetalle()
    {
    }

    public VentaDetalle(int loteId, DateOnly fechaProduccion, decimal cantidad)
    {
        LoteId = loteId;
        FechaProduccion = fechaProduccion;
        Cantidad = cantidad;
    }

    public int Id { get; private set; }
    public int VentaId { get; private set; }
    public int LoteId { get; private set; }
    public DateOnly FechaProduccion { get; private set; }
    public decimal Cantidad { get; private set; }
}
