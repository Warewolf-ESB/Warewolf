#pragma warning disable
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
using GraphQL.Types;

namespace Warewolf.GraphQL
{
    public class RecordSet
    {
        public RecordSet()
        {
            Columns = new List<ScalarList>();
        }

        public string Name { get; set; }
        public List<ScalarList> Columns { get; set; }
    }

    public class RecordsetType : ObjectGraphType<RecordSet>
    {
        public RecordsetType()
        {
            Field(s => s.Name);
            Field(s => s.Columns, type: typeof(ListGraphType<ScalarListType>));
        }
    }
}