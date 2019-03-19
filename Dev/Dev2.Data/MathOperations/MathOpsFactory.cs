#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
        public static IFunctionEvaluator CreateFunctionEvaluator() => new FunctionEvaluator();

        public static IFunction CreateFunction(string functionName, IList<string> arguments, IList<string> argumentDescriptions, string description) => new Function(functionName, arguments, argumentDescriptions, description);

        public static IFunction CreateFunction() => new Function();

        public static IFrameworkRepository<IFunction> FunctionRepository() => new FunctionRepository();

        public static IEvaluationFunction CreateEvaluationFunctionTO(string functionName) {
            return new EvaluationFunctionTO(functionName);
        }
    }
}
