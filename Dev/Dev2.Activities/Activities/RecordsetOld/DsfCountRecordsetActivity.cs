/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Data.TO;
using Dev2.Data.Util;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Util;
using Dev2.Validation;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;


namespace Unlimited.Applications.BusinessDesignStudio.Activities

{
    public class DsfCountRecordsetActivity : DsfActivityAbstract<string>,IEquatable<DsfCountRecordsetActivity>
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

        public DsfCountRecordsetActivity()
            : base("Count Records")
        {
            RecordsetName = string.Empty;
            CountNumber = string.Empty;
            DisplayName = "Count Records";
        }


        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }

        public override List<string> GetOutputs() => new List<string> { CountNumber };

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
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfCountRecordsActivity", allErrors);
                    var errorString = allErrors.MakeDataListReady();
                    dataObject.Environment.AddError(errorString);
                }
                if(dataObject.IsDebugMode())
                {
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }
            }
        }

        private void TryExecuteTool(IDSFDataObject dataObject, int update, ErrorResultTO allErrors)
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
                if (dataObject.Environment.HasRecordSet(RecordsetName))
                {
                    var count = dataObject.Environment.GetCount(rs);
                    var value = count.ToString();
                    dataObject.Environment.Assign(CountNumber, value, update);
                    AddDebugOutputItem(new DebugEvalResult(CountNumber, "", dataObject.Environment, update));

                }
                else
                {
                    allErrors.AddError(String.Format(ErrorResource.NullRecordSet, RecordsetName));
                }
            }
        }

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
            if(updates != null && updates.Count == 1)
            {
                RecordsetName = updates[0].Item2;
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            var itemUpdate = updates?.FirstOrDefault(tuple => tuple.Item1 == CountNumber);
            if(itemUpdate != null)
            {
                CountNumber = itemUpdate.Item2;
            }
        }


        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs() => GetForEachItems(RecordsetName);

        public override IList<DsfForEachItem> GetForEachOutputs() => GetForEachItems(CountNumber);

        #endregion

        public bool Equals(DsfCountRecordsetActivity other)
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
                && string.Equals(DisplayName, other.DisplayName) 
                && string.Equals(CountNumber, other.CountNumber);
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

            return Equals((DsfCountRecordsetActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (RecordsetName != null ? RecordsetName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (CountNumber != null ? CountNumber.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
