/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Dev2.Data.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.TO
{
    [TestClass]
    public class IntellisenseFilterOpsTOTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(IntellisenseFilterOpsTO))]
        public void IntellisenseFilterOpsTO_SetProperty_IsEqual_SetValue()
        {
            //-----------------------Arrange------------------------
            var intellisenseFilterOpsTO = new IntellisenseFilterOpsTO();
            //-----------------------Act----------------------------
            intellisenseFilterOpsTO.FilterCondition = "TestFilterCondition";
            intellisenseFilterOpsTO.FilterType = enIntellisensePartType.JsonObject;
            //-----------------------Assert-------------------------
            Assert.AreEqual("TestFilterCondition", intellisenseFilterOpsTO.FilterCondition);
            Assert.AreEqual(enIntellisensePartType.JsonObject, intellisenseFilterOpsTO.FilterType);
        }
    }
}
