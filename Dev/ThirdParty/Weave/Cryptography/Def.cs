
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{
    #region IDecryptionProvider
    public interface IDecryptionProvider
    {
        int NextKSRN();
        byte Decrypt(byte encrypted);
        void Decrypt(byte[] data, int offset, int length);
        void Decrypt(byte[] encrypted, int encryptOffset, byte[] decrypted, int decryptOffset, int count);
    }
    #endregion

    #region IEncryptionProvider
    public interface IEncryptionProvider
    {
        int NextKSRN();
        byte Encrypt(byte decrypted);
        void Encrypt(byte[] data, int offset, int length);
        void Encrypt(byte[] decrypted, int decryptOffset, byte[] encrypted, int encryptOffset, int count);
    }
    #endregion

    #region ICryptProvider
    public interface ICryptProvider : IDecryptionProvider, IEncryptionProvider
    {
    }
    #endregion

    #region NullCryptProvider
    public sealed class NullCryptProvider : ICryptProvider
    {
        public static readonly NullCryptProvider Singleton = new NullCryptProvider();

        private NullCryptProvider()
        {
        }

        public int NextKSRN()
        {
            return Byte.MinValue;
        }

        public byte Decrypt(byte encrypted)
        {
            return encrypted;
        }

        public void Decrypt(byte[] data, int offset, int length)
        {
        }

        public void Decrypt(byte[] encrypted, int encryptOffset, byte[] decrypted, int decryptOffset, int count)
        {
            Buffer.BlockCopy(encrypted, encryptOffset, decrypted, decryptOffset, count);
        }

        public byte Encrypt(byte decrypted)
        {
            return decrypted;
        }

        public void Encrypt(byte[] data, int offset, int length)
        {
        }

        public void Encrypt(byte[] decrypted, int decryptOffset, byte[] encrypted, int encryptOffset, int count)
        {
            Buffer.BlockCopy(decrypted, decryptOffset, encrypted, encryptOffset, count);
        }
    }
    #endregion
}
