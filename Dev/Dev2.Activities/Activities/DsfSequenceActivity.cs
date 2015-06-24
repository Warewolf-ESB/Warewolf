
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Diagnostics;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Storage;

namespace Dev2.Activities
{
    public class DsfSequenceActivity : DsfActivityAbstract<string>
    {
        private readonly Sequence _innerSequence = new Sequence();
        string _previousParentID;

        public DsfSequenceActivity()
        {
            DisplayName = "Sequence";
            Activities = new Collection<Activity>();
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            base.CacheMetadata(metadata);
            metadata.AddChild(_innerSequence);
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

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment dataList)
        {
            return DebugItem.EmptyList;
        }

        #endregion Get Inputs/Outputs

        #region GetForEachInputs/Outputs

        public override void UpdateForEachInputs(IList<Tuple<string, string>> updates)
        {
            foreach(var activity in Activities)
            {
                var innerActivity = activity as DsfActivityAbstract<string>;
                if(innerActivity != null)
                {
                    innerActivity.UpdateForEachInputs(updates);
                }
            }
        }

        public override void UpdateForEachOutputs(IList<Tuple<string, string>> updates)
        {
            foreach(var activity in Activities)
            {
                var innerActivity = activity as DsfActivityAbstract<string>;
                if(innerActivity != null)
                {
                    innerActivity.UpdateForEachOutputs(updates);
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
        public override void UpdateDebugParentID(IDSFDataObject dataObject)
        {
            WorkSurfaceMappingId = Guid.Parse(UniqueID);
            UniqueID = dataObject.ForEachNestingLevel > 0 ? Guid.NewGuid().ToString() : UniqueID;
        }
        /// <summary>
        /// When overridden runs the activity's execution logic 
        /// </summary>
        /// <param name="context">The context to be used.</param>
        protected override void OnExecute(NativeActivityContext context)
        {
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            dataObject.ForEachNestingLevel++;
            InitializeDebug(dataObject);
            if (dataObject.IsDebugMode())
            {
                DispatchDebugState(dataObject, StateType.Before);
            }
            dataObject.ParentInstanceID = UniqueID;
            dataObject.IsDebugNested = true;
            _innerSequence.Activities.Clear();
            foreach (var dsfActivity in Activities)
            {
                _innerSequence.Activities.Add(dsfActivity);
            }

            if (dataObject.IsDebugMode())
            {
                DispatchDebugState(dataObject, StateType.After);
            }
            context.ScheduleActivity(_innerSequence, OnCompleted);
        }

        protected override void ExecuteTool(IDSFDataObject dataObject)
        {
            _previousParentID = dataObject.ParentInstanceID;
            dataObject.ForEachNestingLevel++;
            InitializeDebug(dataObject);
            if(dataObject.IsDebugMode())
            {
                DispatchDebugState(dataObject, StateType.Before);
            }
            dataObject.ParentInstanceID = UniqueID;
            dataObject.IsDebugNested = true;
            if (dataObject.IsDebugMode())
            {
                DispatchDebugState(dataObject, StateType.After);
            }
           foreach(var dsfActivity in Activities)
            {
                var act = dsfActivity as IDev2Activity;
                if (act != null)
                {
                    act.Execute(dataObject);
                }
            }
            
            OnCompleted(dataObject);
        }

        void OnCompleted(NativeActivityContext context, ActivityInstance completedInstance)
        {
            IDSFDataObject dataObject = context.GetExtension<IDSFDataObject>();
            OnCompleted(dataObject);
        }

        void OnCompleted(IDSFDataObject dataObject)
        {
            dataObject.IsDebugNested = false;
            dataObject.ParentInstanceID = _previousParentID;
            dataObject.ForEachNestingLevel--;
        }

        public override enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.Sequence;
        }

        #endregion
    }
}
