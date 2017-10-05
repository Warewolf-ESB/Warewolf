/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.MathOperations;

namespace Dev2.Data.MathOperations
{
    public static class MathOpsFactory
    {
        public static IFunctionEvaluator CreateFunctionEvaluator() {
            return new FunctionEvaluator();
        }

        public static IFunction CreateFunction(string functionName, IList<string> arguments, IList<string> argumentDescriptions, string description) {
            return new Function(functionName, arguments, argumentDescriptions, description);
        }

        public static IFunction CreateFunction() {
            return new Function();
        }

        public static IFrameworkRepository<IFunction> FunctionRepository() {
            return new FunctionRepository();
        }
    
        public static IEvaluationFunction CreateEvaluationFunctionTO(string functionName) {
            return new EvaluationFunctionTO(functionName);
        }
    }
}
