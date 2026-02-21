using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Website;

public class WebsiteEndpoint(IChannel channel)
{
    public async Task SendOrderAsync(string message)
    {
        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: "sales",
            mandatory: true,
            body: Encoding.UTF8.GetBytes(message));
    }
}
