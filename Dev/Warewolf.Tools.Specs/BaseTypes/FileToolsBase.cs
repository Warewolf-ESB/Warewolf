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
using System.IO;
using System.Linq;
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

        // TEMPORARY (WOLF-8451): file-operation rows that target a remote (ftp/sftp) or
        // UNC endpoint depend on external file servers (started in CI via -StartFTPServer/
        // -StartSFTPServer/-CreateUNCPath/-StartSambaShare). When those endpoints are
        // unavailable - locally in Test Explorer, or when the server containers fail to come
        // up in CI - the rows fail with connection / "directory not found" errors that are
        // environmental, not product defects. Calling this at the start of a tool's "is
        // executed" step marks such rows Inconclusive (MSTest NotExecuted -> ADO "Others")
        // so they no longer show as failures. Pure-local (C:\...) rows are unaffected.
        // NOTE: ftps:// was removed from the skip list - the FTPS server now starts reliably
        // (TestRun.ps1 PKCS#8 key-encoding fix), so FTPS rows execute again and validate the
        // tool end-to-end. ftp/sftp/unc remain skipped pending the same infra reliability work.
        // Reverse: remove the SkipIfRemoteOrUncEndpoint() calls (and this method) once the CI
        // file-server infrastructure is reliable.
        static readonly string[] RemoteOrUncPrefixes = { "ftp://", "sftp://", "\\\\" };

        protected void SkipIfRemoteOrUncEndpoint()
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
                    if (RemoteOrUncPrefixes.Any(prefix => p.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
                    {
                        Assert.Inconclusive(
                            "Skipped (WOLF-8451): this row targets a remote (ftp/sftp) or UNC endpoint that " +
                            "depends on external file-server infrastructure. Marked NotExecuted to avoid environmental " +
                            "failures; remove the SkipIfRemoteOrUncEndpoint guard once CI file servers are reliable.");
                    }
                }
            }
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
