# Distributed Systems 101

## Requirements

The following requirements must be met in order to successfully run the demos:

- [Visual Studio Code](https://code.visualstudio.com/) and the [Remote container extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers).
- [Docker](https://www.docker.com/get-started) must be pre-installed on the machine.
- The repository `devcontainer` setup requires `docker-compose` to be installed on the machine.

## How to configure Visual Studio Code to run the demos

> The following steps are required if you're not using Codespaces

- Clone the repository
  - On Windows make sure to clone on short path, e.g. `c:\dev`, to avoid any "path too long" error
- Open one of the `volume-*` folders in Visual Studio Code
- Make sure Docker is running
  - If you're using Docker for Windows with Hyper-V make sure that the cloned folder, or a parent folder, is mapped in Docker
- Open the Visual Studio Code command palette (`F1` on all supported operating systems, for more information on VS Code keyboard shortcuts refer to [this page](https://www.arungudelli.com/microsoft/visual-studio-code-keyboard-shortcut-cheat-sheet-windows-mac-linux/))
- Type `Reopen in Container`, the command palette supports auto-completion, the command should be available by typing `reop`

Wait for Visual Studio Code Remote container extension to:

- download the required container images
- configure the docker environment
- configure the remote Visual Studio Code instance with the required extensions

> Note: no changes will be made to your Visual Studio Code installation, all changes will be applied to the VS Code instance running in the remote container

The repository `devcontainer` configuration will:

- Create three container instances:
  - One RabbitMQ instance with management plugin support
  - One .NET 5 enabled container where the repository source code will be available
  - Two PostgreSQL instances (Used starting Volume 1/Lesson 4)
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

It is possible to connect to the PostgreSQL instances using the configured PostgreSQL extension, use the following paramters to configure connections to the Shipping and the Finance databases:

- Shipping database
  - Host: `localhost`
  - Port: `5432`
  - User: `db_user`
  - Password: `P@ssw0rd`
- Finance database
  - Host: `localhost`
  - Port: `6432`
  - User: `db_user`
  - Password: `P@ssw0rd`

## Volume 1 - Messaging principles

Volume 1 is composed by 4 lessons focused on messaging and their nuances.

### Lesson 1 - Request/Response

Lesson 1 is focused on basic messaging concepts, and why we need messaging in complex business software solutions. Samples are meant to demonstrate request/response patterns using C# and RabbitMQ. Lesson 1 is composed by the following samples:

1. [Send a message](src/volume-01/req-resp/basic-send)
2. [Request/Response](src/volume-01/req-resp/req-resp)
3. [Request/multi-Response](src/volume-01/req-resp/req-multi-resp)

### Lesson 2 - Publish/Subscribe

Lesson 2 builds on top of lesson 1, evolving the messaging concepts into publish/subscribe using events broadcasting. Samples are meant to demonstrate publish/subscribe patterns using C# and RabbitMQ. Lesson 2 is composed by the following samples:

1. [Publish/Subscribe](src/volume-01/pub-sub/basic-pub-sub)
2. [Publish/multi-Subscribe](src/volume-01/pub-sub/pub-multi-sub)

### Lesson 3 - Architectural concepts: commands and events

Lesson 3 evolves lesson 2 by introducing architectural concepts like Commands and Events. It also introduces recoverability concepts. Samples are built using C#, NServiceBus, PostgreSQL, and RabbitMQ. Lesson 3 is composed by the following samples:

1. [Commands and Pub/Sub using NServiceBus](src/volume-01/arch/cmd-events)
2. [Recoverability](src/volume-01/arch/cmd-events-rec)

### Lesson 4 - Introduction to sagas

Lesson 4 introduces choreography and long running business transactions concepts through the saga pattern. Samples are built using C#, NServiceBus, PostgreSQL, and RabbitMQ. Lesson 4 is composed by the following samples:

1. [Basic saga](src/volume-01/sagas/basic-saga)
2. [Delayed deliveries and timeouts](src/volume-01/sagas/timeouts)

## Volume 2 - Service Oriented Architecture

### Lesson 1 & 2 - data/behavior modelling and ViewModel Composition

### Lesson 3 - Writing data - data de-composition

### Lesson 4 - Long running business transactions (sagas)
