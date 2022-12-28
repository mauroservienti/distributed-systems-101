using System;
using System.Threading.Tasks;
using NServiceBus;
using Npgsql;
using NpgsqlTypes;

namespace Finance
{
    class Program
    {
        public static async Task Main()
        {
            var config = new EndpointConfiguration("finance");
            config.EnableInstallers();

            var conventions = config.Conventions();
            conventions.DefiningCommandsAs(t => t.Namespace != null && t.Namespace.EndsWith("Commands"));
            conventions.DefiningEventsAs(t => t.Namespace != null && t.Namespace.EndsWith("Events"));
            conventions.DefiningMessagesAs(t => t.Namespace != null && t.Namespace.EndsWith("Messages"));

            config.UseTransport(
                new RabbitMQTransport(
                    RoutingTopology.Conventional(QueueType.Quorum),
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
                connectionBuilder: () => new NpgsqlConnection("Host=localhost;Port=6432;Username=db_user;Password=P@ssw0rd;Database=finance_database"));

            var endpoint = await Endpoint.Start(config);

            Console.WriteLine(" NServiceBus Finance endpoint running.");
            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();

            await endpoint.Stop();
        }
    }
}