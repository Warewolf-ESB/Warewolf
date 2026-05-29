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
using Dev2.PathOperations;
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
