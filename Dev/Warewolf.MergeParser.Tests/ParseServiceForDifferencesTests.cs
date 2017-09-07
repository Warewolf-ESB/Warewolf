using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Activities.Presentation.Model;
using Dev2.Studio.Core.Activities.Utils;

namespace Warewolf.MergeParser.Tests
{
    [TestClass]
    public class ParseServiceForDifferencesTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullMergeHead_ShouldThrowException()
        {
            var psd = new ParseServiceForDifferences(null, ModelItemUtils.CreateModelItem());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullHead_ShouldThrowException()
        {
            var psd = new ParseServiceForDifferences(ModelItemUtils.CreateModelItem(), null);
        }
    }
}
