
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime.ServiceModel.Esb
{
    /// <summary>
    /// Represents a contract for an ESB endpoint whose methods are executable.
    /// </summary>
    public interface IExecutableEsbEnpoint
    {
        IOutputDescription TestServiceMethod(Resource resource, ServiceMethod serviceMethod);
        Guid ExecuteServiceMethod(Resource resource, ServiceMethod serviceMethod);
    }
}
