/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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
        string TestsCount { get; set; }
        string TriggersCount { get; set; }
        string ServicesCount { get; set; }
        string OverridesCount { get; set; }
        string NewResourcesCount { get; set; }
    }
}
