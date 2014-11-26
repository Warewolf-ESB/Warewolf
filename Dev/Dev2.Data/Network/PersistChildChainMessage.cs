
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
    public class PersistChildChainMessage : DataListMessage
    {
        public Guid ID { get; set; }

        public PersistChildChainMessage()
        {
        }

        public PersistChildChainMessage(long handle, Guid id)
        {
            Handle = handle;
            ID = id;
        }

        public override void Read(IByteReaderBase reader)
        {
            ID = reader.ReadGuid();
        }

        public override void Write(IByteWriterBase writer)
        {
            writer.Write(ID);
        }
    }

    public class PersistChildChainResultMessage : DataListMessage
    {
        public bool Result { get; set; }

        public PersistChildChainResultMessage()
        {
        }

        public PersistChildChainResultMessage(long handle, bool result, ErrorResultTO errors)
        {
            Handle = handle;
            Result = result;
            Errors = errors;
        }

        public override void Read(IByteReaderBase reader)
        {
            Result = reader.ReadBoolean();
        }

        public override void Write(IByteWriterBase writer)
        {
            writer.Write(Result);
        }
    }
}
