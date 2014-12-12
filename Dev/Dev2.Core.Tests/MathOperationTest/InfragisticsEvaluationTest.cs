
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
using System.Diagnostics.CodeAnalysis;
using Infragistics.Calculations.CalcManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.MathOperationTest
{

    /// <summary>
    /// This Test class exists to test any changes that are made on the Infragistics source code. Ideally, if infragistics was part of the build then this would reside in that solution
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    [Ignore] //TODO: Fix so not dependant on resource file or localize resource file to test project
    public class InfragisticsEvaluationTest
    {

        private Dev2CalculationManager _manager;

        [TestInitialize]
        public void Init()
        {
            _manager = new Dev2CalculationManager();
            
        }

        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Infragistics source code changes")]
        [TestMethod]
        public void IsNumber_Correct_For_Numbers()
        {
            AssertValues(new Tuple<string, string>("true", "isnumber(\"1\")"), new Tuple<string, string>("true", "isnumber(\"-1\")"), new Tuple<string, string>("true", "isnumber(\"-1.1\")"), new Tuple<string, string>("true", "isnumber(\"1.1\")"));
        }

        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Infragistics source code changes")]
        [TestMethod]
        public void IsNumber_Correct_For_Empty()
        {
            AssertValues(new Tuple<string, string>("false", "isnumber(\"\")"), new Tuple<string, string>("false", "isnumber(\"g\")"), new Tuple<string, string>("false", "isnumber(\",\")"));
        }

        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Infragistics source code changes")]
        [TestMethod]
        public void Value_Correct_For_Numbers()
        {
            AssertValues(new Tuple<string, string>("1", "value(\"1\")"), new Tuple<string, string>("-1", "value(\"-1\")"), new Tuple<string, string>("-1.1", "value(\"-1.1\")"), new Tuple<string, string>("1.1", "value(\"1.1\")"));
        }

        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Infragistics source code changes")]
        [TestMethod]
        public void Value_Correct_For_NonNumbers()
        {
            AssertValues(new Tuple<string, string>("#num!", "value(\"a\")"), new Tuple<string, string>("#num!", "value(\"\")"), new Tuple<string, string>("#num!", "value(\" \")"), new Tuple<string, string>("#num!", "value(\",\")"));
        }

        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Infragistics source code changes")]
        [TestMethod]
        public void IsEven_Correct_For_NonNumbers()
        {
            AssertValues(new Tuple<string, string>("#num!", "iseven(\"a\")"), new Tuple<string, string>("#num!", "iseven(\"\")"), new Tuple<string, string>("#num!", "iseven(\" \")"), new Tuple<string, string>("#num!", "iseven(\",\")"));
        }

        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Infragistics source code changes")]
        [TestMethod]
        public void IsEven_Correct_For_Numbers()
        {
            AssertValues(new Tuple<string, string>("true", "iseven(\"2\")"),new Tuple<string, string>("true", "iseven(\"-4\")"),new Tuple<string, string>("false", "iseven(\"1\")"), new Tuple<string, string>("false", "iseven(\"-1\")"));
        }


        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Infragistics source code changes")]
        [TestMethod]
        public void IsOdd_Correct_For_NonNumbers()
        {
            AssertValues(new Tuple<string, string>("#num!", "isodd(\"a\")"), new Tuple<string, string>("#num!", "isodd(\"\")"), new Tuple<string, string>("#num!", "isodd(\" \")"), new Tuple<string, string>("#num!", "isodd(\",\")"));
        }

        [Owner("Leon Rajindrapersadh")]
        [TestCategory("Infragistics source code changes")]
        [TestMethod]
        public void IsOdd_Correct_For_Numbers()
        {
            AssertValues(new Tuple<string, string>("false", "isodd(\"2\")"), new Tuple<string, string>("false", "isodd(\"-4\")"), new Tuple<string, string>("true", "isodd(\"1\")"), new Tuple<string, string>("true", "isodd(\"-1\")"));
        }

        private void AssertValues(params Tuple<string, string> [] functions)
        {
            foreach (var function in  functions)
            {
                Assert.AreEqual( function.Item1, _manager.CalculateFormula(function.Item2).ToString().ToLower());
            }
        }
    }
}
