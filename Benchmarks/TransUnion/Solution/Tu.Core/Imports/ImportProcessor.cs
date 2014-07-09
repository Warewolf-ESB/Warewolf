using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Tu.Extensions;

namespace Tu.Imports
{
    public class ImportProcessor : IImportProcessor
    {
        public const string IsValidOutputDataColumnName = "IsValid";

        readonly IList<OutputColumn> _outputColumns;
        readonly IReadOnlyDictionary<string, string> _inputOutputColumnMapping;
        readonly OutputColumn _keyColumn;

        public ImportProcessor(IList<OutputColumn> outputColumns, IReadOnlyDictionary<string, string> inputOutputColumnMapping)
        {
            if(outputColumns == null)
            {
                throw new ArgumentNullException("outputColumns");
            }
            if(inputOutputColumnMapping == null)
            {
                throw new ArgumentNullException("inputOutputColumnMapping");
            }
            _outputColumns = outputColumns;
            _inputOutputColumnMapping = inputOutputColumnMapping;
            _keyColumn = outputColumns.FirstOrDefault(c => c.IsKey);

            if(_keyColumn == null)
            {
                throw new ArgumentNullException("outputColumns", "outputColumns does not contain a key column.");
            }

            Errors = new DataTable();
            Errors.Columns.Add(outputColumns.First(c => c.IsKey).Name, typeof(string));
            Errors.Columns.Add("Reason", typeof(string));

            OutputData = new DataTable();
            OutputData.Columns.Add(IsValidOutputDataColumnName, typeof(bool)).DefaultValue = false;

            Columns = new List<DataColumn>();
            foreach(var outputDataColumn in _outputColumns.Select(outputColumn => OutputData.Columns.Add(outputColumn.Name, outputColumn.ColumnType)))
            {
                Columns.Add(outputDataColumn);
            }
        }

        public DataTable OutputData { get; private set; }
        public DataTable Errors { get; private set; }
        public IList<DataColumn> Columns { get; private set; }

        public DataRow[] Run(string csvInput)
        {
            Errors.Clear();
            OutputData.Clear();
            FillOutputData(csvInput);
            foreach(DataRow dataRow in OutputData.Rows)
            {
                var keyValue = dataRow[_keyColumn.Name];
                var isValidRow = true;
                foreach(var outputColumn in _outputColumns)
                {
                    // DO NOT use the following as short-circuit boolean evaluation 
                    // will prevent IsValid from being called on every column!
                    //
                    // isValidRow = isValidRow && outputColumn.IsValid(dataRow);
                    //
                    var isValidColumn = outputColumn.IsValid(dataRow);
                    isValidRow = isValidRow && isValidColumn;

                    foreach(var error in outputColumn.Errors)
                    {
                        Errors.Rows.Add(keyValue, error);
                    }
                }
                dataRow[IsValidOutputDataColumnName] = isValidRow;
            }

            return OutputData.Select(IsValidOutputDataColumnName + "=1");
        }

        void FillOutputData(string csvInput)
        {
            var inputData = csvInput.ToDataTable("|");

            foreach(DataRow inputRow in inputData.Rows)
            {
                var outputRow = OutputData.NewRow();

                foreach(DataColumn inputColumn in inputData.Columns)
                {
                    string outputColumnName;
                    if(_inputOutputColumnMapping.TryGetValue(inputColumn.ColumnName, out outputColumnName))
                    {
                        var outputColumn = _outputColumns.First(c => c.Name == outputColumnName);
                        var outputValue = GetOutputValue(outputColumn, inputRow[inputColumn]);
                        if(outputValue == null)
                        {
                            continue;
                        }

                        var value = outputRow[outputColumnName].ToStringSafe();
                        if(string.IsNullOrWhiteSpace(value))
                        {
                            outputRow[outputColumnName] = outputValue;
                        }
                        else
                        {
                            outputRow[outputColumnName] = value + " " + outputValue;
                        }
                    }
                }

                OutputData.Rows.Add(outputRow);
            }
        }

        static object GetOutputValue(OutputColumn outputColumn, object inputValue)
        {
            var inputValueStr = inputValue.ToStringSafe();

            if(string.IsNullOrEmpty(inputValueStr))
            {
                return null;
            }

            if(outputColumn.ColumnType == typeof(DateTime))
            {
                return string.IsNullOrEmpty(outputColumn.InputFormat)
                    ? Convert.ToDateTime(inputValue)
                    : DateTime.ParseExact(inputValueStr, outputColumn.InputFormat, CultureInfo.InvariantCulture);
            }

            if(outputColumn.ColumnType == typeof(bool))
            {
                int n;
                bool b;
                return int.TryParse(inputValueStr, out n)
                           ? Convert.ToBoolean(n)
                           : (bool.TryParse(inputValueStr, out b) && b);
            }

            if(outputColumn.ColumnType == typeof(int))
            {
                int n;
                return int.TryParse(inputValueStr, out n) ? n : 0;
            }

            try
            {
                return Convert.ChangeType(inputValue, outputColumn.ColumnType);
            }
            catch(Exception ex)
            {
            }

            return null;
        }
    }
}