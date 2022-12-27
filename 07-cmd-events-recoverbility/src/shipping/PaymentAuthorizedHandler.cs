using System;
using System.Threading.Tasks;
using Billing.Events;
using Npgsql;
using NServiceBus;

namespace Shipping
{
    public class PaymentAuthorizedHandler : IHandleMessages<PaymentAuthorized>
    {
        public async Task Handle(PaymentAuthorized message, IMessageHandlerContext context)
        {
            using var con = new NpgsqlConnection("Host=localhost;Username=db_user;Password=P@ssw0rd;Database=shipping_database");
            con.Open();

            var sql = $"INSERT INTO shipments(order_id, ready) VALUES('{message.OrderId}',false)";
            using var cmd = new NpgsqlCommand(sql, con);
            await cmd.ExecuteNonQueryAsync();

            var rnd = new Random(DateTime.Now.Millisecond);
            var delay = rnd.Next(1000, 3000);
            await Task.Delay(delay);

            Console.WriteLine("Shipment details created");
        }
    }
}