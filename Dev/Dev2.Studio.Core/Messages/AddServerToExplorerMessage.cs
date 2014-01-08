using System;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class AddServerToExplorerMessage : IMessage
    {
        public AddServerToExplorerMessage(IEnvironmentModel environmentModel, Guid? context, bool forceConnect = false)
        {
            EnvironmentModel = environmentModel;
            Context = context;
            ForceConnect = forceConnect;
        }

        public IEnvironmentModel EnvironmentModel { get; set; }
        public Guid? Context { get; set; }
        public bool ForceConnect { get; set; }
    }
}