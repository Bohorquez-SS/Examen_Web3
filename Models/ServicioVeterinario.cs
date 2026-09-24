using System.ComponentModel.DataAnnotations;

namespace VeterinariaApp.Models
{
    public class ServicioVeterinario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(80, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 80 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(300, ErrorMessage = "La descripción no puede superar los 300 caracteres.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(typeof(decimal), "1", "10000", ErrorMessage = "El precio debe estar entre 1 y 10000.")]
        [DataType(DataType.Currency)]
        public decimal Precio { get; set; }

        // Un servicio puede estar asociado a muchas citas
        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }
}
