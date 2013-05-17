using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Network;
using System.Cryptography;

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
