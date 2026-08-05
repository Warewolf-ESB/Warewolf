/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Warewolf.Security.Encryption;

namespace Warewolf.Execution.QueueProcessor.Configuration
{
    /// <summary>
    /// Reads a Server-written queue-trigger <c>.bite</c> without referencing the assemblies
    /// named in its <c>$type</c> tokens (plan §1.7 "Option A").
    ///
    /// <para>The Server writes these files with <c>Dev2JsonSerializer</c>, i.e.
    /// <c>TypeNameHandling.Objects</c> + <c>PreserveReferencesHandling.Objects</c>
    /// (<c>Dev2JsonSerializer.cs:26-42</c>), so the payload carries <c>$type</c>, <c>$id</c>
    /// and possibly <c>$ref</c> metadata naming <c>Warewolf.Trigger.Queue</c>,
    /// <c>Warewolf.Data</c> and <c>Warewolf.Core</c> types. <see cref="TriggerTypeBinder"/>
    /// maps those names onto the worker's own DTOs, so the staged file remains byte-identical
    /// to what the Server produced while the container's dependency closure stays clean.</para>
    /// </summary>
    public sealed class TriggerBiteReader
    {
        /// <summary>Release-substitution marker, e.g. <c>#{WarewolfProfiler...Concurrency}</c>.</summary>
        const string ReleaseTokenMarker = "#{";

        readonly JsonSerializerSettings _settings = new()
        {
            // Auto (not Objects) so an unexpected root type is a clean bind failure rather
            // than a hard requirement on the exact declared type.
            TypeNameHandling = TypeNameHandling.Auto,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            MetadataPropertyHandling = MetadataPropertyHandling.Default,
            SerializationBinder = new TriggerTypeBinder(),
            MissingMemberHandling = MissingMemberHandling.Ignore,
        };

        /// <summary>
        /// Reads and decrypts a trigger file, failing loudly rather than obscurely on the
        /// three states a container can legitimately meet.
        /// </summary>
        /// <exception cref="TriggerConfigurationException">
        /// The file is DPAPI-encrypted (unusable on Linux), still contains an unsubstituted
        /// release token, or does not deserialize into a trigger.
        /// </exception>
        public TriggerDefinition Read(string path)
        {
            if (!File.Exists(path))
            {
                throw new TriggerConfigurationException(
                    $"Trigger file not found: '{path}'.");
            }

            var raw = File.ReadAllText(path);
            var json = Decrypt(raw, path);

            // Guard BEFORE parsing: an unsubstituted token is not valid JSON, and the parser
            // error ("invalid character '#'") would not tell the operator what to fix.
            if (json.Contains(ReleaseTokenMarker, StringComparison.Ordinal))
            {
                throw new TriggerConfigurationException(
                    $"Trigger file '{path}' still contains an unsubstituted release token " +
                    $"('{ReleaseTokenMarker}...'). The release pipeline must substitute variables such as " +
                    "Concurrency BEFORE the file is staged into the container. " +
                    "Deploy-WwQueueProcessor.ps1 applies the same guard at plan time.");
            }

            TriggerDefinition? trigger;
            try
            {
                trigger = JsonConvert.DeserializeObject<TriggerDefinition>(json, _settings);
            }
            catch (Exception ex)
            {
                throw new TriggerConfigurationException(
                    $"Trigger file '{path}' could not be deserialized: {ex.Message}", ex);
            }

            if (trigger is null)
            {
                throw new TriggerConfigurationException(
                    $"Trigger file '{path}' deserialized to null.");
            }

            Validate(trigger, path);
            return trigger;
        }

        /// <summary>
        /// Enumerates trigger files, honouring the same discovery contract as the deploy
        /// script: an explicit file, or a filtered folder. Zero matches is an error, never a
        /// silent no-op.
        /// </summary>
        public static IReadOnlyList<string> Discover(string path, string filter = "triggers*.bite")
        {
            if (File.Exists(path))
            {
                return new[] { path };
            }

            if (!Directory.Exists(path))
            {
                throw new TriggerConfigurationException(
                    $"Trigger path not found (neither a file nor a folder): '{path}'.");
            }

            var matches = Directory
                .GetFiles(path, filter, SearchOption.TopDirectoryOnly)
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (matches.Length == 0)
            {
                throw new TriggerConfigurationException(
                    $"No trigger files matching '{filter}' were found in '{path}'. " +
                    "Refusing to start with no trigger: check the staged Settings folder and the filter.");
            }

            return matches;
        }

        static string Decrypt(string raw, string path)
        {
            var value = raw.Trim();

            // Key Vault AES (WFAES::) - the hook is wired at startup before any .bite is read.
            if (FileDecryptionHelperIsAes(value))
            {
                try
                {
                    return DpapiWrapper.Decrypt(value);
                }
                catch (Exception ex)
                {
                    throw new TriggerConfigurationException(
                        $"Trigger file '{path}' is WFAES-encrypted but could not be decrypted. " +
                        "Verify the container's Key Vault access and that the key has not been rotated " +
                        "since the file was encrypted.", ex);
                }
            }

            // Plaintext JSON - valid for local development.
            if (value.StartsWith('{'))
            {
                return value;
            }

            // Anything else is almost certainly a Windows DPAPI blob written on-prem. Calling
            // DpapiWrapper.Decrypt here would throw PlatformNotSupportedException from deep
            // inside ProtectedData; say what to do instead.
            throw new TriggerConfigurationException(
                $"Trigger file '{path}' is neither plaintext JSON nor WFAES-encrypted. " +
                "Windows DPAPI blobs cannot be decrypted in a Linux container - re-stage the file " +
                "with Deploy-WwQueueProcessor.ps1, which re-encrypts it as WFAES using the engine's " +
                "Key Vault key.");
        }

        static bool FileDecryptionHelperIsAes(string value)
            => value.StartsWith("WFAES::", StringComparison.Ordinal);

        static void Validate(TriggerDefinition trigger, string path)
        {
            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(trigger.QueueName)) missing.Add(nameof(trigger.QueueName));
            if (string.IsNullOrWhiteSpace(trigger.WorkflowName)) missing.Add(nameof(trigger.WorkflowName));
            if (trigger.QueueSourceId == Guid.Empty) missing.Add(nameof(trigger.QueueSourceId));

            if (missing.Count > 0)
            {
                throw new TriggerConfigurationException(
                    $"Trigger file '{path}' is missing required values: {string.Join(", ", missing)}.");
            }
        }
    }

    /// <summary>
    /// Maps the <c>$type</c> names the Server writes onto the worker's DTOs. Unknown types
    /// fail loudly: silently dropping an option would change queue-declaration arguments and
    /// break <c>QueueDeclare</c> against an existing queue.
    /// </summary>
    internal sealed class TriggerTypeBinder : ISerializationBinder
    {
        public Type BindToType(string? assemblyName, string typeName)
        {
            if (typeName.StartsWith("Warewolf.Trigger.Queue.TriggerQueue", StringComparison.Ordinal))
                return typeof(TriggerDefinition);

            // OptionBool / OptionInt / OptionEnum / OptionAutocomplete - all Name+Value shaped.
            if (typeName.StartsWith("Warewolf.Options.Option", StringComparison.Ordinal))
                return typeof(TriggerOption);

            if (typeName.StartsWith("Warewolf.Core.ServiceInput", StringComparison.Ordinal)
                || typeName.EndsWith("ServiceInput", StringComparison.Ordinal))
                return typeof(TriggerInput);

            // Collections are emitted as concrete generic list types.
            if (typeName.StartsWith("System.Collections.Generic.List", StringComparison.Ordinal))
                return typeof(List<object>);

            throw new TriggerConfigurationException(
                $"Unmapped trigger type '{typeName}' (assembly '{assemblyName}') in the trigger file. " +
                "Add a mapping to TriggerTypeBinder - the worker refuses to silently ignore trigger data.");
        }

        public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
        {
            // The worker never writes trigger files.
            assemblyName = null;
            typeName = null;
        }
    }

    /// <summary>Configuration failure that should stop the worker at startup with an actionable message.</summary>
    public sealed class TriggerConfigurationException : Exception
    {
        public TriggerConfigurationException(string message) : base(message) { }
        public TriggerConfigurationException(string message, Exception inner) : base(message, inner) { }
    }
}
