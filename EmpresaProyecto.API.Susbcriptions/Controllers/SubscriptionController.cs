using EmpresaProyecto.API.Subscriptions.DTO;
using EmpresaProyecto.API.Subscriptions.Services.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace EmpresaProyecto.API.Subscriptions.Controllers
{
    [Route("api/client")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _service;
        public SubscriptionController(ISubscriptionService service)
        {
            _service = service;
        }

        [HttpPost]
        [Route("subscription")]
        [Produces("application/json")]
        public async Task<IActionResult> CreateSubscription([FromBody]SubscriptionRequestDTO requestDTO)
        {
            try
            {
                if (requestDTO == null)
                    return BadRequest("Datos inválidos");

                
                await _service.CreateSubscription(requestDTO);
                return Accepted();

            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpGet]
        [Route("{clientId}/subscription")]
        [Produces("application/json")]
        public async Task<IActionResult> GetCLientSubscription(string clientId)
        {
            try
            {
                return Ok(await _service.GetCLientSubscription(clientId) );

            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
