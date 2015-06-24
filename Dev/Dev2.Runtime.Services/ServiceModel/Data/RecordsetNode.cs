
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Common.Interfaces.Core.Graph;

namespace Dev2.Runtime.ServiceModel.Data
{
    // BUG 9626 - 2013.06.11 - TWR: refactored
    internal class RecordsetNode
    {
        public RecordsetNode()
        {
            MyProps = new Dictionary<string, IPath>();
            ChildNodes = new List<RecordsetNode>();
        }

        public string Name { get; set; }
        public Dictionary<string, IPath> MyProps { get; set; }
        public List<RecordsetNode> ChildNodes { get; set; }
    }
}
