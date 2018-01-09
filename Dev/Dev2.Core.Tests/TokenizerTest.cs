/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common;
using Dev2.Common.Interfaces.StringTokenizer.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;



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
            var dtb = new Dev2TokenizerBuilder { ToTokenize = Search };


            dtb.AddTokenOp("-", false);

            var dt = dtb.Generate();

            var result = string.Empty;

            while (dt.HasMoreOps()) {
               result += dt.NextToken();
            }

            Assert.AreEqual("ABCDDEFGHI", result);
        }

        [TestMethod]
        public void Two_Token_Op_Fwd() {
            var dtb = new Dev2TokenizerBuilder { ToTokenize = Search2 };


            dtb.AddTokenOp("AB", false);

            var dt = dtb.Generate();

            var result = string.Empty;

            while (dt.HasMoreOps()) {
                result += " "+ dt.NextToken();
            }

            Assert.AreEqual("  -CD- -CD", result);
        }

        [TestMethod]
        public void Three_Token_Op_Fwd() {
            var dtb = new Dev2TokenizerBuilder { ToTokenize = Search2 };


            dtb.AddTokenOp("AB-", false);

            var dt = dtb.Generate();

            var result = string.Empty;

            while (dt.HasMoreOps()) {
                result += " " + dt.NextToken();
            }

            Assert.AreEqual("  CD- CD", result);
        }

        [TestMethod]
        public void Token_Op_With_Token_Fwd() {
            var dtb = new Dev2TokenizerBuilder { ToTokenize = Search };


            dtb.AddTokenOp("-", true);

            var dt = dtb.Generate();

            var result = string.Empty;

            while (dt.HasMoreOps()) {
                result += dt.NextToken();
            }

            Assert.AreEqual("AB-CD-DE-FG-HI", result);
        }

        [TestMethod]
        public void Index_Op_Fwd() {
            var dtb = new Dev2TokenizerBuilder { ToTokenize = Search };


            dtb.AddIndexOp(2);

            var dt = dtb.Generate();

            var result = string.Empty;

            while (dt.HasMoreOps()) {
                result += " "+dt.NextToken();
            }

            Assert.AreEqual(" AB -C D- DE -F G- HI", result);
        }

        [TestMethod]
        public void Eof_Op_Fwd() {
            var dtb = new Dev2TokenizerBuilder { ToTokenize = Search };


            dtb.AddEoFOp();

            var dt = dtb.Generate();

            var result = string.Empty;

            var cnt = 0;
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
            var dtb = new Dev2TokenizerBuilder { ToTokenize = Search };


            dtb.AddIndexOp(2);
            dtb.AddEoFOp();

            var dt = dtb.Generate();

            var result = string.Empty;

            var cnt = 0;
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
            var dtb = new Dev2TokenizerBuilder { ToTokenize = Search };


            dtb.AddTokenOp("-", false);
            dtb.AddIndexOp(3);

            var dt = dtb.Generate();

            var result = string.Empty;

            while (dt.HasMoreOps()) {
                result += " " + dt.NextToken();
            }

            Assert.AreEqual(" AB CD- DE FG- HI", result);
        }

        #endregion

        #region Backward Test

        [TestMethod]
        public void Single_Token_Op_Bwd() {
            var dtb = new Dev2TokenizerBuilder
                                       {
                                           ReverseOrder = true,
                                           ToTokenize = Search
                                       };


            dtb.AddTokenOp("-", false);

            var dt = dtb.Generate();

            var result = string.Empty;

            while (dt.HasMoreOps()) {
                result += "."+ dt.NextToken();
            }

            Assert.AreEqual(".HI.FG.DE.CD.AB", result);
        }

        [TestMethod]
        public void Two_Token_Op_Bwd() {
            var dtb = new Dev2TokenizerBuilder
                                       {
                                           ToTokenize = Search2,
                                           ReverseOrder = true
                                       };


            dtb.AddTokenOp("B-", false);

            var dt = dtb.Generate();

            var result = string.Empty;

            while (dt.HasMoreOps()) {
                result += "." + dt.NextToken();
            }

            Assert.AreEqual(".CD.CD-A.A", result);
        }

        [TestMethod]
        public void Three_Token_Op_Bwd() {
            var dtb = new Dev2TokenizerBuilder
                                       {
                                           ToTokenize = Search2,
                                           ReverseOrder = true
                                       };


            dtb.AddTokenOp("AB-", false);

            var dt = dtb.Generate();

            var result = string.Empty;

            while (dt.HasMoreOps()) {
                result += "." + dt.NextToken();
            }

            Assert.AreEqual(".CD.CD-", result);
        }

        [TestMethod]
        public void Token_Op_With_Token_Bwd() {
            var dtb = new Dev2TokenizerBuilder
                                       {
                                           ToTokenize = Search,
                                           ReverseOrder = true
                                       };


            dtb.AddTokenOp("-", true);

            var dt = dtb.Generate();

            var result = string.Empty;

            while (dt.HasMoreOps()) {
                result += "."+dt.NextToken();
            }

            Assert.AreEqual(".-HI.-FG.-DE.-CD.AB", result);
        }

        [TestMethod]
        public void Index_Op_Bwd() {
            var dtb = new Dev2TokenizerBuilder
                                       {
                                           ToTokenize = Search,
                                           ReverseOrder = true
                                       };


            dtb.AddIndexOp(2);

            var dt = dtb.Generate();

            var result = string.Empty;

            while (dt.HasMoreOps()) {
                result += "." + dt.NextToken();
            }

            Assert.AreEqual(".HI.G-.-F.DE.D-.-C.AB", result);
        }

        [TestMethod]
        public void Eof_Op_Bwd() {
            var dtb = new Dev2TokenizerBuilder
                                       {
                                           ToTokenize = Search,
                                           ReverseOrder = true
                                       };


            dtb.AddEoFOp();

            var dt = dtb.Generate();

            var result = string.Empty;

            var cnt = 0;
            while (dt.HasMoreOps()) {
                result += dt.NextToken();
                cnt++;
            }

            Assert.AreEqual("AB-CD-DE-FG-HI", result);
            Assert.IsTrue(cnt == 1);
        }

        [TestMethod]
        public void Token_And_Index_Op_Bwd() {
            var dtb = new Dev2TokenizerBuilder
                                       {
                                           ToTokenize = Search,
                                           ReverseOrder = true
                                       };


            dtb.AddTokenOp("-", false);
            dtb.AddIndexOp(3);

            var dt = dtb.Generate();

            var result = string.Empty;

            while (dt.HasMoreOps()) {
                result += "." + dt.NextToken();
            }

            Assert.AreEqual(".HI.-FG.DE.-CD.AB", result);
        }

        #endregion

        #region Negative Test
        [TestMethod]
        public void Empty_String_Error() {
            var dtb = new Dev2TokenizerBuilder { ToTokenize = string.Empty };


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
    }
}
