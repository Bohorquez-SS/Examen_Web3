using System.ComponentModel.DataAnnotations;

namespace VeterinariaApp.Validations
{
    // Validación personalizada: la fecha no puede ser anterior a hoy
    public class FechaNoPasadaAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is DateTime fecha)
                return fecha.Date >= DateTime.Today;
            return true; // [Required] se encarga de los valores vacíos
        }
    }
}
