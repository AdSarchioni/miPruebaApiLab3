using Microsoft.AspNetCore.Mvc;
using inmoWebApiLab3.Models;
using Microsoft.EntityFrameworkCore;  
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;


namespace inmoWebApiLab3.Controllers.API  // Asegúrate que el namespace coincida con el del proyecto
{

    [Route("api/[controller]")]
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[ApiController]
 public class InmuebleController : ControllerBase
    {
        private readonly DataContext _context;  // Utilizamos tu DataContext aquí
	    private readonly IConfiguration config;
		private readonly IWebHostEnvironment environment;

        public InmuebleController(DataContext context, IConfiguration config, IWebHostEnvironment environment)
        {
            this._context = context;
            this.config = config;
            this.environment = environment;

        }

        // Método para obtener todos los propietarios
 [HttpGet("ObtenerInmuebles")]
// Comentado, pero originalmente este atributo indica que esta acción puede ser accedida por una petición GET a la ruta "ObtenerInmuebles".

public async Task<IActionResult> Obtener()
{
    try
    {
        // Obtiene el nombre del usuario actual desde las claims de identidad (que generalmente es el correo del usuario).
        var usuarioActual = User.Identity.Name;

        // Realiza una consulta a la base de datos, incluye la relación con la tabla Propietario
        // y busca todos los inmuebles cuyo propietario tenga el mismo correo que el usuario actual.
        var inmuebles = await _context.Inmueble
                                      .Include(e => e.propietario) // Incluye los datos del propietario relacionados con el inmueble.
                                      .Where(e => e.propietario.Email == usuarioActual) // Filtra por el correo del propietario.
                                      .ToListAsync(); // Ejecuta la consulta de forma asíncrona y convierte los resultados en una lista.

        // Verifica si no se encontraron inmuebles o si la lista de inmuebles está vacía.
        if (inmuebles == null || !inmuebles.Any())
        {
            // Si no hay inmuebles asociados al usuario actual, responde con un mensaje 404 (No encontrado).
            return NotFound("No se encontraron inmuebles para el usuario actual.");
        }

        // Si hay inmuebles, devuelve una respuesta 200 (OK) junto con la lista de inmuebles.
        return Ok(inmuebles);
    }
    catch (Exception ex)
    {
        // Si ocurre una excepción durante la ejecución, captura el error y devuelve una respuesta 400 (Solicitud incorrecta),
        // incluyendo un mensaje de error con la descripción de la excepción.
        return BadRequest("Se produjo un error al procesar la solicitud." + "\n" + ex.Message);
    }
}

  [HttpGet("ObtenerInmueble/{id}")]
public async Task<IActionResult> ObtenerPorId(int id)
{
    try
    {
        // Obtiene el nombre del usuario actual desde las claims de identidad.
        var usuarioActual = User.Identity.Name;

        // Realiza una consulta a la base de datos, incluyendo la relación con la tabla Propietario.
        // Filtra por el Id_Inmueble y el correo del propietario (usuario actual).
        var inmueble = await _context.Inmueble
                                     .Include(e => e.propietario) // Incluye los datos del propietario relacionados con el inmueble.
                                     .FirstOrDefaultAsync(e => e.Id_Inmueble == id && e.propietario.Email == usuarioActual); // Filtra por Id_Inmueble y correo del propietario.

        // Verifica si el inmueble no se encuentra (es null).
        if (inmueble == null)
        {
            // Si no se encuentra el inmueble, responde con un mensaje 404 (No encontrado).
            return NotFound($"No se encontró el inmueble con ID {id} para el usuario actual.");
        }

        // Si se encuentra el inmueble, devuelve una respuesta 200 (OK) junto con el inmueble.
        return Ok(inmueble);
    }
    catch (Exception ex)
    {
        // Si ocurre una excepción, captura el error y devuelve una respuesta 400 (Solicitud incorrecta),
        // incluyendo un mensaje de error con la descripción de la excepción.
        return BadRequest("Se produjo un error al procesar la solicitud." + "\n" + ex.Message);
    }
}       
[HttpPut("ActualizarInmueble")]
        public async Task<IActionResult> Actualizar([FromBody] Inmueble IEditado)
		{
			try
			{
				var usuarioActual = User.Identity.Name;

				// Verifico si el modelo recibido es válido
				if (!ModelState.IsValid)
				{
					return BadRequest(ModelState);
				}

				// Verifico si el inmueble pertenece al usuario actual
				var inmuebleExistente = await _context.Inmueble
					.Include(i => i.propietario)
					.AsNoTracking()
					.FirstOrDefaultAsync(i => i.Id_Inmueble == IEditado.Id_Inmueble && i.propietario.Email == usuarioActual);

				if (inmuebleExistente != null)
				{
					// Actualizo el inmueble
					_context.Inmueble.Update(IEditado);
					await _context.SaveChangesAsync();

					return Ok(IEditado);
				}
				else
				{
					return NotFound("No se pudo encontrar el inmueble o no tienes permiso para editarlo.");
				}
			}
			catch (Exception ex)
			{

				return BadRequest("Se produjo un error al procesar la solicitud.");
			}
		}




    } 
    
 }
    




