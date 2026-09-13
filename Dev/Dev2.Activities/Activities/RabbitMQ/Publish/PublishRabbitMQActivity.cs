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

using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Toolbox;
using Dev2.Data.ServiceModel;
using Dev2.Util;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common.Interfaces.Data;
using Dev2.Runtime.Hosting;
using Unlimited.Applications.BusinessDesignStudio.Activities.Utilities;
using Warewolf.Core;
using Warewolf.Resource.Errors;
using Dev2.Common.State;
using Dev2.Interfaces;
using Dev2.Runtime.Interfaces;
using Warewolf.Data.Options;
using Dev2.Common.X6;
using Dev2.WorkflowConverters;

namespace Dev2.Activities.RabbitMQ.Publish
{
    [ToolDescriptorInfo("RabbitMq", "RabbitMQ Publish", ToolType.Native, "FFEC6885-597E-49A2-A1AD-AE81E33DF809",
        "Dev2.Activities", "1.0.0.0", "Legacy", "Utility", "/Warewolf.Studio.Themes.Luna;component/Images.xaml",
        "Tool_Utility_Rabbit_MQ_Publish")]
    public class PublishRabbitMQActivity : DsfBaseActivity, IEquatable<PublishRabbitMQActivity>
    {
        public PublishRabbitMQActivity()
            : this(Dev2.Runtime.Hosting.ResourceCatalog.Instance, new ConnectionFactory())
        {
        }

        public PublishRabbitMQActivity(IResourceCatalog resourceCatalog, ConnectionFactory connectionFactory)
        {
            ResourceCatalog = resourceCatalog;
            ConnectionFactory = connectionFactory;
            DisplayName = "RabbitMQ Publish";
            if (BasicProperties is null)
            {
                BasicProperties = new RabbitMqPublishOptions();
            }
        }

        public Guid RabbitMQSourceResourceId { get; set; }
        public RabbitMqPublishOptions BasicProperties { get; set; }

        [Inputs("Queue Name")] [FindMissing] public string QueueName { get; set; }

        [FindMissing] public bool IsDurable { get; set; }

        [FindMissing] public bool IsExclusive { get; set; }

        [FindMissing] public bool IsAutoDelete { get; set; }

        [Inputs("Message")] [FindMissing] public string Message { get; set; }

        [NonSerialized] ConnectionFactory _connectionFactory;

        private IDSFDataObject _dataObject { get; set; }
        private int _update { get; set; }


        internal ConnectionFactory ConnectionFactory
        {
            get => _connectionFactory ?? (_connectionFactory = new ConnectionFactory());
            set => _connectionFactory = value;
        }

        internal IConnection Connection { get; set; }

        internal IModel Channel { get; set; }

        /// <summary>
        /// Name reported to the broker for cached connections, so an operator can tell a pooled
        /// Warewolf publisher apart from a per-execution one in the RabbitMQ management UI.
        /// </summary>
        static string CachedConnectionClientName => "warewolf-publish/" + Environment.MachineName;

        [NonSerialized] Func<IRabbitMqConnectionCache> _connectionCacheResolver;

        /// <summary>
        /// Resolves the connection cache for this execution. Defaults to the ambient registry, which
        /// is null on the full Warewolf server and whenever caching is disabled.
        ///
        /// <para>Indirected through a resolver rather than read from
        /// <see cref="AmbientRabbitMqConnectionCache.Current"/> inline so unit tests can supply a fake
        /// — or force the uncached path — <b>without mutating process-wide static state</b>. That
        /// matters: <c>Dev2.Activities.Tests</c> runs in parallel in CI, and tests that registered a
        /// real ambient cache would flake against each other.</para>
        /// </summary>
        protected Func<IRabbitMqConnectionCache> ConnectionCacheResolver
        {
            get => _connectionCacheResolver ?? (_connectionCacheResolver = () => AmbientRabbitMqConnectionCache.Current);
            set => _connectionCacheResolver = value;
        }

        public RabbitMQSource RabbitMQSource { get; set; }


        public override IEnumerable<StateVariable> GetState()
        {
            return new[]
            {
                new StateVariable
                {
                    Name = "QueueName",
                    Value = QueueName,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Type = StateVariable.StateType.Input,
                    Name = nameof(BasicProperties),
                    Value = BasicProperties?.ToString(),
                },
                new StateVariable
                {
                    Name = "IsDurable",
                    Value = IsDurable.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "IsExclusive",
                    Value = IsExclusive.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "Message",
                    Value = Message,
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "RabbitMQSourceResourceId",
                    Value = RabbitMQSourceResourceId.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "IsAutoDelete",
                    Value = IsAutoDelete.ToString(),
                    Type = StateVariable.StateType.Input
                },
                new StateVariable
                {
                    Name = "Result",
                    Value = Result,
                    Type = StateVariable.StateType.Output
                }
            };
        }

        protected override void OnExecute(NativeActivityContext context)
        {
            var dataObject = context.GetExtension<IDSFDataObject>();
            ExecuteTool(dataObject, _update);
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
                var CorrelationID = GetCorrelationID();
                RabbitMQSource =
                    ResourceCatalog.GetResource<RabbitMQSource>(GlobalConstants.ServerWorkspaceID,
                        RabbitMQSourceResourceId);

                if (RabbitMQSource == null
                    && AmbientSourceLoader.Current?.EnsureSourceLoaded(RabbitMQSourceResourceId) == true
                    && ResourceCatalog.WorkspaceResources
                           .TryGetValue(GlobalConstants.ServerWorkspaceID, out var ws))
                {
                    lock (ws)
                        RabbitMQSource = ws.OfType<RabbitMQSource>().FirstOrDefault(r => r.ResourceID == RabbitMQSourceResourceId);
                }

                if (RabbitMQSource == null)
                {
                    return new List<string> { ErrorResource.RabbitSourceHasBeenDeleted };
                }

                if (!evaluatedValues.TryGetValue("QueueName", out string queueName) ||
                    !evaluatedValues.TryGetValue("Message", out string message))
                {
                    return new List<string> { ErrorResource.RabbitQueueNameAndMessageRequired };
                }

                ConnectionFactory.HostName = RabbitMQSource.HostName;
                ConnectionFactory.Port = RabbitMQSource.Port;
                ConnectionFactory.UserName = RabbitMQSource.UserName;
                ConnectionFactory.Password = RabbitMQSource.Password;
                ConnectionFactory.VirtualHost = RabbitMQSource.VirtualHost;

                // Cache lookup, not a host check: the resolver returns null on the full Warewolf
                // server (nothing registers an ambient cache there) and whenever
                // WAREWOLF_RABBITMQ_CACHE leaves caching disabled, which is the default. In that
                // case this method performs exactly the open-publish-dispose it always has.
                var connectionCache = ConnectionCacheResolver();
                if (connectionCache is null)
                {
                    using (Connection = ConnectionFactory.CreateConnection())
                    {
                        PublishCore(Connection, queueName, message, CorrelationID, null);
                    }
                }
                else
                {
                    var cacheKey = RabbitMqCacheKey.For(RabbitMQSource.HostName, RabbitMQSource.Port,
                        RabbitMQSource.UserName, RabbitMQSource.VirtualHost, RabbitMQSource.Password);

                    // Only on the cached path: these settings exist because the connection will be
                    // long-lived. Applying them to the uncached path would change behaviour that has
                    // shipped for years, for no benefit to a connection that dies in milliseconds.
                    RabbitMqCachedConnectionSettings.Apply(ConnectionFactory,
                        CachedConnectionClientName);

                    // No using(Connection = ...) here: a cached connection outlives this execution
                    // and is owned by the cache. Only the LEASE is disposed, which returns the
                    // connection rather than closing it.
                    using (var lease = connectionCache.Acquire(cacheKey, () => ConnectionFactory.CreateConnection()))
                    {
                        Connection = lease.Connection;
                        try
                        {
                            PublishCore(lease.Connection, queueName, message, CorrelationID, lease);
                        }
                        catch
                        {
                            // A failed publish may have left the connection half-dead. Poison it so
                            // the next execution reconnects instead of inheriting the failure.
                            lease.Invalidate();
                            throw;
                        }
                    }
                }

                Dev2Logger.Debug($"Message published to queue {queueName} CorrelationId: {CorrelationID} ",
                    GlobalConstants.WarewolfDebug);
                return new List<string> { "Success" };
            }
            catch (Exception ex)
            {
                Dev2Logger.Error("PublishRabbitMQActivity", ex, GlobalConstants.WarewolfError);
                throw new Exception(ex.GetAllMessages());
            }
        }

        /// <summary>
        /// Opens a channel, ensures the topology exists, publishes, and disposes the channel.
        ///
        /// <para>Shared verbatim by the cached and uncached paths so the two cannot drift. With
        /// <paramref name="memo"/> null the sequence of broker calls is identical to what this
        /// activity has always issued — which is what lets the pre-existing tests stand unmodified as
        /// the guarantee that disabling the cache restores today's behaviour exactly.</para>
        ///
        /// <para>The channel is always short-lived, even on a cached connection. <c>IModel</c> is
        /// <b>not</b> thread-safe in RabbitMQ.Client 5.1.2 — concurrent publishes on one channel
        /// corrupt the frame stream — whereas <c>IConnection</c> is, so a channel per publish is what
        /// makes sharing a connection across concurrent executions safe without any locking.</para>
        /// </summary>
        void PublishCore(IConnection connection, string queueName, string message, string correlationID,
            IRabbitMqConnectionLease memo)
        {
            // Null memo (uncached) is always "stale", so the declares below always run.
            var topologyFresh = memo != null && memo.IsTopologyFresh(queueName);

            // Through the LEASE, not connection.CreateModel(), so a cached lease can hand back a
            // pooled channel. With the connection cached, channel.open + channel.close are the only
            // broker round-trips a publish still pays - measured at 99.6% of the remaining cost
            // against a remote broker (323.3 ms of 324.7 ms; the BasicPublish itself is 0.03 ms
            // because it is fire-and-forget). With no memo - the uncached path, and every host that
            // registers no cache - the default implementation is connection.CreateModel(), so the
            // sequence of broker calls is byte-for-byte what it has always been.
            //
            // The channel is still owned exclusively by this publish until it is returned below:
            // IModel is not thread-safe in RabbitMQ.Client 5.1.2, IConnection is.
            Channel = memo != null ? memo.RentChannel() : connection.CreateModel();
            try
            {
                if (!topologyFresh)
                {
                    EnsureTopology(connection, queueName);
                }

                var basicProperties = Channel.CreateBasicProperties();
                basicProperties.Persistent = true;
                basicProperties.CorrelationId = correlationID;
                Channel.BasicPublish(queueName, "", basicProperties, Encoding.UTF8.GetBytes(message));

                // Only after a publish actually succeeded: marking earlier would let a run of
                // failures keep refreshing the memo and skipping the declares that would have
                // re-created the missing topology.
                memo?.MarkTopologyDeclared(queueName);
            }
            finally
            {
                // Returned, not disposed, when a lease is in play: a pooling lease keeps the channel
                // for the next publish, and a non-pooling one disposes it - which is what this line
                // did unconditionally before.
                if (memo != null)
                {
                    memo.ReturnChannel(Channel);
                }
                else
                {
                    Channel?.Dispose();
                }
            }
        }

        /// <summary>
        /// Verifies (and creates, if absent) the exchange, queue and binding for
        /// <paramref name="queueName"/>. Unchanged from the long-standing implementation.
        /// </summary>
        void EnsureTopology(IConnection connection, string queueName)
        {
            bool newExchangeOrQueue = false;
            try
            {
                // Check if the exchange exists
                Channel.ExchangeDeclarePassive(queueName);
            }
            catch (OperationInterruptedException)
            {
                // A failed passive declare closes the channel server-side (the
                // broker's 404 NOT_FOUND is a channel-level AMQP exception), so
                // the active declare must run on a brand-new channel - retrying
                // on the one the broker just closed throws AlreadyClosedException
                // instead of creating the exchange (see also
                // RabbitMqDeadLetterPublisher.EnsureChannelAsync, which documents
                // and avoids this same pitfall).
                Channel.Dispose();
                Channel = connection.CreateModel();
                // The exchange does not exist, so declare it
                Channel.ExchangeDeclare(queueName, ExchangeType.Direct, IsDurable, IsAutoDelete, null);
                newExchangeOrQueue = true;
            }
            try
            {
                // Check if the queue exists
                Channel.QueueDeclarePassive(queueName);
            }
            catch (OperationInterruptedException)
            {
                // Same reasoning as above: the queue's passive-declare 404 also
                // closes this channel, so its active declare needs a fresh one too.
                Channel.Dispose();
                Channel = connection.CreateModel();
                // The queue does not exist, so declare it
                Channel.QueueDeclare(queueName, IsDurable, IsExclusive, IsAutoDelete, null);
                newExchangeOrQueue = true;
            }
            if (newExchangeOrQueue)
            {
                Channel.QueueBind(queueName, queueName, "", new Dictionary<string, object>());
            }
        }

        private string GetCorrelationID()
        {
            if (BasicProperties.AutoCorrelation is Manual properties)
            {
                var expr = _dataObject.Environment.EvalToExpression(properties.CorrelationID, _update);
                return Warewolf.Storage.ExecutionEnvironment.WarewolfEvalResultToString(_dataObject.Environment.Eval(expr, _update, false, true));
            }
            else
            {
                if (BasicProperties.AutoCorrelation is CustomTransactionID || !string.IsNullOrEmpty(_dataObject.CustomTransactionID))
                {
                    return _dataObject.CustomTransactionID;
                }

                if (BasicProperties.AutoCorrelation is ExecutionID)
                {
                    return _dataObject.ExecutionID.ToString();
                }

                return _dataObject.ExecutionID.ToString();
            }
        }

#pragma warning disable S1541 // Methods and properties should not be too complex
        public bool Equals(PublishRabbitMQActivity other)
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

            var isSourceEqual = CommonEqualityOps.AreObjectsEqual<IResource>(RabbitMQSource, other.RabbitMQSource);
            return base.Equals(other)
                   && RabbitMQSourceResourceId.Equals(other.RabbitMQSourceResourceId)
                   && string.Equals(QueueName, other.QueueName)
                   && IsDurable == other.IsDurable
                   && IsExclusive == other.IsExclusive
                   && IsAutoDelete == other.IsAutoDelete
                   && string.Equals(Message, other.Message)
                   && string.Equals(DisplayName, other.DisplayName)
                   && isSourceEqual;
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

            return Equals((PublishRabbitMQActivity)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ RabbitMQSourceResourceId.GetHashCode();
                hashCode = (hashCode * 397) ^ (QueueName != null ? QueueName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (DisplayName != null ? DisplayName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsDurable.GetHashCode();
                hashCode = (hashCode * 397) ^ IsExclusive.GetHashCode();
                hashCode = (hashCode * 397) ^ IsAutoDelete.GetHashCode();
                hashCode = (hashCode * 397) ^ (Message != null ? Message.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Connection != null ? Connection.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Channel != null ? Channel.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (RabbitMQSource != null ? RabbitMQSource.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override void ToX6Json(Cell cell)
        {
            if (cell.data == null) cell.data = new System.Collections.Generic.Dictionary<string, object>();

            base.ToX6Json(cell);

            cell.shape = Constants.RABBITMQPUBLISHACTIVITY;
            cell.data[Constants.TYPE] = Constants.RABBITMQPUBLISHACTIVITY.ToLower();
            cell.data[Constants.DISPLAYNAME] = DisplayName ?? Constants.DISPLAYNAME_RABBITMQPUBLISH;
            cell.data[Constants.UNIQUEID] = UniqueID;

            cell.data.TryAdd(Constants.RABBITMQPUBLISH_SOURCEID, RabbitMQSourceResourceId);
            cell.data.TryAdd(Constants.RABBITMQPUBLISH_QUEUENAME, QueueName);
            cell.data.TryAdd(Constants.RABBITMQPUBLISH_SETTINGS_DURABLE, IsDurable);
            cell.data.TryAdd(Constants.RABBITMQPUBLISH_SETTINGS_EXCLUSIVE, IsExclusive);
            cell.data.TryAdd(Constants.RABBITMQPUBLISH_SETTINGS_AUTODELETE, IsAutoDelete);
            cell.data.TryAdd(Constants.RABBITMQPUBLISH_MESSAGE, Message);
            cell.data.TryAdd(Constants.RABBITMQPUBLISH_BASICPROPERTIES, BasicProperties);
            cell.data.TryAdd(Constants.RESULT, Result);
        }

        public override void FromX6Json(Cell cell)
        {
            if (cell == null || cell.data == null) return;

            base.FromX6Json(cell);

            if (cell.data.TryGetString(Constants.DISPLAYNAME, out var displayName)) DisplayName = displayName;
            if (cell.data.TryGetString(Constants.UNIQUEID, out var uniqueId)) UniqueID = uniqueId;

            if (cell.data.TryGetGuid(Constants.RABBITMQPUBLISH_SOURCEID, out var resourceid)) RabbitMQSourceResourceId = resourceid;
            if (cell.data.TryGetString(Constants.RABBITMQPUBLISH_QUEUENAME, out var queuename)) QueueName = queuename;
            if (cell.data.TryGetBool(Constants.RABBITMQPUBLISH_SETTINGS_DURABLE, out var isdurable)) IsDurable = isdurable;
            if (cell.data.TryGetBool(Constants.RABBITMQPUBLISH_SETTINGS_EXCLUSIVE, out var isexclusive)) IsExclusive = isexclusive;
            if (cell.data.TryGetBool(Constants.RABBITMQPUBLISH_SETTINGS_AUTODELETE, out var isautodelete)) IsAutoDelete = isautodelete;
            if (cell.data.TryGetString(Constants.RABBITMQPUBLISH_MESSAGE, out var message)) Message = message;
            if (cell.data.TryGetRabbitMqPublishOptions(out var basicproperties)) BasicProperties = basicproperties;
            if (cell.data.TryGetString(Constants.RESULT, out var result)) Result = result;

        }
    }
}