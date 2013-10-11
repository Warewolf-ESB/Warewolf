using Dev2.Activities;
using Dev2.DataList;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.DataList
{
    /// <summary>
    /// Summary description for ActivityDataMappingBuilderTest
    /// </summary>
    [TestClass]
    public class ActivityDataMappingBuilderTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ActivityDataMappingBuilder_Generate")]
        public void ActivityDataMappingBuilder_SetupActivityData_WhenValidServiceDefintion_ExpectValidInputsAndOutputs()
        {
            //------------Setup for test--------------------------

            #region ServiceDef
            var serviceDefStr = @"<Service ID=""df9e59ed-11f0-4359-bd21-7ad35898b383"" Version=""1.0"" Name=""Get Rows"" ResourceType=""DbService"" IsValid=""false"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"">
  <Actions>
    <Action Name=""Row"" Type=""InvokeStoredProc"" SourceID=""62505a00-b304-4ac0-a55c-50ce85111f16"" SourceName=""GenDev"" SourceMethod=""dbo.proc_get_Rows"">
      <Inputs>
        <Input Name=""Rows"" Source=""Rows"" EmptyToNull=""false"" DefaultValue="""" />
      </Inputs>
      <Outputs>
        <Output Name=""BigID"" MapsTo=""BigID"" Value=""[[Row().BigID]]"" Recordset=""Row"" />
        <Output Name=""Column1"" MapsTo=""Column1"" Value=""[[Row().Column1]]"" Recordset=""Row"" />
        <Output Name=""Column2"" MapsTo=""Column2"" Value=""[[Row().Column2]]"" Recordset=""Row"" />
        <Output Name=""Column3"" MapsTo=""Column3"" Value=""[[Row().Column3]]"" Recordset=""Row"" />
        <Output Name=""Column4"" MapsTo=""Column4"" Value=""[[Row().Column4]]"" Recordset=""Row"" />
        <Output Name=""Column5"" MapsTo=""Column5"" Value=""[[Row().Column5]]"" Recordset=""Row"" />
        <Output Name=""Column6"" MapsTo=""Column6"" Value=""[[Row().Column6]]"" Recordset=""Row"" />
        <Output Name=""Column7"" MapsTo=""Column7"" Value=""[[Row().Column7]]"" Recordset=""Row"" />
        <Output Name=""Column8"" MapsTo=""Column8"" Value=""[[Row().Column8]]"" Recordset=""Row"" />
        <Output Name=""Column9"" MapsTo=""Column9"" Value=""[[Row().Column9]]"" Recordset=""Row"" />
        <Output Name=""Column10"" MapsTo=""Column10"" Value=""[[Row().Column10]]"" Recordset=""Row"" />
      </Outputs>
      
    </Action>
  </Actions>
  <AuthorRoles />
  <Comment />
  <Tags />
  <HelpLink />
  <UnitTestTargetWorkflowService />
  <BizRule />
  <WorkflowActivityDef />
  <XamlDefinition />
  <DataList />
  <TypeOf>InvokeStoredProc</TypeOf>
  <DisplayName>Get Rows</DisplayName>
  <Category></Category>
  <AuthorRoles></AuthorRoles>
  <ErrorMessages />
  <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
    <SignedInfo>
      <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />
      <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
      <Reference URI="""">
        <Transforms>
          <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
        </Transforms>
        <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
        <DigestValue>wnGdgUqy2wpUORyoPj+hiHeya6E=</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>qRUe2AqKMusy9JrUY3vNdXCI//Z8UCMSQKBSHIDPMMVdVkKCbWKnUZl1XYa9/LZHAtzX21idcKjWwCQmpgBmrQHWj2Mcp7/XNKp4Q7sJZNGhfOz1163pHR/pN2Lb2gPK8hzzqFRGvk1zzav0RHqJjqVaEw63A88P/UqVTsY94t4=</SignatureValue>
  </Signature>
</Service>";
            #endregion

            var activityDataMappingBuilder = new ActivityDataMappingBuilder();

            Mock<IContextualResourceModel> resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(c => c.DataList).Returns("<DataList/>");

            Mock<IWebActivity> activity = new Mock<IWebActivity>();

            activity.Setup(c => c.SavedInputMapping).Returns(string.Empty);
            activity.Setup(c => c.SavedOutputMapping).Returns(string.Empty);
            activity.Setup(c => c.ResourceModel.ServiceDefinition).Returns(serviceDefStr);

            //------------Execute Test---------------------------

            activityDataMappingBuilder.SetupActivityData(activity.Object);

            //------------Assert Results-------------------------

            const string inputExpected = @"<Inputs><Input Name=""Rows"" Source=""Rows"" EmptyToNull=""false"" DefaultValue="""" /></Inputs>";
            const string outputExpected = @"<Outputs><Output Name=""BigID"" MapsTo=""BigID"" Value=""[[Row().BigID]]"" Recordset=""Row"" /><Output Name=""Column1"" MapsTo=""Column1"" Value=""[[Row().Column1]]"" Recordset=""Row"" /><Output Name=""Column2"" MapsTo=""Column2"" Value=""[[Row().Column2]]"" Recordset=""Row"" /><Output Name=""Column3"" MapsTo=""Column3"" Value=""[[Row().Column3]]"" Recordset=""Row"" /><Output Name=""Column4"" MapsTo=""Column4"" Value=""[[Row().Column4]]"" Recordset=""Row"" /><Output Name=""Column5"" MapsTo=""Column5"" Value=""[[Row().Column5]]"" Recordset=""Row"" /><Output Name=""Column6"" MapsTo=""Column6"" Value=""[[Row().Column6]]"" Recordset=""Row"" /><Output Name=""Column7"" MapsTo=""Column7"" Value=""[[Row().Column7]]"" Recordset=""Row"" /><Output Name=""Column8"" MapsTo=""Column8"" Value=""[[Row().Column8]]"" Recordset=""Row"" /><Output Name=""Column9"" MapsTo=""Column9"" Value=""[[Row().Column9]]"" Recordset=""Row"" /><Output Name=""Column10"" MapsTo=""Column10"" Value=""[[Row().Column10]]"" Recordset=""Row"" /></Outputs>";

            Assert.AreEqual(inputExpected, activityDataMappingBuilder.ActivityInputDefinitions);
            Assert.AreEqual(outputExpected, activityDataMappingBuilder.ActivityOutputDefinitions);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ActivityDataMappingBuilder_Generate")]
        public void ActivityDataMappingBuilder_SetupActivityData_WhenValidServiceDefintion_ExpectEmptySavedIODataAndCorrectActivityType()
        {
            //------------Setup for test--------------------------

            #region ServiceDef
            var serviceDefStr = @"<Service ID=""df9e59ed-11f0-4359-bd21-7ad35898b383"" Version=""1.0"" Name=""Get Rows"" ResourceType=""DbService"" IsValid=""false"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"">
  <Actions>
    <Action Name=""Row"" Type=""InvokeStoredProc"" SourceID=""62505a00-b304-4ac0-a55c-50ce85111f16"" SourceName=""GenDev"" SourceMethod=""dbo.proc_get_Rows"">
      <Inputs>
        <Input Name=""Rows"" Source=""Rows"" EmptyToNull=""false"" DefaultValue="""" />
      </Inputs>
      <Outputs>
        <Output Name=""BigID"" MapsTo=""BigID"" Value=""[[Row().BigID]]"" Recordset=""Row"" />
        <Output Name=""Column1"" MapsTo=""Column1"" Value=""[[Row().Column1]]"" Recordset=""Row"" />
        <Output Name=""Column2"" MapsTo=""Column2"" Value=""[[Row().Column2]]"" Recordset=""Row"" />
        <Output Name=""Column3"" MapsTo=""Column3"" Value=""[[Row().Column3]]"" Recordset=""Row"" />
        <Output Name=""Column4"" MapsTo=""Column4"" Value=""[[Row().Column4]]"" Recordset=""Row"" />
        <Output Name=""Column5"" MapsTo=""Column5"" Value=""[[Row().Column5]]"" Recordset=""Row"" />
        <Output Name=""Column6"" MapsTo=""Column6"" Value=""[[Row().Column6]]"" Recordset=""Row"" />
        <Output Name=""Column7"" MapsTo=""Column7"" Value=""[[Row().Column7]]"" Recordset=""Row"" />
        <Output Name=""Column8"" MapsTo=""Column8"" Value=""[[Row().Column8]]"" Recordset=""Row"" />
        <Output Name=""Column9"" MapsTo=""Column9"" Value=""[[Row().Column9]]"" Recordset=""Row"" />
        <Output Name=""Column10"" MapsTo=""Column10"" Value=""[[Row().Column10]]"" Recordset=""Row"" />
      </Outputs>
      
    </Action>
  </Actions>
  <AuthorRoles />
  <Comment />
  <Tags />
  <HelpLink />
  <UnitTestTargetWorkflowService />
  <BizRule />
  <WorkflowActivityDef />
  <XamlDefinition />
  <DataList />
  <TypeOf>InvokeStoredProc</TypeOf>
  <DisplayName>Get Rows</DisplayName>
  <Category></Category>
  <AuthorRoles></AuthorRoles>
  <ErrorMessages />
  <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
    <SignedInfo>
      <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />
      <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
      <Reference URI="""">
        <Transforms>
          <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
        </Transforms>
        <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
        <DigestValue>wnGdgUqy2wpUORyoPj+hiHeya6E=</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>qRUe2AqKMusy9JrUY3vNdXCI//Z8UCMSQKBSHIDPMMVdVkKCbWKnUZl1XYa9/LZHAtzX21idcKjWwCQmpgBmrQHWj2Mcp7/XNKp4Q7sJZNGhfOz1163pHR/pN2Lb2gPK8hzzqFRGvk1zzav0RHqJjqVaEw63A88P/UqVTsY94t4=</SignatureValue>
  </Signature>
</Service>";
            #endregion

            var activityDataMappingBuilder = new ActivityDataMappingBuilder();

            Mock<IContextualResourceModel> resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(c => c.DataList).Returns("<DataList/>");

            Mock<IWebActivity> activity = new Mock<IWebActivity>();

            activity.Setup(c => c.SavedInputMapping).Returns(string.Empty);
            activity.Setup(c => c.SavedOutputMapping).Returns(string.Empty);
            activity.Setup(c => c.ResourceModel.ServiceDefinition).Returns(serviceDefStr);
            activity.Setup(c => c.UnderlyingWebActivityObjectType).Returns(typeof (DsfDatabaseActivity));

            //------------Execute Test---------------------------

            activityDataMappingBuilder.SetupActivityData(activity.Object);

            //------------Assert Results-------------------------

            Assert.AreEqual(string.Empty, activityDataMappingBuilder.SavedInputMapping);
            Assert.AreEqual(string.Empty, activityDataMappingBuilder.SavedOutputMapping);
            Assert.AreEqual(typeof(DsfDatabaseActivity), activityDataMappingBuilder.ActivityType);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ActivityDataMappingBuilder_Generate")]
        public void ActivityDataMappingBuilder_Generate_WhenValidServiceDefintion_ExpectValidInputAndOutputList()
        {
            //------------Setup for test--------------------------

            #region ServiceDef
            var serviceDefStr = @"<Service ID=""df9e59ed-11f0-4359-bd21-7ad35898b383"" Version=""1.0"" Name=""Get Rows"" ResourceType=""DbService"" IsValid=""false"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"">
  <Actions>
    <Action Name=""Row"" Type=""InvokeStoredProc"" SourceID=""62505a00-b304-4ac0-a55c-50ce85111f16"" SourceName=""GenDev"" SourceMethod=""dbo.proc_get_Rows"">
      <Inputs>
        <Input Name=""Rows"" Source=""Rows"" EmptyToNull=""false"" DefaultValue="""" />
      </Inputs>
      <Outputs>
        <Output Name=""BigID"" MapsTo=""BigID"" Value=""[[Row().BigID]]"" Recordset=""Row"" />
        <Output Name=""Column1"" MapsTo=""Column1"" Value=""[[Row().Column1]]"" Recordset=""Row"" />
        <Output Name=""Column2"" MapsTo=""Column2"" Value=""[[Row().Column2]]"" Recordset=""Row"" />
        <Output Name=""Column3"" MapsTo=""Column3"" Value=""[[Row().Column3]]"" Recordset=""Row"" />
        <Output Name=""Column4"" MapsTo=""Column4"" Value=""[[Row().Column4]]"" Recordset=""Row"" />
        <Output Name=""Column5"" MapsTo=""Column5"" Value=""[[Row().Column5]]"" Recordset=""Row"" />
        <Output Name=""Column6"" MapsTo=""Column6"" Value=""[[Row().Column6]]"" Recordset=""Row"" />
        <Output Name=""Column7"" MapsTo=""Column7"" Value=""[[Row().Column7]]"" Recordset=""Row"" />
        <Output Name=""Column8"" MapsTo=""Column8"" Value=""[[Row().Column8]]"" Recordset=""Row"" />
        <Output Name=""Column9"" MapsTo=""Column9"" Value=""[[Row().Column9]]"" Recordset=""Row"" />
        <Output Name=""Column10"" MapsTo=""Column10"" Value=""[[Row().Column10]]"" Recordset=""Row"" />
      </Outputs>
      
    </Action>
  </Actions>
  <AuthorRoles />
  <Comment />
  <Tags />
  <HelpLink />
  <UnitTestTargetWorkflowService />
  <BizRule />
  <WorkflowActivityDef />
  <XamlDefinition />
  <DataList />
  <TypeOf>InvokeStoredProc</TypeOf>
  <DisplayName>Get Rows</DisplayName>
  <Category></Category>
  <AuthorRoles></AuthorRoles>
  <ErrorMessages />
  <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
    <SignedInfo>
      <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />
      <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
      <Reference URI="""">
        <Transforms>
          <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
        </Transforms>
        <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
        <DigestValue>wnGdgUqy2wpUORyoPj+hiHeya6E=</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>qRUe2AqKMusy9JrUY3vNdXCI//Z8UCMSQKBSHIDPMMVdVkKCbWKnUZl1XYa9/LZHAtzX21idcKjWwCQmpgBmrQHWj2Mcp7/XNKp4Q7sJZNGhfOz1163pHR/pN2Lb2gPK8hzzqFRGvk1zzav0RHqJjqVaEw63A88P/UqVTsY94t4=</SignatureValue>
  </Signature>
</Service>";
            #endregion

            var activityDataMappingBuilder = new ActivityDataMappingBuilder();

            Mock<IContextualResourceModel> resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(c => c.DataList).Returns("<DataList/>");

            Mock<IWebActivity> activity = new Mock<IWebActivity>();

            activity.Setup(c => c.SavedInputMapping).Returns(string.Empty);
            activity.Setup(c => c.SavedOutputMapping).Returns(string.Empty);
            activity.Setup(c => c.ResourceModel.ServiceDefinition).Returns(serviceDefStr);
            activity.Setup(c => c.UnderlyingWebActivityObjectType).Returns(typeof(DsfDatabaseActivity));

            activityDataMappingBuilder.SetupActivityData(activity.Object);

            //------------Execute Test---------------------------

            var result = activityDataMappingBuilder.Generate();
            

            //------------Assert Results-------------------------

            // check counts first
            Assert.AreEqual(1, result.Inputs.Count);
            Assert.AreEqual(11, result.Outputs.Count);

            // now check data
            Assert.AreEqual("[[Rows]]", result.Inputs[0].Value);

            Assert.AreEqual("[[Row().BigID]]", result.Outputs[0].Value);
            Assert.AreEqual("[[Row().Column1]]", result.Outputs[1].Value);
            Assert.AreEqual("[[Row().Column2]]", result.Outputs[2].Value);
            Assert.AreEqual("[[Row().Column3]]", result.Outputs[3].Value);
            Assert.AreEqual("[[Row().Column4]]", result.Outputs[4].Value);
            Assert.AreEqual("[[Row().Column5]]", result.Outputs[5].Value);
            Assert.AreEqual("[[Row().Column6]]", result.Outputs[6].Value);
            Assert.AreEqual("[[Row().Column7]]", result.Outputs[7].Value);
            Assert.AreEqual("[[Row().Column8]]", result.Outputs[8].Value);
            Assert.AreEqual("[[Row().Column9]]", result.Outputs[9].Value);
            Assert.AreEqual("[[Row().Column10]]", result.Outputs[10].Value);

        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ActivityDataMappingBuilder_Generate")]
        public void ActivityDataMappingBuilder_Generate_WhenValidServiceDefintionAndActivityHasSaveMappingData_ExpectValidInputAndOutputList()
        {
            //------------Setup for test--------------------------

            #region Setup Data

            var outputDefStr = @"      <Outputs>
        <Output Name=""BigID"" MapsTo=""BigID1"" Value=""[[Rowz().BigID]]"" Recordset=""Row"" />
        <Output Name=""Column11"" MapsTo=""Column11"" Value=""[[Rowz().Column1]]"" Recordset=""Rowz"" />
        <Output Name=""Column21"" MapsTo=""Column21"" Value=""[[Rowz().Column2]]"" Recordset=""Rowz"" />
        <Output Name=""Column31"" MapsTo=""Column31"" Value=""[[Rowz().Column3]]"" Recordset=""Rowz"" />
        <Output Name=""Column41"" MapsTo=""Column41"" Value=""[[Rowz().Column4]]"" Recordset=""Rowz"" />
        <Output Name=""Column51"" MapsTo=""Column51"" Value=""[[Rowz().Column5]]"" Recordset=""Rowz"" />
        <Output Name=""Column61"" MapsTo=""Column61"" Value=""[[Rowz().Column6]]"" Recordset=""Rowz"" />
        <Output Name=""Column71"" MapsTo=""Column71"" Value=""[[Rowz().Column7]]"" Recordset=""Rowz"" />
        <Output Name=""Column81"" MapsTo=""Column81"" Value=""[[Rowz().Column8]]"" Recordset=""Rowz"" />
        <Output Name=""Column91"" MapsTo=""Column91"" Value=""[[Rowz().Column9]]"" Recordset=""Rowz"" />
        <Output Name=""Column20"" MapsTo=""Column20"" Value=""[[Rowz().Column10]]"" Recordset=""Rowz"" />
      </Outputs>";

            var inputDefStr = @"<Inputs>
        <Input Name=""RowCnt"" Source=""RowCnt"" EmptyToNull=""false"" DefaultValue="""" />
      </Inputs>";

            var serviceDefStr = @"<Service ID=""df9e59ed-11f0-4359-bd21-7ad35898b383"" Version=""1.0"" Name=""Get Rows"" ResourceType=""DbService"" IsValid=""false"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"">
  <Actions>
    <Action Name=""Row"" Type=""InvokeStoredProc"" SourceID=""62505a00-b304-4ac0-a55c-50ce85111f16"" SourceName=""GenDev"" SourceMethod=""dbo.proc_get_Rows"">
      <Inputs>
        <Input Name=""Rows"" Source=""Rows"" EmptyToNull=""false"" DefaultValue="""" />
      </Inputs>
      <Outputs>
        <Output Name=""BigID"" MapsTo=""BigID"" Value=""[[Row().BigID]]"" Recordset=""Row"" />
        <Output Name=""Column1"" MapsTo=""Column1"" Value=""[[Row().Column1]]"" Recordset=""Row"" />
        <Output Name=""Column2"" MapsTo=""Column2"" Value=""[[Row().Column2]]"" Recordset=""Row"" />
        <Output Name=""Column3"" MapsTo=""Column3"" Value=""[[Row().Column3]]"" Recordset=""Row"" />
        <Output Name=""Column4"" MapsTo=""Column4"" Value=""[[Row().Column4]]"" Recordset=""Row"" />
        <Output Name=""Column5"" MapsTo=""Column5"" Value=""[[Row().Column5]]"" Recordset=""Row"" />
        <Output Name=""Column6"" MapsTo=""Column6"" Value=""[[Row().Column6]]"" Recordset=""Row"" />
        <Output Name=""Column7"" MapsTo=""Column7"" Value=""[[Row().Column7]]"" Recordset=""Row"" />
        <Output Name=""Column8"" MapsTo=""Column8"" Value=""[[Row().Column8]]"" Recordset=""Row"" />
        <Output Name=""Column9"" MapsTo=""Column9"" Value=""[[Row().Column9]]"" Recordset=""Row"" />
        <Output Name=""Column10"" MapsTo=""Column10"" Value=""[[Row().Column10]]"" Recordset=""Row"" />
      </Outputs>
      
    </Action>
  </Actions>
  <AuthorRoles />
  <Comment />
  <Tags />
  <HelpLink />
  <UnitTestTargetWorkflowService />
  <BizRule />
  <WorkflowActivityDef />
  <XamlDefinition />
  <DataList />
  <TypeOf>InvokeStoredProc</TypeOf>
  <DisplayName>Get Rows</DisplayName>
  <Category></Category>
  <AuthorRoles></AuthorRoles>
  <ErrorMessages />
  <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
    <SignedInfo>
      <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />
      <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
      <Reference URI="""">
        <Transforms>
          <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
        </Transforms>
        <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
        <DigestValue>wnGdgUqy2wpUORyoPj+hiHeya6E=</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>qRUe2AqKMusy9JrUY3vNdXCI//Z8UCMSQKBSHIDPMMVdVkKCbWKnUZl1XYa9/LZHAtzX21idcKjWwCQmpgBmrQHWj2Mcp7/XNKp4Q7sJZNGhfOz1163pHR/pN2Lb2gPK8hzzqFRGvk1zzav0RHqJjqVaEw63A88P/UqVTsY94t4=</SignatureValue>
  </Signature>
</Service>";
            #endregion

            var activityDataMappingBuilder = new ActivityDataMappingBuilder();

            Mock<IContextualResourceModel> resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(c => c.DataList).Returns("<DataList/>");

            Mock<IWebActivity> activity = new Mock<IWebActivity>();

            activity.Setup(c => c.SavedInputMapping).Returns(inputDefStr);
            activity.Setup(c => c.SavedOutputMapping).Returns(outputDefStr);
            activity.Setup(c => c.ResourceModel.ServiceDefinition).Returns(serviceDefStr);
            activity.Setup(c => c.UnderlyingWebActivityObjectType).Returns(typeof(DsfDatabaseActivity));

            activityDataMappingBuilder.SetupActivityData(activity.Object);

            //------------Execute Test---------------------------

            var result = activityDataMappingBuilder.Generate();

            //------------Assert Results-------------------------

            // check counts first
            Assert.AreEqual(1, result.Inputs.Count);
            Assert.AreEqual(11, result.Outputs.Count);

            // now check data
            Assert.AreEqual("[[RowCnt]]", result.Inputs[0].Value);

            Assert.AreEqual("[[Rowz().BigID]]", result.Outputs[0].Value);
            Assert.AreEqual("[[Rowz().Column1]]", result.Outputs[1].Value);
            Assert.AreEqual("[[Rowz().Column2]]", result.Outputs[2].Value);
            Assert.AreEqual("[[Rowz().Column3]]", result.Outputs[3].Value);
            Assert.AreEqual("[[Rowz().Column4]]", result.Outputs[4].Value);
            Assert.AreEqual("[[Rowz().Column5]]", result.Outputs[5].Value);
            Assert.AreEqual("[[Rowz().Column6]]", result.Outputs[6].Value);
            Assert.AreEqual("[[Rowz().Column7]]", result.Outputs[7].Value);
            Assert.AreEqual("[[Rowz().Column8]]", result.Outputs[8].Value);
            Assert.AreEqual("[[Rowz().Column9]]", result.Outputs[9].Value);
            Assert.AreEqual("[[Rowz().Column10]]", result.Outputs[10].Value);

        }


        // DO I NEED THIS?
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ActivityDataMappingBuilder_Generate")]
        [Ignore]
        public void ActivityDataMappingBuilder_Generate_WhenValidServiceDefintion_ExpectValidInputAndOutputList1()
        {
            //------------Setup for test--------------------------

            #region ServiceDef
            var serviceDefStr = @"<Service ID=""df9e59ed-11f0-4359-bd21-7ad35898b383"" Version=""1.0"" Name=""Get Rows"" ResourceType=""DbService"" IsValid=""false"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"">
  <Actions>
    <Action Name=""Row"" Type=""InvokeStoredProc"" SourceID=""62505a00-b304-4ac0-a55c-50ce85111f16"" SourceName=""GenDev"" SourceMethod=""dbo.proc_get_Rows"">
      <Inputs>
        <Input Name=""Rows"" Source=""Rows"" EmptyToNull=""false"" DefaultValue="""" />
      </Inputs>
      <Outputs>
        <Output Name=""BigID"" MapsTo=""BigID"" Value=""[[Row().BigID]]"" Recordset=""Row"" />
        <Output Name=""Column1"" MapsTo=""Column1"" Value=""[[Row().Column1]]"" Recordset=""Row"" />
        <Output Name=""Column2"" MapsTo=""Column2"" Value=""[[Row().Column2]]"" Recordset=""Row"" />
        <Output Name=""Column3"" MapsTo=""Column3"" Value=""[[Row().Column3]]"" Recordset=""Row"" />
        <Output Name=""Column4"" MapsTo=""Column4"" Value=""[[Row().Column4]]"" Recordset=""Row"" />
        <Output Name=""Column5"" MapsTo=""Column5"" Value=""[[Row().Column5]]"" Recordset=""Row"" />
        <Output Name=""Column6"" MapsTo=""Column6"" Value=""[[Row().Column6]]"" Recordset=""Row"" />
        <Output Name=""Column7"" MapsTo=""Column7"" Value=""[[Row().Column7]]"" Recordset=""Row"" />
        <Output Name=""Column8"" MapsTo=""Column8"" Value=""[[Row().Column8]]"" Recordset=""Row"" />
        <Output Name=""Column9"" MapsTo=""Column9"" Value=""[[Row().Column9]]"" Recordset=""Row"" />
        <Output Name=""Column10"" MapsTo=""Column10"" Value=""[[Row().Column10]]"" Recordset=""Row"" />
      </Outputs>
      
    </Action>
  </Actions>
  <AuthorRoles />
  <Comment />
  <Tags />
  <HelpLink />
  <UnitTestTargetWorkflowService />
  <BizRule />
  <WorkflowActivityDef />
  <XamlDefinition />
  <DataList />
  <TypeOf>InvokeStoredProc</TypeOf>
  <DisplayName>Get Rows</DisplayName>
  <Category></Category>
  <AuthorRoles></AuthorRoles>
  <ErrorMessages />
  <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
    <SignedInfo>
      <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />
      <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
      <Reference URI="""">
        <Transforms>
          <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
        </Transforms>
        <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
        <DigestValue>wnGdgUqy2wpUORyoPj+hiHeya6E=</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>qRUe2AqKMusy9JrUY3vNdXCI//Z8UCMSQKBSHIDPMMVdVkKCbWKnUZl1XYa9/LZHAtzX21idcKjWwCQmpgBmrQHWj2Mcp7/XNKp4Q7sJZNGhfOz1163pHR/pN2Lb2gPK8hzzqFRGvk1zzav0RHqJjqVaEw63A88P/UqVTsY94t4=</SignatureValue>
  </Signature>
</Service>";
            #endregion

            var activityDataMappingBuilder = new ActivityDataMappingBuilder();

            Mock<IContextualResourceModel> resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(c => c.DataList).Returns("<DataList><Rowz><BigID/><Column1/></Rowz></DataList>");

            Mock<IWebActivity> activity = new Mock<IWebActivity>();

            activity.Setup(c => c.SavedInputMapping).Returns(string.Empty);
            activity.Setup(c => c.SavedOutputMapping).Returns(string.Empty);
            activity.Setup(c => c.ResourceModel.ServiceDefinition).Returns(serviceDefStr);
            activity.Setup(c => c.UnderlyingWebActivityObjectType).Returns(typeof(DsfDatabaseActivity));

            activityDataMappingBuilder.SetupActivityData(activity.Object);

            //------------Execute Test---------------------------

            var result = activityDataMappingBuilder.Generate();

            //------------Assert Results-------------------------

            // check counts first
            Assert.AreEqual(1, result.Inputs.Count);
            Assert.AreEqual(11, result.Outputs.Count);

            // now check data
            Assert.AreEqual("[[Rows]]", result.Inputs[0].Value);

            Assert.AreEqual("[[Rowz().BigID]]", result.Outputs[0].Value);
            Assert.AreEqual("[[Rowz().Column1]]", result.Outputs[1].Value);
            Assert.AreEqual("[[Rowz().Column2]]", result.Outputs[2].Value);
            Assert.AreEqual("[[Rowz().Column3]]", result.Outputs[3].Value);
            Assert.AreEqual("[[Rowz().Column4]]", result.Outputs[4].Value);
            Assert.AreEqual("[[Rowz().Column5]]", result.Outputs[5].Value);
            Assert.AreEqual("[[Rowz().Column6]]", result.Outputs[6].Value);
            Assert.AreEqual("[[Rowz().Column7]]", result.Outputs[7].Value);
            Assert.AreEqual("[[Rowz().Column8]]", result.Outputs[8].Value);
            Assert.AreEqual("[[Rowz().Column9]]", result.Outputs[9].Value);
            Assert.AreEqual("[[Rowz().Column10]]", result.Outputs[10].Value);

        }

    }
}
