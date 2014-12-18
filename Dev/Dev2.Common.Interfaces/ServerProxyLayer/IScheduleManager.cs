using System.Collections.Generic;
using Dev2.Common.Interfaces.Scheduler.Interfaces;

namespace Dev2.Common.Interfaces.ServerProxyLayer
{
    /// <summary>
    /// interface for managing schedules. Acts as a wrapper over the windows task scheduler
    /// </summary>
    public interface IScheduleManager
    {
        /// <summary>
        /// Save a scheduler to the server
        /// </summary>
        /// <param name="scheduledResource">the schedule to save</param>
        /// <param name="userName">the user that the schedule will run as</param>
        /// <param name="password">the users windows password</param>
        void SaveSchedule(IScheduledResource scheduledResource,string userName,string password);
        /// <summary>
        /// get the list of schedules available on the server
        /// </summary>
        /// <returns></returns>
        IList<IScheduledResource> FetchSchedules();

        /// <summary>
        /// get the history of schedule runs for this schedule
        /// </summary>
        /// <param name="scheduleName">the schedule name</param>
        /// <param name="schedulePath"></param>
        /// <returns></returns>
        IList<IResourceHistory> FetchScheduleHistory(string scheduleName,string schedulePath);

        /// <summary>
        /// delete a schedule from a warewolf server
        /// </summary>
        /// <param name="scheduleName"></param>
        /// <param name="schedulePath"></param>
        void Delete(string scheduleName,string schedulePath);

    }
}