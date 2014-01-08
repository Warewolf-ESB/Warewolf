using System;
using Dev2.Studio.Core.Interfaces;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Messages
{
    public class AddServerToDeployMessage
    {
        public IEnvironmentModel Server { get; set; }
        public bool IsSource { get; set; }
        public bool IsDestination { get; set; }
        public Guid? Context { get; set; }

        public AddServerToDeployMessage(IEnvironmentModel server, bool isSource, bool isDestination)
        {
            Server = server;
            IsSource = isSource;
            IsDestination = isDestination;
        }

        public AddServerToDeployMessage(IEnvironmentModel server, Guid? context)
        {
            Server = server;
            Context = context;
        }
    }
}
