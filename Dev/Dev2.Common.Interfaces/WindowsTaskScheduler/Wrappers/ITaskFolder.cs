/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Microsoft.Win32.TaskScheduler;

namespace Dev2.Common.Interfaces.WindowsTaskScheduler.Wrappers
{
    public interface ITaskFolder : IWrappedObject<TaskFolder>
    {
        string Name { get; }
        
        string Path { get; }
        
        ITaskCollection Tasks { get; }

        ITaskFolder CreateFolder(string subFolderName);

        ITaskFolder CreateFolder(string subFolderName, string sddlForm);

        void DeleteTask(string Name);

        void DeleteTask(string Name, bool exceptionOnNotExists);
            
        IDev2Task RegisterTaskDefinition(string Path, IDev2TaskDefinition definition);

        IDev2Task RegisterTaskDefinition(string Path, IDev2TaskDefinition definition, TaskCreation createType, string UserId);

        IDev2Task RegisterTaskDefinition(string Path, IDev2TaskDefinition definition, TaskCreation createType, string UserId, 
            string password, TaskLogonType LogonType, string sddl);

        #region additional methods and properties

        IList<IDev2Task> ValidTasks { get; }

        bool TaskExists(string name);

        #endregion
    }
}