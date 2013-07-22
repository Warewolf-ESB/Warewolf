using Dev2.DataList.Contract;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Services.Execution
{
    public class MockServiceExecutionAbstract<TService, TSource> : ServiceExecutionAbstract<TService, TSource>
        where TService : Service, new() where TSource : Resource, new()
    {
        public MockServiceExecutionAbstract(IDSFDataObject dataObj, bool handlesOutputFormatting = true)
            : base(dataObj, handlesOutputFormatting)
        {
        }

        #region Overrides of ServiceExecutionAbstract<TService,TSource>

        protected override object ExecuteService()
        {
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