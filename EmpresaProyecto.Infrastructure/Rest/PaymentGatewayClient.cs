using EmpresaProyecto.Core.Rest.Contracts;

namespace EmpresaProyecto.Infrastructure.Rest
{
    public class PaymentGatewayClient : IPaymentGateway
    {
        private readonly HttpClient _httpClient;

        public PaymentGatewayClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> ValidatePaymentAsync()
        {
            var response = await _httpClient.GetAsync("/status/200");

            return response.IsSuccessStatusCode;
        }

    }
}
