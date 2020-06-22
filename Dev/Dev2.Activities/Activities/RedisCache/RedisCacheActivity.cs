/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces.Core.DynamicServices;
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
using WarewolfParserInterop;
using System.Globalization;
using Dev2.Data.Interfaces.Enums;
using Warewolf.Data;

namespace Dev2.Activities.RedisCache
{
    [ToolDescriptorInfo(nameof(RedisCache), "Redis Cache", ToolType.Native, "416eb671-64df-4c82-c6f0-43e48172a799", "Dev2.Activities", "1.0.0.0", "Legacy", "Database", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Database_RedisCache")]
    public class RedisCacheActivity : DsfBaseActivity, IEquatable<RedisCacheActivity>
    {
        string _result = "Success";
        private readonly ISerializer _serializer;
        private string _key;
        private IDSFDataObject _dataObject;
        private RedisCacheBase _redisCache;
        internal readonly List<string> _messages = new List<string>();

        public RedisCacheActivity()
            : this(Runtime.Hosting.ResourceCatalog.Instance, new ResponseManager(), null)
        {
        }

        public RedisCacheActivity(IResourceCatalog resourceCatalog, RedisCacheBase redisCache)
            : this(resourceCatalog, new ResponseManager(), redisCache)
        {
        }

        public RedisCacheActivity(IResourceCatalog resourceCatalog, ResponseManager responseManager, RedisCacheBase redisCache)
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

        private IDev2Activity _innerActivity;
        private int _update;

        [Inputs("Key")]
        [FindMissing]
        public string Key
        {
            get => _key;
            set => _key = value;
        }

        private string KeyValue
        {
            get
            {
                var expr = _dataObject.Environment.EvalToExpression(_key, _update);
                var varValue = ExecutionEnvironment.WarewolfEvalResultToString(_dataObject.Environment.Eval(expr, _update, false, true));
                return varValue == _key ? _key : varValue;
            }
        }

        [FindMissing] public string Response { get; set; }

        [FindMissing] public int TTL { get; set; } = 5;

        private IRedisConnection Connection { get; set; }

        public static bool ShouldSerializeConnection() => false;

        public ActivityFunc<string, bool> ActivityFunc { get; set; }

        public override IEnumerable<StateVariable> GetState()
        {
            return new[]
            {
                new StateVariable
                {
                    Name = nameof(Key),
                    Value = Key,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = nameof(TTL),
                    Value = TTL.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = nameof(Result),
                    Value = Result,
                    Type = StateVariable.StateType.Output
                },
                new StateVariable
                {
                    Name = nameof(Response),
                    Value = Response,
                    Type = StateVariable.StateType.Output
                },
                new StateVariable
                {
                    Name = nameof(SourceId),
                    Value = SourceId.ToString(),
                    Type = StateVariable.StateType.Input
                }
            };
        }

        public RedisSource RedisSource { get; set; }

        private TimeSpan CacheTTL => TimeSpan.FromSeconds(TTL);


        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            _dataObject = dataObject;
            _update = update;
            base.ExecuteTool(_dataObject, update);
        }

        protected override List<string> PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            _errorsTo = new ErrorResultTO();

            try
            {
                RedisSource = ResourceCatalog.GetResource<RedisSource>(GlobalConstants.ServerWorkspaceID, SourceId);
                if (RedisSource == null || RedisSource.ResourceType != enSourceType.RedisSource.ToString())
                {
                    _messages.Add(ErrorResource.RedisSourceHasBeenRemoved);
                    return _messages;
                }

                _redisCache = new RedisCacheImpl(RedisSource.HostName, Convert.ToInt32(RedisSource.Port), RedisSource.Password);
                _innerActivity = ActivityFunc.Handler as IDev2Activity;
                if (_innerActivity is null)
                {
                    _errorsTo.AddError($"Activity drop box cannot be null");
                }

                if (_innerActivity.GetOutputs().Count() <= 0)
                {
                    _errorsTo.AddError($"{_innerActivity.GetDisplayName()} activity must have at least one output variable.");
                }

                var keyValue = KeyValue;
                var cachedData = GetCachedOutputs(keyValue);
                if (cachedData != null)
                {
                    _debugOutputs.Clear();

                    var debugItem = new DebugItem();
                    AddDebugItem(new DebugItemStaticDataParams("", "Redis key { " + keyValue + " } found"), debugItem);
                    _debugOutputs.Add(debugItem);

                    LoadCacheIntoEnvironment(cachedData);
                }
                else
                {
                    _debugOutputs.Clear();
                    var debugItem = new DebugItem();

                    AddDebugItem(new DebugItemStaticDataParams("", "Redis key { " + keyValue + " } not found"), debugItem);
                    _debugInputs.Add(debugItem);

                    _innerActivity.Execute(_dataObject, _update);

                    CacheOutputs(keyValue);
                }

                return new List<string> {_result};
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(nameof(RedisCacheActivity), ex, GlobalConstants.WarewolfError);
                throw new Exception(ex.GetAllMessages());
            }
            finally
            {
                if (_errorsTo.HasErrors())
                {
                    var errorString = _errorsTo.MakeDisplayReady();
                    _dataObject.Environment.AddError(errorString);
                }
            }
        }

        private void LoadCacheIntoEnvironment(IDictionary<string, string> cachedData)
        {
            var outputs = _innerActivity.GetOutputs();
            foreach (var outputVar in outputs)
            {
                foreach (var item in cachedData)
                {
                    var recordsetName = DataListUtil.ExtractRecordsetNameFromValue(outputVar);
                    var cacheRecordsetName = DataListUtil.ExtractRecordsetNameFromValue(item.Key);
                    if (cacheRecordsetName == recordsetName)
                    {
                        _dataObject.Environment.Assign(item.Key, item.Value, _update);
                        var debugItem = new DebugItem();
                        AddDebugItem(new DebugItemWarewolfAtomResult("", item.Value, item.Key, "", "", "", "="), debugItem);
                    }
                }
            }
        }

        private IDictionary<string, string> GetCachedOutputs(string keyValue)
        {
            var cachedData = _redisCache.Get(keyValue);
            if (cachedData is null)
            {
                return null;
            }

            var outputs = _serializer.Deserialize<IDictionary<string, string>>(cachedData);
            return outputs;
        }

        private void CacheOutputs(string keyValue)
        {
            var data = new Dictionary<string, string>();
            var debugItem = new DebugItem();
            foreach (var output in _innerActivity.GetOutputs())
            {
                var key = output;
                if (DataListUtil.IsValueRecordset(key) && DataListUtil.GetRecordsetIndexType(key) == enRecordsetIndexType.Blank)
                {
                    key = DataListUtil.ReplaceRecordBlankWithStar(key);
                }

                var result = _dataObject.Environment.Eval(key, _update);
                if (result.IsWarewolfAtomListresult)
                {
                    var x = (result as CommonFunctions.WarewolfEvalResult.WarewolfAtomListresult)?.Item;
                    var atomListResult = x?.ToList();
                    var counter = 1;
                    foreach (var item in atomListResult)
                    {
                        var idxKey = key;
                        if (DataListUtil.GetRecordsetIndexType(idxKey) == enRecordsetIndexType.Star)
                        {
                            idxKey = idxKey.Replace(GlobalConstants.StarExpression, counter.ToString(CultureInfo.InvariantCulture));
                        }

                        data.Add(idxKey, item.ToString());
                        AddEvaluatedDebugItem(_dataObject.Environment, counter, new AssignValue(idxKey, item.ToString()), _update, "", "", debugItem);
                        counter++;
                    }
                }

                if (result.IsWarewolfAtomResult)
                {
                    var value = ExecutionEnvironment.WarewolfEvalResultToString(result);
                    data.Add(key, value);
                    AddEvaluatedDebugItem(_dataObject.Environment, 1, new AssignValue(key, value), _update, "", "", debugItem);
                    _debugInputs.Add(debugItem);
                }
            }

            _redisCache.Set(keyValue, _serializer.Serialize(data), CacheTTL);
        }


        public override List<string> GetOutputs() => new List<string>
        {
            Response, Result
        };


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

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            base.GetDebugOutputs(env, update);
            return _debugOutputs?.Any() ?? false ? _debugOutputs : new List<DebugItem>();
        }

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

            base.GetDebugInputs(env, update);
            return _debugInputs?.Any() ?? false ? _debugInputs : new List<DebugItem>();
        }
#pragma warning disable S1541 // Methods and properties should not be too complex
        public bool Equals(RedisCacheActivity other)
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

            return Equals((RedisCacheActivity) obj);
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

        public override enFindMissingType GetFindMissingType() => enFindMissingType.RedisCache;
    }
}