namespace inmoWebApiLab3.Models;
using System.ComponentModel.DataAnnotations;


public class Tipo_Inmueble
{
    [Key]
    public int Id_Tipo_Inmueble { get; set; }

    public string? Tipo { get; set; }
    
    public int Estado_Tipo_Inmueble { get; set; }
}