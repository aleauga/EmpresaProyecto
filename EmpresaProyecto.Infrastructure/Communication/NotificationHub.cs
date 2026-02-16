using Microsoft.AspNetCore.SignalR;

namespace EmpresaProyecto.Infrastructure.Communication
{
    public class NotificationHub : Hub
    {
        public async Task ClientRegister(string userId)
        {
            Console.WriteLine($"Registrando usuario {userId} con ConnectionId {Context.ConnectionId}");

            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
        }


    }
}
