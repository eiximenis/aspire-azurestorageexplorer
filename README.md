# aspire-azurestorageexplorer

Extension to dotnet aspire to add the azure storage explorer

Current status: early alpha xD

## How to use

Reference this project from your aspire _AppHost_ project and use the `WithStorageExplorer` extension method when adding a Azure Storage resource:

```csharp
var storage = builder.AddAzureStorage("Storage").WithAzureStorageExplorer("storageexplorer");
var storage2 = builder.AddAzureStorage("Storage2").WithAzureStorageExplorer("storageexplorer");
var blobs = storage.AddBlobs("Myblobs");
var tables = storage.AddTables("MyTables");
var blobs2 = storage2.AddBlobs("MyBlobs2");
```

Each `WithAzureStorageExplorer` call will add a container resource running [Azure Storage Explorer](https://github.com/sebagomez/azurestorageexplorer) from [Sebastián Gómez](https://github.com/sebagomez). Each storage explorer will be configured with the correct connection string to connect all the services associated to the specified storage.

## Current status and limitations

This project is in early alpha, just started playing with Aspire.



**Note**: This project only works in emulation scenarios (storage resource must run in emulator using `RunAsEmulator`).

## How to collaborate

Feel free to test it, submit a PR or open an issue!

## Kudos to

1. Sebastián Gómez for the azure storage explorer
2. Dotnet aspire team

