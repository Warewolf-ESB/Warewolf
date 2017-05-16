using System.Linq;
using System.Text;
using Dev2.Common.Interfaces;

namespace Dev2.Runtime.ESB.Execution
{
    public static class UpdateFailureMessages
    {
        private static GetTestStepsAndOutputs _testStatus;
        
        public static bool HasInvalidSteps => _testStatus.InvalidTestSteps?.Any() ?? false;
        public static bool HasPendingSteps => _testStatus.PendingTestSteps?.Any() ?? false;
        public static bool HasFailingSteps => _testStatus.FailingTestSteps?.Any() ?? false;
        public static bool HasFailingOutputs => _testStatus.FailingTestOutputs?.Any() ?? false;
        public static bool HasPendingOutputs => _testStatus.PendingTestOutputs?.Any() ?? false;
        public static bool HasInvalidOutputs => _testStatus.InvalidTestOutputs?.Any() ?? false;
        public static void Execute(GetTestStepsAndOutputs getTestStepsAndOutputs, IServiceTestModelTO test, out StringBuilder failureMessage)
        {
            _testStatus = getTestStepsAndOutputs;
            
            failureMessage = new StringBuilder();
            if (HasFailingSteps)
            {
                foreach (var serviceTestStep in _testStatus.FailingTestSteps)
                {
                    failureMessage.AppendLine("Failed Step: " + serviceTestStep.StepDescription + " ");
                    failureMessage.AppendLine("Message: " + serviceTestStep.Result?.Message);
                }
            }
            if (HasInvalidSteps)
            {
                foreach (var serviceTestStep in _testStatus.InvalidTestSteps)
                {
                    failureMessage.AppendLine("Invalid Step: " + serviceTestStep.StepDescription + " ");
                    failureMessage.AppendLine("Message: " + serviceTestStep.Result?.Message);
                }
            }
            if (HasPendingSteps)
            {
                foreach (var serviceTestStep in _testStatus.PendingTestSteps)
                {
                    failureMessage.AppendLine("Pending Step: " + serviceTestStep.StepDescription);
                }
            }

            if (HasFailingOutputs)
            {
                foreach (var serviceTestStep in _testStatus.FailingTestOutputs)
                {
                    failureMessage.AppendLine("Failed Output For Variable: " + serviceTestStep.Variable + " ");
                    failureMessage.AppendLine("Message: " + serviceTestStep.Result?.Message);
                }
            }
            if (HasInvalidOutputs)
            {
                foreach (var serviceTestStep in _testStatus.InvalidTestOutputs)
                {
                    failureMessage.AppendLine("Invalid Output for Variable: " + serviceTestStep.Variable);
                    failureMessage.AppendLine("Message: " + serviceTestStep.Result?.Message);
                }
            }
            if (HasPendingOutputs)
            {
                foreach (var serviceTestStep in _testStatus.PendingTestOutputs)
                {
                    failureMessage.AppendLine("Pending Output for Variable: " + serviceTestStep.Variable);
                }
            }

            var serviceTestSteps = test.TestSteps;
            if (serviceTestSteps != null)
            {
                failureMessage.AppendLine(string.Join("", serviceTestSteps.Where(step => !string.IsNullOrEmpty(step.Result?.Message)).Select(step => step.Result?.Message)));
            }
        }
    }
}