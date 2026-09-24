using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VeterinariaApp.Models;

namespace VeterinariaApp.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();

            // Si existen migraciones se aplican; si no, se crea la BD directamente
            if (context.Database.GetMigrations().Any())
                await context.Database.MigrateAsync();
            else
                await context.Database.EnsureCreatedAsync();

            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // Roles
            foreach (var rol in new[] { RolesApp.Administrador, RolesApp.Cliente })
            {
                if (!await roleManager.RoleExistsAsync(rol))
                    await roleManager.CreateAsync(new IdentityRole(rol));
            }

            // Usuario administrador por defecto
            const string adminEmail = "admin@veterinaria.com";
            if (await userManager.FindByEmailAsync(adminEmail) is null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    NombreCompleto = "Administrador del Sistema",
                    EmailConfirmed = true
                };
                var resultado = await userManager.CreateAsync(admin, "Admin123!");
                if (resultado.Succeeded)
                    await userManager.AddToRoleAsync(admin, RolesApp.Administrador);
            }

            // Servicios de ejemplo
            if (!await context.ServiciosVeterinarios.AnyAsync())
            {
                context.ServiciosVeterinarios.AddRange(
                    new ServicioVeterinario { Nombre = "Consulta general", Descripcion = "Revisión clínica completa de la mascota.", Precio = 50 },
                    new ServicioVeterinario { Nombre = "Vacunación", Descripcion = "Aplicación de vacunas según el calendario.", Precio = 40 },
                    new ServicioVeterinario { Nombre = "Desparasitación", Descripcion = "Tratamiento interno y externo contra parásitos.", Precio = 30 },
                    new ServicioVeterinario { Nombre = "Baño y corte", Descripcion = "Baño medicado, corte de pelo y uñas.", Precio = 45 },
                    new ServicioVeterinario { Nombre = "Cirugía menor", Descripcion = "Procedimientos quirúrgicos ambulatorios.", Precio = 250 }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}
