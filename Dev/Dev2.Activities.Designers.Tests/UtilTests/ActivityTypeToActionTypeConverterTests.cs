
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Activities.Utils;
using Dev2.Common.Interfaces.Core.DynamicServices;
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
