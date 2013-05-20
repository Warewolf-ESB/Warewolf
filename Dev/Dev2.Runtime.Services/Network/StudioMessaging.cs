using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Network;
using System.Text;
using Dev2.Network;
using Dev2.Network.Messaging;
using Dev2.Network.Messaging.Messages;

namespace Dev2.DynamicServices.Network
{
    /// <summary>
    /// Provides support for sending and revieving messages form the studio
    /// </summary>
    public static class StudioMessaging
    {
        #region Class Members

        private static IServerNetworkMessageAggregator<StudioNetworkSession> _networkMessageAggregator;
        private static NetworkMessageBroker _networkMessageBroker;

        #endregion Class Members

        #region Constructors

        static StudioMessaging()
        {
            _networkMessageAggregator = new ServerNetworkMessageAggregator<StudioNetworkSession>();
            _networkMessageBroker = new NetworkMessageBroker();
        }

        #endregion Constructors

        #region Public Static Properties

        /// <summary>
        /// Gets the message aggregator. All messages coming in form the studio are agreggated through this point.
        /// </summary>
        /// <value>
        /// The message aggregator.
        /// </value>
        public static IServerNetworkMessageAggregator<StudioNetworkSession> MessageAggregator
        {
            get
            {
                return _networkMessageAggregator;
            }
        }

        /// <summary>
        /// Gets the message broker.
        /// </summary>
        /// <value>
        /// The message broker.
        /// </value>
        public static INetworkMessageBroker MessageBroker
        {
            get
            {
                return _networkMessageBroker;
            }
        }

        #endregion Public Static Properties

        #region Public Static Methods



        #endregion Public Static Methods
    }
}
