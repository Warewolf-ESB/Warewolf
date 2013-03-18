using Dev2;
using Dev2.Activities;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Enums;
using Dev2.PathOperations;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{

    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide an activity that can read a folder's contents via FTP, FTPS and file system
    /// </summary>
    public class DsfFolderRead : DsfAbstractFileActivity, IPathInput
    {

        public DsfFolderRead()
            : base("Read Folder")
        {
            InputPath = string.Empty;
        }

        protected override IList<OutputTO> ExecuteConcreteAction(NativeActivityContext context, out ErrorResultTO allErrors)
        {
            allErrors = new ErrorResultTO();
            IList<OutputTO> outputs = new List<OutputTO>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            //IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            ErrorResultTO errors = new ErrorResultTO();
            Guid executionId = dataObject.DataListID;
            IDev2IteratorCollection colItr = Dev2ValueObjectFactory.CreateIteratorCollection();

            //get all the possible paths for all the string variables
            IBinaryDataListEntry inputPathEntry = compiler.Evaluate(executionId, enActionType.User, InputPath, false, out errors);
            allErrors.MergeErrors(errors);
            IDev2DataListEvaluateIterator inputItr = Dev2ValueObjectFactory.CreateEvaluateIterator(inputPathEntry);
            colItr.AddIterator(inputItr);

            IBinaryDataListEntry usernameEntry = compiler.Evaluate(executionId, enActionType.User, Username, false, out errors);
            allErrors.MergeErrors(errors);
            IDev2DataListEvaluateIterator unameItr = Dev2ValueObjectFactory.CreateEvaluateIterator(usernameEntry);
            colItr.AddIterator(unameItr);

            IBinaryDataListEntry passwordEntry = compiler.Evaluate(executionId, enActionType.User, Password, false, out errors);
            allErrors.MergeErrors(errors);
            IDev2DataListEvaluateIterator passItr = Dev2ValueObjectFactory.CreateEvaluateIterator(passwordEntry);
            colItr.AddIterator(passItr);

            if (dataObject.IsDebug)
            {
                AddDebugInputItem(InputPath, "Input Path", inputPathEntry, executionId);
                AddDebugInputItem(Username, "Username", usernameEntry, executionId);
                AddDebugInputItem(Password, "Password", passwordEntry, executionId);                
            }

            while (colItr.HasMoreData())
            {


                IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();
                IActivityIOPath IOpath = ActivityIOFactory.CreatePathFromString(colItr.FetchNextRow(inputItr).TheValue,
                                                                                colItr.FetchNextRow(unameItr).TheValue,
                                                                                colItr.FetchNextRow(passItr).TheValue,
                                                                                IsNotCertVerifiable);
                IActivityIOOperationsEndPoint endPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(IOpath);

                try
                {
                    IList<IActivityIOPath> ListOfDir = broker.ListDirectory(endPoint);
                    if (DataListUtil.IsValueRecordset(Result) && DataListUtil.GetRecordsetIndexType(Result) != enRecordsetIndexType.Numeric)
                    {
                        if (DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Star)
                        {
                            string recsetName = DataListUtil.ExtractRecordsetNameFromValue(Result);
                            string fieldName = DataListUtil.ExtractFieldNameFromValue(Result);

                            int indexToUpsertTo = 1;
                            foreach (IActivityIOPath pa in ListOfDir)
                            {
                                string FullRecsetName = DataListUtil.CreateRecordsetDisplayValue(recsetName, fieldName,
                                                                                                 indexToUpsertTo.ToString());
                                outputs.Add(DataListFactory.CreateOutputTO(DataListUtil.AddBracketsToValueIfNotExist(FullRecsetName), pa.Path));
                                indexToUpsertTo++;
                            }
                        }
                        else if (DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Blank)
                        {
                            foreach (IActivityIOPath pa in ListOfDir)
                            {
                                outputs.Add(DataListFactory.CreateOutputTO(Result, pa.Path));
                            }
                        }

                    }
                    else
                    {
                        string xmlList = string.Join(",", ListOfDir.Select(c => c.Path));
                        outputs.Add(DataListFactory.CreateOutputTO(Result));
                        outputs.Last().OutputStrings.Add(xmlList);
                    }
                }
                catch (Exception e)
                {
                    allErrors.AddError(e.Message);
                }
            }

            return outputs;

        }

        #region Properties

        /// <summary>
        /// Gets or sets the input path.
        /// </summary>
        [Inputs("Input Path")]
        public string InputPath
        {
            get;
            set;
        }

        #endregion Properties

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if (updates.Count == 1)
            {
                InputPath = updates[0].Item2;
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if (updates.Count == 1)
            {
                Result = updates[0].Item2;
            }
        }

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.Before, InputPath);
        }

        public override IList<DsfForEachItem> GetForEachOutputs(NativeActivityContext context)
        {
            return GetForEachItems(context, StateType.After, Result);
        }

        #endregion
    }
}
