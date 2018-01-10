/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
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
        readonly ScenarioContext scenarioContext;

        public FileToolsBase(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null)
            {
                throw new ArgumentNullException("scenarioContext");
            }

            this.scenarioContext = scenarioContext;
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
                var dst = ActivityIOFactory.CreatePathFromString(destLocation,
                    scenarioContext.Get<string>(CommonSteps.DestinationUsernameHolder),
                    scenarioContext.Get<string>(CommonSteps.DestinationPasswordHolder),
                    true);
                var dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);

                if (dstEndPoint.PathIs(dstEndPoint.IOPath) == enPathType.File)
                {
                    broker.Delete(dstEndPoint);
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

        #endregion
    }
}
