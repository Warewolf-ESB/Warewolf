using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Data;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

namespace Dev2.Activities.Exchange
{
    [ToolDescriptorInfo("Utility-SendMail", "Exchange Send", ToolType.Native, "8926E59B-18A3-03BB-A92F-6090C5C3EA80", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Email", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Email_Exchange_Send")]
    public class DsfExchangeEmailActivity : DsfActivityAbstract<string>
    {
        private readonly IDev2EmailSender _emailSender;

        public DsfExchangeEmailActivity()
            : this(new Dev2EmailSender())
        {
            To = string.Empty;
            Cc = string.Empty;
            Bcc = string.Empty;
            Subject = string.Empty;
            Attachments = string.Empty;
            Body = string.Empty;
        }

        public DsfExchangeEmailActivity(IDev2EmailSender emailSender)
            : base("Exchange Email")
        {
            _emailSender = emailSender;
        }

        #region Fields

        IDSFDataObject _dataObject;

        #endregion

        // ReSharper disable MemberCanBePrivate.Global
        public IExchangeSource SavedSource { get; set; }

        [FindMissing]
        public string To { get; set; }
        [FindMissing]
        public string Cc { get; set; }
        [FindMissing]
        public string Bcc { get; set; }

        // ReSharper restore MemberCanBePrivate.Global
        [FindMissing]
        public string Subject { get; set; }
        [FindMissing]
        public string Attachments { get; set; }
        [FindMissing]
        public string Body { get; set; }


        /// <summary>
        /// The property that holds the result string the user enters into the "Result" box
        /// </summary>
        [FindMissing]
        public new string Result { get; set; }


        public override List<string> GetOutputs()
        {
            return new List<string> { Result };
        }


        #region Overrides of DsfNativeActivity<string>

        private bool IsDebug
        {
            get
            {
                if (_dataObject == null)
                {
                    return false;
                }
                return _dataObject.IsDebugMode();
            }
        }
        /// <summary>
        /// When overridden runs the activity's execution logic
        /// </summary>
        /// <param name="context">The context to be used.</param>
        // ReSharper disable MethodTooLong
        protected override void OnExecute(NativeActivityContext context)
        // ReSharper restore MethodTooLong
        {
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();

            ExecuteTool(dataObject, 0);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            _dataObject = dataObject;

            ErrorResultTO allErrors = new ErrorResultTO();
            int indexToUpsertTo = 0;

            InitializeDebug(dataObject);
            try
            {
                IExchange runtimeSource = ResourceCatalog.GetResource<ExchangeSource>(dataObject.WorkspaceID, SavedSource.ResourceID);

                if (runtimeSource == null)
                {
                    dataObject.Environment.Errors.Add(ErrorResource.InvalidEmailSource);
                    return;
                }
                if (IsDebug)
                {
                    AddDebugInputItem(new DebugEvalResult(To, "To", dataObject.Environment, update));
                    AddDebugInputItem(new DebugEvalResult(Subject, "Subject", dataObject.Environment, update));
                    AddDebugInputItem(new DebugEvalResult(Body, "Body", dataObject.Environment, update));
                }
                var colItr = new WarewolfListIterator();

                var toItr = new WarewolfIterator(dataObject.Environment.Eval(To, update));
                colItr.AddVariableToIterateOn(toItr);

                var ccItr = new WarewolfIterator(dataObject.Environment.Eval(Cc, update));
                colItr.AddVariableToIterateOn(ccItr);



                var bccItr = new WarewolfIterator(dataObject.Environment.Eval(Bcc, update));
                colItr.AddVariableToIterateOn(bccItr);

                var subjectItr = new WarewolfIterator(dataObject.Environment.Eval(Subject, update));
                colItr.AddVariableToIterateOn(subjectItr);

                var bodyItr = new WarewolfIterator(dataObject.Environment.Eval(Body, update));
                colItr.AddVariableToIterateOn(bodyItr);

                var attachmentsItr = new WarewolfIterator(dataObject.Environment.Eval(Attachments ?? string.Empty, update));
                colItr.AddVariableToIterateOn(attachmentsItr);

                if (!allErrors.HasErrors())
                {
                    while (colItr.HasMoreData())
                    {
                        ErrorResultTO errors;
                        var result = _emailSender.SendEmail(runtimeSource, colItr, toItr, ccItr, bccItr, subjectItr, bodyItr, attachmentsItr, out errors);
                        allErrors.MergeErrors(errors);
                        if (!allErrors.HasErrors())
                        {
                            indexToUpsertTo = UpsertResult(indexToUpsertTo, dataObject.Environment, result, update);
                        }
                    }
                    if (IsDebug && !allErrors.HasErrors())
                    {
                        if (!string.IsNullOrEmpty(Result))
                        {
                            AddDebugOutputItem(new DebugEvalResult(Result, "", dataObject.Environment, update));
                        }
                    }
                }
                else
                {
                    if (IsDebug)
                    {
                        AddDebugInputItem(To, "To");
                        AddDebugInputItem(Subject, "Subject");
                        AddDebugInputItem(Body, "Body");
                    }
                }
            }
            catch (Exception e)
            {
                Dev2Logger.Error("DSFEmail", e);
                allErrors.AddError(e.Message);
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
                    UpsertResult(indexToUpsertTo, dataObject.Environment, null, update);
                    if (dataObject.IsDebugMode())
                    {
                        AddDebugOutputItem(new DebugItemStaticDataParams("", Result, ""));
                    }
                    DisplayAndWriteError("DsfSendEmailActivity", allErrors);
                }
                if (dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }
            }
        }

        void AddDebugInputItem(string value, string label)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(label))
            {
                return;
            }

            AddDebugInputItem(DataListUtil.IsEvaluated(value) ? new DebugItemStaticDataParams("", value, label) : new DebugItemStaticDataParams(value, label));
        }

        private int UpsertResult(int indexToUpsertTo, IExecutionEnvironment environment, string result, int update)
        {
            string expression;
            if (DataListUtil.IsValueRecordset(Result) && DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Star)
            {
                expression = Result.Replace(GlobalConstants.StarExpression, indexToUpsertTo.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                expression = Result;
            }
            //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result
            foreach (var region in DataListCleaningUtils.SplitIntoRegions(expression))
            {
                environment.Assign(region, result, update);
                indexToUpsertTo++;
            }
            return indexToUpsertTo;
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.StaticActivity;
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            if (updates != null)
            {
                foreach (Tuple<string, string> t in updates)
                {
                    if (t.Item1 == To)
                    {
                        To = t.Item2;
                    }
                    if (t.Item1 == Cc)
                    {
                        Cc = t.Item2;
                    }
                    if (t.Item1 == Bcc)
                    {
                        Bcc = t.Item2;
                    }
                    if (t.Item1 == Subject)
                    {
                        Subject = t.Item2;
                    }
                    if (t.Item1 == Attachments)
                    {
                        Attachments = t.Item2;
                    }
                    if (t.Item1 == Body)
                    {
                        Body = t.Item2;
                    }

                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            var itemUpdate = updates?.FirstOrDefault(tuple => tuple.Item1 == Result);
            if (itemUpdate != null)
            {
                Result = itemUpdate.Item2;
            }
        }

        #region Overrides of DsfNativeActivity<string>

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList, int update)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
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

        #endregion

        #endregion

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(To, Cc, Bcc, Subject, Attachments, Body);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion
    }
}
