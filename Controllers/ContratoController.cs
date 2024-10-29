using Microsoft.AspNetCore.Mvc;
using inmoWebApiLab3.Models; 
using Microsoft.EntityFrameworkCore;  
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace inmoWebApiLab3.Controllers.API  

{
 [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]

    public class ContratoController : ControllerBase
    {
        private readonly DataContext _context;  // Utilizamos tu DataContext aquí
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment environment;

        public ContratoController(DataContext context, IConfiguration config, IWebHostEnvironment environment)
        {
            _context = context;
            this.config = config;
            this.environment = environment;
        }


   [HttpGet("obtenerContratoPorInmueble")]
public async Task<IActionResult> ContratoPorInmueble(int id)
{
    try
    {
        // Obtiene el email del usuario actual del token
        var usuarioActual = User.Identity.Name;
        Console.WriteLine($"Usuario actual: {usuarioActual}");

        // Comprueba si el id del inmueble se recibe correctamente
        Console.WriteLine($"ID del Inmueble recibido: {id}");

        // Realiza la consulta para encontrar el contrato relacionado al inmueble
        var contrato = _context.Contrato
            .Include(c => c.inmueble) // Incluye los datos del inmueble en el resultado
            .Include(c => c.inquilino) // Incluye los datos del inquilino en el resultado
            .Include(c => c.propietario) // Incluye los datos del propietario en el resultado
            .Where(c => c.inmueble.Id_Inmueble == id
                        && c.propietario.Email == usuarioActual) // Filtra por id del inmueble y propietario actual
            .FirstOrDefault();

        // Verifica si el contrato se ha encontrado
        if (contrato == null)
        {
            Console.WriteLine("No se encontró ningún contrato para el inmueble especificado.");
            return NotFound("No se encontró un contrato para el inmueble especificado.");
        }

        // Contrato encontrado, muestra algunos datos de ejemplo
        Console.WriteLine($"Contrato encontrado: ID {contrato.Id_Contrato}, Fecha de Inicio: {contrato.Fecha_Inicio}");

        return Ok(contrato); // Devuelve el objeto contrato como respuesta
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al obtener el contrato: {ex.Message}");
        return BadRequest(ex.Message); // Maneja cualquier excepción que ocurra
    }
}


    }


    }









