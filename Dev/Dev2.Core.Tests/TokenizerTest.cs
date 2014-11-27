
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Dev2.Common;
using Dev2.Common.Interfaces.StringTokenizer.Interfaces;
using Dev2.Tests.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Unlimited.UnitTest.Framework {
    /// <summary>
    /// Summary description for TokenizerTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class TokenizerTest {
        public TokenizerTest() {
            //
            // TODO: Add constructor logic here
            //
        }

        private static string search = "AB-CD-DE-FG-HI";
        private static string search2 = "AB-CD-AB-CD";

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext {
            get {
                return testContextInstance;
            }
            set {
                testContextInstance = value;
            }
        }

        #region Fwd Test
        [TestMethod]
        public void Single_Token_Op_Fwd() {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder();

            dtb.ToTokenize = search;

            dtb.AddTokenOp("-", false);

            IDev2Tokenizer dt = dtb.Generate();

            string result = string.Empty;

            while (dt.HasMoreOps()) {
               result += dt.NextToken();
            }

            Assert.AreEqual("ABCDDEFGHI", result);
        }

        [TestMethod]
        public void Two_Token_Op_Fwd() {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder();

            dtb.ToTokenize = search2;

            dtb.AddTokenOp("AB", false);

            IDev2Tokenizer dt = dtb.Generate();

            string result = string.Empty;

            while (dt.HasMoreOps()) {
                result += " "+ dt.NextToken();
            }

            Assert.AreEqual("  -CD- -CD", result);
        }

        [TestMethod]
        public void Three_Token_Op_Fwd() {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder();

            dtb.ToTokenize = search2;

            dtb.AddTokenOp("AB-", false);

            IDev2Tokenizer dt = dtb.Generate();

            string result = string.Empty;

            while (dt.HasMoreOps()) {
                result += " " + dt.NextToken();
            }

            Assert.AreEqual("  CD- CD", result);
        }

        [TestMethod]
        public void Token_Op_With_Token_Fwd() {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder();

            dtb.ToTokenize = search;

            dtb.AddTokenOp("-", true);

            IDev2Tokenizer dt = dtb.Generate();

            string result = string.Empty;

            while (dt.HasMoreOps()) {
                result += dt.NextToken();
            }

            Assert.AreEqual("AB-CD-DE-FG-HI", result);
        }

        [TestMethod]
        public void Index_Op_Fwd() {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder();

            dtb.ToTokenize = search;

            dtb.AddIndexOp(2);

            IDev2Tokenizer dt = dtb.Generate();

            string result = string.Empty;

            while (dt.HasMoreOps()) {
                result += " "+dt.NextToken();
            }

            Assert.AreEqual(" AB -C D- DE -F G- HI", result);
        }

        [TestMethod]
        public void Eof_Op_Fwd() {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder();

            dtb.ToTokenize = search;
      
            dtb.AddEoFOp();

            IDev2Tokenizer dt = dtb.Generate();

            string result = string.Empty;

            int cnt = 0;
            while (dt.HasMoreOps()) {
                result +=  dt.NextToken();
                cnt++;
            }

            Assert.AreEqual("AB-CD-DE-FG-HI", result);
            Assert.IsTrue(cnt == 1);
        }

        //18.09.2012: massimo.guerrera - Added from a bug that wasnt splitting on a end operation after another operation.
        [TestMethod]
        public void More_Then_One_Op_Fwd()
        {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder();

            dtb.ToTokenize = search;

            dtb.AddIndexOp(2);
            dtb.AddEoFOp();

            IDev2Tokenizer dt = dtb.Generate();

            string result = string.Empty;

            int cnt = 0;
            while (dt.HasMoreOps())
            {
                result += dt.NextToken();
                cnt++;
            }

            Assert.AreEqual("AB-CD-DE-FG-HI", result);
            Assert.IsTrue(cnt == 2);
        }


        [TestMethod]
        public void Token_And_Index_Op_Fwd() {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder();

            dtb.ToTokenize = search;

            dtb.AddTokenOp("-", false);
            dtb.AddIndexOp(3);

            IDev2Tokenizer dt = dtb.Generate();

            string result = string.Empty;

            while (dt.HasMoreOps()) {
                result += " " + dt.NextToken();
            }

            Assert.AreEqual(" AB CD- DE FG- HI", result);
        }

        #endregion

        #region Backward Test

        [TestMethod]
        public void Single_Token_Op_Bwd() {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder();

            dtb.ReverseOrder = true;
            dtb.ToTokenize = search;

            dtb.AddTokenOp("-", false);

            IDev2Tokenizer dt = dtb.Generate();

            string result = string.Empty;

            while (dt.HasMoreOps()) {
                result += "."+ dt.NextToken();
            }

            Assert.AreEqual(".HI.FG.DE.CD.AB", result);
        }

        [TestMethod]
        public void Two_Token_Op_Bwd() {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder();

            dtb.ToTokenize = search2;
            dtb.ReverseOrder = true;

            dtb.AddTokenOp("B-", false);

            IDev2Tokenizer dt = dtb.Generate();

            string result = string.Empty;

            while (dt.HasMoreOps()) {
                result += "." + dt.NextToken();
            }

            Assert.AreEqual(".CD.CD-A.A", result);
        }

        [TestMethod]
        public void Three_Token_Op_Bwd() {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder();

            dtb.ToTokenize = search2;
            dtb.ReverseOrder = true;

            dtb.AddTokenOp("AB-", false);

            IDev2Tokenizer dt = dtb.Generate();

            string result = string.Empty;

            while (dt.HasMoreOps()) {
                result += "." + dt.NextToken();
            }

            Assert.AreEqual(".CD.CD-", result);
        }

        [TestMethod]
        public void Token_Op_With_Token_Bwd() {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder();

            dtb.ToTokenize = search;
            dtb.ReverseOrder = true;

            dtb.AddTokenOp("-", true);

            IDev2Tokenizer dt = dtb.Generate();

            string result = string.Empty;

            while (dt.HasMoreOps()) {
                result += "."+dt.NextToken();
            }

            Assert.AreEqual(".-HI.-FG.-DE.-CD.AB", result);
        }

        [TestMethod]
        public void Index_Op_Bwd() {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder();

            dtb.ToTokenize = search;
            dtb.ReverseOrder = true;

            dtb.AddIndexOp(2);

            IDev2Tokenizer dt = dtb.Generate();

            string result = string.Empty;

            while (dt.HasMoreOps()) {
                result += "." + dt.NextToken();
            }

            Assert.AreEqual(".HI.G-.-F.DE.D-.-C.AB", result);
        }

        [TestMethod]
        public void Eof_Op_Bwd() {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder();

            dtb.ToTokenize = search;
            dtb.ReverseOrder = true;

            dtb.AddEoFOp();

            IDev2Tokenizer dt = dtb.Generate();

            string result = string.Empty;

            int cnt = 0;
            while (dt.HasMoreOps()) {
                result += dt.NextToken();
                cnt++;
            }

            Assert.AreEqual("AB-CD-DE-FG-HI", result);
            Assert.IsTrue(cnt == 1);
        }

        [TestMethod]
        public void Token_And_Index_Op_Bwd() {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder();

            dtb.ToTokenize = search;
            dtb.ReverseOrder = true;

            dtb.AddTokenOp("-", false);
            dtb.AddIndexOp(3);

            IDev2Tokenizer dt = dtb.Generate();

            string result = string.Empty;

            while (dt.HasMoreOps()) {
                result += "." + dt.NextToken();
            }

            Assert.AreEqual(".HI.-FG.DE.-CD.AB", result);
        }

        #endregion

        #region Negative Test
        [TestMethod]
        public void Empty_String_Error() {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder();

            dtb.ToTokenize = string.Empty;

            dtb.AddEoFOp();

            try {
                IDev2Tokenizer dt = dtb.Generate();

                Assert.Fail();
            }
            catch (Exception) {
                Assert.IsTrue(1 == 1);
            }
        }

        #endregion

        #region Performance Test

        [TestMethod]
        public void Single_Token_Perfomance_Op() {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder();

            dtb.ToTokenize = TestStrings.tokenizerBase;

            dtb.AddTokenOp("-", false);

            IDev2Tokenizer dt = dtb.Generate();

            int opCnt = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (dt.HasMoreOps() && opCnt < 100000) {
                dt.NextToken();
                opCnt++;
            }
            sw.Stop();

            long exeTime = sw.ElapsedMilliseconds;

            // can we do 100k ops in less then 100s? 
            // I sure hope so ;)
            Console.WriteLine("Total Time : " + exeTime);
            Assert.IsTrue(opCnt == 100000 && exeTime < 1000, "It took [ " + exeTime + " ]");
        }

        [TestMethod]
        public void Three_Token_Perfomance_Op() {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder();

            dtb.ToTokenize = TestStrings.tokenizerBase;

            dtb.AddTokenOp("AB-", false);

            IDev2Tokenizer dt = dtb.Generate();

            int opCnt = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (dt.HasMoreOps() && opCnt < 35000) {
                string result = dt.NextToken();
                opCnt++;
            }
            sw.Stop();

            long exeTime = sw.ElapsedMilliseconds;

            // can we do it in less then 2.5s? 
            // I sure hope so ;)
            Console.WriteLine("Total Time : " + exeTime);
            Assert.IsTrue(opCnt == 35000 && exeTime < 2500, "It took [ " + exeTime + " ]");
        }

        #endregion
    }
}
