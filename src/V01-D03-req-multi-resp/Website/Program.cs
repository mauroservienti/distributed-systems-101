using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace Website
{
    class Program
    {
        public static void Main()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ConfirmSelect();

                channel.QueueDeclare(queue: "website",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var receivedBody = ea.Body.ToArray();
                    var receivedMessage = Encoding.UTF8.GetString(receivedBody);
                    Console.WriteLine($"Received {receivedMessage} with correlation ID {ea.BasicProperties.CorrelationId}");
                };

                channel.BasicConsume(queue: "website",
                                     autoAck: true,
                                     consumer: consumer);   

                var props = channel.CreateBasicProperties();
                props.ReplyTo = "website";
                props.CorrelationId = "order-abc";

                string message = "Hello World!";
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                     routingKey: "sales",
                                     basicProperties: props,
                                     body: body);
                channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));
                
                Console.WriteLine($"Sent {message}");

                Console.WriteLine(" Website endpoint running.");
                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}