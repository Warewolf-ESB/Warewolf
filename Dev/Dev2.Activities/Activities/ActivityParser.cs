#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities;
using System.Activities.Presentation.Model;
using System.Activities.Statements;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Activities.SelectAndApply;
using Dev2.Common;
using Dev2.Data.SystemTemplates.Models;
using Newtonsoft.Json;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Resource.Errors;

namespace Dev2.Activities
{
    public class ActivityParser : IActivityParser
    {
        private ConcurrentDictionary<string, string> _activitiesCache;
        private ConcurrentDictionary<string, string> ActivityCache => _activitiesCache;

        public ActivityParser()
        {
            _activitiesCache = new ConcurrentDictionary<string, string>();
        }

        public ActivityParser(string notInUse) : this()
        {
            //PBI: this added for us with CustomContainer.CreateInstance
        }

        public IDev2Activity Parse(List<IDev2Activity> seenActivities, object flowChart)
        {
            var modelItem = flowChart as ModelItem;
            var currentValue = modelItem?.GetCurrentValue();
            if (currentValue is null)
            {
                return default;
            }

            if (currentValue is FlowStep start)
            {
                var tool = ParseTools(start, seenActivities);
                return tool.FirstOrDefault();
            }
            if (currentValue is FlowSwitch<string> flowstart)
            {
                return ParseSwitch(flowstart, seenActivities).FirstOrDefault();
            }
            var flowdec = currentValue as FlowDecision;
            return ParseDecision(flowdec, seenActivities).FirstOrDefault();
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
#pragma warning disable S3776 // Cognitive Complexity of methods should not be too high
        public IEnumerable<IDev2Activity> ParseToLinkedFlatList(IDev2Activity topLevelActivity)
#pragma warning restore S3776 // Cognitive Complexity of methods should not be too high
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            var guids = new ConcurrentDictionary<Guid, string>();
            if (topLevelActivity is DsfDecision roodDecision)
            {
                IEnumerable<IDev2Activity> vb;
                if (roodDecision.TrueArm == null)
                {
                    vb = roodDecision.FalseArm;
                }
                else if (roodDecision.FalseArm == null)
                {
                    vb = roodDecision.TrueArm;
                }
                else
                {
                    vb = roodDecision.FalseArm.Union(roodDecision.TrueArm);
                }

                var bbb = vb.Flatten(activity =>
                {
                    if (activity.NextNodes != null)
                    {
                        return activity.NextNodes;
                    }

                    if (activity is DsfDecision a)
                    {
                        if (a.TrueArm == null)
                        {
                            return a.FalseArm;
                        }

                        if (a.FalseArm == null)
                        {
                            return a.TrueArm;
                        }

                        var activities = a.FalseArm.Union(a.TrueArm);
                        return activities;
                    }
                    return new List<IDev2Activity>();
                }).ToList();
                var hasDecision = bbb.Contains(topLevelActivity);
                if (!hasDecision)
                {
                    bbb.Add(topLevelActivity);
                }
                return bbb.ToList();
            }
            var nextNodes = (topLevelActivity.GetType() == typeof(DsfSwitch)) ? ((DsfSwitch)topLevelActivity).GetNextNodes() : topLevelActivity.NextNodes;

            var dev2Activities = nextNodes?.Flatten(activity =>
            {
                var uniqueId = Guid.Parse(activity.UniqueID);
                var displayName = activity.GetDisplayName();

                if (activity.NextNodes != null && !guids.ContainsKey(uniqueId))
                {
                    guids.TryAdd(uniqueId, displayName);
                    return activity.NextNodes;
                }

                if (activity is DsfDecision a)
                {
                    if (a.TrueArm == null)
                    {
                        return a.FalseArm;
                    }

                    if (a.FalseArm == null)
                    {
                        return a.TrueArm;
                    }

                    var activities = a.FalseArm.Union(a.TrueArm);
                    return activities;
                }
                if (activity is DsfSwitch b)
                {
                    var vv = b.Switches.ToDictionary(k => k.Key);
                    var activities = vv.Values.Select(k => k.Value).Union(b.Default);
                    return activities;
                }
                if (activity is DsfForEachActivity c)
                {
                    var dev2Activity = (c.DataFunc.Handler as IDev2Activity);
                    return dev2Activity?.NextNodes ?? new List<IDev2Activity>();
                }
                if (activity is DsfSelectAndApplyActivity d)
                {
                    var dev2Activity = (d.ApplyActivityFunc.Handler as IDev2Activity);
                    return dev2Activity?.NextNodes ?? new List<IDev2Activity>();
                }
                return new List<IDev2Activity>();
            }).ToList() ?? new List<IDev2Activity>();
            var contains = dev2Activities.Contains(topLevelActivity);
            if (!contains)
            {
                dev2Activities.Add(topLevelActivity);
            }
            return dev2Activities;
        }

        public IEnumerable<IDev2Activity> FlattenNextNodesInclusive(IDev2Activity firstOrDefault)
        {
            switch (firstOrDefault)
            {
                case DsfDecision a:
                    {
                        return new List<IDev2Activity>() { a };
                    }
                case DsfSwitch b:
                    {
                        return new List<IDev2Activity>() { b };
                    }
                default:
                    {
                        var truArmToFlatList = ActivityToFlatList(firstOrDefault);
                        return truArmToFlatList;
                    }
            }
        }


        private static List<IDev2Activity> ActivityToFlatList(IDev2Activity decision)
        {
            var truArmToFlatList =
                decision?.NextNodes?.Flatten(activity => activity.NextNodes ?? new List<IDev2Activity>()).ToList() ??
                new List<IDev2Activity>();
            var contains = truArmToFlatList.Contains(decision);
            if (!contains)
            {
                truArmToFlatList.Add(decision);
            }
            return truArmToFlatList;
        }

        public IDev2Activity Parse(DynamicActivity dynamicActivity, List<IDev2Activity> seenActivities)
        {
            try
            {
                if (WorkflowInspectionServices.GetActivities(dynamicActivity).FirstOrDefault() is Flowchart chart)
                {
                    if (chart.StartNode == null)
                    {
                        return null;
                    }

                    if (chart.StartNode is FlowStep start)
                    {
                        var tool = ParseTools(start, seenActivities);
                        return tool.FirstOrDefault();
                    }

                    if (chart.StartNode is FlowSwitch<string> flowstart)
                    {
                        return ParseSwitch(flowstart, seenActivities).FirstOrDefault();
                    }

                    var flowdec = chart.StartNode as FlowDecision;
                    return ParseDecision(flowdec, seenActivities).FirstOrDefault();
                }
                return null;
            }
            catch (InvalidWorkflowException e)
            {

                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                throw;
            }
        }

        public IDev2Activity ParseWithCache(DynamicActivity dynamicActivity, Guid workspaceID, Guid resourceIdGuid, List<IDev2Activity> seenActivities)
        {
            IDev2Activity result = null;
            try
            {
                if (WorkflowInspectionServices.GetActivities(dynamicActivity).FirstOrDefault() is Flowchart chart)
                {
                    if (chart.StartNode == null)
                    {
                        return null;
                    }

                    IDev2Activity cachedActivity = null;
                    string activityId = null;
                    if (chart.StartNode is FlowStep start)
                    {
                        cachedActivity = GetActivityFromCache(workspaceID, resourceIdGuid, out activityId);//GetActivityFromCache(start, out activityId);
                        if (cachedActivity != null) { return cachedActivity; }

                        var tool = ParseTools(start, seenActivities);
                        result = tool.FirstOrDefault();

                        // Serialize and Cache result
                        //activityId = string.Concat(workspaceID, resourceIdGuid);
                        AddActivityToCache(result, activityId);
                        return result;
                    }
                    if (chart.StartNode is FlowSwitch<string> flowstart)
                    {
                        cachedActivity = GetActivityFromCache(workspaceID, resourceIdGuid, out activityId);//GetActivityFromCache(flowstart, out activityId);
                        if (cachedActivity != null) { return cachedActivity; }

                        result = ParseSwitch(flowstart, seenActivities).FirstOrDefault();
                        // Serialize and Cache result 
                        //activityId = string.Concat(workspaceID, resourceIdGuid);
                        AddActivityToCache(result, activityId);
                        return result;
                    }

                    var flowdec = chart.StartNode as FlowDecision;

                    cachedActivity = GetActivityFromCache(workspaceID, resourceIdGuid, out activityId);//GetActivityFromCache(flowdec, out activityId);
                    if (cachedActivity != null) { return cachedActivity; }

                    result = ParseDecision(flowdec, seenActivities).FirstOrDefault();
                    // Serialize and Cache result 
                    //activityId = string.Concat(workspaceID, resourceIdGuid);
                    AddActivityToCache(result, activityId);
                    return result;
                }
                return null;
            }
            catch (InvalidWorkflowException e)
            {

                Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                throw;
            }
        }

        private void AddActivityToCache(IDev2Activity result, string activityId)
        {
            if (string.IsNullOrEmpty(activityId)) return;

            if (result == null) { return; }

            var serializedActivityString = SerializeActivity(result);
            if (string.IsNullOrEmpty(serializedActivityString)) return;

            AddSerializedActivityToCache(activityId, serializedActivityString);

            System.Diagnostics.Debug.WriteLine("AddActivityToCache: {0} => {1}", activityId, result.GetDisplayName());

        }

        //private IDev2Activity GetActivityFromCache(FlowNode start, out string activityId)
        //{
        //    activityId = GetActivityId(start);
        //    return GetActivityFromCache(activityId);
        //}
        private IDev2Activity GetActivityFromCacheInternal(string activityId)
        {
            if (string.IsNullOrEmpty(activityId)) return null;

            var serializedActivity = GetSerializedActivity(activityId);
            if (string.IsNullOrEmpty(serializedActivity)) return null;

            return DeserializeActivity(serializedActivity);
        }

        public IDev2Activity GetActivityFromCache(Guid workspaceID, Guid resourceIdGuid)
        {
            var activityId = string.Concat(workspaceID, resourceIdGuid);

            return GetActivityFromCacheInternal(activityId);
        }

        public IDev2Activity GetActivityFromCache(Guid workspaceID, Guid resourceIdGuid, out string activityId)
        {
            activityId = string.Concat(workspaceID, resourceIdGuid);
            return GetActivityFromCacheInternal(activityId);
        }



        IEnumerable<IDev2Activity> ParseTools(FlowNode startNode, List<IDev2Activity> seenActivities)
        {

            if (startNode == null)
            {
                return null;
            }

            if (startNode is FlowStep step)
            {
                return ParseFlowStep(step, seenActivities);
            }

            if (startNode is FlowDecision node)
            {
                return ParseDecision(node, seenActivities);
            }

            if (startNode is FlowSwitch<string> @switch)
            {
                return ParseSwitch(@switch, seenActivities);
            }

            return null;

        }

        IEnumerable<IDev2Activity> ParseSwitch(FlowSwitch<string> switchFlowSwitch, List<IDev2Activity> seenActivities)
        {
            var activity = switchFlowSwitch.Expression as DsfFlowSwitchActivity;
            if (activity != null)
            {
                if (seenActivities.Contains(activity))
                {
                    return new List<IDev2Activity> { activity };
                }

                var val = new StringBuilder(Dev2DecisionStack.ExtractModelFromWorkflowPersistedData(activity.ExpressionText));
                var ds = new Dev2Switch { SwitchVariable = val.ToString() };
                var swi = new DsfSwitch(activity);
                if (!seenActivities.Contains(activity))
                {
                    seenActivities.Add(swi);
                }
                swi.Switches = switchFlowSwitch.Cases.Select(a => new Tuple<string, IDev2Activity>(a.Key, ParseTools(a.Value, seenActivities).FirstOrDefault())).ToDictionary(a => a.Item1, a => a.Item2);
                swi.Default = ParseTools(switchFlowSwitch.Default, seenActivities);
                swi.Switch = ds.SwitchVariable;
                swi.DisplayName = switchFlowSwitch.DisplayName;
                swi.OnErrorVariable = activity.OnErrorVariable;
                swi.OnErrorWorkflow = activity.OnErrorWorkflow;
                swi.IsEndedOnError = activity.IsEndedOnError;
                swi.Inner.DisplayName = swi.DisplayName;
                return new List<IDev2Activity>
                {  swi
                };
            }
            throw new Exception(ErrorResource.InvalidActivity);

        }

        IEnumerable<IDev2Activity> ParseDecision(FlowDecision decision, List<IDev2Activity> seenActivities)
        {

            var activity = decision.Condition as DsfFlowDecisionActivity;
            if (activity != null)
            {
                if (seenActivities.Contains(activity))
                {
                    return new List<IDev2Activity> { activity };
                }
                var rawText = activity.ExpressionText;

                var activityTextjson = rawText.Substring(rawText.IndexOf("{", StringComparison.Ordinal)).Replace(@""",AmbientDataList)", "").Replace("\"", "!");

                var activityText = Dev2DecisionStack.FromVBPersitableModelToJSON(activityTextjson);
                var decisionStack = JsonConvert.DeserializeObject<Dev2DecisionStack>(activityText);
                var dec = new DsfDecision(activity);
                if (!seenActivities.Contains(activity))
                {
                    seenActivities.Add(dec);
                }
                dec.TrueArm = ParseTools(decision.True, seenActivities);
                dec.FalseArm = ParseTools(decision.False, seenActivities);
                dec.Conditions = decisionStack;
                dec.And = decisionStack.Mode == Dev2DecisionMode.AND;


                return new List<IDev2Activity>
                { dec};
            }
            throw new Exception(ErrorResource.InvalidActivity);
        }

        IEnumerable<IDev2Activity> ParseFlowStep(FlowStep startNode, List<IDev2Activity> seenActivities)
        {
            if (!(startNode.Action is IDev2Activity action))
            {
                return null;
            }

            if (seenActivities.Contains(action))
            {
                return new List<IDev2Activity> { action };
            }

            if (!seenActivities.Contains(action))
            {
                seenActivities.Add(action);
            }
            var tools = ParseTools(startNode.Next, seenActivities);

            action.NextNodes = tools;

            return new List<IDev2Activity> { action };
        }

        public IDev2Activity Parse(DynamicActivity dynamicActivity) => Parse(dynamicActivity, new List<IDev2Activity>());

        public IDev2Activity ParseWithCache(DynamicActivity dynamicActivity, Guid workspaceID, Guid resourceIdGuid) => ParseWithCache(dynamicActivity, workspaceID, resourceIdGuid, new List<IDev2Activity>());

        private static string GetActivityId(FlowNode startNode)
        {

            if (startNode == null)
            {
                return null;
            }

            if (startNode is FlowStep step)
            {
                return GetFlowStepId(step);
            }

            if (startNode is FlowDecision node)
            {
                return GetFlowDecisionId(node);
            }

            if (startNode is FlowSwitch<string> @switch)
            {
                return GetFlowSwitchId(@switch);
            }

            return null;

        }

        private static string GetFlowSwitchId(FlowSwitch<string> switchFlowSwitch)
        {
            var activity = switchFlowSwitch.Expression as DsfFlowSwitchActivity;
            if (activity != null) return activity.UniqueID;
            return null;
        }

        private static string GetFlowDecisionId(FlowDecision decision)
        {
            var activity = decision.Condition as DsfFlowDecisionActivity;
            if (activity != null) return activity.UniqueID;
            return null;
        }

        private static string GetFlowStepId(FlowStep step)
        {
            if (step.Action is IDev2Activity action)
            {
                return action.UniqueID;
            }

            return null;
        }

        public bool AddSerializedActivityToCache(string activityId, string serializedActivityString)
        {
            try
            {
                return ActivityCache.TryAdd(activityId, serializedActivityString);
            }
            catch 
            {
                return false;
            }
        }

        public string GetSerializedActivity(string activityId) => ActivityCache.TryGetValue(activityId, out string value) ? value : null;

        public bool HasSerializedActivityInCache(string activityId) => ActivityCache.ContainsKey(activityId);



        public void ClearSerializedActivityCache()
        {
            _activitiesCache = new ConcurrentDictionary<string, string>();
        }

        public bool RemoveFromSerializedActivityCache(Guid workspaceID, Guid resourceIdGuid)
        {
            var activityId = string.Concat(workspaceID, resourceIdGuid);
            return RemoveSerializedActivityFromCache(activityId);
        }

        public bool RemoveSerializedActivityFromCache(string activityId)
        {
            try
            {
                return ActivityCache.TryRemove(activityId, out string act);
            }
            catch
            {
                return false;
            }
        }


        #region IDev2Activity Serialization - Deserialization
        readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects
        };
        readonly JsonSerializerSettings _deSerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        };

        private IDev2Activity DeserializeActivity(string serializedActivity)
        {
            try
            {
                return JsonConvert.DeserializeObject(serializedActivity, _deSerializerSettings) as IDev2Activity;
            }
            catch
            {
                return null;
            }
        }

        private string SerializeActivity(IDev2Activity activity)
        {
            try
            {
                return JsonConvert.SerializeObject(activity, _serializerSettings);
            }
            catch 
            {
                return null;
            }
        }
        #endregion


    }
}