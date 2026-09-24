using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VeterinariaApp.Data;
using VeterinariaApp.Models;

namespace VeterinariaApp.Controllers
{
    [Authorize]
    public class CitasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CitasController(ApplicationDbContext context) => _context = context;

        private string UsuarioActualId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // ================= CLIENTE =================

        // Formulario para solicitar una cita
        [Authorize(Roles = RolesApp.Cliente)]
        public async Task<IActionResult> Create(int? servicioId)
        {
            if (!await _context.Mascotas.AnyAsync(m => m.UsuarioId == UsuarioActualId))
            {
                TempData["Error"] = "Primero registra una mascota para poder solicitar una cita.";
                return RedirectToAction("Create", "Mascotas");
            }

            var cita = new Cita { FechaCita = DateTime.Today, ServicioVeterinarioId = servicioId ?? 0 };
            await CargarListas(cita);
            return View(cita);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RolesApp.Cliente)]
        public async Task<IActionResult> Create([Bind("FechaCita,MascotaId,ServicioVeterinarioId")] Cita cita)
        {
            // Validación 1: la fecha no puede ser anterior a hoy (también lo valida [FechaNoPasada])
            if (cita.FechaCita.Date < DateTime.Today)
                ModelState.AddModelError(nameof(Cita.FechaCita), "La fecha de la cita no puede ser anterior a hoy.");

            // Validación 2: la mascota debe pertenecer al cliente autenticado
            var mascotaEsDelUsuario = await _context.Mascotas
                .AnyAsync(m => m.Id == cita.MascotaId && m.UsuarioId == UsuarioActualId);
            if (!mascotaEsDelUsuario)
                ModelState.AddModelError(nameof(Cita.MascotaId), "La mascota seleccionada no te pertenece.");

            // Validación 3: el servicio debe existir
            if (!await _context.ServiciosVeterinarios.AnyAsync(s => s.Id == cita.ServicioVeterinarioId))
                ModelState.AddModelError(nameof(Cita.ServicioVeterinarioId), "Seleccione un servicio válido.");

            if (!ModelState.IsValid)
            {
                await CargarListas(cita);
                return View(cita);
            }

            cita.Estado = EstadoCita.Pendiente;
            _context.Citas.Add(cita);
            await _context.SaveChangesAsync();

            TempData["Exito"] = "Cita registrada correctamente.";
            return RedirectToAction(nameof(MisCitas));
        }

        // Citas del cliente autenticado
        [Authorize(Roles = RolesApp.Cliente)]
        public async Task<IActionResult> MisCitas()
        {
            var citas = await _context.Citas
                .Include(c => c.Mascota)
                .Include(c => c.ServicioVeterinario)
                .Where(c => c.Mascota!.UsuarioId == UsuarioActualId)
                .OrderByDescending(c => c.FechaCita)
                .ToListAsync();
            return View(citas);
        }

        // El cliente puede cancelar sus citas pendientes
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RolesApp.Cliente)]
        public async Task<IActionResult> Cancelar(int id)
        {
            var cita = await _context.Citas
                .Include(c => c.Mascota)
                .FirstOrDefaultAsync(c => c.Id == id && c.Mascota!.UsuarioId == UsuarioActualId);

            if (cita == null) return NotFound();

            if (cita.Estado == EstadoCita.Pendiente)
            {
                cita.Estado = EstadoCita.Cancelada;
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Cita cancelada.";
            }
            return RedirectToAction(nameof(MisCitas));
        }

        // ================= ADMINISTRADOR =================

        // Listado general de citas
        [Authorize(Roles = RolesApp.Administrador)]
        public async Task<IActionResult> Index(EstadoCita? estado)
        {
            var consulta = _context.Citas
                .Include(c => c.Mascota).ThenInclude(m => m!.Usuario)
                .Include(c => c.ServicioVeterinario)
                .AsQueryable();

            if (estado.HasValue)
                consulta = consulta.Where(c => c.Estado == estado.Value);

            ViewBag.EstadoFiltro = estado;
            var citas = await consulta.OrderByDescending(c => c.FechaCita).ToListAsync();
            return View(citas);
        }

        // Cambiar el estado de una cita (Pendiente / Atendida / Cancelada)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RolesApp.Administrador)]
        public async Task<IActionResult> CambiarEstado(int id, EstadoCita estado)
        {
            var cita = await _context.Citas.FindAsync(id);
            if (cita == null) return NotFound();

            cita.Estado = estado;
            await _context.SaveChangesAsync();
            TempData["Exito"] = $"La cita #{id} ahora está {estado}.";
            return RedirectToAction(nameof(Index));
        }

        // Llena los <select> de mascotas (solo del usuario) y servicios
        private async Task CargarListas(Cita cita)
        {
            var mascotas = await _context.Mascotas
                .Where(m => m.UsuarioId == UsuarioActualId)
                .OrderBy(m => m.Nombre)
                .Select(m => new { m.Id, Texto = m.Nombre + " (" + m.Especie + ")" })
                .ToListAsync();

            var servicios = await _context.ServiciosVeterinarios
                .OrderBy(s => s.Nombre)
                .ToListAsync();

            ViewBag.Mascotas = new SelectList(mascotas, "Id", "Texto", cita.MascotaId);
            ViewBag.Servicios = new SelectList(
                servicios.Select(s => new { s.Id, Texto = $"{s.Nombre} - $ {s.Precio:N2}" }),
                "Id", "Texto", cita.ServicioVeterinarioId);
        }
    }
}
