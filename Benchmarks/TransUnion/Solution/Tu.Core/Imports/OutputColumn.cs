using System;
using System.Collections.Generic;
using System.Data;
using Tu.Rules;

namespace Tu.Imports
{
    public delegate ValidationResult IsValidFunc(string ruleName, string fieldName, object fieldValue);

    public class OutputColumn
    {
        readonly string _ruleName;
        readonly IsValidFunc _isValid;

        public OutputColumn(string columnName, Type columnType, string ruleName = null, IsValidFunc isValid = null)
        {
            if(string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentNullException("columnName");
            }
            if(columnType == null)
            {
                throw new ArgumentNullException("columnType");
            }
            Errors = new List<string>();
            Name = columnName;
            ColumnType = columnType;
            _ruleName = ruleName;
            _isValid = isValid;
        }

        public bool IsKey { get; set; }
        public string InputFormat { get; set; }
        public string Name { get; private set; }
        public Type ColumnType { get; private set; }
        public List<string> Errors { get; private set; }

        public bool IsValid(DataRow row)
        {
            Errors.Clear();
            if(row == null)
            {
                throw new ArgumentNullException("row");
            }

            if(string.IsNullOrEmpty(_ruleName) || _isValid == null)
            {
                return true;
            }

            var value = row[Name];
            if(Convert.IsDBNull(value))
            {
                value = null;
            }

            var result = _isValid(_ruleName, Name, value);
            if(!result.IsValid)
            {
                Errors.AddRange(result.Errors);
            }
            return result.IsValid;
        }
    }
}
