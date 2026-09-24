using System.ComponentModel.DataAnnotations;

namespace VeterinariaApp.Models
{
    public class Mascota
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La especie es obligatoria.")]
        [StringLength(30, ErrorMessage = "La especie no puede superar los 30 caracteres.")]
        public string Especie { get; set; } = string.Empty;

        [Required(ErrorMessage = "La raza es obligatoria.")]
        [StringLength(50, ErrorMessage = "La raza no puede superar los 50 caracteres.")]
        public string Raza { get; set; } = string.Empty;

        // FK hacia el usuario (se asigna en el controlador con el usuario autenticado)
        public string UsuarioId { get; set; } = string.Empty;
        public ApplicationUser? Usuario { get; set; }

        // Una mascota puede tener muchas citas
        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }
}
