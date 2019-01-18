/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Data.Interfaces;

namespace Dev2.DataList.Contract
{
    public class DataListVerifyPart : IDataListVerifyPart
    {

        public string DisplayValue { get; private set; }

        public string Recordset { get; private set; }

        public string Field { get; private set; }

        public string Description { get; set; }

        public string RecordsetIndex { get; private set; }

        public bool HasRecordsetIndex => !string.IsNullOrEmpty(RecordsetIndex);

        public bool IsScalar => Recordset != null && Recordset.Length == 0;

        public bool IsJson
        {
            get;set;
        }

        internal DataListVerifyPart(string displayName)
        {
            DisplayValue = displayName;
        }
        internal DataListVerifyPart(string recordset, string field) : this(recordset, field, string.Empty, string.Empty) { }

        internal DataListVerifyPart(string recordset, string field, bool useRaw) : this(recordset, field, string.Empty, string.Empty, useRaw) { }

        internal DataListVerifyPart(string recordset, string field, string description) : this(recordset, field, description, string.Empty) { }


        internal DataListVerifyPart(string recordset, string field, string description, string index, bool useRaw = false)
        {
            Recordset = recordset;
            RecordsetIndex = index;

            if (recordset != null && recordset.Contains("[") && recordset.Contains("]"))
            {
                var start = recordset.IndexOf("(", StringComparison.Ordinal);
                Recordset = start != -1 ? recordset.Substring(0, start) : recordset.Replace("[", "").Replace("]", "");
            }
            
            Field = field;
            Description = description;
            
            if(useRaw)
            {
                DisplayValue = field.Length > 0 ? "[[" + recordset + field + "]]" : "[[" + field + "]]";
            }
            else
            {
                if(string.IsNullOrEmpty(Recordset))
                {
                    Recordset = string.Empty;
                    DisplayValue = field.Contains("(") && !field.Contains(")") ? "[[" + field : "[[" + field + "]]";
                }
                else
                {
                    if(field.Length > 0)
                    {
                        DisplayRecordsetWithField(recordset, field);
                    }
                    else
                    {
                        DisplayRecordsetOnly(recordset);
                    }
                }
            }
        }

        private void DisplayRecordsetOnly(string recordset)
        {
            if (recordset != null && recordset.Contains("(") && recordset.Contains(")"))
            {
                var tmp = recordset.Substring(0, recordset.IndexOf("(", StringComparison.Ordinal));
                DisplayValue = "[[" + tmp + "(" + RecordsetIndex + ")]]";
            }
            else
            {
                DisplayValue = "[[" + Recordset + "(" + RecordsetIndex + ")]]";
            }
        }

        private void DisplayRecordsetWithField(string recordset, string field)
        {
            if (recordset != null && recordset.Contains("(") && recordset.Contains(")"))
            {
                var tmp = recordset.Substring(0, recordset.IndexOf("(", StringComparison.Ordinal));

                DisplayValue = "[[" + tmp + "(" + RecordsetIndex + ")." + field + "]]";
            }
            else
            {
                DisplayValue = "[[" + Recordset + "(" + RecordsetIndex + ")." + field + "]]";
            }
        }
    }
}
