using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Common {
    public interface IDev2SplitOp {

        bool IsFinalOp();

        string ExecuteOperation(char[] canidate, int startIdx, bool isReversed);

        int OpLength();
    }
}
