using System.Windows.Input;

namespace Dev2.Studio.Interfaces.Deploy
{
    public interface IDeployViewModel
    {
        /// <summary>
        /// Source Server
        /// </summary>
        IDeploySourceExplorerViewModel Source { get; set; }
        /// <summary>
        /// Destination Server
        /// </summary>
        IDeployDestinationExplorerViewModel Destination { get; set; }
        /// <summary>
        /// Deploy Button Clicked
        /// Must bring up conflict screen. Conflict screen can modify collection
        /// refresh explorer
        /// </summary>
        ICommand DeployCommand { get; set; }
        /// <summary>
        /// Select All Dependencies. Recursive Select
        /// </summary>
        ICommand SelectDependenciesCommand { get; set; }
        /// <summary>
        /// Stats area shows:
        ///     Service count
        ///     Workflow Count
        ///     Source Count
        ///     Unknown
        ///     New Resources in Destination
        ///     Overridden resource in Destination
        ///     Static steps of how to deploy
        /// </summary>
        IDeployStatsViewerViewModel StatsViewModel { get;  }
        string ErrorMessage { get; set; }
        string DeploySuccessMessage { get; set; }

        string SourcesCount { get; set; }
        string ServicesCount { get; set; }
        string OverridesCount { get; set; }
        string NewResourcesCount { get; set; }
    }
}
