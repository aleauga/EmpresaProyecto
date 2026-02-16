using EmpresaProyecto.API.Subscriptions.DTO;
using EmpresaProyecto.Core.Entities;

namespace EmpresaProyecto.API.Subscriptions.Helpers
{
    public static class Mappers
    {
        public static Cliente ConverTo(SubscriptionRequestDTO dto)
        {
            try
            {
                return new Cliente
                {
                    Nombre = dto.Nombre,
                    ApellidoPaterno = dto.ApellidoPaterno,
                    ApellidoMaterno = dto.ApellidoMaterno,
                    Correo = dto.Correo,
                    Telefono = dto.Telefono,
                    IdCliente = dto.IdCliente
                };

            }
            catch (Exception)
            {
                throw;
            }
        }
        public static SubscriptionResponseDTO ConverTo(Cliente c, Suscripcion s)
        {
            try
            {
                return new SubscriptionResponseDTO
                {
                    IdCliente = c.IdCliente,
                    Nombre = c.Nombre,
                    ApellidoPaterno = c.ApellidoPaterno,
                    ApellidoMaterno = c.ApellidoMaterno,
                    Correo = c.Correo,
                    Telefono = c.Telefono,
                    IdSuscripcion = s.IdSuscripcion,
                    FechaCreacion = s.FechaCreacion,
                    FechaPago = s.FechaPago,
                    UltimaFechaModificacion = s.UltimaFechaModificacion,
                    Estado = s.Estado,
                    Plan = s.Plan

                };

            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
