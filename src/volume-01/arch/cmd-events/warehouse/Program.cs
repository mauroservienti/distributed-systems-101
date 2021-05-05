using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Warehouse
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
                channel.QueueDeclare(queue: "warehouse",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                channel.ExchangeDeclare(exchange: "order.items.collected",
                                        durable: true,
                                        type: "topic");

                channel.QueueBind(queue: "warehouse",
                                  exchange: "order.accepted",
                                  routingKey: "order.accepted");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var receivedBody = ea.Body.ToArray();
                    var receivedMessage = Encoding.UTF8.GetString(receivedBody);
                    var receivedProps = ea.BasicProperties;
                    Console.WriteLine($"Received {receivedMessage} with correlation ID {receivedProps.CorrelationId}");
                    Console.WriteLine($"Collecting items for order {receivedProps.CorrelationId}");

                    var eventProps = channel.CreateBasicProperties();
                    eventProps.CorrelationId = receivedProps.CorrelationId;

                    string eventMessage = $"Items for Order {receivedProps.CorrelationId} collected";
                    var eventBody = Encoding.UTF8.GetBytes(eventMessage);

                    channel.BasicPublish(exchange: "order.items.collected",
                                         routingKey: "order.items.collected",
                                         basicProperties: eventProps,
                                         body: eventBody);
                    channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));

                    Console.WriteLine($"Published {eventMessage}");
                };
                channel.BasicConsume(queue: "warehouse",
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Warehouse endpoint running.");
                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}