using System;
using System.Collections.Generic;
using System.ComponentModel;
using Dev2.Runtime.ServiceModel.Data;
using Newtonsoft.Json;

// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

namespace Dev2.Common.Interfaces
{
    public interface IServiceTestModelTO
    {
        string OldTestName { get; set; }
        string TestName { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        DateTime LastRunDate { get; set; }
        List<IServiceTestInput> Inputs { get; set; }
        List<IServiceTestOutput> Outputs { get; set; }
        bool NoErrorExpected { get; set; }
        bool ErrorExpected { get; set; }
        [DefaultValue("")]
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
        string ErrorContainsText { get; set; }
        bool TestPassed { get; set; }
        bool TestFailing { get; set; }
        bool TestInvalid { get; set; }
        bool TestPending { get; set; }
        bool Enabled { get; set; }
        bool IsDirty { get; set; }
        AuthenticationType AuthenticationType { get; set; }
        Guid ResourceId { get; set; }        
        List<IServiceTestStep> TestSteps { get; set; }
        string FailureMessage { get; set; }
        TestRunResult Result { get; set; }
    }
}