using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Common {
    internal class Dev2IndexOp : IDev2SplitOp{

        public int ToIndex { get; private set; }

        internal Dev2IndexOp(int index) {
            ToIndex = index;
        }

        public bool IsFinalOp() {
            return false;
        }

        public bool CanUseEnumerator(bool isReversed)
        {
            return false;
            //return (isReversed != true);
        }

        public string ExecuteOperation(char[] canidate, int startIdx, bool isReversed) {
            StringBuilder result = new StringBuilder();

            int start = startIdx;
            int end = (startIdx + ToIndex);

            // Avoid boundry over-run
            if (end > canidate.Length)
            {
                end = canidate.Length;
            }

            if (isReversed) {
                start = (startIdx - ToIndex)+1;
                end = startIdx+1;
            }

            for (int i = start; i < end; i++) {
                result.Append(canidate[i]);
            }

            return result.ToString();
        }

        public string ExecuteOperation(CharEnumerator candidate, int startIdx, int len, bool isReversed)
        {
            throw new NotImplementedException();
        }


        public int OpLength() {
            return 0;
        }
    }
}
