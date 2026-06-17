using System;
using Dev2.Activities.DropBox2016;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.ActivityTests.DropBox2016
{
    public static class Files
    {
        public static readonly string PathEmpty = String.Empty;
        public static readonly string PathNull = null;
        public static readonly string PathEmptyExcpl = "";
        public static readonly string PathInvalid = "::G";
        public static readonly string PathValid = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        public static readonly string PathInValidWithColon = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\:.txt";
        public static readonly string PathToolLong = @"c:\fffffffffffffffffffffffffffffffffff\gfgfgfgfgfgfgfgfgfgfgfgfgfgfgfgfgfgfgfgf\ggggggggggggggggg\fffffffffffffff\ggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggfffffffffffffffffffffffffffffffffff\gfgfgfgfgfgfgfgfgfgfgfgfgfgfgfgfgfgfgfgf\ggggggggggggggggg\fffffffffffffff\gggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggggg";
    }

    [TestClass]
    public class DropboxSoureFileValidatorShould
    {
        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Construct_GivenFilePath_ShouldNotBeNull()
        {
            //---------------Set up test pack-------------------
            var dropboxSoureFileValidator = new DropboxSoureFileValidator(Files.PathEmpty);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsNotNull(dropboxSoureFileValidator);
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(ArgumentException))]
        public void Validate_GivenEmpty_ShouldThrowArgumentNull()
        {
            //---------------Set up test pack-------------------
            var dropboxSoureFileValidator = new DropboxSoureFileValidator(Files.PathEmpty);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            dropboxSoureFileValidator.Validate();
            //---------------Test Result -----------------------
            Assert.Fail("Exception not thrown");
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(ArgumentException))]
        public void Validate_GivenNull_ShouldThrowArgumentNull()
        {
            //---------------Set up test pack-------------------
            var dropboxSoureFileValidator = new DropboxSoureFileValidator(Files.PathNull);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            dropboxSoureFileValidator.Validate();
            //---------------Test Result -----------------------
            Assert.Fail("Exception not thrown");
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(ArgumentException))]
        public void Validate_GivenEmptyExcplicit_ShouldThrowArgumentNull()
        {
            //---------------Set up test pack-------------------
            var dropboxSoureFileValidator = new DropboxSoureFileValidator(Files.PathEmptyExcpl);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            dropboxSoureFileValidator.Validate();
            //---------------Test Result -----------------------
            Assert.Fail("Exception not thrown");
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(NotSupportedException))]
        public void Validate_GivenInvalid_ShouldThrowNotSupportedException()
        {
            //---------------Set up test pack-------------------
            var dropboxSoureFileValidator = new DropboxSoureFileValidator(Files.PathInvalid);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            dropboxSoureFileValidator.Validate();
            //---------------Test Result -----------------------
            Assert.Fail("Exception not thrown");
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        public void Validate_GivenTooLong_ShouldThrowPathTooLongException()
        {
            //---------------Set up test pack-------------------
            var dropboxSoureFileValidator = new DropboxSoureFileValidator(Files.PathToolLong);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            dropboxSoureFileValidator.Validate();
            //---------------Test Result -----------------------
            // EXPECTED NO EXCEPTION THROWN ON .NET FRAMEWORK 4.7
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(NotSupportedException))]
        public void Validate_GivenPathInValidWithColon_ShouldThrowPathTooLongException()
        {
            //---------------Set up test pack-------------------
            var dropboxSoureFileValidator = new DropboxSoureFileValidator(Files.PathInValidWithColon);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            dropboxSoureFileValidator.Validate();
            //---------------Test Result -----------------------
            Assert.Fail("Exception not thrown");
        }
    }
}
