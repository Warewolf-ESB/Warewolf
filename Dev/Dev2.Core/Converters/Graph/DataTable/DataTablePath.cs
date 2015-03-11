/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces.Core.Graph;
using Unlimited.Framework.Converters.Graph;

namespace Dev2.Converters.Graph.DataTable
{
    public class DataTablePath : BasePath
    {
        public DataTablePath() : this("", "")
        {
        }

        public DataTablePath(string tblName, string colName)
        {
            string tmp = colName;

            if (!string.IsNullOrEmpty(tblName))
            {
                tmp = string.Concat(tblName, "().", colName);
            }

            DisplayPath = tmp;
            ActualPath = tmp;
        }

        public override IEnumerable<IPathSegment> GetSegements()
        {
            throw new NotImplementedException();
        }

        public override IPathSegment CreatePathSegment(string pathSegmentString)
        {
            throw new NotImplementedException();
        }
    }
}