
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
using Dev2.Common.Interfaces.DataList.Contract;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.DataList
{
    /// <summary>
    /// Abstract class that check the validity of the input arguments
    /// </summary>
    public abstract class AbstractRecsetSearchValidation : IFindRecsetOptions
    {
        /// <summary>
        /// Checks the validity of the input argument and returns the fields in a list of strings
        /// </summary>
        /// <param name="to">To.</param>
        /// <param name="bdl">The BDL.</param>
        /// <param name="errors">The errors.</param>
        /// <returns></returns>
        public Func<IList<RecordSetSearchPayload>> GenerateInputRange(IRecsetSearch to, IBinaryDataList bdl, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            ErrorResultTO allErrors = new ErrorResultTO();

            Func<IList<RecordSetSearchPayload>> result = () =>
            {
                IList<RecordSetSearchPayload> fieldList = new List<RecordSetSearchPayload>();
                string InputField = to.FieldsToSearch;
                string recSet = DataListUtil.ExtractRecordsetNameFromValue(DataListUtil.StripLeadingAndTrailingBracketsFromValue(InputField));
                IBinaryDataListEntry bdle;
                string error;

                bdl.TryGetEntry(recSet, out bdle, out error);
                allErrors.AddError(error);

                if(bdle == null)
                {
                    throw new RecordsetNotFoundException("Could not find Recordset [ " + recSet + " ]");
                }

                IList<Dev2Column> realCols = bdle.Columns;
                string[] tmpCols = InputField.Replace(" ", "").Split(',');

                // Travis.Frisinger : 09.25.2012
                // we need to adjust the tmpCols to avoid * causing crap with the match

                int loc = 0;

                foreach(string tc in tmpCols)
                {
                    string recset = DataListUtil.ExtractRecordsetNameFromValue(tc);
                    string field = DataListUtil.ExtractFieldNameFromValue(tc);
                    string myNewSearch = DataListUtil.AddBracketsToValueIfNotExist(DataListUtil.MakeValueIntoHighLevelRecordset(recset));

                    if(field != string.Empty)
                    {
                        myNewSearch = DataListUtil.MakeValueIntoHighLevelRecordset(recset) + "." + field;
                    }

                    tmpCols[loc] = DataListUtil.AddBracketsToValueIfNotExist(myNewSearch);
                    loc++;
                }

                int pos = 0;
                bool found = true;
                int start;
                Int32.TryParse(to.StartIndex, out start);

                if(start == 0)
                {
                    start = 1;
                }

                while(pos < tmpCols.Length && found)
                {
                    int innerPos;
                    if(IsMatch(tmpCols[pos], recSet, realCols, out innerPos))
                    {

                        for(int i = start; i <= bdle.FetchLastRecordsetIndex(); i++)
                        {
                            IBinaryDataListItem tmp = bdle.TryFetchRecordsetColumnAtIndex(realCols[innerPos].ColumnName, i, out error);
                            if(error != string.Empty)
                            {
                                allErrors.AddError(error);
                            }
                            RecordSetSearchPayload p = new RecordSetSearchPayload { Index = i, Payload = tmp.TheValue };
                            fieldList.Add(p);
                        }
                    }
                    else
                    {
                        if(IsRecorsetWithoutField(tmpCols[pos], recSet))
                        {

                            IIndexIterator ixItr = bdle.FetchRecordsetIndexes();
                            while(ixItr.HasMore())
                            {
                                int next = ixItr.FetchNextIndex();
                                foreach(Dev2Column col in realCols)
                                {

                                    IBinaryDataListItem tmp = bdle.TryFetchRecordsetColumnAtIndex(col.ColumnName, next, out error);
                                    RecordSetSearchPayload p = new RecordSetSearchPayload { Index = next, Payload = tmp.TheValue };
                                    fieldList.Add(p);
                                }
                            }
                        }
                        else
                        {
                            found = false;
                        }
                    }
                    pos++;
                }

                if(!found)
                {
                    fieldList.Clear();
                }

                return fieldList;
            };


            return result;
        }

        /// <summary>
        /// Determines whether the specified field is match.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="recSet">The rec set.</param>
        /// <param name="defs">The defs.</param>
        /// <param name="foundPos">The found pos.</param>
        /// <returns>
        ///   <c>true</c> if the specified field is match; otherwise, <c>false</c>.
        /// </returns>
        private bool IsMatch(string field, string recSet, IList<Dev2Column> defs, out int foundPos)
        {
            bool found = false;
            foundPos = 0;
            int pos = 0;
            while(pos < defs.Count && !found)
            {
                string payload = String.Concat("[[", recSet, "().", defs[pos].ColumnName, "]]");
                if(field == payload)
                {
                    found = true;
                    foundPos = pos;
                }
                pos++;
            }

            return found;
        }

        /// <summary>
        /// Determines whether [is recorset without field] [the specified field].
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="recSet">The rec set.</param>
        /// <returns>
        ///   <c>true</c> if [is recorset without field] [the specified field]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsRecorsetWithoutField(string field, string recSet)
        {
            bool result = false;
            if(string.IsNullOrWhiteSpace(DataListUtil.ExtractFieldNameFromValue(field)))
            {
                if(string.IsNullOrWhiteSpace(DataListUtil.ExtractIndexRegionFromRecordset(field)))
                {
                    string recsetName = DataListUtil.ExtractRecordsetNameFromValue(field);
                    if(recsetName == recSet)
                    {
                        result = true;
                    }
                }
            }

            return result;
        }

        public abstract Func<IList<string>> BuildSearchExpression(IBinaryDataList scopingObj, IRecsetSearch to);

        public abstract string HandlesType();
    }
}
