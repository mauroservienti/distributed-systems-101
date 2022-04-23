using System.Threading.Tasks;
using Billing.Events;
using Npgsql;
using NServiceBus;

namespace Shipping
{
    public class PaymentAuthorizedHandler : IHandleMessages<PaymentAuthorized>
    {
        public Task Handle(PaymentAuthorized message, IMessageHandlerContext context)
        {
            using var con = new NpgsqlConnection("Host=localhost;Username=db_user;Password=P@ssw0rd;Database=shipping_database");
            con.Open();

            var sql = $"INSERT INTO shipments(order_id, ready) VALUES('{message.OrderId}',false)";
            using var cmd = new NpgsqlCommand(sql, con);
            return cmd.ExecuteNonQueryAsync();
        }
    }
}