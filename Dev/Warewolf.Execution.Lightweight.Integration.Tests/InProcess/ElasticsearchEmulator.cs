/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  In-process Elasticsearch stub for ElasticsearchLoggerTests.
 *
 *  The shipped Settings/ElasticsearchLoggingSource.bite points at a remote (encrypted)
 *  Elasticsearch instance, so those tests were skipped (Inconclusive) whenever it was
 *  unreachable. This WireMock stub gives the tests a deterministic, always-reachable ES
 *  endpoint: the test's availability probe (a raw GET) gets 200, and the logger's
 *  fire-and-forget IndexAsync calls (Elastic.Clients.Elasticsearch) hit a stub that
 *  returns an Elasticsearch-shaped 200 with the X-elastic-product header the 8.x client
 *  expects, so no background errors are produced.
 *
 *  Lifecycle is driven by the single assembly fixture (IntegrationTestAssemblyInit) —
 *  MSTest permits only one [AssemblyInitialize] per assembly. A random port is used
 *  (the URL is read at runtime by the test, not baked into any fixture), avoiding any
 *  clash with a real Elasticsearch on :9200.
 */

using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Warewolf.Execution.Lightweight.Integration.Tests.InProcess
{
    internal static class ElasticsearchEmulator
    {
        private static WireMockServer? _server;

        /// <summary>Base URL of the running stub, e.g. http://localhost:port. Valid after <see cref="Start"/>.</summary>
        public static string Url => _server?.Url
            ?? throw new System.InvalidOperationException("ElasticsearchEmulator has not been started.");

        private const string ClusterInfo =
            "{\"name\":\"warewolf-test\",\"cluster_name\":\"warewolf-test\"," +
            "\"cluster_uuid\":\"warewolf0000000000000A\"," +
            "\"version\":{\"number\":\"8.13.0\",\"build_flavor\":\"default\"," +
            "\"build_type\":\"docker\",\"lucene_version\":\"9.10.0\"," +
            "\"minimum_wire_compatibility_version\":\"7.17.0\"," +
            "\"minimum_index_compatibility_version\":\"7.0.0\"}," +
            "\"tagline\":\"You Know, for Search\"}";

        public static void Start()
        {
            if (_server != null)
                return;

            _server = WireMockServer.Start();

            // Any request → 200 with the X-elastic-product header the Elastic 8.x client's
            // product check requires. GET / returns cluster info; index/_bulk calls also get
            // a 200 (the logger's calls are fire-and-forget, so the body shape is not asserted).
            _server
                .Given(Request.Create().WithPath(new WildcardMatcher("*")).UsingAnyMethod())
                .RespondWith(
                    Response.Create()
                        .WithStatusCode(200)
                        .WithHeader("Content-Type", "application/json")
                        .WithHeader("X-elastic-product", "Elasticsearch")
                        .WithBody(ClusterInfo));
        }

        public static void Stop()
        {
            _server?.Stop();
            _server = null;
        }
    }
}
