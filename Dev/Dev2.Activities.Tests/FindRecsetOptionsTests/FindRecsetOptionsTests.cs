
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.FindRecsetOptionsTests
{
    [TestClass]
    public class FindRecsetOptionsTests
    {
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("FindRecsetOptions_FindAll")]
        public void FindRecsetOptions_FindAll_GetAllRecsetOptions_RightNumberOfOptionsAndCorrectOrder()
        {
            //------------Setup for test--------------------------
            ObservableCollection<string> expected = GlobalConstants.FindRecordsOperations.ToObservableCollection();
            //------------Execute Test---------------------------
            var actual = new ObservableCollection<string>(FindRecsetOptions.FindAll().Select(c => c.HandlesType()));
            //------------Assert Results-------------------------
            CollectionAssert.AreEqual(expected,actual,"The order of the find records drop down is wrong");
        }
    }
}
