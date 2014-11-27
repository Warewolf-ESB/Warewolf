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
using System.Xml.Serialization;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers
{
    public interface IDev2TaskDefinition : IDisposable, IWrappedObject<TaskDefinition>
    {
        ///// <summary>
        ///// Gets a collection of actions that are performed by the task.
        ///// </summary>
        //[XmlArrayItem(ElementName = "Exec", IsNullable = true, Type = typeof (ExecAction))]
        //[XmlArray]
        IActionCollection Actions { get; }

        /// <summary>
        ///     Gets or sets the data that is associated with the task. This data is ignored by the Task Scheduler service, but is
        ///     used by third-parties who wish to extend the task format.
        /// </summary>
        string Data { get; set; }

        /// <summary>
        ///     Gets the settings that define how the Task Scheduler service performs the task.
        /// </summary>
        ITaskSettings Settings { get; }

        /// <summary>
        ///     Gets a collection of triggers that are used to start a task.
        /// </summary>
        //[XmlArrayItem(ElementName = "BootTrigger", IsNullable = true, Type = typeof (BootTrigger))]
        //[XmlArrayItem(ElementName = "CalendarTrigger", IsNullable = true, Type = typeof (CalendarTrigger))]
        //[XmlArrayItem(ElementName = "IdleTrigger", IsNullable = true, Type = typeof (IdleTrigger))]
        //[XmlArrayItem(ElementName = "LogonTrigger", IsNullable = true, Type = typeof (LogonTrigger))]
        //[XmlArrayItem(ElementName = "TimeTrigger", IsNullable = true, Type = typeof (TimeTrigger))]
        //[XmlArray]
        ITriggerCollection Triggers { get; }


        [XmlIgnore]
        string XmlText { get; set; }

        /// <summary>
        ///     added for easier testability.
        /// </summary>
        IAction Action { get; }

        /// <summary>
        ///     Warewolf will only ever create one trigger. so we can reference it
        /// </summary>
        ITrigger Trigger { get; }

        string UserName { get; }

        /// <summary>
        ///     Additional method to check if this is a warewolf task
        /// </summary>
        /// <returns></returns>
        bool IsValidDev2Task();


        void AddAction(IAction action);

        void AddTrigger(ITrigger trigger);
    }
}