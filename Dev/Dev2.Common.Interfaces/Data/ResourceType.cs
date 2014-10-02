
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

namespace Dev2.Common.Interfaces.Data
{
    [Flags]
    public enum ResourceType
    {
        Unknown = 0,
        WorkflowService = 1,
        DbService = 2,
        Version = 3,
        PluginService = 4,
        WebService = 8,
        DbSource = 16,
        PluginSource = 32,
        WebSource = 64,
        EmailSource = 128,
        ServerSource = 256,
        Folder = 512,
        Server = 1024,
        ReservedService = 2048,
        Message = 3069,
    }
}
