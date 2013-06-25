using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActivityUnitTests {
    public interface IBaseActivityUnitTest {

        string TestData { get; set; }

        dynamic ExecuteProcess();
    }
}
