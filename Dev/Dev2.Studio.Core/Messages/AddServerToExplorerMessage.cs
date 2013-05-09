using System;
using Dev2.Studio.Core.Interfaces;

namespace Dev2.Studio.Core.Messages
{
    public class AddServerToExplorerMessage : IMessage
    {
        public AddServerToExplorerMessage(IEnvironmentModel environmentModel, Guid? context)
        {
            EnvironmentModel = environmentModel;
            Context = context;
        }

        public IEnvironmentModel EnvironmentModel { get; set; }
        public Guid? Context { get; set; }    
    }
}