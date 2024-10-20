/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2022 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

namespace Warewolf.Data
{
    public class AuditSettingsDataBase: BindableBase
    {
        public string Endpoint { get; internal set; }
        public bool IncludeEnvironmentVariable { get; set; }

        public bool Equals(AuditSettingsDataBase other)
        {
            var equals = Endpoint == other.Endpoint;
            equals &= IncludeEnvironmentVariable.Equals(other.IncludeEnvironmentVariable);
            return equals;
        }
    }
}