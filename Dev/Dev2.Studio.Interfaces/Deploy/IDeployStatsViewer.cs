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
        int Unknown { get; set; }
        int NewResources { get; set; }
        int Overrides { get; set; }
        string Status { get; set; }

        void Calculate( IList<IExplorerTreeItem> items);

        IList<Conflict> Conflicts { get; }

        IList<IExplorerTreeItem> New { get; }
        Action CalculateAction { get; set; }
        string RenameErrors { get; }

        void ReCalculate();

        void CheckDestinationPersmisions();
    }
}