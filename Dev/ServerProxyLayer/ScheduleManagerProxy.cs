using System.Collections.Generic;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Common.Interfaces.ServerProxyLayer;
using Dev2.Common.Interfaces.Studio.Core;
using Dev2.Common.Interfaces.Studio.Core.Controller;
using Dev2.Communication;

namespace Warewolf.Studio.ServerProxyLayer
{
    public class ScheduleManagerProxy : ProxyBase, IScheduleManager  
    {
        #region Implementation of IScheduleManager

        


        public ScheduleManagerProxy(ICommunicationControllerFactory communicationControllerFactory, IEnvironmentConnection connection):base(communicationControllerFactory,connection)
        {
            
        }
        /// <summary>
        /// Save a scheduler to the server
        /// </summary>
        /// <param name="scheduledResource">the schedule to save</param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public void SaveSchedule(IScheduledResource scheduledResource,string userName,string password)
        {
            Dev2JsonSerializer jsonSerializer = new Dev2JsonSerializer();
            var builder = jsonSerializer.SerializeToBuilder(scheduledResource);
            var controller = CommunicationControllerFactory.CreateController("SaveScheduledResourceService");
            controller.AddPayloadArgument("Resource", builder);
            controller.AddPayloadArgument("UserName", userName);
            controller.AddPayloadArgument("Password", password);
            controller.ExecuteCommand<string>(Connection, Connection.WorkspaceID);

        }

        /// <summary>
        /// get the list of schedules available on the server
        /// </summary>
        /// <returns></returns>
        public IList<IScheduledResource> FetchSchedules()
        {
            var controller = CommunicationControllerFactory.CreateController("GetScheduledResources");
            return controller.ExecuteCommand<IList<IScheduledResource>>(Connection, Connection.WorkspaceID);

        }

        /// <summary>
        /// get the history of schedule runs for this schedule
        /// </summary>
        /// <param name="scheduleName">the schedule name</param>
        /// <param name="schedulePath"></param>
        /// <returns></returns>
        public IList<IResourceHistory> FetchScheduleHistory(string scheduleName,string schedulePath)
        {
            
            var controller = CommunicationControllerFactory.CreateController("GetScheduledResourceHistoryService");
            controller.AddPayloadArgument("ScheduleName", scheduleName);
            controller.AddPayloadArgument("SchedulePath", schedulePath);
            return controller.ExecuteCommand<IList<IResourceHistory>>(Connection, Connection.WorkspaceID);
        }

        /// <summary>
        /// delete a schedule from a warewolf server
        /// </summary>
        /// <param name="scheduleName"></param>
        /// <param name="schedulePath"></param>
        public void Delete(string scheduleName,string schedulePath)
        {

            var controller = CommunicationControllerFactory.CreateController("DeleteScheduledResourceService");
            controller.AddPayloadArgument("ScheduleName", scheduleName);
            controller.AddPayloadArgument("SchedulePath", schedulePath);
            controller.ExecuteCommand<string>(Connection, Connection.WorkspaceID);
        }

        #endregion
    }
}
