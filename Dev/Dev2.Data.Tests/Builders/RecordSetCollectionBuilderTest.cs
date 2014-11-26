
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Diagnostics.CodeAnalysis;
using Dev2.Data.Builders;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.Builders
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class RecordSetCollectionBuilderTest
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RecordSetCollectionBuilder_SetParsedOutput")]
        public void RecordSetCollectionBuilder_SetParsedOutput_ParameterOutoutIsNull_ParseOutputIsNull()
        {
            var builder = new RecordSetCollectionBuilder();
            builder.SetParsedOutput(null);
            Assert.AreEqual(0, builder.ParsedOutput.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RecordSetCollectionBuilder_SetParsedOutput")]
        public void RecordSetCollectionBuilder_SetParsedOutput_ParameterIsValidDefinitions_ParseOutputIsInitialisedWithParameter()
        {
            const string arguments =
                "<Outputs>" +
                   "<Output Name=\"A\" MapsTo=\"[[A]]\" Value=\"[[recA(*).A]]\" Recordset=\"inrecA\" />" +
                   "<Output Name=\"B\" MapsTo=\"[[B]]\" Value=\"[[recA(*).B]]\" Recordset=\"inrecA\" />" +
                   "<Output Name=\"C\" MapsTo=\"[[C]]\" Value=\"[[recB(*).C]]\" Recordset=\"inrecA\" />" +
                   "<Output Name=\"D\" MapsTo=\"[[D]]\" Value=\"[[recB(*).D]]\" Recordset=\"inrecA\" />" +
                "</Outputs>";

            var defs = DataListFactory.CreateOutputParser().Parse(arguments);
            var builder = new RecordSetCollectionBuilder();
            builder.SetParsedOutput(defs);
            Assert.AreEqual(defs, builder.ParsedOutput);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RecordSetCollectionBuilder_Generate")]
        public void RecordSetCollectionBuilder_Generate_OutputHasTwoRecordsetsAndIsOutputIsFalse_CollectionHasTwoRecordDefinitions()
        {
            //------------Setup for test--------------------------
            const string arguments =
                "<Outputs>" +
                   "<Output Name=\"A\" MapsTo=\"[[A]]\" Value=\"[[recA(*).A]]\" Recordset=\"inrecA\" />" +
                   "<Output Name=\"B\" MapsTo=\"[[B]]\" Value=\"[[recA(*).B]]\" Recordset=\"inrecA\" />" +
                   "<Output Name=\"C\" MapsTo=\"[[C]]\" Value=\"[[recB(*).C]]\" Recordset=\"inrecA\" />" +
                   "<Output Name=\"D\" MapsTo=\"[[D]]\" Value=\"[[recB(*).D]]\" Recordset=\"inrecA\" />" +
                "</Outputs>";

            var defs = DataListFactory.CreateOutputParser().Parse(arguments);
            var builder = new RecordSetCollectionBuilder();
            builder.SetParsedOutput(defs);
            builder.IsOutput = false;
            //------------Execute Test---------------------------
            var collection = builder.Generate();
            //------------Assert Results-------------------------
            Assert.IsNotNull(collection);
            Assert.AreEqual(2, collection.RecordSetNames.Count);
            Assert.AreEqual("recA", collection.RecordSetNames[0]);
            Assert.AreEqual("recB", collection.RecordSetNames[1]);
            Assert.AreEqual(2, collection.RecordSets.Count);
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RecordSetCollectionBuilder_Generate")]
        public void RecordSetCollectionBuilder_Generate_OutputHasTwoRecordsetsAndIsOutputIsTrue_CollectionHasTwoRecordDefinitions()
        {
            //------------Setup for test--------------------------
            const string arguments =
                "<Outputs>" +
                   "<Output Name=\"A\" MapsTo=\"[[A]]\" Value=\"[[recA(*).A]]\" Recordset=\"inrecA\" />" +
                   "<Output Name=\"B\" MapsTo=\"[[B]]\" Value=\"[[recA(*).B]]\" Recordset=\"inrecA\" />" +
                   "<Output Name=\"C\" MapsTo=\"[[C]]\" Value=\"[[recB(*).C]]\" Recordset=\"inrecA\" />" +
                   "<Output Name=\"D\" MapsTo=\"[[D]]\" Value=\"[[recB(*).D]]\" Recordset=\"inrecA\" />" +
                "</Outputs>";

            var defs = DataListFactory.CreateOutputParser().Parse(arguments);
            var builder = new RecordSetCollectionBuilder();
            builder.SetParsedOutput(defs);
            builder.IsOutput = true;
            //------------Execute Test---------------------------
            var collection = builder.Generate();
            //------------Assert Results-------------------------
            Assert.IsNotNull(collection);
            Assert.AreEqual(1, collection.RecordSetNames.Count);
            Assert.AreEqual("inrecA", collection.RecordSetNames[0]);
            Assert.AreEqual(1, collection.RecordSets.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RecordSetCollectionBuilder_Generate")]
        public void RecordSetCollectionBuilder_Generate_OutputHasTwoRecordsetsAndIsOutputIsFalseAndIsDbService_CollectionHasTwoRecordDefinitions()
        {
            //------------Setup for test--------------------------
            const string arguments =
                "<Outputs>" +
                   "<Output Name=\"A\" MapsTo=\"[[A]]\" Value=\"[[recA(*).A]]\" Recordset=\"inrecA\" />" +
                   "<Output Name=\"B\" MapsTo=\"[[B]]\" Value=\"[[recA(*).B]]\" Recordset=\"inrecA\" />" +
                   "<Output Name=\"C\" MapsTo=\"[[C]]\" Value=\"[[recB(*).C]]\" Recordset=\"inrecA\" />" +
                   "<Output Name=\"D\" MapsTo=\"[[D]]\" Value=\"[[recB(*).D]]\" Recordset=\"inrecA\" />" +
                "</Outputs>";

            var defs = DataListFactory.CreateOutputParser().Parse(arguments);
            var builder = new RecordSetCollectionBuilder();
            builder.SetParsedOutput(defs);
            builder.IsOutput = false;
            builder.IsDbService = true;
            //------------Execute Test---------------------------
            var collection = builder.Generate();
            //------------Assert Results-------------------------
            Assert.IsNotNull(collection);
            Assert.AreEqual(2, collection.RecordSetNames.Count);
            Assert.AreEqual("recA", collection.RecordSetNames[0]);
            Assert.AreEqual("recB", collection.RecordSetNames[1]);
            Assert.AreEqual(2, collection.RecordSets.Count);
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RecordSetCollectionBuilder_Generate")]
        public void RecordSetCollectionBuilder_Generate_OutputHasTwoRecordsetsAndIsOutputIsTrueAndIsDbService_CollectionHasTwoRecordDefinitions()
        {
            //------------Setup for test--------------------------
            const string arguments =
                "<Outputs>" +
                   "<Output Name=\"A\" MapsTo=\"[[A]]\" Value=\"[[recA(*).A]]\" Recordset=\"inrecA\" />" +
                   "<Output Name=\"B\" MapsTo=\"[[B]]\" Value=\"[[recA(*).B]]\" Recordset=\"inrecA\" />" +
                   "<Output Name=\"C\" MapsTo=\"[[C]]\" Value=\"[[recB(*).C]]\" Recordset=\"inrecA\" />" +
                   "<Output Name=\"D\" MapsTo=\"[[D]]\" Value=\"[[recB(*).D]]\" Recordset=\"inrecA\" />" +
                "</Outputs>";

            var defs = DataListFactory.CreateOutputParser().Parse(arguments);
            var builder = new RecordSetCollectionBuilder();
            builder.SetParsedOutput(defs);
            builder.IsOutput = true;
            builder.IsDbService = true;
            //------------Execute Test---------------------------
            var collection = builder.Generate();
            //------------Assert Results-------------------------
            Assert.IsNotNull(collection);
            Assert.AreEqual(2, collection.RecordSetNames.Count);
            Assert.AreEqual("recA", collection.RecordSetNames[0]);
            Assert.AreEqual("recB", collection.RecordSetNames[1]);
            Assert.AreEqual(2, collection.RecordSets.Count);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RecordSetCollectionBuilder_Generate")]
        public void RecordSetCollectionBuilder_Generate_IsOutputIsFalse_CollectionsRecordsetIsSetToValueRecordsetName()
        {
            //------------Setup for test--------------------------
            const string arguments =
                "<Outputs>" +
                   "<Output Name=\"A\" MapsTo=\"[[A]]\" Value=\"[[recA(*).A]]\" Recordset=\"Another\" />" +
                "</Outputs>";

            var defs = DataListFactory.CreateOutputParser().Parse(arguments);
            var builder = new RecordSetCollectionBuilder();
            builder.SetParsedOutput(defs);
            builder.IsOutput = false;
            //------------Execute Test---------------------------
            var collection = builder.Generate();
            //------------Assert Results-------------------------
            Assert.IsNotNull(collection);
            Assert.AreEqual(1, collection.RecordSetNames.Count);
            Assert.AreEqual("recA", collection.RecordSetNames[0]);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RecordSetCollectionBuilder_Generate")]
        public void RecordSetCollectionBuilder_Generate_IsOutputIsTrue_CollectionsRecordsetIsSetToRecordsetsValue()
        {
            //------------Setup for test--------------------------
            const string arguments =
                "<Outputs>" +
                   "<Output Name=\"A\" MapsTo=\"[[A]]\" Value=\"[[recA(*).A]]\" Recordset=\"Another\" />" +
                "</Outputs>";

            var defs = DataListFactory.CreateOutputParser().Parse(arguments);
            var builder = new RecordSetCollectionBuilder();
            builder.SetParsedOutput(defs);
            builder.IsOutput = true;
            //------------Execute Test---------------------------
            var collection = builder.Generate();
            //------------Assert Results-------------------------
            Assert.IsNotNull(collection);
            Assert.AreEqual(1, collection.RecordSetNames.Count);
            Assert.AreEqual("Another", collection.RecordSetNames[0]);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("RecordSetCollectionBuilder_Generate")]
        public void RecordSetCollectionBuilder_Generate_ParsedForInput_RecordsetNameIsSetFromSource()
        {
            //------------Setup for test--------------------------
            const string arguments = @"<Inputs><Input Name=""Prefix"" Source=""[[prefix(*).val]]"" /><Input Name=""Suffix"" Source=""[[prefix(*).val]]"" /></Inputs>";            
            var defs = DataListFactory.CreateInputParser().Parse(arguments);
            var builder = new RecordSetCollectionBuilder();
            builder.SetParsedOutput(defs);
            builder.IsOutput = false;
            //------------Execute Test---------------------------
            var collection = builder.Generate();
            //------------Assert Results-------------------------
            Assert.IsNotNull(collection);
            Assert.AreEqual(1, collection.RecordSetNames.Count);
            Assert.AreEqual("prefix", collection.RecordSetNames[0]);
        }
    }
}
