using System;
using System.Threading.Tasks;
using Npgsql;
using NServiceBus;
using Warehouse.Events;

namespace Shipping
{
    public class OrderItemsCollectedHandler : IHandleMessages<OrderItemsCollected>
    {
        public async Task Handle(OrderItemsCollected message, IMessageHandlerContext context)
        {
            using var con = new NpgsqlConnection("Host=localhost;Username=db_user;Password=P@ssw0rd;Database=shipping_database");
            con.Open();

            var sql = $"UPDATE shipments SET ready = 'true' WHERE order_id = '{message.OrderId}'";
            using var cmd = new NpgsqlCommand(sql, con);
            var affectedRecords = await cmd.ExecuteNonQueryAsync();

            var rnd = new Random(DateTime.Now.Millisecond);
            var delay = rnd.Next(1000, 3000);
            await Task.Delay(delay);

            if(affectedRecords == 0)
            {
                throw new Exception("Cannot find any record to update.");
            }

            Console.WriteLine("Shipment details updated");
            
            //probably publish a ShipmentReady event.
        }
    }
}