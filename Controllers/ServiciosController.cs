using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinariaApp.Data;
using VeterinariaApp.Models;

namespace VeterinariaApp.Controllers
{
    // CRUD de servicios: solo el Administrador
    [Authorize(Roles = RolesApp.Administrador)]
    public class ServiciosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiciosController(ApplicationDbContext context) => _context = context;

        // Catálogo público de servicios (lo ven clientes y visitantes)
        [AllowAnonymous]
        public async Task<IActionResult> Catalogo()
        {
            var servicios = await _context.ServiciosVeterinarios.OrderBy(s => s.Nombre).ToListAsync();
            return View(servicios);
        }

        // LISTAR
        public async Task<IActionResult> Index()
        {
            var servicios = await _context.ServiciosVeterinarios
                .Include(s => s.Citas)
                .OrderBy(s => s.Nombre)
                .ToListAsync();
            return View(servicios);
        }

        // CREAR
        public IActionResult Create() => View(new ServicioVeterinario());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Descripcion,Precio")] ServicioVeterinario servicio)
        {
            if (!ModelState.IsValid) return View(servicio);

            _context.ServiciosVeterinarios.Add(servicio);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Servicio creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // EDITAR
        public async Task<IActionResult> Edit(int id)
        {
            var servicio = await _context.ServiciosVeterinarios.FindAsync(id);
            if (servicio == null) return NotFound();
            return View(servicio);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Descripcion,Precio")] ServicioVeterinario servicio)
        {
            if (id != servicio.Id) return NotFound();
            if (!ModelState.IsValid) return View(servicio);

            _context.Update(servicio);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Servicio actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // ELIMINAR
        public async Task<IActionResult> Delete(int id)
        {
            var servicio = await _context.ServiciosVeterinarios
                .Include(s => s.Citas)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (servicio == null) return NotFound();
            return View(servicio);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var servicio = await _context.ServiciosVeterinarios.FindAsync(id);
            if (servicio == null) return NotFound();

            // Integridad: no se elimina un servicio que ya tiene citas
            if (await _context.Citas.AnyAsync(c => c.ServicioVeterinarioId == id))
            {
                TempData["Error"] = "No se puede eliminar: el servicio tiene citas registradas.";
                return RedirectToAction(nameof(Index));
            }

            _context.ServiciosVeterinarios.Remove(servicio);
            await _context.SaveChangesAsync();
            TempData["Exito"] = "Servicio eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
