using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Common {
    public class TokenizeError : Exception {

        public TokenizeError(string msg) : base(msg) {}
    }
}
