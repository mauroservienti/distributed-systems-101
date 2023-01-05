# Distributed Systems 101

## Requirements

The following requirements must be met in order to successfully run the demos:

- [Visual Studio Code](https://code.visualstudio.com/) and the [Dev containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers).
- [Docker](https://www.docker.com/get-started) must be pre-installed on the machine.
- The repository `devcontainer` setup requires `docker-compose` to be installed on the machine.

## How to configure Visual Studio Code to run the demos

- Clone the repository
  - On Windows make sure to clone on short path, e.g. `c:\dev`, to avoid any "path too long" error
- Open one of the demos folder in Visual Studio Code
- Make sure Docker is running
  - If you're using Docker for Windows with Hyper-V make sure that the cloned folder, or a parent folder, is mapped in Docker
- Open the Visual Studio Code command palette (`F1` on all supported operating systems, for more information on VS Code keyboard shortcuts refer to [this page](https://www.arungudelli.com/microsoft/visual-studio-code-keyboard-shortcut-cheat-sheet-windows-mac-linux/))
- Type `Reopen in Container`, the command palette supports auto-completion, the command should be available by typing `reop`

Wait for Visual Studio Code Dev containers extension to:

- download the required container images
- configure the docker environment
- configure the remote Visual Studio Code instance with the required extensions

> Note: no changes will be made to your Visual Studio Code installation, all changes will be applied to the VS Code instance running in the remote container

The repository `devcontainer` configuration will:

- One or more container instances:
  - One RabbitMQ instance with management plugin support
  - One .NET 6 enabled container where the repository source code will be mapped
  - Depending on the chosen lesson, a one or more PostgreSQL instances
- Configure the VS Code remote instance with:
  - The C# extension (`ms-dotnettools.csharp`)
  - Bash as the default terminal

Once configuration is completed VS Code will show a new `Ports` tab, in the bottom-docked terminal area. The `Ports` tab will list all the ports exposed by the remote containers.

### Verify that setup completed successfully

Locate Visual Studio Code `Ports` tab. The tab by default shows four columns, `Port`, `Local Address`, `Running Process`, and `Origin`.

- Locate the row, in the `Port` column, containing the value `15672` (the RabbitMQ management port).
- Hover the located row with the mouse and click on the `Open in Browser` icon in the `Local Address` column.
- A new browser tab is opened and the RabbitMQ management page is displayed.

The following section contains connection information details.

## Containers connection information

The default RabbitMQ credentials are:

- Username: `guest`
- Password: `guest`

The deafult PostgreSQL credentials are:

- User: `db_user`
- Password: `P@ssw0rd`

## How to run the demos

Each demo lives in a separate folder. To run a demo open the demo folder in VS Code, press `F1` and search for `Reopen in container`. Wait for the Dev Container to complete the setup process.

Once the demo content has been reopened in the dev container:

1. Press `F1`, search for `Run task`, and execute the desired task to build the solution or to buiold the solution and deploy the required data
2. Go to the `Run and Debug` VS Code section to select the desired compund to execute.
