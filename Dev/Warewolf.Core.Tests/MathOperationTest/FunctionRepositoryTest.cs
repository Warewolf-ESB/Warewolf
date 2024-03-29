/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Data.MathOperations;
using Dev2.MathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Tests.MathOperationTest
{
    /// <summary>
    /// Summary description for FunctionRepositoryTest
    /// </summary>
    [TestClass]
    public class FunctionRepositoryTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Load Tests

        /// <summary>
        /// FunctionRepository load default repository.
        /// </summary>
        [TestMethod]
        public void FunctionRepository_Load_DefaultRepository()
        {
            var functionRepo = MathOpsFactory.FunctionRepository();

            functionRepo.Load();
            functionRepo.All();

            Assert.IsTrue(functionRepo.All().Count > 0);
        }

        #endregion Load Tests

        #region Find Tests

        /// <summary>
        /// Tests that the FunctionRepository is able to find a function that exists in the repository
        /// </summary>
        [TestMethod]
        public void FunctionRepository_Find_ValidExpressionAndReturnsData_Expected_ListReturned()
        {
            var functionRepo = MathOpsFactory.FunctionRepository();
            // The function repository must be loaded in order to populate the function list
            functionRepo.Load();

            var functions = functionRepo.Find(c => c.FunctionName.Length > 0);


            Assert.IsTrue(functions.Count > 0);
        }

        /// <summary>
        /// Tests that the FunctionRepository returns an empty list when it is unable to find a function
        /// </summary>
        [TestMethod]
        public void FunctionRepository_Find_ExpressionYieldsNoResult_Expected_EmptyCollectionReturned()
        {
            var functionRepo = MathOpsFactory.FunctionRepository();
            // The function repository must be loaded in order to populate the function list
            functionRepo.Load();

            var functions = functionRepo.Find(c => c.FunctionName == string.Empty);

            Assert.AreEqual(0, functions.Count);
        }

        /// <summary>
        ///  Tests that the FunctionRepository throws the correct exception when trying to find a null function to it
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FunctionRepository_Find_NullExpression_Expected_ErrorFromRepository()
        {
            var functionRepo = MathOpsFactory.FunctionRepository();
            // The function repository must be loaded in order to populate the function list
            functionRepo.Load();
            functionRepo.Find(null);
        }

        #endregion Find Tests

        #region FindSingle Tests

        /// <summary>
        /// FunctionRepository FindSingle valid expression expected single result returned.
        /// </summary>
        [TestMethod]
        public void FunctionRepository_FindSingle_ValidExpression_Expected_SingleResultReturned()
        {
            var functionRepo = MathOpsFactory.FunctionRepository();
            // The function repository must be loaded in order to populate the function list
            functionRepo.Load();

            var function = functionRepo.FindSingle(c => c.FunctionName.Contains("s"));

            Assert.IsNotNull(function);

        }

        /// <summary>
        /// FunctionRepository FindSingle expression yields no result expected empty function returned.
        /// </summary>
        [TestMethod]
        public void FunctionRepository_FindSingle_ExpressionYieldsNoResult_Expected_EmptyFunctionReturned()
        {
            var functionRepo = MathOpsFactory.FunctionRepository();
            // The function repository must be loaded in order to populate the function list
            functionRepo.Load();
            var function = functionRepo.FindSingle(c => c.FunctionName == string.Empty);
            Assert.IsInstanceOfType(function, typeof(IFunction));
        }

        /// <summary>
        /// FunctionRepository FindSingle NULL expression expected error from repository.
        /// </summary>
        [TestMethod]
        public void FunctionRepository_FindSingle_NullExpression_Expected_NullReturned()
        {
            var functionRepo = MathOpsFactory.FunctionRepository();
            // The function repository must be loaded in order to populate the function list
            functionRepo.Load();
            var functions = functionRepo.FindSingle(null);
            Assert.IsNull(functions);
        }

        #endregion FindSingle Tests

        #region Save Tests

        /// <summary>
        /// FunctionRepository Save Valid function expected repository updated with new function.
        /// </summary>
        [TestMethod]
        public void FunctionRepository_Save_ValidFunction_Expected_RepoUpdatedWithNewFunction()
        {
            var functionRepo = MathOpsFactory.FunctionRepository();
            const string functionName = "TestFunction";
            var arguments = new List<string> { "args" };
            var argumentDescriptions = new List<string> { "the first argument" };
            const string description = "Test Description";
            // The function repository must be loaded in order to populate the function list
            functionRepo.Load();


            var myFunction = MathOpsFactory.CreateFunction(functionName, arguments, argumentDescriptions, description);

            functionRepo.Save(myFunction);

            Assert.IsNotNull(functionRepo.Find(c => c.FunctionName.Equals(functionName)));

        }

        /// <summary>
        /// FunctionRepository save NULL function expected argument null exception.
        /// </summary>
        [TestMethod]
        public void FunctionRepository_Save_NullFunction_Expected_ArgumentNullException()
        {
            var functionRepo = MathOpsFactory.FunctionRepository();
            // The function repository must be loaded in order to populate the function list
            functionRepo.Load();

            try
            {
                functionRepo.Save((IFunction)null);
            }
            catch(ArgumentNullException)
            {
                // If there was a null argument exception, it's behaving
                Assert.IsTrue(true);
            }
        }

        #endregion Save Tests

        #region Save Collection Tests

        /// <summary>
        /// FunctionRepositorySaveCollection valid function expected Repository updated with new function.
        /// </summary>
        [TestMethod]
        public void FunctionRepository_SaveCollection_ValidFunction_Expected_RepoUpdatedWithNewFunction()
        {
            var functionRepo = MathOpsFactory.FunctionRepository();
            const string functionName = "TestFunction";
            var arguments = new List<string> { "args" };
            var argumentDescriptions = new List<string> { "the first argument" };
            const string description = "Test Description";

            const string function2Name = "TestFunction2";

            // The function repository must be loaded in order to populate the function list
            functionRepo.Load();


            var myfirstFunction = MathOpsFactory.CreateFunction(functionName, arguments, argumentDescriptions, description);
            var mySecondFunction = MathOpsFactory.CreateFunction(function2Name, arguments, argumentDescriptions, description);
            ICollection<IFunction> functionList = new List<IFunction> { myfirstFunction, mySecondFunction };

            functionRepo.Save(functionList);

            Assert.AreEqual(2, functionRepo.Find(c => c.FunctionName.Contains(functionName)).Count);

        }

        /// <summary>
        /// FunctionRepositorySaveCollection empty collection expected repository function count remains the same.
        /// </summary>
        [TestMethod]
        public void FunctionRepository_SaveCollection_EmptyCollection_Expected_RepoFunctionCountRemainsTheSame()
        {
            var functionRepo = MathOpsFactory.FunctionRepository();

            // The function repository must be loaded in order to populate the function list
            functionRepo.Load();
            var beforeEmptySave = functionRepo.Find(c => c.FunctionName != string.Empty).Count;

            ICollection<IFunction> functionList = new List<IFunction>();

            functionRepo.Save(functionList);

            var afterEmptySave = functionRepo.Find(c => c.FunctionName != string.Empty).Count;
            Assert.AreEqual(beforeEmptySave, afterEmptySave);
        }

        /// <summary>
        /// FunctionRepository Save Collection NULL Collection expected argument null exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FunctionRepository_SaveCollection_NullCollection_Expected_ArgumentNullException()
        {
            var functionRepo = MathOpsFactory.FunctionRepository();

            // The function repository must be loaded in order to populate the function list
            functionRepo.Load();
            var beforeEmptySave = functionRepo.Find(c => c.FunctionName != string.Empty).Count;
            functionRepo.Save((ICollection<IFunction>)null);
            Assert.IsNotNull(beforeEmptySave);
        }

        #endregion Save Collection Tests

        #region Remove Tests

        /// <summary>
        /// FunctionRepository Remove valid function expected function removed from repository.
        /// </summary>
        [TestMethod]
        public void FunctionRepository_Remove_ValidFunction_Expected_FunctionRemovedFromRepo()
        {
            var functionRepo = MathOpsFactory.FunctionRepository();
            const string functionName = "TestFunction";
            var arguments = new List<string> { "args" };
            var argumentDescriptions = new List<string> { "the first argument" };
            const string description = "Test Description";
            // The function repository must be loaded in order to populate the function list
            functionRepo.Load();


            var myFunction = MathOpsFactory.CreateFunction(functionName, arguments, argumentDescriptions, description);
            // save the new function
            functionRepo.Save(myFunction);

            functionRepo.Remove(myFunction);

            Assert.AreEqual(0, functionRepo.Find(c => c.FunctionName.Equals(functionName)).Count);

        }

        /// <summary>
        /// FunctionRepository Remove NULL function expected argument null exception.
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FunctionRepository_Remove_NullFunction_Expected_ArgumentNullException()
        {
            var functionRepo = MathOpsFactory.FunctionRepository();
            // The function repository must be loaded in order to populate the function list
            functionRepo.Load();

            functionRepo.Remove((IFunction)null);
        }

        #endregion Remove Tests

        #region Remove Collection Tests

        [TestMethod]
        public void FunctionRepository_RemoveCollection_ValidFunction_Expected_RepoUpdatedWithNewFunction()
        {
            var functionRepo = MathOpsFactory.FunctionRepository();


            // The function repository must be loaded in order to populate the function list
            functionRepo.Load();
            var functionsToRemove = functionRepo.Find(c => c.FunctionName.Contains("s"));
            var functionCountBeforeRemove = functionRepo.Find(c => c.FunctionName != string.Empty).Count;

            functionRepo.Remove(functionsToRemove);

            var functionCountAfterRemove = functionRepo.Find(c => c.FunctionName != string.Empty).Count;

            Assert.IsTrue(functionCountAfterRemove < functionCountBeforeRemove);

        }

        [TestMethod]
        public void FunctionRepository_RemopveCollection_EmptyCollection_Expected_NoFunctionsRemovedFromRepo()
        {
            var functionRepo = MathOpsFactory.FunctionRepository();

            // The function repository must be loaded in order to populate the function list
            functionRepo.Load();
            var beforeEmptySave = functionRepo.Find(c => c.FunctionName != string.Empty).Count;

            ICollection<IFunction> functionList = new List<IFunction>();

            functionRepo.Remove(functionList);

            var afterEmptySave = functionRepo.Find(c => c.FunctionName != string.Empty).Count;
            Assert.AreEqual(beforeEmptySave, afterEmptySave);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void FunctionRepository_RemoveCollection_NullCollection_Expected_ArgumentException()
        {
            var functionRepo = MathOpsFactory.FunctionRepository();

            // The function repository must be loaded in order to populate the function list
            functionRepo.Load();
            var beforeEmptySave = functionRepo.Find(c => c.FunctionName != string.Empty).Count;

            functionRepo.Remove((ICollection<IFunction>)null);
            Assert.IsNotNull(beforeEmptySave);
        }

        #endregion Remove Collection Tests

    }
}
