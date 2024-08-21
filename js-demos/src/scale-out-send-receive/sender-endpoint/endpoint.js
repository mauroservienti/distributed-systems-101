import rl from "readline";
import amqp from "amqplib";

(async () => {

    const prompts = rl.createInterface(process.stdin, process.stdout);

    const destinationQ = "scale-out-send-receive-receiver-endpoint";
    const connection = await amqp.connect("amqp://localhost");
    const channel = await connection.createChannel();

    console.log("Sender started.");

    for (let index = 0; index < 10000; index++) {
        //we assume the destination queue is created by the receiver endpoint
        const msg = 'Hello there, this is the sender endpoint. Index: ' + index;
        const sendResult = await channel.sendToQueue(destinationQ, Buffer.from(msg),{
            persistent: true
        });
        console.log(`- [x] Sent '${msg}' to '${destinationQ}' - operation result: '${sendResult}'.`);
    }

    prompts.question("Press Return to exit.", (key) => {
        channel.close();
        connection.close();
        process.exit(0);
    });
})();