using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Dev2;

namespace Unlimited.UnitTest.Framework
{
    
    
    /// <summary>
    ///This is a test class for TestQueryStringModuleTest and is intended
    ///to contain all TestQueryStringModuleTest Unit Tests
    ///</summary>
    [TestClass()]
    public class EncryptionTest {


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

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        ///// <summary>
        /////A test for Decrypt
        /////</summary>
        //[TestMethod()]
        //public void DecryptTest() {
        //    //string inputText = "evSwokIAcuNlp2O21w8b8fDqDxEgQEXe8DsV2NiB9OkBqIJm7a1e6svTKfTtNaCk%3f";
        //    //string inputText = "evSwokIAcuNlp2O21w8b8fGaAGXJ1Lniwi0AjwPa8vvQb/gpDQXWvDoY/AUAzJYy2O8t7KZqG8aJDqpBw84+nMLsCm1+S6F8v7sX469rUO0=";
        //    //string inputText = "/yLPPs6Xj7Dq2c/4n2ZVZu7tL8C3UR3RRLsOtjkOvh8aZINZmKHIVzr3Dfa1lh5fCX1BepzLlLKoWm7UjtEFwk6c8C5NESWK2KtrMj5l0z89q9h10LKA9tYM7VMzVMz7a9xk4gkRRcgxwmWzbB2l/+9SO1/x2N29T0XxevdfWJt8sl37jRAn6amg4aJrrHYLYUyW28d5JjpV2HhYLE+DBt/gnzaeykzfaCnmpzIQ79eP2j+Gqe8Waqcgk9137SXgucKngYEN7KuLK3BaY2vK1etZkdGWRm5ftTwYq/bzJ937wYfqe7c929Fa0g4kKjikQ17iCAH6Q0/idnF3S0ShuzN0pqMUrdcQpNzp7QHL0Hc=";
        //    //string inputText = "B1zF9QvnRwcChx6bgi1VC7zHHINdS52PzUTWkcRs6mye89AgLFho1DQVegfiAEuMnq6EidkNBj4ApjBHkfxFHnC3VyOXXrzapurhW7C0/AQ=";
        //    //string inputText = "qustr=xyQhLSbcrw9X5KANkSwAhnKb4r5Qk3hIvfDt2Fq5fJDdUmywleNP6RWsovUb4iytVmn3tsH9F1lRey+BWn3eoS2OLQ6B6bcWKTfHOhttp%3a%2f%2fgenisys%2fGeniSys%2fRepository%2f..%2fAttachments%2f1251813240.1921573.wavWsHyFDqF4Wn%2feJUari722AdT27%2f";
        //    string inputText = "xyQhLSbcrw9X5KANkSwAhnKb4r5Qk3hIvfDt2Fq5fJDdUmywleNP6RWsovUb4iytVmn3tsH9F1lRey+BWn3eoS2OLQ6B6bcWKTfHO";
        //    string actual;
        //    actual = Encryption.Decrypt(inputText);
        //}

        ///// <summary>
        /////A test for Encrypt
        /////</summary>
        //[TestMethod()]
        //public void EncryptTest() {
        //    string inputText = "ReturnUrl=Default.aspx";
        //    string actual;
        //    actual = Encryption.Encrypt(inputText);
        //}
    }
}
