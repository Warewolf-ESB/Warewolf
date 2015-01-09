using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Core.Test
{
    [TestClass]
    public class TestExceptionHandler
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfExceptionHandler_Ctor")]
        public void WarewolfExceptionHandler_Ctor_Valid_ExpectWellFormed()
        {
            //------------Setup for test--------------------------
            var warewolfExceptionHandler = new WarewolfExceptionHandler(new Dictionary<Type, Action>());
            
            //------------Execute Test---------------------------
            PrivateObject p = new PrivateObject(warewolfExceptionHandler);
            //------------Assert Results-------------------------
            Assert.IsNotNull(p.GetField("_errorHandlers"));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfExceptionHandler_Ctor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WarewolfExceptionHandler_Ctor_NullActions_ExpectError()
        {
            //------------Setup for test--------------------------
            var warewolfExceptionHandler = new WarewolfExceptionHandler(null);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfExceptionHandler_Handle")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void WarewolfExceptionHandler_Handles_NotFound_Rethrow()
        {
            //------------Setup for test--------------------------
            var warewolfExceptionHandler = new WarewolfExceptionHandler(new Dictionary<Type, Action>());

            //------------Assert Results-------------------------
            warewolfExceptionHandler.Handle(new ArgumentNullException());
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("WarewolfExceptionHandler_Handle")]
        public void WarewolfExceptionHandler_Handles_Found_Swallowed()
        {
            bool handled = false;
            //------------Setup for test--------------------------
            var warewolfExceptionHandler = new WarewolfExceptionHandler(new Dictionary<Type, Action>() { { typeof(ArgumentNullException), () => { handled = true; } } });

            //------------Assert Results-------------------------
            warewolfExceptionHandler.Handle(new ArgumentNullException());
            Assert.IsTrue(handled);
        }
    }
}
