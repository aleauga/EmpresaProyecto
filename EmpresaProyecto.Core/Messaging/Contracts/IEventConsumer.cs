namespace EmpresaProyecto.Core.Messaging.Contracts
{
    public interface IEventConsumer
    {
        Task InitEventConsumerAsync(Func<string,Task> eventCallback);
        void Dispose();
    }
}
