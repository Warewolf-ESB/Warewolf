/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.ComponentModel;

namespace Warewolf.Data.Options.Enums
{
    public enum RetryAlgorithm
    {
        [Description("NoBackoff: On Error Retry Immediately")]
        NoBackoff,
        [Description("ConstantBackoff: Add a fixed delay after every attempt")]
        ConstantBackoff,
        [Description("LinearBackoff: Delay increases along with every attempt on Linear curve")]
        LinearBackoff,
        [Description("FibonacciBackoff: Delays based on the sum of the Fibonacci series")]
        FibonacciBackoff,
        [Description("QuadraticBackoff: Delay increases along with every attempt on Quadratic curve")]
        QuadraticBackoff,
    }
}
