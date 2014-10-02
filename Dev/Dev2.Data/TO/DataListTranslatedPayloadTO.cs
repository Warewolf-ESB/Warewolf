
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Dev2.DataList.Contract.TO {
    public class DataListTranslatedPayloadTO {


        private readonly byte[] _payloadB;
        private readonly string _payloadS;

        public DataListTranslatedPayloadTO(string payload) {
            _payloadS = payload;
            _payloadB = null;
        }

        public DataListTranslatedPayloadTO(byte[] payload) {
            _payloadB = payload;
            _payloadS = null;
        }

        /// <summary>
        /// Fetches as string.
        /// </summary>
        /// <returns></returns>
        public string FetchAsString() {
            string result = string.Empty;

            if (_payloadS != null) {
                result = _payloadS;
            }

            return result;
        }

        /// <summary>
        /// Fetches as byte array.
        /// </summary>
        /// <returns></returns>
        public byte[] FetchAsByteArray() {
            byte[] result = null;

            if (_payloadB != null) {
                result = _payloadB;
            }

            return result;
        }

        public object FetchAsObject()
        {
            object obj = new object();

            if (_payloadB != null)
            {
                using(Stream streamWrite = new MemoryStream(_payloadB))
                {
                    BinaryFormatter binaryWrite = new BinaryFormatter();
                    streamWrite.Seek(0, SeekOrigin.Begin);
                    obj = binaryWrite.Deserialize(streamWrite);
                    streamWrite.Close();
                    streamWrite.Dispose();
                }
            }

            return obj;
        }

        /// <summary>
        /// Determines whether [is string data].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is string data]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsStringData() {
            return (_payloadB == null);
        }

        /// <summary>
        /// Determines whether [is byte data].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is byte data]; otherwise, <c>false</c>.
        /// </returns>
        public bool IsByteData() {
            return (_payloadS == null);
        }

    }
}
