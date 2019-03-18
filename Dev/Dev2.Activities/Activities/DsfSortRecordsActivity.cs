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
using Dev2.Common.State;
using Dev2.Data.TO;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;


namespace Unlimited.Applications.BusinessDesignStudio.Activities

{
    [ToolDescriptorInfo("RecordSet-SortRecords", "Sort", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Activities", "1.0.0.0", "Legacy", "Recordset", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Recordset_Sort")]
    public class DsfSortRecordsActivity : DsfActivityAbstract<string>,IEquatable<DsfSortRecordsActivity>
    {

        /// <summary>
        /// Gets or sets the sort field.
        /// </summary>
        [Inputs("SortField")]
        [FindMissing]
        public string SortField { get; set; }

        /// <summary>
        /// Gets or sets the selected sort.
        /// </summary>
        [Inputs("SelectedSort")]
        
        public string SelectedSort { get; set; }
        

        public DsfSortRecordsActivity()
            : base("Sort Records")
        {
            SortField = string.Empty;
            SelectedSort = "Forward";
            DisplayName = "Sort Records";
        }

        public override IEnumerable<StateVariable> GetState()
        {
            return new[] {
                new StateVariable
                {
                    Name = "SortField",
                    Value = SortField,
                    Type = StateVariable.StateType.InputOutput
                },                 
                new StateVariable
                {
                    Name="SelectedSort",
                    Value = SelectedSort,
                    Type = StateVariable.StateType.Input
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

        public override List<string> GetOutputs() => new List<string> { SortField };

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {

            var allErrors = new ErrorResultTO();

            InitializeDebug(dataObject);

            try
            {
                var descOrder = String.IsNullOrEmpty(SelectedSort) || SelectedSort.Equals("Backwards");
                if (dataObject.IsDebugMode())
                {
                    AddDebugInputItem(SortField, "Sort Field", dataObject.Environment, update);
                }
                if(!string.IsNullOrEmpty(SortField))
                {
                    dataObject.Environment.SortRecordSet(SortField, descOrder, update);
                }
                else
                {
                    allErrors.AddError(ErrorResource.NoRecordSet);
                }
            }
                catch(Exception err)
                {
                    allErrors.AddError(err.Message);
                }
            finally
            {
                if(allErrors.HasErrors())
                {
                    DisplayAndWriteError("DsfSortRecordsActivity", allErrors);
                    foreach(var error in allErrors.FetchErrors())
                    {
                        dataObject.Environment.AddError(error);
                    }
                }
                if(dataObject.IsDebugMode())
                {
                    DebugOutputs(dataObject, update);

                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }
            }
        }

        void DebugOutputs(IDSFDataObject dataObject,int update)
        {
            if(dataObject.IsDebugMode())
            {
                var data = dataObject.Environment.Eval(dataObject.Environment.ToStar(SortField), update);
                if (data.IsWarewolfAtomListresult)
                {
                    var lst = data as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                    AddDebugOutputItem(new DebugItemWarewolfAtomListResult(lst, "", "", SortField, "", "", "="));
                }
                else
                {
                    if (data.IsWarewolfAtomResult && data is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult atomData && atomData.Item.IsNothing)
                    {
                        AddDebugOutputItem(new DebugItemStaticDataParams("", SortField, "", "="));
                    }

                }
            }
        }

        #region Private Methods

        void AddDebugInputItem(string expression, string labelText, IExecutionEnvironment env, int update)
        {
            var data = env.Eval(env.ToStar(expression), update);
            if (data.IsWarewolfAtomListresult)
            {
                var lst = data as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                AddDebugInputItem(new DebugItemWarewolfAtomListResult(lst, "", "", expression, labelText, "", "="));
                AddDebugInputItem(new DebugItemStaticDataParams(SelectedSort, "Sort Order"));
            }
            else
            {
                if (data.IsWarewolfAtomResult && data is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult atomData && atomData.Item.IsNothing)
                {
                    AddDebugInputItem(new DebugItemStaticDataParams("", expression, labelText, "="));
                    AddDebugInputItem(new DebugItemStaticDataParams(SelectedSort, "Sort Order"));
                }

            }
        }

        #endregion Private Methods

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            if(updates != null)
            {
                foreach(Tuple<string, string> t in updates)
                {

                    if(t.Item1 == SortField)
                    {
                        SortField = t.Item2;
                    }
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            if(updates != null)
            {
                foreach(Tuple<string, string> t in updates)
                {

                    if(t.Item1 == SortField)
                    {
                        SortField = t.Item2;
                    }
                }
            }
        }

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs() => GetForEachItems(SortField);

        public override IList<DsfForEachItem> GetForEachOutputs() => GetForEachItems(SortField);

        #endregion


        #region GetDebugInputs/Outputs

        #region GetDebugInputs

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update) => _debugInputs;

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update) => _debugOutputs;

        #endregion

        #endregion

        public bool Equals(DsfSortRecordsActivity other)
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
                && string.Equals(SortField, other.SortField) 
                && string.Equals(SelectedSort, other.SelectedSort);
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

            return Equals((DsfSortRecordsActivity) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (SortField != null ? SortField.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (SelectedSort != null ? SelectedSort.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
