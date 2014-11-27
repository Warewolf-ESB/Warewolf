
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using Dev2.Data.Interfaces;
using Dev2.ViewModels.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.Workflows
{
    [TestClass]
    public class DesignerDataListUtilsTest
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DesignerDataListUtils_BuildDataPart")]
        public void DesignerDataListUtils_BuildDataPart_ValidScalar_ExpectInsert()
        {
            //------------Setup for test--------------------------
            var unique = new Dictionary<IDataListVerifyPart, string> ();
            WorkflowDesignerDataPartUtils.BuildDataPart("[[bob]]",unique);
            Assert.AreEqual(1,unique.Count);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DesignerDataListUtils_BuildDataPart")]
        public void DesignerDataListUtils_BuildDataPart_ValidRecSet_ExpectInsert()
        {
            //------------Setup for test--------------------------
            var unique = new Dictionary<IDataListVerifyPart, string>();
            WorkflowDesignerDataPartUtils.BuildDataPart("[[bob().a]]", unique);
            Assert.AreEqual(2, unique.Count);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DesignerDataListUtils_BuildDataPart")]
        public void DesignerDataListUtils_BuildDataPart_ValidRecSet_NoField_ExpectInsert()
        {
            //------------Setup for test--------------------------
            var unique = new Dictionary<IDataListVerifyPart, string>();
            WorkflowDesignerDataPartUtils.BuildDataPart("[[bob()]]", unique);
            Assert.AreEqual(1, unique.Count);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DesignerDataListUtils_BuildDataPart")]
        public void DesignerDataListUtils_BuildDataPart_InvalidRecSet_ExpectInsert()
        {
            //------------Setup for test--------------------------
            var unique = new Dictionary<IDataListVerifyPart, string>();
            WorkflowDesignerDataPartUtils.BuildDataPart("[[#$%@]]", unique);
            Assert.AreEqual(0, unique.Count);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DesignerDataListUtils_BuildDataPart")]
        public void DesignerDataListUtils_BuildDataPart_InvalidRecSetName_ExpectInsert()
        {
            //------------Setup for test--------------------------
            var unique = new Dictionary<IDataListVerifyPart, string>();
            WorkflowDesignerDataPartUtils.BuildDataPart("[[rec$().a]]", unique);
            Assert.AreEqual(0, unique.Count);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DesignerDataListUtils_BuildDataPart")]
        public void DesignerDataListUtils_BuildDataPart_InvalidRecSetColumnName_ExpectInsert()
        {
            //------------Setup for test--------------------------
            var unique = new Dictionary<IDataListVerifyPart, string>();
            WorkflowDesignerDataPartUtils.BuildDataPart("[[rec().#a]]", unique);
            Assert.AreEqual(1, unique.Count);
        }

    }
}
