
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data.Enums;
using Dev2.Data.Factories;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities
{
    public class DsfSendEmailActivity : DsfActivityAbstract<string>
    {
        #region Fields

        IEmailSender _emailSender;
        IDSFDataObject _dataObject;

        #endregion

        /// <summary>
        /// The property that holds all the conversions
        /// </summary>

        public EmailSource SelectedEmailSource { get; set; }
        [FindMissing]
        public string FromAccount { get; set; }
        [FindMissing]
        public string Password { get; set; }
        [FindMissing]
        public string To { get; set; }
        [FindMissing]
        public string Cc { get; set; }
        [FindMissing]
        public string Bcc { get; set; }

        public enMailPriorityEnum Priority { get; set; }
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

        public IEmailSender EmailSender
        {
            get
            {
                return _emailSender ?? (_emailSender = new EmailSender());
            }
            set
            {
                _emailSender = value;
            }
        }

        #region Ctor

        public DsfSendEmailActivity()
            : base("Email")
        {
            FromAccount = string.Empty;
            To = string.Empty;
            Cc = string.Empty;
            Bcc = string.Empty;
            Password = string.Empty;
            Priority = enMailPriorityEnum.Normal;
            Subject = string.Empty;
            Attachments = string.Empty;
            Body = string.Empty;

        }

        #endregion


        #region Overrides of DsfNativeActivity<string>

        private bool IsDebug
        {
            get
            {
                if(_dataObject == null)
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
        protected override void OnExecute(NativeActivityContext context)
        {
            _debugInputs = new List<DebugItem>();
            _debugOutputs = new List<DebugItem>();
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            _dataObject = dataObject;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            Guid dlId = dataObject.DataListID;
            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors;
            Guid executionId = DataListExecutionID.Get(context);
            int indexToUpsertTo = 0;
            IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);
            toUpsert.IsDebug = dataObject.IsDebugMode();

            InitializeDebug(dataObject);
            try
            {
                var colItr = Dev2ValueObjectFactory.CreateIteratorCollection();

                IBinaryDataListEntry fromAccountEntry = compiler.Evaluate(dlId, enActionType.User, FromAccount ?? string.Empty, false, out errors);

                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator fromAccountItr = Dev2ValueObjectFactory.CreateEvaluateIterator(fromAccountEntry);
                colItr.AddIterator(fromAccountItr);

                IBinaryDataListEntry passwordEntry = compiler.Evaluate(dlId, enActionType.User, Password, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator passwordItr = Dev2ValueObjectFactory.CreateEvaluateIterator(passwordEntry);
                colItr.AddIterator(passwordItr);

                IBinaryDataListEntry toEntry = compiler.Evaluate(dlId, enActionType.User, To, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator toItr = Dev2ValueObjectFactory.CreateEvaluateIterator(toEntry);
                colItr.AddIterator(toItr);

                IBinaryDataListEntry ccEntry = compiler.Evaluate(dlId, enActionType.User, Cc ?? string.Empty, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator ccItr = Dev2ValueObjectFactory.CreateEvaluateIterator(ccEntry);
                colItr.AddIterator(ccItr);

                IBinaryDataListEntry bccEntry = compiler.Evaluate(dlId, enActionType.User, Bcc ?? string.Empty, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator bccItr = Dev2ValueObjectFactory.CreateEvaluateIterator(bccEntry);
                colItr.AddIterator(bccItr);

                IBinaryDataListEntry subjectEntry = compiler.Evaluate(dlId, enActionType.User, Subject ?? string.Empty, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator subjectItr = Dev2ValueObjectFactory.CreateEvaluateIterator(subjectEntry);
                colItr.AddIterator(subjectItr);

                IBinaryDataListEntry bodyEntry = compiler.Evaluate(dlId, enActionType.User, Body ?? string.Empty, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator bodyItr = Dev2ValueObjectFactory.CreateEvaluateIterator(bodyEntry);
                colItr.AddIterator(bodyItr);

                IBinaryDataListEntry attachmentsEntry = compiler.Evaluate(dlId, enActionType.User, Attachments ?? string.Empty, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator attachmentsItr = Dev2ValueObjectFactory.CreateEvaluateIterator(attachmentsEntry);
                colItr.AddIterator(attachmentsItr);

                var runtimeSource = ResourceCatalog.Instance.GetResource<EmailSource>(dataObject.WorkspaceID, SelectedEmailSource.ResourceID);

                if(!allErrors.HasErrors())
                {
                    while(colItr.HasMoreData())
                    {
                        if(IsDebug)
                        {
                            var fromAccount = FromAccount;
                            if(String.IsNullOrEmpty(fromAccount))
                            {
                                fromAccount = runtimeSource.UserName;
                                AddDebugInputItem(fromAccount, "From Account");
                            }
                            else
                            {
                                AddDebugInputItem(new DebugItemVariableParams(FromAccount, "From Account", fromAccountEntry, executionId));
                            }
                            AddDebugInputItem(new DebugItemVariableParams(To, "To", toEntry, executionId));
                            AddDebugInputItem(new DebugItemVariableParams(Subject, "Subject", subjectEntry, executionId));
                            AddDebugInputItem(new DebugItemVariableParams(Body, "Body", bodyEntry, executionId));
                        }

                        var result = SendEmail(runtimeSource, colItr, fromAccountItr, passwordItr, toItr, ccItr, bccItr, subjectItr, bodyItr, attachmentsItr, out errors);
                        allErrors.MergeErrors(errors);
                        if(!allErrors.HasErrors())
                        {
                            indexToUpsertTo = UpsertResult(indexToUpsertTo, toUpsert, result);
                        }
                    }
                    compiler.Upsert(executionId, toUpsert, out errors);
                    allErrors.MergeErrors(errors);
                    if(IsDebug && !allErrors.HasErrors())
                    {
                        foreach(var debugOutputTo in toUpsert.DebugOutputs)
                        {
                            AddDebugOutputItem(new DebugItemVariableParams(debugOutputTo));
                        }
                    }
                }
                else
                {
                    if(IsDebug)
                    {
                        AddDebugInputItem(FromAccount, "From Account");
                        AddDebugInputItem(To, "To");
                        AddDebugInputItem(Subject, "Subject");
                        AddDebugInputItem(Body, "Body");
                    }
                }
            }
            catch(Exception e)
            {
                Dev2Logger.Log.Error("DSFEmail", e);
                allErrors.AddError(e.Message);
            }

            finally
            {
                // Handle Errors

                if(allErrors.HasErrors())
                {
                    UpsertResult(indexToUpsertTo, toUpsert, null);
                    compiler.Upsert(executionId, toUpsert, out errors);
                    if(dataObject.IsDebugMode())
                    {
                        AddDebugOutputItem(new DebugItemStaticDataParams("", Result, ""));
                    }
                    DisplayAndWriteError("DsfSendEmailActivity", allErrors);
                    compiler.UpsertSystemTag(dlId, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }
                if(dataObject.IsDebugMode())
                {
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }
        }

        void AddDebugInputItem(string value, string label)
        {
            if(string.IsNullOrEmpty(value) || string.IsNullOrEmpty(label))
            {
                return;
            }

            AddDebugInputItem(DataListUtil.IsEvaluated(value) ? new DebugItemStaticDataParams("", value, label) : new DebugItemStaticDataParams(value, label));
        }

        private int UpsertResult(int indexToUpsertTo, IDev2DataListUpsertPayloadBuilder<string> toUpsert, string result)
        {
            string expression;
            if(DataListUtil.IsValueRecordset(Result) && DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Star)
            {
                expression = Result.Replace(GlobalConstants.StarExpression, indexToUpsertTo.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                expression = Result;
            }
            //2013.06.03: Ashley Lewis for bug 9498 - handle multiple regions in result
            foreach(var region in DataListCleaningUtils.SplitIntoRegions(expression))
            {
                toUpsert.Add(region, result);
                toUpsert.FlushIterationFrame();

                indexToUpsertTo++;
            }
            return indexToUpsertTo;
        }

        string SendEmail(EmailSource runtimeSource, IDev2IteratorCollection colItr, IDev2DataListEvaluateIterator fromAccountItr, IDev2DataListEvaluateIterator passwordItr, IDev2DataListEvaluateIterator toItr, IDev2DataListEvaluateIterator ccItr, IDev2DataListEvaluateIterator bccItr, IDev2DataListEvaluateIterator subjectItr, IDev2DataListEvaluateIterator bodyItr, IDev2DataListEvaluateIterator attachmentsItr, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            var fromAccountValue = colItr.FetchNextRow(fromAccountItr).TheValue;
            var passwordValue = colItr.FetchNextRow(passwordItr).TheValue;
            var toValue = colItr.FetchNextRow(toItr).TheValue;
            var ccValue = colItr.FetchNextRow(ccItr).TheValue;
            var bccValue = colItr.FetchNextRow(bccItr).TheValue;
            var subjectValue = colItr.FetchNextRow(subjectItr).TheValue;
            var bodyValue = colItr.FetchNextRow(bodyItr).TheValue;
            var attachmentsValue = colItr.FetchNextRow(attachmentsItr).TheValue;
            MailMessage mailMessage = new MailMessage();
            MailPriority priority;
            if(Enum.TryParse(Priority.ToString(), true, out priority))
            {
                mailMessage.Priority = priority;
            }
            mailMessage.Subject = subjectValue;
            AddToAddresses(toValue, mailMessage);
            try
            {
                // Always use source account unless specifically overridden by From Account
                if(!string.IsNullOrEmpty(fromAccountValue))
                {
                    runtimeSource.UserName = fromAccountValue;
                    runtimeSource.Password = passwordValue;
                }
                mailMessage.From = new MailAddress(runtimeSource.UserName);
            }
            catch(Exception)
            {
                errors.AddError(string.Format("From address is not in the valid format: {0}", fromAccountValue));
                return "Failure";
            }
            mailMessage.Body = bodyValue;
            if(!String.IsNullOrEmpty(ccValue))
            {
                AddCcAddresses(ccValue, mailMessage);
            }
            if(!String.IsNullOrEmpty(bccValue))
            {
                AddBccAddresses(bccValue, mailMessage);
            }
            if(!String.IsNullOrEmpty(attachmentsValue))
            {
                AddAttachmentsValue(attachmentsValue, mailMessage);
            }
            string result;
            try
            {
                EmailSender.Send(runtimeSource, mailMessage);
                result = "Success";
            }
            catch(Exception e)
            {
                result = "Failure";
                errors.AddError(e.Message);
            }

            return result;
        }

        private List<string> GetSplitValues(string stringToSplit, char[] splitOn)
        {
            return stringToSplit.Split(splitOn, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        void AddAttachmentsValue(string attachmentsValue, MailMessage mailMessage)
        {
            try
            {
                var attachements = GetSplitValues(attachmentsValue, new[] { ',', ';' });
                attachements.ForEach(s => mailMessage.Attachments.Add(new Attachment(s)));
            }
            catch(Exception exception)
            {
                throw new Exception(string.Format("Attachments is not in the valid format: {0}", attachmentsValue), exception);
            }
        }

        void AddToAddresses(string toValue, MailMessage mailMessage)
        {
            try
            {
                var toAddresses = GetSplitValues(toValue, new[] { ',', ';' });
                toAddresses.ForEach(s => mailMessage.To.Add(new MailAddress(s)));
            }
            catch(FormatException exception)
            {
                throw new Exception(string.Format("To address is not in the valid format: {0}", toValue), exception);
            }
        }

        void AddCcAddresses(string toValue, MailMessage mailMessage)
        {
            try
            {
                var ccAddresses = GetSplitValues(toValue, new[] { ',', ';' });
                ccAddresses.ForEach(s => mailMessage.CC.Add(new MailAddress(s)));
            }
            catch(FormatException exception)
            {
                throw new Exception(string.Format("CC address is not in the valid format: {0}", toValue), exception);
            }
        }

        void AddBccAddresses(string toValue, MailMessage mailMessage)
        {
            try
            {
                var bccAddresses = GetSplitValues(toValue, new[] { ',', ';' });
                bccAddresses.ForEach(s => mailMessage.Bcc.Add(new MailAddress(s)));
            }
            catch(FormatException exception)
            {
                throw new Exception(string.Format("BCC address is not in the valid format: {0}", toValue), exception);
            }
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.StaticActivity;
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                foreach(Tuple<string, string> t in updates)
                {

                    if(t.Item1 == FromAccount)
                    {
                        FromAccount = t.Item2;
                    }
                    if(t.Item1 == Password)
                    {
                        Password = t.Item2;
                    }
                    if(t.Item1 == To)
                    {
                        To = t.Item2;
                    }
                    if(t.Item1 == Cc)
                    {
                        Cc = t.Item2;
                    }
                    if(t.Item1 == Bcc)
                    {
                        Bcc = t.Item2;
                    }
                    if(t.Item1 == Subject)
                    {
                        Subject = t.Item2;
                    }
                    if(t.Item1 == Attachments)
                    {
                        Attachments = t.Item2;
                    }
                    if(t.Item1 == Body)
                    {
                        Body = t.Item2;
                    }

                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            if(updates != null)
            {
                var itemUpdate = updates.FirstOrDefault(tuple => tuple.Item1 == Result);
                if(itemUpdate != null)
                {
                    Result = itemUpdate.Item2;
                }
            }
        }

        #region Overrides of DsfNativeActivity<string>

        public override List<DebugItem> GetDebugInputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            foreach(IDebugItem debugOutput in _debugOutputs)
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
            return GetForEachItems(FromAccount, Password, To, Cc, Bcc, Subject, Attachments, Body);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion

    }
}
