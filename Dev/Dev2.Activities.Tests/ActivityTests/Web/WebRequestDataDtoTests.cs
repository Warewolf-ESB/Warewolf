using Dev2.Activities;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;



namespace Dev2.Tests.Activities.ActivityTests.Web
{
    [TestClass]
    public class WebRequestDataDtoTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateRequestDataDto_GivenMethodGet_ShouldReturnDtoWithMethodGet()
        {
            //---------------Set up test pack-------------------
            var webRequestDataDto = WebRequestDataDto.CreateRequestDataDto(WebRequestMethod.Get, "A", string.Empty);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(webRequestDataDto);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual(webRequestDataDto.WebRequestMethod, WebRequestMethod.Get);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateRequestDataDto_GivenTypeA_ShouldReturnDtoWithTypeA()
        {
            //---------------Set up test pack-------------------
            var webRequestDataDto = WebRequestDataDto.CreateRequestDataDto(WebRequestMethod.Get, "A", string.Empty);
            //---------------Assert Precondition----------------
            Assert.AreEqual(webRequestDataDto.WebRequestMethod, WebRequestMethod.Get);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual(webRequestDataDto.Type.Expression.ToString(), "A");
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateRequestDataDto_GivenDisplayNameB_ShouldReturnDtoWithDisplayNameB()
        {
            //---------------Set up test pack-------------------
            const string displayName = "DisplayNameB";
            var webRequestDataDto = WebRequestDataDto.CreateRequestDataDto(WebRequestMethod.Get, "A", displayName);
            //---------------Assert Precondition----------------
            Assert.AreEqual(webRequestDataDto.Type.Expression.ToString(), "A");
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual(webRequestDataDto.DisplayName, displayName);
        }
    }
}