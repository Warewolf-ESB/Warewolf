using System;
using System.Activities;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using Dev2.Common;
using Dev2.Data.Enums;
using Dev2.Data.Factories;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.DataList.Contract.Builders;
using Dev2.DataList.Contract.Value_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;
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
        private new List<DebugItem> _debugInputs = new List<DebugItem>();
        private new List<DebugItem> _debugOutputs = new List<DebugItem>();

        #endregion

        /// <summary>
        /// The property that holds all the convertions
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
                return _dataObject.IsDebug || ServerLogger.ShouldLog(_dataObject.ResourceID);
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
            IEsbChannel esbChannel = context.GetExtension<IEsbChannel>();
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            Guid dlID = dataObject.DataListID;
            var workspaceID = dataObject.WorkspaceID;
            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();
            Guid executionId = DataListExecutionID.Get(context);
            int indexToUpsertTo = 0;
            IDev2DataListUpsertPayloadBuilder<string> toUpsert = Dev2DataListBuilderFactory.CreateStringDataListUpsertBuilder(true);

            try
            {
                var colItr = Dev2ValueObjectFactory.CreateIteratorCollection();

                IBinaryDataListEntry fromAccountEntry = compiler.Evaluate(dlID, enActionType.User, FromAccount, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator fromAccountItr = Dev2ValueObjectFactory.CreateEvaluateIterator(fromAccountEntry);
                colItr.AddIterator(fromAccountItr);

                IBinaryDataListEntry passwordEntry = compiler.Evaluate(dlID, enActionType.User, Password, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator passwordItr = Dev2ValueObjectFactory.CreateEvaluateIterator(passwordEntry);
                colItr.AddIterator(passwordItr);

                IBinaryDataListEntry toEntry = compiler.Evaluate(dlID, enActionType.User, To, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator toItr = Dev2ValueObjectFactory.CreateEvaluateIterator(toEntry);
                colItr.AddIterator(toItr);

                IBinaryDataListEntry ccEntry = compiler.Evaluate(dlID, enActionType.User, Cc, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator ccItr = Dev2ValueObjectFactory.CreateEvaluateIterator(ccEntry);
                colItr.AddIterator(ccItr);

                IBinaryDataListEntry bccEntry = compiler.Evaluate(dlID, enActionType.User, Bcc, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator bccItr = Dev2ValueObjectFactory.CreateEvaluateIterator(bccEntry);
                colItr.AddIterator(bccItr);

                IBinaryDataListEntry subjectEntry = compiler.Evaluate(dlID, enActionType.User, Subject, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator subjectItr = Dev2ValueObjectFactory.CreateEvaluateIterator(subjectEntry);
                colItr.AddIterator(subjectItr);

                IBinaryDataListEntry bodyEntry = compiler.Evaluate(dlID, enActionType.User, Body, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator bodyItr = Dev2ValueObjectFactory.CreateEvaluateIterator(bodyEntry);
                colItr.AddIterator(bodyItr);

                IBinaryDataListEntry attachmentsEntry = compiler.Evaluate(dlID, enActionType.User, Attachments, false, out errors);
                allErrors.MergeErrors(errors);
                IDev2DataListEvaluateIterator attachmentsItr = Dev2ValueObjectFactory.CreateEvaluateIterator(attachmentsEntry);
                colItr.AddIterator(attachmentsItr);

                var runtimeSource = new EmailSource(SelectedEmailSource.ToXml());

                if(!allErrors.HasErrors())
                {
                    while(colItr.HasMoreData())
                    {
                        if(IsDebug)
                        {
                            AddDebugInputItem(FromAccount, "From Account", fromAccountEntry, executionId, indexToUpsertTo);
                            AddDebugInputItem(To, "To", toEntry, executionId, indexToUpsertTo);
                            AddDebugInputItem(Subject, "Subject", subjectEntry, executionId, indexToUpsertTo);
                            AddDebugInputItem(Body, "Body", bodyEntry, executionId, indexToUpsertTo);
                        }

                        var result = SendEmail(runtimeSource, colItr, fromAccountItr, passwordItr, toItr, ccItr, bccItr, subjectItr, bodyItr, attachmentsItr, out errors);
                        allErrors.MergeErrors(errors);
                        indexToUpsertTo = UpsertResult(indexToUpsertTo, toUpsert, result, dataObject, executionId);
                    }
                    compiler.Upsert(executionId, toUpsert, out errors);
                }
            }
            catch(Exception e)
            {
                allErrors.AddError(e.Message);
            }

            finally
            {
                // Handle Errors

                if(allErrors.HasErrors())
                {
                    UpsertResult(indexToUpsertTo, toUpsert, "Failure", dataObject, executionId);
                    compiler.Upsert(executionId, toUpsert, out errors);
                    DisplayAndWriteError("DsfSendEmailActivity", allErrors);
                    compiler.UpsertSystemTag(dlID, enSystemTag.Dev2Error, allErrors.MakeDataListReady(), out errors);
                }
                if(dataObject.IsDebugMode())
                {
                    DispatchDebugState(context, StateType.Before);
                    DispatchDebugState(context, StateType.After);
                }
            }

        }

        private int UpsertResult(int indexToUpsertTo, IDev2DataListUpsertPayloadBuilder<string> toUpsert, string result,
                                 IDSFDataObject dataObject, Guid executionId)
        {
            string expression;
            if(DataListUtil.IsValueRecordset(Result) &&
                DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Star)
            {
                expression = Result.Replace(GlobalConstants.StarExpression,
                                            indexToUpsertTo.ToString(CultureInfo.InvariantCulture));
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
                if(dataObject.IsDebugMode())
                {
                    AddDebugOutputItem(region, result, executionId, indexToUpsertTo);
                }
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
                AddCCAddresses(ccValue, mailMessage);
            }
            if(!String.IsNullOrEmpty(bccValue))
            {
                AddBCCAddresses(bccValue, mailMessage);
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

        void AddCCAddresses(string toValue, MailMessage mailMessage)
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

        void AddBCCAddresses(string toValue, MailMessage mailMessage)
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
            if(updates != null && updates.Count == 1)
            {
                Result = updates[0].Item2;
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

        #region Private Methods

        private void AddDebugInputItem(string expression, string labelText, IBinaryDataListEntry valueEntry, Guid executionId, int indexToUpsertTo)
        {
            DebugItem itemToAdd = new DebugItem();

            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = (indexToUpsertTo + 1).ToString() });

            if(!string.IsNullOrWhiteSpace(labelText))
            {
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = labelText });
            }

            if(valueEntry != null)
            {
                expression = expression.TrimEnd(new[] { '\r', '\n' });
                itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = GlobalConstants.EqualsExpression });
                if(expression.Contains("[[") && expression.Contains("]]"))
                {
                    var makeParts = DataListFactory.CreateLanguageParser().MakeParts(expression);
                    if(makeParts.Count > 1)
                    {
                        var r = "[[" + makeParts[0].Payload + "]]";
                        var s = "[[" + makeParts[1].Payload + "]]";
                        if(s.Contains("[[") && s.Contains("]]"))
                        {
                            if(valueEntry.IsRecordset)
                            {
                                var debugItemsFromEntry = CreateDebugItemsFromEntry(r, valueEntry, executionId, enDev2ArgumentType.Input);
                                foreach(var debugItemResult in debugItemsFromEntry)
                                {
                                    debugItemResult.GroupName = debugItemResult.GroupName + " " + s;
                                    if(debugItemResult.Type == DebugItemResultType.Variable)
                                    {
                                        debugItemResult.Value = debugItemResult.Value + " " + s;
                                    }
                                }
                                itemToAdd.AddRange(debugItemsFromEntry);
                            }
                            else
                            {
                                var debugItemsFromEntry = CreateDebugItemsFromEntry(r, valueEntry, executionId, enDev2ArgumentType.Input);
                                itemToAdd.AddRange(debugItemsFromEntry);
                            }
                        }
                    }
                    else
                    {
                        var debugItemsFromEntry = CreateDebugItemsFromEntry(expression, valueEntry, executionId, enDev2ArgumentType.Input);
                        itemToAdd.AddRange(debugItemsFromEntry);
                    }
                }
                else
                {
                    itemToAdd.AddRange(CreateDebugItemsFromEntry(expression, valueEntry, executionId, enDev2ArgumentType.Input));
                }

            }
            _debugInputs.Add(itemToAdd);
        }

        private void AddDebugOutputItem(string result, string value, Guid dlId, int iterationCounter)
        {
            DebugItem itemToAdd = new DebugItem();
            itemToAdd.Add(new DebugItemResult { Type = DebugItemResultType.Label, Value = (iterationCounter + 1).ToString() });
            itemToAdd.AddRange(CreateDebugItemsFromString(result, value, dlId, iterationCounter, enDev2ArgumentType.Output));
            _debugOutputs.Add(itemToAdd);
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