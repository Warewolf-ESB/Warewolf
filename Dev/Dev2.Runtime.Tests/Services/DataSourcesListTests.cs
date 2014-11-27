
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Runtime.Services;

namespace Dev2.Tests.Runtime.Dev2.Runtime.Services.Tests
{
    [TestClass]
    public class DataSourcesListTests
    {
        [TestMethod]
        public void List_Expected_ReturnedList()
        {
            var getList = new DataSources();
            Assert.AreNotEqual(getList.list("").Count, 0);
        }
    }
}
