using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VeterinariaApp.Data;
using VeterinariaApp.Models;

namespace VeterinariaApp.Controllers
{
    [Authorize(Roles = RolesApp.Cliente)]
    public class MascotasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MascotasController(ApplicationDbContext context) => _context = context;

        private string UsuarioActualId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // Solo las mascotas del usuario autenticado
        public async Task<IActionResult> Index()
        {
            var mascotas = await _context.Mascotas
                .Where(m => m.UsuarioId == UsuarioActualId)
                .Include(m => m.Citas)
                .OrderBy(m => m.Nombre)
                .ToListAsync();
            return View(mascotas);
        }

        public IActionResult Create() => View(new Mascota());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Especie,Raza")] Mascota mascota)
        {
            if (!ModelState.IsValid) return View(mascota);

            // La mascota queda asociada al usuario autenticado
            mascota.UsuarioId = UsuarioActualId;
            _context.Mascotas.Add(mascota);
            await _context.SaveChangesAsync();

            TempData["Exito"] = $"{mascota.Nombre} fue registrada correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}
