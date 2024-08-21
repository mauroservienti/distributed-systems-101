import rl from "readline";
import amqp from "amqplib";

(async () => {

    const prompts = rl.createInterface(process.stdin, process.stdout);

    const inputQ = "request-multi-response-receiver-endpoint";
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
            console.log(`Going to reply 4 times at '${replyTo}'.`);

            const replyMsg = 'Hello there, this is the receiver endpoint.';
            for (let index = 0; index < 4; index++) {
                var composedReplyMsg = `${replyMsg} - Index: ${index}`;
                const replyResult = await channel.sendToQueue(replyTo, Buffer.from(composedReplyMsg), {
                    persistent: true,
                    correlationId: msg.properties.correlationId
                });
                console.log(`- [x] Sent reply '${composedReplyMsg}' to '${replyTo}', with correlation ID '${msg.properties.correlationId}' - operation result: '${replyResult}'.`);
                
                await new Promise(resolve => setTimeout(resolve, 1000));
            }

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