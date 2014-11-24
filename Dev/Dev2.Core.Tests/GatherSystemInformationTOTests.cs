
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Diagnostics.CodeAnalysis;
using Dev2.Data.Enums;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class GatherSystemInformationTOTests
    {
        [TestMethod]
        public void GatherSystemInformationTOShouldImplementIDev2TOFn()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            GatherSystemInformationTO informationTO = new GatherSystemInformationTO();
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(informationTO,typeof(IDev2TOFn));
        }

        [TestMethod]
        public void ConstructorWhereParametersExpectSetsProperties()
        {
            //------------Setup for test--------------------------
            const string ResultVariable = "[[Output]]";
            var toGather = enTypeOfSystemInformationToGather.OperatingSystem;
            const int IndexNumber = 0;
            //------------Execute Test---------------------------
            var gatherSystemInformationTO = new GatherSystemInformationTO(toGather,ResultVariable,IndexNumber);
            //------------Assert Results-------------------------
            Assert.IsNotNull(gatherSystemInformationTO);
            Assert.AreEqual(ResultVariable,gatherSystemInformationTO.Result);
            Assert.AreEqual(toGather,gatherSystemInformationTO.EnTypeOfSystemInformation);
            Assert.AreEqual(IndexNumber,gatherSystemInformationTO.IndexNumber);

        }

        #region CanAdd Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("GatherSystemInformationTO_CanAdd")]
        public void GatherSystemInformationTO_CanAdd_ResultEmpty_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var gatherSystemInformationTO = new GatherSystemInformationTO { Result = string.Empty };
            //------------Assert Results-------------------------
            Assert.IsFalse(gatherSystemInformationTO.CanAdd());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("GatherSystemInformationTO_CanAdd")]
        public void GatherSystemInformationTO_CanAdd_ResultHasData_ReturnTrue()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var gatherSystemInformationTO = new GatherSystemInformationTO { Result = "Value" };
            //------------Assert Results-------------------------
            Assert.IsTrue(gatherSystemInformationTO.CanAdd());
        }

        #endregion

        #region CanRemove Tests

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("GatherSystemInformationTO_CanRemove")]
        public void GatherSystemInformationTO_CanRemove_ResultEmpty_ReturnTrue()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var gatherSystemInformationTO = new GatherSystemInformationTO { Result = string.Empty };
            //------------Assert Results-------------------------
            Assert.IsTrue(gatherSystemInformationTO.CanRemove());
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("GatherSystemInformationTO_CanRemove")]
        public void GatherSystemInformationTO_CanRemove_ResultWithData_ReturnFalse()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var gatherSystemInformationTO = new GatherSystemInformationTO { Result = "Value" };
            //------------Assert Results-------------------------
            Assert.IsFalse(gatherSystemInformationTO.CanRemove());
        }

        #endregion
    }

  
}
