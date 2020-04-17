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
using Warewolf.Streams;

namespace Warewolf.Data
{
    public interface IEncryptedPayload
    {
        string Payload { get; }
    }

    public class NamedGuidWithEncryptedPayload : NamedGuid, IEncryptedPayload
    {
        protected string _payload;

        public NamedGuidWithEncryptedPayload(string name, Guid value, string payload)
        {
            Name = name;
            Value = value;
            Payload = payload;
        }

        public NamedGuidWithEncryptedPayload()
        {
            Name = "";
            Value = Guid.Empty;
            Payload = "";
        }

        public virtual string Payload
        {
            get => _payload;
            set
            {
                _payload = value;
                OnPropertyChanged();
            }
        }

        public T LoadPayload<T>(IDeserializer deserializer) where T : class, new()
        {
            return default;
        }


        public NamedGuidWithEncryptedPayload Clone()
        {
            return new NamedGuidWithEncryptedPayload
            {
                _name = _name,
                _value = _value,
                _payload = _payload
            };
        }
    }
}