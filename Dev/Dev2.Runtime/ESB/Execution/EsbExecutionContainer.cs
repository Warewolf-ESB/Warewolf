
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
using Dev2.Communication;
using Dev2.DataList.Contract;
using Dev2.DynamicServices.Objects;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Execution
{
    /// <summary>
    /// Wrapper class for all executable types in our ESB
    /// </summary>
    public abstract class EsbExecutionContainer
    {
        protected ServiceAction ServiceAction { get; private set; }
        protected IDSFDataObject DataObject { get; private set; }
        protected IWorkspace TheWorkspace { get; private set; }
        protected IEsbChannel EsbChannel { get; private set; }
        protected EsbExecuteRequest Request { get; private set; }

        public String InstanceOutputDefinition { get; set; }

        protected EsbExecutionContainer(ServiceAction sa, IDSFDataObject dataObject, IWorkspace theWorkspace, IEsbChannel esbChannel)
            : this(sa, dataObject, theWorkspace, esbChannel, null)
        {
        }

        protected EsbExecutionContainer(ServiceAction sa, IDSFDataObject dataObject, IWorkspace theWorkspace, IEsbChannel esbChannel, EsbExecuteRequest request)
        {
            ServiceAction = sa;
            DataObject = dataObject;
            TheWorkspace = theWorkspace;
            EsbChannel = esbChannel;
            Request = request;
        }

        protected EsbExecutionContainer()
        {
        }

        public abstract Guid Execute(out ErrorResultTO errors);
    }
}
