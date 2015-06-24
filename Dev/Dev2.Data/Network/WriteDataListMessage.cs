
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Text;
using Dev2.Common;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Extensions;
using Dev2.DataList.Contract.TO;
using Dev2.Server.DataList.Translators;

namespace Dev2.DataList.Contract.Network
{
    public class WriteDataListMessage : DataListMessage
    {
        public Guid DatalistID { get; set; }
        public IBinaryDataList Datalist { get; set; }

        public WriteDataListMessage()
        {
        }

        public WriteDataListMessage(long handle, Guid datalistID, IBinaryDataList datalist, ErrorResultTO errors)
        {
            Handle = handle;
            DatalistID = datalistID;
            Datalist = datalist;
            Errors = errors;
        }

        public override void Read(IByteReaderBase reader)
        {
            IDataListTranslator translator = new DataListTranslatorFactory().FetchTranslator(DataListFormat.CreateFormat(GlobalConstants._BINARY));

            DatalistID = reader.ReadGuid();

            byte[] datalistData = reader.ReadByteArray();
            Datalist = null;
            if(datalistData != null)
            {
                ErrorResultTO tmpErrors;
                Datalist = translator.ConvertTo(datalistData, new StringBuilder(), out tmpErrors);
            }

            Errors = ErrorResultTOExtensionMethods.FromByteArray(reader.ReadByteArray());
        }

        public override void Write(IByteWriterBase writer)
        {
            IDataListTranslator translator = new DataListTranslatorFactory().FetchTranslator(DataListFormat.CreateFormat(GlobalConstants._BINARY));

            writer.Write(DatalistID);

            byte[] datalistData = null;
            if(Datalist != null)
            {
                ErrorResultTO tmpErrors;
                DataListTranslatedPayloadTO dataListTranslatedPayloadTO = translator.ConvertFrom(Datalist, out tmpErrors);
                datalistData = dataListTranslatedPayloadTO.FetchAsByteArray();
            }

            __IByteWriterBaseExtensions.Write(writer, datalistData);
            __IByteWriterBaseExtensions.Write(writer, Errors.ToByteArray());
        }
    }

    public class WriteDataListResultMessage : DataListMessage
    {
        public bool Result { get; set; }

        public WriteDataListResultMessage()
        {
        }

        public WriteDataListResultMessage(long handle, bool result, ErrorResultTO errors)
        {
            Handle = handle;
            Result = result;
            Errors = errors;
        }

        public override void Read(IByteReaderBase reader)
        {
            Result = reader.ReadBoolean();
            Errors = ErrorResultTOExtensionMethods.FromByteArray(reader.ReadByteArray());
        }

        public override void Write(IByteWriterBase writer)
        {
            writer.Write(Result);
            __IByteWriterBaseExtensions.Write(writer, Errors.ToByteArray());
        }
    }
}
