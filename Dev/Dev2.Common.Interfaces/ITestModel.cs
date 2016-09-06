using System;
using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    public interface ITestModel
    {
        string TestName { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        DateTime LastRunDate { get; set; }
        List<ITestInput> Inputs { get; set; }
        List<ITestOutput> Outputs { get; set; }
        bool Error { get; set; }
        bool IsNewTest { get; set; }
        bool IsTestSelected { get; set; }
        bool TestPassed { get; set; }
        bool TestFailing { get; set; }
        bool TestInvalid { get; set; }
        bool TestPending { get; set; }
    }

    public interface ITestInput
    {
        string Variable { get; set; }
        string Value { get; set; }
        bool EmptyIsNull { get; set; }
    }

    public interface ITestOutput
    {
        string Variable { get; set; }
        string Value { get; set; }
    }
}