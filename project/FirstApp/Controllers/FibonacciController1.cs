using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading.Tasks;

namespace FirstApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FibonacciInitController : ControllerBase
    {
        private readonly ILogger<FibonacciInitController> _logger;
        private readonly ConnectionFactory _connectionFactory;

        public FibonacciInitController(ILogger<FibonacciInitController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Инициализация фабрики подключения RabbitMQ
            _connectionFactory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/"
                // Дополнительные настройки по желанию
            };
        }

        [HttpPost("InitializeCalculations/{numberOfCalculations}")]
        public IActionResult InitializeCalculations(int numberOfCalculations)
        {
            try
            {
                Parallel.For(0, numberOfCalculations, (i) =>
                {
                    SendToSecondApp(i);
                });

                _logger.LogInformation("Инициализация асинхронных расчетов успешно выполнена");
                return Ok("Initialization request sent!");
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка при инициализации расчетов: {errorMessage}", ex.Message);
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        private void SendToSecondApp(int number)
        {
            using (var connection = _connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "fibonacci_queue",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(number.ToString());
                channel.BasicPublish(exchange: "",
                                     routingKey: "fibonacci_queue",
                                     basicProperties: null,
                                     body: body);
            }
        }
    }
}


