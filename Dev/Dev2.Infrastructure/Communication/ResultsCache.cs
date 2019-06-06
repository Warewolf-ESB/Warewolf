/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Dev2.Communication
{
    public class ResultsCache
    {
        readonly ConcurrentDictionary<string, string> _resultCache = new ConcurrentDictionary<string, string>();

        private readonly static ResultsCache _instance = new ResultsCache();
        public static ResultsCache Instance { get => _instance; }

        ResultsCache() { }

        public bool AddResult(FutureReceipt receipt, string payload)
        {
            if(string.IsNullOrEmpty(payload))
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if(receipt == null)
            {
                throw new ArgumentNullException(nameof(receipt));
            }

            return _resultCache.TryAdd(receipt.ToKey(), payload);
        }

        public string FetchResult(FutureReceipt receipt)
        {
            if(receipt == null)
            {
                throw new ArgumentNullException(nameof(receipt));
            }

            if (!_resultCache.TryRemove(receipt.ToKey(), out string result))
            {
                result = string.Empty;
            }
            return result;
        }

        public bool ContainsPendingRequestForUser(string user)
        {
            if(string.IsNullOrEmpty(user))
            {
                throw new ArgumentNullException(nameof(user));
            }

            var hasResults = _resultCache.Keys.Any(c => c.Contains(user + "!"));
            return hasResults;
        }
    }
}
