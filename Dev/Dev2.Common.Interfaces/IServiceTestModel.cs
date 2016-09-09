using System;
using System.Collections.Generic;
using System.ComponentModel;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Common.Interfaces
{
    public interface IServiceTestModel: INotifyPropertyChanged
    {
        Guid ParentId { get; set; }
        string OldTestName { get; set; }
        string TestName { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        DateTime LastRunDate { get; set; }
        List<IServiceTestInput> Inputs { get; set; }
        List<IServiceTestOutput> Outputs { get; set; }
        bool NoErrorExpected { get; set; }
        bool ErrorExpected { get; set; }
        bool IsNewTest { get; set; }
        bool IsTestSelected { get; set; }
        bool TestPassed { get; set; }
        bool TestFailing { get; set; }
        bool TestInvalid { get; set; }
        bool TestPending { get; set; }
        bool Enabled { get; set; }
        string RunSelectedTestUrl { get; set; }
        AuthenticationType AuthenticationType { get; set; }
        string NameForDisplay { get; }
        bool IsDirty { get; set; }
        bool UserAuthenticationSelected { get; }
        
    }

    public interface IServiceTestInput
    {
        string Variable { get; set; }
        string Value { get; set; }
        bool EmptyIsNull { get; set; }
    }

    public interface IServiceTestOutput
    {
        string Variable { get; set; }
        string Value { get; set; }
    }
}