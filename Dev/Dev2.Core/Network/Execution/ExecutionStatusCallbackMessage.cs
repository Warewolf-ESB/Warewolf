/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;

namespace Dev2.Network.Execution
{
    public enum ExecutionStatusCallbackMessageType
    {
        Unknown = 0,
        Add = 1,
        Remove = 2,
        StartedCallback = 3,
        BookmarkedCallback = 4,
        ResumedCallback = 5,
        CompletedCallback = 6,
        ErrorCallback = 7,
    }

    public class ExecutionStatusCallbackMessage
    {
        #region Constructors

        public ExecutionStatusCallbackMessage()
        {
        }

        public ExecutionStatusCallbackMessage(Guid callbackID, ExecutionStatusCallbackMessageType messageType)
        {
            CallbackID = callbackID;
            MessageType = messageType;
        }

        #endregion Constructors

        #region Properties

        public Guid CallbackID { get; set; }
        public ExecutionStatusCallbackMessageType MessageType { get; set; }

        #endregion Properties

        #region INetworkMessage

        public void Read(IByteReaderBase reader)
        {
            CallbackID = reader.ReadGuid();
            MessageType = (ExecutionStatusCallbackMessageType) reader.ReadInt32();
        }

        public void Write(IByteWriterBase writer)
        {
            writer.Write(CallbackID);
            writer.Write((int) MessageType);
        }

        #endregion INetworkMessage
    }
}