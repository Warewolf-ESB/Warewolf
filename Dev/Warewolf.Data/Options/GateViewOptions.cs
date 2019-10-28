/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Warewolf.Data.Options.Enums;
using Warewolf.Options;

namespace Warewolf.Data.Options
{
    public class GateViewOptions 
    {
        [DataValue(nameof(GateFailureActionBase.GateFailureAction))]
        [MultiDataProvider(typeof(Retry))]
        public GateFailureActionBase GateFailureAction { get; set; }

    }

    public class GateFailureActionBase
    {
        public GateFailureAction GateFailureAction { get; set; }
    }

    public class Retry : GateFailureActionBase
    {
        public YesNo Resume { get; set; } = YesNo.No;
        public string ResumeEndpoint { get; set; } = "Not Yet Set";
        public int Count { get; set; } = 2;

        [DataValue(nameof(RetryAlgorithmBase.RetryAlgorithm))]
        [MultiDataProvider(typeof(NoBackoff), typeof(ConstantBackoff), typeof(LinearBackoff), typeof(FibonacciBackoff), typeof(QuadraticBackoff))]
        public RetryAlgorithmBase Strategy { get; set; } = new NoBackoff();
    }

    public enum YesNo
    {
        Yes,
        No
    }


    public class RetryAlgorithmBase
    {
        public RetryAlgorithm RetryAlgorithm { get; set; }
    }

        
    public class NoBackoff : RetryAlgorithmBase
    {
        public NoBackoff()
        {
            RetryAlgorithm = RetryAlgorithm.NoBackoff;
        }

        public int TimeOut { get; set; } = 60000;
    }

    public class ConstantBackoff : RetryAlgorithmBase
    {
        public ConstantBackoff()
        {
            RetryAlgorithm = RetryAlgorithm.ConstantBackoff;
        }

        public int TimeOut { get; set; } = 60000;
    }

    public class LinearBackoff : RetryAlgorithmBase
    {
        public LinearBackoff()
        {
            RetryAlgorithm = RetryAlgorithm.LinearBackoff;
        }

        public int TimeOut { get; set; } = 60000;
    }

    public class FibonacciBackoff : RetryAlgorithmBase
    {
        public FibonacciBackoff()
        {
            RetryAlgorithm = RetryAlgorithm.FibonacciBackoff;
        }

        public int TimeOut { get; set; } = 60000;
    }

    public class QuadraticBackoff : RetryAlgorithmBase
    {
        public QuadraticBackoff()
        {
            RetryAlgorithm = RetryAlgorithm.QuadraticBackoff;
        }

        public int TimeOut { get; set; } = 60000;
    }
}
