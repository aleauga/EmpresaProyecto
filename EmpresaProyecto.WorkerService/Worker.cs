
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
            var eventCallback =  async (string message) => {
                if (!string.IsNullOrEmpty(message))
                {
                    using var scope = _scopeFactory.CreateScope();
                    var subscriptionService = scope.ServiceProvider.GetRequiredService<ISubscriptionService>();


                    var subscriptionRequested = JsonSerializer.Deserialize<SubscriptionRequestedEvent>(message);
                    if(subscriptionRequested !=null)
                        await subscriptionService.SubscriptionRequestedHandler(subscriptionRequested);
                }


            };
            await _eventConsumer.InitEventConsumerAsync(eventCallback);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        public override void Dispose()
        {
            _eventConsumer?.Dispose();
            base.Dispose();
        }
    }
    

}
