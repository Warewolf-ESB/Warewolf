#pragma warning disable
using System;
using System.Collections.Generic;
using Dev2.Interfaces;
using System.Activities.Statements;
using Dev2.Common;
using Dev2.Common.State;

namespace Dev2
{
    public interface IDev2Activity
    {
        /// <summary>
        /// UniqueID is the InstanceID and MUST be a guid.
        /// </summary>
        string UniqueID { get; set; }

        enFindMissingType GetFindMissingType();
        IDev2Activity Execute(IDSFDataObject data, int update);
        IEnumerable<IDev2Activity> NextNodes { get; set; }
        Guid ActivityId { get; set; }

        List<string> GetOutputs();
        IEnumerable<IDev2Activity> GetChildrenNodes();
        FlowNode GetFlowNode();
        string GetDisplayName();
        IEnumerable<StateVariable> GetState();
        IEnumerable<IDev2Activity> GetNextNodes();
        List<(string Description, string Key, string SourceUniqueId, string DestinationUniqueId)> ArmConnectors();
        T As<T>() where T : class, IDev2Activity;
    }
}