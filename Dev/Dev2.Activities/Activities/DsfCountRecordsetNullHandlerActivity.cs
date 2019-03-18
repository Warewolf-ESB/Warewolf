#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Util;
using Dev2.Validation;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;


namespace Unlimited.Applications.BusinessDesignStudio.Activities

{
    [ToolDescriptorInfo("RecordSet-CountRecords", "Count", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Recordset", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Recordset_Count")]
    public class DsfCountRecordsetNullHandlerActivity : DsfActivityAbstract<string>,IEquatable<DsfCountRecordsetNullHandlerActivity>
    {
        #region Fields

        #endregion

        /// <summary>
        /// Gets or sets the name of the recordset.
        /// </summary>  
        [Inputs("RecordsetName"), FindMissing]
        public string RecordsetName { get; set; }

        /// <summary>
        /// Gets or sets the count number.
        /// </summary>  
        [Outputs("CountNumber"), FindMissing]
        public string CountNumber { get; set; }

        public bool TreatNullAsZero { get; set; }

        public DsfCountRecordsetNullHandlerActivity()
            : base("Count Records")
        {
            RecordsetName = string.Empty;
            CountNumber = string.Empty;
            DisplayName = "Count Records";
            TreatNullAsZero = true;
        }

        public override IEnumerable<StateVariable> GetState()
        {
            return new[] {
                new StateVariable
                {
                    Name = "RecordsetName",
                    Value = RecordsetName,
                    Type = StateVariable.StateType.Input
                },
                 new StateVariable
                {
                    Name = "TreatNullAsZero",
                    Value = TreatNullAsZero.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name="CountNumber",
                    Value = CountNumber,
                    Type = StateVariable.StateType.Output
                }
            };
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

        }
        

        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {


            var allErrors = new ErrorResultTO();
            var errors = new ErrorResultTO();
            allErrors.MergeErrors(errors);
            InitializeDebug(dataObject);
            // Process if no errors
            try
            {
                ValidateRecordsetName(RecordsetName, errors);
                allErrors.MergeErrors(errors);
                if (!allErrors.HasErrors())
                {
                    try
                    {
                        TryExecute(dataObject, update, allErrors);
                    }
                    catch (Exception e)
                    {
                        AddDebugInputItem(new DebugItemStaticDataParams("", RecordsetName, "Recordset", "="));
                        allErrors.AddError(e.Message);
                        dataObject.Environment.Assign(CountNumber, "0", update);
                        AddDebugOutputItem(new DebugItemStaticDataParams("0", CountNumber, "", "="));
                    }
                }
            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if (hasErrors)
                {
                    DisplayAndWriteError("DsfCountRecordsActivity", allErrors);
                    var errorString = allErrors.MakeDataListReady();
                    dataObject.Environment.AddError(errorString);
                }
                if (dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }
            }
        }

        private void TryExecute(IDSFDataObject dataObject, int update, ErrorResultTO allErrors)
        {
            var rs = DataListUtil.ExtractRecordsetNameFromValue(RecordsetName);
            if (CountNumber == string.Empty)
            {
                allErrors.AddError(ErrorResource.BlankResultVariable);
            }
            if (dataObject.IsDebugMode())
            {
                AddDebugInputItem(new DebugEvalResult(dataObject.Environment.ToStar(RecordsetName), "Recordset", dataObject.Environment, update));
            }
            var rule = new IsSingleValueRule(() => CountNumber);
            var single = rule.Check();
            if (single != null)
            {
                allErrors.AddError(single.Message);
            }
            else
            {
                var hasRecordSet = dataObject.Environment.HasRecordSet(RecordsetName);
                if (hasRecordSet)//null check happens here
                {
                    var count = dataObject.Environment.GetCount(rs);
                    var value = count.ToString();
                    dataObject.Environment.Assign(CountNumber, value, update);
                    AddDebugOutputItem(new DebugEvalResult(CountNumber, "", dataObject.Environment, update));

                }
                else
                {
                    if (TreatNullAsZero)
                    {
                        dataObject.Environment.Assign(CountNumber, 0.ToString(), update);
                        AddDebugOutputItem(new DebugEvalResult(CountNumber, "", dataObject.Environment, update));

                    }
                    else
                    {
                        allErrors.AddError(string.Format(ErrorResource.NullRecordSet, RecordsetName));
                    }

                }
            }
        }

        public override List<string> GetOutputs() => new List<string> { CountNumber };

        #region Get Debug Inputs/Outputs

        #region GetDebugInputs

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update) => _debugInputs;

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            foreach (IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }



        #endregion




        #endregion Get Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            if (updates != null && updates.Count == 1)
            {
                RecordsetName = updates[0].Item2;
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            var itemUpdate = updates?.FirstOrDefault(tuple => tuple.Item1 == CountNumber);
            if (itemUpdate != null)
            {
                CountNumber = itemUpdate.Item2;
            }
        }


        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs() => GetForEachItems(RecordsetName);

        public override IList<DsfForEachItem> GetForEachOutputs() => GetForEachItems(CountNumber);

        #endregion

        public bool Equals(DsfCountRecordsetNullHandlerActivity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return base.Equals(other) 
                && string.Equals(RecordsetName, other.RecordsetName) 
                && string.Equals(CountNumber, other.CountNumber) 
                && TreatNullAsZero == other.TreatNullAsZero;
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

            return Equals((DsfCountRecordsetNullHandlerActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (RecordsetName != null ? RecordsetName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CountNumber != null ? CountNumber.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ TreatNullAsZero.GetHashCode();
                return hashCode;
            }
        }
    }
}
