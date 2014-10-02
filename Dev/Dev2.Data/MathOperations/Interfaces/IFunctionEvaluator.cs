
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
using System.Collections.Generic;
using Dev2.DataList.Contract;

namespace Dev2.MathOperations
{

    public interface IFunctionEvaluator
    {
        string EvaluateFunction(IEvaluationFunction expressionTO, Guid dlID, out ErrorResultTO errors);
        bool TryEvaluateFunction(IEvaluationFunction expressionTO, out string evaluation, out string error);
        bool TryEvaluateFunction(string expression, out string evaluation, out string error);
        bool TryEvaluateFunction<T>(List<T> value, string expression, out string evaluation, out string error) where T : IConvertible;
    }
}
