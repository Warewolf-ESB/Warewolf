using System;
using System.Collections.Generic;
using Dev2.Data.Enums;
using Dev2.Data.Interfaces;
using Dev2.DataList.Contract;
using Dev2.Studio.Core.Interfaces;
using Dev2.Studio.InterfaceImplementors;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Core.Tests.IntellisenseProvider
{
    [TestClass]
   // ReSharper disable InconsistentNaming
    public class DateTimeIntellisenseProviderTests
    {
        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DateTimeIntellisenseProvider_Construct")]
        public void DateTimeIntellisenseProvider_Construct_DefaultPropertiesAreSet()
        {
            var provider = new DateTimeIntellisenseProvider();

            Assert.IsFalse(provider.HandlesResultInsertion);
            Assert.AreEqual(IntellisenseProviderType.NonDefault, provider.IntellisenseProviderType);
            Assert.IsNotNull(provider.IntellisenseResults);
            Assert.AreEqual(24, provider.IntellisenseResults.Count);
            Assert.IsFalse(provider.Optional);
        }
        
        [TestMethod]
        [Owner("Massimo Guerrera")]
        [TestCategory("DateTimeIntellisenseProvider_GetIntellisenseResults")]
        public void DateTimeIntellisenseProvider_GetIntellisenseResults_PartialMethodMatch_ClosestMatchesReturned()
        {
            var context = new IntellisenseProviderContext
                {
                    CaretPosition = 1,
                    InputText = "d",
                    IsInCalculateMode = false,
                    DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
                };

            var dateTimeIntellisenseProvider = new DateTimeIntellisenseProvider();
            IList<IntellisenseProviderResult> results = dateTimeIntellisenseProvider.GetIntellisenseResults(context);

            Assert.AreEqual(6, results.Count);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DateTimeIntellisenseProvider_Constructor")]
        public void DateTimeIntellisenseProvider_Constructor_Create_ExpectCorrectMembers()
        {
            //------------Setup for test--------------------------
            var dateTimeIntellisenseProvider = new DateTimeIntellisenseProvider();
            
      

            //------------Assert Results-------------------------
            Assert.IsNotNull(dateTimeIntellisenseProvider.Optional);
            Assert.IsTrue(dateTimeIntellisenseProvider.IntellisenseResults.Count >0);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DateTimeIntellisenseProvider_Constructor")]
        public void DateTimeIntellisenseProvider_Constructor_Parameters_ExpectCorrectMembers()
        {
            //------------Setup for test--------------------------
            var res = new Mock<IIntellisenseResult>();
            res.Setup(a => a.ErrorCode).Returns(enIntellisenseErrorCode.InvalidRecordsetNotation);
            res.Setup(a => a.Type).Returns(enIntellisenseResultType.Error);
            res.Setup(a => a.IsClosedRegion).Returns(true);
            var dateTimeIntellisenseProvider = new DateTimeIntellisenseProvider(new List<IIntellisenseResult> { res.Object });

            //------------Assert Results-------------------------
            Assert.IsNotNull(dateTimeIntellisenseProvider.Optional);
            Assert.IsTrue(dateTimeIntellisenseProvider.IntellisenseResults.Count == 1);
            Assert.AreEqual(res.Object,dateTimeIntellisenseProvider.IntellisenseResults[0]);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DateTimeIntellisenseProvider_Constructor")]
        public void DateTimeIntellisenseProvider_Dispose()
        {
            //------------Setup for test--------------------------
            var dateTimeIntellisenseProvider = new DateTimeIntellisenseProvider();
            dateTimeIntellisenseProvider.Dispose();


            //------------Assert Results-------------------------
            Assert.IsNull(dateTimeIntellisenseProvider.IntellisenseResults);

        }

        [TestMethod,ExpectedException(typeof(NotSupportedException))]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DateTimeIntellisenseProvider_PerformInsertion")]
        public void DateTimeIntellisenseProvider_PerformInsertion_ExpectException()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
                {
                    CaretPosition = 1,
                    InputText = "d",
                    IsInCalculateMode = false,
                    DesiredResultSet = IntellisenseDesiredResultSet.ClosestMatch
                };

            var dateTimeIntellisenseProvider = new DateTimeIntellisenseProvider();

            
            //------------Execute Test---------------------------

            dateTimeIntellisenseProvider.PerformResultInsertion("blah", context);

            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DateTimeIntellisenseProvider_PerformInsertion")]
        public void DateTimeIntellisenseProvider_InLiteralRegion_NotInRegion_ExpectFalse()
        {
            //------------Assert Results-------------------------
            Assert.IsFalse(DateTimeIntellisenseProvider.InLiteralRegion(@"2012/01/01", 1));
            Assert.IsFalse(DateTimeIntellisenseProvider.InLiteralRegion(@"2012/01/01", 0));
            Assert.IsFalse(DateTimeIntellisenseProvider.InLiteralRegion(@"2012/01/01", 10));
            Assert.IsFalse(DateTimeIntellisenseProvider.InLiteralRegion(@"2012\\01\01", 5));
            Assert.IsFalse(DateTimeIntellisenseProvider.InLiteralRegion(@"2012\\\''''01\01", 9));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DateTimeIntellisenseProvider_PerformInsertion")]
        public void DateTimeIntellisenseProvider_InLiteralRegion_InRegion_ExpectTrue()
        {
            //------------Assert Results-------------------------
            Assert.IsTrue(DateTimeIntellisenseProvider.InLiteralRegion(@"2012\\\''01\01", 9));

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DateTimeIntellisenseProvider_GetIntellisenseResultsImpl")]
        public void DateTimeIntellisenseProvider_GetIntellisenseResultsImpl_EntireResultSet()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 1,
                InputText = "d",
                IsInCalculateMode = false,
                DesiredResultSet = IntellisenseDesiredResultSet.EntireSet
            };

            var dateTimeIntellisenseProvider = new DateTimeIntellisenseProvider();
            var count = dateTimeIntellisenseProvider.IntellisenseResults.Count;

            //------------Execute Test---------------------------

           var results = dateTimeIntellisenseProvider.GetIntellisenseResultsImpl( context);

            //------------Assert Results-------------------------
           Assert.AreEqual(count,results.Count);

        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DateTimeIntellisenseProvider_GetIntellisenseResults")]
        public void DateTimeIntellisenseProvider_GetIntellisenseResults_ErrorResult_ExpectNoResult()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 1,
                InputText = "dod",
                IsInCalculateMode = false,
                DesiredResultSet = IntellisenseDesiredResultSet.EntireSet
            };

            var res = new Mock<IIntellisenseResult>();
            var opt = new Mock<IDataListVerifyPart>();
            opt.Setup(a => a.DisplayValue).Returns("dora");

            res.Setup(a => a.ErrorCode).Returns(enIntellisenseErrorCode.InvalidRecordsetNotation);
            res.Setup(a => a.Type).Returns(enIntellisenseResultType.Error);
            res.Setup(a => a.IsClosedRegion).Returns(false);
            res.Setup(a => a.Message).Returns("bob");
            res.Setup(a => a.Option).Returns(opt.Object);
            var dateTimeIntellisenseProvider = new DateTimeIntellisenseProvider(new List<IIntellisenseResult> {res.Object});
            var count = dateTimeIntellisenseProvider.IntellisenseResults.Count;

            //------------Execute Test---------------------------

            var results = dateTimeIntellisenseProvider.GetIntellisenseResults(context);

            //------------Assert Results-------------------------
            Assert.AreEqual(0,results.Count); // check nothing added
            Assert.AreEqual(count,1); // check that there was stuff to add
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DefaultIntellisenseProvider_DateTimeIntellisenseProvider")]
        public void DateTimeIntellisenseProvider_GetIntellisenseResults_ContextIsNull_ResultCountIsZero()
        {
            //------------Execute Test---------------------------
            var getResults = new DateTimeIntellisenseProvider().GetIntellisenseResults(null);
            //------------Assert Results-------------------------
            Assert.AreEqual(0, getResults.Count);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DateTimeIntellisenseProvider_GetIntellisenseResults")]
        public void DateTimeIntellisenseProvider_GetIntellisenseResults_ErrorResult_ExpectResultIfInClosedRegion()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 1,
                InputText = "dod",
                IsInCalculateMode = false,
                DesiredResultSet = IntellisenseDesiredResultSet.EntireSet
            };

            var res = new Mock<IIntellisenseResult>();
            var opt = new Mock<IDataListVerifyPart>();
            opt.Setup(a => a.DisplayValue).Returns("dora");

            res.Setup(a => a.ErrorCode).Returns(enIntellisenseErrorCode.InvalidRecordsetNotation);
            res.Setup(a => a.Type).Returns(enIntellisenseResultType.Error);
            res.Setup(a => a.IsClosedRegion).Returns(true);
            res.Setup(a => a.Message).Returns("bob");
            res.Setup(a => a.Option).Returns(opt.Object);
            var dateTimeIntellisenseProvider = new DateTimeIntellisenseProvider(new List<IIntellisenseResult> { res.Object });
            var count = dateTimeIntellisenseProvider.IntellisenseResults.Count;

            //------------Execute Test---------------------------

            var results = dateTimeIntellisenseProvider.GetIntellisenseResults(context);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, results.Count); // check nothing added
            Assert.AreEqual(count, 1); // check that there was stuff to add

        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DateTimeIntellisenseProvider_GetIntellisenseResults")]
        public void DateTimeIntellisenseProvider_GetIntellisenseResults_ErrorResult_ExpectResultIfNonErrorType()
        {
            //------------Setup for test--------------------------
            var context = new IntellisenseProviderContext
            {
                CaretPosition = 1,
                InputText = "dod",
                IsInCalculateMode = false,
                DesiredResultSet = IntellisenseDesiredResultSet.EntireSet
            };

            var res = new Mock<IIntellisenseResult>();
            var opt = new Mock<IDataListVerifyPart>();
            opt.Setup(a => a.DisplayValue).Returns("dora");

            res.Setup(a => a.ErrorCode).Returns(enIntellisenseErrorCode.InvalidRecordsetNotation);
            res.Setup(a => a.Type).Returns(enIntellisenseResultType.Selectable);
            res.Setup(a => a.IsClosedRegion).Returns(false);
            res.Setup(a => a.Message).Returns("bob");
            res.Setup(a => a.Option).Returns(opt.Object);
            var dateTimeIntellisenseProvider = new DateTimeIntellisenseProvider(new List<IIntellisenseResult> { res.Object });
            var count = dateTimeIntellisenseProvider.IntellisenseResults.Count;

            //------------Execute Test---------------------------

            var results = dateTimeIntellisenseProvider.GetIntellisenseResults(context);

            //------------Assert Results-------------------------
            Assert.AreEqual(1, results.Count); 
            Assert.AreEqual(count, 1); // check that there was stuff to add

        }

    }
}