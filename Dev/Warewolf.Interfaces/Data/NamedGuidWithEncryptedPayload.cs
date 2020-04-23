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

namespace Warewolf.Data
{
    public sealed class NamedGuidWithEncryptedPayload : NamedGuid, IEquatable<NamedGuidWithEncryptedPayload>
    {
        private string _payload;

        public NamedGuidWithEncryptedPayload()
        {
            Name = "";
            Value = Guid.Empty;
            Payload = "";
        }

        public string Payload
        {
            get => _payload;
            set
            {
                _payload = value;
                OnPropertyChanged();
            }
        }

        public bool Equals(NamedGuidWithEncryptedPayload other)
        {
            if (other is NamedGuidWithEncryptedPayload guidWithEncryptedPayload)
            {
                var eq = base.Equals(other);
                eq &= Payload.Equals(guidWithEncryptedPayload.Payload);
                return eq;
            }

            return false;
        }
        public override bool Equals(object other)
        {
            return Equals(other as NamedGuidWithEncryptedPayload);
        }

        public new NamedGuidWithEncryptedPayload Clone()
        {
            return (NamedGuidWithEncryptedPayload)MemberwiseClone();
        }
    }
}
