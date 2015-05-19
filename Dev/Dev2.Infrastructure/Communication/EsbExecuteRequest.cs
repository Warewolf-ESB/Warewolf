
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Text;

namespace Dev2.Communication
{

    /// <summary>
    /// Internal Service Request Object - Used mainly by the studio, but server can send request if service is web based
    /// </summary>
    public class EsbExecuteRequest
    {
        public string ServiceName { get; set; }

        public Dictionary<string, StringBuilder> Args { get; set; }

        public StringBuilder ExecuteResult { get; set; }

        public bool WasInternalService { get; set; }
 
        public EsbExecuteRequest()
        {
            ExecuteResult = new StringBuilder();
        }

        public void AddArgument(string key, StringBuilder value)
        {
            if (Args == null)
            {
                Args = new Dictionary<string, StringBuilder>();
            }

            Args.Add(key, value);
        }
    }
}
