/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Warewolf.Options;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Data.Tests
{
    [TestClass]
    public class NamedIntTests
    {
        [TestMethod]
        public void NamedInt_()
        {
            var opts = NamedInt.GetAll(typeof(enDecisionType));

            Assert.AreEqual(39, opts.Count());

            var list = opts.ToList();

            Assert.AreEqual("=", list[0].Name);
            Assert.AreEqual(19, list[0].Value);

            Assert.AreEqual("<> (Not Equal)", list[1].Name);
            Assert.AreEqual(20, list[1].Value);

            Assert.AreEqual("<", list[2].Name);
            Assert.AreEqual(21, list[2].Value);

            Assert.AreEqual("<=", list[3].Name);
            Assert.AreEqual(22, list[3].Value);

            Assert.AreEqual(">", list[4].Name);
            Assert.AreEqual(23, list[4].Value);

            Assert.AreEqual(">=", list[5].Name);
            Assert.AreEqual(24, list[5].Value);
            
            Assert.AreEqual("Contains", list[6].Name);
            Assert.AreEqual(25, list[6].Value);
            
            Assert.AreEqual("Doesn't Contain", list[7].Name);
            Assert.AreEqual(26, list[7].Value);

            Assert.AreEqual("Ends With", list[8].Name);
            Assert.AreEqual(27, list[8].Value);

            Assert.AreEqual("Doesn't End With", list[9].Name);
            Assert.AreEqual(28, list[9].Value);

            Assert.AreEqual("Starts With", list[10].Name);
            Assert.AreEqual(29, list[10].Value);

            Assert.AreEqual("Doesn't Start With", list[11].Name);
            Assert.AreEqual(30, list[11].Value);

            Assert.AreEqual("Is Between", list[12].Name);
            Assert.AreEqual(31, list[12].Value);

            Assert.AreEqual("Not Between", list[13].Name);
            Assert.AreEqual(32, list[13].Value);

            Assert.AreEqual("Is Binary", list[14].Name);
            Assert.AreEqual(33, list[14].Value);

            Assert.AreEqual("Not Binary", list[15].Name);
            Assert.AreEqual(34, list[15].Value);

            Assert.AreEqual("Is Hex", list[16].Name);
            Assert.AreEqual(35, list[16].Value);

            Assert.AreEqual("Not Hex", list[17].Name);
            Assert.AreEqual(36, list[17].Value);

            Assert.AreEqual("Is Base64", list[18].Name);
            Assert.AreEqual(37, list[18].Value);

            Assert.AreEqual("Not Base64", list[19].Name);
            Assert.AreEqual(38, list[19].Value);

            Assert.AreEqual("Not a Valid Decision Type", list[20].Name);
            Assert.AreEqual(0, list[20].Value);
            
            Assert.AreEqual("There is An Error", list[21].Name);
            Assert.AreEqual(1, list[21].Value);
            
            Assert.AreEqual("There is No Error", list[22].Name);
            Assert.AreEqual(2, list[22].Value);
            
            Assert.AreEqual("Is NULL", list[23].Name);
            Assert.AreEqual(3, list[23].Value);

            Assert.AreEqual("Is Not NULL", list[24].Name);
            Assert.AreEqual(4, list[24].Value);
            
            Assert.AreEqual("Is Numeric", list[25].Name);
            Assert.AreEqual(5, list[25].Value);
            
            Assert.AreEqual("Not Numeric", list[26].Name);
            Assert.AreEqual(6, list[26].Value);
            
            Assert.AreEqual("Is Text", list[27].Name);
            Assert.AreEqual(7, list[27].Value);
            
            Assert.AreEqual("Not Text", list[28].Name);
            Assert.AreEqual(8, list[28].Value);
            
            Assert.AreEqual("Is Alphanumeric", list[29].Name);
            Assert.AreEqual(9, list[29].Value);

            Assert.AreEqual("Not Alphanumeric", list[30].Name);
            Assert.AreEqual(10, list[30].Value);
            
            Assert.AreEqual("Is XML", list[31].Name);
            Assert.AreEqual(11, list[31].Value);
            
            Assert.AreEqual("Not XML", list[32].Name);
            Assert.AreEqual(12, list[32].Value);
            
            Assert.AreEqual("Is Date", list[33].Name);
            Assert.AreEqual(13, list[33].Value);
            
            Assert.AreEqual("Not Date", list[34].Name);
            Assert.AreEqual(14, list[34].Value);

            Assert.AreEqual("Is Email", list[35].Name);
            Assert.AreEqual(15, list[35].Value);
            
            Assert.AreEqual("Not Email", list[36].Name);
            Assert.AreEqual(16, list[36].Value);
            
            Assert.AreEqual("Is Regex", list[37].Name);
            Assert.AreEqual(17, list[37].Value);

            Assert.AreEqual("Not Regex", list[38].Name);
            Assert.AreEqual(18, list[38].Value);
        }


        [TestMethod]
        public void NamedInt_GivenTestEnum_ShouldProceed()
        {
            var list = NamedInt.GetAll(typeof(MyTestEnum));
            Assert.AreEqual(3, list.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(IndexAttributeException))]
        public void NamedInt_GivenBrokenTestEnum_ExpectSuccess() => NamedInt.GetAll(typeof(MyBrokenEnumOne));

        [TestMethod]
        [ExpectedException(typeof(IndexAttributeException))]
        public void NamedInt_GivenBrokenTestEnumTwo_ShouldProceed() => NamedInt.GetAll(typeof(MyBrokenEnumTwo));

        [TestMethod]
        [ExpectedException(typeof(IndexAttributeException))]
        public void NamedInt_GivenBrokenTestEnumThree_ShouldProceed() => NamedInt.GetAll(typeof(MyBrokenEnumThree));
    }

    enum MyTestEnum
    {
        A,
        B,
        C,
    }
    enum MyBrokenEnumOne
    {
        [Index(0)] A,
        B,
        C,
    }
    enum MyBrokenEnumTwo
    {
        A,
        B,
        [Index(0)] C,
    }
    enum MyBrokenEnumThree
    {
        A,
        [Index(0)] B,
        C,
    }
}
