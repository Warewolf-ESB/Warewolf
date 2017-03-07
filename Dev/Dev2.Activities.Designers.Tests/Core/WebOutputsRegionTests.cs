using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.DB;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Core;
// ReSharper disable InconsistentNaming

namespace Dev2.Activities.Designers.Tests.Core
{
    [TestClass]
    public class WebOutputsRegionTests
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("OutputsRegion_Ctor")]
        public void OutputsRegion_Ctor_ValidModelItem_ExpectVisibleAndValidData()
        {
            //------------Setup for test--------------------------
            var act = new DsfWebGetActivity() { SourceId = Guid.NewGuid(), Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("a", "b", "c") } };
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act), true);

            //------------Execute Test---------------------------
            Assert.IsTrue(outputsRegion.IsEnabled);
            Assert.IsTrue(outputsRegion.IsObjectOutputUsed);
            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("OutputsRegion_Ctor")]
        public void OutputsRegion_Ctor_NewModelItem_ExpectVisibleAndValidData()
        {
            //------------Setup for test--------------------------
            var act = new DsfWebGetActivity() { SourceId = Guid.NewGuid(), Outputs = null };
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act), true);

            //------------Execute Test---------------------------
            Assert.IsFalse(outputsRegion.IsEnabled);
            Assert.IsTrue(outputsRegion.IsObjectOutputUsed);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("OutputsRegion_Ctor")]
        public void OutputsRegion_AddSomeOutputsExpectHeightChanged()
        {
            //------------Setup for test--------------------------
            var act = new DsfWebGetActivity() { SourceId = Guid.NewGuid(), Outputs = null };
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
        [TestCategory("OutputsRegion_Ctor")]
        public void OutputsRegion_Clone()
        {
            //------------Setup for test--------------------------
            var act = new DsfWebGetActivity() { SourceId = Guid.NewGuid(), Outputs = null };
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
            Assert.AreEqual(x.Outputs.Count, 4);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("OutputsRegion_Ctor")]
        public void OutputsRegion_Restore()
        {
            //------------Setup for test--------------------------
            var act = new DsfWebGetActivity() { SourceId = Guid.NewGuid(), Outputs = null };
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
        public void Outputs_GivenNull_ShouldUpdatesModelItem()
        {
            //---------------Set up test pack-------------------
            var act = new DsfWebGetActivity() { SourceId = Guid.NewGuid(), Outputs = null };
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act), true);
            bool wasCalled = false;
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
        public void Outputs_GivenNull_ShouldAttachEventHandlers()
        {
            //---------------Set up test pack-------------------
            var act = new DsfWebGetActivity() { SourceId = Guid.NewGuid(), Outputs = null };
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act), true);
            var serviceOutputMapping = new ServiceOutputMapping()
            {
                MappedFrom = "j",
                MappedTo = "H"
            };
            var outPutsChanged = false;
            serviceOutputMapping.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "MappedFrom")
                    outPutsChanged = true;
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(outputsRegion.Outputs);
            //---------------Execute Test ----------------------
            outputsRegion.Outputs.Add(serviceOutputMapping);
            outputsRegion.Outputs.First().MappedFrom = "b";
            //---------------Test Result -----------------------
            Assert.IsTrue(outPutsChanged);
        }
    }
}