/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/
using Dev2.Common.Interfaces.Search;
using Dev2.Common.Search;
using Dev2.Data;
using Dev2.Data.SystemTemplates.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Warewolf.Security.Encryption;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class CommonEqualityOpsTests
    {
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(CommonEqualityOps))]
        public void CommonEqualityOps_AreObjectsEqualUnSafe_Objects_ReturnTrue()
        {
            string str = default(string);
            string strB = default(string);
            var handlerActivityIsEqual = CommonEqualityOps.AreObjectsEqualUnSafe(str, strB);

            Assert.IsTrue(handlerActivityIsEqual);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(CommonEqualityOps))]
        public void CommonEqualityOps_AreObjectsEqualUnSafe_Objects_ReturnFalse()
        {
            string str = default(string);
            string strB ="asd";
            var handlerActivityIsEqual = CommonEqualityOps.AreObjectsEqualUnSafe(str, strB);

            Assert.IsFalse(handlerActivityIsEqual);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(CommonEqualityOps))]
        public void CommonEqualityOps_AreObjectsEqual_Objects_ReturnTrue()
        {
            string str = default(string);
            string strB = default(string);
            var handlerActivityIsEqual = CommonEqualityOps.AreObjectsEqual(str, strB);

            Assert.IsTrue(handlerActivityIsEqual);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(CommonEqualityOps))]
        public void CommonEqualityOps_AreObjectsEqual_Objects_ReturnFalse()
        {
            string str = default(string);
            string strB = "asd";
            var handlerActivityIsEqual = CommonEqualityOps.AreObjectsEqual(str, strB);

            Assert.IsFalse(handlerActivityIsEqual);
        }
        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(CommonEqualityOps))]
        public void CommonEqualityOps_AreObjectsEqual_ReturnTrue()
        {
            var _name = "workflowName";
            var _path = "resourcePath";
            var _match = "Input";
            var searchVal = new SearchResult(Guid.NewGuid(), _name, _path, SearchItemType.WorkflowName, _match);
            var otherSearchVal = new SearchResult(Guid.NewGuid(), _name, _path, SearchItemType.WorkflowName, _match);

            var areConditionsEqual = CommonEqualityOps.AreObjectsEqual(searchVal.Name, otherSearchVal.Name);
            Assert.IsTrue(areConditionsEqual);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(CommonEqualityOps))]
        public void CommonEqualityOps_AreObjectsEqual_ReturnFalse()
        {
            var _name = "workflowName";
            var _path = "resourcePath";
            var _match = "Input";
            var searchVal = new SearchResult(Guid.NewGuid(), _name, _path, SearchItemType.WorkflowName, _match);
            var otherSearchVal = new SearchResult(Guid.NewGuid(), _name, _path, SearchItemType.WorkflowName, _match);
            otherSearchVal.Name = "new name";
            var areConditionsEqual = CommonEqualityOps.AreObjectsEqual(searchVal.Name, otherSearchVal.Name);
            Assert.IsFalse(areConditionsEqual);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(CommonEqualityOps))]
        public void CommonEqualityOps_AreObjectsEqualUnSafe_ReturnTrue()
        {
            var _name = "workflowName";
            var _path = "resourcePath";
            var _match = "Input";
            var searchVal = new SearchResult(Guid.NewGuid(), _name, _path, SearchItemType.WorkflowName, _match);
            var otherSearchVal = new SearchResult(Guid.NewGuid(), _name, _path, SearchItemType.WorkflowName, _match);

            var areConditionsEqual = CommonEqualityOps.AreObjectsEqualUnSafe(searchVal.Name, otherSearchVal.Name);
            Assert.IsTrue(areConditionsEqual);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(CommonEqualityOps))]
        public void CommonEqualityOps_AreObjectsEqualUnSafe_ReturnFalse()
        {
            var _name = "workflowName";
            var _path = "resourcePath";
            var _match = "Input";
            var searchVal = new SearchResult(Guid.NewGuid(), _name, _path, SearchItemType.WorkflowName, _match);
            var otherSearchVal = new SearchResult(Guid.NewGuid(), _name, _path, SearchItemType.WorkflowName, _match);
            otherSearchVal.Name = "new name";
            var areConditionsEqual = CommonEqualityOps.AreObjectsEqualUnSafe(searchVal.Name, otherSearchVal.Name);
            Assert.IsFalse(areConditionsEqual);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(CommonEqualityOps))]
        public void CommonEqualityOps_PassWordsCompare_ReturnFalse()
        {
            var areConditionsEqual = CommonEqualityOps.PassWordsCompare("123", "1233");
            Assert.IsFalse(areConditionsEqual);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(CommonEqualityOps))]
        public void CommonEqualityOps_PassWordsCompare_ReturnTrue()
        {
            var areConditionsEqual = CommonEqualityOps.PassWordsCompare(SecurityEncryption.Encrypt("123"), SecurityEncryption.Encrypt("123"));
            Assert.IsTrue(areConditionsEqual);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(CommonEqualityOps))]
        public void CommonEqualityOps_CollectionEquals_ReturnTrue()
        {
            var TheStack = new List<Dev2Decision>();
            var dev2Decision = new Dev2Decision { Col1 = "Col1" };
            TheStack.Add(dev2Decision);

            var collectionEquals = CommonEqualityOps.CollectionEquals(TheStack, TheStack, new Dev2DecisionComparer());

            Assert.IsTrue(collectionEquals);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(CommonEqualityOps))]
        public void CommonEqualityOps_CollectionEquals_ReturnFalse()
        {
            var TheStack = new List<Dev2Decision>();
            var dev2Decision = new Dev2Decision { Col1 = "Col1" };
            TheStack.Add(dev2Decision);

            var TheOtherStack = new List<Dev2Decision>();
            var dev2DecisionOther = new Dev2Decision { Col1 = "Col2" };
            TheOtherStack.Add(dev2DecisionOther);

            var collectionEquals = CommonEqualityOps.CollectionEquals(TheStack, TheOtherStack, new Dev2DecisionComparer());

            Assert.IsFalse(collectionEquals);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(CommonEqualityOps))]
        public void CommonEqualityOps_CollectionEquals__BothObjectsareNull_ReturnTrue()
        {
            var TheStack = new List<Dev2Decision>();
            TheStack = null;
            var TheOtherStack = new List<Dev2Decision>();
            TheOtherStack = null;

            var collectionEquals = CommonEqualityOps.CollectionEquals(TheStack, TheOtherStack, new Dev2DecisionComparer());

            Assert.IsTrue(collectionEquals);
        }

        [TestMethod]
        [Owner("Candice Daniel")]
        [TestCategory(nameof(CommonEqualityOps))]
        public void CommonEqualityOps_CollectionEquals__OneObjectsIsNull_ReturnFalse()
        {
            var TheStack = new List<Dev2Decision>();
            var dev2Decision = new Dev2Decision { Col1 = "Col1" };
            TheStack.Add(dev2Decision);

            var TheOtherStack = new List<Dev2Decision>();
            TheOtherStack = null;

            var collectionEquals = CommonEqualityOps.CollectionEquals(TheStack, TheOtherStack, new Dev2DecisionComparer());
            Assert.IsFalse(collectionEquals);

            collectionEquals = CommonEqualityOps.CollectionEquals(TheOtherStack, TheStack, new Dev2DecisionComparer());
            Assert.IsFalse(collectionEquals);
        }
    }
}
