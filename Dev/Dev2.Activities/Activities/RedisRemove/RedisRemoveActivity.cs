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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Common.State;
using Dev2.Data.ServiceModel;
using Dev2.Diagnostics;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Dev2.Util;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Driver.Redis;
using Warewolf.Resource.Errors;
using Warewolf.Storage;
using Warewolf.Storage.Interfaces;

namespace Dev2.Activities.RedisRemove
{
    [ToolDescriptorInfo(nameof(RedisRemove), "Redis Remove", ToolType.Native, "47671136-49d2-4cca-b0d3-cb25ad424ddd", "Dev2.Activities", "1.0.0.0", "Legacy", "Database", "/Warewolf.Studio.Themes.Luna;component/Images.xaml", "Tool_Database_RedisRemove")]
    public class RedisRemoveActivity : DsfBaseActivity, IEquatable<RedisRemoveActivity>
    {
        string _result = "Success";

        private RedisCacheBase _redisCache;
        internal readonly List<string> _messages = new List<string>();
        private string _key;
        private IDSFDataObject _dataObject;
        private int _update;

        [ExcludeFromCodeCoverage]
        public RedisRemoveActivity()
             : this(Runtime.Hosting.ResourceCatalog.Instance, new ResponseManager(), null)
        {

        }

        public RedisRemoveActivity(IResourceCatalog resourceCatalog, RedisCacheBase redisCache)
            : this(resourceCatalog, new ResponseManager(), redisCache)
        {
        }

        public RedisRemoveActivity(IResourceCatalog resourceCatalog, ResponseManager responseManager, RedisCacheBase redisCache)
        {
            ResponseManager = responseManager;
            ResourceCatalog = resourceCatalog;
            DisplayName = "Redis Remove";
            _redisCache = redisCache;
        }

        public Guid SourceId { get; set; }

        [Inputs("Key")]
        [FindMissing]
        public string Key
        {
            get => _key;
            set 
            {
                if (_key != value)
                {
                    _key = value;
                }
            }
        }

        [FindMissing]
        public string Response { get; set; }
        
        //Note: this is nolonger in use, 
        //TODO: make a new tool to deprecate this one, after removing this variable.
        [ExcludeFromCodeCoverage]
        public RedisSource RedisSource { get; set; }

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

        protected override void ExecuteTool(IDSFDataObject dataObject, int update)
        {
            _dataObject = dataObject;
            _update = update;
            base.ExecuteTool(dataObject, update);
        }

        protected override List<string> PerformExecution(Dictionary<string, string> evaluatedValues)
        {
            try
            {
                var redisSource = ResourceCatalog.GetResource<RedisSource>(GlobalConstants.ServerWorkspaceID, SourceId);
                if (redisSource == null || redisSource.ResourceType != enSourceType.RedisSource.ToString())
                {
                    _messages.Add(ErrorResource.RedisSourceHasBeenRemoved);
                    return _messages;
                }

                _redisCache = _redisCache ?? new RedisCacheImpl(()=> new RedisConnection(redisSource.HostName, Convert.ToInt32(redisSource.Port), redisSource.Password));
                var keyValue = GetKeyValue();
                if (!_redisCache.Remove(keyValue))
                {
                    _result = "Failure";
                }
                Dev2Logger.Debug($"Cache {keyValue} removed: {_result}", GlobalConstants.WarewolfDebug);
                Response = _result;
                return new List<string> { _result };
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(nameof(RedisRemoveActivity), ex, GlobalConstants.WarewolfError);
                throw new Exception(ex.GetAllMessages());
            }
        }
        
        private string GetKeyValue()
        {
            var expr = _dataObject.Environment.EvalToExpression(_key, _update);
            return ExecutionEnvironment.WarewolfEvalResultToString(_dataObject.Environment.Eval(expr, _update, false, true));
        }

        public override IEnumerable<string> GetOutputs() => new List<string> { Response, Result };

        public override List<DebugItem> GetDebugOutputs(IExecutionEnvironment env, int update)
        {
            base.GetDebugOutputs(env, update);

            return _debugOutputs;
        }

        public bool Equals(RedisRemoveActivity other)
        {
            if (other is null)
            {
                return false;
            }

            return ReferenceEquals(this, other)
                ? true
                : base.Equals(other)
                && string.Equals(Result, other.Result)
                && SourceId.Equals(other.SourceId)
                && string.Equals(Key, other.Key)
                && string.Equals(DisplayName, other.DisplayName)
                && string.Equals(Response, other.Response);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() != GetType() ? false : Equals((RedisRemoveActivity)obj);
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
                return hashCode;
            }
        }
    }
}