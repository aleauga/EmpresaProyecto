using EmpresaProyecto.API.Subscriptions.DTO;
using EmpresaProyecto.API.Subscriptions.Helpers;
using EmpresaProyecto.Core.Entities;

namespace EmpresaProyecto.Tests.Helpers
{
    public class MappersTests
    {
        [Fact]
        public void ConverToCliente_ReturnsCliente_WhenDtoIsValid()
        {
            // Arrange
            var dto = new SubscriptionRequestDTO
            {
                Nombre = "Alejandra",
                ApellidoPaterno = "García",
                ApellidoMaterno = "López",
                Correo = "alejandra@example.com",
                Telefono = "5551234567",
                IdCliente = Guid.NewGuid().ToString()
            };

            // Act
            var cliente = Mappers.ConverTo(dto);

            // Assert
            Assert.NotNull(cliente);
            Assert.Equal(dto.Nombre, cliente.Nombre);
            Assert.Equal(dto.ApellidoPaterno, cliente.ApellidoPaterno);
            Assert.Equal(dto.ApellidoMaterno, cliente.ApellidoMaterno);
            Assert.Equal(dto.Correo, cliente.Correo);
            Assert.Equal(dto.Telefono, cliente.Telefono);
            Assert.Equal(dto.IdCliente, cliente.IdCliente);
        }

        [Fact]
        public void ConverToSubscriptionResponse_ReturnsDto_WhenEntitiesAreValid()
        {
            // Arrange
            var cliente = new Cliente
            {
                IdCliente = Guid.NewGuid().ToString(),
                Nombre = "Alejandra",
                ApellidoPaterno = "García",
                ApellidoMaterno = "López",
                Correo = "alejandra@example.com",
                Telefono = "5551234567"
            };

            var suscripcion = new Suscripcion
            {
                IdSuscripcion = 1,
                FechaCreacion = DateTime.UtcNow,
                FechaPago = DateTime.UtcNow.AddDays(1),
                UltimaFechaModificacion = DateTime.UtcNow,
                Estado = "Activa"
            };

            // Act
            var response = Mappers.ConverTo(cliente, suscripcion);

            // Assert
            Assert.NotNull(response);
            Assert.Equal(cliente.IdCliente, response.IdCliente);
            Assert.Equal(cliente.Nombre, response.Nombre);
            Assert.Equal(cliente.ApellidoPaterno, response.ApellidoPaterno);
            Assert.Equal(cliente.ApellidoMaterno, response.ApellidoMaterno);
            Assert.Equal(cliente.Correo, response.Correo);
            Assert.Equal(cliente.Telefono, response.Telefono);
            Assert.Equal(suscripcion.IdSuscripcion, response.IdSuscripcion);
            Assert.Equal(suscripcion.FechaCreacion, response.FechaCreacion);
            Assert.Equal(suscripcion.FechaPago, response.FechaPago);
            Assert.Equal(suscripcion.UltimaFechaModificacion, response.UltimaFechaModificacion);
            Assert.Equal(suscripcion.Estado, response.Estado);
        }

    }
}
