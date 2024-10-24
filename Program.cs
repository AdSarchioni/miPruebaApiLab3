using inmoWebApiLab3.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://192.168.0.18:5028");

// Agregar servicios a la colección de servicios
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configuración para ignorar referencias cíclicas y evitar incluir $id/$ref
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;  // Opcional, para formato más legible
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Agregar el contexto de base de datos con MySQL
var connectionString = builder.Configuration.GetConnectionString("MySql");
builder.Services.AddDbContext<DataContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Agregar soporte para CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

// Agregar soporte para autenticación JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["TokenAuthentication:Issuer"],
        ValidAudience = builder.Configuration["TokenAuthentication:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["TokenAuthentication:SecretKey"]))
    };

    // Manejo de eventos para registrar errores y tokens
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("Error de autenticación: " + context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token validado correctamente: " + context.SecurityToken);
            return Task.CompletedTask;
        },
        OnMessageReceived = context =>
        {
            var authorizationHeader = context.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
            {
                context.Token = authorizationHeader.Substring("Bearer ".Length).Trim();
                Console.WriteLine("Token recibido: " + context.Token);
            }
            return Task.CompletedTask;
        }
    };
});

// Configuración de la aplicación
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseRouting();
// Habilitar archivos estáticos desde wwwroot
app.UseStaticFiles();
app.UseAuthentication();  // Debe estar antes de UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();
