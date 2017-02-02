/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Core.Graph;
using Dev2.Data.TO;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Services.Execution
{
    public class MockServiceExecutionAbstract<TService, TSource> : ServiceExecutionAbstract<TService, TSource>
        where TService : Service, new()
        where TSource : Resource, new()
    {

        public bool DidExecuteServiceInvoke { get; private set; }
        public string ReturnFromExecute { get; set; }
        public MockServiceExecutionAbstract(IDSFDataObject dataObj, bool handlesOutputFormatting = false)
            : base(dataObj, handlesOutputFormatting)
        {
        }

        #region Overrides of ServiceExecutionAbstract<TService,TSource>

        public override void BeforeExecution(ErrorResultTO errors)
        {
        }

        public override void AfterExecution(ErrorResultTO errors)
        {
        }

        protected override object ExecuteService(int update, out ErrorResultTO errors, IOutputFormatter formater = null)
        {
            errors = new ErrorResultTO();
            DidExecuteServiceInvoke = true;
            return ReturnFromExecute;
        }

        #endregion

        #region Exposed Functions

        #region Overrides of ServiceExecutionAbstract<TService,TSource>

        #endregion

        public void MockExecuteImpl(out ErrorResultTO errors)
        {
            ExecuteImpl( out errors, 0);
        }

        #endregion
    }
}
