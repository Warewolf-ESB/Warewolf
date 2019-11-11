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

namespace Dev2.Activities.Redis
{
    [ToolDescriptorInfo("Redis", "Redis Cache", ToolType.Native, "416eb671-64df-4c82-c6f0-43e48172a799", "Dev2.Activities", "1.0.0.0", "Legacy", "Utility", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Utility_Redis")]
    public class RedisActivity : DsfBaseActivity, IEquatable<RedisActivity>
    {
        public static readonly int DefaultTTE = 10000; // (10 seconds)
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
            TTE = DefaultTTE;
            _redisCache = redisCache;
            ResourceCatalog = resourceCatalog;

            DisplayName = "Redis";
            ActivityFunc = new ActivityFunc<string, bool>
            {
                DisplayName = "Data Action",
                Argument = new DelegateInArgument<string>($"explicitData_{DateTime.Now:yyyyMMddhhmmss}")
            };

            _serializer = new Dev2JsonSerializer();

        }

        public Guid SourceId { get; set; }

        [Inputs("Key")]
        [FindMissing]
        public string Key { get; set; }

        [FindMissing]
        public string Response { get; set; }

        [FindMissing]
        public int TTE { get; set; }

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
                    Name = "TTE",
                    Value = TTE.ToString(),
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
                    _messages.Add(ErrorResource.RedisSourceHasBeenDeleted);
                    return _messages;
                }
                _redisCache = new RedisCacheImpl(RedisSource.HostName);

                var cacheTTE = TimeSpan.FromMilliseconds(TTE);

                var activity = ActivityFunc.Handler as IDev2Activity;
                if (activity is null)
                {
                    throw new InvalidOperationException($"Activity drop box cannot be null");
                }
                if (string.IsNullOrWhiteSpace(activity.GetOutputs()[0]))
                {
                    throw new InvalidOperationException($"{activity.GetDisplayName()} activity must have at least one output variable.");
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
                    activity.Execute(DataObject, 0);
                    var outputVars = activity.GetOutputs();

                    CacheOutputs(cacheTTE, outputVars);
                }
                return new List<string> { _result };
            }
            catch (Exception ex)
            {
                Dev2Logger.Error( nameof(RedisActivity), ex, GlobalConstants.WarewolfError);
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

        private void CacheOutputs(TimeSpan cacheTTE, List<string> outputVars)
        {
            var data = new Dictionary<string, string>();
            foreach (var output in outputVars)
            {
                var key = output;
                var value = ExecutionEnvironment.WarewolfEvalResultToString(DataObject.Environment.Eval(output, 0));

                data.Add(key, value);
            }

            _redisCache.Set(Key, _serializer.Serialize<Dictionary<string, string>>(data), cacheTTE);
        }

        public override List<string> GetOutputs() => new List<string> { Response, Result };

        public override List<DebugItem> GetDebugInputs(IExecutionEnvironment env, int update)
        {
            if (env == null)
            {
                return new List<DebugItem>();
            }
            var debugItem = new DebugItem();
            AddDebugItem(new DebugEvalResult(Key, "Key", env, update), debugItem);
            _debugInputs.Add(debugItem);
            return _debugInputs;
        }

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            base.GetDebugOutputs(env, update);

            if (env != null && !string.IsNullOrEmpty(Response))
            {
                var debugItem = new DebugItem();
                AddDebugItem(new DebugEvalResult(Response, "", env, update), debugItem);
                _debugOutputs.Add(debugItem);
            }

            return _debugOutputs?.Any() ?? false ? _debugOutputs : new List<DebugItem>();
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
                && TTE == other.TTE
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
                hashCode = (hashCode * 397) ^ (TTE != null ? TTE.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Connection != null ? Connection.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}