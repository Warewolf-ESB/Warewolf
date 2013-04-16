using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Dev2.Interfaces;
using Dev2.Studio.Core.Wizards.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract;

namespace Dev2.Studio.Core.Wizards.CallBackHandlers
{
    [Export(typeof(IActivitySpecificSettingsWizardCallbackHandler<DsfCaseConvertActivity>))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DsfCaseConvertActivityWizCallback : IActivitySpecificSettingsWizardCallbackHandler<DsfCaseConvertActivity>
    {
        #region Fields

        private ModelItem _activity;
        private Guid _datalistID;
        private Func<IDataListCompiler> _createCompiler;

        #endregion Fields

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DsfCaseConvertActivityWizCallback " /> class.
        /// </summary>
        public DsfCaseConvertActivityWizCallback()
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
                IBinaryDataList wizardDataList = Dev2BinaryDataListFactory.CreateDataList();
                ErrorResultTO errors;
                wizardDataList = compiler.FetchBinaryDataList(_datalistID, out errors);
                if (wizardDataList != null && !errors.HasErrors())
                {
                    IList<ICaseConvertTO> newConvertCollection = new List<ICaseConvertTO>();

                    IList<IBinaryDataListEntry> recsets = wizardDataList.FetchRecordsetEntries();
                    IBinaryDataListEntry recset = recsets.FirstOrDefault(c => c.Namespace == "ConvertCollection");
                    if (recset != null)
                    {
                        int count = 0;
                        string error = string.Empty;
                        while (count < recset.ItemCollectionSize())
                        {
                            IList<IBinaryDataListItem> listOfRows = recset.FetchRecordAt(count + 1, out error);
                            IBinaryDataListItem stringToConvertItem = listOfRows.FirstOrDefault(c => c.FieldName == "StringToConvert");
                            IBinaryDataListItem convertTypeItem = listOfRows.FirstOrDefault(c => c.FieldName == "ConvertType");                            
                            IBinaryDataListItem resultItem = listOfRows.FirstOrDefault(c => c.FieldName == "Result");
                            if (stringToConvertItem != null && convertTypeItem != null && resultItem != null)
                            {
                                newConvertCollection.Add(new CaseConvertTO(stringToConvertItem.TheValue, convertTypeItem.TheValue, resultItem.TheValue, count));
                            }
                            count++;
                        }
                        Activity.Properties.SetValue("ConvertCollection", newConvertCollection);                        
                        //Deletes the data list being kept on the server.
                        compiler.DeleteDataListByID(_datalistID);
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


        #endregion Private Methods

    }
}
