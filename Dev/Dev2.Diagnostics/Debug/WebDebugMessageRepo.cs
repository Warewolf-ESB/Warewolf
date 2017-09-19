using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Diagnostics.Debug;

namespace Dev2.Diagnostics.Debug
{
    public class WebDebugMessageRepo : DebugMessegaBase<WebDebugMessageRepo>
    {
        public override void AddDebugItem(Guid clientId, Guid sessionId, IDebugState ds)
        {
            lock (Lock)
            {
                if (ds.ParentID == default(Guid) && (ds.StateType != StateType.Start))
                {
                    ds.ParentID = null;
                }

                if (ds.ParentID == default(Guid) && (ds.StateType != StateType.End))
                {
                    ds.ParentID = null;
                }

                IList<IDebugState> list;
                var key = new Tuple<Guid, Guid>(clientId, sessionId);
                if (Data.TryGetValue(key, out list))
                {
                    list.Add(ds);
                }
                else
                {
                    list = new List<IDebugState> { ds };
                    Data[key] = list;
                }
            }
        }
    }
}
