using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common;
using Dev2.Data.Binary_Objects;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.BinaryDataList.Converters
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class FetchStudioDataListXMLTranslatorTest
    {
        static readonly IDataListCompiler _compiler = DataListFactory.CreateDataListCompiler();

        private Guid CreateDataList(enDev2ColumnArgumentDirection dir)
        {
            DataListFactory.CreateDataListCompiler();
            string error;
            ErrorResultTO errors;

            IBinaryDataList dl1 = Dev2BinaryDataListFactory.CreateDataList(GlobalConstants.NullDataListID);

            IList<Dev2Column> cols = new List<Dev2Column>();
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f1", dir));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f2", dir));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f3", dir));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f4", dir));
            cols.Add(Dev2BinaryDataListFactory.CreateColumn("f5", dir));

            dl1.TryCreateRecordsetTemplate("recset", string.Empty, cols, true, true, dir, out error);

            dl1.TryCreateScalarTemplate(string.Empty, "myScalar", string.Empty, true, true, dir, out error);
            // dl1.Dispose();
            return (_compiler.PushBinaryDataList(dl1.UID, dl1, out errors));

        }


        #region XML Creation Test

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataListXMLTranslatorWithOutSystemTags_ConvertAndOnlyMapInputs")]
        public void DataListXMLTranslatorWithOutSystemTags_ConvertAndOnlyMapInputs_WhenCallingNormally_ExpectNotImplementedException()
        {
            //------------Setup for test--------------------------
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;

            //------------Execute Test---------------------------
            compiler.ConvertAndOnlyMapInputs(DataListFormat.CreateFormat(GlobalConstants._Studio_XML), string.Empty, string.Empty, out errors);

            //------------Assert Results-------------------------
            var theErrors = errors.FetchErrors();
            Assert.AreEqual(1, theErrors.Count);
            StringAssert.Contains(theErrors[0], "The method or operation is not implemented.");

        }

        [TestMethod]
        public void Can_Create_XML_With_ColumnIODirection_Input()
        {
            Guid tmp = CreateDataList(enDev2ColumnArgumentDirection.Input);
            ErrorResultTO errors;

            string result = _compiler.ConvertFrom(tmp, DataListFormat.CreateFormat(GlobalConstants._Studio_XML), enTranslationDepth.Shape, out errors);

            const string expected = @"<DataList><recset Description="""" IsEditable=""True"" ColumnIODirection=""Input"" ><f1 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /><f2 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /><f3 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /><f4 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /><f5 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /></recset><myScalar Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /></DataList>";

            var res = _compiler.HasErrors(tmp);

            _compiler.ForceDeleteDataListByID(tmp);

            Assert.AreEqual(expected, result);
            Assert.IsFalse(res);
        }

        [TestMethod]
        public void Can_Create_XML_With_ColumnIODirection_Output()
        {
            Guid tmp = CreateDataList(enDev2ColumnArgumentDirection.Output);
            ErrorResultTO errors;

            string result = _compiler.ConvertFrom(tmp, DataListFormat.CreateFormat(GlobalConstants._Studio_XML), enTranslationDepth.Shape, out errors);

            const string expected = @"<DataList><recset Description="""" IsEditable=""True"" ColumnIODirection=""Output"" ><f1 Description="""" IsEditable=""True"" ColumnIODirection=""Output"" /><f2 Description="""" IsEditable=""True"" ColumnIODirection=""Output"" /><f3 Description="""" IsEditable=""True"" ColumnIODirection=""Output"" /><f4 Description="""" IsEditable=""True"" ColumnIODirection=""Output"" /><f5 Description="""" IsEditable=""True"" ColumnIODirection=""Output"" /></recset><myScalar Description="""" IsEditable=""True"" ColumnIODirection=""Output"" /></DataList>";

            var res = _compiler.HasErrors(tmp);

            _compiler.ForceDeleteDataListByID(tmp);

            Assert.AreEqual(expected, result);
            Assert.IsFalse(res);
        }

        [TestMethod]
        public void Can_Create_XML_With_ColumnIODirection_Both()
        {
            Guid tmp = CreateDataList(enDev2ColumnArgumentDirection.Both);
            ErrorResultTO errors;

            string result = _compiler.ConvertFrom(tmp, DataListFormat.CreateFormat(GlobalConstants._Studio_XML), enTranslationDepth.Shape, out errors);

            const string expected = @"<DataList><recset Description="""" IsEditable=""True"" ColumnIODirection=""Both"" ><f1 Description="""" IsEditable=""True"" ColumnIODirection=""Both"" /><f2 Description="""" IsEditable=""True"" ColumnIODirection=""Both"" /><f3 Description="""" IsEditable=""True"" ColumnIODirection=""Both"" /><f4 Description="""" IsEditable=""True"" ColumnIODirection=""Both"" /><f5 Description="""" IsEditable=""True"" ColumnIODirection=""Both"" /></recset><myScalar Description="""" IsEditable=""True"" ColumnIODirection=""Both"" /></DataList>";

            _compiler.ForceDeleteDataListByID(tmp);

            Assert.AreEqual(expected, result);
        }

        #endregion

        #region BinaryDataList Creation
        [TestMethod]
        public void Can_Create_BinaryDataList_With_ColumnIODirection_Input()
        {
            ErrorResultTO errors;
            const string shape = @"<DataList><recset Description="""" IsEditable=""True"" ColumnIODirection=""Input"" ><f1 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /><f2 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /><f3 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /><f4 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /><f5 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /></recset><myScalar Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /></DataList>";


            Guid tmp = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._Studio_XML), shape, shape, out errors);
            IBinaryDataList bdl = _compiler.FetchBinaryDataList(tmp, out errors);
            _compiler.DeleteDataListByID(tmp);

            foreach(IBinaryDataListEntry entry in bdl.FetchAllEntries())
            {
                if(entry.IsRecordset)
                {
                    Assert.AreEqual(entry.ColumnIODirection, enDev2ColumnArgumentDirection.Input);

                    foreach(Dev2Column c in entry.Columns)
                    {
                        Assert.AreEqual(enDev2ColumnArgumentDirection.Input, c.ColumnIODirection);
                    }
                }
                else
                {
                    Assert.AreEqual(enDev2ColumnArgumentDirection.Input, entry.ColumnIODirection);
                }
            }

            _compiler.ForceDeleteDataListByID(tmp);

        }

        [TestMethod]
        public void Can_Create_BinaryDataList_With_ColumnIODirection_Output()
        {
            ErrorResultTO errors;
            const string shape = @"<DataList><recset Description="""" IsEditable=""True"" ColumnIODirection=""Output"" ><f1 Description="""" IsEditable=""True"" ColumnIODirection=""Output"" /><f2 Description="""" IsEditable=""True"" ColumnIODirection=""Output"" /><f3 Description="""" IsEditable=""True"" ColumnIODirection=""Output"" /><f4 Description="""" IsEditable=""True"" ColumnIODirection=""Output"" /><f5 Description="""" IsEditable=""True"" ColumnIODirection=""Output"" /></recset><myScalar Description="""" IsEditable=""True"" ColumnIODirection=""Output"" /></DataList>";


            Guid tmp = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._Studio_XML), shape, shape, out errors);
            IBinaryDataList bdl = _compiler.FetchBinaryDataList(tmp, out errors);
            _compiler.DeleteDataListByID(tmp);

            foreach(IBinaryDataListEntry entry in bdl.FetchAllEntries())
            {
                if(entry.IsRecordset)
                {
                    Assert.AreEqual(enDev2ColumnArgumentDirection.Output, entry.ColumnIODirection);

                    foreach(Dev2Column c in entry.Columns)
                    {
                        Assert.AreEqual(enDev2ColumnArgumentDirection.Output, c.ColumnIODirection);
                    }
                }
                else
                {
                    Assert.AreEqual(enDev2ColumnArgumentDirection.Output, entry.ColumnIODirection);
                }
            }

            _compiler.ForceDeleteDataListByID(tmp);
        }


        [TestMethod]
        public void Can_Create_BinaryDataList_With_ColumnIODirection_Both()
        {
            ErrorResultTO errors;
            const string shape = @"<DataList><recset Description="""" IsEditable=""True"" ColumnIODirection=""Both"" ><f1 Description="""" IsEditable=""True"" ColumnIODirection=""Both"" /><f2 Description="""" IsEditable=""True"" ColumnIODirection=""Both"" /><f3 Description="""" IsEditable=""True"" ColumnIODirection=""Both"" /><f4 Description="""" IsEditable=""True"" ColumnIODirection=""Both"" /><f5 Description="""" IsEditable=""True"" ColumnIODirection=""Both"" /></recset><myScalar Description="""" IsEditable=""True"" ColumnIODirection=""Both"" /></DataList>";


            Guid tmp = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._Studio_XML), shape, shape, out errors);
            IBinaryDataList bdl = _compiler.FetchBinaryDataList(tmp, out errors);
            _compiler.DeleteDataListByID(tmp);

            foreach(IBinaryDataListEntry entry in bdl.FetchAllEntries())
            {
                if(entry.IsRecordset)
                {
                    Assert.AreEqual(enDev2ColumnArgumentDirection.Both, entry.ColumnIODirection);

                    foreach(Dev2Column c in entry.Columns)
                    {
                        Assert.AreEqual(enDev2ColumnArgumentDirection.Both, c.ColumnIODirection);
                    }
                }
                else
                {
                    Assert.AreEqual(enDev2ColumnArgumentDirection.Both, entry.ColumnIODirection);
                }
            }

            _compiler.ForceDeleteDataListByID(tmp);
        }

        [TestMethod]
        public void Can_Create_BinaryDataList_With_Blank_IsEditable_Blank_Expected_All_IsEditable_Properties_Are_True()
        {
            ErrorResultTO errors;
            const string shape = @"<DataList><recset Description="""" ColumnIODirection=""Both"" ><f1 Description="""" ColumnIODirection=""Both"" /><f2 Description="""" ColumnIODirection=""Both"" /><f3 Description="""" ColumnIODirection=""Both"" /><f4 Description="""" ColumnIODirection=""Both"" /><f5 Description="""" ColumnIODirection=""Both"" /></recset><myScalar Description="""" ColumnIODirection=""Both"" /></DataList>";


            Guid tmp = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._Studio_XML), shape, shape, out errors);
            IBinaryDataList bdl = _compiler.FetchBinaryDataList(tmp, out errors);
            _compiler.DeleteDataListByID(tmp);

            foreach(IBinaryDataListEntry entry in bdl.FetchAllEntries())
            {
                if(entry.IsRecordset)
                {
                    Assert.AreEqual(true, entry.IsEditable);

                    foreach(Dev2Column c in entry.Columns)
                    {
                        Assert.AreEqual(true, c.IsEditable);
                    }
                }
                else
                {
                    Assert.AreEqual(true, entry.IsEditable);
                }
            }

            _compiler.ForceDeleteDataListByID(tmp);
        }


        #region Negative Test

        [TestMethod]
        public void Can_Create_BinaryDataList_With_ColumnIODirection_InvalidDirection_Yield_Both()
        {
            ErrorResultTO errors;
            const string shape = @"<DataList><recset Description="""" IsEditable=""True"" ColumnIODirection=""Both"" ><f1 Description="""" IsEditable=""True"" ColumnIODirection=""Both"" /><f2 Description="""" IsEditable=""True"" ColumnIODirection=""Both"" /><f3 Description="""" IsEditable=""True"" ColumnIODirection=""Both"" /><f4 Description="""" IsEditable=""True"" ColumnIODirection=""Both"" /><f5 Description="""" IsEditable=""True"" ColumnIODirection=""Both"" /></recset><myScalar Description="""" IsEditable=""True"" ColumnIODirection=""MalformedDirection"" /></DataList>";


            Guid tmp = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._Studio_XML), shape, shape, out errors);
            IBinaryDataList bdl = _compiler.FetchBinaryDataList(tmp, out errors);
            _compiler.DeleteDataListByID(tmp);

            foreach(IBinaryDataListEntry entry in bdl.FetchAllEntries())
            {
                if(entry.IsRecordset)
                {
                    Assert.AreEqual(enDev2ColumnArgumentDirection.Both, entry.ColumnIODirection);

                    foreach(Dev2Column c in entry.Columns)
                    {
                        Assert.AreEqual(enDev2ColumnArgumentDirection.Both, c.ColumnIODirection);
                    }
                }
                else
                {
                    Assert.AreEqual(enDev2ColumnArgumentDirection.None, entry.ColumnIODirection);
                }
            }

            _compiler.ForceDeleteDataListByID(tmp);
        }

        [TestMethod]
        public void Can_Create_BinaryDataList_With_No_ColumnDirection()
        {
            ErrorResultTO errors;
            const string shape = @"<DataList><recset Description="""" IsEditable=""True""  ><f1 Description="""" IsEditable=""True""  /><f2 Description="""" IsEditable=""True""  /><f3 Description="""" IsEditable=""True""  /><f4 Description="""" IsEditable=""True""  /><f5 Description="""" IsEditable=""True""  /></recset><myScalar Description="""" IsEditable=""True"" /></DataList>";


            Guid tmp = _compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._Studio_XML), string.Empty, shape, out errors);
            IBinaryDataList bdl = _compiler.FetchBinaryDataList(tmp, out errors);
            _compiler.DeleteDataListByID(tmp);

            foreach(IBinaryDataListEntry entry in bdl.FetchAllEntries())
            {
                if(entry.IsRecordset)
                {
                    Assert.AreEqual(enDev2ColumnArgumentDirection.None, entry.ColumnIODirection);

                    foreach(Dev2Column c in entry.Columns)
                    {
                        Assert.AreEqual(enDev2ColumnArgumentDirection.None, c.ColumnIODirection);
                    }
                }
                else
                {
                    Assert.AreEqual(enDev2ColumnArgumentDirection.None, entry.ColumnIODirection);
                }
            }

            _compiler.ForceDeleteDataListByID(tmp);
        }

        #endregion

        #endregion
    }
}
