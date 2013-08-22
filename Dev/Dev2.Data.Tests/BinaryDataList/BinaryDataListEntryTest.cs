using System;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.BinaryDataList
{
    /// <summary>
    /// Test for the BinaryDataListEntry ;)
    /// </summary>
    [TestClass]
    public class BinaryDataListEntryTest
    {
        [TestMethod]
        [Owner("Travis")]
        [TestCategory("BinaryDataListEntry,UnitTest")]
        [Description("A test to ensure that once we delete all records with () notation and append new ones we get the correct indexing")]
        public void CanFetchCorrectAppendIndexWhenFullyDeletedViaBlankIndexing()
        {
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), "<xml><rs><val>1</val></rs><rs><val>1</val></rs><rs><val>1</val></rs></xml>", "<xml><rs><val/></rs></xml>", out errors);

            try
            {
                IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

                string error;
                IBinaryDataListEntry entry;

                // emulate the delete ;)
                if (bdl.TryGetEntry("rs", out entry, out error))
                {
                    entry.TryDeleteRows("");
                    entry.TryDeleteRows("");
                    entry.TryDeleteRows("");
                }

                Assert.AreEqual(1, entry.FetchAppendRecordsetIndex());
            }
            finally
            {
                // clean up ;)
                compiler.ForceDeleteDataListByID(dlID);

            }
        }

        [TestMethod]
        [Owner("Travis")]
        [TestCategory("BinaryDataListEntry,UnitTest")]
        [Description("A test to ensure that once we delete all records with numeric index notation and append new ones we get the correct indexing")]
        public void CanFetchCorrectAppendIndexWhenFullyDeletedViaNumericIndexing()
        {
            var compiler = DataListFactory.CreateDataListCompiler();
            ErrorResultTO errors;
            Guid dlID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML), "<xml><rs><val>1</val></rs><rs><val>1</val></rs><rs><val>1</val></rs></xml>", "<xml><rs><val/></rs></xml>", out errors);

            try
            {
                IBinaryDataList bdl = compiler.FetchBinaryDataList(dlID, out errors);

                string error;
                IBinaryDataListEntry entry;

                // emulate the delete ;)
                if (bdl.TryGetEntry("rs", out entry, out error))
                {
                    entry.TryDeleteRows("3");
                    entry.TryDeleteRows("2");
                    entry.TryDeleteRows("1");
                }

                Assert.AreEqual(1, entry.FetchAppendRecordsetIndex());
            }
            finally
            {
                // clean up ;)
                compiler.ForceDeleteDataListByID(dlID);

            }
        }
    }
}
