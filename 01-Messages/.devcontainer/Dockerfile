FROM mcr.microsoft.com/vscode/devcontainers/dotnetcore:0-5.0

RUN apt-get update && apt-get -y install git procps

RUN apt-get autoremove -y \
    && apt-get clean -y \
    && rm -rf /var/lib/apt/lists/*