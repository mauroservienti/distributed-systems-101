import rl from "readline";
import amqp from "amqplib";

(async () => {

    const prompts = rl.createInterface(process.stdin, process.stdout);

    const inputQ = "request-response-receiver-endpoint";
    const connection = await amqp.connect("amqp://localhost");
    const channel = await connection.createChannel();

    await channel.assertQueue(inputQ, {
        durable: true
    });

    console.log(`- [x] '${inputQ}' input queue created.`);

    channel.prefetch(10);

    //consume messages as they come in
    channel.consume(inputQ, (async (msg) => {
        if (msg !== null) {
            console.log(`- [x] Received '${msg.content.toString()}'.`);

            var replyTo = msg.properties.replyTo;
            console.log(`Going to reply at '${replyTo}'.`);

            const replyMsg = 'Hello there, this is the receiver endpoint.';
            const replyResult = await channel.sendToQueue(replyTo, Buffer.from(replyMsg), {
                persistent: true
            });

            console.log(`- [x] Sent reply '${replyMsg}' to '${replyTo}' - operation result: '${replyResult}'.`);

            channel.ack(msg);
        } else {
            console.log('Consumer cancelled by server');
        }
    }));

    console.log("Receiver started.");

    prompts.question("Press Return to exit.", (key) => {
        channel.close();
        connection.close();
        process.exit(0);
    });
})();