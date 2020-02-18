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

namespace Warewolf.Configuration
{
    public class ClusterSettingsData : IEquatable<ClusterSettingsData>
    {
        public string Key { get; set; }

        public bool Equals(ClusterSettingsData other)
        {
            var equals = true;
            equals &= string.Equals(Key, other.Key, StringComparison.InvariantCultureIgnoreCase);

            return equals;
        }
    }
}
