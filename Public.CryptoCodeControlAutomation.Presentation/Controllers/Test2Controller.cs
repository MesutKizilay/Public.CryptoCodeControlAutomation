using System.Net.Sockets;
using System.Text.Json;
using CryptoCodeControlAutomation.Presentation.Services;
using Microsoft.AspNetCore.Mvc;

namespace CryptoCodeControlAutomation.Presentation.Controllers
{
    public class Test2Controller : Controller
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
        private readonly MoxaTcpDemoService _moxaTcpDemoService;

        public Test2Controller(MoxaTcpDemoService moxaTcpDemoService)
        {
            _moxaTcpDemoService = moxaTcpDemoService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Status()
        {
            return Ok(new
            {
                isRunning = _moxaTcpDemoService.IsRunning,
                port = _moxaTcpDemoService.Port
            });
        }

        [HttpPost]
        public async Task<IActionResult> Start([FromBody] StartMoxaListenerRequest request)
        {
            if (request.Port is < 1 or > 65535)
                return BadRequest(new { message = "Port 1 ile 65535 arasında olmalıdır." });

            try
            {
                await _moxaTcpDemoService.StartAsync(request.Port);

                return Ok(new
                {
                    message = $"{request.Port} portu dinleniyor.",
                    port = request.Port
                });
            }
            catch (SocketException exception)
            {
                return BadRequest(new { message = $"Port açılamadı: {exception.Message}" });
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Stop()
        {
            await _moxaTcpDemoService.StopAsync();
            return Ok(new { message = "TCP dinleyici durduruldu." });
        }

        [HttpPost]
        public IActionResult PublishTestMessage([FromBody] PublishMoxaTestMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Value))
                return BadRequest(new { message = "Test verisi boş olamaz." });

            _moxaTcpDemoService.PublishTestMessage(request.Value);
            return Ok();
        }

        [HttpGet]
        public async Task Stream(CancellationToken cancellationToken)
        {
            Response.ContentType = "text/event-stream";
            Response.Headers.CacheControl = "no-cache";
            Response.Headers.Append("X-Accel-Buffering", "no");

            var (subscriptionId, reader) = _moxaTcpDemoService.Subscribe();

            try
            {
                await Response.WriteAsync(": connected\n\n", cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);

                await foreach (var message in reader.ReadAllAsync(cancellationToken))
                {
                    var json = JsonSerializer.Serialize(message, JsonOptions);
                    await Response.WriteAsync($"event: code\ndata: {json}\n\n", cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
            }
            finally
            {
                _moxaTcpDemoService.Unsubscribe(subscriptionId);
            }
        }
    }

    public sealed class StartMoxaListenerRequest
    {
        public int Port { get; set; }
    }

    public sealed class PublishMoxaTestMessageRequest
    {
        public string Value { get; set; } = string.Empty;
    }
}
