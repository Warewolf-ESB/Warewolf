
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dev2.Common.Interfaces.Scheduler.Interfaces
{
    public interface IScheduledResourceModel:IDisposable
    {

        #region Studio
        /// <summary>
        /// get all the resources that have a schedule. This will be a join between whatever is in windows scheduler and the list of resources
        /// </summary>
        ObservableCollection<IScheduledResource> ScheduledResources { get; set; }

        /// <summary>
        /// Get a scheduled resource
        /// </summary>

        /// <returns></returns>
        ObservableCollection<IScheduledResource> GetScheduledResources();


        /// <summary>
        /// delete a schedule and return the resource
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        void DeleteSchedule(IScheduledResource resource);

        /// <summary>
        /// save a schedule and return the resource. the underlying resource behaves like an upsert
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        bool Save(IScheduledResource resource, out string errorMessage);

        /// <summary>
        /// save a schedule and return the resource. the underlying resource behaves like an upsert
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        void Save(IScheduledResource resource, string userName, string password);

        /// <summary>
        /// Create the history for a given task
        /// </summary>

        /// <returns></returns>
        IList<IResourceHistory> CreateHistory(IScheduledResource resource);

        #endregion
    }
}
