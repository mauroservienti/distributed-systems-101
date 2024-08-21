import rl from "readline";
import amqp from "amqplib";
import { safeCheckExchange } from "./safeCheckExchange.js";

(async () => {

    const prompts = rl.createInterface(process.stdin, process.stdout);

    const exchange = "publish-subscribe-something-happened";
    const routingKey = "publish-subscribe-something-happened";
    const connection = await amqp.connect("amqp://localhost");
    const channel = await connection.createChannel();

    console.log("Publisher started.");

    const exchangeExist = await safeCheckExchange(exchange);
    // if the exchange doesn't exist it means that no one is subscribed, yet.
    // There is no point in publishing. 
    if(exchangeExist){
        const msg = 'Hey, did you know that something happened?';
        channel.publish(
            exchange,
            routingKey,
            Buffer.from(msg),
            {
                persistent: true
            });

        console.log(`- [x] Published '${msg}' to the '${exchange}' exchange.`);
    }else{
        console.log(`- [x] No subscribers for the '${routingKey}' event.`);
    }

    prompts.question("Press Return to exit.", (key) => {
        channel.close();
        connection.close();
        process.exit(0);
    });
})();