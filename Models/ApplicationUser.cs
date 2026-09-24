using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace VeterinariaApp.Models
{
    // Usuario de Identity extendido con el nombre completo
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres.")]
        [Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; } = string.Empty;

        // Un usuario puede registrar muchas mascotas
        public ICollection<Mascota> Mascotas { get; set; } = new List<Mascota>();
    }
}
