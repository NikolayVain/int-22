using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace SecondApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FibonacciController : ControllerBase
    {
        private readonly ILogger<FibonacciController> _logger;
        private readonly ConnectionFactory _connectionFactory;

        public FibonacciController(ILogger<FibonacciController> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _connectionFactory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                VirtualHost = "/"
            };
        }

        [HttpPost("ProcessFibonacci")]
        public IActionResult ProcessFibonacci()
        {
            try
            {
                using (var connection = _connectionFactory.CreateConnection())
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "fibonacci_queue",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        int number = int.Parse(message);

                        // Логика расчета чисел Фибоначчи
                        int result = CalculateFibonacciValue(number);

                        // Отправка результата обратно первому приложению через RabbitMQ
                        SendResultToFirstApp(result);
                    };

                    channel.BasicConsume(queue: "fibonacci_queue",
                                         autoAck: true,
                                         consumer: consumer);

                    _logger.LogInformation("Ожидание расчетов от первого приложения");
                    return Ok("Waiting for calculations from the first app!");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка при обработке чисел Фибоначчи: {errorMessage}", ex.Message);
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        private void SendResultToFirstApp(int result)
        {
            using (var connection = _connectionFactory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "fibonacci_result_queue",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var body = Encoding.UTF8.GetBytes(result.ToString());
                channel.BasicPublish(exchange: "",
                                     routingKey: "fibonacci_result_queue",
                                     basicProperties: null,
                                     body: body);
            }
        }

        private int CalculateFibonacciValue(int n)
        {
            if (n <= 1)
                return n;

            int a = 0, b = 1, result = 0;
            for (int i = 2; i <= n; i++)
            {
                result = a + b;
                a = b;
                b = result;
            }
            return result;
        }
    }
}




