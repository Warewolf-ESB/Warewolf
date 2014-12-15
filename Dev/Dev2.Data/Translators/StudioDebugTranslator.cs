
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
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.TO;

// ReSharper disable CheckNamespace
namespace Dev2.Server.DataList.Translators
// ReSharper restore CheckNamespace
{
    internal sealed class StudioDebugTranslator : StudioTranslator, IDataListTranslator
    {
        public StudioDebugTranslator()
            : base(DataListFormat.CreateFormat(GlobalConstants._Studio_Debug_XML), Encoding.UTF8)
        {
        }

        public DataListTranslatedPayloadTO ConvertFrom(IBinaryDataList payload, out ErrorResultTO errors)
        {
            if(payload == null)
            {
                throw new ArgumentNullException("payload");
            }

            StringBuilder result = new StringBuilder("<" + RootTag + ">");
            errors = new ErrorResultTO();
            var itemKeys = payload.FetchAllKeys();

            foreach(var key in itemKeys)
            {
                IBinaryDataListEntry entry;
                string error;
                if(payload.TryGetEntry(key, out entry, out error))
                {
                    if(entry.IsRecordset)
                    {
                        DoRecordSetAppending(errors, entry, result);
                    }
                    else
                    {
                        var fName = entry.Namespace;
                        var val = entry.FetchScalar();
                        if(val != null && (entry.ColumnIODirection == enDev2ColumnArgumentDirection.Both || entry.ColumnIODirection == enDev2ColumnArgumentDirection.Input))
                        {
                            DoScalarAppending(result, fName, val);
                        }
                    }
                }

            }

            result.Append("</" + RootTag + ">");

            DataListTranslatedPayloadTO tmp = new DataListTranslatedPayloadTO(result.ToString());

            return tmp;
        }

        /// <summary>
        /// Converts from a binary representation in the specified <see cref="IDataListTranslator.Format" /> to the standard
        /// binary representation of a datalist.
        /// </summary>
        /// <param name="input">The binary representation in the specified <see cref="IDataListTranslator.Format" /></param>
        /// <param name="shape">The shape.</param>
        /// <param name="errors">The errors.</param>
        /// <returns>
        /// An array of bytes that represent the datalist in the standard format.
        /// </returns>
        public IBinaryDataList ConvertTo(byte[] input, StringBuilder shape, out ErrorResultTO errors)
        {
            return base.ConvertTo(input, shape, out errors);
        }

        #region Private Methods

        #endregion
    }
}
