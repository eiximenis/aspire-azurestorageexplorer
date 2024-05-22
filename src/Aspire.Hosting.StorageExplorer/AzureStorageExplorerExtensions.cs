using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;
using Aspire.Hosting.Lifecycle;
using Aspire.Hosting.StorageExplorer;

namespace Aspire.Hosting;
public static class AzureStorageExplorerExtensions
{
    public static IResourceBuilder<T> WithAzureStorageExplorer<T>(
        this IResourceBuilder<T> builder, string name = "storageexplorer", string? tag = null)
        where T: AzureStorageResource
    {

        var previousCount = builder.ApplicationBuilder.Resources.OfType<AzureStorageExplorerResource>().Count();

        builder.ApplicationBuilder.Services.TryAddLifecycleHook<SetEnvironmentVariablesForAzureStorageExplorer>();

        var parent = builder.Resource;
        var resource = new AzureStorageExplorerResource($"{name}-{previousCount}", parent);

        var azureStorageBuilder = builder.ApplicationBuilder
            .AddResource(resource)
            .WithImage(AzureStorageExplorerImageTags.Image)
            .WithImageTag(tag ?? AzureStorageExplorerImageTags.Tag)
            .WithHttpEndpoint(targetPort: 8080, name: "http");

        return builder;

    }

}

internal class SetEnvironmentVariablesForAzureStorageExplorer : IDistributedApplicationLifecycleHook
{
    public async Task AfterEndpointsAllocatedAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken = default)
    {
        var admins = appModel.Resources.OfType<AzureStorageExplorerResource>().ToArray();

        

        foreach (var admin in admins)
        {
            var storage = admin.Parent;
            if (storage.IsEmulator)
            {

                var constrBuilder = new StorageConnectionStringBuilder("");
                var blobs = appModel.Resources.OfType<AzureBlobStorageResource>().Where(b => b.Parent == storage);
                var tables = appModel.Resources.OfType<AzureTableStorageResource>().Where(t => t.Parent == storage);

                foreach (var blob in blobs)
                {
                    var blobConstr = await blob.ConnectionStringExpression.GetValueAsync(CancellationToken.None);
                    constrBuilder.MergeConnectionString(blobConstr.Replace("127.0.0.1", "host.docker.internal"));       // TODO: How to avoid hardcoding?
                    ;
                }

                foreach (var table in tables)
                {
                    var tableConstr = await table.ConnectionStringExpression.GetValueAsync(CancellationToken.None);
                    constrBuilder.MergeConnectionString(tableConstr.Replace("127.0.0.1", "host.docker.internal"));      // TODO: How to avoid hardcoding?
                }

            }

            // TODO: What to do if not emulator?
        }
    }

}


public enum ConstrKeyMergeType
{
    CurrentWins,
    OtherWins,
    Error
}

public class StorageConnectionStringBuilder
{

    private Dictionary<string, string> _parts = new();

    public StorageConnectionStringBuilder(string constr = "")
    {
        _parts = ParseConnectionString(constr);
    }

    public void MergeConnectionString(string other, ConstrKeyMergeType mergeType = ConstrKeyMergeType.CurrentWins)
    {
        var otherConstr = ParseConnectionString(other);
        foreach (var kvp in otherConstr)
        {
            var (key, value) = kvp;
            if (!_parts.ContainsKey(kvp.Key))
            {
                _parts[key] = value;
            }
            else
            {
                switch (mergeType)
                {
                    case ConstrKeyMergeType.CurrentWins:
                        break;
                    case ConstrKeyMergeType.OtherWins:
                        _parts[key] = value;
                        break;
                    case ConstrKeyMergeType.Error:
                        default:
                        throw new Exception("Invalid merge type");
                }
            }
        }
    }

    public override string ToString() => string.Join(';', _parts.Select(kv => $"{kv.Key}={kv.Value}"));
    

    private static Dictionary<string, string> ParseConnectionString(string constr)
    {
        if (string.IsNullOrWhiteSpace(constr))
        {
            return new Dictionary<string, string>();
        }

        var data = new Dictionary<string, string>();
        var entries = constr.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var entry in entries)
        {
            var equalSignIdx = entry.IndexOf('=');
            if (equalSignIdx == -1)
            {
                throw new Exception("Invalid connection string");
            }
            data[entry[..equalSignIdx]] = entry[(equalSignIdx + 1)..];
        }

        return data;

    }
}


static class AzureStorageExplorerImageTags
{
    internal const string Registry = "docker.io";

    internal const string Image = "sebagomez/azurestorageexplorer";

    internal const string Tag = "2.16.1";
}