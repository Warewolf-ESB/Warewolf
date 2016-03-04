using System;
using System.Collections.Generic;
using Dev2.Activities.Designers2.Core;
using Dev2.Common.Interfaces.DB;
using Dev2.Studio.Core.Activities.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Core;

namespace Dev2.Activities.Designers.Tests.Core
{
    [TestClass]
    public class WebOutputsRegion
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("OutputsRegion_Ctor")]
        public void OutputsRegion_Ctor_ValidModelItem_ExpectVisibleAndValidData()
        {
            //------------Setup for test--------------------------
            var act = new DsfWebGetActivity() { SourceId = Guid.NewGuid(), Outputs = new List<IServiceOutputMapping>{new ServiceOutputMapping("a","b","c")}};
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act));
            
            //------------Execute Test---------------------------
            Assert.IsTrue(outputsRegion.IsVisible);
            Assert.AreEqual(outputsRegion.CurrentHeight,60);
            Assert.AreEqual(outputsRegion.MaxHeight, 75);
            Assert.AreEqual(outputsRegion.MinHeight, 60);
            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("OutputsRegion_Ctor")]
        public void OutputsRegion_Ctor_NewModelItem_ExpectVisibleAndValidData()
        {
            //------------Setup for test--------------------------
            var act = new DsfWebGetActivity() { SourceId = Guid.NewGuid(), Outputs = null};
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act));

            //------------Execute Test---------------------------
            Assert.IsFalse(outputsRegion.IsVisible);
            Assert.AreEqual(60,outputsRegion.CurrentHeight);
            Assert.AreEqual(60,outputsRegion.MaxHeight);
            Assert.AreEqual(60,outputsRegion.MinHeight);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("OutputsRegion_Ctor")]
        public void OutputsRegion_AddSomeOutputsExpectHeightChanged()
        {
            //------------Setup for test--------------------------
            var act = new DsfWebGetActivity() { SourceId = Guid.NewGuid(), Outputs = null };
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act));
            outputsRegion.Outputs.Add(new ServiceOutputMapping());
            outputsRegion.Outputs.Add(new ServiceOutputMapping());
            outputsRegion.Outputs.Add(new ServiceOutputMapping());
            outputsRegion.Outputs.Add(new ServiceOutputMapping());
            //------------Execute Test---------------------------
            Assert.IsFalse(outputsRegion.IsVisible);
            Assert.AreEqual(90, outputsRegion.CurrentHeight);
            Assert.AreEqual(135, outputsRegion.MaxHeight);
            Assert.AreEqual(90, outputsRegion.MinHeight);
            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("OutputsRegion_Ctor")]
        public void OutputsRegion_Clone()
        {
            //------------Setup for test--------------------------
            var act = new DsfWebGetActivity() { SourceId = Guid.NewGuid(), Outputs = null };
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act));
            outputsRegion.Outputs.Add(new ServiceOutputMapping());
            outputsRegion.Outputs.Add(new ServiceOutputMapping());
            outputsRegion.Outputs.Add(new ServiceOutputMapping());
            outputsRegion.Outputs.Add(new ServiceOutputMapping());
            //------------Execute Test---------------------------
            Assert.IsFalse(outputsRegion.IsVisible);
            Assert.AreEqual(90, outputsRegion.CurrentHeight);
            Assert.AreEqual(135, outputsRegion.MaxHeight);
            Assert.AreEqual(90, outputsRegion.MinHeight);

            var x = outputsRegion.CloneRegion() as OutputsRegion;
            //------------Assert Results-------------------------
            Assert.IsNotNull(x, "x != null");
            Assert.AreEqual(x.Outputs.Count,4);
            Assert.AreEqual(x.CurrentHeight,outputsRegion.CurrentHeight);
            Assert.AreEqual(x.MaxHeight, outputsRegion.MaxHeight);
            Assert.AreEqual(x.MinHeight, outputsRegion.MinHeight);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("OutputsRegion_Ctor")]
        public void OutputsRegion_Restore()
        {
            //------------Setup for test--------------------------
            var act = new DsfWebGetActivity() { SourceId = Guid.NewGuid(), Outputs = null };
            var outputsRegion = new OutputsRegion(ModelItemUtils.CreateModelItem(act));
            outputsRegion.Outputs.Add(new ServiceOutputMapping());
            outputsRegion.Outputs.Add(new ServiceOutputMapping());
            outputsRegion.Outputs.Add(new ServiceOutputMapping());
            outputsRegion.Outputs.Add(new ServiceOutputMapping());
            
            //------------Execute Test---------------------------
            Assert.IsFalse(outputsRegion.IsVisible);
            Assert.AreEqual(90, outputsRegion.CurrentHeight);
            Assert.AreEqual(135, outputsRegion.MaxHeight);
            Assert.AreEqual(90, outputsRegion.MinHeight);

            var x = outputsRegion.CloneRegion() as OutputsRegion;
            outputsRegion.Outputs.Clear();
            Assert.IsFalse(outputsRegion.IsVisible);
            Assert.AreEqual(60, outputsRegion.CurrentHeight);
            Assert.AreEqual(75, outputsRegion.MaxHeight);
            Assert.AreEqual(60, outputsRegion.MinHeight);

            //------------Assert Results-------------------------
            outputsRegion.RestoreRegion(x);
            Assert.IsFalse(outputsRegion.IsVisible);
            Assert.AreEqual(90, outputsRegion.CurrentHeight);
            Assert.AreEqual(135, outputsRegion.MaxHeight);
            Assert.AreEqual(90, outputsRegion.MinHeight);
        }


    }
}