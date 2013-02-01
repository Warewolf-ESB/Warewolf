using Dev2;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.Diagnostics;
using Dev2.PathOperations;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide a base activity for all file activities to inherit from
    /// </summary>
    public abstract class DsfAbstractFileActivity : DsfActivityAbstract<string>, IPathAuth, IResult, IPathCertVerify
    {

        private string preExecute = string.Empty;

        public DsfAbstractFileActivity(string displayName)
            : base(displayName)
        {
            Username = string.Empty;
            Password = string.Empty;
            Result = string.Empty;
        }

        protected override void OnExecute(NativeActivityContext context)
        {

            IList<OutputTO> outputs = new List<OutputTO>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();

            IDataListCompiler compiler = context.GetExtension<IDataListCompiler>();

            Guid dlID = dataObject.DataListID;
            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            Guid executionId = DataListExecutionID.Get(context);

            IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);

            // Process if no errors
            if (!errors.HasErrors())
            {
                try
                {
                    //Execute the concrete action for the specified activity
                    outputs = ExecuteConcreteAction(context, out errors);
                    allErrors.MergeErrors(errors);
                    if (outputs.Count > 0)
                    {
                        //IList<string> expressionList = new List<string>();
                        //IList<string> valueList = new List<string>();
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
                                        toUpsert.Add(output.OutPutDescription, value);
                                    }

                                }

                                toUpsert.FlushIterationFrame();
                            }
                        }

                        compiler.Upsert(executionId, toUpsert, out errors);
                        allErrors.MergeErrors(errors);
                        compiler.Shape(executionId, enDev2ArgumentType.Output, OutputMapping, out errors);
                        allErrors.MergeErrors(errors);

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
                        DisplayAndWriteError("DsfFileActivity", allErrors);
                        compiler.UpsertSystemTag(dataObject.DataListID, enSystemTag.Error, allErrors.MakeDataListReady(), out errors);
                    }
                }
            }
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
        public string Password
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        [Inputs("Username")]
        public string Username
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        [Outputs("Result")]
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

        public override IList<IDebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            IList<IDebugItem> results = new List<IDebugItem>();

            var props = this.GetType().GetProperties();

            foreach (PropertyInfo propertyInfo in props)
            {
                Inputs[] inputAttribs = propertyInfo.GetCustomAttributes(typeof(Inputs), true).OfType<Inputs>().ToArray();

                if (inputAttribs.Length == 0)
                {
                    continue;
                }

                string labelVal = string.IsNullOrEmpty(inputAttribs[0].UserVisibleName) ? string.Empty : (" " + inputAttribs[0].UserVisibleName + " ");
                var val = propertyInfo.GetValue(this, null) as String;

                DebugItem itemToAdd = new DebugItem();
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = labelVal });
                if (!string.IsNullOrEmpty(val))
                {
                    itemToAdd.AddRange(CreateDebugItems(val, dataList));
                }
                results.Add(itemToAdd);
            }

            return results;
        }

        public override IList<IDebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            IList<IDebugItem> results = new List<IDebugItem>();

            var props = this.GetType().GetProperties();

            foreach (PropertyInfo propertyInfo in props)
            {
                Outputs[] outputAttribs = propertyInfo.GetCustomAttributes(typeof(Outputs), true).OfType<Outputs>().ToArray();

                if (outputAttribs.Length == 0)
                {
                    continue;
                }

                var val = propertyInfo.GetValue(this, null) as String;

                DebugItem itemToAdd = new DebugItem();
                if (!string.IsNullOrEmpty(val))
                {
                    itemToAdd.AddRange(CreateDebugItems(val, dataList));
                }
                results.Add(itemToAdd);
            }

            return results;
        }

        #endregion Get Inputs/Outputs
    }
}
