using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace Sales
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
                channel.QueueDeclare(queue: "sales",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                channel.ExchangeDeclare(exchange: "order.accepted",
                                        durable: true,
                                        type: "topic");

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var receivedBody = ea.Body.ToArray();
                    var receivedMessage = Encoding.UTF8.GetString(receivedBody);
                    var receivedProps = ea.BasicProperties;
                    Console.WriteLine($"Received {receivedMessage} with correlation ID {receivedProps.CorrelationId}");

                    var replyProps = channel.CreateBasicProperties();
                    replyProps.CorrelationId = receivedProps.CorrelationId;

                    string replyMessage = $"Order {receivedProps.CorrelationId} on its way...";
                    var replyBody = Encoding.UTF8.GetBytes(replyMessage);

                    channel.BasicPublish(exchange: "",
                                     routingKey: receivedProps.ReplyTo,
                                     basicProperties: replyProps,
                                     body: replyBody);
                    channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));
                
                    Console.WriteLine($"Replied {replyMessage}");

                    var eventProps = channel.CreateBasicProperties();
                    eventProps.CorrelationId = receivedProps.CorrelationId;

                    string eventMessage = $"Order {receivedProps.CorrelationId} accepted";
                    var eventBody = Encoding.UTF8.GetBytes(eventMessage);

                    channel.BasicPublish(exchange: "order.accepted",
                                         routingKey: "order.accepted",
                                         basicProperties: eventProps,
                                         body: eventBody);
                    channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));

                    Console.WriteLine($"Published {eventMessage}");
                };
                channel.BasicConsume(queue: "sales",
                                     autoAck: true,
                                     consumer: consumer);

                Console.WriteLine(" Sales endpoint running.");
                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}