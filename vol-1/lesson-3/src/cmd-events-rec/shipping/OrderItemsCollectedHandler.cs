using System.Threading.Tasks;
using Npgsql;
using NServiceBus;
using Warehouse.Events;

namespace Shipping
{
    public class OrderItemsCollectedHandler : IHandleMessages<OrderItemsCollected>
    {
        public Task Handle(OrderItemsCollected message, IMessageHandlerContext context)
        {
            using var con = new NpgsqlConnection("Host=localhost;Username=db_user;Password=P@ssw0rd;Database=shipping_database");
            con.Open();

            var sql = $"UPDATE shipments SET ready = 'true' WHERE order_id = '{message.OrderId}'";
            using var cmd = new NpgsqlCommand(sql, con);
            return cmd.ExecuteNonQueryAsync();
            
            //probably publish a ShipmentReady event.
        }
    }
}