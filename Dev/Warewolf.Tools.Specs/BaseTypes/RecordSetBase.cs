/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using ActivityUnitTests;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Util;
using TechTalk.SpecFlow;
using Dev2.Common.Interfaces;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Studio.Core;
using Dev2.Studio.Core.Models;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.ViewModels.DataList;
using WarewolfParserInterop;

namespace Warewolf.Tools.Specs.BaseTypes
{
    [Binding]
    public abstract class RecordSetBases : BaseActivityUnitTest
    {
        private readonly ScenarioContext scenarioContext;

        public RecordSetBases(ScenarioContext scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
            _commonSteps = new CommonSteps(this.scenarioContext);
        }

        protected const string ResultVariable = "[[result]]";
        private readonly CommonSteps _commonSteps;

        protected abstract void BuildDataList();

        protected virtual List<IDebugItemResult> GetDebugInputItemResults(Activity activity)
        {
            return _commonSteps.GetInputDebugItems(activity,DataObject.Environment);
        }

        protected virtual List<IDebugItemResult> GetDebugOutputItemResults(Activity activity)
        {
            return _commonSteps.GetOutputDebugItems(activity, DataObject.Environment);
        }

        protected void BuildShapeAndTestData()
        {
            var shape = new XElement("root");
            var data = new XElement("root");
            var dataListViewModel = new DataListViewModel();
            dataListViewModel.InitializeDataListViewModel(new ResourceModel(null));
            DataListSingleton.SetDataList(dataListViewModel);
            // ReSharper disable NotAccessedVariable
            int row = 0;
            dynamic variableList;
            scenarioContext.TryGetValue("variableList", out variableList);
            if(variableList != null)
            {
                try
                {
                    foreach (dynamic variable in variableList)
                    {
                        var variableName = DataListUtil.AddBracketsToValueIfNotExist(variable.Item1);
                        if (!string.IsNullOrEmpty(variable.Item1) && !string.IsNullOrEmpty(variable.Item2))
                        {
                            string value = variable.Item2 == "blank" ? "" : variable.Item2;
                            if (value.ToUpper() == "NULL")
                            {
                                DataObject.Environment.AssignDataShape(variable.Item1);
                            }
                            else
                            {
                                DataObject.Environment.Assign(variableName, value, 0);
                            }
                        }
                        if (DataListUtil.IsValueScalar(variableName))
                        {
                            var scalarName = DataListUtil.RemoveLanguageBrackets(variableName);
                            var scalarItemModel = new ScalarItemModel(scalarName);
                            if (!scalarItemModel.HasError)
                            {
                                DataListSingleton.ActiveDataList.ScalarCollection.Add(scalarItemModel);
                            }
                        }
                        if (DataListUtil.IsValueRecordsetWithFields(variableName))
                        {
                            var rsName = DataListUtil.ExtractRecordsetNameFromValue(variableName);
                            var fieldName = DataListUtil.ExtractFieldNameOnlyFromValue(variableName);
                            var rs = DataListSingleton.ActiveDataList.RecsetCollection.FirstOrDefault(model => model.Name == rsName);
                            if (rs == null)
                            {
                                var recordSetItemModel = new RecordSetItemModel(rsName);
                                DataListSingleton.ActiveDataList.RecsetCollection.Add(recordSetItemModel);
                                recordSetItemModel.Children.Add(new RecordSetFieldItemModel(fieldName,
                                    recordSetItemModel));
                            }
                            else
                            {
                                var recordSetFieldItemModel = rs.Children.FirstOrDefault(model => model.Name == fieldName);
                                if (recordSetFieldItemModel == null)
                                {
                                    rs.Children.Add(new RecordSetFieldItemModel(fieldName, rs));
                                }
                            }
                        }
                        //Build(variable, shape, data, row);
                        row++;
                    }
                    DataListSingleton.ActiveDataList.WriteToResourceModel();
                }
                    // ReSharper disable EmptyGeneralCatchClause
                catch
                    // ReSharper restore EmptyGeneralCatchClause
                {
                    
                }
            }

            List<Tuple<string, string>> emptyRecordset;
            bool isAdded = scenarioContext.TryGetValue("rs", out emptyRecordset);
            if (isAdded)
            {
                foreach (Tuple<string, string> emptyRecord in emptyRecordset)
                {
                    DataObject.Environment.Assign(DataListUtil.AddBracketsToValueIfNotExist(emptyRecord.Item1), emptyRecord.Item2, 0);
                }
            }

            dynamic objList;
            scenarioContext.TryGetValue("objList", out objList);
            if (objList != null)
            {
                try
                {
                    foreach (dynamic variable in objList)
                    {
                        if (!string.IsNullOrEmpty(variable.Item1) && !string.IsNullOrEmpty(variable.Item2))
                        {
                            string value = variable.Item2 == "blank" ? "" : variable.Item2;
                            if (value.ToUpper() == "NULL")
                            {
                                DataObject.Environment.AssignDataShape(variable.Item1);
                            }
                            else
                            {
                                DataObject.Environment.AssignJson(new AssignValue(DataListUtil.AddBracketsToValueIfNotExist(variable.Item1), value), 0);
                            }
                        }
                    }
                }
                // ReSharper disable EmptyGeneralCatchClause
                catch
                // ReSharper restore EmptyGeneralCatchClause
                {

                }
            }

            CurrentDl = shape.ToString();
            TestData = data.ToString();
        }


        protected string RetrieveItemForEvaluation(enIntellisensePartType partType, string value)
        {
            return _commonSteps.RetrieveItemForEvaluation(partType, value);
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
