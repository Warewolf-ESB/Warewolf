
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.ServiceModel;

namespace Dev2
{
    public interface IFrameworkDataChannel
    {
        string ExecuteCommand(string xmlRequest, Guid workspaceID, Guid dataListID);
    }

    public interface IFrameworkWorkspaceChannel : IFrameworkDataChannel
    {

    }

    public interface IFrameworkActivityChannel
    {
        bool ExecuteParallel(IFrameworkActivityInstruction[] instructions);
    }

    public interface IFrameworkActivityInstruction
    {
        string Instruction { get; }
        string Result { get; set; }
        Guid dataListID { get; set; }
    }
}
