/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Data.TO;
using Dev2.Common.Interfaces.Scheduler.Interfaces;
using Dev2.Data.TO;
using System;
namespace Dev2.Core.Tests.Triggers
{
    class ScheduledResourceForTest : IScheduledResource
    {
        bool _isNewItem;
        bool _isDirty;

        public ScheduledResourceForTest()
        {
            Errors = new ErrorResultTO();
        }

        /// <summary>
        /// Property to check if the scheduled resouce is saved
        /// </summary>
        public bool IsDirty
        {
            get
            {
                return _isDirty;
            }
            set
            {
                _isDirty = value;
            }
        }
        /// <summary>
        ///     Schedule Name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Represents the old name of the task
        /// </summary>
        public string OldName { get; set; }
        /// <summary>
        ///     Schedule Status
        /// </summary>
        public SchedulerStatus Status { get; set; }
        /// <summary>
        ///     The next time that this schedule will run
        /// </summary>
        public DateTime NextRunDate { get; set; }
        /// <summary>
        ///     Trigger
        /// </summary>
        public IScheduleTrigger Trigger { get; set; }
        /// <summary>
        /// NumberOfHistoryToKeep
        /// </summary>
        public int NumberOfHistoryToKeep { get; set; }
        /// <summary>
        /// The workflow that we will run
        /// </summary>
        public string WorkflowName { get; set; }


        /// <summary>
        /// The workflow that we will run
        /// </summary>
        public Guid ResourceId { get; set; }
        /// <summary>
        /// If a schedule is missed execute as soon as possible
        /// </summary>
        public bool RunAsapIfScheduleMissed { get; set; }
        public bool AllowMultipleIstances { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public IErrorResultTO Errors { get; set; }
        public bool IsNew { get; set; }
        public bool IsNewItem
        {
            get
            {
                return _isNewItem;
            }
            set
            {
                _isNewItem = value;
            }
        }
        public string NameForDisplay { get; private set; }

        public void SetItem(IScheduledResource item)
        {
        }

        public bool Equals(IScheduledResource other)
        {
            return !IsDirty;
        }
    }
}
