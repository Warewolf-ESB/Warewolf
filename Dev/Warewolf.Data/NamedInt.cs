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
using System.Collections.Generic;
using Warewolf.Options;

namespace Warewolf.Data
{
    public class NamedInt : INamedInt
    {
        public string Name { get; set; }
        public int Value { get; set; }

        public static IEnumerable<INamedInt> GetAll(Type type)
        {
            var enums = Enum.GetValues(type);
            var result = new List<INamedInt>();
            foreach (var entry in enums)
            {
                result.Add(new NamedInt
                {
                    Name = Enum.GetName(type, entry),
                    Value = (int)entry,
                });
            }
            return result;
        }
    }
}
