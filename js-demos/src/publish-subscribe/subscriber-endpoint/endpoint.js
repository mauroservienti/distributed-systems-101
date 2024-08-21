import rl from "readline";
import amqp from "amqplib";

(async () => {

    const prompts = rl.createInterface(process.stdin, process.stdout);

    const inputQ = "publish-subscribe-subscriber-endpoint";
    const exchange = "publish-subscribe-something-happened";
    const routingKey = "publish-subscribe-something-happened";

    const connection = await amqp.connect("amqp://localhost");
    const channel = await connection.createChannel();

    await channel.assertQueue(inputQ, {
        durable: true
    });
    console.log(`- [x] '${inputQ}' input queue created.`);
    
    await channel.assertExchange(exchange, "topic", {
        persistent: true
    });
    console.log(`- [x] '${exchange}' exchange created.`);
    
    await channel.bindQueue(inputQ, exchange, routingKey);
    console.log(`- [x] '${inputQ}' to '${exchange}' binding created.`);

    channel.prefetch(10);

    //consume messages as they come in
    channel.consume(inputQ, (async (msg) => {
        if (msg !== null) {
            console.log(`- [x] Received '${msg.content.toString()}'.`);
            channel.ack(msg);
        } else {
            console.log('Consumer cancelled by server');
        }
    }));

    console.log("Subscriber started.");

    prompts.question("Press Return to exit.", (key) => {
        channel.close();
        connection.close();
        process.exit(0);
    });
})();