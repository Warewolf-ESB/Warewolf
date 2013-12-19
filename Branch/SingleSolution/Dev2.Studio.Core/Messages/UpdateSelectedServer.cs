using System;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Messages;

namespace Dev2.Messages
{
    public class UpdateSelectedServer:IMessage
    {
        public IEnvironmentModel EnvironmentModel { get; set; }
        public bool IsSourceServer { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public UpdateSelectedServer(IEnvironmentModel environmentModel,bool isSourceServer)
        {
            if(environmentModel == null)
            {
                throw new ArgumentNullException("environmentModel");
            }
            EnvironmentModel = environmentModel;
            IsSourceServer = isSourceServer;
        }
    }
}