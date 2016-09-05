using System.Collections.Generic;

namespace Dev2.Common.Interfaces
{
    public interface ITestFrameworkModel
    {
        string Testname { get; set; }
        string UserName { get; set; }
        string Password { get; set; }
        List<ITestInput> Inputs { get; set; }
        List<ITestOutPut> OutPuts { get; set; }
        bool Error { get; set; }

    }

    public interface ITestInput
    {
        string Variable { get; set; }
        string Value { get; set; }
        bool EmptyIsNull { get; set; }
    }

    public interface ITestOutPut
    {
        string Variable { get; set; }
        string Value { get; set; }
    }
}