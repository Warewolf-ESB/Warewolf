#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.Debug;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.ServiceModel;
using Dev2.Diagnostics;
using Dev2.Util;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Data.Util;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;
using Dev2.Common.State;
using Warewolf.Interfaces;
using Warewolf.Driver.Redis;
using System.Activities;
using Dev2.Common.Serializers;
using Warewolf.Storage;
using Dev2.Common.Interfaces.Communication;
using Dev2.Data.TO;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using WarewolfParserInterop;
using Dev2.Common.Interfaces;
using Dev2.MathOperations;
using System.Globalization;
using Dev2.Data.Interfaces.Enums;

namespace Dev2.Activities.Redis
{
    [ToolDescriptorInfo("Redis", "Redis Cache", ToolType.Native, "416eb671-64df-4c82-c6f0-43e48172a799", "Dev2.Activities", "1.0.0.0", "Legacy", "Utility", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Utility_Redis")]
    public class RedisActivity : DsfBaseActivity, IEquatable<RedisActivity>
    {
        string _result = "Success";
        private readonly ISerializer _serializer;

        private RedisCacheBase _redisCache;
        internal List<string> _messages = new List<string>();

        public RedisActivity()
             : this(Dev2.Runtime.Hosting.ResourceCatalog.Instance, new ResponseManager(), null)
        {

        }

        public RedisActivity(IResourceCatalog resourceCatalog, RedisCacheBase redisCache)
            : this(resourceCatalog, new ResponseManager(), redisCache)
        {
        }

        public RedisActivity(IResourceCatalog resourceCatalog, ResponseManager responseManager, RedisCacheBase redisCache)
        {
            ResponseManager = responseManager;
            _redisCache = redisCache;
            ResourceCatalog = resourceCatalog;

            DisplayName = "Redis Cache";
            ActivityFunc = new ActivityFunc<string, bool>
            {
                DisplayName = "Data Action",
                Argument = new DelegateInArgument<string>($"explicitData_{DateTime.Now:yyyyMMddhhmmss}")
            };

            _serializer = new Dev2JsonSerializer();

        }

        public Guid SourceId { get; set; }

        public IDev2Activity _innerActivity { get; private set; }

        [Inputs("Key")]
        [FindMissing]
        public string Key { get; set; }

        [FindMissing]
        public string Response { get; set; }

        [FindMissing]
        public int TTL { get; set; }

        public bool ShouldSerializeConsumer() => false;

        public bool ShouldSerializeConnectionFactory() => false;

        internal IRedisConnection Connection { get; set; }

        public bool ShouldSerializeConnection() => false;

        public ActivityFunc<string, bool> ActivityFunc { get; set; }

        public override IEnumerable<StateVariable> GetState()
        {
            return new[] {
                new StateVariable
                {
                    Name = "Key",
                    Value = Key,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "TTL",
                    Value = TTL.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name="Result",
                    Value = Result,
                    Type = StateVariable.StateType.Output
                }
                ,
                new StateVariable
                {
                    Name="Response",
                    Value = Response,
                    Type = StateVariable.StateType.Output
                },
                new StateVariable
                {
                    Name = "SourceId",
                    Value = SourceId.ToString(),
                    Type = StateVariable.StateType.Input
                }
            };
        }

        public RedisSource RedisSource { get; set; }

        protected override List<string> PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            try
            {
                RedisSource = ResourceCatalog.GetResource<RedisSource>(GlobalConstants.ServerWorkspaceID, SourceId);
                if (RedisSource == null || RedisSource.ResourceType != enSourceType.RedisSource.ToString())
                {
                    _messages.Add(ErrorResource.RedisSourceHasBeenRemoved);
                    return _messages;
                }
                _redisCache = new RedisCacheImpl(RedisSource.HostName, Convert.ToInt32(RedisSource.Port), RedisSource.Password);

                var cacheTTL = TimeSpan.FromMilliseconds(TTL);

                _innerActivity = ActivityFunc.Handler as IDev2Activity;
                if (_innerActivity is null)
                {
                    throw new InvalidOperationException($"Activity drop box cannot be null");
                }
                if (string.IsNullOrWhiteSpace(_innerActivity.GetOutputs()[0]))
                {
                    throw new InvalidOperationException($"{_innerActivity.GetDisplayName()} activity must have at least one output variable.");
                }

                IDictionary<string, string> cachedData = GetCachedOutputs();
                if (cachedData != null)
                {
                    foreach (var item in cachedData)
                    {
                        DataObject.Environment.Assign(item.Key, item.Value, 0);
                    }
                }
                else
                {
                    _innerActivity.Execute(DataObject, 0);
                    var outputVars = _innerActivity.GetOutputs();

                    CacheOutputs(cacheTTL, outputVars);
                }
                return new List<string> { _result };
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(nameof(RedisActivity), ex, GlobalConstants.WarewolfError);
                throw new Exception(ex.GetAllMessages());
            }
        }

        private IDictionary<string, string> GetCachedOutputs()
        {
            var cachedData = _redisCache.Get(Key);
            if (cachedData is null)
            {
                return null;
            }
            var outputs = _serializer.Deserialize<IDictionary<string, string>>(cachedData);
            return outputs;
        }

        private void CacheOutputs(TimeSpan cacheTTL, List<string> outputVars)
        {
            var data = new Dictionary<string, string>();
            foreach (var output in outputVars)
            {
                var key = output;
                var value = ExecutionEnvironment.WarewolfEvalResultToString(DataObject.Environment.Eval(output, 0));

                data.Add(key, value);
            }

            _redisCache.Set(Key, _serializer.Serialize<Dictionary<string, string>>(data), cacheTTL);
        }

        public override List<string> GetOutputs() => new List<string> { Response, Result };

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            if (update == 0 && _debugInputs.Count > 1)
            {
                return _debugInputs;
            }

            if (env == null)
            {
                return new List<DebugItem>();
            }
            var debugItem = new DebugItem();
            AddDebugItem(new DebugEvalResult(Key, "Key", env, update), debugItem);

            var debugItemm = new DebugItem();
            var innerCount = 1;
            foreach (var t in GetAssignValue(_innerActivity.GetOutputs()))
            {
                 debugItemm = TryCreateDebugInput(env, innerCount++, t, update);
                _debugInputs.Add(debugItemm);
            }

            return _debugInputs;
        }

        private void AddEvaluatedDebugItem(IExecutionEnvironment environment, int innerCount, IAssignValue assignValue, int update, string VariableLabelText, string NewFieldLabelText, DebugItem debugItem)
        {
            if (DataListUtil.IsEvaluated(assignValue.Value))
            {
                var evalResult = environment.Eval(assignValue.Name, update);
                AddDebugItem(new DebugItemStaticDataParams("", innerCount.ToString(CultureInfo.InvariantCulture)), debugItem);
                if (evalResult.IsWarewolfAtomResult && evalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult scalarResult)
                {
                    AddDebugItem(new DebugItemWarewolfAtomResult(ExecutionEnvironment.WarewolfAtomToString(scalarResult.Item), "", environment.EvalToExpression(assignValue.Name, update), "", VariableLabelText, NewFieldLabelText, "="), debugItem);
                }

                var evalResult2 = environment.Eval(assignValue.Value, update);
                if (evalResult.IsWarewolfAtomListresult && evalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recSetResult)
                {

                    AddDebugItem(DataListUtil.GetRecordsetIndexType(assignValue.Name) == enRecordsetIndexType.Blank ? new DebugItemWarewolfAtomListResult(recSetResult, evalResult2, "", assignValue.Name, VariableLabelText, NewFieldLabelText, "=") : new DebugItemWarewolfAtomListResult(recSetResult, environment.EvalToExpression(assignValue.Value, update), "", environment.EvalToExpression(assignValue.Name, update), VariableLabelText, NewFieldLabelText, "="), debugItem);
                }
            }
        }

        DebugItem TryCreateDebugInput(IExecutionEnvironment environment, int innerCount, IAssignValue assignValue, int update)
        {
            var debugItem = new DebugItem();
            const string VariableLabelText = "";
            const string NewFieldLabelText = "";

            try
            {
                CreateDebugInput(environment, innerCount, assignValue, update, debugItem, VariableLabelText, NewFieldLabelText);
            }
            catch (Exception e)
            {
                if (e.Message.Contains("ParseError"))
                {
                    AddDebugItem(new DebugItemWarewolfAtomResult("", assignValue.Value, environment.EvalToExpression(assignValue.Name, update), "", VariableLabelText, NewFieldLabelText, "="), debugItem);
                    return debugItem;
                }
                if (!ExecutionEnvironment.IsValidVariableExpression(assignValue.Name, out string errorMessage, update))
                {
                    return null;
                }
                AddErrorDebugItem(environment, innerCount, assignValue, update, debugItem, VariableLabelText, NewFieldLabelText);
            }
            return debugItem;
        }

        void CreateDebugInput(IExecutionEnvironment environment, int innerCount, IAssignValue assignValue, int update, DebugItem debugItem, string VariableLabelText, string NewFieldLabelText)
        {
            if (!DataListUtil.IsEvaluated(assignValue.Value))
            {
                var evalResult = environment.Eval(assignValue.Name, update);
                AddDebugItem(new DebugItemStaticDataParams("", innerCount.ToString(CultureInfo.InvariantCulture)), debugItem);
                if (evalResult.IsWarewolfAtomResult)
                {
                    if (evalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult scalarResult)
                    {
                        AddDebugItem(new DebugItemWarewolfAtomResult(ExecutionEnvironment.WarewolfAtomToString(scalarResult.Item), assignValue.Value, environment.EvalToExpression(assignValue.Name, update), "", VariableLabelText, NewFieldLabelText, "="), debugItem);
                    }
                }
                else
                {
                    AddWarewolfAtomListDebugResult(environment, assignValue, update, debugItem, VariableLabelText, NewFieldLabelText, evalResult);
                }
            }
            else
            {
                if (DataListUtil.IsEvaluated(assignValue.Value))
                {
                    AddEvaluatedDebugItem(environment, innerCount, assignValue, update, NewFieldLabelText, VariableLabelText, debugItem);
                }
            }
        }

        void AddWarewolfAtomListDebugResult(IExecutionEnvironment environment, IAssignValue assignValue, int update, DebugItem debugItem, string VariableLabelText, string NewFieldLabelText, CommonFunctions.WarewolfEvalResult evalResult)
        {
            if (evalResult.IsWarewolfAtomListresult)
            {
                if (DataListUtil.GetRecordsetIndexType(assignValue.Name) == enRecordsetIndexType.Blank)
                {
                    AddDebugItem(new DebugItemWarewolfAtomListResult(null, assignValue.Value, "", environment.EvalToExpression(assignValue.Name, update), VariableLabelText, NewFieldLabelText, "="), debugItem);
                }
                else
                {
                    if (evalResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult recSetResult)
                    {
                        AddDebugItem(new DebugItemWarewolfAtomListResult(recSetResult, assignValue.Value, "", environment.EvalToExpression(assignValue.Name, update), VariableLabelText, NewFieldLabelText, "="), debugItem);
                    }
                }
            }
        }


        private void AddErrorDebugItem(IExecutionEnvironment environment, int innerCount, IAssignValue assignValue, int update, DebugItem debugItem, string VariableLabelText, string NewFieldLabelText)
        {
            AddDebugItem(new DebugItemStaticDataParams("", innerCount.ToString(CultureInfo.InvariantCulture)), debugItem);
            if (DataListUtil.IsEvaluated(assignValue.Value))
            {
                var newValueResult = environment.Eval(assignValue.Value, update);

                if (newValueResult.IsWarewolfAtomResult)
                {
                    if (newValueResult is CommonFunctions.WarewolfEvalResult.WarewolfAtomResult valueResult)
                    {
                        AddDebugItem(new DebugItemWarewolfAtomResult("", ExecutionEnvironment.WarewolfAtomToString(valueResult.Item), environment.EvalToExpression(assignValue.Name, update), assignValue.Value, VariableLabelText, NewFieldLabelText, "="), debugItem);
                    }
                }
                else
                {
                    if (newValueResult.IsWarewolfAtomListresult)
                    {
                        AddDebugItem(new DebugItemWarewolfAtomListResult(null, newValueResult, environment.EvalToExpression(assignValue.Value, update), assignValue.Name, VariableLabelText, NewFieldLabelText, "="), debugItem);
                    }
                }
            }
            else
            {
                AddDebugItem(new DebugItemWarewolfAtomResult("", assignValue.Value, environment.EvalToExpression(assignValue.Name, update), "", VariableLabelText, NewFieldLabelText, "="), debugItem);
            }
        }


        private List<AssignValue> GetAssignValue(List<string> outputVars)
        {
            var data = new List<AssignValue>();
            foreach (var output in outputVars)
            {
                if (!string.IsNullOrWhiteSpace(output))
                {
                    var key = output;
                    var value = ExecutionEnvironment.WarewolfEvalResultToString(DataObject.Environment.Eval(output, 0));
                    data.Add(new AssignValue(key, value));
                }
            }

            return data;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            base._debugOutputs.Clear();

            IDictionary<string, string> cachedData = GetCachedOutputs();
            if (cachedData != null)
            {
                AddKeyToDebugOutputs(env, update);

                var count = 1;
                var assignValues = new List<AssignValue>();
                foreach (var item in cachedData)
                {
                    DataObject.Environment.Assign(item.Key, item.Value, 0);

                    var key = item.Key;
                    var value = item.Value;
                    var debugItem = new DebugItem();
                    if (!string.IsNullOrWhiteSpace(item.Key))
                    {
                        debugItem = TryCreateDebugInput(DataObject.Environment, count++, new AssignValue(key, value), update);
                        _debugOutputs.Add(debugItem);
                    }
                }
            }
            else
            {
                _innerActivity.Execute(DataObject, 0);
                var outputVars = _innerActivity.GetOutputs();

                if (env != null && !string.IsNullOrEmpty(Key))
                {
                    AddKeyToDebugOutputs(env, update);

                    var debugItem = new DebugItem();
                    var count = 1;
                    foreach (var output in GetAssignValue(outputVars))
                    {
                        if (!string.IsNullOrWhiteSpace(output.Name))
                        {
                            debugItem = TryCreateDebugInput(DataObject.Environment, count++, output, update);
                            _debugOutputs.Add(debugItem);
                        }
                    }
                }
            }

            return _debugOutputs?.Any() ?? false ? _debugOutputs : new List<DebugItem>();
        }

        private void AddKeyToDebugOutputs(IExecutionEnvironment env, int update)
        {
            var debugItem = new DebugItem();
            AddDebugItem(new DebugEvalResult(Key, "Key", env, update), debugItem);
            _debugOutputs.Add(debugItem);
        }



#pragma warning disable S1541 // Methods and properties should not be too complex
        public bool Equals(RedisActivity other)
#pragma warning restore S1541 // Methods and properties should not be too complex
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
                && string.Equals(Result, other.Result)
                && TTL == other.TTL
                && SourceId.Equals(other.SourceId)
                && string.Equals(Key, other.Key)
                && string.Equals(DisplayName, other.DisplayName)
                && string.Equals(Response, other.Response);
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

            return Equals((RedisActivity)obj);
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
        public override int GetHashCode()
#pragma warning restore S1541 // Methods and properties should not be too complex
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Result != null ? Result.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DisplayName != null ? DisplayName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ SourceId.GetHashCode();
                hashCode = (hashCode * 397) ^ (Key != null ? Key.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Response != null ? Response.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (TTL != null ? TTL.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Connection != null ? Connection.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}