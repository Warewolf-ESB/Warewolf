using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.DB_Sanity
{
    public interface IDataProviderSanitizer
    {

        string SanitizePayload(string xmlFormatedPayload);

        enSupportedDBTypes HandlesType();
    }
}
