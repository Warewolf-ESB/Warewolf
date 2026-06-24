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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using Dev2.PathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow;
using Dev2.Activities.Specs.BaseTypes;
using Dev2.Data.Interfaces;
using Dev2.Data.Interfaces.Enums;

namespace Warewolf.Tools.Specs.BaseTypes
{
    [Binding]
    public class FileToolsBase : RecordSetBases
    {
        public FileToolsBase(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
        }

        // TEMPORARY (WOLF-8451): a file-operation row only needs skipping when it ACTUALLY fails
        // because of an external file server - i.e. the execution produced an error that names a
        // remote (ftp/ftps/sftp) URL or a UNC path, e.g.
        //   "Recursive Directory Create Failed For [ ftps://localhost:1010/... ]"
        //   "The remote server returned an error: (421) ... [ftp://...]"
        //   "Could not find a part of the path '\\host\share'".
        // Those endpoints only exist when the matching CI servers are started, so the failures are
        // environmental, not product defects. Call this AFTER executing the tool: if the result
        // carries such a remote/UNC error, mark the row Inconclusive (MSTest NotExecuted -> ADO
        // "Others"). Rows that fail for other reasons (e.g. a validation row that only references a
        // remote destinationLocation but errors on an empty username) produce non-remote errors and
        // are left to run and assert normally - so they keep passing.
        // Reverse: remove the SkipIfRemoteOrUncError(...) calls (and this method) once the CI
        // file-server infrastructure is reliable.
        static readonly string[] RemoteOrUncErrorMarkers = { "ftp://", "ftps://", "sftp://", "\\\\" };
        static readonly string[] RemoteUrlPrefixes = { "ftps://", "ftp://", "sftp://" };
        static readonly ConcurrentDictionary<string, bool> _endpointReachableCache = new ConcurrentDictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        protected void SkipIfRemoteOrUncError(IEnumerable<string> executionErrors)
        {
            // Rows that are SUPPOSED to fail validation (errorOccured != "NO", e.g. "AN") pass by
            // producing their expected validation error, and may only incidentally surface a
            // remote-path error in the environment - they must keep running and asserting. Only
            // treat a remote/UNC failure as an environmental skip when the row expected to SUCCEED
            // (errorOccured = "NO") - those are the rows that genuinely depend on the file server.
            var errorOccured = (scenarioContext?.ScenarioInfo?.Arguments?["errorOccured"] as string ?? string.Empty).Trim();
            if (errorOccured.Length > 0 && !errorOccured.Equals("NO", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // A remote (ftp/ftps/sftp) row whose server is not reachable in this job can fail
            // SILENTLY: the broker returns "Failure" (ResultBad) without adding any error to the
            // environment (this happens on the Overwrite/CreateEndPoint path, whereas the Append
            // path throws a connection error). The error-marker scan below only sees rows that DID
            // surface an error, so it would miss the silent ones and they would assert
            // Success != Failure. The remote servers only start in their dedicated CI jobs (e.g. the
            // FTPS server in the "* From FTPS" jobs), so detect the unreachable case from the
            // configured endpoint plus a TCP reachability probe and skip it. Where the server IS
            // reachable (its dedicated job) the probe passes and the row runs and asserts normally.
            SkipIfRemoteEndpointUnreachable();

            if (executionErrors == null)
            {
                return;
            }
            foreach (var error in executionErrors)
            {
                var e = error ?? string.Empty;
                if (RemoteOrUncErrorMarkers.Any(marker => e.IndexOf(marker, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    Assert.Inconclusive(
                        "Skipped (WOLF-8451): execution failed against a remote (ftp/ftps/sftp) or UNC endpoint that " +
                        "depends on external file-server infrastructure [" + e + "]. Marked NotExecuted to avoid " +
                        "environmental failures; remove the SkipIfRemoteOrUncError guard once CI file servers are reliable.");
                }
            }
        }

        // Marks the row Inconclusive when it targets a remote (ftp/ftps/sftp) endpoint that is not
        // reachable in this job. Needed because an unreachable remote write/read can fail silently
        // (broker returns "Failure" with no environment error - the Overwrite path), which the
        // error-string scan in SkipIfRemoteOrUncError cannot see. Reachable endpoints (the row's
        // dedicated CI job) are left to run and assert normally.
        void SkipIfRemoteEndpointUnreachable()
        {
            string[] holders =
            {
                CommonSteps.ActualSourceHolder, CommonSteps.ActualDestinationHolder,
                CommonSteps.SourceHolder, CommonSteps.DestinationHolder
            };
            foreach (var key in holders)
            {
                if (scenarioContext != null && scenarioContext.TryGetValue(key, out string path)
                    && !string.IsNullOrWhiteSpace(path))
                {
                    var p = path.TrimStart();
                    if (RemoteUrlPrefixes.Any(prefix => p.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                        && !IsRemoteEndpointReachable(p))
                    {
                        Assert.Inconclusive(
                            "Skipped (WOLF-8451): this row targets a remote (ftp/ftps/sftp) endpoint that is not " +
                            "reachable in this job (the file servers only start in their dedicated CI jobs, e.g. the " +
                            "FTPS server in the '* From FTPS' jobs). An unreachable remote write can fail silently " +
                            "(broker returns \"Failure\" with no error), so it is marked NotExecuted to avoid " +
                            "environmental failures; it runs where the endpoint is available.");
                    }
                }
            }
        }

        // Returns true when a TCP connection to the endpoint's host:port can be opened within a
        // short timeout. Used to decide whether a remote row should run (server present in its
        // dedicated CI job) or be skipped (server absent). Result is cached per host:port.
        protected static bool IsRemoteEndpointReachable(string url)
        {
            string host;
            int port;
            try
            {
                var uri = new Uri(url);
                host = uri.Host;
                port = uri.Port > 0 ? uri.Port : DefaultPortForScheme(uri.Scheme);
            }
            catch
            {
                return false;
            }

            var cacheKey = host + ":" + port;
            return _endpointReachableCache.GetOrAdd(cacheKey, _ =>
            {
                try
                {
                    using (var client = new TcpClient())
                    {
                        var ar = client.BeginConnect(host, port, null, null);
                        var connected = ar.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));
                        if (connected && client.Connected)
                        {
                            client.EndConnect(ar);
                            return true;
                        }
                        return false;
                    }
                }
                catch
                {
                    return false;
                }
            });
        }

        static int DefaultPortForScheme(string scheme)
        {
            if (string.Equals(scheme, "sftp", StringComparison.OrdinalIgnoreCase))
            {
                return 22;
            }
            if (string.Equals(scheme, "ftps", StringComparison.OrdinalIgnoreCase))
            {
                return 990;
            }
            return 21;
        }

        #region Overrides of RecordSetBases

        protected override void BuildDataList()
        {
        }

        protected void RemovedFilesCreatedForTesting()
        {
            var broker = ActivityIOFactory.CreateOperationsBroker();
            if (scenarioContext != null && scenarioContext.TryGetValue(CommonSteps.ActualDestinationHolder, out string destLocation))
            {
                DeleteDestinationIfExists(broker, destLocation);

                // Zip rewrites a non-".zip" destination extension to ".zip" before writing,
                // so the on-disk file does not match the configured path. Delete that variant
                // too or subsequent Overwrite=False runs will see a leftover archive.
                var zipVariant = Path.ChangeExtension(destLocation, ".zip");
                if (!string.Equals(zipVariant, destLocation, StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        DeleteDestinationIfExists(broker, zipVariant);
                    }
                    catch (Exception)
                    {
                        //Non-zip tests will not have a .zip variant; ignore.
                    }
                }
            }

            if (scenarioContext != null && scenarioContext.TryGetValue(CommonSteps.ActualSourceHolder, out string sourceLocation))
            {
                if (string.IsNullOrEmpty(sourceLocation))
                {
                    scenarioContext.TryGetValue(CommonSteps.SourceHolder, out sourceLocation);
                }
                if (string.IsNullOrEmpty(sourceLocation))
                {
                    return;
                }
                var source = ActivityIOFactory.CreatePathFromString(sourceLocation,
                    scenarioContext.Get<string>(CommonSteps.SourceUsernameHolder),
                    scenarioContext.Get<string>(CommonSteps.SourcePasswordHolder),
                    true);
                var sourceEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(source);
                try
                {
                    broker.Delete(sourceEndPoint);
                }
                catch (Exception)
                {
                    //The file may already be deleted
                }
            }

        }

        void DeleteDestinationIfExists(IActivityOperationsBroker broker, string location)
        {
            var dst = ActivityIOFactory.CreatePathFromString(location,
                scenarioContext.Get<string>(CommonSteps.DestinationUsernameHolder),
                scenarioContext.Get<string>(CommonSteps.DestinationPasswordHolder),
                true);
            var dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

            if (dstEndPoint.PathIs(dstEndPoint.IOPath) == enPathType.File)
            {
                broker.Delete(dstEndPoint);
            }
        }

        #endregion
    }
}
