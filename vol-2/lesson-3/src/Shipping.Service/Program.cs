﻿using NServiceBus;
using System;
using System.Threading.Tasks;

namespace Shipping.Service
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceName = typeof(Program).Namespace;
            Console.Title = serviceName!;

            var config = new EndpointConfiguration(serviceName);
            config.ApplyCommonConfigurationWithPersistence(@"Host=localhost;Port=7432;Username=db_user;Password=P@ssw0rd;Database=shipping_database");

            var endpointInstance = await Endpoint.Start(config);

            Console.WriteLine($"{serviceName} started. Press any key to stop.");
            Console.ReadLine();

            await endpointInstance.Stop();
        }
    }
}
