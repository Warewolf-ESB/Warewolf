using System;

namespace Dev2.Common {
    public interface IDev2SplitOp {

        bool IsFinalOp();

        string ExecuteOperation(char[] canidate, int startIdx, bool isReversed);

        string ExecuteOperation(CharEnumerator parts, int startIdx, bool isReversed);
        
        int OpLength();

        bool CanUseEnumerator(bool isReversed);
    }
}
