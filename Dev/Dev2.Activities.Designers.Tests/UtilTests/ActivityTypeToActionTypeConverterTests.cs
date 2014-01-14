using Dev2.Activities.Utils;
using Dev2.DynamicServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Activities.Designers.Tests.UtilTests
{
    [TestClass]
    public class ActivityTypeToActionTypeConverterTests
    {
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ActivityTypeToActionTypeConverter_ConvertToActionType")]
        public void ActivityTypeToActionTypeConverter_ConvertToActionType_ConvertWorkflow_ExpectedWorkflowEnum()
        {
            //------------Execute Test---------------------------
            enActionType actionType = ActivityTypeToActionTypeConverter.ConvertToActionType("Workflow");
            //------------Assert Results-------------------------
            Assert.AreEqual(enActionType.Workflow, actionType);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ActivityTypeToActionTypeConverter_ConvertToActionType")]
        public void ActivityTypeToActionTypeConverter_ConvertToActionType_ConvertWebService_ExpectedWebServiceEnum()
        {
            //------------Execute Test---------------------------
            enActionType actionType = ActivityTypeToActionTypeConverter.ConvertToActionType("WebService");
            //------------Assert Results-------------------------
            Assert.AreEqual(enActionType.InvokeWebService, actionType);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ActivityTypeToActionTypeConverter_ConvertToActionType")]
        public void ActivityTypeToActionTypeConverter_ConvertToActionType_ConvertPluginService_ExpectedPluginServiceEnum()
        {
            //------------Execute Test---------------------------
            enActionType actionType = ActivityTypeToActionTypeConverter.ConvertToActionType("PluginService");
            //------------Assert Results-------------------------
            Assert.AreEqual(enActionType.Plugin, actionType);
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ActivityTypeToActionTypeConverter_ConvertToActionType")]
        public void ActivityTypeToActionTypeConverter_ConvertToActionType_ConvertDbService_ExpectedDbServiceEnum()
        {
            //------------Execute Test---------------------------
            enActionType actionType = ActivityTypeToActionTypeConverter.ConvertToActionType("DbService");
            //------------Assert Results-------------------------
            Assert.AreEqual(enActionType.InvokeStoredProc, actionType);
        }
    }
}
