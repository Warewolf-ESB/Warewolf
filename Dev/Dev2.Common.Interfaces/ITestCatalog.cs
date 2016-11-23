using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Data;

namespace Dev2.Common.Interfaces
{
    public interface ITestCatalog
    {
        void SaveTests(Guid resourceID, List<IServiceTestModelTO> serviceTestModelTos);
        void Load();
        List<IServiceTestModelTO> Fetch(Guid resourceId);
        void DeleteTest(Guid resourceID, string testName);
        void DeleteAllTests(Guid resourceId);
        void DeleteAllTests(List<string> testsToList);
        IServiceTestModelTO FetchTest(Guid resourceID, string testName);
        void SaveTest(Guid resourceID, IServiceTestModelTO test);
        void UpdateTestsBasedOnIOChange(Guid resourceID, IList<IDev2Definition> inputDefs, IList<IDev2Definition> outputDefs);

        void ReloadAllTests();
    }
}