
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Cryptography;
using System.Network;
using System.Text;

namespace Dev2.DynamicServices.Network.Auxiliary
{
    public sealed class StudioAuxiliaryAccount : NetworkAccount
    {
        #region Static Members
        private static Encoding _encoding = Encoding.ASCII;

        private static byte[] GetPasswordBytes(string password)
        {
            return _encoding.GetBytes(password);
        }
        #endregion

        #region Instance Fields
        private byte _usernameByteCount;
        #endregion

        #region Constructors
        public StudioAuxiliaryAccount(string username, string password)
            : base(username, GetPasswordBytes(password))
        {
        }

        public StudioAuxiliaryAccount(IByteReaderBase reader)
            : base(reader)
        {
        }
        #endregion

        #region [Get/Set] Handling
        public void SetPassword(string password)
        {
            SetPassword(GetPasswordBytes(password));
        }

        protected override void SetPassword(byte[] value)
        {
            if (_usernameByteCount == Byte.MinValue) _usernameByteCount = (byte)Encoding.ASCII.GetByteCount(_username.ToUpper() + ":");
            byte[] source = new byte[_usernameByteCount + value.Length];
            Encoding.ASCII.GetBytes(_username.ToUpper() + ":", 0, _username.Length + 1, source, 0);
            Buffer.BlockCopy(value, 0, source, _usernameByteCount, value.Length);
            _password = SecureRemotePassword.GenerateCredentialsHash(source);
        }
        #endregion
    }
}
