using System;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    public interface ITestCatalog
    {
        void SaveTests(Guid resourceID, List<IServiceTestModelTO> serviceTestModelTos);

        void Load();

        List<IServiceTestModelTO> Fetch(Guid resourceId);

        void DeleteTest(Guid resourceID, string testName);
    }
}