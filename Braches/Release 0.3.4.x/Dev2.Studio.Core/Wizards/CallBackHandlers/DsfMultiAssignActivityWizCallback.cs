using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Wizards.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract;

namespace Dev2.Studio.Core.Wizards.CallBackHandlers
{
    // Old Wizard Functionlity
    //[Export(typeof(IActivitySpecificSettingsWizardCallbackHandler<DsfMultiAssignActivity>))]
    //[PartCreationPolicy(CreationPolicy.NonShared)]
    public class DsfMultiAssignActivityWizCallback : IActivitySpecificSettingsWizardCallbackHandler<DsfMultiAssignActivity>
    {
        #region Fields

        private ModelItem _activity;
        private Guid _datalistID;
        private Func<IDataListCompiler> _createCompiler;

        #endregion Fields

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DsfMultiAssignActivityWizCallback" /> class.
        /// </summary>
        public DsfMultiAssignActivityWizCallback()
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
                    IList<ActivityDTO> newFieldsCollection = new List<ActivityDTO>();

                    IList<IBinaryDataListEntry> recsets = wizardDataList.FetchRecordsetEntries();
                    IBinaryDataListEntry recset = recsets.FirstOrDefault(c => c.Namespace == "FieldsCollection");
                    if (recset != null)
                    {
                        int count = 0;
                        string error = string.Empty;
                        while (count < recset.ItemCollectionSize())
                        {
                            IList<IBinaryDataListItem> listOfRows = recset.FetchRecordAt(count + 1, out error);
                            IBinaryDataListItem fieldNameItem = listOfRows.FirstOrDefault(c => c.FieldName == "FieldName");
                            IBinaryDataListItem fieldValueItem = listOfRows.FirstOrDefault(c => c.FieldName == "FieldValue");
                            if(fieldNameItem != null && fieldValueItem != null){
                                newFieldsCollection.Add(new ActivityDTO(fieldNameItem.TheValue,fieldValueItem.TheValue,count));
                            }                            
                            count++;
                        }
                        _activity.Properties.SetValue("FieldsCollection", newFieldsCollection);                        
                        //Deletes the data list being kept on the server.
                        compiler.DeleteDataListByID(_datalistID);
                    }
                }
            }
        }

        public void CancelCallback()
        {
            IDataListCompiler compiler = CreateCompiler();

            if (_activity != null && _datalistID != null && compiler != null)
            {
                //Deletes the data list being kept on the server.
                compiler.DeleteDataListByID(_datalistID);
            }
        }
        #endregion Methods

        #region Private Methods

      
        #endregion Private Methods

    }
}
