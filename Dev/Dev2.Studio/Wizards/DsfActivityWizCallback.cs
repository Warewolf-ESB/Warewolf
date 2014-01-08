using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Wizards.Interfaces;
using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel.Composition;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Wizards
{
    [Export(typeof(IActivitySpecificSettingsWizardCallbackHandler))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DsfActivityWizCallback : IActivitySpecificSettingsWizardCallbackHandler
    {
        #region Fields

        private ModelItem _activity;
        private Guid _datalistID;
        private Func<IDataListCompiler> _createCompiler;

        #endregion Fields

        #region Properties

        public ModelItem Activity
        {
            get
            {
                return _activity;
            }
            set
            {
                _activity = value;
            }
        }

        public Guid DatalistID
        {
            get
            {
                return _datalistID;
            }
            set
            {
                _datalistID = value;
            }
        }

        public Func<IDataListCompiler> CreateCompiler
        {
            get
            {
                if(_createCompiler == null) return () => null;
                return _createCompiler;
            }
            set
            {
                _createCompiler = value;
            }
        }

        #endregion Properties

        #region Methods
        public void CompleteCallback()
        {
            IDataListCompiler compiler = CreateCompiler();

            if(_activity != null && compiler != null)
            {
                ErrorResultTO errors;
                IBinaryDataList wizardDataList = compiler.FetchBinaryDataList(_datalistID, out errors);
                if(wizardDataList != null && !errors.HasErrors())
                {
                    compiler.ConvertFrom(wizardDataList.UID, DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), enTranslationDepth.Data, out errors);
                    if(!errors.HasErrors())
                    {
                        {
                            //Create new lists to output to

                            ModelProperty modelProperty = _activity.Properties["InputMapping"];
                            if(modelProperty != null)
                            {
                                string inMapping = modelProperty.ComputedValue as string;
                                ModelProperty property = _activity.Properties["OutputMapping"];
                                if(property != null)
                                {
                                    string outMapping = property.ComputedValue as string;


                                    //Get the current input and output mapping from the activity

                                    IDev2LanguageParser outputParser = DataListFactory.CreateOutputParser();
                                    IDev2LanguageParser inputParser = DataListFactory.CreateInputParser();

                                    IList<IDev2Definition> outputs = outputParser.ParseAndAllowBlanks(outMapping);
                                    IList<IDev2Definition> inputs = inputParser.ParseAndAllowBlanks(inMapping);

                                    //loop through the outputs and get the new values
                                    IList<IDev2Definition> newOutputs = CreateNewListOfDefs(outputs, wizardDataList);
                                    //loop through the inputs and get the new values
                                    IList<IDev2Definition> newInputs = CreateNewListOfDefs(inputs, wizardDataList);

                                    //Setting the input and output mapping on the activity
                                    _activity.Properties.SetValue("InputMapping", DataMappingListFactory.GenerateMapping(newInputs, enDev2ArgumentType.Input));
                                    _activity.Properties.SetValue("OutputMapping", DataMappingListFactory.GenerateMapping(newOutputs, enDev2ArgumentType.Output));
                                }
                            }
                            compiler.DeleteDataListByID(_datalistID);

                            //
                            // Send out a message to update the current data mapping
                            //
                            compiler.ForceDeleteDataListByID(wizardDataList.UID);
                        }
                    }
                }
            }
        }

        public void CancelCallback()
        {
            IDataListCompiler compiler = CreateCompiler();

            if(compiler != null)
            {
                //Deletes the data list being kept on the server.
                compiler.DeleteDataListByID(_datalistID);
            }
        }
        #endregion Methods

        #region Private Methods

        /// <summary>
        /// Creates the new list of defs.
        /// </summary>
        /// <param name="listToLoopThrough">The list to loop through.</param>
        /// <param name="wizDl">The wizard data list.</param>
        /// <returns>List with all the new values</returns>
        private IList<IDev2Definition> CreateNewListOfDefs(IEnumerable<IDev2Definition> listToLoopThrough, IBinaryDataList wizDl)
        {
            IList<IDev2Definition> result = new List<IDev2Definition>();
            foreach(IDev2Definition def in listToLoopThrough)
            {
                string errorString;
                wizDl.FetchAllEntries();
                //Find a match in the returned defintions
                IBinaryDataListEntry entry;
                if(def.IsRecordSet)
                {
                    wizDl.TryGetEntry(def.RecordSetName + GlobalConstants.RecordsetJoinChar + def.Name, out entry, out errorString);
                }
                else
                {
                    wizDl.TryGetEntry(def.Name, out entry, out errorString);
                }

                if(entry != null)
                {
                    IBinaryDataListItem item = entry.FetchScalar();
                    IDev2Definition tmpDef = DataListFactory.CreateDefinition(def.Name, item.TheValue, item.TheValue, def.RecordSetName, def.IsEvaluated, def.DefaultValue, def.IsRequired, item.TheValue, false) ?? def;
                    result.Add(tmpDef);
                }

            }
            return result;
        }

        #endregion Private Methods
    }
}
