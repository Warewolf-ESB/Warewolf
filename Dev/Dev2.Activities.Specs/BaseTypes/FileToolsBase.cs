using System;
using System.Net;
using System.Threading;
using Dev2.Data.PathOperations.Enums;
using Dev2.PathOperations;
using Nuane.Net;
using TechTalk.SpecFlow;

namespace Dev2.Activities.Specs.BaseTypes
{
    [Binding]
    public class FileToolsBase : RecordSetBases
    {
        public static SftpServer Server;
        public static readonly object ServerLock = new object();
        public static int port = 2345;
        public static bool UsesSFTP = false;
        #region Overrides of RecordSetBases

        protected override void BuildDataList()
        {
        }



        protected static void RemovedFilesCreatedForTesting()
        {
            // ReSharper disable EmptyGeneralCatchClause

            var broker = ActivityIOFactory.CreateOperationsBroker();
            string destLocation;
            if(ScenarioContext.Current != null && ScenarioContext.Current.TryGetValue(CommonSteps.ActualDestinationHolder, out destLocation))
            {
                IActivityIOPath dst = ActivityIOFactory.CreatePathFromString(destLocation,
                    ScenarioContext.Current.Get<string>(CommonSteps.DestinationUsernameHolder),
                    ScenarioContext.Current.Get<string>(CommonSteps.DestinationPasswordHolder),
                    true);
                IActivityIOOperationsEndPoint dstEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(dst);
                try
                {
                    if(dstEndPoint.PathIs(dstEndPoint.IOPath) == enPathType.File)
                    {
                        broker.Delete(dstEndPoint);
                    }
                }
                catch(Exception)
                {
                    Thread.Sleep(5000);
                }
            }

            string sourceLocation;
            if(ScenarioContext.Current != null && ScenarioContext.Current.TryGetValue(CommonSteps.ActualSourceHolder, out sourceLocation))
            {
                IActivityIOPath source = ActivityIOFactory.CreatePathFromString(sourceLocation,
                    ScenarioContext.Current.Get<string>(CommonSteps.SourceUsernameHolder),
                    ScenarioContext.Current.Get<string>(CommonSteps.SourcePasswordHolder),
                    true);
                IActivityIOOperationsEndPoint sourceEndPoint = ActivityIOFactory.CreateOperationEndPointFromIOPath(source);
                try
                {
                    if(sourceEndPoint.PathIs(sourceEndPoint.IOPath) == enPathType.File)
                    {
                        broker.Delete(sourceEndPoint);
                    }
                }
                catch(Exception)
                {
                    Thread.Sleep(5000);
                    //The file may already be deleted
                    //
                }
            }

            // SOME SILLY CHICKEN BUNDLED TWO DIS-JOIN OPERATIONS IN THIS METHOD. 
            // THIS CAUSED THE SFTP SERVER TO NEVER SHUTDOWN WHEN THE COMMONSTEPS.ACTUALSOURCEHOLDER KEY WAS NOT PRESENT! 
            // ;)
        }

  
        #endregion
    }
}