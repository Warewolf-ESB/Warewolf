using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Common {
    internal class Dev2EoFOp : IDev2SplitOp{

        public bool IsFinalOp() {
            //18.09.2012: Massimo.Guerrera - Changed to false because of bug, no executing the last op.
            return false;
        }

        public int OpLength() {
            return 0;
        }

        public bool CanUseEnumerator(bool isReversed)
        {
            return (isReversed != true);
        }

        public string ExecuteOperation(char[] canidate, int startIdx, bool isReversed) {
            StringBuilder result = new StringBuilder();

            int start = startIdx;
            int end = canidate.Length;

            if (isReversed) {
                start = 0;
                end = startIdx+1;
            }

            for (int i = start; i < end; i++) {
                result.Append(canidate[i]);
            }

            return result.ToString();
        }

        public string ExecuteOperation(CharEnumerator candidate, int startIdx, bool isReversed)
        {
            StringBuilder result = new StringBuilder();


            while(candidate.MoveNext())
            {
                result.Append(candidate.Current);
            }

            //int start = startIdx;
            //int end = canidate.Length;

            //if (isReversed)
            //{
            //    start = 0;
            //    end = startIdx + 1;
            //}

            //for (int i = start; i < end; i++)
            //{
            //    result.Append(canidate[i]);
            //}

            return result.ToString();
        }
    
    }
}
