/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Data.ServiceModel;
using Dev2.Diagnostics;
using Dev2.Runtime.Interfaces;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Driver.Redis;
using Warewolf.Interfaces;
using Warewolf.Resource.Errors;
using Warewolf.Storage.Interfaces;

namespace Dev2.Activities.RedisCounter
{
    [ToolDescriptorInfo(nameof(RedisCounter), "Redis Counter", ToolType.Native, "3446a1fa-ea47-477f-8ee7-6eb72b0d41d9", "Dev2.Activities", "1.0.0.0", "Legacy", "Utility", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Utility_RedisCounter")]
    public class RedisCounterActivity : DsfBaseActivity, IEquatable<RedisCounterActivity>
    {
        long _result;

        private RedisCacheBase _redisCache;
        internal readonly List<string> _messages = new List<string>();

        public RedisCounterActivity()
             : this(Dev2.Runtime.Hosting.ResourceCatalog.Instance, new ResponseManager(), null)
        {

        }

        public RedisCounterActivity(IResourceCatalog resourceCatalog, RedisCacheBase redisCache)
            : this(resourceCatalog, new ResponseManager(), redisCache)
        {
        }

        public RedisCounterActivity(IResourceCatalog resourceCatalog, ResponseManager responseManager, RedisCacheBase redisCache)
        {
            ResponseManager = responseManager;
            ResourceCatalog = resourceCatalog;
            DisplayName = "Redis Counter";
            _redisCache = redisCache;
        }

        public Guid SourceId { get; set; }

        [Inputs("Key")]
        [FindMissing]
        public string Key { get; set; }

        [Inputs("CounterType")]
        [FindMissing]
        public string CounterType { get; set; }

        [Inputs("StepSize")]
        [FindMissing]
        public string StepSize { get; set; }

        [FindMissing]
        public string Response { get; set; }

        internal IRedisConnection Connection { get; set; }

        public override IEnumerable<StateVariable> GetState()
        {
            return new[] {
                new StateVariable
                {
                    Name = nameof(Key),
                    Value = Key,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = nameof(StepSize),
                    Value = StepSize.ToString(),
                    Type = StateVariable.StateType.Input
                },
                 new StateVariable
                {
                    Name = nameof(CounterType),
                    Value = CounterType,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name=nameof(Result),
                    Value = Result,
                    Type = StateVariable.StateType.Output
                }
                ,
                new StateVariable
                {
                    Name=nameof(Response),
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
                _redisCache = new RedisCacheImpl(RedisSource.HostName, Convert.ToInt32(RedisSource.Port), RedisSource.Password);
                if (CounterType == "Increment")
                {
                    _result = _redisCache.Increment(Key, StepSize);
                    
                    Dev2Logger.Debug($"Cache {Key} Incremented {StepSize}: {_result}", GlobalConstants.WarewolfDebug);
                }
                else
                {
                    _result = _redisCache.Decrement(Key, StepSize);
                    Dev2Logger.Debug($"Cache {Key} Decremented {StepSize}: {_result}", GlobalConstants.WarewolfDebug);
                }
                return new List<string> { _result.ToString() };
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(nameof(RedisCounterActivity), ex, GlobalConstants.WarewolfError);
                throw new Exception(ex.GetAllMessages());
            }
        }

        public override List<string> GetOutputs() => new List<string> { Response, Result };

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
        public bool Equals(RedisCounterActivity other)
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
                && SourceId.Equals(other.SourceId)
                && string.Equals(Key, other.Key)
                && string.Equals(StepSize, other.StepSize)
                && string.Equals(CounterType, other.CounterType)
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

            return Equals((RedisCounterActivity)obj);
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
                hashCode = (hashCode * 397) ^ (CounterType != null ? CounterType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ SourceId.GetHashCode();
                hashCode = (hashCode * 397) ^ (Response != null ? Response.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Connection != null ? Connection.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}