namespace VeterinariaApp.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalServicios { get; set; }
        public int TotalMascotas { get; set; }
        public int TotalUsuarios { get; set; }
        public int TotalCitas { get; set; }
        public int CitasPendientes { get; set; }
        public int Anio { get; set; }
        public int[] CitasPorMes { get; set; } = new int[12];
        public List<int> AniosDisponibles { get; set; } = new();
    }
}
