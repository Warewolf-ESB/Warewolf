using System;

namespace Dev2.Common {
    public interface IDev2SplitOp {

        bool IsFinalOp();

        string ExecuteOperation(char[] candidate, int startIdx, bool isReversed);

        string ExecuteOperation(CharEnumerator parts, int startIdx, int len, bool isReversed);
        
        int OpLength();

        bool CanUseEnumerator(bool isReversed);
    }
}
