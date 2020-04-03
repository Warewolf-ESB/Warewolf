/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Warewolf.Interfaces.Auditing;
using Warewolf.Triggers;

namespace Warewolf.Auditing
{
    public abstract class AuditQueryable : IAuditQueryable
    {
        
        protected bool IsUrlEncoded(string text)
        {
            return (WebUtility.UrlDecode(text)) != text;
        }

        public string GetDecoded(string encodedText)
        {
            return WebUtility.UrlDecode(encodedText);
        }

        public abstract IEnumerable<IExecutionHistory> QueryTriggerData(Dictionary<string, StringBuilder> values);

        public abstract IEnumerable<IAudit> QueryLogData(Dictionary<string, StringBuilder> values);

        protected T GetValue<T>(string key, Dictionary<string, StringBuilder> values)
        {
            var toReturn = default(T);
            if (values.TryGetValue(key, out StringBuilder value))
            {
                var item = value.ToString();
                return (T) Convert.ChangeType(item, typeof(T));
            }
            return toReturn;
        }
    }
}