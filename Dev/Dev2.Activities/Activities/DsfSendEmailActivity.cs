
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
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data;
using Dev2.Data.Enums;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;

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
            int indexToUpsertTo = 0;

            InitializeDebug(dataObject);
            try
            {
                var colItr = new WarewolfListIterator();
                
                var fromAccountItr = new WarewolfIterator(dataObject.Environment.Eval(FromAccount??string.Empty));
                colItr.AddVariableToIterateOn(fromAccountItr);

                var passwordItr = new WarewolfIterator(dataObject.Environment.Eval(Password));
                colItr.AddVariableToIterateOn(passwordItr);

                var toItr = new WarewolfIterator(dataObject.Environment.Eval(To));
                colItr.AddVariableToIterateOn(toItr);

                var ccItr = new WarewolfIterator(dataObject.Environment.Eval(Cc));
                colItr.AddVariableToIterateOn(ccItr);

                var bccItr = new WarewolfIterator(dataObject.Environment.Eval(Bcc));
                colItr.AddVariableToIterateOn(bccItr);

                var subjectItr = new WarewolfIterator(dataObject.Environment.Eval(Subject));
                colItr.AddVariableToIterateOn(subjectItr);

                var bodyItr = new WarewolfIterator(dataObject.Environment.Eval(Body??string.Empty));
                colItr.AddVariableToIterateOn(bodyItr);

                var attachmentsItr = new WarewolfIterator(dataObject.Environment.Eval(Attachments ?? string.Empty));
                colItr.AddVariableToIterateOn(attachmentsItr);

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
                                AddDebugInputItem(new DebugEvalResult(FromAccount, "From Account", dataObject.Environment));
                            }
                            AddDebugInputItem(new DebugEvalResult(To, "To", dataObject.Environment));
                            AddDebugInputItem(new DebugEvalResult(Subject, "Subject", dataObject.Environment));
                            AddDebugInputItem(new DebugEvalResult(Body, "Body", dataObject.Environment));
                        }

                        var result = SendEmail(runtimeSource, colItr, fromAccountItr, passwordItr, toItr, ccItr, bccItr, subjectItr, bodyItr, attachmentsItr, out errors);
                        allErrors.MergeErrors(errors);
                        if(!allErrors.HasErrors())
                        {
                            indexToUpsertTo = UpsertResult(indexToUpsertTo, dataObject.Environment, result);
                        }
                    }
                    if(IsDebug && !allErrors.HasErrors())
                    {
                        AddDebugOutputItem(new DebugEvalResult(Result,"",dataObject.Environment));
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
                    foreach (var err in allErrors.FetchErrors())
                    {
                        dataObject.Environment.Errors.Add(err);
                    }
                    UpsertResult(indexToUpsertTo, dataObject.Environment, null);
                    if(dataObject.IsDebugMode())
                    {
                        AddDebugOutputItem(new DebugItemStaticDataParams("", Result, ""));
                    }
                    DisplayAndWriteError("DsfSendEmailActivity", allErrors);

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

        private int UpsertResult(int indexToUpsertTo, IExecutionEnvironment environment, string result)
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
                environment.Assign(region, result);
                indexToUpsertTo++;
            }
            return indexToUpsertTo;
        }

        string SendEmail(EmailSource runtimeSource, IWarewolfListIterator colItr, IWarewolfIterator fromAccountItr, IWarewolfIterator passwordItr, IWarewolfIterator toItr, IWarewolfIterator ccItr, IWarewolfIterator bccItr, IWarewolfIterator subjectItr, IWarewolfIterator bodyItr, IWarewolfIterator attachmentsItr, out ErrorResultTO errors)
        {
            errors = new ErrorResultTO();
            var fromAccountValue = colItr.FetchNextValue(fromAccountItr);
            var passwordValue = colItr.FetchNextValue(passwordItr);
            var toValue = colItr.FetchNextValue(toItr);
            var ccValue = colItr.FetchNextValue(ccItr);
            var bccValue = colItr.FetchNextValue(bccItr);
            var subjectValue = colItr.FetchNextValue(subjectItr);
            var bodyValue = colItr.FetchNextValue(bodyItr);
            var attachmentsValue = colItr.FetchNextValue(attachmentsItr);
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

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment dataList)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList)
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
