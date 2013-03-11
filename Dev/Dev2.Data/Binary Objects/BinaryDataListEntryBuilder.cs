using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;

namespace Dev2.DataList.Contract.Binary_Objects
{
    [Serializable]
    public class BinaryDataListEntryBuilder
    {
        #region Fields

        private static readonly string recsetName = "Results";
        private static readonly string fieldName = "Field";

        #endregion Fields

        #region Methods

        //public static ActivityUpsertTO CreateEntriesFromOutputTOs(OutputTO outputTO, IDataListCompiler compiler, IDev2DataLanguageParser parser, Guid dlID, out ErrorResultTO errors)
        //{
        //    IList<OutputTO> listOfOutputs = new List<OutputTO>() { outputTO };
        //    errors = new ErrorResultTO();

        //    ActivityUpsertTO result = CreateEntriesFromOutputTOs(listOfOutputs, compiler, dlID, out errors);

        //    return result;
        //}

        public static ActivityUpsertTO CreateEntriesFromOutputTOs(IList<OutputTO> outputTOList, IDataListCompiler compiler, Guid dlID, out ErrorResultTO errors)
        {

            errors = new ErrorResultTO();
            ActivityUpsertTO result = new ActivityUpsertTO();

            string error = string.Empty;

            foreach (OutputTO outputTO in outputTOList)
            {

                // I first need to detect if the entry is a recordset!!!!!!!!!!!
                // Then if scalar upsert scalar else upsert a recordset-- how was this to ever work?!?!

                // Break into parts so we can correctly create the required entry......

                IBinaryDataListEntry entry = Dev2BinaryDataListFactory.CreateEntry(recsetName, string.Empty, dlID);

                int idx = 1;
                foreach (string output in outputTO.OutputStrings)
                {
                    IBinaryDataListItem itemToAdd = Dev2BinaryDataListFactory.CreateBinaryItem(output, recsetName, fieldName, idx);
                    idx++;
                    entry.TryAppendRecordItem(itemToAdd, out error);
                    if (error != string.Empty)
                    {
                        errors.AddError(error);
                    }
                }
                // push entry one time, no looping ;)  
                result.AddEntry(entry, outputTO.OutPutDescription);
            }

            return result;
        }

        public static IBinaryDataListEntry CreateScalarEntry(string valueForScalar, Guid dlID, out string error)
        {
            IBinaryDataListEntry entry = Dev2BinaryDataListFactory.CreateEntry(GlobalConstants.NullEntryNamespace, string.Empty, dlID);
            IBinaryDataListItem item = Dev2BinaryDataListFactory.CreateBinaryItem(valueForScalar, string.Empty);
            entry.TryPutScalar(item, out error);
            return entry;
        }


        #endregion Methods

    }
}
