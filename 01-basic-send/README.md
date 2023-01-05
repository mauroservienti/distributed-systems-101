# Demo 1 - Send a message

> Note:
>
> - for this demos the order in which endpoints are executed is important
> - folder paths are case sensitive

_Tip: quickly open a folder in a new terminal window by right clicking on a folder in the explorer pane and selecting `Open in Integrated Terminal`._

## Start the demo

To start the demo follow the instructions on the root [README file](/README.md#how-to-run-the-demos).

## Exercise: Sales endpoint sends a response message to the website endpoint

Steps to complete the exercise:

- Modify the Website endpoint to create a `website` queue upon startup
- Modify the Sales message received handler to create a response message and publish it to the `sales` queue
- Modify the Website endpoint to create `EventingBasicConsumer` and handle the incoming message by printing to the console the message body

> Note: the exercise solution is in the [Request/Response sample folder.](/02-req-resp).
