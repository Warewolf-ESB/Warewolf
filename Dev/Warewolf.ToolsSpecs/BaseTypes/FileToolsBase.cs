/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Data.PathOperations.Enums;
using Dev2.PathOperations;
using TechTalk.SpecFlow;
using Dev2.Activities.Specs.BaseTypes;

namespace Warewolf.Tools.Specs.BaseTypes
{
    [Binding]
    public class FileToolsBase : RecordSetBases
    {
        private readonly ScenarioContext scenarioContext;

        public FileToolsBase(ScenarioContext scenarioContext)
            : base(scenarioContext)
        {
            if (scenarioContext == null) throw new ArgumentNullException("scenarioContext");
            this.scenarioContext = scenarioContext;
        }

        #region Overrides of RecordSetBases

        protected override void BuildDataList()
        {
        }

        protected void RemovedFilesCreatedForTesting()
        {
            // ReSharper disable EmptyGeneralCatchClause



            var broker = ActivityIOFactory.CreateOperationsBroker();
            string destLocation;
            if (scenarioContext != null && scenarioContext.TryGetValue(CommonSteps.ActualDestinationHolder, out destLocation))
            {
                IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(destLocation,
                    scenarioContext.Get<string>(CommonSteps.DestinationUsernameHolder),
                    scenarioContext.Get<string>(CommonSteps.DestinationPasswordHolder),
                    true);
                IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
                try
                {
                    if (dstEndPoint.PathIs(dstEndPoint.IOPath) == enPathType.File)
                    {
                        broker.Delete(dstEndPoint);
                    }
                }
                catch (Exception)
                {
                //    throw;
                }
            }

            string sourceLocation;
            if (scenarioContext != null && scenarioContext.TryGetValue(CommonSteps.ActualSourceHolder, out sourceLocation))
            {
                if (string.IsNullOrEmpty(sourceLocation))
                {
                    scenarioContext.TryGetValue(CommonSteps.SourceHolder, out sourceLocation);
                }
                if (string.IsNullOrEmpty(sourceLocation))
                {
                    return;
                }
                IActivityIOPath source = ActivityIOFactory.CreatePathFromString(sourceLocation,
                    scenarioContext.Get<string>(CommonSteps.SourceUsernameHolder),
                    scenarioContext.Get<string>(CommonSteps.SourcePasswordHolder),
                    true);
                IActivityIOOperationsEndPoint sourceEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(source);
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
