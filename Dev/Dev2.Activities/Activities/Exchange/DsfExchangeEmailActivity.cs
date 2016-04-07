using System;
using System.Activities;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Exchange;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.Interfaces.ToolBase.ExchangeEmail;
using Dev2.Data;
using Dev2.Data.Enums;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Util;
using Microsoft.Exchange.WebServices.Data;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Security.Encryption;
using Warewolf.Storage;
using ExchangeService = Microsoft.Exchange.WebServices.Data.ExchangeService;

namespace Dev2.Activities.Exchange
{
    [ToolDescriptorInfo("Utility-SendMail", "Email", ToolType.Native, "8926E59B-18A3-03BB-A92F-6090C5C3EA80", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Exchange", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]
    public class DsfExchangeEmailActivity : DsfActivityAbstract<string>
    {
        public DsfExchangeEmailActivity()
            : base("Exchange Email")
        {
            To = string.Empty;
            Cc = string.Empty;
            Bcc = string.Empty;
            Password = string.Empty;
            Priority = enMailPriorityEnum.Normal;
            Subject = string.Empty;
            Attachments = string.Empty;
            Body = string.Empty;
        }


        #region Fields

        IDSFDataObject _dataObject;
        string _password;
        private IExchangeServiceFactory _exchangeServiceFactory;
        private ExchangeService _exchangeService;

        #endregion

        /// <summary>
        /// The property that holds all the conversions
        /// </summary>

        // ReSharper disable MemberCanBePrivate.Global
        public IExchangeSource SavedSource { get; set; }


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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        // ReSharper disable once MemberCanBePrivate.Global
        protected string DecryptedPassword
        {
            get
            {
                return DataListUtil.NotEncrypted(Password) ? Password : DpapiWrapper.Decrypt(Password);
            }
        }

        [FindMissing]
        public string To { get; set; }
        [FindMissing]
        public string Cc { get; set; }
        [FindMissing]
        public string Bcc { get; set; }

        // ReSharper disable MemberCanBePrivate.Global
        public enMailPriorityEnum Priority { get; set; }
        // ReSharper restore MemberCanBePrivate.Global
        [FindMissing]
        public string Subject { get; set; }
        [FindMissing]
        public string Attachments { get; set; }
        [FindMissing]
        public string Body { get; set; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public bool IsHtml { get; set; }

        /// <summary>
        /// The property that holds the result string the user enters into the "Result" box
        /// </summary>
        [FindMissing]
        public new string Result { get; set; }

        public IExchangeEmailSender EmailSender { get; set; }
        
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
                var runtimeSource = ResourceCatalog.GetResource<ExchangeSource>(dataObject.WorkspaceID, SavedSource.ResourceID);

                if (runtimeSource == null)
                {
                    dataObject.Environment.Errors.Add("Invalid Email Source");
                    return;
                }
                if (IsDebug)
                {
                    AddDebugInputItem(new DebugEvalResult(To, "To", dataObject.Environment, update));
                    AddDebugInputItem(new DebugEvalResult(Subject, "Subject", dataObject.Environment, update));
                    AddDebugInputItem(new DebugEvalResult(Body, "Body", dataObject.Environment, update));
                }
                var colItr = new WarewolfListIterator();

                var passwordItr = new WarewolfIterator(dataObject.Environment.Eval(DecryptedPassword, update));
                colItr.AddVariableToIterateOn(passwordItr);

                var toItr = new WarewolfIterator(dataObject.Environment.Eval(To, update));
                colItr.AddVariableToIterateOn(toItr);

                var ccItr = new WarewolfIterator(dataObject.Environment.Eval(Cc, update));
                colItr.AddVariableToIterateOn(ccItr);

                var bccItr = new WarewolfIterator(dataObject.Environment.Eval(Bcc, update));
                colItr.AddVariableToIterateOn(bccItr);

                var subjectItr = new WarewolfIterator(dataObject.Environment.Eval(Subject, update));
                colItr.AddVariableToIterateOn(subjectItr);

                var bodyItr = new WarewolfIterator(dataObject.Environment.Eval(Body ?? string.Empty, update));
                colItr.AddVariableToIterateOn(bodyItr);

                var attachmentsItr = new WarewolfIterator(dataObject.Environment.Eval(Attachments ?? string.Empty, update));
                colItr.AddVariableToIterateOn(attachmentsItr);

                if (!allErrors.HasErrors())
                {
                    while (colItr.HasMoreData())
                    {
                        ErrorResultTO errors;
                        var result = SendEmail(runtimeSource, colItr, toItr, ccItr, bccItr, subjectItr, bodyItr, attachmentsItr, out errors);
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

        // ReSharper disable TooManyArguments
        string SendEmail(IExchangeSource runtimeSource, IWarewolfListIterator colItr,IWarewolfIterator toItr, IWarewolfIterator ccItr, IWarewolfIterator bccItr, IWarewolfIterator subjectItr, IWarewolfIterator bodyItr, IWarewolfIterator attachmentsItr, out ErrorResultTO errors)
        // ReSharper restore TooManyArguments
        {
            InitializeService();

            errors = new ErrorResultTO();
            var toValue = colItr.FetchNextValue(toItr);
            var ccValue = colItr.FetchNextValue(ccItr);
            var bccValue = colItr.FetchNextValue(bccItr);
            var subjectValue = colItr.FetchNextValue(subjectItr);
            var bodyValue = colItr.FetchNextValue(bodyItr);
            var attachmentsValue = colItr.FetchNextValue(attachmentsItr);
            var mailMessage = new EmailMessage(_exchangeService) {Subject = subjectValue};

            AddToAddresses(toValue, mailMessage);

            mailMessage.Body = bodyValue;

            if (!String.IsNullOrEmpty(ccValue))
            {
                AddCcAddresses(ccValue, mailMessage);
            }
            if (!String.IsNullOrEmpty(bccValue))
            {
                AddBccAddresses(bccValue, mailMessage);
            }
            if (!String.IsNullOrEmpty(attachmentsValue))
            {
                AddAttachmentsValue(attachmentsValue, mailMessage);
            }
            string result;
            try
            {
                EmailSender = new ExchangeEmailSender(runtimeSource);

                EmailSender.Send(_exchangeService,mailMessage);

                result = "Success";
            }
            catch (Exception e)
            {
                result = "Failure";
                errors.AddError(e.Message);
            }

            return result;
        }

        private void InitializeService()
        {
            _exchangeServiceFactory = new ExchangeServiceFactory();
            _exchangeService = _exchangeServiceFactory.Create();
        }

        private List<string> GetSplitValues(string stringToSplit, char[] splitOn)
        {
            return stringToSplit.Split(splitOn, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        void AddAttachmentsValue(string attachmentsValue, EmailMessage mailMessage)
        {
            try
            {
                var attachements = GetSplitValues(attachmentsValue, new[] { ',', ';' });
                attachements.ForEach(s => mailMessage.Attachments.AddFileAttachment(s));
            }
            catch (Exception exception)
            {
                throw new Exception(string.Format("Attachments is not in the valid format: {0}", attachmentsValue), exception);
            }
        }

        void AddToAddresses(string toValue, EmailMessage mailMessage)
        {
            try
            {
                var toAddresses = GetSplitValues(toValue, new[] { ',', ';' });
                toAddresses.ForEach(s => mailMessage.ToRecipients.Add(s));
            }
            catch (FormatException exception)
            {
                throw new Exception(string.Format("To address is not in the valid format: {0}", toValue), exception);
            }
        }

        void AddCcAddresses(string toValue, EmailMessage mailMessage)
        {
            try
            {
                var ccAddresses = GetSplitValues(toValue, new[] { ',', ';' });
                ccAddresses.ForEach(s => mailMessage.CcRecipients.Add(s));
            }
            catch (FormatException exception)
            {
                throw new Exception(string.Format("CC address is not in the valid format: {0}", toValue), exception);
            }
        }

        void AddBccAddresses(string toValue, EmailMessage mailMessage)
        {
            try
            {
                var bccAddresses = GetSplitValues(toValue, new[] { ',', ';' });
                bccAddresses.ForEach(s => mailMessage.BccRecipients.Add(s));
            }
            catch (FormatException exception)
            {
                throw new Exception(string.Format("BCC address is not in the valid format: {0}", toValue), exception);
            }
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
                    if (t.Item1 == Password)
                    {
                        Password = t.Item2;
                    }
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
            if (updates != null)
            {
                var itemUpdate = updates.FirstOrDefault(tuple => tuple.Item1 == Result);
                if (itemUpdate != null)
                {
                    Result = itemUpdate.Item2;
                }
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
            return GetForEachItems(Password, To, Cc, Bcc, Subject, Attachments, Body);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion

    }
}
