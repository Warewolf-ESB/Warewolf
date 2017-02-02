/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("DesignerDataListUtils_BuildDataPart")]
        public void DesignerDataListUtils_BuildDataPart_InvalidJsonObjectVariable_ExpectNotInserted()
        {
            //------------Setup for test--------------------------
            var unique = new Dictionary<IDataListVerifyPart, string>();
            WorkflowDesignerDataPartUtils.BuildDataPart("[[@rec().#a]]", unique, true);
            Assert.AreEqual(0, unique.Count);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("DesignerDataListUtils_BuildDataPart")]
        public void DesignerDataListUtils_BuildDataPart_ValidJsonObjectVariable_ExpectInserted()
        {
            //------------Setup for test--------------------------
            var unique = new Dictionary<IDataListVerifyPart, string>();
            WorkflowDesignerDataPartUtils.BuildDataPart("[[@rec]]", unique, true);
            Assert.AreEqual(1, unique.Count);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("DesignerDataListUtils_BuildDataPart")]
        public void DesignerDataListUtils_BuildDataPart_JsonObjectGivenVariationOfVariables()
        {
            var variables = new List<string>
            {
                //Valid
                "@Var","@Var()","@Var.Field","[[@Rec1(1)]]",
                "@Object()","@Object().Field","@Rec1(*)",
                "@Rec1","@Rec1.Field1","@Object(500)",
                //Invalid
                "@","@.","@()","@().",
                "@.Field","@Object.", "@1",
                "@1.","@1.1","@Rec1.",
                "@Rec1.1","@1Rec","@Rec1.#Field#",
                "@Rec1.1Field", "@Var;iable",
                "@(Rec1@)", "[[@(Rec1@)]]",
                "@(Rec(*))", "@;;;;p"
            };
            //------------Setup for test--------------------------
            var unique = new Dictionary<IDataListVerifyPart, string>();
            foreach(var variable in variables)
                WorkflowDesignerDataPartUtils.BuildDataPart(variable, unique, true);
            Assert.AreEqual(10, unique.Count);
        }
        
        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("DesignerDataListUtils_BuildDataPart")]
        public void DesignerDataListUtils_BuildDataPart_JsonObjectVariableStartsWithNumber_ExpectNotInserted()
        {
            //------------Setup for test--------------------------
            var unique = new Dictionary<IDataListVerifyPart, string>();
            WorkflowDesignerDataPartUtils.BuildDataPart("[[@55rec]]", unique, true);
            Assert.AreEqual(0, unique.Count);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        [TestCategory("DesignerDataListUtils_BuildDataPart")]
        public void DesignerDataListUtils_BuildDataPart_JsonObjectVariableAroundBrackets_ExpectNotInserted()
        {
            //------------Setup for test--------------------------
            var unique = new Dictionary<IDataListVerifyPart, string>();
            WorkflowDesignerDataPartUtils.BuildDataPart("[[@(type())]]", unique, true);
            Assert.AreEqual(0, unique.Count);
        }
    }
}
