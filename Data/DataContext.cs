
using Microsoft.EntityFrameworkCore;

namespace inmoWebApiLab3.Models
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        { }

        public DbSet<Propietario> Propietario { get; set; }
        public DbSet<Inquilino> Inquilino { get; set; }
        public DbSet<Inmueble> Inmueble { get; set; }
        public DbSet<Tipo_Inmueble> Tipo_Inmueble {get; set;}

        public DbSet<Contrato> Contrato { get; set; }
        public DbSet<Pago> Pago { get; set; }


  // Sobrescribe el método OnModelCreating para definir relaciones y configuraciones personalizadas
  /*  protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configura la relación entre Inmueble y Propietario usando Id_Propietario como clave externa
        modelBuilder.Entity<Inmueble>()
            .HasOne(i => i.propietario)
            .WithMany() // Con Many si un propietario tiene múltiples inmuebles o puedes especificar la colección aquí
            .HasForeignKey(i => i.Id_Propietario);

        // Otras configuraciones que necesites para otras entidades...
    }
*/

    }
}
