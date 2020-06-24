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
using Warewolf.Data;

namespace Warewolf.Configuration
{
    public class AuditingSettingsData : AuditSettingsDataBase, IEquatable<AuditingSettingsData>
    {
        private NamedGuidWithEncryptedPayload _loggingDataSource = new NamedGuidWithEncryptedPayload();
        private bool? _encryptDataSource;

        public NamedGuidWithEncryptedPayload LoggingDataSource
        {
            get => _loggingDataSource;
            set => SetProperty(ref _loggingDataSource, value);
        }

        public bool EncryptDataSource {
            get => _encryptDataSource ?? true;
            set => SetProperty(ref _encryptDataSource, value);

        }

        public AuditingSettingsData Clone()
        {
            var result = (AuditingSettingsData)MemberwiseClone();
            result._loggingDataSource = LoggingDataSource.Clone();
            return result;
        }

        public bool Equals(AuditingSettingsData obj)
        {
            if (obj is AuditingSettingsData other)
            {
                var equals = base.Equals(other);
                equals &= LoggingDataSource.Equals(other.LoggingDataSource);
                equals &= EncryptDataSource.Equals(other.EncryptDataSource);
                return equals;
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AuditingSettingsData);
        }
    }
}
