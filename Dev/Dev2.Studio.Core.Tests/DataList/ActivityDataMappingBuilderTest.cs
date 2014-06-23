using System.Text;
using Dev2.Activities;
using Dev2.DataList;
using Dev2.Studio.Core.AppResources.Enums;
using Dev2.Studio.Core.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using Moq;
using Unlimited.Applications.BusinessDesignStudio.Activities;

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

        #region Test

        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("ActivityDataMappingBuilder_Generate")]
        public void ActivityDataMappingBuilder_SetupActivityData_WhenValidInputOutputMappingAndNoServiceDef_ExpectValidInputsAndOutputs()
        {
            //------------Setup for test--------------------------

            #region ServiceDef
            var inputString = @"<Inputs><Input Name=""Rows"" Source=""Rows"" EmptyToNull=""false"" DefaultValue="""" /></Inputs>";
            var outputString = @"<Outputs><Output Name=""BigID"" MapsTo=""BigID"" Value=""[[Row().BigID]]"" Recordset=""Row"" /><Output Name=""Column1"" MapsTo=""Column1"" Value=""[[Row().Column1]]"" Recordset=""Row"" /><Output Name=""Column2"" MapsTo=""Column2"" Value=""[[Row().Column2]]"" Recordset=""Row"" /><Output Name=""Column3"" MapsTo=""Column3"" Value=""[[Row().Column3]]"" Recordset=""Row"" /><Output Name=""Column4"" MapsTo=""Column4"" Value=""[[Row().Column4]]"" Recordset=""Row"" /><Output Name=""Column5"" MapsTo=""Column5"" Value=""[[Row().Column5]]"" Recordset=""Row"" /><Output Name=""Column6"" MapsTo=""Column6"" Value=""[[Row().Column6]]"" Recordset=""Row"" /><Output Name=""Column7"" MapsTo=""Column7"" Value=""[[Row().Column7]]"" Recordset=""Row"" /><Output Name=""Column8"" MapsTo=""Column8"" Value=""[[Row().Column8]]"" Recordset=""Row"" /><Output Name=""Column9"" MapsTo=""Column9"" Value=""[[Row().Column9]]"" Recordset=""Row"" /><Output Name=""Column10"" MapsTo=""Column10"" Value=""[[Row().Column10]]"" Recordset=""Row"" /></Outputs>";
            
            #endregion

            var activityDataMappingBuilder = new ActivityDataMappingBuilder();

            Mock<IContextualResourceModel> resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(c => c.DataList).Returns("<DataList/>");

            Mock<IWebActivity> activity = new Mock<IWebActivity>();

            activity.Setup(c => c.SavedInputMapping).Returns(string.Empty);
            activity.Setup(c => c.SavedOutputMapping).Returns(string.Empty);
            activity.Setup(c => c.ResourceModel.Outputs).Returns(outputString);
            activity.Setup(c => c.ResourceModel.Inputs).Returns(inputString);            
            activity.Setup(c => c.ResourceModel.WorkflowXaml).Returns(new StringBuilder());
            activity.Setup(c => c.ResourceModel.ResourceType).Returns(ResourceType.Service);

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
        public void ActivityDataMappingBuilder_SetupActivityData_WhenValidServiceDefintion_ExpectValidInputsAndOutputs()
        {
            //------------Setup for test--------------------------

            #region ServiceDef
      
            var inputString = @"<Inputs><Input Name=""Rows"" Source=""Rows"" EmptyToNull=""false"" DefaultValue="""" /></Inputs>";

            var outputString = @"<Outputs><Output Name=""BigID"" MapsTo=""BigID"" Value=""[[Row().BigID]]"" Recordset=""Row"" /><Output Name=""Column1"" MapsTo=""Column1"" Value=""[[Row().Column1]]"" Recordset=""Row"" /><Output Name=""Column2"" MapsTo=""Column2"" Value=""[[Row().Column2]]"" Recordset=""Row"" /><Output Name=""Column3"" MapsTo=""Column3"" Value=""[[Row().Column3]]"" Recordset=""Row"" /><Output Name=""Column4"" MapsTo=""Column4"" Value=""[[Row().Column4]]"" Recordset=""Row"" /><Output Name=""Column5"" MapsTo=""Column5"" Value=""[[Row().Column5]]"" Recordset=""Row"" /><Output Name=""Column6"" MapsTo=""Column6"" Value=""[[Row().Column6]]"" Recordset=""Row"" /><Output Name=""Column7"" MapsTo=""Column7"" Value=""[[Row().Column7]]"" Recordset=""Row"" /><Output Name=""Column8"" MapsTo=""Column8"" Value=""[[Row().Column8]]"" Recordset=""Row"" /><Output Name=""Column9"" MapsTo=""Column9"" Value=""[[Row().Column9]]"" Recordset=""Row"" /><Output Name=""Column10"" MapsTo=""Column10"" Value=""[[Row().Column10]]"" Recordset=""Row"" /></Outputs>";
            
            #endregion

            var activityDataMappingBuilder = new ActivityDataMappingBuilder();

            Mock<IContextualResourceModel> resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(c => c.DataList).Returns("<DataList/>");

            Mock<IWebActivity> activity = new Mock<IWebActivity>();

            activity.Setup(c => c.SavedInputMapping).Returns(string.Empty);
            activity.Setup(c => c.SavedOutputMapping).Returns(string.Empty);
            activity.Setup(c => c.ResourceModel.Inputs).Returns(inputString);
            activity.Setup(c => c.ResourceModel.Outputs).Returns(outputString);
            activity.Setup(c => c.ResourceModel.WorkflowXaml).Returns(new StringBuilder());
            activity.Setup(c => c.ResourceModel.ResourceType).Returns(ResourceType.Service);

            //------------Execute Test---------------------------

            activityDataMappingBuilder.SetupActivityData(activity.Object);

            //------------Assert Results-------------------------

            const string inputExpected = "<Inputs><Input Name=\"Rows\" Source=\"Rows\" EmptyToNull=\"false\" DefaultValue=\"\" /></Inputs>";
            const string outputExpected = @"<Outputs><Output Name=""BigID"" MapsTo=""BigID"" Value=""[[Row().BigID]]"" Recordset=""Row"" /><Output Name=""Column1"" MapsTo=""Column1"" Value=""[[Row().Column1]]"" Recordset=""Row"" /><Output Name=""Column2"" MapsTo=""Column2"" Value=""[[Row().Column2]]"" Recordset=""Row"" /><Output Name=""Column3"" MapsTo=""Column3"" Value=""[[Row().Column3]]"" Recordset=""Row"" /><Output Name=""Column4"" MapsTo=""Column4"" Value=""[[Row().Column4]]"" Recordset=""Row"" /><Output Name=""Column5"" MapsTo=""Column5"" Value=""[[Row().Column5]]"" Recordset=""Row"" /><Output Name=""Column6"" MapsTo=""Column6"" Value=""[[Row().Column6]]"" Recordset=""Row"" /><Output Name=""Column7"" MapsTo=""Column7"" Value=""[[Row().Column7]]"" Recordset=""Row"" /><Output Name=""Column8"" MapsTo=""Column8"" Value=""[[Row().Column8]]"" Recordset=""Row"" /><Output Name=""Column9"" MapsTo=""Column9"" Value=""[[Row().Column9]]"" Recordset=""Row"" /><Output Name=""Column10"" MapsTo=""Column10"" Value=""[[Row().Column10]]"" Recordset=""Row"" /></Outputs>";

            Assert.AreEqual(inputExpected, activityDataMappingBuilder.ActivityInputDefinitions);
            Assert.AreEqual(outputExpected, activityDataMappingBuilder.ActivityOutputDefinitions);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("ActivityDataMappingBuilder_Generate")]
        public void ActivityDataMappingBuilder_SetupActivityData_WhenValidServiceDefintion_ActivityNotAvailable()
        {
            //------------Setup for test--------------------------

            #region ServiceDef
            var serviceDefStr = @"
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
      
    </Action>";
            #endregion

            var activityDataMappingBuilder = new ActivityDataMappingBuilder();

            Mock<IContextualResourceModel> resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(c => c.DataList).Returns("<DataList/>");
            resourceModel.Setup(a => a.Inputs).Returns("Bob");
            resourceModel.Setup(a => a.Outputs).Returns("Builder");

            Mock<IWebActivity> activity = new Mock<IWebActivity>();

            activity.Setup(c => c.SavedInputMapping).Returns(string.Empty);
            activity.Setup(c => c.SavedOutputMapping).Returns(string.Empty);
            activity.Setup(c => c.ResourceModel.WorkflowXaml).Returns(new StringBuilder(serviceDefStr));
            activity.Setup(c => c.UnderlyingWebActivityObjectType).Returns(typeof(DsfDatabaseActivity));
            activity.Setup(a => a.IsNotAvailable()).Returns(true);
            activity.Setup(a => a.ResourceModel).Returns(resourceModel.Object);
            //------------Execute Test---------------------------

            activityDataMappingBuilder.SetupActivityData(activity.Object);

            //------------Assert Results-------------------------

            Assert.AreEqual("Bob", activityDataMappingBuilder.ActivityInputDefinitions);
            Assert.AreEqual("Builder", activityDataMappingBuilder.ActivityOutputDefinitions);
           
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ActivityDataMappingBuilder_Generate")]
        public void ActivityDataMappingBuilder_SetupActivityData_WhenValidServiceDefintion_ExpectEmptySavedIODataAndCorrectActivityType()
        {
            //------------Setup for test--------------------------

            #region ServiceDef
            var serviceDefStr = @"
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
      
    </Action>";
            #endregion

            var activityDataMappingBuilder = new ActivityDataMappingBuilder();

            Mock<IContextualResourceModel> resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(c => c.DataList).Returns("<DataList/>");

            Mock<IWebActivity> activity = new Mock<IWebActivity>();

            activity.Setup(c => c.SavedInputMapping).Returns(string.Empty);
            activity.Setup(c => c.SavedOutputMapping).Returns(string.Empty);
            activity.Setup(c => c.ResourceModel.WorkflowXaml).Returns(new StringBuilder(serviceDefStr));
            activity.Setup(c => c.UnderlyingWebActivityObjectType).Returns(typeof(DsfDatabaseActivity));

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

            var inputString = @"<Inputs>
        <Input Name=""Rows"" Source=""Rows"" EmptyToNull=""false"" DefaultValue="""" />
      </Inputs>";

            var outputString = @"<Outputs>
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
      </Outputs>";
      
            #endregion

            var activityDataMappingBuilder = new ActivityDataMappingBuilder();

            Mock<IContextualResourceModel> resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(c => c.DataList).Returns("<DataList/>");

            Mock<IWebActivity> activity = new Mock<IWebActivity>();

            activity.Setup(c => c.SavedInputMapping).Returns(string.Empty);
            activity.Setup(c => c.SavedOutputMapping).Returns(string.Empty);
            activity.Setup(c => c.ResourceModel.Inputs).Returns(inputString);
            activity.Setup(c => c.ResourceModel.Outputs).Returns(outputString);
            activity.Setup(c => c.ResourceModel.WorkflowXaml).Returns(new StringBuilder());
            activity.Setup(c => c.UnderlyingWebActivityObjectType).Returns(typeof(DsfDatabaseActivity));
            activity.Setup(c => c.ResourceModel.ResourceType).Returns(ResourceType.Service);

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
        public void ActivityDataMappingBuilder_Generate_WhenValidServiceDefintionAndActivityHasSaveMappingData_ExpectNewDefaultAndIsRequiredTransfered()
        {
            //------------Setup for test--------------------------

            #region Setup Data

            var inputString = @"<Inputs>
        <Input Name=""Rows"" Source=""Rows"" DefaultValue=""5"" EmptyToNull=""true"">
            <Validator Type=""Required"" />
        </Input>
</Inputs>";

            var outputString = @"<Outputs>
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
      </Outputs>";

            var inputDefStr = @"<Inputs>
        <Input Name=""Rows"" Source=""[[RowCnt]]""/>
      </Inputs>";
      
            #endregion

            var activityDataMappingBuilder = new ActivityDataMappingBuilder();

            Mock<IContextualResourceModel> resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(c => c.DataList).Returns("<DataList/>");

            Mock<IWebActivity> activity = new Mock<IWebActivity>();

            activity.Setup(c => c.SavedInputMapping).Returns(inputDefStr);
            activity.Setup(c => c.SavedOutputMapping).Returns(string.Empty);
            activity.Setup(c => c.ResourceModel.Inputs).Returns(inputString);
            activity.Setup(c => c.ResourceModel.Outputs).Returns(outputString);
            activity.Setup(c => c.ResourceModel.WorkflowXaml).Returns(new StringBuilder());
            activity.Setup(c => c.UnderlyingWebActivityObjectType).Returns(typeof(DsfDatabaseActivity));
            activity.Setup(c => c.ResourceModel.ResourceType).Returns(ResourceType.Service);

            activityDataMappingBuilder.SetupActivityData(activity.Object);

            //------------Execute Test---------------------------

            var result = activityDataMappingBuilder.Generate();

            //------------Assert Results-------------------------

            // check counts first
            Assert.AreEqual(1, result.Inputs.Count);
            Assert.AreEqual(11, result.Outputs.Count);

            // now check data
            Assert.AreEqual("[[RowCnt]]", result.Inputs[0].Value);
            Assert.AreEqual("5", result.Inputs[0].DefaultValue);
            Assert.AreEqual(true, result.Inputs[0].Required);

        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ActivityDataMappingBuilder_Generate")]
        public void ActivityDataMappingBuilder_Generate_WhenValidServiceDefintionAndActivityHasSaveMappingData_ExpectValidInputAndOutputList()
        {
            //------------Setup for test--------------------------

            #region Setup Data

            var inputString = @"<Inputs>
        <Input Name=""Rows"" Source=""Rows"" EmptyToNull=""false"" DefaultValue="""" />
      </Inputs>";

            var outputString = @"<Outputs>
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
      </Outputs>";

            var outputDefStr = @"      <Outputs>
        <Output Name=""BigID"" MapsTo=""[[BigID]]"" Value=""[[Rowz().BigIDs]]"" Recordset=""Row"" />
        <Output Name=""Column1"" MapsTo=""[[Column1]]"" Value=""[[Rowz().Column1]]"" Recordset=""Row"" />
        <Output Name=""Column2"" MapsTo=""[[Column2]]"" Value=""[[Rowz().Column2]]"" Recordset=""Row"" />
        <Output Name=""Column3"" MapsTo=""[[Column3]]"" Value=""[[Rowz().Column3]]"" Recordset=""Row"" />
        <Output Name=""Column4"" MapsTo=""[[Column4]]"" Value=""[[Rowz().Column4]]"" Recordset=""Row"" />
        <Output Name=""Column5"" MapsTo=""[[Column5]]"" Value=""[[Rowz().Column5]]"" Recordset=""Row"" />
        <Output Name=""Column6"" MapsTo=""[[Column6]]"" Value=""[[Rowz().Column6]]"" Recordset=""Row"" />
        <Output Name=""Column7"" MapsTo=""[[Column7]]"" Value=""[[Rowz().Column7]]"" Recordset=""Row"" />
        <Output Name=""Column8"" MapsTo=""[[Column8]]"" Value=""[[Rowz().Column8]]"" Recordset=""Row"" />
        <Output Name=""Column9"" MapsTo=""[[Column9]]"" Value=""[[Rowz().Column9]]"" Recordset=""Row"" />
        <Output Name=""Column10"" MapsTo=""[[Column10]]"" Value=""[[Rowz().Column10]]"" Recordset=""Row"" />
      </Outputs>";

            var inputDefStr = @"<Inputs>
        <Input Name=""Rows"" Source=""[[RowCnt]]""/>
      </Inputs>";

            #endregion

            var activityDataMappingBuilder = new ActivityDataMappingBuilder();

            Mock<IContextualResourceModel> resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(c => c.DataList).Returns("<DataList/>");

            Mock<IWebActivity> activity = new Mock<IWebActivity>();

            activity.Setup(c => c.SavedInputMapping).Returns(inputDefStr);
            activity.Setup(c => c.SavedOutputMapping).Returns(outputDefStr);
            activity.Setup(c => c.ResourceModel.Inputs).Returns(inputString);
            activity.Setup(c => c.ResourceModel.Outputs).Returns(outputString);
            activity.Setup(c => c.ResourceModel.WorkflowXaml).Returns(new StringBuilder());
            activity.Setup(c => c.UnderlyingWebActivityObjectType).Returns(typeof(DsfDatabaseActivity));
            activity.Setup(c => c.ResourceModel.ResourceType).Returns(ResourceType.Service);

            activityDataMappingBuilder.SetupActivityData(activity.Object);

            //------------Execute Test---------------------------

            var result = activityDataMappingBuilder.Generate();

            //------------Assert Results-------------------------

            // check counts first
            Assert.AreEqual(1, result.Inputs.Count);
            Assert.AreEqual(11, result.Outputs.Count);

            // now check data
            Assert.AreEqual("[[RowCnt]]", result.Inputs[0].Value);

            Assert.AreEqual("[[Rowz().BigIDs]]", result.Outputs[0].Value);
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

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ActivityDataMappingBuilder_Generate")]
        public void ActivityDataMappingBuilder_Generate_WhenValidServiceDefintionAndActivityHasSaveMappingDataWithStaticValue_ExpectValidInputAndOutputList()
        {
            //------------Setup for test--------------------------

            #region Setup Data

            var outputDefStr = @"      <Outputs>
        <Output Name=""BigID"" MapsTo=""[[BigID]]"" Value=""[[Rowz().BigIDs]]"" Recordset=""Row"" />
        <Output Name=""Column1"" MapsTo=""[[Column1]]"" Value=""[[Rowz().Column1]]"" Recordset=""Row"" />
        <Output Name=""Column2"" MapsTo=""[[Column2]]"" Value=""[[Rowz().Column2]]"" Recordset=""Row"" />
        <Output Name=""Column3"" MapsTo=""[[Column3]]"" Value=""[[Rowz().Column3]]"" Recordset=""Row"" />
        <Output Name=""Column4"" MapsTo=""[[Column4]]"" Value=""[[Rowz().Column4]]"" Recordset=""Row"" />
        <Output Name=""Column5"" MapsTo=""[[Column5]]"" Value=""[[Rowz().Column5]]"" Recordset=""Row"" />
        <Output Name=""Column6"" MapsTo=""[[Column6]]"" Value=""[[Rowz().Column6]]"" Recordset=""Row"" />
        <Output Name=""Column7"" MapsTo=""[[Column7]]"" Value=""[[Rowz().Column7]]"" Recordset=""Row"" />
        <Output Name=""Column8"" MapsTo=""[[Column8]]"" Value=""[[Rowz().Column8]]"" Recordset=""Row"" />
        <Output Name=""Column9"" MapsTo=""[[Column9]]"" Value=""[[Rowz().Column9]]"" Recordset=""Row"" />
        <Output Name=""Column10"" MapsTo=""[[Column10]]"" Value=""[[Rowz().Column10]]"" Recordset=""Row"" />
      </Outputs>";

            var inputDefStr = @"<Inputs>
        <Input Name=""Rows"" Source=""5""/>
      </Inputs>";

            var inputString = @"<Inputs>
        <Input Name=""Rows"" Source=""Rows"" EmptyToNull=""false"" DefaultValue="""" />
      </Inputs>";
            var outputString = @"<Outputs>
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
      </Outputs>";
      
            #endregion

            var activityDataMappingBuilder = new ActivityDataMappingBuilder();

            Mock<IContextualResourceModel> resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(c => c.DataList).Returns("<DataList/>");

            Mock<IWebActivity> activity = new Mock<IWebActivity>();

            activity.Setup(c => c.SavedInputMapping).Returns(inputDefStr);
            activity.Setup(c => c.SavedOutputMapping).Returns(outputDefStr);
            activity.Setup(c => c.ResourceModel.WorkflowXaml).Returns(new StringBuilder());
            activity.Setup(c => c.ResourceModel.Inputs).Returns(inputString);
            activity.Setup(c => c.ResourceModel.Outputs).Returns(outputString);
            activity.Setup(c => c.UnderlyingWebActivityObjectType).Returns(typeof(DsfDatabaseActivity));
            activity.Setup(c => c.ResourceModel.ResourceType).Returns(ResourceType.Service);

            activityDataMappingBuilder.SetupActivityData(activity.Object);

            //------------Execute Test---------------------------

            var result = activityDataMappingBuilder.Generate();

            //------------Assert Results-------------------------

            // check counts first
            Assert.AreEqual(1, result.Inputs.Count);
            Assert.AreEqual(11, result.Outputs.Count);

            // now check data
            Assert.AreEqual("5", result.Inputs[0].Value);

            Assert.AreEqual("[[Rowz().BigIDs]]", result.Outputs[0].Value);
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


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ActivityDataMappingBuilder_Generate")]
        public void ActivityDataMappingBuilder_Generate_WhenValidWorkflow_ExpectValidOutputList()
        {
            //------------Setup for test--------------------------

            #region ServiceDef
            var serviceDefStr = @"<Service ID=""8912e8db-074f-43e4-85ea-9376162d3332"" Version=""1.0"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"" Name=""fileTest"" ResourceType=""WorkflowService"" IsValid=""true"">
  <DisplayName>fileTest</DisplayName>
  <Category>Mo</Category>
  <IsNewWorkflow>false</IsNewWorkflow>
  <AuthorRoles>Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Folder Redirection Accounts,All Users,Windows SBS SharePoint_MembersGroup,Windows SBS Link Users,Company Users,Business Design Studio Developers,Test Engineers,DEV2 Limited Internet Access,</AuthorRoles>
  <Comment>a</Comment>
  <Tags></Tags>
  <IconPath>pack://application:,,,/Warewolf Studio;component/images/Workflow-32.png</IconPath>
  <HelpLink>a:/</HelpLink>
  <UnitTestTargetWorkflowService></UnitTestTargetWorkflowService>
  <DataList>
    <result Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
    <recset1 Description="""" IsEditable=""True"" ColumnIODirection=""None"">
      <f1 Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    </recset1>
    <recset2 Description="""" IsEditable=""True"" ColumnIODirection=""None"">
      <f2 Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    </recset2>
  </DataList>
  <Action Name=""InvokeWorkflow"" Type=""Workflow"">
    <XamlDefinition>&lt;Activity x:Class=""fileTest"" xmlns=""http://schemas.microsoft.com/netfx/2009/xaml/activities"" xmlns:av=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:dc=""clr-namespace:Dev2.Common;assembly=Dev2.Common"" xmlns:ddc=""clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data"" xmlns:ddcb=""clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data"" xmlns:ddd=""clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data"" xmlns:dddo=""clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data"" xmlns:ddsm=""clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data"" xmlns:dpe=""clr-namespace:Dev2.Providers.Errors;assembly=Dev2.Infrastructure"" xmlns:mva=""clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities"" xmlns:s=""clr-namespace:System;assembly=mscorlib"" xmlns:sads=""http://schemas.microsoft.com/netfx/2010/xaml/activities/debugger"" xmlns:sap=""http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation"" xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=mscorlib"" xmlns:sco=""clr-namespace:System.Collections.ObjectModel;assembly=mscorlib"" xmlns:uaba=""clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities"" xmlns:uf=""clr-namespace:Unlimited.Framework;assembly=Dev2.Core"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""&gt;&lt;x:Members&gt;&lt;x:Property Name=""AmbientDataList"" Type=""InOutArgument(scg:List(x:String))"" /&gt;&lt;x:Property Name=""ParentWorkflowInstanceId"" Type=""InOutArgument(s:Guid)"" /&gt;&lt;x:Property Name=""ParentServiceName"" Type=""InOutArgument(x:String)"" /&gt;&lt;/x:Members&gt;&lt;sap:VirtualizedContainerService.HintSize&gt;719,676&lt;/sap:VirtualizedContainerService.HintSize&gt;&lt;mva:VisualBasic.Settings&gt;Assembly references and imported namespaces serialized as XML namespaces&lt;/mva:VisualBasic.Settings&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""7""&gt;&lt;x:String&gt;Dev2.Common&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Framework&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco:Collection x:TypeArguments=""AssemblyReference""&gt;&lt;AssemblyReference&gt;Dev2.Common&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Core&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=""fileTest"" sap:VirtualizedContainerService.HintSize=""679,636"" mva:VisualBasic.Settings=""Assembly references and imported namespaces serialized as XML namespaces""&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=""scg:List(x:String)"" Name=""InstructionList"" /&gt;&lt;Variable x:TypeArguments=""x:String"" Name=""LastResult"" /&gt;&lt;Variable x:TypeArguments=""x:Boolean"" Name=""HasError"" /&gt;&lt;Variable x:TypeArguments=""x:String"" Name=""ExplicitDataList"" /&gt;&lt;Variable x:TypeArguments=""x:Boolean"" Name=""IsValid"" /&gt;&lt;Variable x:TypeArguments=""uf:UnlimitedObject"" Name=""d"" /&gt;&lt;Variable x:TypeArguments=""uaba:Util"" Name=""t"" /&gt;&lt;Variable x:TypeArguments=""ddd:Dev2DataListDecisionHandler"" Name=""Dev2DecisionHandler"" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;60,75&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;300,77.5 300,133 313.395,133&lt;/av:PointCollection&gt;&lt;x:Double x:Key=""Width""&gt;665&lt;/x:Double&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Reference&gt;__ReferenceID1&lt;/x:Reference&gt;&lt;/Flowchart.StartNode&gt;&lt;FlowStep x:Name=""__ReferenceID0""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;135,243.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;264,116&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfDataMergeActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DisplayName=""Data Merge (1)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""264,116"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" Result=""[[sdfsdf]]"" SimulationMode=""OnDemand"" UniqueID=""99f5593a-0f6f-4f8b-a34f-bf93190e14c6""&gt;&lt;uaba:DsfDataMergeActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfDataMergeActivity.AmbientDataList&gt;&lt;uaba:DsfDataMergeActivity.MergeCollection&gt;&lt;scg:List x:TypeArguments=""uaba:DataMergeDTO"" Capacity=""4""&gt;&lt;uaba:DataMergeDTO Alignment=""Left"" At="""" EnableAt=""False"" IndexNumber=""1"" InputVariable=""sdsd"" Inserted=""False"" MergeType=""New Line"" Padding="""" WatermarkTextVariable=""[[Recordset().F1]]"" /&gt;&lt;uaba:DataMergeDTO Alignment=""Left"" At="""" EnableAt=""False"" IndexNumber=""2"" InputVariable="""" Inserted=""False"" MergeType=""None"" Padding="""" WatermarkTextVariable=""[[Recordset().F2]]"" /&gt;&lt;/scg:List&gt;&lt;/uaba:DsfDataMergeActivity.MergeCollection&gt;&lt;uaba:DsfDataMergeActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfDataMergeActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfDataMergeActivity&gt;&lt;/FlowStep&gt;&lt;FlowStep x:Name=""__ReferenceID1""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;313.395,94&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;230,78&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;428.395,172 428.395,301.5 399,301.5&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfMultiAssignActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign (6)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""230,78"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""2cc98df1-da95-421d-b413-98ff091f7397"" UpdateAllOccurrences=""False""&gt;&lt;uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;scg:List x:TypeArguments=""uaba:ActivityDTO"" Capacity=""8""&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset1(1).f1]]"" FieldValue=""test1"" IndexNumber=""1"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable1]]""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset2(1).f2]]"" FieldValue=""test2"" IndexNumber=""2"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable2]]""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset1(2).f1]]"" FieldValue=""test3"" IndexNumber=""3"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset2(2).f2]]"" FieldValue=""test4"" IndexNumber=""4"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset1(3).f1]]"" FieldValue=""test5"" IndexNumber=""5"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset2(3).f2]]"" FieldValue=""test6"" IndexNumber=""6"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName="""" FieldValue="""" IndexNumber=""7"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;/scg:List&gt;&lt;/uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfMultiAssignActivity&gt;&lt;FlowStep.Next&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/FlowStep.Next&gt;&lt;/FlowStep&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition>
  </Action>
  <ErrorMessages />
  <Source />
  <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
    <SignedInfo>
      <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />
      <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
      <Reference URI="""">
        <Transforms>
          <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
        </Transforms>
        <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
        <DigestValue>VWE/gfMxoDaAnF2QgBKVXjvfTVs=</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>bH2cevYAj3616fNuu55cKdl4pehbJN/lcqTXJPdfRXBhxrJ/iMbMRX/sU03mzycS323KU/2sEyLfQYUYOZh0EHcKD4Bchny+/I04n+PxDbtdGJe9QM561vBQZ6g6fUnQB63lh2uNneQQ8nd+sQ4JY4/C2v6CbPTmEryuFJTclUQ=</SignatureValue>
  </Signature>
</Service>";

            var datalistFragment = @"<DataList>
    <result Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
    <recset1 Description="""" IsEditable=""True"" ColumnIODirection=""None"">
      <f1 Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    </recset1>
    <recset2 Description="""" IsEditable=""True"" ColumnIODirection=""None"">
      <f2 Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    </recset2>
  </DataList>";

            #endregion

            var activityDataMappingBuilder = new ActivityDataMappingBuilder();

            Mock<IContextualResourceModel> resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(c => c.DataList).Returns("<DataList/>");

            Mock<IWebActivity> activity = new Mock<IWebActivity>();

            activity.Setup(c => c.SavedInputMapping).Returns(string.Empty);
            activity.Setup(c => c.SavedOutputMapping).Returns(string.Empty);
            activity.Setup(c => c.ResourceModel.WorkflowXaml).Returns(new StringBuilder(serviceDefStr));
            activity.Setup(c => c.UnderlyingWebActivityObjectType).Returns(typeof(DsfActivity));
            activity.Setup(c => c.ResourceModel.DataList).Returns(datalistFragment);

            activityDataMappingBuilder.SetupActivityData(activity.Object);

            //------------Execute Test---------------------------

            var result = activityDataMappingBuilder.Generate();

            //------------Assert Results-------------------------

            // check counts first
            Assert.AreEqual(0, result.Inputs.Count);
            Assert.AreEqual(1, result.Outputs.Count);

            // now check data
            Assert.AreEqual("[[result]]", result.Outputs[0].Value);

        }

        // NEW
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ActivityDataMappingBuilder_Generate")]
        public void ActivityDataMappingBuilder_Generate_WhenValidWorkflowWithDataListMatchingColumn_ExpectValidOutputList()
        {
            //------------Setup for test--------------------------

            #region ServiceDef
            var serviceDefStr = @"<Service ID=""8912e8db-074f-43e4-85ea-9376162d3332"" Version=""1.0"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"" Name=""fileTest"" ResourceType=""WorkflowService"" IsValid=""true"">
  <DisplayName>fileTest</DisplayName>
  <Category>Mo</Category>
  <IsNewWorkflow>false</IsNewWorkflow>
  <AuthorRoles>Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Folder Redirection Accounts,All Users,Windows SBS SharePoint_MembersGroup,Windows SBS Link Users,Company Users,Business Design Studio Developers,Test Engineers,DEV2 Limited Internet Access,</AuthorRoles>
  <Comment>a</Comment>
  <Tags></Tags>
  <IconPath>pack://application:,,,/Warewolf Studio;component/images/Workflow-32.png</IconPath>
  <HelpLink>a:/</HelpLink>
  <UnitTestTargetWorkflowService></UnitTestTargetWorkflowService>
  <DataList>
    <result Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
    <recset1 Description="""" IsEditable=""True"" ColumnIODirection=""Output"">
      <f1 Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
    </recset1>
    <recset2 Description="""" IsEditable=""True"" ColumnIODirection=""None"">
      <f2 Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    </recset2>
  </DataList>
  <Action Name=""InvokeWorkflow"" Type=""Workflow"">
    <XamlDefinition>&lt;Activity x:Class=""fileTest"" xmlns=""http://schemas.microsoft.com/netfx/2009/xaml/activities"" xmlns:av=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:dc=""clr-namespace:Dev2.Common;assembly=Dev2.Common"" xmlns:ddc=""clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data"" xmlns:ddcb=""clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data"" xmlns:ddd=""clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data"" xmlns:dddo=""clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data"" xmlns:ddsm=""clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data"" xmlns:dpe=""clr-namespace:Dev2.Providers.Errors;assembly=Dev2.Infrastructure"" xmlns:mva=""clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities"" xmlns:s=""clr-namespace:System;assembly=mscorlib"" xmlns:sads=""http://schemas.microsoft.com/netfx/2010/xaml/activities/debugger"" xmlns:sap=""http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation"" xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=mscorlib"" xmlns:sco=""clr-namespace:System.Collections.ObjectModel;assembly=mscorlib"" xmlns:uaba=""clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities"" xmlns:uf=""clr-namespace:Unlimited.Framework;assembly=Dev2.Core"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""&gt;&lt;x:Members&gt;&lt;x:Property Name=""AmbientDataList"" Type=""InOutArgument(scg:List(x:String))"" /&gt;&lt;x:Property Name=""ParentWorkflowInstanceId"" Type=""InOutArgument(s:Guid)"" /&gt;&lt;x:Property Name=""ParentServiceName"" Type=""InOutArgument(x:String)"" /&gt;&lt;/x:Members&gt;&lt;sap:VirtualizedContainerService.HintSize&gt;719,676&lt;/sap:VirtualizedContainerService.HintSize&gt;&lt;mva:VisualBasic.Settings&gt;Assembly references and imported namespaces serialized as XML namespaces&lt;/mva:VisualBasic.Settings&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""7""&gt;&lt;x:String&gt;Dev2.Common&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Framework&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco:Collection x:TypeArguments=""AssemblyReference""&gt;&lt;AssemblyReference&gt;Dev2.Common&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Core&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=""fileTest"" sap:VirtualizedContainerService.HintSize=""679,636"" mva:VisualBasic.Settings=""Assembly references and imported namespaces serialized as XML namespaces""&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=""scg:List(x:String)"" Name=""InstructionList"" /&gt;&lt;Variable x:TypeArguments=""x:String"" Name=""LastResult"" /&gt;&lt;Variable x:TypeArguments=""x:Boolean"" Name=""HasError"" /&gt;&lt;Variable x:TypeArguments=""x:String"" Name=""ExplicitDataList"" /&gt;&lt;Variable x:TypeArguments=""x:Boolean"" Name=""IsValid"" /&gt;&lt;Variable x:TypeArguments=""uf:UnlimitedObject"" Name=""d"" /&gt;&lt;Variable x:TypeArguments=""uaba:Util"" Name=""t"" /&gt;&lt;Variable x:TypeArguments=""ddd:Dev2DataListDecisionHandler"" Name=""Dev2DecisionHandler"" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;60,75&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;300,77.5 300,133 313.395,133&lt;/av:PointCollection&gt;&lt;x:Double x:Key=""Width""&gt;665&lt;/x:Double&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Reference&gt;__ReferenceID1&lt;/x:Reference&gt;&lt;/Flowchart.StartNode&gt;&lt;FlowStep x:Name=""__ReferenceID0""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;135,243.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;264,116&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfDataMergeActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DisplayName=""Data Merge (1)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""264,116"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" Result=""[[sdfsdf]]"" SimulationMode=""OnDemand"" UniqueID=""99f5593a-0f6f-4f8b-a34f-bf93190e14c6""&gt;&lt;uaba:DsfDataMergeActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfDataMergeActivity.AmbientDataList&gt;&lt;uaba:DsfDataMergeActivity.MergeCollection&gt;&lt;scg:List x:TypeArguments=""uaba:DataMergeDTO"" Capacity=""4""&gt;&lt;uaba:DataMergeDTO Alignment=""Left"" At="""" EnableAt=""False"" IndexNumber=""1"" InputVariable=""sdsd"" Inserted=""False"" MergeType=""New Line"" Padding="""" WatermarkTextVariable=""[[Recordset().F1]]"" /&gt;&lt;uaba:DataMergeDTO Alignment=""Left"" At="""" EnableAt=""False"" IndexNumber=""2"" InputVariable="""" Inserted=""False"" MergeType=""None"" Padding="""" WatermarkTextVariable=""[[Recordset().F2]]"" /&gt;&lt;/scg:List&gt;&lt;/uaba:DsfDataMergeActivity.MergeCollection&gt;&lt;uaba:DsfDataMergeActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfDataMergeActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfDataMergeActivity&gt;&lt;/FlowStep&gt;&lt;FlowStep x:Name=""__ReferenceID1""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;313.395,94&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;230,78&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;428.395,172 428.395,301.5 399,301.5&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfMultiAssignActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign (6)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""230,78"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""2cc98df1-da95-421d-b413-98ff091f7397"" UpdateAllOccurrences=""False""&gt;&lt;uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;scg:List x:TypeArguments=""uaba:ActivityDTO"" Capacity=""8""&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset1(1).f1]]"" FieldValue=""test1"" IndexNumber=""1"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable1]]""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset2(1).f2]]"" FieldValue=""test2"" IndexNumber=""2"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable2]]""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset1(2).f1]]"" FieldValue=""test3"" IndexNumber=""3"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset2(2).f2]]"" FieldValue=""test4"" IndexNumber=""4"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset1(3).f1]]"" FieldValue=""test5"" IndexNumber=""5"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset2(3).f2]]"" FieldValue=""test6"" IndexNumber=""6"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName="""" FieldValue="""" IndexNumber=""7"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;/scg:List&gt;&lt;/uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfMultiAssignActivity&gt;&lt;FlowStep.Next&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/FlowStep.Next&gt;&lt;/FlowStep&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition>
  </Action>
  <ErrorMessages />
  <Source />
  <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
    <SignedInfo>
      <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />
      <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
      <Reference URI="""">
        <Transforms>
          <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
        </Transforms>
        <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
        <DigestValue>VWE/gfMxoDaAnF2QgBKVXjvfTVs=</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>bH2cevYAj3616fNuu55cKdl4pehbJN/lcqTXJPdfRXBhxrJ/iMbMRX/sU03mzycS323KU/2sEyLfQYUYOZh0EHcKD4Bchny+/I04n+PxDbtdGJe9QM561vBQZ6g6fUnQB63lh2uNneQQ8nd+sQ4JY4/C2v6CbPTmEryuFJTclUQ=</SignatureValue>
  </Signature>
</Service>";

            var datalistFragment = @"<DataList>
    <result Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
    <recordSet Description="""" IsEditable=""True"" ColumnIODirection=""Output"">
      <f1 Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
    </recordSet>
    <recset2 Description="""" IsEditable=""True"" ColumnIODirection=""None"">
      <f2 Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    </recset2>
  </DataList>";

            #endregion

            var activityDataMappingBuilder = new ActivityDataMappingBuilder();

            Mock<IContextualResourceModel> resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(c => c.DataList).Returns("<DataList/>");

            Mock<IWebActivity> activity = new Mock<IWebActivity>();

            activity.Setup(c => c.SavedInputMapping).Returns(string.Empty);
            activity.Setup(c => c.SavedOutputMapping).Returns(string.Empty);
            activity.Setup(c => c.ResourceModel.WorkflowXaml).Returns(new StringBuilder(serviceDefStr));
            activity.Setup(c => c.UnderlyingWebActivityObjectType).Returns(typeof(DsfActivity));
            activity.Setup(c => c.ResourceModel.DataList).Returns(datalistFragment);

            activityDataMappingBuilder.SetupActivityData(activity.Object);

            //------------Execute Test---------------------------

            var result = activityDataMappingBuilder.Generate();

            //------------Assert Results-------------------------

            // check counts first
            Assert.AreEqual(0, result.Inputs.Count);
            Assert.AreEqual(2, result.Outputs.Count);

            // now check data
            Assert.AreEqual("[[result]]", result.Outputs[0].Value);
            Assert.AreEqual("[[recordSet().f1]]", result.Outputs[1].Value);

        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ActivityDataMappingBuilder_Generate")]
        public void ActivityDataMappingBuilder_Generate_WhenValidWorkflow_ExpectValidInputList()
        {
            //------------Setup for test--------------------------

            #region ServiceDef
            var serviceDefStr = @"<Service ID=""8912e8db-074f-43e4-85ea-9376162d3332"" Version=""1.0"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"" Name=""fileTest"" ResourceType=""WorkflowService"" IsValid=""true"">
  <DisplayName>fileTest</DisplayName>
  <Category>Mo</Category>
  <IsNewWorkflow>false</IsNewWorkflow>
  <AuthorRoles>Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Folder Redirection Accounts,All Users,Windows SBS SharePoint_MembersGroup,Windows SBS Link Users,Company Users,Business Design Studio Developers,Test Engineers,DEV2 Limited Internet Access,</AuthorRoles>
  <Comment>a</Comment>
  <Tags></Tags>
  <IconPath>pack://application:,,,/Warewolf Studio;component/images/Workflow-32.png</IconPath>
  <HelpLink>a:/</HelpLink>
  <UnitTestTargetWorkflowService></UnitTestTargetWorkflowService>
  <DataList>
    <result Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
    <recset1 Description="""" IsEditable=""True"" ColumnIODirection=""None"">
      <f1 Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    </recset1>
    <recset2 Description="""" IsEditable=""True"" ColumnIODirection=""None"">
      <f2 Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    </recset2>
  </DataList>
  <Action Name=""InvokeWorkflow"" Type=""Workflow"">
    <XamlDefinition>&lt;Activity x:Class=""fileTest"" xmlns=""http://schemas.microsoft.com/netfx/2009/xaml/activities"" xmlns:av=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:dc=""clr-namespace:Dev2.Common;assembly=Dev2.Common"" xmlns:ddc=""clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data"" xmlns:ddcb=""clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data"" xmlns:ddd=""clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data"" xmlns:dddo=""clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data"" xmlns:ddsm=""clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data"" xmlns:dpe=""clr-namespace:Dev2.Providers.Errors;assembly=Dev2.Infrastructure"" xmlns:mva=""clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities"" xmlns:s=""clr-namespace:System;assembly=mscorlib"" xmlns:sads=""http://schemas.microsoft.com/netfx/2010/xaml/activities/debugger"" xmlns:sap=""http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation"" xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=mscorlib"" xmlns:sco=""clr-namespace:System.Collections.ObjectModel;assembly=mscorlib"" xmlns:uaba=""clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities"" xmlns:uf=""clr-namespace:Unlimited.Framework;assembly=Dev2.Core"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""&gt;&lt;x:Members&gt;&lt;x:Property Name=""AmbientDataList"" Type=""InOutArgument(scg:List(x:String))"" /&gt;&lt;x:Property Name=""ParentWorkflowInstanceId"" Type=""InOutArgument(s:Guid)"" /&gt;&lt;x:Property Name=""ParentServiceName"" Type=""InOutArgument(x:String)"" /&gt;&lt;/x:Members&gt;&lt;sap:VirtualizedContainerService.HintSize&gt;719,676&lt;/sap:VirtualizedContainerService.HintSize&gt;&lt;mva:VisualBasic.Settings&gt;Assembly references and imported namespaces serialized as XML namespaces&lt;/mva:VisualBasic.Settings&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""7""&gt;&lt;x:String&gt;Dev2.Common&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Framework&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco:Collection x:TypeArguments=""AssemblyReference""&gt;&lt;AssemblyReference&gt;Dev2.Common&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Core&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=""fileTest"" sap:VirtualizedContainerService.HintSize=""679,636"" mva:VisualBasic.Settings=""Assembly references and imported namespaces serialized as XML namespaces""&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=""scg:List(x:String)"" Name=""InstructionList"" /&gt;&lt;Variable x:TypeArguments=""x:String"" Name=""LastResult"" /&gt;&lt;Variable x:TypeArguments=""x:Boolean"" Name=""HasError"" /&gt;&lt;Variable x:TypeArguments=""x:String"" Name=""ExplicitDataList"" /&gt;&lt;Variable x:TypeArguments=""x:Boolean"" Name=""IsValid"" /&gt;&lt;Variable x:TypeArguments=""uf:UnlimitedObject"" Name=""d"" /&gt;&lt;Variable x:TypeArguments=""uaba:Util"" Name=""t"" /&gt;&lt;Variable x:TypeArguments=""ddd:Dev2DataListDecisionHandler"" Name=""Dev2DecisionHandler"" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;60,75&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;300,77.5 300,133 313.395,133&lt;/av:PointCollection&gt;&lt;x:Double x:Key=""Width""&gt;665&lt;/x:Double&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Reference&gt;__ReferenceID1&lt;/x:Reference&gt;&lt;/Flowchart.StartNode&gt;&lt;FlowStep x:Name=""__ReferenceID0""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;135,243.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;264,116&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfDataMergeActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DisplayName=""Data Merge (1)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""264,116"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" Result=""[[sdfsdf]]"" SimulationMode=""OnDemand"" UniqueID=""99f5593a-0f6f-4f8b-a34f-bf93190e14c6""&gt;&lt;uaba:DsfDataMergeActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfDataMergeActivity.AmbientDataList&gt;&lt;uaba:DsfDataMergeActivity.MergeCollection&gt;&lt;scg:List x:TypeArguments=""uaba:DataMergeDTO"" Capacity=""4""&gt;&lt;uaba:DataMergeDTO Alignment=""Left"" At="""" EnableAt=""False"" IndexNumber=""1"" InputVariable=""sdsd"" Inserted=""False"" MergeType=""New Line"" Padding="""" WatermarkTextVariable=""[[Recordset().F1]]"" /&gt;&lt;uaba:DataMergeDTO Alignment=""Left"" At="""" EnableAt=""False"" IndexNumber=""2"" InputVariable="""" Inserted=""False"" MergeType=""None"" Padding="""" WatermarkTextVariable=""[[Recordset().F2]]"" /&gt;&lt;/scg:List&gt;&lt;/uaba:DsfDataMergeActivity.MergeCollection&gt;&lt;uaba:DsfDataMergeActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfDataMergeActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfDataMergeActivity&gt;&lt;/FlowStep&gt;&lt;FlowStep x:Name=""__ReferenceID1""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;313.395,94&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;230,78&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;428.395,172 428.395,301.5 399,301.5&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfMultiAssignActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign (6)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""230,78"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""2cc98df1-da95-421d-b413-98ff091f7397"" UpdateAllOccurrences=""False""&gt;&lt;uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;scg:List x:TypeArguments=""uaba:ActivityDTO"" Capacity=""8""&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset1(1).f1]]"" FieldValue=""test1"" IndexNumber=""1"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable1]]""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset2(1).f2]]"" FieldValue=""test2"" IndexNumber=""2"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable2]]""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset1(2).f1]]"" FieldValue=""test3"" IndexNumber=""3"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset2(2).f2]]"" FieldValue=""test4"" IndexNumber=""4"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset1(3).f1]]"" FieldValue=""test5"" IndexNumber=""5"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset2(3).f2]]"" FieldValue=""test6"" IndexNumber=""6"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName="""" FieldValue="""" IndexNumber=""7"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;/scg:List&gt;&lt;/uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfMultiAssignActivity&gt;&lt;FlowStep.Next&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/FlowStep.Next&gt;&lt;/FlowStep&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition>
  </Action>
  <ErrorMessages />
  <Source />
  <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
    <SignedInfo>
      <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />
      <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
      <Reference URI="""">
        <Transforms>
          <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
        </Transforms>
        <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
        <DigestValue>VWE/gfMxoDaAnF2QgBKVXjvfTVs=</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>bH2cevYAj3616fNuu55cKdl4pehbJN/lcqTXJPdfRXBhxrJ/iMbMRX/sU03mzycS323KU/2sEyLfQYUYOZh0EHcKD4Bchny+/I04n+PxDbtdGJe9QM561vBQZ6g6fUnQB63lh2uNneQQ8nd+sQ4JY4/C2v6CbPTmEryuFJTclUQ=</SignatureValue>
  </Signature>
</Service>";

            var datalistFragment = @"<DataList>
    <result Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
    <recset1 Description="""" IsEditable=""True"" ColumnIODirection=""None"">
      <f1 Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    </recset1>
    <recset2 Description="""" IsEditable=""True"" ColumnIODirection=""None"">
      <f2 Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    </recset2>
  </DataList>";

            #endregion

            var activityDataMappingBuilder = new ActivityDataMappingBuilder();

            Mock<IContextualResourceModel> resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(c => c.DataList).Returns("<DataList/>");

            Mock<IWebActivity> activity = new Mock<IWebActivity>();

            activity.Setup(c => c.SavedInputMapping).Returns(string.Empty);
            activity.Setup(c => c.SavedOutputMapping).Returns(string.Empty);
            activity.Setup(c => c.ResourceModel.WorkflowXaml).Returns(new StringBuilder(serviceDefStr));
            activity.Setup(c => c.UnderlyingWebActivityObjectType).Returns(typeof(DsfActivity));
            activity.Setup(c => c.ResourceModel.DataList).Returns(datalistFragment);

            activityDataMappingBuilder.SetupActivityData(activity.Object);

            //------------Execute Test---------------------------

            var result = activityDataMappingBuilder.Generate();

            //------------Assert Results-------------------------

            // check counts first
            Assert.AreEqual(1, result.Inputs.Count);
            Assert.AreEqual(0, result.Outputs.Count);

            // now check data
            Assert.AreEqual("[[result]]", result.Inputs[0].Value);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ActivityDataMappingBuilder_Generate")]
        public void ActivityDataMappingBuilder_Generate_WhenValidWorkflow_ExpectValidInputAndOutputList()
        {
            //------------Setup for test--------------------------

            #region ServiceDef

            var serviceDefStr = @"<Service ID=""8912e8db-074f-43e4-85ea-9376162d3332"" Version=""1.0"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"" Name=""fileTest"" ResourceType=""WorkflowService"" IsValid=""true"">
  <DisplayName>fileTest</DisplayName>
  <Category>Mo</Category>
  <IsNewWorkflow>false</IsNewWorkflow>
  <AuthorRoles>Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Folder Redirection Accounts,All Users,Windows SBS SharePoint_MembersGroup,Windows SBS Link Users,Company Users,Business Design Studio Developers,Test Engineers,DEV2 Limited Internet Access,</AuthorRoles>
  <Comment>a</Comment>
  <Tags></Tags>
  <IconPath>pack://application:,,,/Warewolf Studio;component/images/Workflow-32.png</IconPath>
  <HelpLink>a:/</HelpLink>
  <UnitTestTargetWorkflowService></UnitTestTargetWorkflowService>
  <DataList>
    <result Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
    <recset1 Description="""" IsEditable=""True"" ColumnIODirection=""None"">
      <f1 Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    </recset1>
    <recset2 Description="""" IsEditable=""True"" ColumnIODirection=""None"">
      <f2 Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    </recset2>
  </DataList>
  <Action Name=""InvokeWorkflow"" Type=""Workflow"">
    <XamlDefinition>&lt;Activity x:Class=""fileTest"" xmlns=""http://schemas.microsoft.com/netfx/2009/xaml/activities"" xmlns:av=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:dc=""clr-namespace:Dev2.Common;assembly=Dev2.Common"" xmlns:ddc=""clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data"" xmlns:ddcb=""clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data"" xmlns:ddd=""clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data"" xmlns:dddo=""clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data"" xmlns:ddsm=""clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data"" xmlns:dpe=""clr-namespace:Dev2.Providers.Errors;assembly=Dev2.Infrastructure"" xmlns:mva=""clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities"" xmlns:s=""clr-namespace:System;assembly=mscorlib"" xmlns:sads=""http://schemas.microsoft.com/netfx/2010/xaml/activities/debugger"" xmlns:sap=""http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation"" xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=mscorlib"" xmlns:sco=""clr-namespace:System.Collections.ObjectModel;assembly=mscorlib"" xmlns:uaba=""clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities"" xmlns:uf=""clr-namespace:Unlimited.Framework;assembly=Dev2.Core"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""&gt;&lt;x:Members&gt;&lt;x:Property Name=""AmbientDataList"" Type=""InOutArgument(scg:List(x:String))"" /&gt;&lt;x:Property Name=""ParentWorkflowInstanceId"" Type=""InOutArgument(s:Guid)"" /&gt;&lt;x:Property Name=""ParentServiceName"" Type=""InOutArgument(x:String)"" /&gt;&lt;/x:Members&gt;&lt;sap:VirtualizedContainerService.HintSize&gt;719,676&lt;/sap:VirtualizedContainerService.HintSize&gt;&lt;mva:VisualBasic.Settings&gt;Assembly references and imported namespaces serialized as XML namespaces&lt;/mva:VisualBasic.Settings&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""7""&gt;&lt;x:String&gt;Dev2.Common&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Framework&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco:Collection x:TypeArguments=""AssemblyReference""&gt;&lt;AssemblyReference&gt;Dev2.Common&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Core&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=""fileTest"" sap:VirtualizedContainerService.HintSize=""679,636"" mva:VisualBasic.Settings=""Assembly references and imported namespaces serialized as XML namespaces""&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=""scg:List(x:String)"" Name=""InstructionList"" /&gt;&lt;Variable x:TypeArguments=""x:String"" Name=""LastResult"" /&gt;&lt;Variable x:TypeArguments=""x:Boolean"" Name=""HasError"" /&gt;&lt;Variable x:TypeArguments=""x:String"" Name=""ExplicitDataList"" /&gt;&lt;Variable x:TypeArguments=""x:Boolean"" Name=""IsValid"" /&gt;&lt;Variable x:TypeArguments=""uf:UnlimitedObject"" Name=""d"" /&gt;&lt;Variable x:TypeArguments=""uaba:Util"" Name=""t"" /&gt;&lt;Variable x:TypeArguments=""ddd:Dev2DataListDecisionHandler"" Name=""Dev2DecisionHandler"" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;60,75&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;300,77.5 300,133 313.395,133&lt;/av:PointCollection&gt;&lt;x:Double x:Key=""Width""&gt;665&lt;/x:Double&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Reference&gt;__ReferenceID1&lt;/x:Reference&gt;&lt;/Flowchart.StartNode&gt;&lt;FlowStep x:Name=""__ReferenceID0""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;135,243.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;264,116&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfDataMergeActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DisplayName=""Data Merge (1)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""264,116"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" Result=""[[sdfsdf]]"" SimulationMode=""OnDemand"" UniqueID=""99f5593a-0f6f-4f8b-a34f-bf93190e14c6""&gt;&lt;uaba:DsfDataMergeActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfDataMergeActivity.AmbientDataList&gt;&lt;uaba:DsfDataMergeActivity.MergeCollection&gt;&lt;scg:List x:TypeArguments=""uaba:DataMergeDTO"" Capacity=""4""&gt;&lt;uaba:DataMergeDTO Alignment=""Left"" At="""" EnableAt=""False"" IndexNumber=""1"" InputVariable=""sdsd"" Inserted=""False"" MergeType=""New Line"" Padding="""" WatermarkTextVariable=""[[Recordset().F1]]"" /&gt;&lt;uaba:DataMergeDTO Alignment=""Left"" At="""" EnableAt=""False"" IndexNumber=""2"" InputVariable="""" Inserted=""False"" MergeType=""None"" Padding="""" WatermarkTextVariable=""[[Recordset().F2]]"" /&gt;&lt;/scg:List&gt;&lt;/uaba:DsfDataMergeActivity.MergeCollection&gt;&lt;uaba:DsfDataMergeActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfDataMergeActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfDataMergeActivity&gt;&lt;/FlowStep&gt;&lt;FlowStep x:Name=""__ReferenceID1""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;313.395,94&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;230,78&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;428.395,172 428.395,301.5 399,301.5&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfMultiAssignActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign (6)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""230,78"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""2cc98df1-da95-421d-b413-98ff091f7397"" UpdateAllOccurrences=""False""&gt;&lt;uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;scg:List x:TypeArguments=""uaba:ActivityDTO"" Capacity=""8""&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset1(1).f1]]"" FieldValue=""test1"" IndexNumber=""1"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable1]]""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset2(1).f2]]"" FieldValue=""test2"" IndexNumber=""2"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable2]]""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset1(2).f1]]"" FieldValue=""test3"" IndexNumber=""3"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset2(2).f2]]"" FieldValue=""test4"" IndexNumber=""4"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset1(3).f1]]"" FieldValue=""test5"" IndexNumber=""5"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset2(3).f2]]"" FieldValue=""test6"" IndexNumber=""6"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName="""" FieldValue="""" IndexNumber=""7"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;/scg:List&gt;&lt;/uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfMultiAssignActivity&gt;&lt;FlowStep.Next&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/FlowStep.Next&gt;&lt;/FlowStep&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition>
  </Action>
  <ErrorMessages />
  <Source />
  <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
    <SignedInfo>
      <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />
      <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
      <Reference URI="""">
        <Transforms>
          <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
        </Transforms>
        <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
        <DigestValue>VWE/gfMxoDaAnF2QgBKVXjvfTVs=</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>bH2cevYAj3616fNuu55cKdl4pehbJN/lcqTXJPdfRXBhxrJ/iMbMRX/sU03mzycS323KU/2sEyLfQYUYOZh0EHcKD4Bchny+/I04n+PxDbtdGJe9QM561vBQZ6g6fUnQB63lh2uNneQQ8nd+sQ4JY4/C2v6CbPTmEryuFJTclUQ=</SignatureValue>
  </Signature>
</Service>";

            var datalistFragment = @"<DataList><result Description="""" IsEditable=""True"" ColumnIODirection=""Output"" /><recset1 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" ><f1 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /></recset1><recset2 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" ><f2 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /></recset2></DataList>";

            #endregion

            var activityDataMappingBuilder = new ActivityDataMappingBuilder();

            Mock<IContextualResourceModel> resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(c => c.DataList).Returns("<DataList/>");

            Mock<IWebActivity> activity = new Mock<IWebActivity>();

            activity.Setup(c => c.SavedInputMapping).Returns(string.Empty);
            activity.Setup(c => c.SavedOutputMapping).Returns(string.Empty);
            activity.Setup(c => c.ResourceModel.WorkflowXaml).Returns(new StringBuilder(serviceDefStr));
            activity.Setup(c => c.UnderlyingWebActivityObjectType).Returns(typeof(DsfActivity));
            activity.Setup(c => c.ResourceModel.DataList).Returns(datalistFragment);

            activityDataMappingBuilder.SetupActivityData(activity.Object);

            //------------Execute Test---------------------------

            var result = activityDataMappingBuilder.Generate();

            //------------Assert Results-------------------------

            // check counts first
            Assert.AreEqual(2, result.Inputs.Count);
            Assert.AreEqual(1, result.Outputs.Count);

            // now check data
            Assert.AreEqual("[[recset1(*).f1]]", result.Inputs[0].MapsTo);
            Assert.AreEqual("[[recset2(*).f2]]", result.Inputs[1].MapsTo);

            Assert.AreEqual("[[result]]", result.Outputs[0].Value);

        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ActivityDataMappingBuilder_Generate")]
        public void ActivityDataMappingBuilder_Generate_WhenValidWorkflowWithSavedMappings_ExpectSavedInputAndOutputList()
        {
            //------------Setup for test--------------------------

            #region ServiceDef
            var serviceDefStr = @"<Service ID=""8912e8db-074f-43e4-85ea-9376162d3332"" Version=""1.0"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"" Name=""fileTest"" ResourceType=""WorkflowService"" IsValid=""true"">
  <DisplayName>fileTest</DisplayName>
  <Category>Mo</Category>
  <IsNewWorkflow>false</IsNewWorkflow>
  <AuthorRoles>Domain Users,Windows SBS Remote Web Workplace Users,Windows SBS Fax Users,Windows SBS Folder Redirection Accounts,All Users,Windows SBS SharePoint_MembersGroup,Windows SBS Link Users,Company Users,Business Design Studio Developers,Test Engineers,DEV2 Limited Internet Access,</AuthorRoles>
  <Comment>a</Comment>
  <Tags></Tags>
  <IconPath>pack://application:,,,/Warewolf Studio;component/images/Workflow-32.png</IconPath>
  <HelpLink>a:/</HelpLink>
  <UnitTestTargetWorkflowService></UnitTestTargetWorkflowService>
  <DataList>
    <result Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
    <recset1 Description="""" IsEditable=""True"" ColumnIODirection=""None"">
      <f1 Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    </recset1>
    <recset2 Description="""" IsEditable=""True"" ColumnIODirection=""None"">
      <f2 Description="""" IsEditable=""True"" ColumnIODirection=""None"" />
    </recset2>
  </DataList>
  <Action Name=""InvokeWorkflow"" Type=""Workflow"">
    <XamlDefinition>&lt;Activity x:Class=""fileTest"" xmlns=""http://schemas.microsoft.com/netfx/2009/xaml/activities"" xmlns:av=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:dc=""clr-namespace:Dev2.Common;assembly=Dev2.Common"" xmlns:ddc=""clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data"" xmlns:ddcb=""clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data"" xmlns:ddd=""clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data"" xmlns:dddo=""clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data"" xmlns:ddsm=""clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data"" xmlns:dpe=""clr-namespace:Dev2.Providers.Errors;assembly=Dev2.Infrastructure"" xmlns:mva=""clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities"" xmlns:s=""clr-namespace:System;assembly=mscorlib"" xmlns:sads=""http://schemas.microsoft.com/netfx/2010/xaml/activities/debugger"" xmlns:sap=""http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation"" xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=mscorlib"" xmlns:sco=""clr-namespace:System.Collections.ObjectModel;assembly=mscorlib"" xmlns:uaba=""clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities"" xmlns:uf=""clr-namespace:Unlimited.Framework;assembly=Dev2.Core"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""&gt;&lt;x:Members&gt;&lt;x:Property Name=""AmbientDataList"" Type=""InOutArgument(scg:List(x:String))"" /&gt;&lt;x:Property Name=""ParentWorkflowInstanceId"" Type=""InOutArgument(s:Guid)"" /&gt;&lt;x:Property Name=""ParentServiceName"" Type=""InOutArgument(x:String)"" /&gt;&lt;/x:Members&gt;&lt;sap:VirtualizedContainerService.HintSize&gt;719,676&lt;/sap:VirtualizedContainerService.HintSize&gt;&lt;mva:VisualBasic.Settings&gt;Assembly references and imported namespaces serialized as XML namespaces&lt;/mva:VisualBasic.Settings&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""7""&gt;&lt;x:String&gt;Dev2.Common&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Framework&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco:Collection x:TypeArguments=""AssemblyReference""&gt;&lt;AssemblyReference&gt;Dev2.Common&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Core&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=""fileTest"" sap:VirtualizedContainerService.HintSize=""679,636"" mva:VisualBasic.Settings=""Assembly references and imported namespaces serialized as XML namespaces""&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=""scg:List(x:String)"" Name=""InstructionList"" /&gt;&lt;Variable x:TypeArguments=""x:String"" Name=""LastResult"" /&gt;&lt;Variable x:TypeArguments=""x:Boolean"" Name=""HasError"" /&gt;&lt;Variable x:TypeArguments=""x:String"" Name=""ExplicitDataList"" /&gt;&lt;Variable x:TypeArguments=""x:Boolean"" Name=""IsValid"" /&gt;&lt;Variable x:TypeArguments=""uf:UnlimitedObject"" Name=""d"" /&gt;&lt;Variable x:TypeArguments=""uaba:Util"" Name=""t"" /&gt;&lt;Variable x:TypeArguments=""ddd:Dev2DataListDecisionHandler"" Name=""Dev2DecisionHandler"" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;60,75&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;300,77.5 300,133 313.395,133&lt;/av:PointCollection&gt;&lt;x:Double x:Key=""Width""&gt;665&lt;/x:Double&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Reference&gt;__ReferenceID1&lt;/x:Reference&gt;&lt;/Flowchart.StartNode&gt;&lt;FlowStep x:Name=""__ReferenceID0""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;135,243.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;264,116&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfDataMergeActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" DatabindRecursive=""False"" DisplayName=""Data Merge (1)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""264,116"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" Result=""[[sdfsdf]]"" SimulationMode=""OnDemand"" UniqueID=""99f5593a-0f6f-4f8b-a34f-bf93190e14c6""&gt;&lt;uaba:DsfDataMergeActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfDataMergeActivity.AmbientDataList&gt;&lt;uaba:DsfDataMergeActivity.MergeCollection&gt;&lt;scg:List x:TypeArguments=""uaba:DataMergeDTO"" Capacity=""4""&gt;&lt;uaba:DataMergeDTO Alignment=""Left"" At="""" EnableAt=""False"" IndexNumber=""1"" InputVariable=""sdsd"" Inserted=""False"" MergeType=""New Line"" Padding="""" WatermarkTextVariable=""[[Recordset().F1]]"" /&gt;&lt;uaba:DataMergeDTO Alignment=""Left"" At="""" EnableAt=""False"" IndexNumber=""2"" InputVariable="""" Inserted=""False"" MergeType=""None"" Padding="""" WatermarkTextVariable=""[[Recordset().F2]]"" /&gt;&lt;/scg:List&gt;&lt;/uaba:DsfDataMergeActivity.MergeCollection&gt;&lt;uaba:DsfDataMergeActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfDataMergeActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfDataMergeActivity&gt;&lt;/FlowStep&gt;&lt;FlowStep x:Name=""__ReferenceID1""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;313.395,94&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;230,78&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;428.395,172 428.395,301.5 399,301.5&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfMultiAssignActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign (6)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""230,78"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""2cc98df1-da95-421d-b413-98ff091f7397"" UpdateAllOccurrences=""False""&gt;&lt;uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;scg:List x:TypeArguments=""uaba:ActivityDTO"" Capacity=""8""&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset1(1).f1]]"" FieldValue=""test1"" IndexNumber=""1"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable1]]""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset2(1).f2]]"" FieldValue=""test2"" IndexNumber=""2"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable2]]""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset1(2).f1]]"" FieldValue=""test3"" IndexNumber=""3"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset2(2).f2]]"" FieldValue=""test4"" IndexNumber=""4"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset1(3).f1]]"" FieldValue=""test5"" IndexNumber=""5"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[recset2(3).f2]]"" FieldValue=""test6"" IndexNumber=""6"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName="""" FieldValue="""" IndexNumber=""7"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue="""" WatermarkTextVariable=""""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;/scg:List&gt;&lt;/uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfMultiAssignActivity&gt;&lt;FlowStep.Next&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/FlowStep.Next&gt;&lt;/FlowStep&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition>
  </Action>
  <ErrorMessages />
  <Source />
  <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
    <SignedInfo>
      <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />
      <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
      <Reference URI="""">
        <Transforms>
          <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
        </Transforms>
        <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
        <DigestValue>VWE/gfMxoDaAnF2QgBKVXjvfTVs=</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>bH2cevYAj3616fNuu55cKdl4pehbJN/lcqTXJPdfRXBhxrJ/iMbMRX/sU03mzycS323KU/2sEyLfQYUYOZh0EHcKD4Bchny+/I04n+PxDbtdGJe9QM561vBQZ6g6fUnQB63lh2uNneQQ8nd+sQ4JY4/C2v6CbPTmEryuFJTclUQ=</SignatureValue>
  </Signature>
</Service>";

            var datalistFragment = @"<DataList><result Description="""" IsEditable=""True"" ColumnIODirection=""Output"" /><result2 Description="""" IsEditable=""True"" ColumnIODirection=""Output"" /><recset1 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" ><f1 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /></recset1><recset2 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" ><f2 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /></recset2></DataList>";

            #endregion

            var activityDataMappingBuilder = new ActivityDataMappingBuilder();

            Mock<IContextualResourceModel> resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(c => c.DataList).Returns("<DataList/>");

            Mock<IWebActivity> activity = new Mock<IWebActivity>();

            activity.Setup(c => c.SavedInputMapping).Returns(@"<Inputs><Input Name=""f1"" Source=""[[recset1(*).f1a]]"" Recordset=""recset1"" /><Input Name=""f2"" Source=""[[recset2(*).f2a]]"" Recordset=""recset2"" /></Inputs>");
            activity.Setup(c => c.SavedOutputMapping).Returns(@"<Outputs><Output Name=""result"" MapsTo=""[[result]]"" Value=""[[resultValue]]"" /></Outputs>");
            activity.Setup(c => c.ResourceModel.WorkflowXaml).Returns(new StringBuilder(serviceDefStr));
            activity.Setup(c => c.UnderlyingWebActivityObjectType).Returns(typeof(DsfActivity));
            activity.Setup(c => c.ResourceModel.DataList).Returns(datalistFragment);

            activityDataMappingBuilder.SetupActivityData(activity.Object);

            //------------Execute Test---------------------------

            var result = activityDataMappingBuilder.Generate();

            //------------Assert Results-------------------------

            // check counts first
            Assert.AreEqual(2, result.Inputs.Count);

            // now check data
            Assert.AreEqual("[[recset1(*).f1a]]", result.Inputs[0].MapsTo);
            Assert.AreEqual("[[recset2(*).f2a]]", result.Inputs[1].MapsTo);

            // check counts first
            Assert.AreEqual(2, result.Outputs.Count);

            // now check data
            Assert.AreEqual("[[resultValue]]", result.Outputs[0].Value);
            Assert.AreEqual("[[result2]]", result.Outputs[1].Value);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ActivityDataMappingBuilder_Generate")]
        public void ActivityDataMappingBuilder_Generate_WhenValidWorkflowWithDifferentRecordsetNameAndFirstColumn_ExpectInputMappingWithDataListRecordsetName()
        {
            //------------Setup for test--------------------------

            #region ServiceDef
            var serviceDefStr = @"<Service ID=""3354620a-2af5-424d-9364-b60408c111ab"" Version=""1.0"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"" Name=""bob"" ResourceType=""WorkflowService"" IsValid=""true"">
  <DisplayName>bob</DisplayName>
  <Category></Category>
  <IsNewWorkflow>false</IsNewWorkflow>
  <AuthorRoles></AuthorRoles>
  <Comment></Comment>
  <Tags></Tags>
  <IconPath>pack://application:,,,/Warewolf Studio;component/images/Workflow-32.png</IconPath>
  <HelpLink></HelpLink>
  <UnitTestTargetWorkflowService></UnitTestTargetWorkflowService>
  <DataList>
    <rec Description="""" IsEditable=""True"" ColumnIODirection=""Input"">
      <vale Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
      <valeSecond Description="""" IsEditable=""True"" ColumnIODirection=""Input"" />
    </rec>
  </DataList>
  <Action Name=""InvokeWorkflow"" Type=""Workflow"">
    <XamlDefinition>&lt;Activity x:Class=""bob"" xmlns=""http://schemas.microsoft.com/netfx/2009/xaml/activities"" xmlns:av=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:dc=""clr-namespace:Dev2.Common;assembly=Dev2.Common"" xmlns:ddc=""clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data"" xmlns:ddcb=""clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data"" xmlns:ddd=""clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data"" xmlns:dddo=""clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data"" xmlns:ddsm=""clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data"" xmlns:dpe=""clr-namespace:Dev2.Providers.Errors;assembly=Dev2.Infrastructure"" xmlns:mva=""clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities"" xmlns:s=""clr-namespace:System;assembly=mscorlib"" xmlns:sap=""http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation"" xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=mscorlib"" xmlns:sco=""clr-namespace:System.Collections.ObjectModel;assembly=mscorlib"" xmlns:uaba=""clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities"" xmlns:uf=""clr-namespace:Unlimited.Framework;assembly=Dev2.Core"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""&gt;&lt;x:Members&gt;&lt;x:Property Name=""AmbientDataList"" Type=""InOutArgument(scg:List(x:String))"" /&gt;&lt;x:Property Name=""ParentWorkflowInstanceId"" Type=""InOutArgument(s:Guid)"" /&gt;&lt;x:Property Name=""ParentServiceName"" Type=""InOutArgument(x:String)"" /&gt;&lt;/x:Members&gt;&lt;sap:VirtualizedContainerService.HintSize&gt;654,676&lt;/sap:VirtualizedContainerService.HintSize&gt;&lt;mva:VisualBasic.Settings&gt;Assembly references and imported namespaces serialized as XML namespaces&lt;/mva:VisualBasic.Settings&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""7""&gt;&lt;x:String&gt;Dev2.Common&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Framework&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco:Collection x:TypeArguments=""AssemblyReference""&gt;&lt;AssemblyReference&gt;Dev2.Common&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Core&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=""bob"" sap:VirtualizedContainerService.HintSize=""614,636""&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=""scg:List(x:String)"" Name=""InstructionList"" /&gt;&lt;Variable x:TypeArguments=""x:String"" Name=""LastResult"" /&gt;&lt;Variable x:TypeArguments=""x:Boolean"" Name=""HasError"" /&gt;&lt;Variable x:TypeArguments=""x:String"" Name=""ExplicitDataList"" /&gt;&lt;Variable x:TypeArguments=""x:Boolean"" Name=""IsValid"" /&gt;&lt;Variable x:TypeArguments=""uf:UnlimitedObject"" Name=""d"" /&gt;&lt;Variable x:TypeArguments=""uaba:Util"" Name=""t"" /&gt;&lt;Variable x:TypeArguments=""ddd:Dev2DataListDecisionHandler"" Name=""Dev2DecisionHandler"" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;60,75&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;300,77.5 300,127.5&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/Flowchart.StartNode&gt;&lt;FlowStep x:Name=""__ReferenceID0""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;185,127.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;230,78&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfMultiAssignActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign (1)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""230,78"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""6b4f7d09-bc25-4b43-9c21-a213b8bc264b"" UpdateAllOccurrences=""False""&gt;&lt;uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;scg:List x:TypeArguments=""uaba:ActivityDTO"" Capacity=""4""&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[rec().vale]]"" FieldValue=""12"" IndexNumber=""1"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable1]]""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName="""" FieldValue="""" IndexNumber=""2"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable2]]""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;/scg:List&gt;&lt;/uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfMultiAssignActivity&gt;&lt;/FlowStep&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition>
  </Action>
  <ErrorMessages />
  <Source />
  <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
    <SignedInfo>
      <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />
      <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
      <Reference URI="""">
        <Transforms>
          <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
        </Transforms>
        <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
        <DigestValue>vWmN0jnD5L5etfUXFuDmTvVceeg=</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>m8ogAOvBBm4Gue8hZ0vzv5KSgsg/xqCK4hFlxLzTsduLLYTmOmB1VPDelwYZ7OwjnYrQtCf7Rv1rF/r3lhpjtDh4DltZ9j55hWq3zzckItGkAYJHWkNzx3mto+hrrz7cYaDzgLcaNhF11XGSOvU3mbgff4vTFkai9och+0aASu8=</SignatureValue>
  </Signature>
</Service>";

            var datalistFragment = @"<DataList><recset1 Description="""" IsEditable=""True"" ColumnIODirection=""Input"" ><vale Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /><valeSecond Description="""" IsEditable=""True"" ColumnIODirection=""Input"" /></recset1></DataList>";

            #endregion

            var activityDataMappingBuilder = new ActivityDataMappingBuilder
            {
                DataList = "<DataList><recordSet><vale/></recordSet></DataList>"
            };

            Mock<IContextualResourceModel> resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(c => c.DataList).Returns("<DataList/>");

            Mock<IWebActivity> activity = new Mock<IWebActivity>();

            activity.Setup(c => c.SavedInputMapping).Returns(string.Empty);
            activity.Setup(c => c.SavedOutputMapping).Returns(string.Empty);
            activity.Setup(c => c.ResourceModel.WorkflowXaml).Returns(new StringBuilder(serviceDefStr));
            activity.Setup(c => c.UnderlyingWebActivityObjectType).Returns(typeof(DsfActivity));
            activity.Setup(c => c.ResourceModel.DataList).Returns(datalistFragment);

            activityDataMappingBuilder.SetupActivityData(activity.Object);

            //------------Execute Test---------------------------

            var result = activityDataMappingBuilder.Generate();

            //------------Assert Results-------------------------

            // check counts first
            Assert.AreEqual(2, result.Inputs.Count);

            // now check data
            Assert.AreEqual("[[recordSet(*).vale]]", result.Inputs[0].MapsTo);
            Assert.AreEqual("[[recset1(*).valeSecond]]", result.Inputs[1].MapsTo);

            // check counts first
            Assert.AreEqual(0, result.Outputs.Count);

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("ActivityDataMappingBuilder_Generate")]
        public void ActivityDataMappingBuilder_Generate_WhenValidWorkflowWithDifferentRecordsetNameAndFirstColumn_ExpectOutputMappingWithDataListRecordsetName()
        {
            //------------Setup for test--------------------------

            #region ServiceDef
            var serviceDefStr = @"<Service ID=""3354620a-2af5-424d-9364-b60408c111ab"" Version=""1.0"" ServerID=""51a58300-7e9d-4927-a57b-e5d700b11b55"" Name=""bob"" ResourceType=""WorkflowService"" IsValid=""true"">
  <DisplayName>bob</DisplayName>
  <Category></Category>
  <IsNewWorkflow>false</IsNewWorkflow>
  <AuthorRoles></AuthorRoles>
  <Comment></Comment>
  <Tags></Tags>
  <IconPath>pack://application:,,,/Warewolf Studio;component/images/Workflow-32.png</IconPath>
  <HelpLink></HelpLink>
  <UnitTestTargetWorkflowService></UnitTestTargetWorkflowService>
  <DataList>
    <rec Description="""" IsEditable=""True"" ColumnIODirection=""Output"">
      <vale Description="""" IsEditable=""True"" ColumnIODirection=""Output"" />
    </rec>
  </DataList>
  <Action Name=""InvokeWorkflow"" Type=""Workflow"">
    <XamlDefinition>&lt;Activity x:Class=""bob"" xmlns=""http://schemas.microsoft.com/netfx/2009/xaml/activities"" xmlns:av=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"" xmlns:dc=""clr-namespace:Dev2.Common;assembly=Dev2.Common"" xmlns:ddc=""clr-namespace:Dev2.DataList.Contract;assembly=Dev2.Data"" xmlns:ddcb=""clr-namespace:Dev2.DataList.Contract.Binary_Objects;assembly=Dev2.Data"" xmlns:ddd=""clr-namespace:Dev2.Data.Decision;assembly=Dev2.Data"" xmlns:dddo=""clr-namespace:Dev2.Data.Decisions.Operations;assembly=Dev2.Data"" xmlns:ddsm=""clr-namespace:Dev2.Data.SystemTemplates.Models;assembly=Dev2.Data"" xmlns:dpe=""clr-namespace:Dev2.Providers.Errors;assembly=Dev2.Infrastructure"" xmlns:mva=""clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities"" xmlns:s=""clr-namespace:System;assembly=mscorlib"" xmlns:sap=""http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation"" xmlns:scg=""clr-namespace:System.Collections.Generic;assembly=mscorlib"" xmlns:sco=""clr-namespace:System.Collections.ObjectModel;assembly=mscorlib"" xmlns:uaba=""clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities"" xmlns:uf=""clr-namespace:Unlimited.Framework;assembly=Dev2.Core"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""&gt;&lt;x:Members&gt;&lt;x:Property Name=""AmbientDataList"" Type=""InOutArgument(scg:List(x:String))"" /&gt;&lt;x:Property Name=""ParentWorkflowInstanceId"" Type=""InOutArgument(s:Guid)"" /&gt;&lt;x:Property Name=""ParentServiceName"" Type=""InOutArgument(x:String)"" /&gt;&lt;/x:Members&gt;&lt;sap:VirtualizedContainerService.HintSize&gt;654,676&lt;/sap:VirtualizedContainerService.HintSize&gt;&lt;mva:VisualBasic.Settings&gt;Assembly references and imported namespaces serialized as XML namespaces&lt;/mva:VisualBasic.Settings&gt;&lt;TextExpression.NamespacesForImplementation&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""7""&gt;&lt;x:String&gt;Dev2.Common&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.Decisions.Operations&lt;/x:String&gt;&lt;x:String&gt;Dev2.Data.SystemTemplates.Models&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract&lt;/x:String&gt;&lt;x:String&gt;Dev2.DataList.Contract.Binary_Objects&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Framework&lt;/x:String&gt;&lt;x:String&gt;Unlimited.Applications.BusinessDesignStudio.Activities&lt;/x:String&gt;&lt;/scg:List&gt;&lt;/TextExpression.NamespacesForImplementation&gt;&lt;TextExpression.ReferencesForImplementation&gt;&lt;sco:Collection x:TypeArguments=""AssemblyReference""&gt;&lt;AssemblyReference&gt;Dev2.Common&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Data&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Core&lt;/AssemblyReference&gt;&lt;AssemblyReference&gt;Dev2.Activities&lt;/AssemblyReference&gt;&lt;/sco:Collection&gt;&lt;/TextExpression.ReferencesForImplementation&gt;&lt;Flowchart DisplayName=""bob"" sap:VirtualizedContainerService.HintSize=""614,636""&gt;&lt;Flowchart.Variables&gt;&lt;Variable x:TypeArguments=""scg:List(x:String)"" Name=""InstructionList"" /&gt;&lt;Variable x:TypeArguments=""x:String"" Name=""LastResult"" /&gt;&lt;Variable x:TypeArguments=""x:Boolean"" Name=""HasError"" /&gt;&lt;Variable x:TypeArguments=""x:String"" Name=""ExplicitDataList"" /&gt;&lt;Variable x:TypeArguments=""x:Boolean"" Name=""IsValid"" /&gt;&lt;Variable x:TypeArguments=""uf:UnlimitedObject"" Name=""d"" /&gt;&lt;Variable x:TypeArguments=""uaba:Util"" Name=""t"" /&gt;&lt;Variable x:TypeArguments=""ddd:Dev2DataListDecisionHandler"" Name=""Dev2DecisionHandler"" /&gt;&lt;/Flowchart.Variables&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;False&lt;/x:Boolean&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;270,2.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;60,75&lt;/av:Size&gt;&lt;av:PointCollection x:Key=""ConnectorLocation""&gt;300,77.5 300,127.5&lt;/av:PointCollection&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;Flowchart.StartNode&gt;&lt;x:Reference&gt;__ReferenceID0&lt;/x:Reference&gt;&lt;/Flowchart.StartNode&gt;&lt;FlowStep x:Name=""__ReferenceID0""&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;av:Point x:Key=""ShapeLocation""&gt;185,127.5&lt;/av:Point&gt;&lt;av:Size x:Key=""ShapeSize""&gt;230,78&lt;/av:Size&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;uaba:DsfMultiAssignActivity Compiler=""{x:Null}"" CurrentResult=""{x:Null}"" DataObject=""{x:Null}"" ExplicitDataList=""{x:Null}"" InputMapping=""{x:Null}"" InputTransformation=""{x:Null}"" OnResumeKeepList=""{x:Null}"" OutputMapping=""{x:Null}"" ParentServiceID=""{x:Null}"" ParentServiceName=""{x:Null}"" ParentWorkflowInstanceId=""{x:Null}"" ResultTransformation=""{x:Null}"" ScenarioID=""{x:Null}"" ScopingObject=""{x:Null}"" ServiceHost=""{x:Null}"" SimulationOutput=""{x:Null}"" Add=""False"" CreateBookmark=""False"" DatabindRecursive=""False"" DisplayName=""Assign (1)"" HasError=""[HasError]"" sap:VirtualizedContainerService.HintSize=""230,78"" InstructionList=""[InstructionList]"" IsSimulationEnabled=""False"" IsUIStep=""False"" IsValid=""[IsValid]"" IsWorkflow=""False"" OnResumeClearAmbientDataList=""False"" OnResumeClearTags=""FormView,InstanceId,Bookmark,ParentWorkflowInstanceId,ParentServiceName,WebPage"" SimulationMode=""OnDemand"" UniqueID=""6b4f7d09-bc25-4b43-9c21-a213b8bc264b"" UpdateAllOccurrences=""False""&gt;&lt;uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;InOutArgument x:TypeArguments=""scg:List(x:String)"" /&gt;&lt;/uaba:DsfMultiAssignActivity.AmbientDataList&gt;&lt;uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;scg:List x:TypeArguments=""uaba:ActivityDTO"" Capacity=""4""&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName=""[[rec().vale]]"" FieldValue=""12"" IndexNumber=""1"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable1]]""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;uaba:ActivityDTO ErrorMessage=""{x:Null}"" FieldName="""" FieldValue="""" IndexNumber=""2"" Inserted=""False"" IsFieldNameFocused=""False"" WatermarkTextValue=""Value"" WatermarkTextVariable=""[[Variable2]]""&gt;&lt;uaba:ActivityDTO.Errors&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, scg:List(dpe:IActionableErrorInfo)"" /&gt;&lt;/uaba:ActivityDTO.Errors&gt;&lt;uaba:ActivityDTO.OutList&gt;&lt;scg:List x:TypeArguments=""x:String"" Capacity=""0"" /&gt;&lt;/uaba:ActivityDTO.OutList&gt;&lt;/uaba:ActivityDTO&gt;&lt;/scg:List&gt;&lt;/uaba:DsfMultiAssignActivity.FieldsCollection&gt;&lt;uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;InOutArgument x:TypeArguments=""x:String"" /&gt;&lt;/uaba:DsfMultiAssignActivity.ParentInstanceID&gt;&lt;sap:WorkflowViewStateService.ViewState&gt;&lt;scg:Dictionary x:TypeArguments=""x:String, x:Object""&gt;&lt;x:Boolean x:Key=""IsExpanded""&gt;True&lt;/x:Boolean&gt;&lt;/scg:Dictionary&gt;&lt;/sap:WorkflowViewStateService.ViewState&gt;&lt;/uaba:DsfMultiAssignActivity&gt;&lt;/FlowStep&gt;&lt;/Flowchart&gt;&lt;/Activity&gt;</XamlDefinition>
  </Action>
  <ErrorMessages />
  <Source />
  <Signature xmlns=""http://www.w3.org/2000/09/xmldsig#"">
    <SignedInfo>
      <CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315"" />
      <SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1"" />
      <Reference URI="""">
        <Transforms>
          <Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature"" />
        </Transforms>
        <DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1"" />
        <DigestValue>vWmN0jnD5L5etfUXFuDmTvVceeg=</DigestValue>
      </Reference>
    </SignedInfo>
    <SignatureValue>m8ogAOvBBm4Gue8hZ0vzv5KSgsg/xqCK4hFlxLzTsduLLYTmOmB1VPDelwYZ7OwjnYrQtCf7Rv1rF/r3lhpjtDh4DltZ9j55hWq3zzckItGkAYJHWkNzx3mto+hrrz7cYaDzgLcaNhF11XGSOvU3mbgff4vTFkai9och+0aASu8=</SignatureValue>
  </Signature>
</Service>";

            var datalistFragment = @"<DataList><recset1 Description="""" IsEditable=""True"" ColumnIODirection=""Output"" ><vale Description="""" IsEditable=""True"" ColumnIODirection=""Output"" /><valeSecond Description="""" IsEditable=""True"" ColumnIODirection=""Output"" /></recset1></DataList>";

            #endregion

            var activityDataMappingBuilder = new ActivityDataMappingBuilder
            {
                DataList = "<DataList><recordSet><vale/></recordSet></DataList>"
            };

            Mock<IContextualResourceModel> resourceModel = new Mock<IContextualResourceModel>();
            resourceModel.Setup(c => c.DataList).Returns("<DataList/>");

            Mock<IWebActivity> activity = new Mock<IWebActivity>();

            activity.Setup(c => c.SavedInputMapping).Returns(string.Empty);
            activity.Setup(c => c.SavedOutputMapping).Returns(string.Empty);
            activity.Setup(c => c.ResourceModel.WorkflowXaml).Returns(new StringBuilder(serviceDefStr));
            activity.Setup(c => c.UnderlyingWebActivityObjectType).Returns(typeof(DsfActivity));
            activity.Setup(c => c.ResourceModel.DataList).Returns(datalistFragment);

            activityDataMappingBuilder.SetupActivityData(activity.Object);

            //------------Execute Test---------------------------

            var result = activityDataMappingBuilder.Generate();

            //------------Assert Results-------------------------

            // check counts first
            Assert.AreEqual(2, result.Outputs.Count);

            // now check data
            Assert.AreEqual("[[recordSet().vale]]", result.Outputs[0].Value);
            Assert.AreEqual("[[recset1().valeSecond]]", result.Outputs[1].Value);

            // check counts first
            Assert.AreEqual(0, result.Inputs.Count);

        }

        #endregion

    }
}
