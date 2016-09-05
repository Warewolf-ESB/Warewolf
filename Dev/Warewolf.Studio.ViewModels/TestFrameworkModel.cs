using System.Collections.Generic;
using Dev2.Common.Interfaces;

namespace Warewolf.Studio.ViewModels
{
    public class TestFrameworkModel : ITestFrameworkModel
    {
        #region Implementation of ITestFrameworkModel

        public string Testname { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public List<ITestInput> Inputs { get; set; }
        public List<ITestOutPut> OutPuts { get; set; }
        public bool Error { get; set; }

        #endregion
    }
}