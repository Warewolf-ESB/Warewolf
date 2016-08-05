using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using System;
using System.Activities;
using System.Collections.Generic;
using Dev2.Interfaces;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage;

// ReSharper disable MemberCanBePrivate.Global

namespace Dev2.Activities.SelectAndApply
{
    [ToolDescriptorInfo("SelectApply", "Select and apply", ToolType.Native, "8999E59A-38A3-43BB-A98F-6090D8C8FA3E", "Dev2.Acitivities", "1.0.0.0", "Legacy", "Loop Constructs", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "")]
    public class DsfSelectAndApplyActivity : DsfActivityAbstract<bool>
    {
        public DsfSelectAndApplyActivity()
        {
            DisplayName = "Select and apply";
            ApplyActivityFunc = new ActivityFunc<string, bool>
            {
                DisplayName = "Data Action",
                Argument = new DelegateInArgument<string>(string.Format("explicitData_{0}", DateTime.Now.ToString("yyyyMMddhhmmss")))
            };
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            metadata.AddDelegate(ApplyActivityFunc);

            base.CacheMetadata(metadata);
        }

        #region Overrides of DsfNativeActivity<bool>
        [FindMissing]
        public string DataSource { get; set; }
        [FindMissing]
        public string Alias { get; set; }
        public ActivityFunc<string, bool> ApplyActivityFunc { get; set; }

        private readonly object _selectApplyExecutionObject = new object();
        private string _previousParentId;

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

        #endregion Get Debug Inputs/Outputs

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {

            ErrorResultTO allErrors = new ErrorResultTO();
            InitializeDebug(dataObject);

            if (string.IsNullOrEmpty(DataSource))
            {
                allErrors.AddError(ErrorResource.DataSourceEmpty);
            }
            if (string.IsNullOrEmpty(Alias))
            {
                allErrors.AddError(string.Format(ErrorResource.CanNotBeEmpty, "Alias"));
            }
            if (allErrors.HasErrors())
            {
                DisplayAndWriteError("DsfSelectAndApplyActivity", allErrors);
                foreach (var fetchError in allErrors.FetchErrors())
                {
                    dataObject.Environment.AddError(fetchError);
                }
                return;
            }
            lock (_selectApplyExecutionObject)
            {
                var startTime = DateTime.Now;
                _previousParentId = dataObject.ParentInstanceID;
                _debugInputs = new List<DebugItem>();
                _debugOutputs = new List<DebugItem>();

                dataObject.ForEachNestingLevel++;
                var ds = dataObject.Environment.ToStar(DataSource);
                var expressions = dataObject.Environment.GetIndexes(ds);
                if (expressions.Count == 0)
                {
                    expressions.Add(ds);
                }
                try
                {
                    if(dataObject.IsDebugMode())
                    {
                        AddDebugInputItem(new DebugItemStaticDataParams(Alias, "As", DataSource));
                    }
                   
                    var executionEnvironment = new ScopedEnvironment(dataObject.Environment, ds, Alias);

                    //Push the new environment
                    dataObject.PushEnvironment(executionEnvironment);
                    dataObject.ForEachNestingLevel++;
                    if(dataObject.IsDebugMode())
                    {
                        DispatchDebugState(dataObject, StateType.Before, update);
                    }
                    dataObject.ParentInstanceID = UniqueID;
                    dataObject.IsDebugNested = true;
                    if (dataObject.IsDebugMode())
                    {
                        DispatchDebugState(dataObject, StateType.After, update);
                    }
                    foreach(var exp in expressions)
                    {

                        //Assign the warewolfAtom to Alias using new environment
                        executionEnvironment.SetDataSource(exp);

                        var exeAct = ApplyActivityFunc.Handler as IDev2Activity;
                        if(exeAct != null)
                        {
                            exeAct.Execute(dataObject, 0);
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
                        foreach(var expression in expressions)
                        {
                            var data = dataObject.Environment.Eval(expression, update, false);
                            if (data.IsWarewolfAtomListresult)
                            {
                                var lst = data as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult;
                                AddDebugOutputItem(new DebugItemWarewolfAtomListResult(lst, "", "", expression, "", "", "="));
                            }
                            else
                            {
                                if (data.IsWarewolfAtomResult)
                                {
                                    var atom = data as CommonFunctions.WarewolfEvalResult.WarewolfAtomResult;
                                    if (atom != null)
                                        AddDebugOutputItem(new DebugItemWarewolfAtomResult(atom.Item.ToString(), expression, ""));
                                }
                            }
                        }                        
                        DispatchDebugState(dataObject, StateType.End, update, startTime,DateTime.Now);
                    }
                    dataObject.ParentInstanceID = _previousParentId;
                    dataObject.IsDebugNested = false;
                }
            }
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.ForEach;
        }
        #endregion Overrides of DsfNativeActivity<bool>
    }
}