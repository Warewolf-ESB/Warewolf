using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Common {
    public interface IDev2Tokenizer {

        bool HasMoreOps();

        string NextToken();

    }
}
