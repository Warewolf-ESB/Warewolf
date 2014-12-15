
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
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
        private readonly ConcurrentDictionary<string, string> _resultCache = new ConcurrentDictionary<string, string>();

        private static ResultsCache _instance;
        public static ResultsCache Instance
        {
            get
            {
                return _instance ?? (_instance = new ResultsCache());
            }
        }

        private ResultsCache() { }

        public bool AddResult(FutureReceipt receipt, string payload)
        {
            if(string.IsNullOrEmpty(payload))
            {
                throw new ArgumentNullException("payload");
            }

            if(receipt == null)
            {
                throw new ArgumentNullException("receipt");
            }

            return _resultCache.TryAdd(receipt.ToKey(), payload);
        }

        public string FetchResult(FutureReceipt receipt)
        {
            if(receipt == null)
            {
                throw new ArgumentNullException("receipt");
            }

            string result;
            if(!_resultCache.TryRemove(receipt.ToKey(), out result))
            {
                result = string.Empty;
            }
            return result;
        }

        public bool ContainsPendingRequestForUser(string user)
        {
            if(string.IsNullOrEmpty(user))
            {
                throw new ArgumentNullException("user");
            }

            var hasResults = _resultCache.Keys.Any(c => c.Contains(user + "!"));
            return hasResults;
        }
    }
}
