public class InmuebleDto
{
    public string? Direccion { get; set; }
    public string? Uso { get; set; }
    public int Ambientes { get; set; }
    public double Tamano { get; set; }
    public string? Servicios { get; set; }
    public int Bano { get; set; }
    public int Cochera { get; set; }
    public int Patio { get; set; }
    public double Precio { get; set; }
    public string? Condicion { get; set; }
    public int IdPropietario { get; set; }
    public IFormFile? Imagen { get; set; } // Recibe la imagen cargada
    public int Id_Tipo_Inmueble { get; set; }
    public int Estado_Inmueble { get; set; }
}
