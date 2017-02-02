using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Storage.Tests
{
    [ExcludeFromCodeCoverage]
    [TestClass]
    public class FsInteropFunctionsTests
    {
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Given_ParseLanguageExpression_Should()
        {
            const string personChildName = "[[@Person.Child.Name]]";
            var languageExpression = FsInteropFunctions.ParseLanguageExpression(personChildName, 0);
            Assert.IsNotNull(languageExpression);
        }

        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Given_ParseLanguageExpressionWithoutUpdate_Should()
        {
            const string personChildName = "[[@Person.Child.Name]]";
            var languageExpression = FsInteropFunctions.ParseLanguageExpressionWithoutUpdate(personChildName);
            var isJsonIdentifierExpression = languageExpression.IsJsonIdentifierExpression;
            Assert.IsNotNull(languageExpression);
            Assert.IsTrue(isJsonIdentifierExpression);
        }
        [TestMethod]
        [Owner("Sanele Mthembu")]
        public void Given_LanguageExpressionToString_Should()
        {
            const string personChildName = "[[@Person.Child.Name]]";
            Assert.IsNotNull(FsInteropFunctions.PositionColumn);
            var languageExpression = FsInteropFunctions.ParseLanguageExpressionWithoutUpdate(personChildName);
            var languageExpressionToString = FsInteropFunctions.LanguageExpressionToString(languageExpression);
            Assert.AreEqual(typeof(string), languageExpressionToString.GetType());
        }
    }
}