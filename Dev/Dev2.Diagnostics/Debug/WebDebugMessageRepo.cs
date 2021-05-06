/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

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

                var key = new Tuple<Guid, Guid>(clientId, sessionId);
                if (Data.TryGetValue(key, out IList<IDebugState> list))
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
