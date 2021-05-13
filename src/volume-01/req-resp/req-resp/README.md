# Demo 2 - Request/Response

> Note:
>
> - for this demos the order in which endpoints are executed is important
> - folder paths are case sensitive

_Tip: quickly open a folder in a new terminal window by right clicking on a folder in the explorer pane and selecting `Open in Integrated Terminal`._

## Start the Sales endpoint

Using the Visual Studio Code terminal window (if none is present create a new one), navigate from the root of the repository to the `src/volume-01/req-resp/req-resp/sales` folder. Build the Sales project using the `dotnet build` command. Once the project is built run it using the `dotnet run` command.

## Start the Website endpoint

Using a new Visual Studio Code terminal window, navigate from the root of the repository to the `src/volume-01/req-resp/req-resp/website` folder. Build the Sales project using the `dotnet build` command. Once the project is built run it using the `dotnet run` command.

## Exercise: Sales endpoint sends multiple reply messages to the website endpoint

Steps to complete the exercise:

- Modify the Sales message received handler to create more than one response message, appending the same correlation ID, and publish them to the `sales` queue

> Note: the exercise solution is in the [Request/multi-Response sample folder.](../req-multi-resp)