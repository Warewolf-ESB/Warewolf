using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Extensions;
using Dev2.DataList.Contract.TO;
using Dev2.Network.Messages;
using Dev2.Server.DataList.Translators;

namespace Dev2.DataList.Contract.Network
{
    public class ReadDataListMessage : INetworkMessage
    {
        public long Handle { get; set; }
        public Guid DatalistID { get; set; }
        public ErrorResultTO Errors { get; set; }

        public ReadDataListMessage()
        {
        }

        public ReadDataListMessage(long handle, Guid datalistID, ErrorResultTO errors)
        {
            Handle = handle;
            DatalistID = datalistID;
            Errors = errors;
        }

        public void Read(IByteReaderBase reader)
        {
            DatalistID = reader.ReadGuid();
            Errors = ErrorResultTOExtensionMethods.FromByteArray(reader.ReadByteArray());
        }

        public void Write(IByteWriterBase writer)
        {
            writer.Write(DatalistID);
            __IByteWriterBaseExtensions.Write(writer, Errors.ToByteArray());
        }
    }

    public class ReadDataListResultMessage : INetworkMessage
    {
        public long Handle { get; set; }
        public IBinaryDataList Datalist { get; set; }
        public ErrorResultTO Errors { get; set; }

        public ReadDataListResultMessage()
        {
        }

        public ReadDataListResultMessage(long handle, IBinaryDataList datalist, ErrorResultTO errors)
        {
            Handle = handle;
            Datalist = datalist;
            Errors = errors;
        }

        public void Read(IByteReaderBase reader)
        {
            IDataListTranslator translator = DataListTranslatorFactory.FetchBinaryTranslator();
            ErrorResultTO tmpErrors;

            byte[] datalistData = reader.ReadByteArray();
            Datalist = null;
            if (datalistData != null)
            {
                Datalist = translator.ConvertTo(datalistData, "", out tmpErrors);
            }

            Errors = ErrorResultTOExtensionMethods.FromByteArray(reader.ReadByteArray());
        }

        public void Write(IByteWriterBase writer)
        {
            IDataListTranslator translator = DataListTranslatorFactory.FetchBinaryTranslator();
            ErrorResultTO tmpErrors;

            byte[] datalistData = null;
            if (Datalist != null)
            {
                DataListTranslatedPayloadTO dataListTranslatedPayloadTO = translator.ConvertFrom(Datalist, out tmpErrors);
                datalistData = dataListTranslatedPayloadTO.FetchAsByteArray();
            }

            __IByteWriterBaseExtensions.Write(writer, datalistData);
            __IByteWriterBaseExtensions.Write(writer, Errors.ToByteArray());
        }
    }
}
