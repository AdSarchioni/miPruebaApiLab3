
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace inmoWebApiLab3.Models;

public class Contrato
{
   [Key]
   public int Id_Contrato { get; set; }
   public int Id_Inmueble { get; set; }
   public int Id_Propietario { get; set; }
   public int Id_Inquilino { get; set; }
   [Required(ErrorMessage = "El campo Fecha de Inicio es obligatorio.")]
   [Display(Name = "Fecha desde")]
   [DataType(DataType.Date)]
   public DateTime Fecha_Inicio { get; set; }
   [Required(ErrorMessage = "El campo Mes es obligatorio.")]
   [RegularExpression(@"^\d+$", ErrorMessage = "El campo debe contener solo números.")]
   public int Meses { get; set; }
   [Display(Name = "Fecha hasta")]
   [DataType(DataType.Date)]
   public DateTime Fecha_Finalizacion { get; set; }
   public double Monto { get; set; }
   public DateTime? Finalizacion_Anticipada { get; set; }
   public int Id_Creado_Por { get; set; }
   public int Id_Terminado_Por { get; set; }
   public int Estado_Contrato { get; set; }
    [ForeignKey("Id_Inmueble")]
   public Inmueble? inmueble { get; set; }
  //  [ForeignKey("Id_Tipo_Inmueble")]
  // public Tipo_Inmueble? tipo_inmueble { get; set; }
    [ForeignKey("Id_Propietario")]
   public Propietario? propietario { get; set; }
   [ForeignKey("Id_Inquilino")]
   public Inquilino? inquilino { get; set; }
   //public int MesesPagos { get; set; }
   //public Usuario? usuario { get; set; }



}
