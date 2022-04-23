# Demo 1 - Pub/Sub

> Note:
>
> - for this demos the order in which endpoints are executed is important
> - folder paths are case sensitive

_Tip: quickly open a folder in a new terminal window by right clicking on a folder in the explorer pane and selecting `Open in Integrated Terminal`._

## Start the Sales endpoint

Using the Visual Studio Code terminal window (if none is present create a new one), navigate from the root of the repository to the `src/volume-01/pub-sub/basic-pub-sub/sales` folder. Build the Sales project using the `dotnet build` command. Once the project is built run it using the `dotnet run` command.

## Start the Billing endpoint

Using a new Visual Studio Code terminal window, navigate from the root of the repository to the `src/volume-01/pub-sub/basic-pub-sub/billing` folder. Build the Billing project using the `dotnet build` command. Once the project is built run it using the `dotnet run` command.

## Start the Website endpoint

Using a new Visual Studio Code terminal window, navigate from the root of the repository to the `src/volume-01/pub-sub/basic-pub-sub/website` folder. Build the Sales project using the `dotnet build` command. Once the project is built run it using the `dotnet run` command.
