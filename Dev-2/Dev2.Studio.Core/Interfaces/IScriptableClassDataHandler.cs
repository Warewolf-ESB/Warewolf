using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Studio.Core.Interfaces {
    public interface IScriptableClassDataHandler {
        void DataReceived(string data);
        void DataReceived(string data, string uri);
        void Close();
    }
}
