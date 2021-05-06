# Distributed Systems 101

## Requirements

If your GitHub account is enabled to use [Codespaces](https://github.com/features/codespaces) you can open this repository directly in Codespaces. Otherwise the following requirements must be met in order to successfully run the demos:

- [Visual Studio Code](https://code.visualstudio.com/) and the [Remote container extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote)remote-containers).
- [Docker](https://www.docker.com/get-started) must be pre-installed on the machine.
- The repository `devcontainer` setup requires `docker-compose` to be installed on the machine.

## How to configure Visual Studio Code to run the demos

> The following steps are required if you're not using Codespaces

- Clone the repository
  - On Windows make sure to clone on short path, e.g. `c:\dev`, to avoid any "path too long" error
- Open the root folder of the repository in Visual Studio Code
- Open the Visual Studio Code command palette (`F1` on all supported operating systems, for more information on VS Code keyboard shortcuts refer to [this page](https://www.arungudelli.com/microsoft/visual-studio-code-keyboard-shortcut-cheat-sheet-windows-mac-linux/))
- Type `Reopen in Container`, the command palette supports auto-completion, the command should be available by typing `reop`

Wait for Visual Studio Code Remote container extension to:

- download the required container images
- configure the docker environment
- configure the remote Visual Studio Code instance with the required extensions

> Note: no changes will be made to your Visual Studio Code installation, all changes will be applied to the VS Code instance running in the remote container

The repository `devcontainer` configuration will:

- Create three container instances:
  - A RabbitMQ instance with management plugin support
  - A .NET 5 enabled container where the repository source code will be available
  - A PostgreSQL instance (Used starting Volume 1/Lesson 4)
- Configure the VS Code remote instance with:
  - The C# extension (`ms-dotnettools.csharp`)
  - Bash as the default terminal

Once configuration is completed VS Code will show a new `Ports` tab, in the bottom-docked terminal area. The `Ports` tab will list all the ports exposed by the remote containers.

### Verify that setup completed successfully

Locate Visual Studio Code `Ports` tab. The tab by default shows four columns, `Port`, `Local Address`, `Running Process`, and `Origin`.

- Locate the row, in the `Port` column, containing the value `15672` (the RabbitMQ management port).
- Hover the located row with the mouse and click on the `Open in Browser` icon in the `Local Address` column.
- A new browser tab is opened and the RabbitMQ management page is displayed.

The default RabbitMQ credentials are:

- Username:`guest`
- Password: `guest`
