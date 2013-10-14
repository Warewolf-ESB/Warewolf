using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Dev2.Common.ExtMethods;
using Dev2.Data.Binary_Objects;
using Dev2.Data.Interfaces;
using Dev2.DataList.Contract;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.ViewModels.DataList;

namespace Dev2.DataList
{
    /// <summary>
    /// Used to perform fuzzy matching of sorts ;)
    /// </summary>
    class FuzzyMatchVO
    {
        internal IList<IDev2Definition> FuzzyDefinitions { get; private set; }

        internal IList<string> FuzzyValueHash { get; private set; }
        internal IList<string> FuzzyMapsToHash { get; private set; }

        internal FuzzyMatchVO(IList<IDev2Definition> defs)
        {
            FuzzyDefinitions = defs;

            // bootstrap hashes ;)
            foreach (var def in defs)
            {
                if (!string.IsNullOrEmpty(def.Value))
                {
                    FuzzyValueHash.Add(def.Value.GenerateDoubleMetaphone());
                }

                if (!string.IsNullOrEmpty(def.MapsTo))
                {
                    FuzzyMapsToHash.Add(def.MapsTo.GenerateDoubleMetaphone());
                }
            }
        }

        /// <summary>
        /// Fetches the match.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="useValueHash">if set to <c>true</c> [use value hash].</param>
        /// <returns></returns>
        internal string FetchMatch(string token, bool useValueHash)
        {
            return string.Empty;
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
            if (activity != null)
            {
                SavedInputMapping = activity.SavedInputMapping;
                SavedOutputMapping = activity.SavedOutputMapping;
                ActivityType = activity.UnderlyingWebActivityObjectType;

                if (activity.ResourceModel != null)
                {
                    // extract the IO data too ;)
                    string serviceDefinition = activity.ResourceModel.ServiceDefinition;
                    if (!string.IsNullOrEmpty(serviceDefinition))
                    {
                        string inputs = string.Empty;
                        string outputs = string.Empty;

                        // handle workflows differently ;)
                        if (activity.ResourceModel.ResourceType == ResourceType.WorkflowService)
                        {
                            var datalist = activity.ResourceModel.DataList;
                            var compiler = DataListFactory.CreateDataListCompiler();

                            inputs = compiler.GenerateSerializableDefsFromDataList(datalist, enDev2ColumnArgumentDirection.Input);
                            outputs = compiler.GenerateSerializableDefsFromDataList(datalist, enDev2ColumnArgumentDirection.Output);
                        }
                        else
                        {
                            // handle services ;)
                            inputs = ExtractMappingData(serviceDefinition, "Inputs");
                            outputs = ExtractMappingData(serviceDefinition, "Outputs");
                        }

                        ActivityInputDefinitions = inputs;
                        ActivityOutputDefinitions = outputs;     
                    }
                }
            }
        }

        /// <summary>
        /// Generates this instance.
        /// </summary>
        public MappingViewModelTO Generate()
        {

            MappingViewModelTO result = null;

            var outputParser = DataListFactory.CreateOutputParser();
            var inputParser = DataListFactory.CreateInputParser();

            var inputList = GenerateMapping(SavedInputMapping, ActivityInputDefinitions, inputParser);
            
            var outputList = GenerateMapping(SavedOutputMapping, ActivityOutputDefinitions, outputParser);

            result = new MappingViewModelTO(inputList, outputList);
           
            // and set the data to save?!
            if(string.IsNullOrEmpty(SavedInputMapping))
            {
                SavedInputMapping = ActivityInputDefinitions;
            }

            if(string.IsNullOrEmpty(SavedOutputMapping))
            {
                SavedOutputMapping = ActivityOutputDefinitions;
            }

           
            return result;
        }

        /// <summary>
        /// Generates the mapping.
        /// </summary>
        /// <param name="savedMappingData">The mapping data.</param>
        /// <param name="mappingDefinitions">The mapping definitions.</param>
        /// <param name="parser">The parser.</param>
        /// <returns></returns>
        private IList<IInputOutputViewModel> GenerateMapping(string savedMappingData, string mappingDefinitions, IDev2LanguageParser parser)
        {
            IList<IInputOutputViewModel> result;

            if (string.IsNullOrEmpty(savedMappingData))
            {
                // TODO : Inject fuzzy matching logic here ;)
                var fuzzyMatchDefinitions = GenerateMatchFragmentsFromDataList();

                result = CreateMappingList(mappingDefinitions, parser, true, fuzzyMatchDefinitions);
            }
            else
            {
                
                // generate the master view ;)
                var masterView = CreateMappingList(mappingDefinitions, parser, true);

                // use existing data ;)
                var existingView = CreateMappingList(savedMappingData, parser, false);

                // Now adjust for the difference between the two views ;)
                result = ReconsileExitingAndMasterView(masterView, existingView);
            }

            return result;
        }

        /// <summary>
        /// Reconsiles the exiting and master view.
        /// </summary>
        /// <param name="masterView">The master view.</param>
        /// <param name="existingView">The existing view.</param>
        /// <returns></returns>
        private IList<IInputOutputViewModel> ReconsileExitingAndMasterView(IList<IInputOutputViewModel> masterView, IList<IInputOutputViewModel> existingView)
        {
            IList<IInputOutputViewModel> result = null;

            var equalityCompareImpl = new InputOutputViewModelEqualityComparer();

            var intersectionResult = existingView.Intersect(masterView, equalityCompareImpl);

            // ordering maters ;)
            result = intersectionResult.Union(masterView, equalityCompareImpl).ToList();

            return result;
        }

        /// <summary>
        /// Creates the mapping list.
        /// </summary>
        /// <param name="mappingDefinitions">The mapping definitions.</param>
        /// <param name="parser">The parser.</param>
        /// <param name="autoAddBrackets">if set to <c>true</c> [automatic add brackets].</param>
        /// <param name="fuzzyMatch">The fuzzy match.</param>
        /// <returns></returns>
        private IList<IInputOutputViewModel> CreateMappingList(string mappingDefinitions, IDev2LanguageParser parser, bool autoAddBrackets, FuzzyMatchVO fuzzyMatch = null)
        {
            IList<IInputOutputViewModel> result = new List<IInputOutputViewModel>();
            IList<IDev2Definition> concreteDefinitions = parser.ParseAndAllowBlanks(mappingDefinitions);

            foreach(var def in concreteDefinitions)
            {
                var injectValue = def.RawValue;

                if (!string.IsNullOrEmpty(injectValue))
                {
                    if (autoAddBrackets)
                    {
                        injectValue = DataListUtil.AddBracketsToValueIfNotExist(injectValue);
                    }
                }
                else
                {
                    if(!def.IsRecordSet)
                    {
                        injectValue = DataListUtil.AddBracketsToValueIfNotExist(def.Name);
                    }
                    else
                    {
                        var tmp = DataListUtil.ComposeIntoUserVisibleRecordset(def.RecordSetName, string.Empty, def.Name);
                        injectValue = DataListUtil.AddBracketsToValueIfNotExist(tmp);
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

                // TODO : perform fuzzy matching ;)
                /*if (fuzzyMatch != null)
                {
                    injectValue = FilterForFuzzyMatch(injectValue, true, fuzzyMatch);

                    injectMapsTo = FilterForFuzzyMatch(injectMapsTo, false, fuzzyMatch);
                }*/

                var viewModel = new InputOutputViewModel(def.Name, injectValue, injectMapsTo, def.DefaultValue, def.IsRequired, def.RecordSetName, def.EmptyToNull);

                result.Add(viewModel);
            }

            return result;
        }

        /// <summary>
        /// Filters for fuzzy match.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="useValueHash">if set to <c>true</c> [use value hash].</param>
        /// <param name="fuzzyMatch">The fuzzy match.</param>
        private string FilterForFuzzyMatch(string value, bool useValueHash, FuzzyMatchVO fuzzyMatch)
        {
            var result = value;
            var fuzzyInject = fuzzyMatch.FetchMatch(value, true);
            if(!string.IsNullOrEmpty(fuzzyInject))
            {
                result = fuzzyInject;
            }

            return result;
        }

        /// <summary>
        /// Generates the match fragments from data list.
        /// </summary>
        /// <returns></returns>
        private FuzzyMatchVO GenerateMatchFragmentsFromDataList()
        {
            FuzzyMatchVO result = null;

            IList<IDev2Definition> resultList = new List<IDev2Definition>();

            if (string.IsNullOrEmpty(DataList))
            {
                var compiler = DataListFactory.CreateDataListCompiler();

                if (string.IsNullOrEmpty(DataList))
                {
                    resultList = compiler.GenerateDefsFromDataList(DataList);
                }
            }

            result = new FuzzyMatchVO(resultList);

            return result;
        }

        /// <summary>
        /// Extracts the mapping data.
        /// </summary>
        /// <param name="serviceDefinition">The service definition.</param>
        /// <param name="regionName">Name of the region.</param>
        /// <returns></returns>
        private string ExtractMappingData(string serviceDefinition, string regionName)
        {
            string result = string.Empty;

            XElement xe = XElement.Parse(serviceDefinition);
            var actions = xe.Element("Actions");

            if(actions != null)
            {
                var action = actions.Element("Action");
                if(action != null)
                {
                    var region = action.Element(regionName);

                    if (region != null)
                    {
                        result = region.ToString(SaveOptions.DisableFormatting);
                    }
                }
            }

            return result;
        }
    }
}
