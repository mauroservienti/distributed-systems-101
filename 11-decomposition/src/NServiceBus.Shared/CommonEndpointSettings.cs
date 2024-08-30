using Npgsql;
using NpgsqlTypes;

namespace NServiceBus
{
    public static class CommonEndpointSettings
    {
        public static void ApplyCommonConfiguration(this EndpointConfiguration config) 
        {
            config.EnableInstallers();

            config.AuditProcessedMessagesTo("audit");
            config.SendFailedMessagesTo("error");

            config.UseSerialization<SystemJsonSerializer>();
            config.UseTransport(
                new RabbitMQTransport(
                    RoutingTopology.Conventional(QueueType.Classic), "host=localhost"));

            var messageConventions = config.Conventions();
            messageConventions.DefiningMessagesAs(t => t.Namespace != null && t.Namespace.EndsWith(".Messages"));
            messageConventions.DefiningEventsAs(t => t.Namespace != null && t.Namespace.EndsWith(".Messages.Events"));
            messageConventions.DefiningCommandsAs(t => t.Namespace != null && t.Namespace.EndsWith(".Messages.Commands"));
        }

        public static void ApplyCommonConfigurationWithPersistence(this EndpointConfiguration config, string sqlPersistenceConnectionString)
        {
            ApplyCommonConfiguration(config);

            var persistence = config.UsePersistence<SqlPersistence>();
            
            var dialect = persistence.SqlDialect<SqlDialect.PostgreSql>();
            dialect.JsonBParameterModifier(
                modifier: parameter =>
                {
                    var npgsqlParameter = (NpgsqlParameter)parameter;
                    npgsqlParameter.NpgsqlDbType = NpgsqlDbType.Jsonb;
                });
            persistence.ConnectionBuilder(
                connectionBuilder: () => new NpgsqlConnection(sqlPersistenceConnectionString));

            config.EnableOutbox();
        }
    }
}
