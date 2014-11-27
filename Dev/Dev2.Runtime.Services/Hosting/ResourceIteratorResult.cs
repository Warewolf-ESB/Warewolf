
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

namespace Dev2.Runtime.Hosting
{
    public class ResourceIteratorResult
    {
        public ResourceIteratorResult()
        {
            Values = new Dictionary<int, string>();
        }

        public string Content { get; set; }

        public Dictionary<int, string> Values { get; private set; }
    }
}
