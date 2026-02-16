namespace EmpresaProyecto.Core.Messaging.Events
{
    public class SubscriptionRequestedEvent
    {
        public string IdCliente { get; set; }
        public long IdSuscripcion { get; set; }
        public string MetodoPagoEncriptado { get; set; }
        public string NombreEvento { get; } = "SubscriptionRequestedEvent";

    }
}
