
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common.Interfaces.Core.Graph;
using Dev2.DataList.Contract;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Services.Execution
{
    public class MockServiceExecutionAbstract<TService, TSource> : ServiceExecutionAbstract<TService, TSource>
        where TService : Service, new()
        where TSource : Resource, new()
    {

        public bool DidExecuteServiceInvoke { get; private set; }

        public MockServiceExecutionAbstract(IDSFDataObject dataObj, bool handlesOutputFormatting = false)
            : base(dataObj, handlesOutputFormatting, false)
        {
        }

        #region Overrides of ServiceExecutionAbstract<TService,TSource>

        public override void BeforeExecution(ErrorResultTO errors)
        {
        }

        public override void AfterExecution(ErrorResultTO errors)
        {
        }

        protected override object ExecuteService(out ErrorResultTO errors, IOutputFormatter formater = null)
        {
            errors = new ErrorResultTO();
            DidExecuteServiceInvoke = true;
            return null;
        }

        #endregion

        #region Exposed Functions

        public void MockCreateService(ResourceCatalog catalog)
        {
            CreateService(catalog);
        }

        public void MockExecuteImpl(IDataListCompiler compiler, out DataList.Contract.ErrorResultTO errors)
        {
            ExecuteImpl(compiler, out errors);
        }

        #endregion
    }
}
