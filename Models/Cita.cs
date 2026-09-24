using System.ComponentModel.DataAnnotations;
using VeterinariaApp.Validations;

namespace VeterinariaApp.Models
{
    public class Cita
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "La fecha de la cita es obligatoria.")]
        [DataType(DataType.Date)]
        [FechaNoPasada(ErrorMessage = "La fecha de la cita no puede ser anterior a hoy.")]
        [Display(Name = "Fecha de la cita")]
        public DateTime FechaCita { get; set; }

        public EstadoCita Estado { get; set; } = EstadoCita.Pendiente;

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una mascota.")]
        [Display(Name = "Mascota")]
        public int MascotaId { get; set; }
        public Mascota? Mascota { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione un servicio.")]
        [Display(Name = "Servicio veterinario")]
        public int ServicioVeterinarioId { get; set; }
        public ServicioVeterinario? ServicioVeterinario { get; set; }
    }
}
