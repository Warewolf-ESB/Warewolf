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
using Unlimited.Framework;


using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Studio.Core.Wizards.CallBackHandlers
{
    [Export(typeof(IActivitySpecificSettingsWizardCallbackHandler<DsfBaseConvertActivity>))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DsfBaseConvertActivityWizCallback : IActivitySpecificSettingsWizardCallbackHandler<DsfBaseConvertActivity>
    {
        #region Fields

        private ModelItem _activity;
        private Guid _datalistID;
        private Func<IDataListCompiler> _createCompiler;

        #endregion Fields

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DsfBaseConvertActivityWizCallback " /> class.
        /// </summary>
        public DsfBaseConvertActivityWizCallback()
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
            string error = string.Empty;
            IDataListCompiler compiler = CreateCompiler();

            if (_activity != null && _datalistID != null && compiler != null)
            {
                IBinaryDataList wizardDataList;
                ErrorResultTO errors;
                wizardDataList = compiler.FetchBinaryDataList(_datalistID, out errors);
                if (wizardDataList != null && !errors.HasErrors())
                {
                    IList<BaseConvertTO> newConvertCollection = new List<BaseConvertTO>();

                    IList<IBinaryDataListEntry> recsets = wizardDataList.FetchRecordsetEntries();
                    IBinaryDataListEntry recset = recsets.FirstOrDefault(c => c.Namespace == "ConvertCollection");
                    if (recset != null)
                    {
                        int count = 0;
                        while (count < recset.ItemCollectionSize())
                        {
                            IList<IBinaryDataListItem> listOfRows = recset.FetchRecordAt(count + 1, out error);
                            IBinaryDataListItem fromExpressionItem = listOfRows.FirstOrDefault(c => c.FieldName == "FromExpression");
                            IBinaryDataListItem fromTypeItem = listOfRows.FirstOrDefault(c => c.FieldName == "FromType");
                            IBinaryDataListItem toTypeItem = listOfRows.FirstOrDefault(c => c.FieldName == "ToType");
                            IBinaryDataListItem toExpressionItem = listOfRows.FirstOrDefault(c => c.FieldName == "Result");
                            if (fromExpressionItem != null && fromTypeItem != null && toTypeItem != null && toExpressionItem != null)
                            {
                                newConvertCollection.Add(new BaseConvertTO(fromExpressionItem.TheValue, fromTypeItem.TheValue, toTypeItem.TheValue, toExpressionItem.TheValue, count));
                            }
                            count++;
                        }
                        Activity.Properties.SetValue("ConvertCollection", newConvertCollection);

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
                compiler.DeleteDataListByID(_datalistID);
            }
        }
        #endregion Methods

        #region Private Methods


        #endregion Private Methods

    }
}
