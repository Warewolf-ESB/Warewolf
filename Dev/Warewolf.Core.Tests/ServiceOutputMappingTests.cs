using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Warewolf.Core.Tests
{
    [TestClass]
    public class ServiceOutputMappingTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceOutputMapping_Constructor")]
        public void ServiceOutputMapping_Constructor_EmptyConstructor_ShouldStillConstruct()
        {
            //------------Setup for test--------------------------
            //------------Execute Test---------------------------
            var serviceOutputMapping = new ServiceOutputMapping();
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceOutputMapping);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceOutputMapping_Constructor")]
        public void ServiceOutputMapping_Constructor_EmptyMappingFrom_ShouldStillConsturct()
        {
            //------------Setup for test--------------------------
            const string mappingTo = "mapTo";
            const string recordSet = "recset";
            const string variableNameMappingTo = "[[recset().mapTo]]";
            
            //------------Execute Test---------------------------
            var serviceOutputMapping = new ServiceOutputMapping("",mappingTo,recordSet);
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceOutputMapping);
            Assert.AreEqual(variableNameMappingTo,serviceOutputMapping.MappedTo);
            Assert.AreEqual(recordSet,serviceOutputMapping.RecordSetName);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceOutputMapping_Constructor")]
        public void ServiceOutputMapping_Constructor_EmptyMappingTo_ShouldStillConsturct()
        {
            //------------Setup for test--------------------------
            const string mappingFrom = "mapFrom";
            const string recordSet = "recset";
            
            //------------Execute Test---------------------------
            var serviceOutputMapping = new ServiceOutputMapping(mappingFrom,"",recordSet);
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceOutputMapping);
            Assert.AreEqual("",serviceOutputMapping.MappedTo);
            Assert.AreEqual(mappingFrom,serviceOutputMapping.MappedFrom);
            Assert.AreEqual(recordSet,serviceOutputMapping.RecordSetName);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceOutputMapping_Constructor")]
        public void ServiceOutputMapping_Constructor_WhenNoRecordsetName_ShouldConstructorScalarMappedTo()
        {
            //------------Setup for test--------------------------
            const string mappingFrom = "mapFrom";
            const string mapTo = "mapTo";
            const string variableMapTo = "[[mapTo]]";
            //------------Execute Test---------------------------
            var serviceOutputMapping = new ServiceOutputMapping(mappingFrom,mapTo,"");
            //------------Assert Results-------------------------
            Assert.IsNotNull(serviceOutputMapping);
            Assert.AreEqual("",serviceOutputMapping.RecordSetName);
            Assert.AreEqual(mappingFrom,serviceOutputMapping.MappedFrom);
            Assert.AreEqual(variableMapTo,serviceOutputMapping.MappedTo);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceOutputMapping_RecordsetName")]
        public void ServiceOutputMapping_RecordsetName_WhenContainedInMapTo_ShouldUpdateMapToValue()
        {
            //------------Setup for test--------------------------
            const string mappingFrom = "mapFrom";
            const string recordSet = "recset";
            const string mapTo = "mapTo";
            const string variableNameMappingTo = "[[recset().mapTo]]";
            const string variableNameMappingToChanged = "[[rec().mapTo]]";
            var serviceOutputMapping = new ServiceOutputMapping(mappingFrom,mapTo,recordSet);
            //------------Assert Precondition--------------------
            Assert.IsNotNull(serviceOutputMapping);
            Assert.AreEqual(variableNameMappingTo,serviceOutputMapping.MappedTo);
            //------------Execute Test---------------------------
            serviceOutputMapping.RecordSetName = "rec";
            //------------Assert Results-------------------------
            Assert.AreEqual(variableNameMappingToChanged,serviceOutputMapping.MappedTo);
        }
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceOutputMapping_RecordsetName")]
        public void ServiceOutputMapping_RecordsetName_WhenNotContainedInMapTo_ShouldNotUpdateMapToValue()
        {
            //------------Setup for test--------------------------
            const string mappingFrom = "mapFrom";
            const string recordSet = "recset";
            const string mapTo = "mapTo";
            const string variableNameMappingTo = "[[recset().mapTo]]";
            const string variableNameMappingToChanged = "[[rec().mapTo]]";
            var serviceOutputMapping = new ServiceOutputMapping(mappingFrom, mapTo, recordSet) { MappedTo = "[[rec().mapTo]]" };
            //------------Assert Precondition--------------------
            Assert.IsNotNull(serviceOutputMapping);
            Assert.AreEqual(variableNameMappingToChanged,serviceOutputMapping.MappedTo);
            //------------Execute Test---------------------------
            serviceOutputMapping.RecordSetName = "recset";
            //------------Assert Results-------------------------
            Assert.AreEqual(variableNameMappingToChanged,serviceOutputMapping.MappedTo);
            Assert.AreNotEqual(variableNameMappingTo,serviceOutputMapping.MappedTo);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceOutputMapping_RecordsetName")]
        public void ServiceOutputMapping_RecordsetName_WhenContainedInMapToRecsetCleared_ShouldUpdateMapToScalarValue()
        {
            //------------Setup for test--------------------------
            const string mappingFrom = "mapFrom";
            const string recordSet = "recset";
            const string mapTo = "mapTo";
            const string variableNameMappingTo = "[[recset().mapTo]]";
            const string variableNameMappingToChanged = "[[mapTo]]";
            var serviceOutputMapping = new ServiceOutputMapping(mappingFrom, mapTo, recordSet);
            //------------Assert Precondition--------------------
            Assert.IsNotNull(serviceOutputMapping);
            Assert.AreEqual(variableNameMappingTo, serviceOutputMapping.MappedTo);
            //------------Execute Test---------------------------
            serviceOutputMapping.RecordSetName = "";
            //------------Assert Results-------------------------
            Assert.AreEqual(variableNameMappingToChanged, serviceOutputMapping.MappedTo);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceOutputMapping_RecordsetName")]
        public void ServiceOutputMapping_RecordsetName_WhenEmptyRecsetIsUpdated_ShouldUpdateMapToScalarValue()
        {
            //------------Setup for test--------------------------
            const string mappingFrom = "mapFrom";
            const string recordSet = "";
            const string mapTo = "mapTo";
            const string variableNameMappingTo = "[[recset().mapTo]]";
            const string variableNameMappingToChanged = "[[mapTo]]";
            var serviceOutputMapping = new ServiceOutputMapping(mappingFrom, mapTo, recordSet);
            //------------Assert Precondition--------------------
            Assert.IsNotNull(serviceOutputMapping);
            Assert.AreEqual(variableNameMappingToChanged, serviceOutputMapping.MappedTo);
            //------------Execute Test---------------------------
            serviceOutputMapping.RecordSetName = "recset";
            //------------Assert Results-------------------------
            Assert.AreEqual(variableNameMappingTo, serviceOutputMapping.MappedTo);
        }
        
    }
}
