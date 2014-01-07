using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Studio.Core.AppResources.ExtensionMethods;
using Dev2.Studio.Core.Wizards.Interfaces;
using System;
using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Studio.Core.Wizards.CallBackHandlers
{
    // Old Wizard Functionlity
    //[Export(typeof(IActivitySpecificSettingsWizardCallbackHandler<DsfDataSplitActivity>))]
    //[PartCreationPolicy(CreationPolicy.NonShared)]
    public class DsfDataSplitActivityWizCallback : IActivitySpecificSettingsWizardCallbackHandler<DsfDataSplitActivity>
    {
        #region Fields

        private ModelItem _activity;
        private Guid _datalistID;
        private Func<IDataListCompiler> _createCompiler;

        #endregion Fields

        #region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="DsfDataSplitActivityWizCallback" /> class.
        /// </summary>
        public DsfDataSplitActivityWizCallback()
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
                if(_createCompiler == null) return new Func<IDataListCompiler>(() => null);
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

            if(_activity != null && _datalistID != null && compiler != null)
            {
                IBinaryDataList wizardDataList;
                ErrorResultTO errors;
                wizardDataList = compiler.FetchBinaryDataList(_datalistID, out errors);
                //****DELETE CODE BELOW
                //IBinaryDataList result = Dev2BinaryDataListFactory.CreateDataList();
                //string error;
                //string recordsetName = "ResultsCollection";
                //result.TryCreateScalarTemplate(string.Empty, "SourceString", string.Empty, true, out error);
                //result.TryCreateScalarValue("SourceStringTestData", "SourceString", out error);
                //result.TryCreateScalarTemplate(string.Empty, "ReverseOrder", string.Empty, true, out error);
                //result.TryCreateScalarValue("True", "ReverseOrder", out error);
                //result.TryCreateRecordsetTemplate(recordsetName, string.Empty, new List<Dev2Column>() { DataListFactory.CreateDev2Column("SplitType", string.Empty), DataListFactory.CreateDev2Column("At", string.Empty), DataListFactory.CreateDev2Column("Include", string.Empty), DataListFactory.CreateDev2Column("Result", string.Empty) }, true, out error);


                //result.TryCreateRecordsetValue("Index", "SplitType", recordsetName, 0, out error);
                //result.TryCreateRecordsetValue("2", "At", recordsetName, 0, out error);
                //result.TryCreateRecordsetValue("True", "Include", recordsetName, 0, out error);
                //result.TryCreateRecordsetValue("[[Out1]]", "Result", recordsetName, 0, out error);

                //result.TryCreateRecordsetValue("Chars", "SplitType", recordsetName, 1, out error);
                //result.TryCreateRecordsetValue("s", "At", recordsetName, 1, out error);
                //result.TryCreateRecordsetValue("True", "Include", recordsetName, 1, out error);
                //result.TryCreateRecordsetValue("[[Out2]]", "Result", recordsetName, 1, out error);
                //wizardDataList = result;
                //****

                if(wizardDataList != null && !errors.HasErrors())
                {
                    IList<DataSplitDTO> newResultsCollection = new List<DataSplitDTO>();

                    IList<IBinaryDataListEntry> recsets = wizardDataList.FetchRecordsetEntries();
                    IList<IBinaryDataListEntry> scalarEntries = wizardDataList.FetchScalarEntries();
                    IList<IBinaryDataListItem> scalars = new List<IBinaryDataListItem>();
                    foreach(IBinaryDataListEntry item in scalarEntries)
                    {
                        scalars.Add(item.FetchScalar());
                    }
                    IBinaryDataListItem sourceStringItem = scalars.FirstOrDefault(c => c.FieldName == "SourceString");
                    IBinaryDataListItem reverseOrderItem = scalars.FirstOrDefault(c => c.FieldName == "ReverseOrder");

                    if(sourceStringItem != null)
                    {
                        Activity.Properties.SetValue("SourceString", sourceStringItem.TheValue);
                    }
                    if(reverseOrderItem != null)
                    {
                        string _reverseOrder = reverseOrderItem.TheValue;
                        if(_reverseOrder.Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            Activity.Properties.SetValue("ReverseOrder", true);
                        }
                        else
                        {
                            Activity.Properties.SetValue("ReverseOrder", false);
                        }

                    }

                    IBinaryDataListEntry recset = recsets.FirstOrDefault(c => c.Namespace == "ResultsCollection");
                    if(recset != null)
                    {
                        int count = 0;
                        while(count < recset.ItemCollectionSize())
                        {
                            string error = string.Empty;
                            IList<IBinaryDataListItem> listOfRows = recset.FetchRecordAt(count + 1, out error);
                            IBinaryDataListItem splitTypeItem = listOfRows.FirstOrDefault(c => c.FieldName == "SplitType");
                            IBinaryDataListItem atItem = listOfRows.FirstOrDefault(c => c.FieldName == "At");
                            IBinaryDataListItem includeItem = listOfRows.FirstOrDefault(c => c.FieldName == "Include");
                            IBinaryDataListItem resultItem = listOfRows.FirstOrDefault(c => c.FieldName == "Result");
                            if(splitTypeItem != null && atItem != null && includeItem != null && resultItem != null)
                            {
                                newResultsCollection.Add(new DataSplitDTO(resultItem.TheValue, splitTypeItem.TheValue, atItem.TheValue, count + 1, Convert.ToBoolean(includeItem.TheValue)));
                            }
                            count++;
                        }
                        Activity.Properties.SetValue("ResultsCollection", newResultsCollection);
                        //Deletes the data list being kept on the server.
                        compiler.DeleteDataListByID(_datalistID);
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


        #endregion Private Methods

    }
}
