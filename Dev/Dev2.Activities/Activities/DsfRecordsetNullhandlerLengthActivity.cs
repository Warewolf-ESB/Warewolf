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
using Dev2.Activities;
using Dev2.Activities.Debug;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
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
using System.Linq;
using Dev2.Common.State;

namespace Unlimited.Applications.BusinessDesignStudio.Activities

{

    [ToolDescriptorInfo("RecordSet-Length", "Length", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Recordset", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Recordset_Length")]
    public class DsfRecordsetNullhandlerLengthActivity : DsfActivityAbstract<string>,IEquatable<DsfRecordsetNullhandlerLengthActivity>
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
        [Outputs("Length"), FindMissing]
        public string RecordsLength { get; set; }

        public DsfRecordsetNullhandlerLengthActivity()
            : base("Length")
        {
            RecordsetName = string.Empty;
            RecordsLength = string.Empty;
            DisplayName = "Length";
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
                    Name="RecordsLength",
                    Value = RecordsLength,
                    Type = StateVariable.StateType.Output
                }
            };
        }

        public override List<string> GetOutputs() => new List<string> { RecordsLength };

        public bool TreatNullAsZero { get; set; }

        
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
                if(!allErrors.HasErrors())
                {
                    try
                    {
                        TryExecuteTool(dataObject, update, allErrors);
                    }
                    catch (Exception e)
                    {
                        allErrors.AddError(e.Message);
                        dataObject.Environment.Assign(RecordsLength, "0", update);
                        AddDebugOutputItem(new DebugItemStaticDataParams("0", RecordsLength, "", "="));
                    }
                }
            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfRecordsetNullhandlerLengthActivity", allErrors);
                    var errorString = allErrors.MakeDisplayReady();
                    dataObject.Environment.AddError(errorString);
                }
                if(dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }
            }
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        private void TryExecuteTool(IDSFDataObject dataObject, int update, ErrorResultTO allErrors)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            var rs = DataListUtil.ExtractRecordsetNameFromValue(RecordsetName);
            if (RecordsLength == string.Empty)
            {
                allErrors.AddError(ErrorResource.BlankResultVariable);
            }
            if (dataObject.IsDebugMode())
            {
                var warewolfEvalResult = dataObject.Environment.Eval(RecordsetName.Replace("()", "(*)"), update);
                if (warewolfEvalResult.IsWarewolfRecordSetResult && warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfRecordSetResult recsetResult)
                {
                    AddDebugInputItem(new DebugItemWarewolfRecordset(recsetResult.Item, RecordsetName, "Recordset", "="));
                }

                //Because the environment eval above where you can only send through a recordset name and not list this code wont be reached.
                //No Coverage added.
                if (warewolfEvalResult.IsWarewolfAtomListresult && warewolfEvalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult atomListResult)
                {
                    AddDebugInputItem(new DebugEvalResult(RecordsetName, "Recordset", dataObject.Environment, update));
                }

            }
            var rule = new IsSingleValueRule(() => RecordsLength);
            var single = rule.Check();
            if (single != null)
            {
                allErrors.AddError(single.Message);
            }
            else
            {

                if (dataObject.Environment.HasRecordSet(RecordsetName))
                {
                    var count = dataObject.Environment.GetLength(rs);
                    var value = count.ToString();
                    dataObject.Environment.Assign(RecordsLength, value, update);
                    if (dataObject.Environment.Errors != null && !dataObject.Environment.Errors.Any())
                    {
                        AddDebugOutputItem(new DebugItemWarewolfAtomResult(value, RecordsLength, ""));
                    }

                }
                else
                {
                    if (TreatNullAsZero)
                    {
                        dataObject.Environment.Assign(RecordsLength, 0.ToString(), update);
                        AddDebugOutputItem(new DebugItemWarewolfAtomResult(0.ToString(), RecordsLength, ""));
                    }
                    else
                    {
                        allErrors.AddError(string.Format(ErrorResource.NullRecordSet, RecordsetName));
                    }

                }

            }
        }

        #region Get Debug Inputs/Outputs

        #region GetDebugInputs

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            foreach(IDebugItem debugInput in _debugInputs)
            {
                debugInput.FlushStringBuilder();
            }
            return _debugInputs;
        }

        #endregion

        #region GetDebugOutputs

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            foreach(IDebugItem debugOutput in _debugOutputs)
            {
                debugOutput.FlushStringBuilder();
            }
            return _debugOutputs;
        }

        #endregion


        #endregion Get Inputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            if(updates != null && updates.Count == 1)
            {
                RecordsetName = updates[0].Item2;
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            if(updates != null && updates.Count == 1)
            {
                RecordsLength = updates[0].Item2;
            }
        }

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs() => GetForEachItems(RecordsetName);

        public override IList<DsfForEachItem> GetForEachOutputs() => GetForEachItems(RecordsLength);

        #endregion

        public bool Equals(DsfRecordsetNullhandlerLengthActivity other)
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
                && string.Equals(RecordsLength, other.RecordsLength) 
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

            return Equals((DsfRecordsetNullhandlerLengthActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (RecordsetName != null ? RecordsetName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RecordsLength != null ? RecordsLength.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ TreatNullAsZero.GetHashCode();
                return hashCode;
            }
        }
    }
}
