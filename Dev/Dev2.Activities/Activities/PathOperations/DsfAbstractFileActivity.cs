using System;
using System.Activities;
using System.Collections.Generic;
using Dev2;
using Dev2.Common;
using Dev2.Data.Factories;
using Dev2.Data.PathOperations.Interfaces;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.Diagnostics;
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
    /// Purpose : To provide a base activity for all file activities to inherit from
    /// </summary>
    public abstract class DsfAbstractFileActivity : DsfActivityAbstract<string>, IPathAuth, IResult, IPathCertVerify
    {
        // Travis.Frisinger - 01.02.2013 : Bug 8579
        bool _isStandardUpsert = true;

        internal string DefferedReadFileContents = string.Empty;
        private string _username;
        private string _password;

        public DsfAbstractFileActivity(string displayName)
            : base(displayName)
        {
            Username = string.Empty;
            Password = string.Empty;
            Result = string.Empty;
        }

        protected override void OnExecute(NativeActivityContext context)
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();
            IList<OutputTO> outputs;
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();

            Guid dlID = dataObject.DataListID;
            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();

            IDev2DataListUpsertPayloadBuilder<IBinaryDataListEntry> toUpsertDeferred = Dev2DataListBuilderFactory.CreateBinaryDataListUpsertBuilder(true);
            IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
            // Process if no errors
            if(!errors.HasErrors())
            {
                try
                {
                    //Execute the concrete action for the specified activity
                    outputs = ExecuteConcreteAction(context, out errors);
                    allErrors.MergeErrors(errors);

                    if(outputs.Count > 0)
                    {
                        foreach(OutputTO output in outputs)
                        {
                            if(output.OutputStrings.Count > 0)
                            {
                                foreach(string value in output.OutputStrings)
                                {
                                    if(output.OutPutDescription == GlobalConstants.ErrorPayload)
                                    {
                                        errors.AddError(value);
                                    }
                                    else
                                    {
                                        if(_isStandardUpsert)
                                        {
                                            //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result
                                            foreach(var region in DataListCleaningUtils.SplitIntoRegions(output.OutPutDescription))
                                            {

                                                toUpsert.Add(region, value);
                                            }
                                        }
                                    }

                                }

                                if(_isStandardUpsert)
                                {
                                    toUpsert.FlushIterationFrame();
                                }
                                else
                                {
                                    // deferred read ;)
                                    toUpsertDeferred.FlushIterationFrame();
                                }
                            }
                        }

                        if(_isStandardUpsert)
                        {
                            compiler.Upsert(dlID, toUpsert, out errors);
                        }
                        else
                        {
                            // deferred read ;)
                            compiler.Upsert(dlID, toUpsertDeferred, out errors);
                        }
                        if(dataObject.IsDebug || dataObject.RemoteInvoke)
                        {
                            ErrorResultTO error;
                            if(!String.IsNullOrEmpty(Result))
                            {
                                string tmpRes = Result;
                                if(tmpRes.Contains("()."))
                                {
                                    tmpRes = tmpRes.Replace("().", "(*).");
                                }
                                IBinaryDataListEntry binaryDataListEntry = compiler.Evaluate(dlID, enActionType.User, tmpRes, true, out error);
                                allErrors.MergeErrors(error);
                                AddDebugOutputItem(tmpRes, string.Empty, binaryDataListEntry, dlID);
                            }
                        }

                        allErrors.MergeErrors(errors);
                    }

                }
                catch(Exception ex)
                {
                    allErrors.AddError(ex.Message);
                }
                finally
                {
                    // Handle Errors
                    if(allErrors.HasErrors())
                    {
                        DisplayAndWriteError("DsfFileActivity", allErrors);
                        compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                    }

                    if(dataObject.IsDebug || ServerLogger.ShouldLog(dataObject.ResourceID))
                    {
                        DispatchDebugState(context, StateType.Before);
                        DispatchDebugState(context, StateType.After);
                    }
                }
            }
        }


        /// <summary>
        /// Makes the deferred action.
        /// </summary>
        public void MakeDeferredAction(string deferredLoc)
        {
            // Do nothing ;)
        }

        /// <summary>
        /// Status : New
        /// Purpose : To provide an overidable concrete method to execute an activity's logic through
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="error">The error.</param>
        /// <returns></returns>
        protected abstract IList<OutputTO> ExecuteConcreteAction(NativeActivityContext context, out ErrorResultTO error);

        #region Properties

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        [Inputs("Password")]
        [FindMissing]
        public string Password
        {
            get { return _password; }
            set { _password = value; }
        }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        [Inputs("Username")]
        [FindMissing]
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        [Outputs("Result")]
        [FindMissing]
        public new string Result
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is not cert verifiable.
        /// </summary>
        [Inputs("Is Not Certificate Verifiable")]
        public bool IsNotCertVerifiable
        {
            get;
            set;
        }

        #endregion Properties

        #region Get Debug Inputs/Outputs

        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FetchResultsList();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {

            foreach (IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion Get Inputs/Outputs

        #region Internal Methods

        internal void AddDebugInputItem(string expression, string labelText, IBinaryDataListEntry valueEntry, Guid executionId)
        {
            DebugItem itemToAdd = new DebugItem();

            if (!string.IsNullOrWhiteSpace(labelText))
            {
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = labelText });
            }           

            if (valueEntry != null)
            {
                itemToAdd.AddRange(CreateDebugItemsFromEntry(expression, valueEntry, executionId, enDev2ArgumentType.Input));
            }

            _debugInputs.Add(itemToAdd);
        }

        internal void AddDebugOutputItem(string expression, string labelText, IBinaryDataListEntry valueEntry, Guid executionId)
        {
            DebugItem itemToAdd = new DebugItem();

            if (!string.IsNullOrWhiteSpace(labelText))
            {
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = labelText });
            }

            if (valueEntry != null)
            {
                itemToAdd.AddRange(CreateDebugItemsFromEntry(expression, valueEntry, executionId, enDev2ArgumentType.Output));
            }

            _debugOutputs.Add(itemToAdd);
        }

        #endregion

        protected void AddDebugInputItemUserNamePassword(Guid executionId, IBinaryDataListEntry usernameEntry)
        {
            AddDebugInputItem(Username, "Username", usernameEntry, executionId);
            AddDebugInputItemPassword("Password", Password);
        }

        protected void AddDebugInputItemDestinationUsernamePassword(Guid executionId, IBinaryDataListEntry destUsernameEntry, string destinationPassword, string userName)
        {
            AddDebugInputItem(userName, "Username", destUsernameEntry, executionId);
            AddDebugInputItemPassword("Password", destinationPassword);
        }

        protected void AddDebugInputItemPassword(string label, string password)
        {
            var itemToAdd = new DebugItem();
            itemToAdd.ResultsList.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = label });
            itemToAdd.ResultsList.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = GetBlankedOutPassword(password) });
            _debugInputs.Add(itemToAdd);
        }

        protected void AddDebugInputItemOverwrite(Guid executionId, bool overWrite)
        {
            var itemToAdd = new DebugItem();
            itemToAdd.ResultsList.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = "Overwrite" });
            itemToAdd.ResultsList.Add(new DebugItemResult { Type = DebugItemResultType.Value, Value = overWrite ? "True" : "False" });
            _debugInputs.Add(itemToAdd);
        }

        static string GetBlankedOutPassword(string password)
        {
            return "".PadRight((password ?? "").Length, '*');
        }
    }
}
