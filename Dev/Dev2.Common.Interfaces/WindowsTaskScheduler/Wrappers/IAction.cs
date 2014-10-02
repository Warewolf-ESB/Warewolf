
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
using System.ComponentModel;
using System.Xml.Serialization;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers
{
    public interface IAction : IDisposable, IWrappedObject<Microsoft.Win32.TaskScheduler.Action>
    {
        /// <summary>
        ///     Gets the type of the action.
        /// </summary>
        /// <value>The type of the action.</value>
        [XmlIgnore]
        TaskActionType ActionType { get; }

        /// <summary>
        ///     Gets or sets the identifier of the action.
        /// </summary>
        [DefaultValue(null)]
        [XmlAttribute(AttributeName = "id", DataType = "ID")]
        string Id { get; set; }
    }
}
