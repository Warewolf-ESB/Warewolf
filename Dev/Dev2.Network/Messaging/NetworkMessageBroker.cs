using System;
using System.ComponentModel.Composition;
using System.Network;
using Dev2.Network.Messages;

namespace Dev2.Network.Messaging
{
    [Export(typeof(INetworkMessageBroker))]
    public class NetworkMessageBroker : INetworkMessageBroker
    {
        #region Methods

        /// <summary>
        /// Sends the specified message.
        /// </summary>
        public void Send<T>(T message, INetworkOperator networkOperator) where T : INetworkMessage
        {
            if(message == null)
            {
                throw new InvalidOperationException("Netowrk operator is null.");
            }

            if(networkOperator == null)
            {
                throw new InvalidOperationException("Netowrk operator is null.");
            }

            //
            // Create packet and write data to it
            //
            Packet p = new Packet(PacketTemplates.Both_OnNetworkMessageRevieved);
            p.Write(typeof(T).AssemblyQualifiedName);
            p.Write(message.Handle);
            message.Write(p);

            //
            // Send packet
            //
            networkOperator.Send(p);
        }

        /// <summary>
        /// Processes a recieved message.
        /// </summary>
        public INetworkMessage Recieve(IByteReaderBase reader)
        {
            if(reader == null)
            {
                throw new InvalidOperationException("Reader is null.");
            }

            string fullyQualifiedName = reader.ReadString();
            long handle = reader.ReadInt64();

            Type t = Type.GetType(fullyQualifiedName, false);

            if(t == null)
            {
                throw new InvalidOperationException("Type '" + fullyQualifiedName + "' isn't available, possible version miss match between server and client.");
            }

            if(!typeof(INetworkMessage).IsAssignableFrom(t))
            {
                throw new InvalidOperationException("Message type isn't assignable from '" + typeof(INetworkMessage).AssemblyQualifiedName + "'.");
            }

            INetworkMessage result = (INetworkMessage)Activator.CreateInstance(t);

            if(result != null)
            {
                result.Handle = handle;
                result.Read(reader);
            }

            return result;
        }

        #endregion Methods
    }
}
