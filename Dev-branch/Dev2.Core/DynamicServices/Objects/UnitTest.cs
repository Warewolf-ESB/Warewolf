using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DynamicServices {
    public class UnitTest : DynamicServiceObjectBase {
        public UnitTest() : base(enDynamicServiceObjectType.UnitTest) {

        }

        public string ServiceName { get; set; }
        public string InputXml { get; set; }
        public string RequiredTagName { get; set; }
        public string ValidationExpression { get; set; }

//        <UnitTest Name="" ServiceName="">
//    <InputXml>
		
//    </InputXml>
//    <Assert RequiredTagNames="" ValidationExpression="" />
//</UnitTest>

    }
}
