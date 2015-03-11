
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using ActivityUnitTests;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.BaseTypes
{
    [Binding]
    public abstract class RecordSetBases : BaseActivityUnitTest
    {
        protected const string ResultVariable = "[[result]]";
        private int _lastAddedIndex;

        protected abstract void BuildDataList();

        protected virtual List<IDebugItemResult> GetDebugInputItemResults(Activity activity)
        {
            return CommonSteps.GetInputDebugItems(activity);
        }

        protected virtual List<IDebugItemResult> GetDebugOutputItemResults(Activity activity)
        {
            return CommonSteps.GetOutputDebugItems(activity);
        }

        protected void BuildShapeAndTestData()
        {
            var shape = new XElement("root");
            var data = new XElement("root");

            // ReSharper disable NotAccessedVariable
            int row = 0;
            dynamic variableList;
            ScenarioContext.Current.TryGetValue("variableList", out variableList);

            if(variableList != null)
            {
                foreach(dynamic variable in variableList)
                {
                    Build(variable, shape, data, row);
                    row++;
                }
            }

            List<Tuple<string, string>> emptyRecordset;
            bool isAdded = ScenarioContext.Current.TryGetValue("rs", out emptyRecordset);
            if(isAdded)
            {
                foreach(Tuple<string, string> emptyRecord in emptyRecordset)
                {
                    var recSetElement = shape
                                      .Descendants(emptyRecord.Item1)
                                      .FirstOrDefault();
                    if(recSetElement == null)
                    {
                        shape.Add(new XElement(emptyRecord.Item1, new XElement(emptyRecord.Item2)));
                    }
                    else
                    {
                        recSetElement.Add(new XElement(emptyRecord.Item2));
                    }
                }
            }

            CurrentDl = shape.ToString();
            TestData = data.ToString();
        }

        private void Build(dynamic variable, XElement shape, XElement data, int rowIndex)
        {
            try
            {
                if(DataListUtil.IsValueRecordset(variable.Item1))
                {
                    string recordset = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetsOnly, variable.Item1);
                    string recordField = RetrieveItemForEvaluation(enIntellisensePartType.RecordsetFields, variable.Item1);

                    List<string> addedRecordsets;
                    ScenarioContext.Current.TryGetValue("addedRecordsets", out addedRecordsets);

                    if(addedRecordsets == null)
                    {
                        addedRecordsets = new List<string>();
                        ScenarioContext.Current.Add("addedRecordsets", addedRecordsets);
                    }

                    List<string> addedFieldset;
                    ScenarioContext.Current.TryGetValue("addedFieldset", out addedFieldset);

                    if(addedFieldset == null)
                    {
                        addedFieldset = new List<string>();
                        ScenarioContext.Current.Add("addedFieldset", addedFieldset);
                    }

                    var recordSetElement = shape
                                            .Descendants(recordset)
                                            .FirstOrDefault();
                    if(recordSetElement == null)
                    {
                        shape.Add(new XElement(recordset, new XElement(recordField)));
                    }
                    else
                    {
                        var recordSetField = recordSetElement
                                          .Elements(recordField)
                                          .FirstOrDefault();

                        if(recordSetField == null)
                        {
                            recordSetElement.Add(new XElement(recordField));
                        }
                    }

                    var indexRegionFromRecordset = DataListUtil.ExtractIndexRegionFromRecordset(variable.Item1);

                    if(!string.IsNullOrEmpty(indexRegionFromRecordset))
                    {
                        int indexForRecset;
                        int.TryParse(indexRegionFromRecordset, out indexForRecset);

                        var blankRowsToAdd = rowIndex == 0 ? 0 : (indexForRecset - 1) - _lastAddedIndex;

                        if(blankRowsToAdd > 0)
                        {
                            for(int i = 0; i < blankRowsToAdd; i++)
                            {
                                if(!String.IsNullOrEmpty(variable.Item2))
                                {
                                    data.Add(new XElement(recordset, new XElement(recordField)));
                                }
                                _lastAddedIndex++;
                            }
                        }
                    }

                    if(!String.IsNullOrEmpty(variable.Item2))
                    {
                        try
                        {
                            data.Add(new XElement(recordset, new XElement(recordField, XElement.Parse(variable.Item2))));
                        }
                        catch
                        {
                            data.Add(new XElement(recordset, new XElement(recordField, variable.Item2)));
                        }
                        _lastAddedIndex++;
                    }

                    string rec;
                    ScenarioContext.Current.TryGetValue("recordset", out rec);

                    if(string.IsNullOrEmpty(rec))
                    {
                        ScenarioContext.Current.Add("recordset", recordset);
                    }

                    string field;
                    ScenarioContext.Current.TryGetValue("recordField", out field);

                    if(string.IsNullOrEmpty(field))
                    {
                        ScenarioContext.Current.Add("recordField", recordField);
                    }
                }
                else
                {
                    string variableName = DataListUtil.RemoveLanguageBrackets(variable.Item1);
                    if(!String.IsNullOrEmpty(variableName))
                    {
                        shape.Add(new XElement(variableName));
                        try
                        {
                            data.Add(new XElement(variableName, XElement.Parse(variable.Item2)));
                        }
                        catch
                        {
                            data.Add(new XElement(variableName, (string)variable.Item2));
                        }
                    }
                }
            }
            // ReSharper disable EmptyGeneralCatchClause
            catch(Exception)
            // ReSharper restore EmptyGeneralCatchClause
            {
                //Swallow xml exception to allow testing of special characters on variables
            }
        }


        protected string RetrieveItemForEvaluation(enIntellisensePartType partType, string value)
        {
            return CommonSteps.RetrieveItemForEvaluation(partType, value);
        }

        protected string ReadFile(string resourceName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            using(Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if(stream == null)
                {
                    return string.Empty;
                }

                using(var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
