using System;
using System.Threading.Tasks;
using NServiceBus;
using Npgsql;
using NpgsqlTypes;

namespace Shipping
{
    class Program
    {
        public static async Task Main()
        {
            var config = new EndpointConfiguration("shipping");
            config.EnableInstallers();

            var conventions = config.Conventions();
            conventions.DefiningCommandsAs(t => t.Namespace != null && t.Namespace.EndsWith("Commands"));
            conventions.DefiningEventsAs(t => t.Namespace != null && t.Namespace.EndsWith("Events"));
            conventions.DefiningMessagesAs(t => t.Namespace != null && t.Namespace.EndsWith("Messages"));

            config.UseTransport(
                new RabbitMQTransport(
                    RoutingTopology.Conventional(QueueType.Classic),
                    "host=localhost"
                )
            );
            
            var persistence = config.UsePersistence<SqlPersistence>();
            var dialect = persistence.SqlDialect<SqlDialect.PostgreSql>();
            dialect.JsonBParameterModifier(
                modifier: parameter =>
                {
                    var npgsqlParameter = (NpgsqlParameter)parameter;
                    npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Jsonb;
                });
            persistence.ConnectionBuilder(
                connectionBuilder: () => new NpgsqlConnection("Host=localhost;Username=db_user;Password=P@ssw0rd;Database=shipping_database"));

            var endpoint = await Endpoint.Start(config);

            Console.WriteLine(" NServiceBus Shipping endpoint running.");
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();

            await endpoint.Stop();
        }
    }
}