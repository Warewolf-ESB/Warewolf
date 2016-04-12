using System;
using System.Activities;
using System.Collections.Generic;
using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.Util;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Storage;
// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Activities.SelectAndApply
{
    [ToolDescriptorInfo("Execution-ForEach", "Select and apply", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090D8C8FA3E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Loop Constructs", "/Warewolf.Studio.Themes.Luna;component/Images.xaml")]
    public class DsfSelectAndApplyActivity : DsfActivityAbstract<bool>
    {
        public DsfSelectAndApplyActivity()
        {
            DisplayName = "Select and apply";
        }

        #region Overrides of DsfNativeActivity<bool>
        public string DataSource { get; set; }
        public string Alias { get; set; }
        public object ApplyActivity { get; set; }

        readonly object _selectApplyExecutionObject = new object();
        string _previousParentId;

        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {

        }

        public override void UpdateDebugParentID(IDSFDataObject dataObject)
        {
            WorkSurfaceMappingId = Guid.Parse(UniqueID);
            UniqueID = dataObject.ForEachNestingLevel > 0 ? Guid.NewGuid().ToString() : UniqueID;
        }


        protected override void OnBeforeExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            _previousParentId = dataObject.ParentInstanceID;
        }

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            throw new NotImplementedException();
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            return GetForEachItems(Alias);
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            return GetForEachItems(Alias.Replace("*", ""));
        }

        #region Get Debug Inputs/Outputs

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

            return _debugOutputs;

        }

        #endregion Get Inputs/Outputs

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            lock (_selectApplyExecutionObject)
            {
                _previousParentId = dataObject.ParentInstanceID;
                _debugInputs = new List<DebugItem>();
                _debugOutputs = new List<DebugItem>();

                ErrorResultTO allErrors = new ErrorResultTO();
                InitializeDebug(dataObject);
                dataObject.ForEachNestingLevel++;
                try
                {
                    if (dataObject.IsDebugMode())
                    {
                        AddDebugInputItem(new DebugItemStaticDataParams(Alias, "As", DataSource));
                    }
                    //Eval list using DataSource
                    var atoms = dataObject.Environment.EvalAsList(dataObject.Environment.ToStar(DataSource), update);
                    //Create a new Execution Environment
                    var executionEnvironment = new ScopedEnvironment(dataObject.Environment,DataSource,Alias);

                    //Push the new environment
                    dataObject.PushEnvironment(executionEnvironment);

                    dataObject.ForEachNestingLevel++;
                    if (dataObject.IsDebugMode())
                    {
                        DispatchDebugState(dataObject, StateType.Before, update);
                    }
                    dataObject.ParentInstanceID = UniqueID;
                    dataObject.IsDebugNested = true;
                    if (dataObject.IsDebugMode())
                    {
                        DispatchDebugState(dataObject, StateType.After, update);
                    }
                    int upd = 0;
                    foreach (var warewolfAtom in atoms)
                    {
                        upd++;
                        
                        //Assign the warewolfAtom to Alias using new environment
                        executionEnvironment.Assign(Alias, warewolfAtom.ToString(), upd);

                        var exeAct = ApplyActivity as IDev2Activity;
                        if (exeAct != null)
                        {
                            exeAct.Execute(dataObject, upd);
                        }
                    }
                }
                catch (Exception e)
                {
                    Dev2Logger.Error("DSFSelectAndApply", e);
                    allErrors.AddError(e.Message);
                }
                finally
                {
                    dataObject.PopEnvironment();
                    
                    dataObject.ForEachNestingLevel--;
                    if (allErrors.HasErrors())
                    {
                        if (allErrors.HasErrors())
                        {
                            DisplayAndWriteError("DsfSelectAndApplyActivity", allErrors);
                            foreach (var fetchError in allErrors.FetchErrors())
                            {
                                dataObject.Environment.AddError(fetchError);
                            }
                        }
                    }
                    if (dataObject.IsDebugMode())
                    {
                        var data = dataObject.Environment.Eval(dataObject.Environment.ToStar(DataSource), update, false);
                        if (data.IsWarewolfAtomListresult)
                        {
                            var lst = data as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                            AddDebugOutputItem(new DebugItemWarewolfAtomListResult(lst, "", "", DataSource, "", "", "="));
                        }
                       
                        var dt = DateTime.Now;
                        DispatchDebugState(dataObject, StateType.End, update, dt);
                    }
                    dataObject.ParentInstanceID = _previousParentId;
                    dataObject.IsDebugNested = false;
                }
            }
        }
        #endregion
    }
}
