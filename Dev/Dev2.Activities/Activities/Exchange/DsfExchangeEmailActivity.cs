#pragma warning disable
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
using Dev2.Data.Interfaces.Enums;
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
using Warewolf.Storage.Interfaces;
using Warewolf.Exchange.Email.Wrapper;
using Dev2.Common.State;

namespace Dev2.Activities.Exchange
{
#pragma warning disable S3776,S1541,S134,CC0075,S1066,S1067
    public class DsfExchangeEmailActivity : DsfActivityAbstract<string>,IEquatable<DsfExchangeEmailActivity>
    {
        readonly IDev2EmailSender _emailSender;

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

        IDSFDataObject _dataObject;

        public IExchangeSource SavedSource { get; set; }

        [FindMissing]
        public string To { get; set; }
        [FindMissing]
        public string Cc { get; set; }
        [FindMissing]
        public string Bcc { get; set; }
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

        public override List<string> GetOutputs() => new List<string> { Result };

        public override IEnumerable<StateVariable> GetState()
        {
            return new[]
            {
                new StateVariable
                {
                    Name="SavedSource.ResourceID",
                    Type=StateVariable.StateType.Input,
                    Value= SavedSource.ResourceID.ToString()
                },
                new StateVariable
                {
                    Name="To",
                    Type=StateVariable.StateType.Input,
                    Value= To
                },
                new StateVariable
                {
                    Name="Cc",
                    Type=StateVariable.StateType.Input,
                    Value= Cc
                },
                new StateVariable
                {
                    Name="Bcc",
                    Type=StateVariable.StateType.Input,
                    Value= Bcc
                },
                new StateVariable
                {
                    Name="Subject",
                    Type=StateVariable.StateType.Input,
                    Value= Subject
                },
                new StateVariable
                {
                    Name="Attachments",
                    Type=StateVariable.StateType.Input,
                    Value= Attachments
                },
                new StateVariable
                {
                    Name="Body",
                    Type=StateVariable.StateType.Input,
                    Value= Body
                },
                new StateVariable
                {
                    Name="Result",
                    Type=StateVariable.StateType.Output,
                    Value= Result
                }
            };
        }

        #region Overrides of DsfNativeActivity<string>

        bool IsDebug
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

        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            _dataObject = dataObject;

            var allErrors = new ErrorResultTO();
            var indexToUpsertTo = 0;

            InitializeDebug(dataObject);
            try
            {
                IExchange runtimeSource = ResourceCatalog.GetResource<ExchangeSource>(dataObject.WorkspaceID, SavedSource.ResourceID);

                if (runtimeSource == null)
                {
                    dataObject.Environment.Errors.Add(ErrorResource.InvalidEmailSource);
                    return;
                }
                indexToUpsertTo = TryExecute(dataObject, update, allErrors, indexToUpsertTo, runtimeSource);
            }
            catch (Exception e)
            {
                Dev2Logger.Error("DSFEmail", e, GlobalConstants.WarewolfError);
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

        private int TryExecute(IDSFDataObject dataObject, int update, ErrorResultTO allErrors, int indexToUpsertTo, IExchange runtimeSource)
        {
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
                    var result = _emailSender.SendEmail(runtimeSource, colItr, toItr, ccItr, bccItr, subjectItr, bodyItr, attachmentsItr, out ErrorResultTO errors);
                    allErrors.MergeErrors(errors);
                    if (!allErrors.HasErrors())
                    {
                        indexToUpsertTo = UpsertResult(indexToUpsertTo, dataObject.Environment, result, update);
                    }
                }
                if (IsDebug && !allErrors.HasErrors() && !string.IsNullOrEmpty(Result))
                {
                    AddDebugOutputItem(new DebugEvalResult(Result, "", dataObject.Environment, update));
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

            return indexToUpsertTo;
        }

        void AddDebugInputItem(string value, string label)
        {
            if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(label))
            {
                return;
            }
            AddDebugInputItem(DataListUtil.IsEvaluated(value) ? new DebugItemStaticDataParams("", value, label) : new DebugItemStaticDataParams(value, label));
        }

        int UpsertResult(int indexToUpsertTo, IExecutionEnvironment environment, string result, int update)
        {
            string expression;
            expression = DataListUtil.IsValueRecordset(Result) && DataListUtil.GetRecordsetIndexType(Result) == enRecordsetIndexType.Star ? Result.Replace(GlobalConstants.StarExpression, indexToUpsertTo.ToString(CultureInfo.InvariantCulture)) : Result;
            foreach (var region in DataListCleaningUtils.SplitIntoRegions(expression))
            {
                environment.Assign(region, result, update);
                indexToUpsertTo++;
            }
            return indexToUpsertTo;
        }

        public override enFindMissingType GetFindMissingType() => enFindMissingType.StaticActivity;

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

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            foreach (IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
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

        public override IList<DsfForEachItem> GetForEachInputs() => GetForEachItems(To, Cc, Bcc, Subject, Attachments, Body);

        public override IList<DsfForEachItem> GetForEachOutputs() => GetForEachItems(Result);

        #endregion

        public bool Equals(DsfExchangeEmailActivity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            var isSourceEqual = CommonEqualityOps.AreObjectsEqual(SavedSource, other.SavedSource);
            return base.Equals(other)
                && isSourceEqual
                && string.Equals(To, other.To)
                && string.Equals(Cc, other.Cc)
                && string.Equals(Bcc, other.Bcc)
                && string.Equals(Subject, other.Subject)
                && string.Equals(Attachments, other.Attachments)
                && string.Equals(Body, other.Body)
                && string.Equals(DisplayName, other.DisplayName)
                && string.Equals(Result, other.Result);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((DsfExchangeEmailActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (SavedSource != null ? SavedSource.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (To != null ? To.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Cc != null ? Cc.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Bcc != null ? Bcc.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DisplayName != null ? DisplayName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Subject != null ? Subject.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Attachments != null ? Attachments.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Body != null ? Body.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Result != null ? Result.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
#pragma warning restore S3776,S1541,S134,CC0075,S1066,S1067
}
