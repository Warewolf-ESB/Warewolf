/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Data.TO;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
// ReSharper restore CheckNamespace
{
    [ToolDescriptorInfo("RecordSet-Delete", "Delete", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090C5C9EA1E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Recordset", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Recordset_Delete")]
    public class DsfDeleteRecordNullHandlerActivity : DsfActivityAbstract<string>
    {
        /// <summary>
        /// Gets or sets the name of the recordset.
        /// </summary>  
        [Inputs("RecordsetName"), FindMissing]
        public string RecordsetName { get; set; }

        /// <summary>
        /// Gets or sets the count number.
        /// </summary>  
        [Outputs("Result"), FindMissing]
        public new string Result { get; set; }

        public DsfDeleteRecordNullHandlerActivity()
            : base("Delete Record")
        {
            RecordsetName = string.Empty;
            Result = string.Empty;
            TreatNullAsZero = true;
        }



        public override List<string> GetOutputs()
        {
            return new List<string> { Result };
        }

        public bool TreatNullAsZero { get; set; }
        // ReSharper disable RedundantOverridenMember
        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);

        }
        // ReSharper restore RedundantOverridenMember

        protected override void OnExecute(NativeActivityContext context)
        {
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, 0);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {


            ErrorResultTO allErrors = new ErrorResultTO();
            ErrorResultTO errors = new ErrorResultTO();

            InitializeDebug(dataObject);
            try
            {
                var hasRecordSet = dataObject.Environment.HasRecordSet(RecordsetName);
                if (hasRecordSet)//null check happens here
                {
                    ValidateRecordsetName(RecordsetName, errors);

                    GetDebug(dataObject, update);
                    dataObject.Environment.EvalDelete(RecordsetName, update);
                    if (!string.IsNullOrEmpty(Result))
                    {
                        dataObject.Environment.Assign(Result, "Success", update);
                        AddDebugOutputItem(new DebugEvalResult(Result, "", dataObject.Environment, update));
                    }
                }
                else
                {
                    if (TreatNullAsZero)
                    {
                        dataObject.Environment.Assign(Result, "Success", update);
                        AddDebugOutputItem(new DebugEvalResult(Result, "", dataObject.Environment, update));
                    }
                    else
                    {
                        allErrors.AddError(string.Format(ErrorResource.NullRecordSet, RecordsetName));
                    }

                }
            }
            catch(Exception e)
            {
                 allErrors.AddError(e.Message);
            }
            finally
            {
                // Handle Errors
                var hasErrors = allErrors.HasErrors();
                if(hasErrors)
                {
                    DisplayAndWriteError("DsfDeleteRecordsActivity", allErrors);
                    var errorString = allErrors.MakeDisplayReady();
                    dataObject.Environment.AddError(errorString);
                    if (!string.IsNullOrEmpty(Result))
                    {
                        dataObject.Environment.Assign(Result, "Failure", update);
                    }
                }

                if(dataObject.IsDebugMode())
                {
                    if(hasErrors)
                    {
                        AddDebugOutputItem(new DebugItemStaticDataParams("Failure", Result, ""));
                    }
                    DispatchDebugState(dataObject, StateType.Before, update);
                    DispatchDebugState(dataObject, StateType.After, update);
                }
            }
        }

        void GetDebug(IDSFDataObject dataObject, int update)
        {
            try
            {


                if (dataObject.IsDebugMode() && ExecutionEnvironment.IsRecordSetName(RecordsetName))
                {
                    AddDebugInputItem(new DebugEvalResult(RecordsetName, "Records", dataObject.Environment, update));
                }

            }
             
            catch(Exception)
            {

                AddDebugInputItem(new DebugItemWarewolfAtomResult("", RecordsetName, "Recordset", "", "", "", "="));
            }
        }

        #region Get Debug Inputs/Outputs

        #region GetDebugInputs


        #endregion

        #endregion Get Inputs/Outputs



        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            return _debugInputs;
        }

   

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            return _debugOutputs;
        }


        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            if(updates != null)
            {
                foreach(var t in updates)
                {

                    if(t.Item1 == RecordsetName)
                    {
                        RecordsetName = t.Item2;
                    }
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            var itemUpdate = updates?.FirstOrDefault(tuple => tuple.Item1 == Result);
            if(itemUpdate != null)
            {
                Result = itemUpdate.Item2;
            }
        }

        #region GetForEachInputs/Outputs

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(RecordsetName);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Result);
        }

        #endregion
    }
}
