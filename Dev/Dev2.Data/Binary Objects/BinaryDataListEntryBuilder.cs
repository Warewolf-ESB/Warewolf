
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
using System.Collections.Generic;
using Dev2.Common;
using Dev2.Common.Interfaces.DataList.Contract;

namespace Dev2.DataList.Contract.Binary_Objects
{
    [Serializable]
    public class BinaryDataListEntryBuilder
    {
        #region Fields

        const string RecsetName = "Results";
        const string FieldName = "Field";

        #endregion Fields

        #region Methods

        public static ActivityUpsertTO CreateEntriesFromOutputTOs(IList<OutputTO> outputToList, IDataListCompiler compiler, Guid dlId, out ErrorResultTO errors)
        {

            errors = new ErrorResultTO();
            ActivityUpsertTO result = new ActivityUpsertTO();

            foreach(OutputTO outputTo in outputToList)
            {

                // I first need to detect if the entry is a recordset!!!!!!!!!!!
                // Then if scalar upsert scalar else upsert a recordset-- how was this to ever work?!?!

                // Break into parts so we can correctly create the required entry......

                IBinaryDataListEntry entry = Dev2BinaryDataListFactory.CreateEntry(RecsetName, string.Empty, dlId);

                int idx = 1;
                foreach(string output in outputTo.OutputStrings)
                {
                    IBinaryDataListItem itemToAdd = Dev2BinaryDataListFactory.CreateBinaryItem(output, RecsetName, FieldName, idx);
                    idx++;
                    string error;
                    entry.TryAppendRecordItem(itemToAdd, out error);
                    if(error != string.Empty)
                    {
                        errors.AddError(error);
                    }
                }
                // push entry one time, no looping ;)  
                result.AddEntry(entry, outputTo.OutPutDescription);
            }

            return result;
        }

        public static IBinaryDataListEntry CreateScalarEntry(string valueForScalar, Guid dlId, out string error)
        {
            IBinaryDataListEntry entry = Dev2BinaryDataListFactory.CreateEntry(GlobalConstants.NullEntryNamespace, string.Empty, dlId);
            IBinaryDataListItem item = Dev2BinaryDataListFactory.CreateBinaryItem(valueForScalar, string.Empty);
            entry.TryPutScalar(item, out error);
            return entry;
        }


        #endregion Methods

    }
}
