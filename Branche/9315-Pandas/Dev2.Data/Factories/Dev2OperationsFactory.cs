
using Dev2.Data.Interfaces;

namespace Dev2.Data.Operations
{
    public static class Dev2OperationsFactory
    {
        public static IDev2ReplaceOperation CreateReplaceOperation()
        {
            return new Dev2ReplaceOperation();
        }
    }
}
