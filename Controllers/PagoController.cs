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
    public class PagoController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment environment;
        public PagoController(DataContext context, IConfiguration config, IWebHostEnvironment environment)
        {
            _context = context;
            this.config = config;
            this.environment = environment;
        }
         [HttpGet("obtenerPagosPorContrato")]
		public async Task<IActionResult> PagosPorContrato(int id)
        {

			try
			{
				var usuarioActual = User.Identity.Name;

			var pagos = _context.Pago
								.Where(e => e.Id_Contrato == id)
								.Include(e => e.contrato);

			if (pagos == null){
				return NotFound("No se encontró un contrato válido para este inmueble y usuario.");
			}else{
				return Ok(pagos);
			}
			}
			catch (Exception ex)
			{
				return BadRequest("Error interno del servidor:"+ ex.Message);
			}
		}














        }
}