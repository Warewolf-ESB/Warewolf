using System;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Interfaces.Messages;

namespace Dev2.Studio.Core.Messages
{
    public class RemoveEnvironmentMessage : IMessage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public RemoveEnvironmentMessage(IEnvironmentModel environmentModel, Guid? context)
        {
            EnvironmentModel = environmentModel;
            Context = context;
        }

        public IEnvironmentModel EnvironmentModel { get; set; }
        public Guid? Context { get; set; }
    }
}