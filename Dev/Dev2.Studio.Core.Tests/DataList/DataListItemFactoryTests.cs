
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
using Dev2.Studio.Core.Factories;
using Dev2.Studio.Core.Interfaces.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Core.Tests.DataList
{
    [TestClass]
    public class DataListItemFactoryTests
    {
        [TestMethod]
        public void CreateDataListModelWithParentAndDisplayNameShouldCorrectlySetName()
        {
            //-------------------------Setup---------------------------------------
            IDataListItemModel parentItem = DataListItemModelFactory.CreateDataListModel("testRec");
            //-------------------------Execute-----------------------------------
            IDataListItemModel childItem = DataListItemModelFactory.CreateDataListModel("field1", "this is a field", parentItem);
            //-------------------------Assert-------------------------------------
            Assert.AreEqual("field1",childItem.Name);
            Assert.AreEqual("testRec", childItem.Parent.Name);
        }
    }
}
