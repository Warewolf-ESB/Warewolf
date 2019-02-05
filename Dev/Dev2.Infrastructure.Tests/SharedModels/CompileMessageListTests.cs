/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces.Infrastructure.SharedModels;
using Dev2.Data.ServiceModel.Messages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Dev2.Infrastructure.Tests.SharedModels
{
    [TestClass]
    public class CompileMessageListTests
    {
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(CompileMessageList))]
        public void CompileMessageList_Validate_Default()
        {
            var expectedMessageList = new List<ICompileMessageTO>
            {
                new CompileMessageTO()
            };
            var expectedServiceId = Guid.NewGuid();
            var expectedDependants = new List<string> { "test" };

            var compileMessageList = new CompileMessageList
            {
                MessageList = expectedMessageList,
                ServiceID = expectedServiceId,
                Dependants = expectedDependants
            };

            Assert.AreEqual(expectedMessageList, compileMessageList.MessageList);
            Assert.AreEqual(1, compileMessageList.MessageList.Count);
            Assert.AreEqual(1, compileMessageList.Count);
            Assert.AreEqual(expectedServiceId, compileMessageList.ServiceID);
            Assert.AreEqual(expectedDependants, compileMessageList.Dependants);
            Assert.AreEqual(1, compileMessageList.Dependants.Count);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(CompileMessageList))]
        public void CompileMessageList_Count_Expected_Zero()
        {
            var compileMessageList = new CompileMessageList
            {
                MessageList = null
            };

            Assert.AreEqual(0, compileMessageList.Count);
        }
    }
}
