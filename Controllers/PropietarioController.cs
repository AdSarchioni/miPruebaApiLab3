using Microsoft.AspNetCore.Mvc;
using inmoWebApiLab3.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using MimeKit;

namespace inmoWebApiLab3.Controllers.API  // Asegúrate que el namespace coincida con el del proyecto
{

    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]


    public class PropietariosController : ControllerBase
    {
        private readonly DataContext _context;  // Utilizamos tu DataContext aquí
        private readonly IConfiguration config;
        private readonly IWebHostEnvironment environment;

        public PropietariosController(DataContext context, IConfiguration config, IWebHostEnvironment environment)
        {
            this._context = context;
            this.config = config;
            this.environment = environment;

        }

        // Método para obtener todos los propietarios



        [HttpGet]
        public async Task<IActionResult> GetPropietarios()  // Se usa async porque es una consulta a una base de datos asíncronamente
        {
            var propietarios = await _context.Propietario.ToListAsync();  // Recupera todos los propietarios de la base de datos

            return Ok(propietarios);  // Devuelve los propietarios en formato JSON
        }

        //Método para obtener un propietario por su ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPropietario(int id)
        {
            var propietario = await _context.Propietario.FirstOrDefaultAsync(p => p.Id_Propietario == id);  // Busca un propietario por ID

            if (propietario == null)
            {
                return NotFound();  // Si no se encuentra, devuelve un error 404
            }

            return Ok(propietario);  // Devuelve el propietario en formato JSON
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePropietario(int id, Propietario propietario)
        {
            // Verificar que el ID proporcionado coincide con el del modelo
            if (id != propietario.Id_Propietario)
            {
                return BadRequest("El ID proporcionado no coincide con el ID del Propietario.");  // Devuelve un error 400 si los IDs no coinciden
            }

            // Verificar si el propietario existe en la base de datos
            var existingPropietario = await _context.Propietario.FindAsync(id);
            if (existingPropietario == null)
            {
                return NotFound("El Propietario con el ID especificado no existe.");  // Devuelve un error 404 si no existe
            }

            // Actualizar las propiedades del propietario existente
            existingPropietario.Nombre = propietario.Nombre;  // Asigna los valores que quieres actualizar
            existingPropietario.Apellido = propietario.Apellido;// Asegúrate de incluir todas las propiedades necesarias
            existingPropietario.Direccion = propietario.Direccion;
            existingPropietario.Telefono = propietario.Telefono;
            existingPropietario.Email = propietario.Email;
            existingPropietario.Dni = propietario.Dni;
            existingPropietario.Estado_Propietario = propietario.Estado_Propietario;



            if (ModelState.IsValid)
            {
                try
                {
                    await _context.SaveChangesAsync();  // Guarda los cambios
                    return NoContent();  // Éxito, devuelve 204
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException)
                {
                    // Verificar si el propietario sigue existiendo
                    if (!_context.Propietario.Any(e => e.Id_Propietario == id))
                    {
                        return NotFound("El Propietario con el ID especificado ya no existe.");
                    }
                    else
                    {
                        throw;  // Si ocurre algún otro error, lanzarlo
                    }
                }
            }

            return BadRequest(ModelState);  // Devuelve un error si el modelo es inválido
        }






        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginView loginView)
        {
            Console.WriteLine("datos de api clave + usuario: " + loginView.Clave + loginView.Usuario);
            try
            {
                // Verifica si el modelo es válido
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Hash de la contraseña ingresada
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: loginView.Clave,
                    salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 1000,
                    numBytesRequested: 256 / 8));

                // Busca al propietario en la base de datos
                var propietario = await _context.Propietario.FirstOrDefaultAsync(x => x.Email == loginView.Usuario);
                if (propietario == null || propietario.Clave != hashed)
                {
                    return BadRequest("Nombre de usuario o clave incorrecta");
                }
                else
                {
                    // Genera el token JWT
                    var key = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(config["TokenAuthentication:SecretKey"]));
                    var credenciales = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                    var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, propietario.Email),
                new Claim("FullName", propietario.Nombre + " " + propietario.Apellido),
                new Claim("IdPropietario",propietario.Id_Propietario.ToString()),
                new Claim(ClaimTypes.Role, "Propietario"),
            };

                    var token = new JwtSecurityToken(
                        issuer: config["TokenAuthentication:Issuer"],
                        audience: config["TokenAuthentication:Audience"],
                        claims: claims,
                        expires: DateTime.Now.AddMinutes(60),
                        signingCredentials: credenciales
                    );

                    // Registra el token generado
                    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
                    Console.WriteLine("Token generado: " + tokenString); // Agrega esta línea para depurar

                    // Retorna solo el token como una cadena
                    return Ok(tokenString);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet("perfil")]
        public async Task<IActionResult> Perfil()
        {

            try
            {
                var email = User.FindFirst(ClaimTypes.Name)?.Value;
                var propietario = await _context.Propietario.SingleOrDefaultAsync(x => x.Email == email);

                if (propietario == null)
                {
                    return NotFound();
                }
                return Ok(propietario);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("editar_perfil")]

        public async Task<IActionResult> EditarPerfil([FromBody] Propietario propietarioActualizado)
        {
            try
            {
                // Verifica si el modelo es válido
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var email = User.FindFirst(ClaimTypes.Name)?.Value;
                var propietario = await _context.Propietario.SingleOrDefaultAsync(x => x.Email == email);

                if (propietario == null)
                {
                    return NotFound("Propietario no encontrado");
                }

                // Actualiza los campos necesarios
                propietario.Nombre = propietarioActualizado.Nombre;
                propietario.Apellido = propietarioActualizado.Apellido;
                propietario.Telefono = propietarioActualizado.Telefono; // Asegúrate de que este campo existe en tu modelo
                propietario.Direccion = propietarioActualizado.Direccion; // Asegúrate de que este campo existe en tu modelo
                propietario.Dni = propietarioActualizado.Dni;


                // Guarda los cambios en la base de datos
                _context.Propietario.Update(propietario);
                await _context.SaveChangesAsync();

                return Ok(propietario);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("cambiar_contrasena")]
        [Authorize]
        public async Task<IActionResult> CambiarContrasena([FromBody] CambiarContrasenaView cambiarContrasenaView)
        {
            try
            {
                // Verifica si el modelo es válido
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var email = User.FindFirst(ClaimTypes.Name)?.Value;
                var propietario = await _context.Propietario.SingleOrDefaultAsync(x => x.Email == email);

                if (propietario == null)
                {
                    return NotFound("Propietario no encontrado");
                }

                // Hash de la contraseña ingresada
                string hashedActual = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: cambiarContrasenaView.ContrasenaActual,
                    salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 1000,
                    numBytesRequested: 256 / 8));

                // Verifica si la contraseña actual es correcta
                if (propietario.Clave != hashedActual)
                {
                    return BadRequest("La contraseña actual es incorrecta");
                }

                // Hash de la nueva contraseña
                string hashedNueva = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: cambiarContrasenaView.ContrasenaNueva,
                    salt: System.Text.Encoding.ASCII.GetBytes(config["Salt"]),
                    prf: KeyDerivationPrf.HMACSHA1,
                    iterationCount: 1000,
                    numBytesRequested: 256 / 8));

                // Actualiza la contraseña en el modelo
                propietario.Clave = hashedNueva;

                // Guarda los cambios en la base de datos
                _context.Propietario.Update(propietario);
                await _context.SaveChangesAsync();

                return Ok("Contraseña cambiada con éxito");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        //recupero de olvido mi contraseña para que le mande el mail 

        // GET api/<controller>/token
        [HttpGet("token")]
        public async Task<IActionResult> Token()
        {

            try
            {
                var perfil = new
                {
                    Email = User.Claims.First(x => x.Type == ClaimTypes.Email).Value, // sacamos el email correcto
                    Nombre = User.Claims.First(x => x.Type == "Apellido").Value, // sacamos el apellido 
                };
                // Busca el propietario por nombre de usuario
                var propietario = await _context.Propietario.FirstOrDefaultAsync(p => p.Email == perfil.Email);
                //se genera clave de 4 
                Random rand = new Random(Environment.TickCount);
                string randomChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789";
                string nuevaClave = "";
                for (int i = 0; i < 4; i++)
                {
                    nuevaClave += randomChars[rand.Next(0, randomChars.Length)];
                }
                byte[] salt;
                salt = System.Text.Encoding.ASCII.GetBytes(config["Salt"]);
                // Generar el hash de la nueva contraseña
                string nuevaContrasena = nuevaClave;  //generar una nueva contraseña por defecto.
                string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                                password: nuevaContrasena,
                                salt: salt,
                                prf: KeyDerivationPrf.HMACSHA1,
                                iterationCount: 1000,
                                numBytesRequested: 256 / 8));


                // actualiza la contraseña del propietario
                propietario.Clave = hashed;
                // guarda los cambios en la base de datos
                _context.SaveChanges();


                //Console.WriteLine($"Enviando correo a: {perfil.Email}, con nombre: {perfil.Nombre}");

                // Crear el mensaje de correo
                var message = new MimeKit.MimeMessage();
                message.To.Add(new MailboxAddress(perfil.Nombre, perfil.Email));
                message.From.Add(new MailboxAddress("Sistema", config["SMTPSettings:SMTPUser"]));
                message.Subject = "Prueba de Correo desde API";
                message.Body = new TextPart("html")
                {
                    Text = @$"<h1>Hola</h1>
                     <p>¡Solicitaste el cambio tu Clave, {perfil.Nombre}!</p>
                     <p>Tu nueva clave es: {nuevaClave}</p>",
                };

                // Usar MailKit.Net.Smtp.SmtpClient para Mailtrap
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.ServerCertificateValidationCallback = (object sender,
                        System.Security.Cryptography.X509Certificates.X509Certificate certificate,
                        System.Security.Cryptography.X509Certificates.X509Chain chain,
                        System.Net.Security.SslPolicyErrors sslPolicyErrors) => true;

                    // Conectar a Mailtrap
                    client.Connect("sandbox.smtp.mailtrap.io", 2525, MailKit.Security.SecureSocketOptions.StartTls);
                    client.Authenticate(config["SMTPSettings:SMTPUser"], config["SMTPSettings:SMTPPass"]); // Autenticación con Mailtrap
                    await client.SendAsync(message);
                    client.Disconnect(true);
                }

                return Ok(perfil);
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Error al generar no sabemos que : {ex.Message}");
                return BadRequest(ex.Message);

            }
        }

        [HttpPost("email")]
        [AllowAnonymous]
        public async Task<IActionResult> email([FromForm] string email)
        {
            Console.WriteLine(email);

            try
            {
                var entidad = await _context.Propietario.FirstOrDefaultAsync(x => x.Email == email);

                if (entidad != null)
                {
                    var token = GenerarToken(entidad);
                    Console.WriteLine(token);
                    return Ok(token); // Solo devuelve el token
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public string GenerarToken(Propietario propietario)
        {
            // Configuración del token desde el archivo appsettings.json
            var issuer = config["TokenAuthentication:Issuer"];
            var audience = config["TokenAuthentication:Audience"];
            var secretKey = config["TokenAuthentication:SecretKey"];

            // Convierte la clave secreta a bytes
            var key = Encoding.ASCII.GetBytes(secretKey);

            // Crea el manejador de tokens
            var tokenHandler = new JwtSecurityTokenHandler();

            // Define el descriptor del token, que contiene los claims y las configuraciones
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim(ClaimTypes.NameIdentifier, propietario.Id_Propietario.ToString()),  // Identificador único del propietario
            new Claim("Apellido", propietario.Apellido),  // Apellido del propietario
            new Claim(ClaimTypes.Name, propietario.Nombre),  // Nombre del propietario
            new Claim(ClaimTypes.Email, propietario.Email),  // Email del propietario
                }),
                Expires = DateTime.UtcNow.AddHours(1),  // El token será válido por 1 hora
                Issuer = issuer,  // Configuración del Issuer
                Audience = audience,  // Configuración del Audience
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)  // Configuración de las credenciales de firma
            };

            // Crea el token
            var tokenMail = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(tokenMail);

            // Escribe el token en formato string
            return tokenString;
        }



    }
}





