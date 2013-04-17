using System;
using System.Activities;
using Dev2;
using Dev2.Diagnostics;

namespace Unlimited.Applications.BusinessDesignStudio.Activities
{
    public class DsfWorkflowActivity : DsfNativeActivity<bool>
    {
        Activity _activity;
        string _previousParentID;

        #region Ctor

        public DsfWorkflowActivity(Activity activity, string displayName)
            : base(true, null)
        {
            Initialize(activity, displayName);
        }

        public DsfWorkflowActivity(Activity activity, string displayName, IDebugDispatcher debugDispatcher)
            : base(true, null, debugDispatcher)
        {
            Initialize(activity, displayName);
        }

        void Initialize(Activity activity, string displayName)
        {
            if(activity == null)
            {
                throw new ArgumentNullException("activity");
            }
            _activity = activity;
            DisplayName = displayName;
            IsWorkflow = true;
        }

        #endregion

        #region CacheMetadata

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.AddChild(_activity);
        }

        #endregion

        protected override void OnBeforeExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            if(string.IsNullOrEmpty(dataObject.ParentInstanceID))
            {
                dataObject.ParentInstanceID = InstanceID;
            }
        }

        #region OnExecute

        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            _previousParentID = dataObject.ParentInstanceID;
            dataObject.ParentInstanceID = InstanceID;
            context.ScheduleActivity(_activity, OnCompleted, OnFaulted);
        }

        #endregion

        #region OnCompleted

        void OnCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            dataObject.ParentInstanceID = _previousParentID;
            OnExecutedCompleted(context, false, dataObject.WorkflowResumeable);
        }

        #endregion

        #region OnFaulted

        void OnFaulted(NativeActivityFaultContext context, Exception propagatedException, ActivityInstance propagatedFrom)
        {
            OnExecutedCompleted(context, true, false);
        }

        #endregion

        public override void UpdateForEachInputs(System.Collections.Generic.IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }

        public override void UpdateForEachOutputs(System.Collections.Generic.IList<Tuple<string, string>> updates, NativeActivityContext context)
        {
            throw new NotImplementedException();
        }
    }
}
