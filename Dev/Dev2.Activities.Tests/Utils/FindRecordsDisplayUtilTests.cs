
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Text;
using System.Collections.Generic;
using Dev2.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.Utils
{
    /// <summary>
    /// Summary description for FindRecordsMigrationUtilTests
    /// </summary>
    [TestClass]
    public class FindRecordsDisplayUtilTests
    {
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("FindRecordsMigrationUtil_ConvertForDisplay")]
        public void FindRecordsMigrationUtil_ConvertForDisplay_TryAllOptions_CorrectStringsReturned()
        {
            Assert.AreEqual("=", FindRecordsDisplayUtil.ConvertForDisplay("Equals"));
            Assert.AreEqual("<> (Not Equal)", FindRecordsDisplayUtil.ConvertForDisplay("Not Equals"));
            Assert.AreEqual(">=", FindRecordsDisplayUtil.ConvertForDisplay(">="));
            Assert.AreEqual("<=", FindRecordsDisplayUtil.ConvertForDisplay("<="));
            Assert.AreEqual("Doesn't Contain", FindRecordsDisplayUtil.ConvertForDisplay("Not Contains"));
            Assert.AreEqual("Is Regex", FindRecordsDisplayUtil.ConvertForDisplay("Regex"));
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("FindRecordsMigrationUtil_ConvertForWriting")]
        public void FindRecordsMigrationUtil_ConvertForWriting_TryAllOptions_CorrectStringsReturned()
        {
            Assert.AreEqual("Equals", FindRecordsDisplayUtil.ConvertForWriting("="));
            Assert.AreEqual("Not Equals", FindRecordsDisplayUtil.ConvertForWriting("<> (Not Equal)"));
            Assert.AreEqual(">=", FindRecordsDisplayUtil.ConvertForWriting(">="));
            Assert.AreEqual("<=", FindRecordsDisplayUtil.ConvertForWriting("<="));
            Assert.AreEqual("Not Contains", FindRecordsDisplayUtil.ConvertForWriting("Doesn't Contain"));
            Assert.AreEqual("Regex", FindRecordsDisplayUtil.ConvertForWriting("Is Regex"));
        }
    }
}
