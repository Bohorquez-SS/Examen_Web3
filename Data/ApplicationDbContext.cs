using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VeterinariaApp.Models;

namespace VeterinariaApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Mascota> Mascotas => Set<Mascota>();
        public DbSet<ServicioVeterinario> ServiciosVeterinarios => Set<ServicioVeterinario>();
        public DbSet<Cita> Citas => Set<Cita>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // necesario para las tablas de Identity

            // Usuario 1 --- N Mascotas
            builder.Entity<Mascota>()
                .HasOne(m => m.Usuario)
                .WithMany(u => u.Mascotas)
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Mascota 1 --- N Citas
            builder.Entity<Cita>()
                .HasOne(c => c.Mascota)
                .WithMany(m => m.Citas)
                .HasForeignKey(c => c.MascotaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Servicio 1 --- N Citas (no se puede borrar un servicio con citas)
            builder.Entity<Cita>()
                .HasOne(c => c.ServicioVeterinario)
                .WithMany(s => s.Citas)
                .HasForeignKey(c => c.ServicioVeterinarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // El estado se guarda como texto ("Pendiente", "Atendida", "Cancelada")
            builder.Entity<Cita>()
                .Property(c => c.Estado)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Entity<ServicioVeterinario>()
                .Property(s => s.Precio)
                .HasPrecision(10, 2);
        }
    }
}
