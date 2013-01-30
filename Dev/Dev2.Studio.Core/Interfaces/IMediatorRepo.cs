using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Studio.Core;

namespace Dev2.Studio {
    public interface IMediatorRepo {

        void addKeyList(int itemHashCode, MediatorMessages msg, IList<string> keyList);

        void addKey(int itemHashCode, MediatorMessages msg, string key);

        void deregisterAllItemMessages(int itemHashCode);

        void deregisterItemMessage(int itemHashCode, MediatorMessages msg);

        void suspendAllItemMessages(int itemHashCode);

        void reregisterAllItemMessages(int itemHashCode);
    }
}
