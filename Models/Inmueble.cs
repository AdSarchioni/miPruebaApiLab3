
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using inmoWebApiLab3.Models;

public class Inmueble
{
    [Key]
    public int Id_Inmueble { get; set; }
    

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    public string? Direccion { get; set; }
   
    [Required(ErrorMessage = "El uso es obligatorio.")]
    public string? Uso { get; set; }
   
    [Required(ErrorMessage = "La cantidad de ambientes es obligatoria.")]
    public int Ambientes { get; set; }
    public string? Latitud { get; set; }
    public string? Longitud { get; set; }
    [Required(ErrorMessage = "El tamaño es obligatorio.")]
    public double Tamano { get; set; }
    [Required(ErrorMessage = "El tipo de inmueble es obligatorio.")]
  
    public string? Servicios { get; set; }
    [Required(ErrorMessage = "La cantidad de baños es obligatoria.")]
    public int Bano { get; set; }
    [Required(ErrorMessage = "La cochera es obligatoria.")]
    public int Cochera { get; set; }
    [Required(ErrorMessage = "El patio es obligatorio.")]
    public int Patio { get; set; }
    [Required(ErrorMessage = "El precio es obligatorio.")]

    public double Precio { get; set; }
    [Required(ErrorMessage = "La condición es obligatoria.")]

    public string? Condicion { get; set; }


    public string? imagen { get; set; }


    public int Estado_Inmueble { get; set; }
   [ForeignKey("Id_Tipo_Inmueble")]
   public Tipo_Inmueble? tipo { get; set; }
   [ForeignKey("Id_Propietario")]
    public Propietario? propietario { get; set; }
 


}

