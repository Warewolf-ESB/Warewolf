using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ActivityUnitTests.Utils {
    public class StringComparer : Comparer<string> {
        public override int Compare(string x, string y) {
            if(x == y) {
                return 0;
            }
            else {
                return 10;
            }
        }
    }
}
