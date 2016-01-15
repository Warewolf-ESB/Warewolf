
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.InterfaceImplementors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests
{
    [TestClass]
    public class CompositeIntellisenseProviderTests
    {

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CompositeIntellisenseProvider_Constructor")]
        public void CompositeIntellisenseProvider_Constructor_Construct_ExpectNonOptionalProvider()
        {
            //------------Setup for test--------------------------
            var compositeIntellisenseProvider = new CompositeIntellisenseProvider();
            
            //------------Execute Test---------------------------

            //------------Assert Results-------------------------
            Assert.IsFalse(compositeIntellisenseProvider.Optional);
        }

        [TestMethod,ExpectedException(typeof(NotSupportedException))]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CompositeIntellisenseProvider_PerformResultInsertion")]
        public void CompositeIntellisenseProvider_PerformResultInsertion_ExpectException()
        {
            //------------Setup for test--------------------------
            var compositeIntellisenseProvider = new CompositeIntellisenseProvider();

            compositeIntellisenseProvider.PerformResultInsertion("", null);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CompositeIntellisenseProvider_Dispose")]
        public void CompositeIntellisenseProvider_Dispose_ExpectAllProvidersDisposed()
        {
            //------------Setup for test--------------------------
            var compositeIntellisenseProvider = new CompositeIntellisenseProvider();
            var mockProviders = TestUtil.GenerateMockEnumerable<IIntellisenseProvider>(3).ToList();
             mockProviders.ForEach(a=>a.Setup(b=>b.Dispose()).Verifiable());
            compositeIntellisenseProvider.AddRange(TestUtil.ProxiesFromMockEnumerable(mockProviders));
            compositeIntellisenseProvider.Dispose();
            mockProviders.ForEach(a=>a.Verify(b=>b.Dispose()));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CompositeIntellisenseProvider_PerformResultInsertion")]
        public void CompositeIntellisenseProvider_GetIntellisenseResults_OptionalProviderReturnsWhenEntireResultsetSelected()
        {
            //------------Setup for test--------------------------
            var compositeIntellisenseProvider = new CompositeIntellisenseProvider();
            var mockProviders = TestUtil.GenerateMockEnumerable<IIntellisenseProvider>(3).ToList();
            mockProviders.ForEach(a => a.Setup(b => b.Dispose()).Verifiable());
            compositeIntellisenseProvider.AddRange(TestUtil.ProxiesFromMockEnumerable(mockProviders));

            IList<IntellisenseProviderResult> results = new List<IntellisenseProviderResult> { new IntellisenseProviderResult(mockProviders[0].Object, "bob", "dave") };

            IList<IntellisenseProviderResult> results2 = new List<IntellisenseProviderResult> { new IntellisenseProviderResult(mockProviders[0].Object, "d", "x") };
        
            mockProviders[0].Setup(a => a.GetIntellisenseResults(It.IsAny<IntellisenseProviderContext>()))
                            .Returns(results);
            mockProviders[0].Setup(a => a.Optional).Returns(true);

            mockProviders[1].Setup(a => a.GetIntellisenseResults(It.IsAny<IntellisenseProviderContext>()))
                   .Returns(new List<IntellisenseProviderResult>());
            mockProviders[1].Setup(a => a.Optional).Returns(false);

            mockProviders[2].Setup(a => a.GetIntellisenseResults(It.IsAny<IntellisenseProviderContext>()))
                   .Returns(results2);
            mockProviders[2].Setup(a => a.Optional).Returns(true);

            var res =
                compositeIntellisenseProvider.GetIntellisenseResults(new IntellisenseProviderContext
                    {
                        CaretPosition = 3,
                        InputText = "bob",
                        DesiredResultSet = IntellisenseDesiredResultSet.EntireSet
                    });
            
            Assert.AreEqual(2,res.Count);
            Assert.AreEqual(results.First() ,res[0]);

            Assert.AreEqual(results2.First(), res[1]);

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CompositeIntellisenseProvider_PerformResultInsertion")]
        public void CompositeIntellisenseProvider_GetIntellisenseResults_OptionalProviderReturnsWhenIfError()
        {
            //------------Setup for test--------------------------
            var compositeIntellisenseProvider = new CompositeIntellisenseProvider();
            var mockProviders = TestUtil.GenerateMockEnumerable<IIntellisenseProvider>(3).ToList();
            mockProviders.ForEach(a => a.Setup(b => b.Dispose()).Verifiable());
            compositeIntellisenseProvider.AddRange(TestUtil.ProxiesFromMockEnumerable(mockProviders));

            IList<IntellisenseProviderResult> results = new List<IntellisenseProviderResult> { new IntellisenseProviderResult(mockProviders[0].Object, "bob", "dave","monkeys",true) };

            IList<IntellisenseProviderResult> results2 = new List<IntellisenseProviderResult> { new IntellisenseProviderResult(mockProviders[0].Object, "d", "x") };

            mockProviders[0].Setup(a => a.GetIntellisenseResults(It.IsAny<IntellisenseProviderContext>()))
                            .Returns(results);
            mockProviders[0].Setup(a => a.Optional).Returns(false);

            mockProviders[1].Setup(a => a.GetIntellisenseResults(It.IsAny<IntellisenseProviderContext>()))
                   .Returns(new List<IntellisenseProviderResult>());
            mockProviders[1].Setup(a => a.Optional).Returns(false);

            mockProviders[2].Setup(a => a.GetIntellisenseResults(It.IsAny<IntellisenseProviderContext>()))
                   .Returns(results2);
            mockProviders[2].Setup(a => a.Optional).Returns(true);

            var res =
                compositeIntellisenseProvider.GetIntellisenseResults(new IntellisenseProviderContext
                {
                    CaretPosition = 3,
                    InputText = "bob",
                    DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
                });

            Assert.AreEqual(2, res.Count);


            Assert.AreEqual(results2.First(), res[1]);
            Assert.AreEqual(results.First(), res[0]);
            Assert.IsTrue(res[0].IsError);
        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CompositeIntellisenseProvider_PerformResultInsertion")]
        public void CompositeIntellisenseProvider_GetIntellisenseResults_OptionalProviderDoesNotReturnsWhenNoErrors()
        {
            //------------Setup for test--------------------------
            var compositeIntellisenseProvider = new CompositeIntellisenseProvider();
            var mockProviders = TestUtil.GenerateMockEnumerable<IIntellisenseProvider>(3).ToList();
            mockProviders.ForEach(a => a.Setup(b => b.Dispose()).Verifiable());
            compositeIntellisenseProvider.AddRange(TestUtil.ProxiesFromMockEnumerable(mockProviders));

            IList<IntellisenseProviderResult> results = new List<IntellisenseProviderResult> { new IntellisenseProviderResult(mockProviders[0].Object, "bob", "dave", "monkeys", false) };

            IList<IntellisenseProviderResult> results2 = new List<IntellisenseProviderResult> { new IntellisenseProviderResult(mockProviders[0].Object, "d", "x") };

            mockProviders[0].Setup(a => a.GetIntellisenseResults(It.IsAny<IntellisenseProviderContext>()))
                            .Returns(results);
            mockProviders[0].Setup(a => a.Optional).Returns(false);

            mockProviders[1].Setup(a => a.GetIntellisenseResults(It.IsAny<IntellisenseProviderContext>()))
                   .Returns(new List<IntellisenseProviderResult>());
            mockProviders[1].Setup(a => a.Optional).Returns(false);

            mockProviders[2].Setup(a => a.GetIntellisenseResults(It.IsAny<IntellisenseProviderContext>()))
                   .Returns(results2);
            mockProviders[2].Setup(a => a.Optional).Returns(true);

            var res =
                compositeIntellisenseProvider.GetIntellisenseResults(new IntellisenseProviderContext
                {
                    CaretPosition = 3,
                    InputText = "bob",
                    DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
                });

            Assert.AreEqual(1, res.Count);


            Assert.AreEqual(results.First(), res.First());

        }


        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("CompositeIntellisenseProvider_PerformResultInsertion")]
        public void CompositeIntellisenseProvider_GetIntellisenseResults_ContextAtEndAndDefault_ExpectEmpty()
        {
            //------------Setup for test--------------------------
            var compositeIntellisenseProvider = new CompositeIntellisenseProvider();
            var mockProviders = TestUtil.GenerateMockEnumerable<IIntellisenseProvider>(3).ToList();
            mockProviders.ForEach(a => a.Setup(b => b.Dispose()).Verifiable());
            var proxiesFromMockEnumerable = TestUtil.ProxiesFromMockEnumerable(mockProviders).ToList();
            compositeIntellisenseProvider.AddRange(proxiesFromMockEnumerable);

            IList<IntellisenseProviderResult> results = new List<IntellisenseProviderResult> { new IntellisenseProviderResult(mockProviders[0].Object, "bob", "dave", "monkeys", false) };

            IList<IntellisenseProviderResult> results2 = new List<IntellisenseProviderResult> { new IntellisenseProviderResult(mockProviders[0].Object, "d", "x") };

            mockProviders[0].Setup(a => a.GetIntellisenseResults(It.IsAny<IntellisenseProviderContext>()))
                            .Returns(results);
            mockProviders[0].Setup(a => a.Optional).Returns(false);
            mockProviders[0].Setup(a => a.IntellisenseProviderType).Returns(IntellisenseProviderType.Default);
            mockProviders[1].Setup(a => a.GetIntellisenseResults(It.IsAny<IntellisenseProviderContext>()))
                   .Returns(new List<IntellisenseProviderResult>());
            mockProviders[1].Setup(a => a.Optional).Returns(false);

            mockProviders[2].Setup(a => a.GetIntellisenseResults(It.IsAny<IntellisenseProviderContext>()))
                   .Returns(results2);
            mockProviders[2].Setup(a => a.Optional).Returns(true);

            var res =
                compositeIntellisenseProvider.GetIntellisenseResults(new IntellisenseProviderContext
                {
                    CaretPosition = 4,
                    InputText = "bob)",
                    DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
                });

            Assert.AreEqual(0, res.Count);

        }

    }
}
