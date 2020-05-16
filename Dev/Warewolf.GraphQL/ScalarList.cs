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
    public class ScalarList
    {
        public ScalarList()
        {
            Value = new List<string>();
        }

        public List<string> Value { get; set; }
        public string Name { get; set; }
    }

    public class ScalarListType : ObjectGraphType<ScalarList>
    {
        public ScalarListType()
        {
            Field(list => list.Name);
            Field(list => list.Value, type: typeof(ListGraphType<StringGraphType>));
        }
    }
}