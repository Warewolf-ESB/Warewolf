
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Statements;
using System.Diagnostics.CodeAnalysis;
using ActivityUnitTests;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for FindRecordsActivityTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class FindRecordsActivityTest : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void GreaterThan_FullRecordsetWithStarIndex_Expected_Results_For_2_3_6_7_8_9()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfFindRecordsActivity
                {
                    FieldsToSearch = "[[Recset(*)]]",
                    SearchCriteria = "32",
                    SearchType = ">",
                    StartIndex = "",
                    Result = "[[Result().res]]"
                }
            };

            const string data = @"<ADL>
  <Recset>
	<Field1>Mr A</Field1>
	<Field2>25</Field2>
	<Field3>a@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr B</Field1>
	<Field2>651</Field2>
	<Field3>b@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr C</Field1>
	<Field2>48</Field2>
	<Field3>c@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr D</Field1>
	<Field2>1</Field2>
	<Field3>d@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr E</Field1>
	<Field2>22</Field2>
	<Field3>e@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr F</Field1>
	<Field2>321</Field2>
	<Field3>f@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr G</Field1>
	<Field2>51</Field2>
	<Field3>g@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr H</Field1>
	<Field2>2120</Field2>
	<Field3>h@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr I</Field1>
	<Field2>46</Field2>
	<Field3>i@abc.co.za</Field3>
  </Recset>
</ADL>";

            CurrentDl = "<DL><Recset><Field1/><Field2/><Field3/></Recset><Result><res/></Result></DL>";
            TestData = "<root>" + data + "</root>";
            IDSFDataObject result = ExecuteProcess();

            string error;
            IBinaryDataListEntry entry;
            ErrorResultTO errors;
            IBinaryDataList bdl = Compiler.FetchBinaryDataList(result.DataListID, out errors);
            bdl.TryGetEntry("Result", out entry, out error);

            var res = entry.ItemCollectionSize();

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(6, res);
        }

        [TestMethod]
        public void GreaterThan_Expected_Results_For_2_3_6_7_8_9()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfFindRecordsActivity
                {
                    FieldsToSearch = "[[Recset().Field1]],[[Recset().Field2]],[[Recset().Field3]]",
                    SearchCriteria = "32",
                    SearchType = ">",
                    StartIndex = "",
                    Result = "[[Result().res]]"
                }
            };

            const string data = @"<ADL>
  <Recset>
	<Field1>Mr A</Field1>
	<Field2>25</Field2>
	<Field3>a@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr B</Field1>
	<Field2>651</Field2>
	<Field3>b@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr C</Field1>
	<Field2>48</Field2>
	<Field3>c@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr D</Field1>
	<Field2>1</Field2>
	<Field3>d@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr E</Field1>
	<Field2>22</Field2>
	<Field3>e@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr F</Field1>
	<Field2>321</Field2>
	<Field3>f@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr G</Field1>
	<Field2>51</Field2>
	<Field3>g@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr H</Field1>
	<Field2>2120</Field2>
	<Field3>h@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr I</Field1>
	<Field2>46</Field2>
	<Field3>i@abc.co.za</Field3>
  </Recset>
</ADL>";

            CurrentDl = "<DL><Recset><Field1/><Field2/><Field3/></Recset><Result><res/></Result></DL>";
            TestData = "<root>" + data + "</root>";
            IDSFDataObject result = ExecuteProcess();

            string error;
            IBinaryDataListEntry entry;
            ErrorResultTO errors;
            IBinaryDataList bdl = Compiler.FetchBinaryDataList(result.DataListID, out errors);
            bdl.TryGetEntry("Result", out entry, out error);

            var res = entry.ItemCollectionSize();

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(6, res);
        }

        [TestMethod]
        public void GreaterThan_SearchWithIndex_Expected_Results_For_2_3_6_7_8_9()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfFindRecordsActivity
                {
                    FieldsToSearch = "[[Recset(*).Field1]],[[Recset().Field2]],[[Recset().Field3]]",
                    SearchCriteria = "32",
                    SearchType = ">",
                    StartIndex = "",
                    Result = "[[Result().res]]"
                }
            };

            const string data = @"<ADL>
  <Recset>
	<Field1>Mr A</Field1>
	<Field2>25</Field2>
	<Field3>a@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr B</Field1>
	<Field2>651</Field2>
	<Field3>b@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr C</Field1>
	<Field2>48</Field2>
	<Field3>c@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr D</Field1>
	<Field2>1</Field2>
	<Field3>d@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr E</Field1>
	<Field2>22</Field2>
	<Field3>e@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr F</Field1>
	<Field2>321</Field2>
	<Field3>f@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr G</Field1>
	<Field2>51</Field2>
	<Field3>g@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr H</Field1>
	<Field2>2120</Field2>
	<Field3>h@abc.co.za</Field3>
  </Recset>
  <Recset>
	<Field1>Mr I</Field1>
	<Field2>46</Field2>
	<Field3>i@abc.co.za</Field3>
  </Recset>
</ADL>";

            CurrentDl = "<DL><Recset><Field1/><Field2/><Field3/></Recset><Result><res/></Result></DL>";
            TestData = "<root>" + data + "</root>";
            IDSFDataObject result = ExecuteProcess();
            string error;
            IBinaryDataListEntry entry;
            ErrorResultTO errors;
            IBinaryDataList bdl = Compiler.FetchBinaryDataList(result.DataListID, out errors);
            bdl.TryGetEntry("Result", out entry, out error);

            var res = entry.ItemCollectionSize();

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(6, res);
        }

        [TestMethod]
        public void GreaterThan_WithTextInMatchField_Expected_NoResults()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfFindRecordsActivity
                {
                    FieldsToSearch = "[[Recset().Field1]],[[Recset().Field2]],[[Recset().Field3]]",
                    SearchCriteria = "jimmy",
                    SearchType = ">",
                    StartIndex = "",
                    Result = "[[Result().res]]"
                }
            };

            CurrentDl = "<DL><Recset><Field1/><Field2/><Field3/></Recset><Result><res/></Result></DL>";
            TestData = "<root>" + ActivityStrings.FindRecords_PreDataList + "</root>";
            IDSFDataObject result = ExecuteProcess();

            string error;
            IBinaryDataListEntry entry;
            ErrorResultTO errors;
            IBinaryDataList bdl = Compiler.FetchBinaryDataList(result.DataListID, out errors);
            bdl.TryGetEntry("Result", out entry, out error);

            var res = entry.ItemCollectionSize();

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(1, res);
        }

        [TestMethod]
        public void ErrorHandeling_Expected_ErrorTag()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfFindRecordsActivity
                {
                    FieldsToSearch = "[[Recset().Field1]],[[Recset().Field2]],[[Recset().Field3]]",
                    SearchCriteria = "2",
                    SearchType = ">",
                    StartIndex = "",
                    Result = "[[//().res]]"
                }
            };

            CurrentDl = "<DL><Recset><Field1/><Field2/><Field3/></Recset><Result><res/></Result></DL>";
            TestData = "<root>" + ActivityStrings.FindRecords_PreDataList + "</root>";
            IDSFDataObject result = ExecuteProcess();

            var res = Compiler.HasErrors(result.DataListID);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.IsTrue(res);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfFindRecordsActivity_Execute")]
        public void DsfFindRecordsActivity_Execute_MultipleResultField_ExpectError()
        {
            TestStartNode = new FlowStep
            {
                Action = new DsfFindRecordsActivity
                {
                    FieldsToSearch = "[[Recset().Field1]],[[Recset().Field2]],[[Recset().Field3]]",
                    SearchCriteria = "2",
                    SearchType = ">",
                    StartIndex = "",
                    Result = "[[a]][[b]]"
                }
            };

            CurrentDl = "<DL><Recset><Field1/><Field2/><Field3/></Recset><Result><res/></Result></DL>";
            TestData = "<root>" + ActivityStrings.FindRecords_PreDataList + "</root>";
            IDSFDataObject result = ExecuteProcess();

            var res = Compiler.HasErrors(result.DataListID);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.IsTrue(res);
        }

        #region Get Input/Output Tests

        [TestMethod]
        public void FindRecords_GetInputs_Expected_Five_Input()
        {
            DsfFindRecordsActivity testAct = new DsfFindRecordsActivity();

            IBinaryDataList inputs = testAct.GetInputs();

            var res = inputs.FetchAllEntries().Count;

            // remove test datalist ;)
            DataListRemoval(inputs.UID);


            Assert.AreEqual(5, res);
        }

        [TestMethod]
        public void FindRecords_GetOutputs_Expected_One_Output()
        {
            DsfFindRecordsActivity testAct = new DsfFindRecordsActivity();

            IBinaryDataList outputs = testAct.GetOutputs();

            var res = outputs.FetchAllEntries().Count;

            // remove test datalist ;)
            DataListRemoval(outputs.UID);

            Assert.AreEqual(1, res);
        }

        #endregion Get Input/Output Tests

    }
}
