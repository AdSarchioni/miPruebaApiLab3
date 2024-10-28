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




[HttpPut("actualizarInmueble")]
public async Task<IActionResult> Put([FromBody] Inmueble entidad)
{
    try
    {
        if (ModelState.IsValid)
        {
            // Buscar el inmueble existente por su Id y propietario
            var inmuebleExistente = _context.Inmueble
                .Include(e => e.propietario)
                .FirstOrDefault(e => e.Id_Inmueble == entidad.Id_Inmueble && e.propietario.Email == User.Identity.Name);

            if (inmuebleExistente != null)
            {
                // Actualizar únicamente el atributo Estado_Inmueble
                inmuebleExistente.Estado_Inmueble = entidad.Estado_Inmueble;
                
                // Guardar los cambios en el contexto
                _context.Inmueble.Update(inmuebleExistente);
                await _context.SaveChangesAsync();
                
                return Ok(inmuebleExistente);
            }
            return NotFound("Inmueble no encontrado o no pertenece al usuario actual.");
        }
        return BadRequest("Modelo inválido.");
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}

/*		// POST api/<controller>
		[HttpPost("crearInmueble")]
		public async Task<IActionResult> Post([FromBody] Inmueble entidad)
		{
			try
			{
				if (ModelState.IsValid)
				{
					entidad.Id_Inmueble = _context.Propietario.Single(e => e.Email == User.Identity.Name).Id_Propietario;
					_context.Inmueble.Add(entidad);
					_context.SaveChanges();
					return CreatedAtAction(nameof(ObtenerPorId), new { id = entidad.Id_Inmueble }, entidad);
				}
				return BadRequest();
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}
*/
        
		[HttpPost("crearInmueble4")]
public async Task<IActionResult> Post14([FromBody] Inmueble entidad)
{
    try
    {
        if (ModelState.IsValid)
        {
            // Obtener el propietario autenticado
            var propietario = await _context.Propietario.SingleOrDefaultAsync(e => e.Email == User.Identity.Name);
            if (propietario == null)
            {
                return Unauthorized("Propietario no encontrado.");
            }

            // Asignar el propietario al inmueble
            entidad.propietario = propietario;

            // Agregar y guardar el nuevo inmueble
            _context.Inmueble.Add(entidad);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ObtenerPorId), new { id = entidad.Id_Inmueble }, entidad);
        }
        return BadRequest("Datos del inmueble inválidos.");
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}


// Código en tu controlador


[HttpPost("crearInmueble3")]
public async Task<IActionResult> CrearInmueble([FromBody] Inmueble inmueble)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    try
    {
        // Verificar que el tipo de inmueble existe en la base de datos
        var tipoInmueble = await _context.Tipo_Inmueble
            .FirstOrDefaultAsync(t => t.Id_Tipo_Inmueble == inmueble.Id_Tipo_Inmueble);

        if (tipoInmueble == null)
        {
            return NotFound(new { message = "El tipo de inmueble especificado no existe." });
        }

        // Asignar el tipo de inmueble al objeto de inmueble (sin crear una nueva instancia)
        inmueble.tipo = tipoInmueble;

        // Agregar el inmueble a la base de datos
        _context.Inmueble.Add(inmueble);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(CrearInmueble), new { id = inmueble.Id_Inmueble }, inmueble);
    }
    catch (DbUpdateException ex)
    {
        // Captura de excepción si ocurre algún problema al guardar
        return StatusCode(500, new { message = "Ocurrió un error al guardar el inmueble.", error = ex.Message });
    }
}

[HttpPost("crearInmueble2")]
public async Task<IActionResult> Post1([FromBody] Inmueble entidad)
{
    try
    {
        if (ModelState.IsValid)
        {
            // Obtener el propietario autenticado
            var propietario = await _context.Propietario.SingleOrDefaultAsync(e => e.Email == User.Identity.Name);
            if (propietario == null)
            {
                return Unauthorized("Propietario no encontrado.");
            }

            // Asignar el propietario al inmueble
            entidad.propietario = propietario;

            // Si no se ha especificado un tipo de inmueble en el JSON, buscar un tipo predeterminado
            if (entidad.tipo == null)
            {
                // Buscar un tipo predeterminado (puedes ajustar la lógica según tus necesidades)
                var tipoInmueblePredeterminado = await _context.Tipo_Inmueble
                    .FirstOrDefaultAsync(t => t.Id_Tipo_Inmueble == entidad.Id_Tipo_Inmueble); // O cualquier criterio para seleccionar el tipo por defecto

                if (tipoInmueblePredeterminado == null)
                {
                    return NotFound("Tipo de inmueble predeterminado no encontrado.");
                }

                // Asignar el tipo predeterminado al inmueble
                entidad.tipo = tipoInmueblePredeterminado;
            }

            // Agregar y guardar el nuevo inmueble
            _context.Inmueble.Add(entidad);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ObtenerPorId), new { id = entidad.Id_Inmueble }, entidad);
        }
        return BadRequest("Datos del inmueble inválidos.");
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}
 [HttpPost("crearInmueble")]
public async Task<IActionResult> Post([FromForm] InmuebleDto entidadDto)
{
    try
    {
        if (ModelState.IsValid)
        {
            // Obtiene el Id_Propietario de las claims
            var propietarioId = _context.Propietario
                .Single(e => e.Email == User.Identity.Name)
                .Id_Propietario;

            // Crea una instancia de Inmueble y asigna los valores del DTO
            var entidad = new Inmueble
            {
                Direccion = entidadDto.Direccion,
                Uso = entidadDto.Uso,
                Ambientes = entidadDto.Ambientes,
                Bano = entidadDto.Bano,
                Condicion = entidadDto.Condicion,
                Servicios = entidadDto.Servicios,
                Precio = entidadDto.Precio,
                Tamano = entidadDto.Tamano,
                Patio = entidadDto.Patio,
                Cochera = entidadDto.Cochera,
                Id_Tipo_Inmueble = entidadDto.Id_Tipo_Inmueble,
                Estado_Inmueble = entidadDto.Estado_Inmueble,
                Id_Propietario = propietarioId
            };

            // Si hay una imagen, guárdala en la carpeta `wwwroot/img`
            if (entidadDto.Imagen != null)
            {
                // Genera la ruta completa para guardar la imagen
                var imagePath = Path.Combine("wwwroot/img", entidadDto.Imagen.FileName);
                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await entidadDto.Imagen.CopyToAsync(stream);
                }

                // Guarda solo el nombre del archivo en la base de datos
                entidad.imagen = entidadDto.Imagen.FileName; // Guardamos solo el nombre del archivo
            }

            // Guarda el inmueble en la base de datos
            _context.Inmueble.Add(entidad);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ObtenerPorId), new { id = entidad.Id_Inmueble }, entidad);
        }

        return BadRequest();
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}
	[HttpGet("ObtenerAlquilados")]
		public async Task<IActionResult> ObtenerAlquilados()
		{
			try
			{
				var usuarioActual = User.Identity.Name;

				return Ok(_context.Contrato.Include(e => e.inmueble)
                                            .Include(e => e.inquilino)
                                            .Include(e => e.inmueble.propietario)
                                            .Where(e => e.inmueble.propietario.Email == usuarioActual)
											.Select(e => e.inmueble ));
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}

}

    } 
    
 
    




