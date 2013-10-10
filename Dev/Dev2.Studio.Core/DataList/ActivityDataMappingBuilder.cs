using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Dev2.Data.Interfaces;
using Dev2.DataList.Contract;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.Factory;
using Dev2.Studio.ViewModels.DataList;

namespace Dev2.DataList
{

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

                        var inputs = ExtractMappingData(serviceDefinition, "Inputs");
                        var outputs = ExtractMappingData(serviceDefinition, "Outputs");

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
                result = CreateMappingList(mappingDefinitions, parser);
            }
            else
            {
                // use existing data ;)
                result = CreateMappingList(savedMappingData, parser);
            }

            return result;
        }

        /// <summary>
        /// Creates the mapping list.
        /// </summary>
        /// <param name="mappingDefinitions">The mapping definitions.</param>
        /// <param name="parser">The parser.</param>
        /// <returns></returns>
        private IList<IInputOutputViewModel> CreateMappingList(string mappingDefinitions, IDev2LanguageParser parser)
        {
            IList<IInputOutputViewModel> result = new List<IInputOutputViewModel>();
            IList<IDev2Definition> concreteDefinitions = parser.ParseAndAllowBlanks(mappingDefinitions);

            foreach(var def in concreteDefinitions)
            {
                var viewModel = new InputOutputViewModel(def.Name, def.Value, def.MapsTo, def.DefaultValue,
                                                         def.IsRequired, def.RecordSetName, def.EmptyToNull);

                result.Add(viewModel);
            }

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

        //private string ConvertModelDataToString(IList<IInputOutputViewModel> modelData)
        //{
        //    string outputString = string.Empty;
        //    IList<IDev2Definition> outputs = new List<IDev2Definition>();
        //    if(modelData.Count != 0)
        //    {
        //        foreach(IInputOutputViewModel otp in modelData)
        //        {
        //            outputs.Add(otp.GetGenerationTO());
        //        }
        //        outputString = DataMappingListFactory.GenerateMapping(outputs, enDev2ArgumentType.Output);
        //    }
        //    return outputString;
        //}
    }
}
