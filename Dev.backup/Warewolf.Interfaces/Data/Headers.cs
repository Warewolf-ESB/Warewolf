/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;

namespace Warewolf.Data
{
    public class Headers
    {
        Dictionary<string, string[]> _headers = new Dictionary<string, string[]>();
       
        public string[] this[string key, string[] default_=null]
        {
            get => KeyExists(key) ? _headers[key] : default_;
            set => _headers[key] = value;
        }

        public bool KeyExists(string key)
        {
            return _headers.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
        {
            return _headers.GetEnumerator();
        }
    }
}