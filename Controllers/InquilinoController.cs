using Microsoft.AspNetCore.Mvc;
using inmoWebApiLab3.Models; // Asegúrate de que el modelo Inquilino esté en este namespace
using Microsoft.EntityFrameworkCore;  
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace inmoWebApiLab3.Controllers.API  // Asegúrate que el namespace coincida con el del proyecto
{
    
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    public class InquilinosController : ControllerBase
    {
        private readonly DataContext _context;  // Utilizamos tu DataContext aquí
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment environment;

        public InquilinosController(DataContext context, IConfiguration config, IWebHostEnvironment environment)
        {
            this._context = context;
            this.config = config;
            this.environment = environment;
        }

          
   
  [HttpGet("obtenerInquilinoPorInmueble")]
public async Task<IActionResult> InquilinoPorInmueble(int id)
{
    try
    {
        // Obtiene el email del usuario actual del token
        var usuarioActual = User.Identity.Name;
        Console.WriteLine($"Usuario actual: {usuarioActual}");

        // Comprueba si el id del inmueble se recibe correctamente
        Console.WriteLine($"ID del Inmueble recibido: {id}");

        // Realiza la consulta para encontrar al inquilino relacionado a través de Contrato
        var inquilino = _context.Contrato
            .Include(c => c.inquilino) // Incluye los datos del inquilino en el resultado
            .Include(c => c.inmueble) // Incluye la relación con el inmueble
            .Include(c => c.propietario) // Incluye la relación con el propietario
            .Where(c => c.inmueble.Id_Inmueble == id
                        && c.propietario.Email == usuarioActual) // Filtra por id del inmueble y propietario actual
            .Select(c => c.inquilino) // Selecciona solo los datos del inquilino
            .FirstOrDefault();

        // Verifica si el inquilino se ha encontrado
        if (inquilino == null)
        {
            Console.WriteLine("No se encontró ningún inquilino para el inmueble especificado.");
            return NotFound("No se encontró un inquilino para el inmueble especificado.");
        }

        // Inquilino encontrado, muestra algunos datos de ejemplo
        Console.WriteLine($"Inquilino encontrado: {inquilino.Nombre} {inquilino.Apellido}");

        return Ok(inquilino); // Devuelve el objeto inquilino como respuesta
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al obtener el inquilino: {ex.Message}");
        return BadRequest(ex.Message); // Maneja cualquier excepción que ocurra
    }
} 

   [HttpGet("obtenerInquilinos")]
public async Task<IActionResult> GetInquilinos()
{
    try
    {
        // Obtiene todos los inquilinos de la base de datos
        var inquilinos = await _context.Inquilino.ToListAsync();

        // Verifica si la lista está vacía
        if (inquilinos == null || !inquilinos.Any())
        {
            Console.WriteLine("No se encontraron inquilinos.");
            return NotFound("No se encontraron inquilinos.");
        }

        // Muestra cuántos inquilinos fueron encontrados
        Console.WriteLine($"Inquilinos encontrados: {inquilinos.Count}");

        // Devuelve la lista de inquilinos como respuesta
        return Ok(inquilinos);
    }
    catch (Exception ex)
    {
        // Imprime el mensaje de error en la consola
        Console.WriteLine($"Error al obtener los inquilinos: {ex.Message}");
        return BadRequest(ex.Message);
    }
}
   

   
    }

}  
 
