/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Execution.QueueProcessor.Configuration;

namespace Warewolf.Execution.QueueProcessor.Tests
{
    /// <summary>
    /// Cold-start configuration: reading the Server-written trigger <c>.bite</c> and the
    /// RabbitMQ source <c>.bite</c> without referencing the assemblies named in their
    /// <c>$type</c> tokens, and failing loudly on the states a container can legitimately meet.
    ///
    /// Fixtures are the REAL artefacts (a production trigger definition and its RabbitMQ
    /// source), so these tests pin the actual on-disk contract rather than an idealised one.
    /// </summary>
    [TestClass]
    public class ConfigurationTests
    {
        /// <summary>
        /// Staged layout, matching what <c>Deploy-WwQueueProcessor.ps1</c> bakes into the image:
        /// TRIGGER definitions in <c>Settings\triggers\</c> and every referenced SOURCE in
        /// <c>Settings\sources\</c>. The two are separate folders so a source file can never be
        /// mistaken for a trigger during discovery, or the reverse.
        /// </summary>
        static string SettingsDir => Path.Combine(AppContext.BaseDirectory, "TestResources");
        static string TriggersDir => Path.Combine(SettingsDir, "triggers");
        static string SourcesDir => Path.Combine(SettingsDir, "sources");

        static string ResourcePath(string name) => Path.Combine(SourcesDir, name);
        static string TriggerPath(string name) => Path.Combine(TriggersDir, name);

        const string RealTrigger = "triggers-mandate.bite";
        const string TokenTrigger = "triggers-mandate-token.bite";
        const string MixedDurabilityTrigger = "triggers-mixed-durability.bite";
        const string AtInputTrigger = "triggers-at-input.bite";
        const string RealSource = "0b142714-8f6d-41b7-9832-2aefa8c731ec.bite";
        const string TlsSource = "tls-source.bite";
        const string NamedSource = "Warewolf DevOps RabbitMQ Source.bite";

        /// <summary>Options wired at the staged tree, with the engine fields validation needs.</summary>
        static QueueProcessorOptions StagedOptions(string triggerFilter) => new()
        {
            SettingsPath = SettingsDir,
            TriggerFilter = triggerFilter,
            BaseUrl = "https://engine",
            ResourceAppId = "app",
        };

        static QueueConfigurationLoader LoaderFor(QueueProcessorOptions options) =>
            new(new OptionsWrapper<QueueProcessorOptions>(options), new TriggerBiteReader());

        // ── Unsupported input shape: '@'-prefixed MapEntireMessage ───────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Load_AtPrefixedMapEntireMessage_RefusesToStart()
        {
            // An '@'-prefixed single input with MapEntireMessage makes the forwarder post
            // multipart/form-data (parity with WarewolfWebRequestForwarder.cs:100-108). The full
            // server binds that - SubmittedData.ExtractKeyValuePairForPostMethod branches on
            // IsMimeMultipartContent("form-data") - but the Lightweight engine binds only the query
            // string, a JSON body and an XML body. Verified live against the deployed engine:
            // multipart/form-data returned 500 "Scalar value { x } is NULL".
            //
            // Left unguarded the failure mode is the worst kind: the replica starts, consumes every
            // message, gets a non-2xx for each, and dead-letters + acks them all. The queue drains,
            // the app scales back to zero, and nothing looks wrong. Refusing to start turns silent
            // data movement into a deployment-time error.
            var ex = Assert.ThrowsException<InvalidOperationException>(
                () => LoaderFor(StagedOptions(AtInputTrigger)).Load());

            StringAssert.Contains(ex.Message, "multipart/form-data");
            StringAssert.Contains(ex.Message, "@object");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Load_MapEntireMessageWithPlainInputName_IsAccepted()
        {
            // The shape both production triggers use: MapEntireMessage with a plain input name, sent
            // as a JSON body. Guards against the check above over-reaching.
            var resolved = LoaderFor(StagedOptions(RealTrigger)).Load();

            Assert.IsTrue(resolved.Trigger.MapEntireMessage);
            Assert.AreEqual("PayloadRequest", resolved.Trigger.Inputs[0].Name);
        }

        // ── TriggerBiteReader ────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TriggerBiteReader_RealFile_ParsesTypeAndIdMetadataOntoLocalDtos()
        {
            var trigger = new TriggerBiteReader().Read(TriggerPath(RealTrigger));

            Assert.AreEqual(Guid.Parse("1ac40da8-3b56-45f8-a1aa-00e6864db38b"), trigger.TriggerId);
            Assert.AreEqual("MandateCollectionSuccessTrigger", trigger.Name);
            Assert.AreEqual("profiler.mandatecollectionsuccess.request", trigger.QueueName);
            Assert.AreEqual(Guid.Parse("0b142714-8f6d-41b7-9832-2aefa8c731ec"), trigger.QueueSourceId);
            Assert.AreEqual(5, trigger.Concurrency);
            Assert.IsTrue(trigger.MapEntireMessage);
            Assert.AreEqual("profiler.mandatecollectionsuccess.error.request", trigger.DeadLetterQueue);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TriggerBiteReader_RealFile_ResolvesPrefetchAndDurableOption()
        {
            var trigger = new TriggerBiteReader().Read(TriggerPath(RealTrigger));

            // Prefetch is a STRING in the trigger contract; 10 must survive as a ushort.
            Assert.AreEqual((ushort)10, trigger.ResolvedPrefetch);

            // Only Durable is declared, so Exclusive/AutoDelete must stay false - a mismatch
            // here would fail QueueDeclare against the existing queue.
            Assert.IsTrue(trigger.OptionBool("Durable"));
            Assert.IsFalse(trigger.OptionBool("Exclusive"));
            Assert.IsFalse(trigger.OptionBool("AutoDelete"));
            Assert.IsTrue(trigger.DeadLetterOptionBool("Durable"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TriggerBiteReader_RealFile_MapsTheSingleInput()
        {
            var trigger = new TriggerBiteReader().Read(TriggerPath(RealTrigger));

            Assert.IsNotNull(trigger.Inputs);
            Assert.AreEqual(1, trigger.Inputs!.Count);
            Assert.AreEqual("PayloadRequest", trigger.Inputs[0].Name);
            Assert.IsTrue(trigger.Inputs[0].RequiredField);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TriggerBiteReader_UnsubstitutedReleaseToken_ThrowsActionableError()
        {
            var ex = Assert.ThrowsException<TriggerConfigurationException>(
                () => new TriggerBiteReader().Read(TriggerPath(TokenTrigger)));

            // The message must name the cause and the fix, not surface a JSON parser error
            // about an unexpected '#'.
            StringAssert.Contains(ex.Message, "unsubstituted release token");
            StringAssert.Contains(ex.Message, "Concurrency");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TriggerBiteReader_DpapiShapedPayload_ThrowsPlatformActionableError()
        {
            // A Windows DPAPI blob is base64 - decrypting it in a Linux container throws
            // PlatformNotSupportedException from deep inside ProtectedData, so the reader must
            // intercept it first and say what to do.
            var path = Path.Combine(Path.GetTempPath(), $"dpapi-{Guid.NewGuid()}.bite");
            File.WriteAllText(path, Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }));

            try
            {
                var ex = Assert.ThrowsException<TriggerConfigurationException>(
                    () => new TriggerBiteReader().Read(path));

                StringAssert.Contains(ex.Message, "DPAPI");
                StringAssert.Contains(ex.Message, "Deploy-WwQueueProcessor.ps1");
            }
            finally
            {
                File.Delete(path);
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void TriggerBiteReader_MissingFile_Throws()
        {
            Assert.ThrowsException<TriggerConfigurationException>(
                () => new TriggerBiteReader().Read(ResourcePath("does-not-exist.bite")));
        }

        // ── Discovery ────────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Discover_FolderWithMatches_ReturnsThemSorted()
        {
            var folder = TriggersDir;

            var found = TriggerBiteReader.Discover(folder, "*.bite");

            // Every staged trigger fixture matches; asserted against the folder rather than a
            // hard-coded count so adding a fixture does not fail an unrelated test.
            var expected = Directory.GetFiles(folder, "*.bite").Length;
            Assert.IsTrue(expected >= 3, "the fixture set must cover real, token and mixed-durability");
            Assert.AreEqual(expected, found.Count);
            CollectionAssert.AllItemsAreUnique(found.ToList());
            CollectionAssert.AreEqual(found.OrderBy(f => f, StringComparer.Ordinal).ToList(),
                                      found.ToList(), "Discover must return a stable sorted order");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Discover_NoMatches_ThrowsRatherThanReturningEmpty()
        {
            var folder = TriggersDir;

            // Silently consuming nothing is the worst outcome: the app would look healthy while
            // the queue backed up.
            var ex = Assert.ThrowsException<TriggerConfigurationException>(
                () => TriggerBiteReader.Discover(folder, "nothing-matches*.bite"));

            StringAssert.Contains(ex.Message, "Refusing to start with no trigger");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Discover_ExplicitFile_ReturnsThatFileOnly()
        {
            var found = TriggerBiteReader.Discover(TriggerPath(RealTrigger));

            Assert.AreEqual(1, found.Count);
            Assert.AreEqual(TriggerPath(RealTrigger), found[0]);
        }

        // ── RabbitMqSourceOptions ────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SourceOptions_RealFile_ParsesTheFiveActivityConnectionFields()
        {
            var source = RabbitMqSourceOptions.FromBiteFile(ResourcePath(RealSource));

            // Exactly the fields PublishRabbitMQActivity puts on its ConnectionFactory.
            Assert.AreEqual("server.ngrok.io", source.HostName);
            Assert.AreEqual(20313, source.Port);
            Assert.AreEqual("testuser", source.UserName);
            Assert.AreEqual("test123", source.Password);
            Assert.AreEqual("/", source.VirtualHost);
            Assert.AreEqual(Guid.Parse("0b142714-8f6d-41b7-9832-2aefa8c731ec"), source.SourceId);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SourceOptions_PlainAmqpPort_LeavesTlsOffForActivityParity()
        {
            var source = RabbitMqSourceOptions.FromBiteFile(ResourcePath(RealSource));

            // Deliberate parity default: the activity never sets ConnectionFactory.Ssl.
            Assert.IsFalse(source.UseSsl);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SourceOptions_TlsPort5671_EnablesSslWithoutAnExplicitFlag()
        {
            var source = RabbitMqSourceOptions.FromBiteFile(ResourcePath(TlsSource));

            Assert.IsTrue(source.UseSsl);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SourceOptions_ForceSslOverride_WinsOverTheConnectionString()
        {
            var source = RabbitMqSourceOptions.FromBiteFile(ResourcePath(RealSource), forceSsl: true);

            Assert.IsTrue(source.UseSsl, "RABBITMQ__USESSL must be able to turn TLS on for a plain source");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SourceOptions_Describe_NeverLeaksThePassword()
        {
            var source = RabbitMqSourceOptions.FromBiteFile(ResourcePath(RealSource));

            var described = source.Describe();

            Assert.IsFalse(described.Contains("test123", StringComparison.Ordinal),
                $"Describe() is used in startup logs and must not contain the password: '{described}'");
            StringAssert.Contains(described, "testuser");
            StringAssert.Contains(described, "server.ngrok.io");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SourceOptions_OperatorNamedSourceInSettingsRoot_ParsesLikeTheElasticsearchSource()
        {
            // The staging convention is the engine's own: a NAMED source file staged directly into
            // Settings\, exactly as Settings\ElasticsearchLoggingSource.bite is. The file name
            // carries no meaning - the ID attribute inside is what binds it to the trigger.
            var source = RabbitMqSourceOptions.FromBiteFile(ResourcePath(NamedSource));

            Assert.AreEqual(Guid.Parse("1a82a341-b678-4992-a25a-39cdd57198d4"), source.SourceId);
            Assert.AreEqual("Warewolf DevOps RabbitMQ Source", source.SourceName);
            Assert.AreEqual("4.tcp.eu.ngrok.io", source.HostName);
            Assert.AreEqual(20313, source.Port);
            Assert.AreEqual("testuser", source.UserName);
            Assert.AreEqual("/", source.VirtualHost);
            Assert.IsFalse(source.UseSsl, "an ngrok TCP tunnel on 20313 is plain AMQP");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SourceOptions_MissingFile_Throws()
        {
            Assert.ThrowsException<TriggerConfigurationException>(
                () => RabbitMqSourceOptions.FromBiteFile(ResourcePath("no-such-source.bite")));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SourceOptions_NoConnectionStringAttribute_Throws()
        {
            var path = Path.Combine(Path.GetTempPath(), $"src-{Guid.NewGuid()}.bite");
            File.WriteAllText(path, "<Source ID=\"0b142714-8f6d-41b7-9832-2aefa8c731ec\" />");

            try
            {
                var ex = Assert.ThrowsException<TriggerConfigurationException>(
                    () => RabbitMqSourceOptions.FromBiteFile(path));
                StringAssert.Contains(ex.Message, "ConnectionString");
            }
            finally
            {
                File.Delete(path);
            }
        }

        // ── ResolvedQueueConfiguration (workflow path + queue arguments) ──────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ResolvedConfiguration_NormalisesBackslashWorkflowSeparators()
        {
            var resolved = BuildResolved();

            // The Server stores 'ProfilerWrapper\Queue\MandateCollectionSuccessConsume'.
            // Per-segment escaping splits on '/', so an unnormalised backslash would be
            // percent-encoded to %5C and the engine route would not resolve.
            Assert.AreEqual("ProfilerWrapper/Queue/MandateCollectionSuccessConsume", resolved.WorkflowPath);
            Assert.IsFalse(resolved.WorkflowPath.Contains('\\'));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ResolvedConfiguration_SurfacesQueueDeclareArgumentsAndDeadLetter()
        {
            var resolved = BuildResolved();

            Assert.IsTrue(resolved.Durable);
            Assert.IsFalse(resolved.Exclusive);
            Assert.IsFalse(resolved.AutoDelete);
            Assert.AreEqual((ushort)10, resolved.Prefetch);
            Assert.IsTrue(resolved.HasDeadLetter);
            Assert.IsTrue(resolved.DeadLetterDurable);
        }

        static ResolvedQueueConfiguration BuildResolved()
        {
            var trigger = new TriggerBiteReader().Read(TriggerPath(RealTrigger));
            var source = RabbitMqSourceOptions.FromBiteFile(ResourcePath(RealSource));
            return new ResolvedQueueConfiguration
            {
                Trigger = trigger,
                Source = source,
                DeadLetterSource = source,
            };
        }

        // ── QueueProcessorOptions.Validate (timeout nesting) ─────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Options_EngineTimeoutWithinDrainWindow_IsAccepted()
        {
            var options = new QueueProcessorOptions
            {
                BaseUrl = "https://engine", ResourceAppId = "app",
                EngineTimeoutSeconds = 45, ShutdownGraceSeconds = 60,
            };

            options.Validate(terminationGracePeriodSeconds: 90);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Options_EngineTimeoutExceedingDrainWindow_ThrowsExplainingDuplicates()
        {
            var options = new QueueProcessorOptions
            {
                BaseUrl = "https://engine", ResourceAppId = "app",
                EngineTimeoutSeconds = 120, ShutdownGraceSeconds = 60,
            };

            var ex = Assert.ThrowsException<TriggerConfigurationException>(
                () => options.Validate(terminationGracePeriodSeconds: 90));

            StringAssert.Contains(ex.Message, "duplicate execution");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Options_DrainWindowNotInsideTerminationGrace_Throws()
        {
            var options = new QueueProcessorOptions
            {
                BaseUrl = "https://engine", ResourceAppId = "app",
                EngineTimeoutSeconds = 45, ShutdownGraceSeconds = 90,
            };

            var ex = Assert.ThrowsException<TriggerConfigurationException>(
                () => options.Validate(terminationGracePeriodSeconds: 90));

            StringAssert.Contains(ex.Message, "SIGKILL");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Options_EffectiveScope_DefaultsToTheEngineResourceAppId()
        {
            var options = new QueueProcessorOptions { ResourceAppId = "abc-123" };

            Assert.AreEqual("api://abc-123/.default", options.EffectiveScope);
        }

        // ── Loader (trigger selection) ───────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Loader_MultipleTriggersWithoutTriggerId_ThrowsAsAmbiguous()
        {
            // This app serves exactly one queue, so two staged triggers and no selector is a
            // deployment mistake that must not be resolved by picking one arbitrarily.
            var options = new QueueProcessorOptions
            {
                SettingsPath = SettingsDir,
                TriggerFilter = "*.bite",
                BaseUrl = "https://engine",
                ResourceAppId = "app",
            };

            var loader = new QueueConfigurationLoader(
                new OptionsWrapper<QueueProcessorOptions>(options), new TriggerBiteReader());

            var ex = Assert.ThrowsException<TriggerConfigurationException>(() => loader.Load());
            StringAssert.Contains(ex.Message, "ambiguous");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Loader_UnknownTriggerId_Throws()
        {
            var options = new QueueProcessorOptions
            {
                SettingsPath = SettingsDir,
                TriggerFilter = "triggers-mandate.bite",
                TriggerId = Guid.NewGuid().ToString(),
                BaseUrl = "https://engine",
                ResourceAppId = "app",
            };

            var loader = new QueueConfigurationLoader(
                new OptionsWrapper<QueueProcessorOptions>(options), new TriggerBiteReader());

            var ex = Assert.ThrowsException<TriggerConfigurationException>(() => loader.Load());
            StringAssert.Contains(ex.Message, "No staged trigger file matched");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Loader_SingleTrigger_ResolvesTriggerAndSourceAndSharesTheDeadLetterSource()
        {
            var options = new QueueProcessorOptions
            {
                SettingsPath = SettingsDir,
                TriggerFilter = "triggers-mandate.bite",
                BaseUrl = "https://engine",
                ResourceAppId = "app",
            };

            var loader = new QueueConfigurationLoader(
                new OptionsWrapper<QueueProcessorOptions>(options), new TriggerBiteReader());

            var resolved = loader.Load();

            Assert.AreEqual("profiler.mandatecollectionsuccess.request", resolved.QueueName);
            Assert.AreEqual("server.ngrok.io", resolved.Source.HostName);

            // QueueSinkId == QueueSourceId in the real trigger, so one staged source serves both.
            Assert.AreSame(resolved.Source, resolved.DeadLetterSource);
        }

        // ── Durability is per-queue, driven only by the trigger ──────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Trigger_DurableIsReadIndependentlyForTheWorkQueueAndTheDeadLetterQueue()
        {
            // A queue is durable or not according to the TRIGGER, and the two option lists are
            // independent: Options drives the work queue, DeadLetterOptions the dead-letter queue.
            // Conflating them makes the broker reject the re-declare with
            // "PRECONDITION_FAILED - inequivalent arg 'durable'" and the replica cannot consume.
            var trigger = new TriggerBiteReader().Read(TriggerPath(MixedDurabilityTrigger));

            Assert.IsTrue(trigger.OptionBool("Durable"), "work queue is declared durable");
            Assert.IsFalse(trigger.DeadLetterOptionBool("Durable"), "dead-letter queue is NOT durable");

            // Exclusive is declared only on the work queue; it must not leak to the dead-letter side.
            Assert.IsTrue(trigger.OptionBool("Exclusive"));
            Assert.IsFalse(trigger.DeadLetterOptionBool("Exclusive"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ResolvedConfiguration_SurfacesTheTwoDurabilityFlagsSeparately()
        {
            var resolved = LoaderFor(StagedOptions(MixedDurabilityTrigger)).Load();

            Assert.IsTrue(resolved.Durable, "QueueDeclare(durable) for the work queue");
            Assert.IsFalse(resolved.DeadLetterDurable, "QueueDeclare(durable) for the dead-letter queue");
            Assert.IsTrue(resolved.Exclusive);
            Assert.IsFalse(resolved.AutoDelete, "undeclared options stay false");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Trigger_UndeclaredDurableDefaultsToFalseRatherThanTheOptionDefaultField()
        {
            // The option carries BOTH Value and Default; only Value describes the live queue.
            // Falling back to Default (true in the Server's contract) would declare a durable
            // queue against a non-durable one.
            var trigger = new TriggerBiteReader().Read(TriggerPath(MixedDurabilityTrigger));

            Assert.IsFalse(trigger.OptionBool("NoSuchOption"));
            Assert.IsFalse(trigger.DeadLetterOptionBool("NoSuchOption"));
        }

        // ── Sources folder (Settings/sources) ────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Options_SourcesPath_DefaultsToSourcesUnderSettings()
        {
            var options = new QueueProcessorOptions { SettingsPath = @"X:\app\Settings" };

            Assert.AreEqual(@"X:\app\Settings\sources", options.SourcesPath);
            Assert.AreEqual(@"X:\app\Settings\triggers", options.TriggersPath);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Options_DefaultSettingsTree_IsAnchoredToBaseDirectoryNotWorkingDirectory()
        {
            // The tree is deployed ALONGSIDE the assembly. Anchoring to the working directory
            // silently resolved the wrong folder depending on how the worker was launched
            // (dotnet run vs WORKDIR /app vs debugger).
            var options = new QueueProcessorOptions();

            Assert.AreEqual(Path.Combine(AppContext.BaseDirectory, "Settings"), options.SettingsPath);
            StringAssert.StartsWith(options.TriggersPath, AppContext.BaseDirectory);
            StringAssert.StartsWith(options.SourcesPath, AppContext.BaseDirectory);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Options_AbsoluteSubPath_OverridesTheSettingsRoot()
        {
            var options = new QueueProcessorOptions
            {
                SettingsPath = @"X:\app\Settings",
                TriggersSubPath = @"Y:\elsewhere\triggers",
                SourcesSubPath = @"Y:\elsewhere\sources",
            };

            Assert.AreEqual(@"Y:\elsewhere\triggers", options.TriggersPath);
            Assert.AreEqual(@"Y:\elsewhere\sources", options.SourcesPath);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Loader_ResolvesTheSourceFromTheSourcesSubFolder()
        {
            // The staged fixture tree has sources ONLY in TestResources\sources\, so a resolution
            // that still looked in the Settings root would fail here.
            var resolved = LoaderFor(StagedOptions(RealTrigger)).Load();

            Assert.AreEqual(Guid.Parse("0b142714-8f6d-41b7-9832-2aefa8c731ec"), resolved.Source.SourceId);
            Assert.AreEqual("server.ngrok.io", resolved.Source.HostName);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Loader_ResolvesAnOperatorNamedSourceFileByScanningForTheMatchingId()
        {
            // Studio writes "<Display Name>.bite", the deploy writes "{sourceId}.bite". Both must
            // resolve, or a hand-staged source silently fails at cold start.
            var options = StagedOptions(MixedDurabilityTrigger);
            var resolved = LoaderFor(options).Load();

            Assert.AreEqual(Guid.Parse("0b142714-8f6d-41b7-9832-2aefa8c731ec"), resolved.Source.SourceId);
            Assert.IsTrue(File.Exists(ResourcePath(NamedSource)),
                "the operator-named fixture must stay in place - it covers the ID-scan branch");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Loader_MissingSource_NamesEveryFolderItSearched()
        {
            // A cold-start failure must say WHERE it looked; "not found" alone sends operators
            // hunting through the image.
            var dir = Path.Combine(Path.GetTempPath(), "wwqp-nosource-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(dir, "triggers"));
            Directory.CreateDirectory(Path.Combine(dir, "sources"));
            try
            {
                File.Copy(TriggerPath(RealTrigger), Path.Combine(dir, "triggers", RealTrigger));

                var options = StagedOptions(RealTrigger);
                options.SettingsPath = dir;

                var ex = Assert.ThrowsException<TriggerConfigurationException>(
                    () => LoaderFor(options).Load());

                StringAssert.Contains(ex.Message, "sources");
                StringAssert.Contains(ex.Message, "0b142714-8f6d-41b7-9832-2aefa8c731ec");
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Loader_LegacyFlatLayout_StillResolvesSourcesFromTheSettingsRoot()
        {
            // Backward compatibility with the earlier layout (and the engine's own
            // Settings\ElasticsearchLoggingSource.bite convention): a deployment that staged
            // sources in the root must keep starting after the upgrade.
            var dir = Path.Combine(Path.GetTempPath(), "wwqp-flat-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path.Combine(dir, "triggers"));
            try
            {
                File.Copy(TriggerPath(RealTrigger), Path.Combine(dir, "triggers", RealTrigger));
                File.Copy(ResourcePath(RealSource), Path.Combine(dir, RealSource));

                var options = StagedOptions(RealTrigger);
                options.SettingsPath = dir;

                var resolved = LoaderFor(options).Load();

                Assert.AreEqual(Guid.Parse("0b142714-8f6d-41b7-9832-2aefa8c731ec"),
                                resolved.Source.SourceId);
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }

        // ── Source catalog: one startup scan, cached by id ───────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SourceCatalog_ReadsEveryStagedSourceOnceAndIndexesThemById()
        {
            var catalog = new RabbitMqSourceCatalog(
                new OptionsWrapper<QueueProcessorOptions>(StagedOptions("*.bite")));

            // The fixture sources folder holds the real source, a TLS source and an
            // operator-named copy of the dev source - all indexed by their ID attribute, not by
            // filename, so both naming conventions resolve from one scan.
            Assert.IsTrue(catalog.Count >= 2, $"expected the staged sources to be cached, got {catalog.Count}");
            Assert.IsTrue(catalog.TryGet(Guid.Parse("0b142714-8f6d-41b7-9832-2aefa8c731ec"), out var real));
            Assert.AreEqual("server.ngrok.io", real!.HostName);
            Assert.IsTrue(catalog.TryGet(Guid.Parse("1a82a341-b678-4992-a25a-39cdd57198d4"), out var named));
            Assert.AreEqual("4.tcp.eu.ngrok.io", named!.HostName);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SourceCatalog_ReturnsTheSameCachedInstanceOnEveryLookup()
        {
            // The point of the cache: a reference is served from memory, so nothing re-reads a
            // .bite after startup. A reconnect or dead-letter publish therefore cannot fail on a
            // removed or half-written file mid-message.
            var catalog = new RabbitMqSourceCatalog(
                new OptionsWrapper<QueueProcessorOptions>(StagedOptions("*.bite")));
            var id = Guid.Parse("0b142714-8f6d-41b7-9832-2aefa8c731ec");

            Assert.AreSame(catalog.Get(id, "queue"), catalog.Get(id, "dead-letter"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SourceCatalog_SurvivesNonRabbitFilesInTheTree()
        {
            // The trigger files and the engine's Elasticsearch source legitimately live in this
            // tree. They must be skipped, not abort the scan.
            var dir = Path.Combine(Path.GetTempPath(), "wwqp-mixed-" + Guid.NewGuid().ToString("N"));
            var sources = Path.Combine(dir, "sources");
            Directory.CreateDirectory(sources);
            try
            {
                File.Copy(ResourcePath(RealSource), Path.Combine(sources, RealSource));
                File.Copy(TriggerPath(RealTrigger), Path.Combine(sources, "a-trigger.bite"));
                File.WriteAllText(Path.Combine(sources, "not-a-source.bite"), "<Source Type=\"Elasticsearch\" />");

                var options = StagedOptions("*.bite");
                options.SettingsPath = dir;
                var catalog = new RabbitMqSourceCatalog(new OptionsWrapper<QueueProcessorOptions>(options));

                Assert.AreEqual(1, catalog.Count, "only the RabbitMQ source is cached");
                Assert.IsTrue(catalog.TryGet(Guid.Parse("0b142714-8f6d-41b7-9832-2aefa8c731ec"), out _));
            }
            finally
            {
                Directory.Delete(dir, recursive: true);
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SourceCatalog_UnknownId_ReportsFoldersSearchedAndIdsAvailable()
        {
            var catalog = new RabbitMqSourceCatalog(
                new OptionsWrapper<QueueProcessorOptions>(StagedOptions("*.bite")));
            var missing = Guid.NewGuid();

            var ex = Assert.ThrowsException<TriggerConfigurationException>(
                () => catalog.Get(missing, "queue"));

            StringAssert.Contains(ex.Message, missing.ToString());
            StringAssert.Contains(ex.Message, "sources");
            StringAssert.Contains(ex.Message, "0b142714-8f6d-41b7-9832-2aefa8c731ec",
                "the message must list what IS staged, so the gap is obvious");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SourceCatalog_EmptySourceId_IsReportedAsAMissingTriggerFieldNotAMissingFile()
        {
            var catalog = new RabbitMqSourceCatalog(
                new OptionsWrapper<QueueProcessorOptions>(StagedOptions("*.bite")));

            var ex = Assert.ThrowsException<TriggerConfigurationException>(
                () => catalog.Get(Guid.Empty, "dead-letter"));

            StringAssert.Contains(ex.Message, "no dead-letter source id");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void SourceCatalog_MissingFolders_LoadEmptyRatherThanThrowing()
        {
            // A replica with no sources staged must fail when a TRIGGER asks for one (a clear,
            // attributable error), not while scanning an absent directory.
            var options = StagedOptions("*.bite");
            options.SettingsPath = Path.Combine(Path.GetTempPath(), "wwqp-absent-" + Guid.NewGuid().ToString("N"));

            var catalog = new RabbitMqSourceCatalog(new OptionsWrapper<QueueProcessorOptions>(options));

            Assert.AreEqual(0, catalog.Count);
            Assert.ThrowsException<TriggerConfigurationException>(() => catalog.Get(Guid.NewGuid(), "queue"));
        }

        // ── "Empty means unset" (the two silent config defects) ──────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ConfigurationValues_ReadString_TreatsBlankAsUnsetSoPlaceholdersCannotWin()
        {
            // appsettings.json ships "" placeholders and ACA turns an unset variable into "".
            // With `??` those placeholders beat the default and QUEUE__SETTINGSPATH="" resolved
            // the settings tree against the process working directory.
            Assert.AreEqual("fallback", ConfigurationValues.ReadString(null, "fallback"));
            Assert.AreEqual("fallback", ConfigurationValues.ReadString("", "fallback"));
            Assert.AreEqual("fallback", ConfigurationValues.ReadString("   ", "fallback"));
            Assert.AreEqual("supplied", ConfigurationValues.ReadString("supplied", "fallback"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void ConfigurationValues_NullIfBlank_CollapsesEmptyToNull()
        {
            Assert.IsNull(ConfigurationValues.NullIfBlank(null));
            Assert.IsNull(ConfigurationValues.NullIfBlank(""));
            Assert.IsNull(ConfigurationValues.NullIfBlank("\t "));
            Assert.AreEqual("value", ConfigurationValues.NullIfBlank("value"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Options_EffectiveTenantId_IsNullWhenBlankSoCredentialsCanInferTheTenant()
        {
            // Azure credentials REJECT "" with "Invalid tenant id provided" but accept null as
            // "infer it" - which is the documented default for a system-assigned managed
            // identity. Propagating "" failed every credential in the chain and looked like a
            // missing app role rather than a missing setting.
            Assert.IsNull(new QueueProcessorOptions { TenantId = "" }.EffectiveTenantId);
            Assert.IsNull(new QueueProcessorOptions { TenantId = "  " }.EffectiveTenantId);
            Assert.IsNull(new QueueProcessorOptions { TenantId = null }.EffectiveTenantId);
            Assert.AreEqual("tid-123",
                new QueueProcessorOptions { TenantId = "tid-123" }.EffectiveTenantId);
        }

        // ── Prefetch: the optimum is Prefetch == MaxConcurrency ──────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Trigger_MissingOrInvalidPrefetch_DefaultsToOneNotZero()
        {
            // Prefetch 0 in AMQP means UNLIMITED, which would hand the whole queue to one replica
            // and defeat scale-out entirely. 1 is both the safe and the optimum default.
            Assert.AreEqual((ushort)1, new TriggerDefinition { Prefetch = null }.ResolvedPrefetch);
            Assert.AreEqual((ushort)1, new TriggerDefinition { Prefetch = "" }.ResolvedPrefetch);
            Assert.AreEqual((ushort)1, new TriggerDefinition { Prefetch = "0" }.ResolvedPrefetch);
            Assert.AreEqual((ushort)1, new TriggerDefinition { Prefetch = "not-a-number" }.ResolvedPrefetch);
            Assert.AreEqual((ushort)7, new TriggerDefinition { Prefetch = "7" }.ResolvedPrefetch);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Options_DefaultMaxConcurrencyIsOne_MatchingTheMeasuredSerialDispatch()
        {
            // Dispatch is serial per channel (measured: deliveries ~2.2s apart, no overlap), so
            // one replica runs one workflow at a time. This default is what makes
            // maxReplicas = Concurrency the correct parity mapping; raising it is a deliberate
            // throughput change that also requires raising Prefetch to match.
            Assert.AreEqual(1, new QueueProcessorOptions().MaxConcurrency);
        }
    }
}

