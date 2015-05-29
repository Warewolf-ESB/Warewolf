using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Core.Test
{
    [TestClass]
    public class WebserviceDefinitionTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WebServiceDefinition_Ctor")]
        public void WebServiceDefinition_Ctor_ExpectEmptyValues()
        {
            //------------Setup for test--------------------------
            var webServiceDefinition = new WebServiceDefinition(){Headers = new List<NameValue>{new NameValue{Name = "a",Value = "b"}}, Name = "bob" , PostData = "post", Path = "xsd"};
          
            

            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
        }
    }
}
