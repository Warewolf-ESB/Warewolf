
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using System.Collections.Generic;
using System.Globalization;
using Dev2.CustomControls.Converters;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Core.Tests.ConverterTests
{
    [TestClass]
    public class RowToIndexConverterTests
    {
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RowToIndexConverter_Convert")]
        public void RowToIndexConverter_Convert_FindsValue_ReturnsIndex()
        {
            //------------Setup for test--------------------------
            var converter = new RowToIndexConverter();

            var activityDtos = new List<ActivityDTO> { new ActivityDTO("name", "value", 0), new ActivityDTO("name1", "value1", 1), new ActivityDTO("name2", "value2", 2) };

            DsfMultiAssignActivity multiAssign = new DsfMultiAssignActivity();
            multiAssign.FieldsCollection = activityDtos;

            dynamic modelItem = ModelItemUtils.CreateModelItem(multiAssign);

            ModelItemCollection collection = modelItem.FieldsCollection as ModelItemCollection;
            //------------Execute Test---------------------------
            if(collection != null)
            {
                var result = converter.Convert(collection[1], typeof(int), null, CultureInfo.CurrentCulture);

                //------------Assert Results-------------------------
                Assert.AreEqual(2, result);
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("RowToIndexConverter_Convert")]
        public void RowToIndexConverter_Convert_DoesntFindValue_ReturnsMinusOne()
        {
            var converter = new RowToIndexConverter();

            var activityDtos = new List<ActivityDTO> { new ActivityDTO("name", "value", 0), new ActivityDTO("name1", "value1", 1), new ActivityDTO("name2", "value2", 2) };

            DsfMultiAssignActivity multiAssign = new DsfMultiAssignActivity();
            multiAssign.FieldsCollection = activityDtos;
            ModelItem modelItemThatdoesntExist = ModelItemUtils.CreateModelItem(new ActivityDTO("thing", "stuff", 8));

            dynamic modelItem = ModelItemUtils.CreateModelItem(multiAssign);

            ModelItemCollection collection = modelItem.FieldsCollection as ModelItemCollection;
            //------------Execute Test---------------------------
            if(collection != null)
            {
                var result = converter.Convert(modelItemThatdoesntExist, typeof(int), null, CultureInfo.CurrentCulture);

                //------------Assert Results-------------------------
                Assert.AreEqual(-1, result);
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}
