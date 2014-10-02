
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

namespace System.Cryptography
{
    public sealed class HashDataBroker
    {
        #region Instance Fields
        internal byte[] RawData;
        #endregion

        #region Internal Properties
        internal int Length { get { return RawData.Length; } }
        #endregion

        #region Constructor
        public HashDataBroker(byte[] data)
        {
            RawData = data;
        }
        #endregion

        #region Conversion Handling
        public static implicit operator HashDataBroker(byte[] data)
        {
            return new HashDataBroker(data);
        }

        public static implicit operator HashDataBroker(string str)
        {
            return new HashDataBroker(CryptUtility.Encoding.GetBytes(str));
        }

        public static implicit operator HashDataBroker(BigInteger integer)
        {
            return new HashDataBroker(integer.GetBytes());
        }

        public static implicit operator HashDataBroker(uint integer)
        {
            return new HashDataBroker(new BigInteger(integer).GetBytes());
        }
        #endregion
    }
}
