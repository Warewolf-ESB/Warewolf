using System.ComponentModel.Composition;
using Dev2.Composition;
using Dev2.Network;

namespace Dev2.Studio.Core.Network
{
    /// <summary>
    /// Provides support for sending and revieving messages form the server
    /// </summary>
    public class ServerMessaging
    {
        #region Constructors

        public ServerMessaging()
        {
            ImportService.SatisfyImports(this);
        }

        #endregion Constructors

        #region Public Properties

        /// <summary>
        /// Gets the message aggregator. All messages coming in form the server are agreggated through this point.
        /// </summary>
        /// <value>
        /// The message aggregator.
        /// </value>
        [Import]
        public IStudioNetworkMessageAggregator MessageAggregator { get; set; }

        /// <summary>
        /// Gets the message broker.
        /// </summary>
        /// <value>
        /// The message broker.
        /// </value>
        [Import]
        public INetworkMessageBroker MessageBroker { get; set; }

        #endregion Public Properties
    }
}
