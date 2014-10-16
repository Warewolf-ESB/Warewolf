
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
using System.Text;
using Dev2.Common;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Extensions;
using Dev2.DataList.Contract.TO;
using Dev2.Server.DataList.Translators;

namespace Dev2.DataList.Contract.Network
{
    public class ReadDataListMessage : DataListMessage
    {
        public Guid DatalistID { get; set; }

        public ReadDataListMessage()
        {
        }

        public ReadDataListMessage(long handle, Guid datalistID, ErrorResultTO errors)
        {
            Handle = handle;
            DatalistID = datalistID;
            Errors = errors;
        }

        public override void Read(IByteReaderBase reader)
        {
            DatalistID = reader.ReadGuid();
            Errors = ErrorResultTOExtensionMethods.FromByteArray(reader.ReadByteArray());
        }

        public override void Write(IByteWriterBase writer)
        {
            writer.Write(DatalistID);
            __IByteWriterBaseExtensions.Write(writer, Errors.ToByteArray());
        }
    }

    public class ReadDataListResultMessage : DataListMessage
    {
        public IBinaryDataList Datalist { get; set; }

        public ReadDataListResultMessage()
        {
        }

        public ReadDataListResultMessage(long handle, IBinaryDataList datalist, ErrorResultTO errors)
        {
            Handle = handle;
            Datalist = datalist;
            Errors = errors;
        }

        public override void Read(IByteReaderBase reader)
        {
            IDataListTranslator translator = new DataListTranslatorFactory().FetchTranslator(DataListFormat.CreateFormat(GlobalConstants._BINARY));
            ErrorResultTO tmpErrors;

            byte[] datalistData = reader.ReadByteArray();
            Datalist = null;
            if(datalistData != null)
            {
                Datalist = translator.ConvertTo(datalistData, new StringBuilder(), out tmpErrors);
            }

            Errors = ErrorResultTOExtensionMethods.FromByteArray(reader.ReadByteArray());
        }

        public override void Write(IByteWriterBase writer)
        {
            IDataListTranslator translator = new DataListTranslatorFactory().FetchTranslator(DataListFormat.CreateFormat(GlobalConstants._BINARY));
            ErrorResultTO tmpErrors;

            byte[] datalistData = null;
            if(Datalist != null)
            {
                DataListTranslatedPayloadTO dataListTranslatedPayloadTO = translator.ConvertFrom(Datalist, out tmpErrors);
                datalistData = dataListTranslatedPayloadTO.FetchAsByteArray();
            }

            __IByteWriterBaseExtensions.Write(writer, datalistData);
            __IByteWriterBaseExtensions.Write(writer, Errors.ToByteArray());
        }
    }
}
