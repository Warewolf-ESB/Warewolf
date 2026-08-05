/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

namespace Warewolf.Execution.QueueProcessor.Configuration
{
    /// <summary>
    /// Worker-local DTO for a queue trigger, deserialized from the Server-written
    /// trigger <c>.bite</c> by <see cref="TriggerBiteReader"/>.
    ///
    /// <para><b>Why a DTO and not <c>Warewolf.Trigger.Queue.TriggerQueue</c>.</b> That type
    /// lives in a project whose closure includes <c>Dev2.Data</c> (and therefore
    /// <c>Warewolf.Driver.RabbitMQ</c> → <c>RabbitMQ.Client</c> 5.1.2), plus
    /// <c>Dev2.Infrastructure</c>, <c>Dev2.Core</c> and <c>Warewolf.UI</c>. Referencing it
    /// would re-pin the RabbitMQ client version this worker deliberately moves off
    /// (plan §1.6). The <c>$type</c> names in the file are mapped onto these DTOs by a
    /// serialization binder instead, so the staged file stays byte-identical to what the
    /// Server writes.</para>
    ///
    /// Only the fields the worker actually consumes are modelled; everything else in the
    /// file is ignored.
    /// </summary>
    public sealed class TriggerDefinition
    {
        public Guid TriggerId { get; set; }
        public string? Name { get; set; }

        public Guid QueueSourceId { get; set; }
        public string? QueueName { get; set; }
        public string? WorkflowName { get; set; }

        /// <summary>Maps to <c>maxReplicas</c> at deploy time; 0 means the trigger is disabled.</summary>
        public int Concurrency { get; set; }

        /// <summary>Per-consumer <c>BasicQos</c> prefetch. Empty or &lt; 1 is treated as 1.</summary>
        public string? Prefetch { get; set; }

        /// <summary>Dead-letter broker source; frequently the same GUID as <see cref="QueueSourceId"/>.</summary>
        public Guid QueueSinkId { get; set; }
        public string? DeadLetterQueue { get; set; }

        public bool MapEntireMessage { get; set; }

        public List<TriggerOption>? Options { get; set; }
        public List<TriggerOption>? DeadLetterOptions { get; set; }
        public List<TriggerInput>? Inputs { get; set; }

        /// <summary>The workflow resource id — telemetry/audit correlation only.</summary>
        public Guid ResourceId { get; set; }

        /// <summary>
        /// The per-trigger basic-auth identity is intentionally NOT modelled: the worker
        /// authenticates to the engine with its managed identity (decision #4), and reading
        /// credentials we never use would only put them in memory.
        /// </summary>
        public ushort ResolvedPrefetch =>
            ushort.TryParse(Prefetch, out var p) && p >= 1 ? p : (ushort)1;

        /// <summary>Reads a named boolean option (e.g. <c>Durable</c>), defaulting to false.</summary>
        public bool OptionBool(string name, bool @default = false)
            => FindOption(Options, name)?.Value as bool? ?? @default;

        public bool DeadLetterOptionBool(string name, bool @default = false)
            => FindOption(DeadLetterOptions, name)?.Value as bool? ?? @default;

        static TriggerOption? FindOption(List<TriggerOption>? options, string name)
            => options?.FirstOrDefault(o =>
                string.Equals(o.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// A trigger option. The Server serializes concrete option types
    /// (<c>OptionBool</c>, <c>OptionInt</c>, <c>OptionEnum</c>, <c>OptionAutocomplete</c>);
    /// they all carry a <c>Name</c> and a <c>Value</c>, which is all the worker needs.
    /// </summary>
    public sealed class TriggerOption
    {
        public string? Name { get; set; }
        public object? Value { get; set; }
        public object? Default { get; set; }
    }

    /// <summary>One workflow input mapping (<c>Warewolf.Core.ServiceInput</c>).</summary>
    public sealed class TriggerInput
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
        public bool RequiredField { get; set; }
        public bool EmptyIsNull { get; set; }
        public bool IsObject { get; set; }
    }
}
