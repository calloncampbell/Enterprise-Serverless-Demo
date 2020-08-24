# Enterprise Serverless Demo

## Getting Started

Download and install the necessary requirements, and then open the solution and run the project. The Azure Functions runtime will load up on your machine and is the same runtime that runs in Azure.

You will use Azure Storage Explorer to interact with your local Storage (blob and queues) along with a Cosmos DB emulator, so make sure you install those components.

### Requirements

You will need to have an active Azure account. If you don't already have an Azure account, [free accounts are available](https://azure.microsoft.com/free/) or use a sandbox from [Microsoft Learn](https://docs.microsoft.com/en-us/learn/).

- Azure subcription
- Visual Studio 2019 with the **Azure development** workload which can be accessed from the Visual Studio Installer
- Visual Studio Code (optional if not using VS2019)
- Azure CLI
- Azure Functions runtime/SDK - v3
- Azure Cosmos DB Emulator
- Azure Storage Explorer
- .NET Core 3.1

## Tooling

### Azure Functions

Take a look at the [Azure Functions developer guide](https://docs.microsoft.com/en-us/azure/azure-functions/functions-reference) to familiarize yourself with this environment.

Learn how to [develop Azure Functions using Visual Studio](https://docs.microsoft.com/en-us/azure/azure-functions/functions-develop-vs).

### Azure Storage Explorer

The Azure Storage Explorer allows you to work with local emulator resources for Storage and Cosmos DB. It also allows you to connect to an Azure Subscription and work with those resources for Storage and Cosmos DB.

You can download and install the Azure Storage Explorer from [here](https://azure.microsoft.com/features/storage-explorer/).

How to [work with data using Azure Storage Explorer](https://docs.microsoft.com/en-us/azure/cosmos-db/storage-explorer).

### Cosmos DB Emulator

You can download and install the Azure Cosmos Emulator from the [Microsoft Download Center](https://aka.ms/cosmosdb-emulator).

How to [use the Azure Cosmos Emulator for local development and testing](https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator).
