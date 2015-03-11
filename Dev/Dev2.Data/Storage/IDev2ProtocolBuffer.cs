
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using Dev2.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dev2.Data.Storage
{
    /// <summary>
    /// Create a byte streamable object for persist storage, based upon Google's ProtoBuf
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IDev2ProtocolBuffer : ISpookyLoadable<string>
    {

        /// <summary>
        /// Push an object into a storage byte array ;)
        /// </summary>
        /// <returns></returns>
        byte[] ToByteArray();

        /// <summary>
        /// Convert a storable byte array into an object ;)
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        void ToObject(byte[] bytes); 
    }
}
