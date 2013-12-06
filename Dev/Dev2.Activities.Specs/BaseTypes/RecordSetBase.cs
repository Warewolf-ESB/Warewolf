using System;
using System.Collections.Generic;
using System.Text;
using ActivityUnitTests;
using Dev2.DataList.Contract;

namespace Dev2.Activities.Specs.BaseTypes
{
    public abstract class RecordSetBases : BaseActivityUnitTest
    {
        protected const string ResultVariable = "[[result]]";
        private readonly List<string> _addedFieldset = new List<string>();
        private readonly List<string> _addedRecordsets = new List<string>();
        protected readonly dynamic _variableList;
        protected string FieldName = "";
        protected string RecordSetName = "";
        protected string Recordset;
        protected IDSFDataObject _result;

        protected RecordSetBases(dynamic variableList)
        {
            _variableList = variableList;
        }

        protected void BuildShapeAndTestData(Tuple<string, string> variable)
        {
            _variableList.Add(variable);
            BuildShapeAndTestData();
        }

        protected void BuildShapeAndTestData(Tuple<string, string, string> variable)
        {
            _variableList.Add(variable);
            BuildShapeAndTestData();
        }

        protected void BuildShapeAndTestData(Tuple<string, string, string, string> variable)
        {
            _variableList.Add(variable);
            BuildShapeAndTestData();
        }

        protected void BuildShapeAndTestData()
        {
            var shape = new StringBuilder();
            shape.Append("<root>");

            var data = new StringBuilder();
            data.Append("<root>");

            int row = 1;
            foreach (dynamic variable in _variableList)
            {
                Build(variable, shape, data);
                row++;
            }
            shape.Append("</root>");
            data.Append("</root>");

            CurrentDl = shape.ToString();
            TestData = data.ToString();
        }

        private void Build(dynamic variable, StringBuilder shape, StringBuilder data)
        {
            if (DataListUtil.IsValueRecordset(variable.Item1))
            {
                dynamic recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecorsetsOnly, variable.Item1);
                dynamic recordField = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, variable.Item1);

                if (!(_addedRecordsets.Contains(recordset) && _addedFieldset.Contains(recordField)))
                {
                    shape.Append(string.Format("<{0}>", recordset));
                    shape.Append(string.Format("<{0}/>", recordField));
                    shape.Append(string.Format("</{0}>", recordset));
                    _addedRecordsets.Add(recordset);
                    _addedFieldset.Add(recordField);
                }

                data.Append(string.Format("<{0}>", recordset));
                data.Append(string.Format("<{0}>{1}</{0}>", recordField, variable.Item2));
                data.Append(string.Format("</{0}>", recordset));

                RecordSetName = recordset;
                FieldName = recordField;
            }
            else
            {
                string variableName = DataListUtil.RemoveLanguageBrackets(variable.Item1);
                shape.Append(string.Format("<{0}/>", variableName));
                data.Append(string.Format("<{0}>{1}</{0}>", variableName, variable.Item2));
            }
        }

        protected string RetrieveItemForEvaluation(enIntellisensePartType partType, string value)
        {
            string rawRef = DataListUtil.StripBracketsFromValue(value);
            string objRef = string.Empty;

            if (partType == enIntellisensePartType.RecorsetsOnly)
            {
                objRef = DataListUtil.ExtractRecordsetNameFromValue(rawRef);
            }
            else if (partType == enIntellisensePartType.RecordsetFields)
            {
                objRef = DataListUtil.ExtractFieldNameFromValue(rawRef);
            }

            return objRef;
        }
    }
}