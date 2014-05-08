using System.Activities;
using Dev2.Enums;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.Workflows
{
    public class TestActivity : Activity, IDev2Activity
    {
        public string UniqueID { get; set; }

        public enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.StaticActivity;
        }
    }

    public class TestDecisionActivity : Activity<bool>, IDev2Activity
    {
        public string UniqueID { get; set; }

        public enFindMissingType GetFindMissingType()
        {
            return enFindMissingType.StaticActivity;
        }
    }
}