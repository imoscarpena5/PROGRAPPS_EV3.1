namespace Comedor31;

public class Platillo
{
    public string Nombre { get; set; } = "";
    public string Categoria { get; set; } = "";
    public decimal Precio { get; set; }
}

public class LineaTicket
{
    public string Platillo { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal Precio { get; set; }
    public decimal Subtotal => Cantidad * Precio;
}
