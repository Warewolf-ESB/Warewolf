
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
using System.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Enums;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;

namespace Dev2.Server.DataList.Translators
{
    internal sealed class DataListBinaryTranslator : IDataListTranslator
    {
        private readonly DataListFormat _format;
        private readonly Encoding _encoding;

        public DataListFormat Format { get { return _format; } }
        public Encoding TextEncoding { get { return _encoding; } }

        public DataListBinaryTranslator()
        {
            _format = DataListFormat.CreateFormat(GlobalConstants._BINARY);
            _encoding = Encoding.UTF8;
        }

        public DataListFormat HandlesType()
        {
            return _format;
        }

        /// <summary>
        /// Converts from a binary representation in the standard format to the specified <see cref="Format" />.
        /// </summary>
        /// <param name="input">The binary representation of the datalist.</param>
        /// <param name="errors">The errors.</param>
        /// <returns>
        /// An array of bytes that represent the datalist in the specified <see cref="Format" />
        /// </returns>
        /// <exception cref="System.ArgumentNullException">input</exception>
        public DataListTranslatedPayloadTO ConvertFrom(IBinaryDataList input, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            if(input == null)
            {
                errors.AddError("Null input argument");
                throw new ArgumentNullException("input");
            }

            DataListTranslatedPayloadTO resultTO;
            BinaryFormatter bf = new BinaryFormatter();
            using(MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, input);

                resultTO = new DataListTranslatedPayloadTO(ms.ToArray());
            }

            return resultTO;
        }
        /// <summary>
        /// Converts from a binary representation in the specified <see cref="Format" /> to the standard
        /// binary representation of a datalist.
        /// </summary>
        /// <param name="input">The binary representation in the specifeid <see cref="Format" /></param>
        /// <param name="shape"></param>
        /// <param name="errors"></param>
        /// <returns>
        /// An array of bytes that represent the datalist in the standard format.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">input</exception>
        public IBinaryDataList ConvertTo(byte[] input, StringBuilder shape, out ErrorResultTO errors)
        {

            errors = new ErrorResultTO();
            if(input == null)
            {
                errors.AddError("Null input argument");
                throw new ArgumentNullException("input");
            }

            IBinaryDataList result;

            using(MemoryStream ms = new MemoryStream(input))
            {
                BinaryFormatter bf = new BinaryFormatter();
                ms.Position = 0;
                result = (IBinaryDataList)bf.Deserialize(ms);
            }

            return result;
        }

        public IBinaryDataList ConvertTo(object input, StringBuilder shape, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Converts the and only map inputs.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="shape">The shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public IBinaryDataList ConvertAndOnlyMapInputs(byte[] input, StringBuilder shape, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }

        public Guid Populate(object input, Guid targetDl, string outputDefs, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }

        public StringBuilder ConvertAndFilter(IBinaryDataList input, StringBuilder filterShape, out ErrorResultTO errors)
        {
            throw new NotImplementedException();
        }

        public DataTable ConvertToDataTable(IBinaryDataList input, string recsetName, out ErrorResultTO errors, PopulateOptions populateOptions)
        {
            errors = null;
            throw new NotImplementedException();
        }
    }
}
