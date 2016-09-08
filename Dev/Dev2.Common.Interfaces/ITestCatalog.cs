using System;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    public interface ITestCatalog
    {
        void SaveTests(Guid resourceID, List<IServiceTestModel> serviceTestModelTos);
    }
}