using Aspire.Hosting.ApplicationModel;
using Aspire.Hosting.Azure;

namespace Aspire.Hosting.StorageExplorer;

public class AzureStorageExplorerResource : ContainerResource, IResourceWithParent<AzureStorageResource>
{
    public AzureStorageExplorerResource(string name, AzureStorageResource parent) : base(name)
    {
        Parent = parent;
    }

    public AzureStorageResource Parent { get; }
}
