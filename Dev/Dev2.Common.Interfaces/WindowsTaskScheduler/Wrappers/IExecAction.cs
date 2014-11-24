/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.ComponentModel;
using System.Xml.Serialization;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers
{
    public interface IExecAction : IAction, IWrappedObject<ExecAction>
    {
        /// <summary>
        ///     Gets or sets the path to an executable file.
        /// </summary>
        [XmlElement("Command")]
        string Path { get; set; }

        /// <summary>
        ///     Gets or sets the arguments associated with the command-line operation.
        /// </summary>
        [DefaultValue("")]
        string Arguments { get; set; }

        /// <summary>
        ///     Gets or sets the directory that contains either the executable file or the files that are used by the executable
        ///     file.
        /// </summary>
        [DefaultValue("")]
        string WorkingDirectory { get; set; }
    }
}