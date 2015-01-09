using System;
using Dev2.Common.Interfaces.Deploy;

namespace Warewolf.Studio.ViewModels
{
    public class Conflict : IConflict {
        public Conflict(Guid id, string sourceName, string destinationName)
        {
            DestinationName = destinationName;
            SourceName = sourceName;
            Id = id;
        }

        #region Implementation of IConflict

        public Guid Id { get; private set; }
        public string SourceName { get; private set; }
        public string DestinationName { get; private set; }

        #endregion
    }
}