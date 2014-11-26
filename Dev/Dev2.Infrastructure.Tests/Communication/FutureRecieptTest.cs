
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
using Dev2.Communication;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Infrastructure.Tests.Communication
{
    [TestClass]
    public class FutureRecieptTest
    {

        private static readonly Guid RequestID = Guid.NewGuid();

        // ReSharper disable InconsistentNaming

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("FutureReceipt_ToKey")]
        public void FutureReceipt_ToKey_WhenValidKeyParts_ExpectKey()
        {
            //------------Setup for test--------------------------
            var futureReciept = new FutureReceipt { PartID = 1, RequestID = RequestID, User = "Bob" };

            //------------Execute Test---------------------------
            var result = futureReciept.ToKey();

            //------------Assert Results-------------------------
            StringAssert.Contains(result, RequestID+"-1-Bob!");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("FutureReceipt_ToKey")]
        [ExpectedException(typeof(Exception))]
        public void FutureReceipt_ToKey_WhenPartIDLessThenZero_ExpectException()
        {
            //------------Setup for test--------------------------
            var futureReciept = new FutureReceipt { PartID = -1, RequestID = RequestID, User = "Bob" };

            //------------Execute Test---------------------------
            futureReciept.ToKey();
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("FutureReceipt_ToKey")]
        [ExpectedException(typeof(Exception))]
        public void FutureReceipt_ToKey_WhenRequestIDEmpty_ExpectException()
        {
            //------------Setup for test--------------------------
            var futureReciept = new FutureReceipt { PartID = 1,RequestID = Guid.Empty, User = "Bob" };

            //------------Execute Test---------------------------
            futureReciept.ToKey();
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("FutureReceipt_ToKey")]
        [ExpectedException(typeof(Exception))]
        public void FutureReceipt_ToKey_WhenRequestIDNotSet_ExpectException()
        {
            //------------Setup for test--------------------------
            var futureReciept = new FutureReceipt { PartID = 1, User = "Bob" };

            //------------Execute Test---------------------------
            futureReciept.ToKey();
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("FutureReceipt_ToKey")]
        [ExpectedException(typeof(Exception))]
        public void FutureReceipt_ToKey_WhenUserEmpty_ExpectException()
        {
            //------------Setup for test--------------------------
            var futureReciept = new FutureReceipt { PartID = 1, RequestID = RequestID, User = string.Empty };

            //------------Execute Test---------------------------
            futureReciept.ToKey();
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("FutureReceipt_ToKey")]
        [ExpectedException(typeof(Exception))]
        public void FutureReceipt_ToKey_WhenUserNull_ExpectException()
        {
            //------------Setup for test--------------------------
            var futureReciept = new FutureReceipt { PartID = 1, RequestID = RequestID, User = null };

            //------------Execute Test---------------------------
            futureReciept.ToKey();
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("FutureReceipt_ToKey")]
        [ExpectedException(typeof(Exception))]
        public void FutureReceipt_ToKey_WhenUserNotSet_ExpectException()
        {
            //------------Setup for test--------------------------
            var futureReciept = new FutureReceipt { PartID = 1, RequestID = RequestID };

            //------------Execute Test---------------------------
            futureReciept.ToKey();
        }


        // ReSharper restore InconsistentNaming
    }
}
