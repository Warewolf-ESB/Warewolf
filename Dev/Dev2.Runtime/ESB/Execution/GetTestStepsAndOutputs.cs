using System.Collections.Generic;
using System.Linq;
using Dev2.Common.Interfaces;

namespace Dev2.Runtime.ESB.Execution
{
    public class GetTestStepsAndOutputs
    {
        private readonly List<IServiceTestStep> _serviceTestSteps;
        private readonly IServiceTestModelTO _test;

        public GetTestStepsAndOutputs(IServiceTestModelTO test)
        {
            _test = test;
            _serviceTestSteps = test.TestSteps;
        }

        public IEnumerable<IServiceTestStep> PendingSteps
        { 
            get
            {
                var pendingSteps = _serviceTestSteps?.Where(step => step.Type != StepType.Mock && step.Result?.RunTestResult == RunResult.TestPending);
                return pendingSteps;
            }
        }
        public IEnumerable<IServiceTestStep> InvalidSteps
        {
            get
            {
                var invalidSteps = _serviceTestSteps?.Where(step => step.Type != StepType.Mock && step.Result?.RunTestResult == RunResult.TestInvalid);
                return invalidSteps;
            }
        }
        public IEnumerable<IServiceTestStep> FailingSteps
        {
            get
            {
                var failingSteps = _serviceTestSteps?.Where(step => step.Type != StepType.Mock && step.Result?.RunTestResult == RunResult.TestFailed);
                return failingSteps;
            }
        }
        public IEnumerable<IServiceTestOutput> PendingOutputs
        {
            get
            {
                var pendingOutputs = _test.Outputs?.Where(output => output.Result?.RunTestResult == RunResult.TestPending);
                return pendingOutputs;
            }
        }
        public IEnumerable<IServiceTestOutput> InvalidOutputs
        {
            get
            {
                var invalidOutputs = _test.Outputs?.Where(output => output.Result?.RunTestResult == RunResult.TestInvalid);
                return invalidOutputs;
            }
        }
        public IEnumerable<IServiceTestOutput> FailingOutputs
        {
            get
            {
                var failingOutputs = _test.Outputs?.Where(output => output.Result?.RunTestResult == RunResult.TestFailed);
                return failingOutputs;
            }
        }
        public IList<IServiceTestStep> PendingTestSteps
        {
            get
            {
                var pendingTestSteps = PendingSteps as IList<IServiceTestStep> ?? PendingSteps?.ToList();
                return pendingTestSteps;
            }
        }
        public IList<IServiceTestStep> FailingTestSteps
        {
            get
            {
                var failingTestSteps = FailingSteps as IList<IServiceTestStep> ?? FailingSteps?.ToList();
                return failingTestSteps;
            }
        }
        public IList<IServiceTestStep> InvalidTestSteps
        {
            get
            {
                var invalidTestSteps = InvalidSteps as IList<IServiceTestStep> ?? InvalidSteps?.ToList();
                return invalidTestSteps;
            }
        }
        public IList<IServiceTestOutput> InvalidTestOutputs
        {
            get
            {
                var invalidTestOutputs = InvalidOutputs as IList<IServiceTestOutput> ?? InvalidOutputs?.ToList();
                return invalidTestOutputs;
            }
        }
        public IList<IServiceTestOutput> FailingTestOutputs
        {
            get
            {
                var failingTestOutputs = FailingOutputs as IList<IServiceTestOutput> ?? FailingOutputs?.ToList();
                return failingTestOutputs;
            }
        }
        public IList<IServiceTestOutput> PendingTestOutputs
        {
            get
            {
                var pendingTestOutputs = PendingOutputs as IList<IServiceTestOutput> ?? PendingOutputs?.ToList();
                return pendingTestOutputs;
            }
        }
    }
}