
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

namespace Dev2.Runtime.Execution
{
    public interface IExecutableService
    {
        Guid ID { get; set; }
        Guid WorkspaceID { get; set; }
        IList<IExecutableService> AssociatedServices { get; }
        Guid ParentID { get; set; }

        void Run();
        void Terminate();
        void Resume(IDSFDataObject dataObject);
        void Terminate(Exception exception);
    }
}
