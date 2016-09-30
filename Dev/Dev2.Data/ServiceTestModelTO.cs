using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Data
{
    public class ServiceTestModelTO : IServiceTestModelTO
    {
        private bool _testPassed;
        public string OldTestName { get; set; }
        public string TestName { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public DateTime LastRunDate { get; set; }
        public List<IServiceTestInput> Inputs { get; set; }
        public List<IServiceTestOutput> Outputs { get; set; }
        public bool NoErrorExpected { get; set; }
        public bool ErrorExpected { get; set; }
        public string ErrorContainsText { get; set; }
        public TestRunResult Result { get; set; }
        public bool TestPassed
        {
            get
            {
                return _testPassed;
            }
            set
            {
                _testPassed = value;
                if (value)
                {
                    FailureMessage = "";
                }
            }
        }
        public bool TestFailing { get; set; }
        public bool TestInvalid { get; set; }
        public bool TestPending { get; set; }
        public bool Enabled { get; set; }
        public bool IsDirty { get; set; }
        public AuthenticationType AuthenticationType { get; set; }
        public Guid ResourceId { get; set; }
        public List<IServiceTestStep> TestSteps { get; set; }
        public string FailureMessage { get; set; }
    }
}