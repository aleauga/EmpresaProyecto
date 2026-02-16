namespace EmpresaProyecto.Core.Rest.Contracts
{
    public interface IPaymentGateway
    {
        Task<bool> ValidatePaymentAsync();

    }
}
