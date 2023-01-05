# Demo 2 - Request/Response

> Note:
>
> - for this demos the order in which endpoints are executed is important
> - folder paths are case sensitive

## How to start the demo

To start the demo follow the instructions on the root [README file](/README.md#how-to-run-the-demos).

## Exercise: Sales endpoint sends multiple reply messages to the website endpoint

Steps to complete the exercise:

- Modify the Sales message received handler to create more than one response message, appending the same correlation ID, and publish them to the `sales` queue

> Note: the exercise solution is in the [Request/multi-Response sample folder.](../req-multi-resp)