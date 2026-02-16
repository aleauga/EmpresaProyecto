using EmpresaProyecto.Infrastructure.Rest;
using System.Net;

namespace EmpresaProyecto.Tests.Rest
{
    public class PaymentGatewayClientTests
    {
        private class FakeHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpResponseMessage _fakeResponse;

            public FakeHttpMessageHandler(HttpResponseMessage fakeResponse)
            {
                _fakeResponse = fakeResponse;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(_fakeResponse);
            }
        }

        [Fact]
        public async Task ValidatePaymentAsync_ReturnsTrue_WhenStatusIsSuccess()
        {
            // Arrange: simulamos un 200 OK
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.OK);
            var handler = new FakeHttpMessageHandler(fakeResponse);
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://api.example.com")
            };

            var client = new PaymentGatewayClient(httpClient);

            // Act
            var result = await client.ValidatePaymentAsync();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidatePaymentAsync_ReturnsFalse_WhenStatusIsFailure()
        {
            // Arrange: simulamos un 500 Internal Server Error
            var fakeResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);
            var handler = new FakeHttpMessageHandler(fakeResponse);
            var httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://api.example.com")
            };

            var client = new PaymentGatewayClient(httpClient);

            // Act
            var result = await client.ValidatePaymentAsync();

            // Assert
            Assert.False(result);
        }

    }
}
