
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
using System.Security;
using System.Security.Cryptography;
using System.IO;

namespace System.Cryptography
{
    public static class CryptUtility
    {
        #region Instance Fields
        private static object _syncSHA1 = new object();
        private static byte[] _sha1Buffer = new byte[1024];
        private static SHA1CryptoServiceProvider _sha1Provider = new SHA1CryptoServiceProvider();
        private static MD5 _md5 = MD5.Create();

        internal static Encoding Encoding = Encoding.ASCII;
        #endregion

        #region SHA1 Handling
        public static string GetHashSHA1String(string phrase)
        {
            return BitConverter.ToString(GetHashSHA1(phrase));
        }

        public static byte[] GetHashSHA1(string phrase)
        {
            byte[] toReturn = null;
            int length = phrase.Length;

            lock (_syncSHA1)
            {
                if (length > _sha1Buffer.Length) _sha1Buffer = new byte[length];
                length = Encoding.GetBytes(phrase, 0, length, _sha1Buffer, 0);
                toReturn = _sha1Provider.ComputeHash(_sha1Buffer, 0, length);
            }

            return toReturn;
        }
        #endregion

        #region MD5 Handling
        public static byte[] CalculateMD5(Stream buffer)
        {
            byte[] toReturn = null;
            lock (_md5) toReturn = _md5.ComputeHash(buffer);
            return toReturn;
        }

        public static byte[] CalculateMD5(byte[] buffer)
        {
            byte[] toReturn = null;
            lock (_md5) toReturn = _md5.ComputeHash(buffer);
            return toReturn;
        }

        public static byte[] CalculateMD5(byte[] buffer, int offset, int count)
        {
            byte[] toReturn = null;
            lock (_md5) toReturn = _md5.ComputeHash(buffer, offset, count);
            return toReturn;
        }
        #endregion

        #region HashDataBroker Handling
        public static byte[] FinalizeHash(HashAlgorithm algorithm, params HashDataBroker[] brokers)
        {
            MemoryStream buffer = new MemoryStream();
            foreach (HashDataBroker broker in brokers) buffer.Write(broker.RawData, 0, broker.Length);
            buffer.Position = 0;
            return algorithm.ComputeHash(buffer);
        }

        public static BigInteger HashToBigInteger(HashAlgorithm algorithm, params HashDataBroker[] brokers)
        {
            return new BigInteger(FinalizeHash(algorithm, brokers));
        }
        #endregion
    }
}
