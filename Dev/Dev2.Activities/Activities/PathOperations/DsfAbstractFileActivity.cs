/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.PathOperations;
using Dev2.Data.PathOperations.Interfaces;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.PathOperations;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Security.Encryption;
using Warewolf.Storage;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
// ReSharper disable ConvertToAutoProperty
{
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide a base activity for all file activities to inherit from
    /// </summary>
    public abstract class DsfAbstractFileActivity : DsfActivityAbstract<string>, IPathAuth, IResult, IPathCertVerify
    {

        private string _username;
        private string _password;

        protected DsfAbstractFileActivity(string displayName)
            : base(displayName)
        {
            Username = string.Empty;
            Password = string.Empty;
            Result = string.Empty;
            PrivateKeyFile = string.Empty;
        }

        protected override void OnExecute(NativeActivityContext context)
        {
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();

            // Process if no errors

            if (dataObject.IsDebugMode())
            {
                InitializeDebug(dataObject);
            }

            if (!errors.HasErrors())
            {
                try
                {
                    //Execute the concrete action for the specified activity
                    IList<OutputTO> outputs = ExecuteConcreteAction(dataObject, out errors, update);

                    allErrors.MergeErrors(errors);

                    if (outputs.Count > 0)
                    {
                        foreach (OutputTO output in outputs)
                        {
                            if (output.OutputStrings.Count > 0)
                            {
                                foreach (string value in output.OutputStrings)
                                {
                                    if (output.OutPutDescription == GlobalConstants.ErrorPayload)
                                    {
                                        errors.AddError(value);
                                    }
                                    else
                                    {
                                        foreach (var region in DataListCleaningUtils.SplitIntoRegions(output.OutPutDescription))
                                        {                                            
                                            dataObject.Environment.Assign(region, value, update);
                                        }
                                    }
                                }
                            }
                        }
                        allErrors.MergeErrors(errors);
                    }
                    else
                    {
                        foreach (var region in DataListCleaningUtils.SplitIntoRegions(Result))
                        {
                            dataObject.Environment.Assign(region, "",update);
                        }
                    }
                    if (dataObject.IsDebugMode())
                        {
                        if (!String.IsNullOrEmpty(Result))
                            {
                                AddDebugOutputItem(new DebugEvalResult(Result, "", dataObject.Environment, update));
                            }
                    }
                }
                catch (Exception ex)
                {
                    allErrors.AddError(ex.Message);
                }
                finally
                {
                    // Handle Errors
                    if (allErrors.HasErrors())
                    {
                        foreach (var err in allErrors.FetchErrors())
                        {
                            dataObject.Environment.Errors.Add(err);
                        }
                        foreach (var region in DataListCleaningUtils.SplitIntoRegions(Result))
                        {
                            dataObject.Environment.Assign(region, "", update);
                        }
                    }

                    if (dataObject.IsDebugMode())
                    {
                        DispatchDebugState(dataObject, StateType.Before, update);
                        DispatchDebugState(dataObject, StateType.After, update);
                    }
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)] 
        protected string DecryptedPassword => DataListUtil.NotEncrypted(Password) ? Password : DpapiWrapper.Decrypt(Password);

        /// <summary>
        /// Status : New
        /// Purpose : To provide an overidable concrete method to execute an activity's logic through
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="error">The error.</param>
        /// <param name="update"></param>
        /// <returns></returns>
        protected abstract IList<OutputTO> ExecuteConcreteAction(IDSFDataObject context, out ErrorResultTO error, int update);

        #region Properties

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        [Inputs("Password")]
        [FindMissing]
        public string Password
        {
            get { return _password; }
            set
            {
                if (DataListUtil.ShouldEncrypt(value))
                {
                    try
                    {
                        _password = DpapiWrapper.Encrypt(value);
                    }
                    catch (Exception)
                    {
                        _password = value;
                    }
                }
                else
                {
                    _password = value;
                }
            }
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

        [Inputs("PrivateKeyFile")]
        [FindMissing]
        public string PrivateKeyFile { get; set; }

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

        public override List<string> GetOutputs()
        {
            return new List<string> { Result };
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

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList, int update)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FetchResultsList();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList, int update)
        {

            foreach (IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion Get Inputs/Outputs

        #region Internal Methods

        internal void AddDebugInputItem(string expression, string labelText, IExecutionEnvironment environment, int update)
        {
            AddDebugInputItem(new DebugEvalResult(expression, labelText, environment, update));
        }

        #endregion

        protected void AddDebugInputItemUserNamePassword(IExecutionEnvironment environment, int update)
        {
            AddDebugInputItem(new DebugEvalResult(Username, "Username", environment, update));
            AddDebugInputItemPassword("Password", Password);
        }

        protected void AddDebugInputItemDestinationUsernamePassword(IExecutionEnvironment environment, string destinationPassword, string userName, int update)
        {
            AddDebugInputItem(new DebugEvalResult(userName, "Destination Username", environment, update));
            AddDebugInputItemPassword("Destination Password", destinationPassword);
        }

        protected void AddDebugInputItemPassword(string label, string password)
        {
            AddDebugInputItem(new DebugItemStaticDataParams(GetBlankedOutPassword(password), label));
        }

        static string GetBlankedOutPassword(string password)
        {
            return "".PadRight((password ?? "").Length, '*');
        }
    }
}
