/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Designers2.Core;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.DB;
using Dev2.Studio.Core.Activities.Utils;
using Dev2.Studio.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Serialization;
using Warewolf.Core;
using Warewolf.Storage;

namespace Dev2.Activities.Designers.Tests.Core
{
    [TestClass]
    public class WebOutputsRegionTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory(nameof(OutputsRegion))]
        public void OutputsRegion_Ctor_ValidModelItem_ExpectVisibleAndValidData()
        {
            //------------Setup for test--------------------------
            var act = new DsfWebGetActivity
            {
                SourceId = Guid.NewGuid(),
                Outputs = new List<IServiceOutputMapping>
                {
                    new ServiceOutputMapping("a", "b", "c")
                }
            };
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act), true);

            //------------Execute Test---------------------------
            Assert.IsTrue(outputsRegion.IsEnabled);
            Assert.IsTrue(outputsRegion.IsObjectOutputUsed);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory(nameof(OutputsRegion))]
        public void OutputsRegion_Ctor_NewModelItem_ExpectVisibleAndValidData()
        {
            //------------Setup for test--------------------------
            var act = new DsfWebGetActivity
            {
                SourceId = Guid.NewGuid(),
                Outputs = new List<IServiceOutputMapping>(),
            };
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act), true);

            //------------Execute Test---------------------------
            Assert.IsFalse(outputsRegion.IsEnabled);
            Assert.IsTrue(outputsRegion.IsObjectOutputUsed);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [TestCategory(nameof(OutputsRegion))]
        public void OutputsRegion_Given_MappedTo_Is_Changed_From_RecordSetTo_Scalar_Sets_RecordSet_To_Empty()
        {
            //------------Setup for test--------------------------
            var act = new DsfWebGetActivity
            {
                SourceId = Guid.NewGuid(),
                Outputs = new List<IServiceOutputMapping>(),
            };
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act), true);
            outputsRegion.Outputs.Add(new ServiceOutputMapping { MappedFrom = "A", MappedTo = "[[Person().A]]", RecordSetName = "Person" });
            outputsRegion.Outputs.Add(new ServiceOutputMapping { MappedFrom = "B", MappedTo = "[[Person().B]]", RecordSetName = "Person" });
            //------------Execute Test---------------------------
            outputsRegion.Outputs.First().MappedTo = "A";
            //------------Assert Results-------------------------
            Assert.IsTrue(String.IsNullOrEmpty(outputsRegion.Outputs.First().RecordSetName));
        }

        [TestMethod]
        [TestCategory(nameof(OutputsRegion))]
        public void OutputsRegion_Given_MappedTo_Is_Changed_From_Scalar_To_RecordSet_Sets_RecordSet_Name()
        {
            //------------Setup for test--------------------------
            var act = new DsfWebGetActivity
            {
                SourceId = Guid.NewGuid(),
                Outputs = new List<IServiceOutputMapping>(),
            };
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act), true);
            outputsRegion.Outputs.Add(new ServiceOutputMapping { MappedFrom = "A", MappedTo = "[[A]]", RecordSetName = "" });
            outputsRegion.Outputs.Add(new ServiceOutputMapping { MappedFrom = "B", MappedTo = "[[Person().B]]", RecordSetName = "Person" });
            //------------Execute Test---------------------------
            Assert.IsTrue(string.IsNullOrEmpty(outputsRegion.Outputs.First().RecordSetName));
            outputsRegion.Outputs.First().MappedTo = "[[Person().A]]";
            //------------Assert Results-------------------------
            Assert.AreEqual("Person", outputsRegion.Outputs.First().RecordSetName);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory(nameof(OutputsRegion))]
        public void OutputsRegion_AddSomeOutputsExpectHeightChanged()
        {
            //------------Setup for test--------------------------
            var act = new DsfWebGetActivity
            {
                SourceId = Guid.NewGuid(),
                Outputs = new List<IServiceOutputMapping>(),
            };
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act), true);
            outputsRegion.Outputs.Add(new ServiceOutputMapping());
            outputsRegion.Outputs.Add(new ServiceOutputMapping());
            outputsRegion.Outputs.Add(new ServiceOutputMapping());
            outputsRegion.Outputs.Add(new ServiceOutputMapping());
            //------------Execute Test---------------------------
            Assert.IsFalse(outputsRegion.IsEnabled);
            Assert.IsTrue(outputsRegion.IsObjectOutputUsed);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory(nameof(OutputsRegion))]
        public void OutputsRegion_Clone()
        {
            CustomContainer.Register<IFieldAndPropertyMapper>(new FieldAndPropertyMapper());
            //------------Setup for test--------------------------
            var act = new DsfWebGetActivity
            {
                SourceId = Guid.NewGuid(),
                Outputs = new List<IServiceOutputMapping>(),
            };
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act), true);
            outputsRegion.Outputs.Add(new ServiceOutputMapping());
            outputsRegion.Outputs.Add(new ServiceOutputMapping());
            outputsRegion.Outputs.Add(new ServiceOutputMapping());
            outputsRegion.Outputs.Add(new ServiceOutputMapping());
            //------------Execute Test---------------------------
            Assert.IsFalse(outputsRegion.IsEnabled);
            Assert.IsTrue(outputsRegion.IsObjectOutputUsed);
            var x = outputsRegion.CloneRegion() as OutputsRegion;
            //------------Assert Results-------------------------
            Assert.IsNotNull(x, "x != null");
            Assert.AreEqual(4, x.Outputs.Count);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory(nameof(OutputsRegion))]
        public void OutputsRegion_Restore()
        {
            CustomContainer.Register<IFieldAndPropertyMapper>(new FieldAndPropertyMapper());
            //------------Setup for test--------------------------
            var act = new DsfWebGetActivity
            {
                SourceId = Guid.NewGuid(),
                Outputs = new List<IServiceOutputMapping>(),
            };
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act), true);
            outputsRegion.Outputs.Add(new ServiceOutputMapping());
            outputsRegion.Outputs.Add(new ServiceOutputMapping());
            outputsRegion.Outputs.Add(new ServiceOutputMapping());
            outputsRegion.Outputs.Add(new ServiceOutputMapping());

            //------------Execute Test---------------------------
            Assert.IsFalse(outputsRegion.IsEnabled);
            Assert.IsTrue(outputsRegion.IsObjectOutputUsed);
            var x = outputsRegion.CloneRegion() as OutputsRegion;
            outputsRegion.Outputs.Clear();
            Assert.IsFalse(outputsRegion.IsEnabled);

            //------------Assert Results-------------------------
            outputsRegion.RestoreRegion(x);
            Assert.IsFalse(outputsRegion.IsEnabled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(OutputsRegion))]
        public void OutputsRegion_GivenNull_ShouldUpdatesModelItem()
        {
            //---------------Set up test pack-------------------
            var act = new DsfWebGetActivity
            {
                SourceId = Guid.NewGuid(),
                Outputs = new List<IServiceOutputMapping>(),
            };
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act), true);
            var wasCalled = false;
            outputsRegion.PropertyChanged += (sender, args) =>
            {
                wasCalled = true;
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(outputsRegion.Outputs);
            //---------------Execute Test ----------------------
            Assert.IsNotNull(act.Outputs);
            //---------------Test Result -----------------------
            outputsRegion.Outputs = null;
            Assert.AreEqual(0, act.Outputs.Count);
            Assert.IsTrue(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(OutputsRegion))]
        public void OutputsRegion_GivenNull_ShouldAttachEventHandlers()
        {
            //---------------Set up test pack-------------------
            var act = new DsfWebGetActivity
            {
                SourceId = Guid.NewGuid(),
                Outputs = new List<IServiceOutputMapping>(),
            };
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act), true);
            var serviceOutputMapping = new ServiceOutputMapping
            {
                MappedFrom = "j",
                MappedTo = "H"
            };
            var outPutsChanged = false;
            serviceOutputMapping.PropertyChanged += (sender, args) =>
            {
                outPutsChanged |= args.PropertyName == "MappedFrom";
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(outputsRegion.Outputs);
            //---------------Execute Test ----------------------
            outputsRegion.Outputs.Add(serviceOutputMapping);
            outputsRegion.Outputs.First().MappedFrom = "b";
            //---------------Test Result -----------------------
            Assert.IsTrue(outPutsChanged);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(OutputsRegion))]
        public void OutputsRegion_GivenNull_ShouldRemovedEventHandlers()
        {
            //---------------Set up test pack-------------------
            var act = new DsfWebGetActivity
            {
                SourceId = Guid.NewGuid(),
                Outputs = new List<IServiceOutputMapping>(),
            };
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act), true);
            var serviceOutputMapping = new ServiceOutputMapping
            {
                MappedFrom = "j",
                MappedTo = "H"
            };
            var outPutsChanged = false;
            serviceOutputMapping.PropertyChanged += (sender, args) =>
            {
                outPutsChanged |= args.PropertyName == "MappedFrom";
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(outputsRegion.Outputs);
            //---------------Execute Test ----------------------
            outputsRegion.Outputs.Add(serviceOutputMapping);
            outputsRegion.Outputs.Add(new ServiceOutputMapping());
            outputsRegion.Outputs.First(mapping => mapping.MappedFrom == "j").MappedFrom = "b";
            Assert.IsTrue(outPutsChanged);
            outPutsChanged = false;
            //---------------Test Result -----------------------
            outputsRegion.Outputs.Remove(serviceOutputMapping);
            outputsRegion.Outputs.First().MappedFrom = "b";
            Assert.IsFalse(outPutsChanged);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(OutputsRegion))]
        public void OutputsRegion_ObjectName_GivenIsObjectAndNullObjectResult_ShouldNotFireChanges()
        {
            //---------------Set up test pack-------------------
            var act = new DsfWebGetActivity
            {
                SourceId = Guid.NewGuid(),
                Outputs = new List<IServiceOutputMapping>(),
                IsObject = true,
            };
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act), true);
            var wasCalled = false;
            outputsRegion.PropertyChanged += (sender, args) =>
            {
                wasCalled = true;
            };
            //---------------Assert Precondition----------------
            Assert.IsTrue(outputsRegion.IsObject);
            //---------------Execute Test ----------------------
            outputsRegion.ObjectName = "a";
            //---------------Test Result -----------------------
            Assert.IsFalse(wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(OutputsRegion))]
        public void OutputsRegion_ObjectName_GivenIsObjectAndObjectResult_ShouldFireChanges()
        {
            //---------------Set up test pack-------------------
            var act = new DsfWebGetActivity
            {
                SourceId = Guid.NewGuid(),
                Outputs = new List<IServiceOutputMapping>(),
                IsObject = true,
            };
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act), true)
            {
                ObjectResult = this.SerializeToJsonString(new DefaultSerializationBinder())
            };
            var wasCalled = false;
            outputsRegion.PropertyChanged += (sender, args) =>
            {
                wasCalled = true;
            };
            //---------------Assert Precondition----------------
            Assert.IsTrue(outputsRegion.IsObject);
            //---------------Execute Test ----------------------
            outputsRegion.ObjectName = "a";
            //---------------Test Result -----------------------
            Assert.IsTrue(wasCalled);
            Assert.AreEqual(outputsRegion.ObjectName, act.ObjectName);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(OutputsRegion))]
        public void OutputsRegion_ObjectName_GivenIsObjectAndObjectResult_ShouldUpdateDatalist()
        {
            //---------------Set up test pack-------------------
            CustomContainer.DeRegister<IShellViewModel>();
            var shellVm = new Mock<IShellViewModel>();
            shellVm.Setup(model => model.UpdateCurrentDataListWithObjectFromJson(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            CustomContainer.Register(shellVm.Object);
            var act = new DsfWebGetActivity
            {
                SourceId = Guid.NewGuid(),
                Outputs = new List<IServiceOutputMapping>(),
                IsObject = true,
            };
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act), true)
            {
                ObjectResult = this.SerializeToJsonString(new DefaultSerializationBinder())
            };

            //---------------Assert Precondition----------------
            Assert.IsTrue(outputsRegion.IsObject);
            Assert.IsTrue(!string.IsNullOrEmpty(outputsRegion.ObjectResult));
            Assert.IsTrue(FsInteropFunctions.ParseLanguageExpressionWithoutUpdate("[[@objName]]").IsJsonIdentifierExpression);
            //---------------Execute Test ----------------------
            outputsRegion.ObjectName = "[[@objName]]";
            //---------------Test Result -----------------------
            shellVm.Verify(model => model.UpdateCurrentDataListWithObjectFromJson(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.AreEqual(outputsRegion.ObjectName, act.ObjectName);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(OutputsRegion))]
        public void OutputsRegion_ObjectName_GivenIsObjectAndNullValues_ShouldUpdateDatalist()
        {
            //---------------Set up test pack-------------------
            var act = new DsfWebGetActivity
            {
                SourceId = Guid.NewGuid(),
                Outputs = new List<IServiceOutputMapping>(),
                IsObject = true,
            };
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act), true)
            {
                ObjectResult = this.SerializeToJsonString(new DefaultSerializationBinder())
            };

            //---------------Assert Precondition----------------
            Assert.IsTrue(outputsRegion.IsObject);
            //---------------Execute Test ----------------------
            outputsRegion.ObjectName = null;
            //---------------Test Result -----------------------
            Assert.AreEqual(string.Empty, act.ObjectName);
            Assert.AreEqual(outputsRegion.ObjectName, string.Empty);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory(nameof(OutputsRegion))]
        public void OutputsRegion_ResetOutputs_GivenUpdatedOutputs_ShouldUpdateOutPutsCollection()
        {
            //---------------Set up test pack-------------------
            var updatedOutputs = new List<IServiceOutputMapping>
            {
                new ServiceOutputMapping { MappedFrom = "Name", MappedTo = "[[Person().Firstname]]", RecordSetName = "Person"},
                new ServiceOutputMapping { MappedFrom = "Surname", MappedTo = "[[Person().Surname]]", RecordSetName = "Person" }
            };
            var act = new DsfWebGetActivity
            {
                SourceId = Guid.NewGuid(),
                Outputs = new List<IServiceOutputMapping>(),
                IsObject = true,
            };
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act), true);
            outputsRegion.Outputs.Add(new ServiceOutputMapping { MappedFrom = "Name", MappedTo = "[[Person().Name]]", RecordSetName = "Person" });
            outputsRegion.Outputs.Add(new ServiceOutputMapping { MappedFrom = "Surname", MappedTo = "[[Person().Surname]]", RecordSetName = "Person" });
            //---------------Assert Precondition----------------
            Assert.AreEqual(2, outputsRegion.Outputs.Count);
            Assert.AreEqual("[[Person().Name]]", outputsRegion.Outputs.First().MappedTo);
            Assert.AreEqual("[[Person().Surname]]", outputsRegion.Outputs.Last().MappedTo);
            //---------------Execute Test ----------------------
            outputsRegion.ResetOutputs(updatedOutputs);
            //---------------Test Result -----------------------
            Assert.AreEqual(2, outputsRegion.Outputs.Count);
            Assert.AreEqual("[[Person().Firstname]]", outputsRegion.Outputs.First().MappedTo);
            Assert.AreEqual("[[Person().Surname]]", outputsRegion.Outputs.Last().MappedTo);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(OutputsRegion))]
        public void OutputsRegion_ViewObjectResult()
        {
            //---------------Set up test pack-------------------
            var updatedOutputs = new List<IServiceOutputMapping>
            {
                new ServiceOutputMapping { MappedFrom = "Name", MappedTo = "[[Person().Firstname]]", RecordSetName = "Person"},
            };
            var act = new DsfWebGetActivity
            {
                SourceId = Guid.NewGuid(),
                Outputs = new List<IServiceOutputMapping>(),
                IsObject = true,
            };

            var mockJsonObjectsView = new Mock<IJsonObjectsView>();
            mockJsonObjectsView.Setup(o => o.ShowJsonString(It.IsAny<string>())).Verifiable();
            CustomContainer.RegisterInstancePerRequestType<IJsonObjectsView>(() => mockJsonObjectsView.Object);

            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act), true);
            outputsRegion.Outputs.Add(new ServiceOutputMapping { MappedFrom = "Name", MappedTo = "[[Person().Name]]", RecordSetName = "Person" });
            //---------------Assert Precondition----------------
            Assert.AreEqual(1, outputsRegion.Outputs.Count);
            Assert.AreEqual("[[Person().Name]]", outputsRegion.Outputs.First().MappedTo);
            //---------------Execute Test ----------------------
            outputsRegion.ResetOutputs(updatedOutputs);
            //---------------Test Result -----------------------
            outputsRegion.ViewObjectResult.Execute(null);

            mockJsonObjectsView.Verify(o => o.ShowJsonString(It.IsAny<string>()), Times.Once);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(OutputsRegion))]
        public void OutputsRegion_Errors_Expect_NoObjectName()
        {
            //---------------Set up test pack-------------------
            var updatedOutputs = new List<IServiceOutputMapping>
            {
                new ServiceOutputMapping { MappedFrom = "Name", MappedTo = "[[Person().Firstname]]", RecordSetName = "Person"},
            };
            var act = new DsfWebGetActivity
            {
                SourceId = Guid.NewGuid(),
                Outputs = new List<IServiceOutputMapping>(),
                IsObject = true,
            };
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act), true);
            outputsRegion.Outputs.Add(new ServiceOutputMapping { MappedFrom = "Name", MappedTo = "[[Person().Name]]", RecordSetName = "Person" });
            //---------------Assert Precondition----------------
            Assert.AreEqual(1, outputsRegion.Outputs.Count);
            Assert.AreEqual("[[Person().Name]]", outputsRegion.Outputs.First().MappedTo);
            //---------------Execute Test ----------------------
            outputsRegion.ResetOutputs(updatedOutputs);
            //---------------Test Result -----------------------
            Assert.AreEqual(1, outputsRegion.Outputs.Count);
            Assert.AreEqual("[[Person().Firstname]]", outputsRegion.Outputs.First().MappedTo);

            Assert.AreEqual(1, outputsRegion.Errors.Count);
            Assert.AreEqual("Please enter an 'Object Name' as 'Is Object' has been selected.", outputsRegion.Errors[0]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(OutputsRegion))]
        public void OutputsRegion_Errors_Expect_Parse_Error()
        {
            //---------------Set up test pack-------------------
            var act = new DsfWebGetActivity
            {
                SourceId = Guid.NewGuid(),
                Outputs = new List<IServiceOutputMapping>(),
                IsObject = false,
            };
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act), true);
            outputsRegion.Outputs.Add(new ServiceOutputMapping { MappedFrom = "Name", MappedTo = "[[Person().Name", RecordSetName = "Person" });
            //---------------Assert Precondition----------------
            Assert.AreEqual(1, outputsRegion.Outputs.Count);
            Assert.AreEqual("[[Person().Name", outputsRegion.Outputs.First().MappedTo);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual(1, outputsRegion.Errors.Count);
            Assert.AreEqual("parse error", outputsRegion.Errors[0]);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory(nameof(OutputsRegion))]
        public void OutputsRegion_Errors_Expect_No_Error()
        {
            //---------------Set up test pack-------------------
            var act = new DsfWebGetActivity
            {
                SourceId = Guid.NewGuid(),
                Outputs = new List<IServiceOutputMapping>(),
                IsObject = false,
            };
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act), true);
            outputsRegion.Outputs.Add(new ServiceOutputMapping { MappedFrom = "Name", MappedTo = "[[Person().Name]]", RecordSetName = "Person" });
            //---------------Assert Precondition----------------
            Assert.AreEqual(1, outputsRegion.Outputs.Count);
            Assert.AreEqual("[[Person().Name]]", outputsRegion.Outputs.First().MappedTo);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual(0, outputsRegion.Errors.Count);
        }
    }
}