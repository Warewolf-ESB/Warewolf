/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces.Enums;
using Microsoft.Practices.Prism.Mvvm;
using Warewolf.Data;

namespace Warewolf.Configuration
{
    public class LegacySettingsData : AuditSettingsDataBase, IEquatable<LegacySettingsData>
    {
        public string AuditFilePath { get; set; }

        public bool Equals(LegacySettingsData other)
        {
            var equals = base.Equals(other);
            equals &= string.Equals(AuditFilePath, other.AuditFilePath, StringComparison.InvariantCultureIgnoreCase);
            return equals;
        }
    }
}