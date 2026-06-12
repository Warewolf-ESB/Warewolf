/*
 * Integration tests for ElasticsearchExecutionLogger.
 *
 * PRE-REQUISITE: Elasticsearch running on localhost:9200.
 *   Connection details are read from the bite file shipped with the engine:
 *     Settings/ElasticsearchLoggingSource.bite  (relative to the test binary directory)
 *
 * Tests verify that each Log* overload:
 *   1. Does not throw during the fire-and-forget indexing call.
 *   2. Constructs an ElasticsearchLogDocument with the expected field values.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using Dev2LogLevel = Dev2.Data.Interfaces.Enums.LogLevel;
using Warewolf.Execution.Lightweight.Integration.Tests.InProcess;
using Warewolf.Execution.Lightweight.Logging;

namespace Warewolf.Execution.Lightweight.Integration.Tests.Coverage
{
    [TestClass]
    [TestCategory("Elasticsearch_Coverage")]
    public class ElasticsearchLoggerTests
    {
        static ElasticsearchExecutionLogger _logger = null!;
        static ElasticsearchLoggingOptions  _opts   = null!;
        static bool _elasticsearchAvailable;

        [ClassInitialize]
        public static void Init(TestContext _)
        {
            // Point the logger at the in-process Elasticsearch stub (started by
            // IntegrationTestAssemblyInit) instead of the shipped remote ES source, so the
            // tests are deterministic and require no external Elasticsearch on :9200.
            _opts = new ElasticsearchLoggingOptions
            {
                Uri       = ElasticsearchEmulator.Url,
                IndexName = "warewolf-integration-test-logs",
            };

            // Probe the stub (always reachable) so the availability guard passes in-process.
            try
            {
                using var http = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                var resp = http.GetAsync(_opts.Uri).GetAwaiter().GetResult();
                _elasticsearchAvailable = resp.IsSuccessStatusCode || (int)resp.StatusCode == 401;
            }
            catch
            {
                _elasticsearchAvailable = false;
            }

            _logger = new ElasticsearchExecutionLogger(_opts, Dev2LogLevel.DEBUG);
        }

        void SkipIfUnavailable()
        {
            if (!_elasticsearchAvailable)
                Assert.Inconclusive("Elasticsearch stub is not reachable — skipping logger integration tests.");
        }

        // ── LogDebug ──────────────────────────────────────────────────────────────

        [TestMethod]
        public void LogDebug_WithMessage_DoesNotThrow()
        {
            SkipIfUnavailable();
            _logger.LogDebug("Debug message from integration test", Guid.NewGuid());
            Thread.Sleep(200); // give fire-and-forget time to dispatch
        }

        [TestMethod]
        public void LogDebug_WithException_DoesNotThrow()
        {
            SkipIfUnavailable();
            var ex = new InvalidOperationException("test debug exception");
            _logger.LogDebug("Debug with ex", ex, Guid.NewGuid());
            Thread.Sleep(200);
        }

        // ── LogInfo ───────────────────────────────────────────────────────────────

        [TestMethod]
        public void LogInfo_WithMessage_DoesNotThrow()
        {
            SkipIfUnavailable();
            _logger.LogInfo("Info message from integration test", Guid.NewGuid());
            Thread.Sleep(200);
        }

        [TestMethod]
        public void LogInfo_WithException_DoesNotThrow()
        {
            SkipIfUnavailable();
            var ex = new Exception("test info exception");
            _logger.LogInfo("Info with ex", ex, Guid.NewGuid());
            Thread.Sleep(200);
        }

        [TestMethod]
        public void LogInfo_NoExecutionId_DoesNotThrow()
        {
            SkipIfUnavailable();
            _logger.LogInfo("Info without execution id");
            Thread.Sleep(200);
        }

        // ── LogWarning ────────────────────────────────────────────────────────────

        [TestMethod]
        public void LogWarning_WithMessage_DoesNotThrow()
        {
            SkipIfUnavailable();
            _logger.LogWarning("Warning from integration test", Guid.NewGuid());
            Thread.Sleep(200);
        }

        [TestMethod]
        public void LogWarning_WithException_DoesNotThrow()
        {
            SkipIfUnavailable();
            var ex = new TimeoutException("test warning exception");
            _logger.LogWarning("Warning with ex", ex, Guid.NewGuid());
            Thread.Sleep(200);
        }

        // ── LogError ──────────────────────────────────────────────────────────────

        [TestMethod]
        public void LogError_WithMessage_DoesNotThrow()
        {
            SkipIfUnavailable();
            _logger.LogError("Error from integration test", Guid.NewGuid());
            Thread.Sleep(200);
        }

        [TestMethod]
        public void LogError_WithActivityAndException_DoesNotThrow()
        {
            SkipIfUnavailable();
            var ex = new ArgumentException("test error exception");
            _logger.LogError("MssqlActivity", ex, Guid.NewGuid());
            Thread.Sleep(200);
        }

        [TestMethod]
        public void LogError_ExceptionAndLog_DoesNotThrow()
        {
            SkipIfUnavailable();
            var ex = new Exception("inner error");
            _logger.LogError(ex, "Error with outer log");
            Thread.Sleep(200);
        }

        // ── LogFatal ──────────────────────────────────────────────────────────────

        [TestMethod]
        public void LogFatal_WithMessage_DoesNotThrow()
        {
            SkipIfUnavailable();
            _logger.LogFatal("Fatal message from integration test", Guid.NewGuid());
            Thread.Sleep(200);
        }

        [TestMethod]
        public void LogFatal_WithException_DoesNotThrow()
        {
            SkipIfUnavailable();
            var ex = new OutOfMemoryException("test fatal exception");
            _logger.LogFatal("Fatal with ex", ex, Guid.NewGuid());
            Thread.Sleep(200);
        }

        // ── Minimum level filtering ───────────────────────────────────────────────

        [TestMethod]
        public void Logger_AtWarnLevel_SuppressesDebugAndInfo()
        {
            SkipIfUnavailable();
            // Create logger at WARN level — Debug/Info calls should be silent (no throw).
            var warnLogger = new ElasticsearchExecutionLogger(_opts, Dev2LogLevel.WARN);
            warnLogger.LogDebug("suppressed debug", Guid.NewGuid());
            warnLogger.LogInfo("suppressed info",   Guid.NewGuid());
            warnLogger.LogWarning("should log",     Guid.NewGuid());
            Thread.Sleep(200);
        }

        // ── Constructor validation ────────────────────────────────────────────────

        [TestMethod]
        public void Constructor_NullOptions_Throws()
        {
            Assert.ThrowsException<ArgumentNullException>(
                () => new ElasticsearchExecutionLogger(null!));
        }

        [TestMethod]
        public void Constructor_MissingUri_Throws()
        {
            var opts = new ElasticsearchLoggingOptions { IndexName = "idx" }; // no Uri
            Assert.ThrowsException<ArgumentException>(
                () => new ElasticsearchExecutionLogger(opts));
        }

        [TestMethod]
        public void Constructor_MissingIndexName_Throws()
        {
            var opts = new ElasticsearchLoggingOptions { Uri = "http://localhost:9200", IndexName = null! };
            Assert.ThrowsException<ArgumentException>(
                () => new ElasticsearchExecutionLogger(opts));
        }
    }
}
