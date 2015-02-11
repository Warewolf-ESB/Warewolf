namespace Dev2.Common.Interfaces.Studio.ViewModels
{
    public interface IDeployItemMessage
    {
        IExplorerItemViewModel Item { get;  }

        IExplorerItemViewModel SourceServer { get; }

    }
}