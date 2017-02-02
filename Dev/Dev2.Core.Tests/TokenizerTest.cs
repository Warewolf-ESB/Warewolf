/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics;
using Dev2.Common;
using Dev2.Common.Interfaces.StringTokenizer.Interfaces;
using Dev2.Tests.Properties;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming
// ReSharper disable LocalizableElement

namespace Dev2.Tests {
    /// <summary>
    /// Summary description for TokenizerTest
    /// </summary>
    [TestClass]
    public class TokenizerTest {
        const string Search = "AB-CD-DE-FG-HI";
        const string Search2 = "AB-CD-AB-CD";

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Fwd Test
        [TestMethod]
        public void Single_Token_Op_Fwd() {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder { ToTokenize = Search };


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
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder { ToTokenize = Search2 };


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
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder { ToTokenize = Search2 };


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
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder { ToTokenize = Search };


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
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder { ToTokenize = Search };


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
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder { ToTokenize = Search };


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
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder { ToTokenize = Search };


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
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder { ToTokenize = Search };


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
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder
                                       {
                                           ReverseOrder = true,
                                           ToTokenize = Search
                                       };


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
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder
                                       {
                                           ToTokenize = Search2,
                                           ReverseOrder = true
                                       };


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
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder
                                       {
                                           ToTokenize = Search2,
                                           ReverseOrder = true
                                       };


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
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder
                                       {
                                           ToTokenize = Search,
                                           ReverseOrder = true
                                       };


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
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder
                                       {
                                           ToTokenize = Search,
                                           ReverseOrder = true
                                       };


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
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder
                                       {
                                           ToTokenize = Search,
                                           ReverseOrder = true
                                       };


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
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder
                                       {
                                           ToTokenize = Search,
                                           ReverseOrder = true
                                       };


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
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder { ToTokenize = string.Empty };


            dtb.AddEoFOp();

            try
            {
                dtb.Generate();

                Assert.Fail();
            }
            catch (Exception) {
                Assert.IsTrue(true);
            }
        }

        #endregion

        #region Performance Test

        [TestMethod]
        public void Single_Token_Perfomance_Op() {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder { ToTokenize = TestStrings.tokenizerBase };


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

            // can we do 100k ops in less then 1,2s? 
            // I sure hope so ;)
            Console.WriteLine(@"Total Time : " + exeTime);
            Assert.IsTrue(opCnt == 100000 && exeTime < 1200, "Expecting it to take 1200 ms but it took " + exeTime + " ms.");
        }

        [TestMethod]
        public void Three_Token_Perfomance_Op() {
            Dev2TokenizerBuilder dtb = new Dev2TokenizerBuilder { ToTokenize = TestStrings.tokenizerBase };


            dtb.AddTokenOp("AB-", false);

            IDev2Tokenizer dt = dtb.Generate();

            int opCnt = 0;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (dt.HasMoreOps() && opCnt < 35000)
            {
                dt.NextToken();
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
