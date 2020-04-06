/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

namespace Warewolf.Configuration
{
    public class AuditingSettingsData : IHasChanged
    {
        public string Endpoint { get; internal set; } = "ws://127.0.0.1:5000/ws";

        public bool Equals(AuditingSettingsData other)
        {
            var equals = Endpoint == other.Endpoint;

            return equals;
        }

        public bool HasChanged { get; set; }
    }
}
