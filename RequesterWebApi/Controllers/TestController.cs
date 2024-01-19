using Core;
using MassTransit;
using MassTransit.Clients;
using Microsoft.AspNetCore.Mvc;


namespace RequesterWebApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]  
    public class TestController : ControllerBase
    {
        private readonly IRequestClient<TestMessage> _requestClient;
        private readonly ISendEndpointProvider _sendEndpointProvider;
        private readonly IBus _bus;
        public TestController(IRequestClient<TestMessage> requestClient, ISendEndpointProvider sendEndpointProvider, IBus bus)
        {
            _requestClient = requestClient;
            _sendEndpointProvider = sendEndpointProvider;
            _bus = bus;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] TestMessage message)
        {  
            var (response, _) = await _requestClient.GetResponse<TestMessageResponse>(message);
            return Ok(response.Message);
        }

        [HttpPost]
        public async Task<IActionResult> SendDynamicQueue([FromBody] TestMessage message)
        {
            // Gelen mesaja göre kuyruk adını belirle
            var queueName = message.QueueName;

            // Yanıt beklemek için bir RequestClient oluştur
            var requestClient = _bus.CreateRequestClient<TestMessage>(new Uri($"queue:{queueName}"));

            // Mesajı gönder ve yanıtı bekle
            var (response, _) = await requestClient.GetResponse<TestMessageResponse>(message);

            return Ok(response.Message);

        }

    }
}
