
using EmpresaProyecto.Infrastructure.Communication;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace EmpresaProyecto.Tests.Communication
{
    public class NotificationHubTests
    {
        [Fact]
        public async Task ClientRegister_AddsConnectionToGroup()
        {
            // Arrange
            var userId = "user-123";
            var connectionId = "conn-456";

            // Mock del contexto del Hub
            var mockContext = new Mock<HubCallerContext>();
            mockContext.Setup(c => c.ConnectionId).Returns(connectionId);

            // Mock del GroupManager
            var mockGroups = new Mock<IGroupManager>();
            mockGroups.Setup(g => g.AddToGroupAsync(connectionId, userId, default))
                      .Returns(Task.CompletedTask)
                      .Verifiable();

            var hub = new NotificationHub
            {
                Context = mockContext.Object,
                Groups = mockGroups.Object
            };

            // Act
            await hub.ClientRegister(userId);

            // Assert
            mockGroups.Verify(g => g.AddToGroupAsync(connectionId, userId, default), Times.Once);
        }

    }
}
