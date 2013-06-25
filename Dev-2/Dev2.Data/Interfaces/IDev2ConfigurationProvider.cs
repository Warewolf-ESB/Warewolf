using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DataList.Contract
{
    public interface IDev2ConfigurationProvider {

        string ReadKey(string key);

        void OnReadFailure();
    }
}
