using EmpresaProyecto.Core.Messaging.Contracts; 
using EmpresaProyecto.Core.Messaging.Events;    
using EmpresaProyecto.WorkerService.Services.Contracts; 
using System.Text.Json;                         

namespace EmpresaProyecto.WorkerService
{
    public class Worker(IEventConsumer _eventConsumer, IServiceScopeFactory _scopeFactory) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Callback que se ejecutará cada vez que llegue un mensaje desde RabbitMQ
            var eventCallback = async (string message) => {
                if (!string.IsNullOrEmpty(message)) 
                {
                    // Crea un nuevo scope de dependencias (para obtener servicios con ciclo de vida Scoped)
                    using var scope = _scopeFactory.CreateScope();

                    // Obtiene el servicio de suscripciones desde el contenedor de dependencias
                    var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();

                    var subscriptionRequested = JsonSerializer.Deserialize<SubscriptionRequestedEvent>(message);

                    if (subscriptionRequested != null)
                        await subscriptionService.SubscriptionRequestedHandler(subscriptionRequested);
                }
            };

            // Inicializa el consumidor de eventos con el callback definido
            await _eventConsumer.InitEventConsumerAsync(eventCallback);

            // Mantiene el servicio en ejecución indefinidamente hasta que se cancele
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }

        public override void Dispose()
        {
            _eventConsumer?.Dispose(); 
            base.Dispose();           
        }
    }
}