namespace EmpresaProyecto.Core.Messaging.Contracts
{
    public interface IEventPublisher
    {
        Task PublishAsync<TEvent>(TEvent message);
    }
}
