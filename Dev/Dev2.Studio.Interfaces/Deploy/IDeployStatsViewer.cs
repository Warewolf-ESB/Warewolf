/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;

namespace Dev2.Studio.Interfaces.Deploy
{
    public class Conflict
    {
        public string SourceName { get; set; }
        public string DestinationName { get; set; }
        public Guid SourceId { get; set; }
        public Guid DestinationId { get; set; }
        
    }
    public interface IDeployStatsViewerViewModel
    {
        int Connectors { get; set; }
        int Services { get; set; }
        int Sources { get; set; }
        int Tests { get; set; }
        int Triggers { get; set; }
        int Unknown { get; set; }
        int NewResources { get; set; }
        int Overrides { get; set; }
        string Status { get; set; }

        void TryCalculate( IList<IExplorerTreeItem> items);

        IList<Conflict> Conflicts { get; }

        IList<IExplorerTreeItem> New { get; }
        Action CalculateAction { get; set; }
        string RenameErrors { get; }

        void ReCalculate();

        void CheckDestinationPermissions();
    }
}