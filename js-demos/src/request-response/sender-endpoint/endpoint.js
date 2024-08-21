import rl from "readline";
import amqp from "amqplib";

(async () => {

    const prompts = rl.createInterface(process.stdin, process.stdout);

    const inputQ = "request-response-sender-endpoint";
    const destinationQ = "request-response-receiver-endpoint";
    const connection = await amqp.connect("amqp://localhost");
    const channel = await connection.createChannel();

    await channel.assertQueue(inputQ, {
        durable: true
    });

    console.log(`- [x] '${inputQ}' input queue created.`);

    channel.prefetch(10);

    //consume messages as they come in
    channel.consume(inputQ, (msg) => {
        if (msg !== null) {
            console.log(`- [x] Received '${msg.content.toString()}'.`);
            channel.ack(msg);
        } else {
            console.log('Consumer cancelled by server');
        }
    });

    console.log("Sender started.");

    //we assume the destination queue is created by the receiver endpoint
    const msg = 'Hello there, this is the sender endpoint.';
    const sendResult = await channel.sendToQueue(destinationQ, Buffer.from(msg),{
        persistent: true,
        replyTo: inputQ
    });

    console.log(`- [x] Sent '${msg}' to '${destinationQ}' - operation result: '${sendResult}'.`);

    prompts.question("Press Return to exit.", (key) => {
        channel.close();
        connection.close();
        process.exit(0);
    });
})();