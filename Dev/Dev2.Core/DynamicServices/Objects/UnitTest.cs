using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.DynamicServices.Objects.Base;

namespace Dev2.DynamicServices {
    public class UnitTest : DynamicServiceObjectBase {
        public UnitTest() : base(enDynamicServiceObjectType.UnitTest) {

        }

        public string ServiceName { get; set; }
        public string InputXml { get; set; }
        public string RequiredTagName { get; set; }
        public string ValidationExpression { get; set; }

    }
}
