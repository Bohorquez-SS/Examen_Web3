using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinariaApp.Data;
using VeterinariaApp.Models;
using VeterinariaApp.ViewModels;

namespace VeterinariaApp.Controllers
{
    [Authorize(Roles = RolesApp.Administrador)]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index(int? anio)
        {
            var anioSeleccionado = anio ?? DateTime.Today.Year;

            // Citas agrupadas por mes del año seleccionado
            var porMes = await _context.Citas
                .Where(c => c.FechaCita.Year == anioSeleccionado)
                .GroupBy(c => c.FechaCita.Month)
                .Select(g => new { Mes = g.Key, Total = g.Count() })
                .ToListAsync();

            var datos = new int[12];
            foreach (var item in porMes)
                datos[item.Mes - 1] = item.Total;

            var anios = await _context.Citas
                .Select(c => c.FechaCita.Year)
                .Distinct()
                .ToListAsync();
            if (!anios.Contains(DateTime.Today.Year)) anios.Add(DateTime.Today.Year);

            var modelo = new DashboardViewModel
            {
                TotalServicios = await _context.ServiciosVeterinarios.CountAsync(),
                TotalMascotas = await _context.Mascotas.CountAsync(),
                TotalUsuarios = await _context.Users.CountAsync(),
                TotalCitas = await _context.Citas.CountAsync(),
                CitasPendientes = await _context.Citas.CountAsync(c => c.Estado == EstadoCita.Pendiente),
                Anio = anioSeleccionado,
                CitasPorMes = datos,
                AniosDisponibles = anios.OrderBy(a => a).ToList()
            };

            return View(modelo);
        }
    }
}
