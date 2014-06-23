using System;

// ReSharper disable CheckNamespace
// ReSharper disable InconsistentNaming
namespace Dev2.Studio.Core.Messages
{
    public class SelectItemInDeployMessage : IMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public SelectItemInDeployMessage(Guid resourceID, Guid environmentID)
        {
            EnvironmentID = environmentID;
            ResourceID = resourceID;
        }

        public Guid ResourceID { get; set; }

        public Guid EnvironmentID { get; set; }
    }
}