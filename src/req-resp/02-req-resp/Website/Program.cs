using System;
using RabbitMQ.Client;
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

                var props = channel.CreateBasicProperties();
                props.ReplyTo = "website";
                props.CorrelationId = "order-abc"

                string message = "Hello World!";
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                     routingKey: "sales",
                                     basicProperties: null,
                                     body: body);
                channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));
                
                Console.WriteLine($"Sent {message}");
            }

            Console.WriteLine(" Website endpoint running.");
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}