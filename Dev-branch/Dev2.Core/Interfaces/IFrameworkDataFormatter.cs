using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2 {
    public interface IFrameworkDataFormatter {
        string FormatData(string formatDefinition, string dataToFormat, bool toDataFile = false);
    }
}
