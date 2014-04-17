using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2;
using Dev2.Activities;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Dev2.Diagnostics;
using Dev2.Enums;

// ReSharper disable CheckNamespace
namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DsfSequenceActivity : DsfActivityAbstract<string>
    {
        private Sequence innerSequence = new Sequence();
        string _previousParentID;

        public DsfSequenceActivity()
        {
            DisplayName = "Sequence";
            Activities = new Collection<Activity>();
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.AddChild(innerSequence);
        }


        public Collection<Activity> Activities
        {
            get;
            set;
        }

        #region Get Debug Inputs/Outputs

        public List<DebugItem> GetDebugInputs()
        {
            return DebugItem.EmptyList;
        }

        public override List<DebugItem> GetDebugOutputs(IBinaryDataList dataList)
        {
            return DebugItem.EmptyList;
        }

        #endregion Get Inputs/Outputs

        #region GetForEachInputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            foreach(var activity in Activities)
            {
                var innerActivity = activity as DsfActivityAbstract<string>;
                if(innerActivity != null)
                {
                    innerActivity.UpdateForEachInputs(updates, context);
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            foreach(var activity in Activities)
            {
                var innerActivity = activity as DsfActivityAbstract<string>;
                if(innerActivity != null)
                {
                    innerActivity.UpdateForEachOutputs(updates, context);
                }
            }
        }

        public override IList<DsfForEachItem> GetForEachInputs()
        {
            var forEachInputs = new List<DsfForEachItem>();

            foreach(var activity in Activities)
            {
                var innerActivity = activity as DsfActivityAbstract<string>;
                if(innerActivity != null)
                {
                    forEachInputs.AddRange(innerActivity.GetForEachInputs());
                }
            }

            return forEachInputs;
        }

        public override IList<DsfForEachItem> GetForEachOutputs()
        {
            var forEachOutputs = new List<DsfForEachItem>();

            foreach(var activity in Activities)
            {
                var innerActivity = activity as DsfActivityAbstract<string>;
                if(innerActivity != null)
                {
                    forEachOutputs.AddRange(innerActivity.GetForEachOutputs());
                }
            }

            return forEachOutputs;
        }

        #endregion

        #region Overrides of DsfNativeActivity<string>

        protected override void OnBeforeExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            _previousParentID = dataObject.ParentInstanceID;
        }

        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
            InitializeDebug(context.GetExtension<IDSFDataObject>());
            DispatchDebugState(context, StateType.Before);
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            dataObject.ParentInstanceID = UniqueID;
            dataObject.IsDebugNested = true;
            innerSequence.Activities.Clear();
            foreach(var dsfActivity in Activities)
            {
                innerSequence.Activities.Add(dsfActivity);
            }
            context.ScheduleActivity(innerSequence, OnCompleted);
            DispatchDebugState(context, StateType.After);
        }

        void OnCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            dataObject.IsDebugNested = false;
            dataObject.ParentInstanceID = _previousParentID;
            IDataListCompiler compiler = DataListFactory.CreateDataListCompiler();
            if(compiler != null)
            {
                DoErrorHandling(context, compiler, dataObject);
            }
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.Sequence;
        }

        #endregion
    }
}