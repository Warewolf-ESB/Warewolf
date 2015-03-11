
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable InconsistentNaming
namespace Dev2.Data.Tests.BinaryDataList.Converters
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DataListXMLTranslatorWithOutSystemTagsTest
    {
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListXMLTranslatorWithOutSystemTags_ConvertFrom")]
        [Description("Created to address bug with 10229 found in merge process @ integration level")]
        public void DataListXMLTranslatorWithOutSystemTags_ConvertFrom_WhereIndexOneDeleted_AllItemsButIndexOne()
        {
            //------------Setup for test--------------------------
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            var format = DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags);
            StringBuilder data = new StringBuilder("<root><person><fname>bob</fname><lname>smith</lname></person><person><fname>sara</fname><lname>jones</lname></person></root>");
            StringBuilder shape = new StringBuilder("<root><person><fname/><lname/></person></root>");
            var dlID = compiler.ConvertTo(format, data, shape, out errors);

            var bdl = compiler.FetchBinaryDataList(dlID, out errors);

            string error;
            IBinaryDataListEntry entry;
            bdl.TryGetEntry("person", out entry, out error);
            entry.TryDeleteRows("1", out error);

            //------------Execute Test---------------------------

            var result = compiler.ConvertFrom(dlID, format, enTranslationDepth.Data, out errors);

            //------------Assert Results-------------------------
            const string expected = "<DataList><person><fname>sara</fname><lname>jones</lname></person></DataList>";

            StringAssert.Contains(result.ToString(), expected);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListXMLTranslatorWithOutSystemTags_ConvertAndOnlyMapInputs")]
        public void DataListXMLTranslatorWithOutSystemTags_ConvertAndOnlyMapInputs_WhenCallingNormally_ExpectNotImplementedException()
        {
            //------------Setup for test--------------------------
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;

            //------------Execute Test---------------------------
            compiler.ConvertAndOnlyMapInputs(DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), new StringBuilder(), new StringBuilder(), out errors);

            //------------Assert Results-------------------------
            var theErrors = errors.FetchErrors();
            Assert.AreEqual(1, theErrors.Count);
            StringAssert.Contains(theErrors[0], "The method or operation is not implemented.");

        }
    }
}
