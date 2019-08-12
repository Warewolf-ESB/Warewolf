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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Dev2.Common.Interfaces.Queue
{
    public interface IQueueResourceModel : IDisposable
    {

        /// <summary>
        ///     get all the resources that have a Queue.
        /// </summary>
        ObservableCollection<IQueueResource> QueueResources { get; set; }

        /// <summary>
        ///     Get a Queue resource
        /// </summary>
        /// <returns></returns>
        ObservableCollection<IQueueResource> GetQueueResources();


        /// <summary>
        ///     delete a Queue and return the resource
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        void DeleteQueue(IQueueResource resource);

        /// <summary>
        ///     save a Queue and return the resource. the underlying resource behaves like an upsert
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        bool Save(IQueueResource resource, out string errorMessage);

        /// <summary>
        ///     save a Queue and return the resource. the underlying resource behaves like an upsert
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        void Save(IQueueResource resource, string userName, string password);

        /// <summary>
        ///     Create the history for a given task
        /// </summary>
        /// <returns></returns>
        IList<IExecutionHistory> CreateHistory(IQueueResource resource);      
    }
}