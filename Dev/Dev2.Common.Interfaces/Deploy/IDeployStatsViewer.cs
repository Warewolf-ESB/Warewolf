using System;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces.Deploy
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
        /// <summary>
        /// Services being deployed
        /// </summary>
        int Connectors { get; set; }
        /// <summary>
        /// Services Being Deployed
        /// </summary>
        int Services { get; set; }
        /// <summary>
        /// Sources being Deployed
        /// </summary>
        int Sources { get; set; }
        /// <summary>
        /// What is unknown is unknown to me
        /// </summary>
        int Unknown { get; set; }
        /// <summary>
        /// The count of new resources being deployed
        /// </summary>
        int NewResources { get; set; }
        /// <summary>
        /// The count of overidded resources
        /// </summary>
        int Overrides { get; set; }
        /// <summary>
        /// The status of the last deploy
        /// </summary>
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