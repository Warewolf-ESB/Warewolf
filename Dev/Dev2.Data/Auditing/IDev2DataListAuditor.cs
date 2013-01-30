using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Server.Datalist.Auditing
{
    public interface IDev2DataListAuditor
    {

        /// <summary>
        /// Resets this instance.
        /// </summary>
        void Reset();

        /// <summary>
        /// Pushes the change.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="val">The val.</param>
        /// <param name="direction">The direction.</param>
        void PushChange(string key, IBinaryDataListEntry val, string direction);

        /// <summary>
        /// Fetches the changes.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <returns></returns>
        IList<KeyValuePair<string, IBinaryDataListEntry>> FetchChanges(string direction);
    }
}
