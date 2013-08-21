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
            MessageAggregator = ImportService.GetExportValue<IStudioNetworkMessageAggregator>();
            MessageBroker = ImportService.GetExportValue<INetworkMessageBroker>();
        }

        #endregion Constructors

        #region Public Properties

        /// <summary>
        /// Gets the message aggregator. All messages coming in form the server are agreggated through this point.
        /// </summary>
        /// <value>
        /// The message aggregator.
        /// </value>
        public IStudioNetworkMessageAggregator MessageAggregator { get; private set; }

        /// <summary>
        /// Gets the message broker.
        /// </summary>
        /// <value>
        /// The message broker.
        /// </value>
        public INetworkMessageBroker MessageBroker { get; private set; }

        #endregion Public Properties
    }
}
