using System;
using System.Activities;
using System.Collections.Generic;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common.ExtMethods;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.PathOperations;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide an activity that can write a file and its contents via FTP, FTPS and file system
    /// </summary>
    public class DsfFileWrite : DsfAbstractFileActivity, IFileWrite, IPathOutput, IPathOverwrite
    {

        public DsfFileWrite()
            : base("Write File")
        {
            OutputPath = string.Empty;
            FileContents = string.Empty;
        }

        protected override IList<OutputTO> ExecuteConcreteAction(NativeActivityContext context, out ErrorResultTO allErrors)
        {
            IList<OutputTO> outputs = new List<OutputTO>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();

            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            allErrors = new ErrorResultTO();
            ErrorResultTO errors;
            Guid executionId = dataObject.DataListID;
            IDev2IteratorCollection colItr = Dev2ValueObjectFactory.CreateIteratorCollection();

            //get all the possible paths for all the string variables
            IBinaryDataListEntry inputPathEntry = compiler.Evaluate(executionId, enActionType.User, OutputPath, false, out errors);
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

            IBinaryDataListEntry contentsEntry = compiler.Evaluate(executionId, enActionType.User, FileContents, false, out errors);
            allErrors.MergeErrors(errors);
            IDev2DataListEvaluateIterator contentItr = Dev2ValueObjectFactory.CreateEvaluateIterator(contentsEntry);
            colItr.AddIterator(contentItr);

            outputs.Add(DataListFactory.CreateOutputTO(Result));

            if(dataObject.IsDebugMode())
            {
                AddDebugInputItem(OutputPath, "Output Path", inputPathEntry, executionId);
                AddDebugInputItem(new DebugItemStaticDataParams(GetMethod(), "Method"));
                AddDebugInputItemUserNamePassword(executionId, usernameEntry);
                AddDebugInputItem(FileContents, "File Contents", contentsEntry, executionId);
            }

            while(colItr.HasMoreData())
            {
                IActivityOperationsBroker broker = ActivityIOFactory.CreateOperationsBroker();
                var writeType = GetCorrectWriteType();
                Dev2PutRawOperationTO putTO = ActivityIOFactory.CreatePutRawOperationTO(writeType, colItr.FetchNextRow(contentItr).TheValue);
                IActivityIOPath IOpath = ActivityIOFactory.CreatePathFromString(colItr.FetchNextRow(inputItr).TheValue,
                                                                                colItr.FetchNextRow(unameItr).TheValue,
                                                                                colItr.FetchNextRow(passItr).TheValue,
                                                                                true);
                IActivityIOOperationsEndPoint endPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(IOpath);

                try
                {
                    string result = broker.PutRaw(endPoint, putTO);
                    outputs[0].OutputStrings.Add(result);
                }
                catch(Exception e)
                {
                    outputs[0].OutputStrings.Add("Failure");
                    allErrors.AddError(e.Message);
                }
            }

            return outputs;
        }

        WriteType GetCorrectWriteType()
        {
            if(AppendBottom)
            {
                return WriteType.AppendBottom;
            }
            if(AppendTop)
            {
                return WriteType.AppendTop;
            }
            if(Overwrite)
            {
                return WriteType.Overwrite;
            }
            return WriteType.AppendBottom;
        }

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DsfFileWrite" /> is append.
        /// </summary>
        [Inputs("Append")]        
        public bool Append
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the file contents.
        /// </summary>
        [Inputs("File Contents")]
        [FindMissing]
        public string FileContents
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the output path.
        /// </summary>
        [Inputs("Output Path")]
        [FindMissing]
        public string OutputPath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DsfFileWrite" /> is overwrite.
        /// </summary>
        [Inputs("Overwrite")]
        public bool Overwrite
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DsfFileWrite" /> is append top.
        /// </summary>
        [Inputs("Append Top")]
        public bool AppendTop
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DsfFileWrite" /> is append bottom.
        /// </summary>
        [Inputs("Append Bottom")]
        public bool AppendBottom
        {
            get;
            set;
        }

        #endregion Properties

        #region Private Methods


        private string GetMethod()
        {
            return GetCorrectWriteType().GetDescription();
        }

        #endregion

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                foreach(Tuple<string, string> t in updates)
                {
                    if(t.Item1 == OutputPath)
                    {
                        OutputPath = t.Item2;
                    }

                    if(t.Item1 == FileContents)
                    {
                        FileContents = t.Item2;
                    }
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null && updates.Count == 1)
            {
                Result = updates[0].Item2;
            }
        }


        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(OutputPath, FileContents);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion
    }
}
