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
using System.Reflection;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Studio.Core.Wizards.CallBackHandlers
{
    [Export(typeof(IActivitySpecificSettingsWizardCallbackHandler<DsfActivity>))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DsfActivityWizCallback : IActivitySpecificSettingsWizardCallbackHandler<DsfActivity>
    {
        #region Fields

        private ModelItem _activity;
        private Guid _datalistID;
        private Func<IDataListCompiler> _createCompiler;

        #endregion Fields

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DsfActivityWizCallback" /> class.
        /// </summary>
        public DsfActivityWizCallback()
        {
        }

        #endregion Ctor

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
                if (_createCompiler == null) return new Func<IDataListCompiler>(() => null);
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

            if (_activity != null && _datalistID != null && compiler != null)
            {
                IBinaryDataList wizardDataList;
                ErrorResultTO errors;
                wizardDataList = compiler.FetchBinaryDataList(_datalistID, out errors);
                if (wizardDataList != null && !errors.HasErrors())
                {
                    string wizDlString = compiler.ConvertFrom(wizardDataList.UID, DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), enTranslationDepth.Data, out errors);
                    if (!errors.HasErrors())
                    {
                        if (wizardDataList != null)
                        {
                            //Create new lists to output to
                            IList<IDev2Definition> newOutputs = new List<IDev2Definition>();
                            IList<IDev2Definition> newInputs = new List<IDev2Definition>();

                            string inMapping = _activity.Properties["InputMapping"].ComputedValue as string;
                            string outMapping = _activity.Properties["OutputMapping"].ComputedValue as string;


                            //Get the current input and output mapping from the activity

                            IDev2LanguageParser _outputParser = DataListFactory.CreateOutputParser();
                            IDev2LanguageParser _inputParser = DataListFactory.CreateInputParser();

                            IList<IDev2Definition> outputs = _outputParser.ParseAndAllowBlanks(outMapping);
                            IList<IDev2Definition> inputs = _inputParser.ParseAndAllowBlanks(inMapping);

                            //loop through the outputs and get the new values
                            newOutputs = CreateNewListOfDefs(outputs, wizardDataList, enDev2ArgumentType.Output);
                            //loop through the inputs and get the new values
                            newInputs = CreateNewListOfDefs(inputs, wizardDataList, enDev2ArgumentType.Input);

                            //Setting the input and output mapping on the activity
                            _activity.Properties.SetValue("InputMapping", DataMappingListFactory.GenerateMapping(newInputs, enDev2ArgumentType.Input));
                            _activity.Properties.SetValue("OutputMapping", DataMappingListFactory.GenerateMapping(newOutputs, enDev2ArgumentType.Output));
                            compiler.DeleteDataListByID(_datalistID);

                            //
                            // Send out a message to update the current data mapping
                            //
                            Mediator.SendMessage(MediatorMessages.UpdateDataMapping, null);
                            compiler.ForceDeleteDataListByID(wizardDataList.UID);
                        }
                    }
                }
            }
        }

        public void CancelCallback()
        {
            IDataListCompiler compiler = CreateCompiler();

            if (compiler != null)
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
        private IList<IDev2Definition> CreateNewListOfDefs(IList<IDev2Definition> listToLoopThrough, IBinaryDataList wizDl, enDev2ArgumentType type)
        {
            IList<IDev2Definition> result = new List<IDev2Definition>();
            foreach (IDev2Definition def in listToLoopThrough)
            {
                string errorString;
                IList<IBinaryDataListEntry> entries = wizDl.FetchAllEntries();
                //Find a match in the returned defintions
                IBinaryDataListEntry entry;
                if (def.IsRecordSet)
                {
                    wizDl.TryGetEntry(def.RecordSetName + GlobalConstants.RecordsetJoinChar + def.Name, out entry, out errorString);
                }
                else
                {
                    wizDl.TryGetEntry(def.Name, out entry, out errorString);
                }

                if (entry != null)
                {
                    IBinaryDataListItem item = entry.FetchScalar();
                    IDev2Definition tmpDef = DataListFactory.CreateDefinition(def.Name, item.TheValue, item.TheValue, def.RecordSetName, def.IsEvaluated, def.DefaultValue, def.IsRequired, item.TheValue, false);
                    if (tmpDef == null)
                    {
                        tmpDef = def;
                    }
                    result.Add(tmpDef);
                }

            }
            return result;
        }

        /// <summary>
        /// Gets the inputs for an activity.
        /// </summary>
        /// <param name="activity">The activity.</param>
        private IBinaryDataList GetInputs(ModelItem activity)
        {
            MethodInfo mi = activity.ItemType.GetMethod("GetInputs");
            if (mi != null)
            {
                return mi.Invoke(activity.GetCurrentValue(), null) as IBinaryDataList;
            }
            return null;
        }

        /// <summary>
        /// Gets the outputs for an activity.
        /// </summary>
        /// <param name="activity">The activity.</param>
        private IBinaryDataList GetOutputs(ModelItem activity)
        {
            MethodInfo mi = activity.ItemType.GetMethod("GetOutputs");
            if (mi != null)
            {
                return mi.Invoke(activity.GetCurrentValue(), null) as IBinaryDataList;
            }
            return null;
        }

        #endregion Private Methods
    }
}
