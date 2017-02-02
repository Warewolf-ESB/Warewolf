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
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
using Dev2.Data;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Core.Models.DataList;
using Dev2.Studio.ViewModels.DataList;

namespace Dev2.DataList
{
    /// <summary>
    /// Used to perform fuzzy matching of sorts ;)
    /// NOTE : Commented out code for future extention on this piece
    /// </summary>
    class FuzzyMatchVo
    {
        internal IDictionary<Tuple<string, string>, string> RecordsetColumnsToName;

        internal FuzzyMatchVo(IDictionary<Tuple<string,string>, string> matches)
        {
            RecordsetColumnsToName = matches;
        }

        /// <summary>
        /// Fetches the match.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="recset"></param>
        /// <returns></returns>
        internal string FetchMatch(string token,string recset)
        {
            string result;

            RecordsetColumnsToName.TryGetValue(new Tuple<string, string>(token,recset), out result);

            return result;
        }
    }

    /// <summary>
    /// Used to return the mapping view models ;)
    /// </summary>
    public class MappingViewModelTO
    {
        public IList<IInputOutputViewModel> Inputs { get; private set; }

        public IList<IInputOutputViewModel> Outputs { get; private set; }

        public MappingViewModelTO(IList<IInputOutputViewModel> inputs, IList<IInputOutputViewModel> outputs)
        {
            Inputs = inputs;
            Outputs = outputs;
        }
    }

    /// <summary>
    /// Used to generate the data mapping view model data via the builder pattern ;)
    /// </summary>
    public class ActivityDataMappingBuilder
    {
        private List<ComplexObjectItemModel> _complexObjects;

        /// <summary>
        /// Gets or sets the data list.
        /// </summary>
        /// <value>
        /// The data list.
        /// </value>
        public string DataList { get; set; }

        /// <summary>
        /// Gets or sets the saved input mapping.
        /// </summary>
        /// <value>
        /// The saved input mapping.
        /// </value>
        public string SavedInputMapping { get; private set; }

        /// <summary>
        /// Gets or sets the saved output mapping.
        /// </summary>
        /// <value>
        /// The saved output mapping.
        /// </value>
        public string SavedOutputMapping { get; private set; }

        /// <summary>
        /// Gets the activity input definitions.
        /// </summary>
        /// <value>
        /// The activity input definitions.
        /// </value>
        public string ActivityInputDefinitions { get; private set; }

        /// <summary>
        /// Gets the activity output definitions.
        /// </summary>
        /// <value>
        /// The activity output definitions.
        /// </value>
        public string ActivityOutputDefinitions { get; private set; }

        /// <summary>
        /// Gets the type of the activity.
        /// </summary>
        /// <value>
        /// The type of the activity.
        /// </value>
        public Type ActivityType { get; private set; }

        /// <summary>
        /// Setups the activity data.
        /// </summary>
        /// <param name="activity">The activity.</param>
        public void SetupActivityData(IWebActivity activity)
        {
            if(activity != null)
            {
                SavedInputMapping = activity.SavedInputMapping;
                SavedOutputMapping = activity.SavedOutputMapping;
                ActivityType = activity.UnderlyingWebActivityObjectType;
                _complexObjects = new List<ComplexObjectItemModel>();
                if (activity.ResourceModel != null)
                {
                    string inputs;
                    string outputs;

                    // handle workflows differently ;)

                    if(activity.IsNotAvailable())
                    {
                        inputs = activity.ResourceModel.Inputs;
                        outputs = activity.ResourceModel.Outputs;
                    }
                    else
                    {
                        var datalist = activity.ResourceModel.DataList;
                        AddComplexObjects(datalist);
                        inputs = DataListUtil.GenerateSerializableDefsFromDataList(datalist,
                            enDev2ColumnArgumentDirection.Input);
                        outputs = DataListUtil.GenerateSerializableDefsFromDataList(datalist,
                            enDev2ColumnArgumentDirection.Output);
                    }                

                    ActivityInputDefinitions = inputs;
                    ActivityOutputDefinitions = outputs;
                }
            }
        }

        private void AddComplexObjects(string datalist)
        {
            try
            {
                var xDoc = new XmlDocument();
                xDoc.LoadXml(datalist);
                if (xDoc.DocumentElement != null)
                {
                    var children = xDoc.DocumentElement.ChildNodes;
                    foreach (XmlNode c in children)
                    {
                        if (!DataListUtil.IsSystemTag(c.Name))
                        {
                            var jsonAttribute = false;
                            var xmlAttribute = c.Attributes?["IsJson"];
                            if (xmlAttribute != null)
                            {
                                bool.TryParse(xmlAttribute.Value, out jsonAttribute);
                            }
                            if (jsonAttribute)
                            {
                                AddComplexObjectFromXmlNode(c, null);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error(e);
            }
        }

        private void AddComplexObjectFromXmlNode(XmlNode c, ComplexObjectItemModel parent)
        {
            var isArray = false;
            var ioDirection = enDev2ColumnArgumentDirection.None;
            if (c.Attributes != null)
            {
                isArray = ParseBoolAttribute(c.Attributes["IsArray"]);
                ioDirection = ParseColumnIODirection(c.Attributes[GlobalConstants.DataListIoColDirection]);
            }
            var name = GetNameForArrayComplexObject(c, isArray);
            var complexObjectItemModel = new ComplexObjectItemModel(name) { IsArray = isArray, Parent = parent, ColumnIODirection = ioDirection };
            if (parent != null)
            {
                parent.Children.Add(complexObjectItemModel);
            }
            else
            {
                _complexObjects.Add(complexObjectItemModel);
            }
            if (c.HasChildNodes)
            {
                var children = c.ChildNodes;
                foreach (XmlNode childNode in children)
                {
                    AddComplexObjectFromXmlNode(childNode, complexObjectItemModel);
                }
            }
        }
        private bool ParseBoolAttribute(XmlAttribute attr)
        {
            var result = true;
            if (attr != null)
            {
                bool.TryParse(attr.Value, out result);
            }
            return result;
        }
        private enDev2ColumnArgumentDirection ParseColumnIODirection(XmlAttribute attr)
        // ReSharper restore InconsistentNaming
        {
            enDev2ColumnArgumentDirection result = enDev2ColumnArgumentDirection.None;

            if (attr == null)
            {
                return result;
            }
            if (!Enum.TryParse(attr.Value, true, out result))
            {
                result = enDev2ColumnArgumentDirection.None;
            }
            return result;
        }
        private static string GetNameForArrayComplexObject(XmlNode xmlNode, bool isArray)
        {
            var name = isArray ? xmlNode.Name + "()" : xmlNode.Name;
            return name;
        }

        /// <summary>
        /// Generates this instance.
        /// </summary>
        public MappingViewModelTO Generate()
        {
            var outputParser = DataListFactory.CreateOutputParser();
            var inputParser = DataListFactory.CreateInputParser();

            var inputList = GenerateMapping(SavedInputMapping, ActivityInputDefinitions, false, inputParser);

            var outputList = GenerateMapping(SavedOutputMapping, ActivityOutputDefinitions, true, outputParser);

            MappingViewModelTO result = new MappingViewModelTO(inputList, outputList);

            // and set the data to save?!
            if(string.IsNullOrEmpty(SavedInputMapping))
            {
                SavedInputMapping = GetInputString(inputList);
            }

            if(string.IsNullOrEmpty(SavedOutputMapping))
            {
                SavedOutputMapping = GetOutputString(outputList);
            }

            return result;
        }

        /// <summary>
        /// Gets the input string.
        /// </summary>
        /// <param name="inputData">The input data.</param>
        /// <returns></returns>
        private string GetInputString(IList<IInputOutputViewModel> inputData)
        {
            string inputString = string.Empty;
            IList<IDev2Definition> inputs = new List<IDev2Definition>();
            if(inputData.Count != 0)
            {
                foreach(IInputOutputViewModel itp in inputData)
                {
                    inputs.Add(itp.GetGenerationTO());
                }
                inputString = DataMappingListFactory.GenerateMapping(inputs, enDev2ArgumentType.Input);
            }
            return inputString;
        }

        /// <summary>
        /// Gets the output string.
        /// </summary>
        /// <param name="outputData">The output data.</param>
        /// <returns></returns>
        private string GetOutputString(IList<IInputOutputViewModel> outputData)
        {
            string outputString = string.Empty;
            IList<IDev2Definition> outputs = new List<IDev2Definition>();
            if(outputData.Count != 0)
            {
                foreach(IInputOutputViewModel otp in outputData)
                {
                    outputs.Add(otp.GetGenerationTO());
                }
                outputString = DataMappingListFactory.GenerateMapping(outputs, enDev2ArgumentType.Output);
            }
            return outputString;
        }

        /// <summary>
        /// Generates the mapping.
        /// </summary>
        /// <param name="savedMappingData">The mapping data.</param>
        /// <param name="mappingDefinitions">The mapping definitions.</param>
        /// <param name="isOutputMapping">if set to <c>true</c> [is output mapping].</param>
        /// <param name="parser">The parser.</param>
        /// <returns></returns>
        private IList<IInputOutputViewModel> GenerateMapping(string savedMappingData, string mappingDefinitions, bool isOutputMapping, IDev2LanguageParser parser)
        {
            IList<IInputOutputViewModel> result;

            if(string.IsNullOrEmpty(savedMappingData))
            {
                // TODO : Inject fuzzy matching logic here ;)
                var fuzzyMatchDefinitions = GenerateMatchFragmentsFromDataList();

                result = CreateMappingList(mappingDefinitions, parser, true, isOutputMapping, fuzzyMatchDefinitions);
            }
            else
            {

                // generate the master view ;)
                var masterView = CreateMappingList(mappingDefinitions, parser, true, isOutputMapping);

                // use existing data ;)
                var existingView = CreateMappingList(savedMappingData, parser, false, isOutputMapping);

                // Now adjust for the difference between the two views ;)
                result = ReconcileExistingAndMasterView(masterView, existingView);
            }

            return result;
        }

        /// <summary>
        /// Reconsiles the exiting and master view.
        /// </summary>
        /// <param name="masterView">The master view.</param>
        /// <param name="existingView">The existing view.</param>
        /// <returns></returns>
        private IList<IInputOutputViewModel> ReconcileExistingAndMasterView(IList<IInputOutputViewModel> masterView, IEnumerable<IInputOutputViewModel> existingView)
        {
            var equalityCompareImpl = new InputOutputViewModelEqualityComparer();

            var intersectionResult = existingView.Intersect(masterView, equalityCompareImpl);

            IEnumerable<IInputOutputViewModel> inputOutputViewModels = intersectionResult as IList<IInputOutputViewModel> ?? intersectionResult.ToList();
            foreach(var intersectionRowItem in inputOutputViewModels)
            {
                //  Find a match in master list and tranfer properties ;)
                var match = masterView.FirstOrDefault(c => c.DisplayName == intersectionRowItem.DisplayName);

                if(match != null)
                {
                    intersectionRowItem.Required = match.Required;
                    intersectionRowItem.DefaultValue = match.DefaultValue;
                    intersectionRowItem.IsObject = match.IsObject;
                }
            }


            // ordering maters ;)
            IList<IInputOutputViewModel> result = inputOutputViewModels.Union(masterView, equalityCompareImpl).ToList();

            return result;
        }

        /// <summary>
        /// Creates the mapping list.
        /// </summary>
        /// <param name="mappingDefinitions">The mapping definitions.</param>
        /// <param name="parser">The parser.</param>
        /// <param name="autoAddBrackets">if set to <c>true</c> [automatic add brackets].</param>
        /// <param name="isOutputMapping">if set to <c>true</c> [is output mapping].</param>
        /// <param name="fuzzyMatch">The fuzzy match.</param>
        /// <returns></returns>
        private IList<IInputOutputViewModel> CreateMappingList(string mappingDefinitions, IDev2LanguageParser parser, bool autoAddBrackets, bool isOutputMapping, FuzzyMatchVo fuzzyMatch = null)
        {
            IList<IInputOutputViewModel> result = new List<IInputOutputViewModel>();
            IList<IDev2Definition> concreteDefinitions = parser.ParseAndAllowBlanks(mappingDefinitions);

            foreach(var def in concreteDefinitions)
            {
                var injectValue = def.RawValue;
                if(autoAddBrackets)
                {
                    // When output mapping we need to replace the recordset name if present with MasterRecordset
                    // 
                    string masterRecordsetName;
                    if(isOutputMapping && def.IsRecordSet && fuzzyMatch != null)
                    {
                        var field = def.Name;

                        string recordsetName = fuzzyMatch.FetchMatch(def.Name, def.RecordSetName);
                        if(!string.IsNullOrEmpty(recordsetName))
                        {
                            masterRecordsetName = recordsetName;
                        }
                        else
                        {
                            // we have no match, use the current mapping value ;)
                            masterRecordsetName = def.RecordSetName;
                        }

                        injectValue = FormatString(masterRecordsetName, field);
                    }
                    else
                    {
                        if(def.IsRecordSet)
                        {
                            if(fuzzyMatch != null)
                            {
                                string recordsetName = fuzzyMatch.FetchMatch(def.Name, def.RecordSetName);
                                masterRecordsetName = !String.IsNullOrEmpty(recordsetName) ? recordsetName : def.RecordSetName;
                            }
                            else
                            {
                                masterRecordsetName = def.RecordSetName;
                            }

                            injectValue = FormatString(masterRecordsetName, def.Name);
                        }
                        else
                        {
                            injectValue = DataListUtil.AddBracketsToValueIfNotExist(def.Name);
                        }
                    }
                }                
                var injectMapsTo = def.MapsTo;

                // no saved mappings add brackets ;)
                if(!string.IsNullOrEmpty(injectMapsTo) && string.IsNullOrEmpty(SavedInputMapping))
                {
                    injectMapsTo = DataListUtil.AddBracketsToValueIfNotExist(injectMapsTo);
                }
                else
                {
                    if(def.IsRecordSet)
                    {
                        var tmp = injectValue.Replace("()", "(*)");
                        injectMapsTo = tmp; // tag it as the same ;)
                    }
                    else
                    {
                        injectMapsTo = injectValue; // tag it as the same ;)
                    }
                }

                // def.RecordSetName -> recordsetName
                var viewModel = new InputOutputViewModel(def.Name, injectValue, injectMapsTo, def.DefaultValue, def.IsRequired, def.RecordSetName, def.EmptyToNull);
                viewModel.IsObject = def.IsObject;
                if(def.IsObject)
                {
                    var complexObjectItemModel = _complexObjects.FirstOrDefault(model => model.Name == def.Name);
                    if(complexObjectItemModel != null)
                    {
                        viewModel.JsonString = complexObjectItemModel.GetJson();
                    }
                }
                result.Add(viewModel);
            }

            return result;
        }

        /// <summary>
        /// Formats the string.
        /// </summary>
        /// <param name="recset">The recset.</param>
        /// <param name="field">The field.</param>
        /// <returns></returns>
        private string FormatString(string recset, string field)
        {
            var tmp = DataListUtil.CreateRecordsetDisplayValue(recset, field, string.Empty);
            return DataListUtil.AddBracketsToValueIfNotExist(tmp);
        }

        /// <summary>
        /// Generates the match fragments from data list.
        /// </summary>
        /// <returns></returns>
        private FuzzyMatchVo GenerateMatchFragmentsFromDataList()
        {
            FuzzyMatchVo result = null;

            if (!string.IsNullOrEmpty(DataList))
            {

                var dataListModel = new DataListModel();
                dataListModel.Create(DataList,DataList);
                IDictionary<Tuple<string, string>, string> tmp = new Dictionary<Tuple<string, string>, string>();

                    foreach (var rs in dataListModel.RecordSets)
                    {
                        // build map for each column in a recordset ;)
                        foreach (var col in rs.Columns)
                        {
                            foreach(var scalar in col.Value)
                            {
                                if (!tmp.Keys.Any(a => a.Item2 == scalar.Name && a.Item1 == rs.Name))
                                {
                                    tmp[new Tuple<string, string>(rs.Name, scalar.Name)] = rs.Name;
                                }
                            }
                            
                        }
                    }

                result = new FuzzyMatchVo(tmp);

            }

            return result;
        }
    }
}
