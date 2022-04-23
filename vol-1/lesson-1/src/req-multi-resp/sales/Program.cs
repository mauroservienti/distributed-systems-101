﻿using RabbitMQ.Client;
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
                
                    Console.WriteLine($"Sent {replyMessage}");

                    //suppose storing the order in a databse
                    //a batch job will check for orders status
                    //and use the stored correlation ID to generate
                    //more reply messages
                    System.Threading.Thread.Sleep(2000);

                    var shippedProps = channel.CreateBasicProperties();
                    shippedProps.CorrelationId = receivedProps.CorrelationId;

                    string shippedMessage = $"Order {receivedProps.CorrelationId} shipped";
                    var shippedBody = Encoding.UTF8.GetBytes(shippedMessage);

                    channel.BasicPublish(exchange: "",
                                     routingKey: receivedProps.ReplyTo,
                                     basicProperties: replyProps,
                                     body: shippedBody);
                    channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5));
                
                    Console.WriteLine($"Sent {shippedMessage}");
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
