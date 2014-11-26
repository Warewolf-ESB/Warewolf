
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Activities;
using System.Diagnostics.CodeAnalysis;
using Dev2.DataList.Contract;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.ESB.Execution;
using Dev2.Utilities;
using Dev2.Workspaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Runtime.ESB
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class WfExecutionContainerTests
    {
        #region Execute

        // BUG 9304 - 2013.05.08 - TWR - .NET 4.5 upgrade
        [TestMethod]
        public void WfExecutionContainerExecuteInvokesWorkflowHelperCompileExpressions()
        {
            var sa = new ServiceAction();
            var dataObj = new Mock<IDSFDataObject>();
            var workspace = new Mock<IWorkspace>();
            var esbChannel = new Mock<IEsbChannel>();

            var wh = new Mock<IWorkflowHelper>();
            wh.Setup(h => h.CompileExpressions(It.IsAny<DynamicActivity>())).Verifiable();

            var exec = new WfExecutionContainer(sa, dataObj.Object, workspace.Object, esbChannel.Object, wh.Object);
            ErrorResultTO errors;
            exec.Execute(out errors);
            wh.Verify(h => h.CompileExpressions(It.IsAny<DynamicActivity>()));
        }

        #endregion
    }
}
