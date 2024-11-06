﻿using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Sales;

class Program
{
    public static async Task Main()
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true));
        await channel.QueueDeclareAsync(queue: "sales",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);


        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var receivedBody = ea.Body.ToArray();
            var receivedMessage = Encoding.UTF8.GetString(receivedBody);
            var receivedProps = ea.BasicProperties;
            Console.WriteLine($"Received {receivedMessage} with correlation ID {receivedProps.CorrelationId}");

            var replyProps = new BasicProperties
            {
                CorrelationId = receivedProps.CorrelationId
            };

            var replyMessage = $"Order {receivedProps.CorrelationId} on its way...";
            var replyBody = Encoding.UTF8.GetBytes(replyMessage);

            await channel!.BasicPublishAsync(
                "",
                receivedProps!.ReplyTo!,
                true,
                basicProperties: replyProps,
                body: replyBody);

            Console.WriteLine($"Sent {replyMessage}");

            //suppose storing the order in a database
            //a batch job will check for orders status
            //and use the stored correlation ID to generate
            //more reply messages
            System.Threading.Thread.Sleep(2000);

            var shippedProps = new BasicProperties
            {
                CorrelationId = receivedProps.CorrelationId
            };

            var shippedMessage = $"Order {receivedProps.CorrelationId} shipped";
            var shippedBody = Encoding.UTF8.GetBytes(shippedMessage);

            await channel!.BasicPublishAsync(
                "",
                receivedProps!.ReplyTo!,
                true,
                basicProperties: shippedProps,
                body: shippedBody);

            Console.WriteLine($"Sent {shippedMessage}");
        };
            
        await channel.BasicConsumeAsync(queue: "sales",
            autoAck: true,
            consumer: consumer);

        Console.WriteLine(" Sales endpoint running.");
        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }
}