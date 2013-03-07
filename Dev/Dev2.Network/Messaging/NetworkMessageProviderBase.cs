using System;
using Dev2.Network.Messages;
using Dev2.Network.Messaging.Messages;

namespace Dev2.Network.Messaging
{
    /// <summary>
    /// A network message provider.
    /// </summary>
    /// <author>Trevor.Williams-Ros</author>
    /// <date>2013/03/07</date>
    public abstract class NetworkMessageProviderBase<TNetworkMessage>
        where TNetworkMessage : INetworkMessage
    {
        /// <summary>
        /// Gets the subscription token used by the aggregator.
        /// </summary>
        public Guid SubscriptionToken { get; private set; }

        /// <summary>
        /// Processes the the given message.
        /// </summary>
        /// <param name="request">The request to be processed.</param>
        /// <returns>The result of processing the message.</returns>
        public abstract TNetworkMessage ProcessMessage(TNetworkMessage request);

        #region Start

        /// <summary>
        /// Starts subscribing to the given aggregator.
        /// <remarks>
        /// The aggregator callback invokes <see cref="ProcessMessage"/> and then <paramref name="messageBroker"/>.Send() with the result.
        /// </remarks>
        /// </summary>
        /// <typeparam name="TContext">The type of the network context.</typeparam>
        /// <param name="aggregator">The aggregator to be used for subscribing.</param>
        /// <param name="messageBroker">The message broker to be used for sending.</param>
        /// <exception cref="System.ArgumentNullException">aggregator</exception>
        /// <exception cref="System.ArgumentNullException">messageBroker</exception>
        public void Start<TContext>(IServerNetworkMessageAggregator<TContext> aggregator, INetworkMessageBroker messageBroker)
            where TContext : NetworkContext, new()
        {
            if(aggregator == null)
            {
                throw new ArgumentNullException("aggregator");
            }
            if(messageBroker == null)
            {
                throw new ArgumentNullException("messageBroker");
            }
            SubscriptionToken = aggregator.Subscribe((TNetworkMessage request, NetworkContext context) =>
            {
                try
                {
                    var result = ProcessMessage(request);
                    result.Handle = request.Handle;
                    messageBroker.Send(result, context);
                }
                catch(Exception ex)
                {
                    var error = new ErrorMessage(request.Handle, ex.Message);
                    messageBroker.Send(error, context);
                }
            });
        }

        #endregion

        #region Stop

        /// <summary>
        /// Stops subscribing from the specified aggregator.
        /// </summary>
        /// <typeparam name="TContext">The type of the network context.</typeparam>
        /// <param name="aggregator">The aggregator to be unsubscribed from.</param>
        /// <exception cref="System.ArgumentNullException">aggregator</exception>
        public void Stop<TContext>(IServerNetworkMessageAggregator<TContext> aggregator)
            where TContext : NetworkContext, new()
        {
            if(aggregator == null)
            {
                throw new ArgumentNullException("aggregator");
            }

            aggregator.Unsubscibe(SubscriptionToken);
        }

        #endregion
    }
}