using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeterinariaApp.Data;
using VeterinariaApp.Models;
using VeterinariaApp.Services;

namespace VeterinariaApp.Controllers
{
    [Authorize(Roles = RolesApp.Administrador)]
    public class ReportesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ReportePdfService _pdf;

        public ReportesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ReportePdfService pdf)
        {
            _context = context;
            _userManager = userManager;
            _pdf = pdf;
        }

        public async Task<IActionResult> Index()
        {
            var clientes = await _userManager.GetUsersInRoleAsync(RolesApp.Cliente);
            ViewBag.Clientes = new SelectList(
                clientes.OrderBy(c => c.NombreCompleto)
                        .Select(c => new { c.Id, Texto = $"{c.NombreCompleto} ({c.Email})" }),
                "Id", "Texto");
            return View();
        }

        // 1. Listado general de citas
        public async Task<IActionResult> CitasGeneral()
        {
            var citas = await _context.Citas
                .Include(c => c.Mascota).ThenInclude(m => m!.Usuario)
                .Include(c => c.ServicioVeterinario)
                .OrderBy(c => c.FechaCita)
                .ToListAsync();

            var filas = citas.Select(c => new[]
            {
                c.Mascota?.Nombre ?? "-",
                c.Mascota?.Usuario?.NombreCompleto ?? "-",
                c.ServicioVeterinario?.Nombre ?? "-",
                c.FechaCita.ToString("dd/MM/yyyy"),
                c.Estado.ToString()
            }).ToList();

            var bytes = _pdf.GenerarReporte("Listado general de citas", null,
                new[] { "Mascota", "Dueño", "Servicio", "Fecha", "Estado" }, filas);

            return File(bytes, "application/pdf");
        }

        // 2. Citas de un cliente seleccionado
        public async Task<IActionResult> CitasPorUsuario(string? usuarioId)
        {
            if (string.IsNullOrEmpty(usuarioId))
            {
                TempData["Error"] = "Seleccione un cliente para generar el reporte.";
                return RedirectToAction(nameof(Index));
            }

            var usuario = await _userManager.FindByIdAsync(usuarioId);
            if (usuario == null) return NotFound();

            var citas = await _context.Citas
                .Include(c => c.Mascota)
                .Include(c => c.ServicioVeterinario)
                .Where(c => c.Mascota!.UsuarioId == usuarioId)
                .OrderBy(c => c.FechaCita)
                .ToListAsync();

            var filas = citas.Select(c => new[]
            {
                c.Mascota?.Nombre ?? "-",
                c.ServicioVeterinario?.Nombre ?? "-",
                c.FechaCita.ToString("dd/MM/yyyy"),
                c.Estado.ToString()
            }).ToList();

            var bytes = _pdf.GenerarReporte("Citas por cliente",
                $"Cliente: {usuario.NombreCompleto} ({usuario.Email})",
                new[] { "Mascota", "Servicio", "Fecha", "Estado" }, filas);

            return File(bytes, "application/pdf");
        }

        // 3. Servicios ordenados por cantidad de citas
        public async Task<IActionResult> ServiciosMasSolicitados()
        {
            var datos = await _context.ServiciosVeterinarios
                .Select(s => new { s.Nombre, s.Precio, Total = s.Citas.Count })
                .OrderByDescending(x => x.Total)
                .ThenBy(x => x.Nombre)
                .ToListAsync();

            var posicion = 1;
            var filas = datos.Select(d => new[]
            {
                (posicion++).ToString(),
                d.Nombre,
                $"$ {d.Precio:N2}",
                d.Total.ToString()
            }).ToList();

            var bytes = _pdf.GenerarReporte("Servicios más solicitados", "Ordenados por cantidad de citas registradas",
                new[] { "#", "Servicio", "Precio", "Cantidad de citas" }, filas);

            return File(bytes, "application/pdf");
        }
    }
}
