
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

namespace Dev2.DataList.Contract.Network
{
    public class DeleteDataListMessage : DataListMessage
    {
        public Guid ID { get; set; }
        public bool OnlyIfNotPersisted { get; set; }

        public DeleteDataListMessage()
        {
        }

        public DeleteDataListMessage(long handle, Guid id, bool onlyIfNotPersisted)
        {
            Handle = handle;
            ID = id;
            OnlyIfNotPersisted = onlyIfNotPersisted;
        }

        public override void Read(IByteReaderBase reader)
        {
            ID = reader.ReadGuid();
            OnlyIfNotPersisted = reader.ReadBoolean();
        }

        public override void Write(IByteWriterBase writer)
        {
            writer.Write(ID);
            writer.Write(OnlyIfNotPersisted);
        }
    }

    public class DeleteDataListResultMessage : DataListMessage
    {
        public DeleteDataListResultMessage()
        {
        }

        public DeleteDataListResultMessage(long handle, ErrorResultTO errors)
        {
            Handle = handle;
            Errors = errors;
        }

        #region Overrides of NetworkMessage

        public override void Read(IByteReaderBase reader)
        {
            //Nothing to do since handle is the only property and it is handled by the broker
        }

        public override void Write(IByteWriterBase writer)
        {
            //Nothing to do since handle is the only property and it is handled by the broker
        }

        #endregion
    }
}
