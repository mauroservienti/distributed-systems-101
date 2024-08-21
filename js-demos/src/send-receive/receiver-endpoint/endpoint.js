import rl from "readline";
import amqp from "amqplib";

(async () => {

    const prompts = rl.createInterface(process.stdin, process.stdout);

    const inputQ = "send-receive-receiver-endpoint";
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

    console.log("Receiver started.");

    prompts.question("Press Return to exit.", (key) => {
        channel.close();
        connection.close();
        process.exit(0);
    });
})();