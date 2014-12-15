
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Studio.Core.Models;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Interfaces
{
    public interface IDecisionType
    {
        string DecisionTypeName { get; set; }
        List<OperatorType> OperatorTypes { get; set; }
        string StringDecorator { get; }
        string FunctionName { get; }
        bool IsValid { get; }
        string GetExpression();
        string BuildStringExpression(string functionName, string decorator, string expression, string op, object value, object endvalue);
    }
}
