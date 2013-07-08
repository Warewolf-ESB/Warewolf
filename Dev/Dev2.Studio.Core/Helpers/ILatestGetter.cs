
using System;

namespace Dev2.Studio.Core.Helpers
{
    // PBI 9512 - 2013.06.07 - TWR: added
    public interface ILatestGetter
    {
        event EventHandler Invoked;
        void GetLatest(string uri, string filePath);
    }
}
