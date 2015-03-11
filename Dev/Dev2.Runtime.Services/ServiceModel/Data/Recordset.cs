
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dev2.Runtime.ServiceModel.Data
{
    public class Recordset
    {
        #region CTOR

        public Recordset()
        {
            Fields = new List<RecordsetField>();
            Records = new List<RecordsetRecord>();
        }

        #endregion

        #region Properties

        public string Name { get; set; }

        public bool HasErrors { get; set; }

        public string ErrorMessage { get; set; }

        public List<RecordsetField> Fields { get; private set; }

        public List<RecordsetRecord> Records { get; private set; }

        #endregion

        #region NewRecord

        /// <summary>
        /// Creates a new record with a label.
        /// </summary>
        /// <returns>A new record instance.</returns>
        public RecordsetRecord NewRecord()
        {
            var name = Name;
            if(!string.IsNullOrEmpty(Name) && Name.Contains("()"))
            {
                name = name.Replace("()", "");
                name = name + "(" + (Records.Count + 1) + ")";
            }
            return new RecordsetRecord
            {
                Label = Name + "(" + (Records.Count + 1) + ")",
                Name = name
            };
        }

        #endregion

        #region SetValue

        /// <summary>
        /// Sets the value of the field at the given record/field index.
        /// <remarks>
        /// A new row is added if <paramref name="recordIndex"/> is greater than or equal to <see cref="Records"/> count.
        /// A new field is added if <paramref name="fieldIndex"/> is greater than or equal to <see cref="Fields"/> count.
        /// </remarks>
        /// </summary>
        /// <param name="recordIndex">The index of the record to be updated.</param>
        /// <param name="fieldIndex">The index of the field to be updated.</param>
        /// <param name="value">The value.</param>
        public void SetValue(int recordIndex, int fieldIndex, string value)
        {
            var record = recordIndex >= Records.Count ? null : Records[recordIndex];
            SetValue(ref record, fieldIndex, value);
        }

        /// <summary>
        /// Sets the value of the field at the given field index in the given row.
        /// <remarks>
        /// A new row is added if <paramref name="record"/> is <code>null</code>.
        /// A new field is added if <paramref name="fieldIndex"/> is greater than or equal to <see cref="Fields"/> count.
        /// </remarks>
        /// </summary>
        /// <param name="record">The record to be updated; may be null.</param>
        /// <param name="fieldIndex">The index of the field to be updated.</param>
        /// <param name="value">The value.</param>
        public void SetValue(ref RecordsetRecord record, int fieldIndex, string value)
        {
            if(record == null)
            {
                record = NewRecord();
                Records.Add(record);
            }

            var cell = new RecordsetCell
            {
                Name = record.Label + "." + Fields[fieldIndex].Alias,
                Label = Fields[fieldIndex].Alias,
                Value = value
            };
            if(fieldIndex >= record.Count)
            {
                record.Add(cell);
            }
            else
            {
                record[fieldIndex] = cell;
            }
        }

        #endregion

        #region ToString

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        #endregion

    }

}
