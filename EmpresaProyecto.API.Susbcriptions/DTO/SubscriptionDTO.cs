
namespace EmpresaProyecto.API.Subscriptions.DTO
{
    public class SubscriptionRequestDTO
    {
        public string IdCliente { get; set; }
        public string Nombre { get; set; }
        public string ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }
        public string Correo { get; set; }
        public string Telefono { get; set; }
        public string Plan { get; set; }
        public DatosPagoDto Tarjeta {  get; set; }

    }
    public class DatosPagoDto
    {
        public string NumeroTarjeta { get; set; } 
        public string Expiracion { get; set; }
        public string Cvv { get; set; } 
    }

    public class SubscriptionResponseDTO : SubscriptionRequestDTO
    {
        public long IdSuscripcion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaPago { get; set; }
        public DateTime UltimaFechaModificacion { get; set; }
        public string Estado { get; set; }
        public string IdCliente { get; set; }
       
    }

}
