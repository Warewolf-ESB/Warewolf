/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.IO;

namespace Dev2.Common.Interfaces.Runtime.Services
{
    public interface IMemoryStreamFactory
    {
        MemoryStream New();
        MemoryStream New(byte[] byteArray);
    }
    public class MemoryStreamFactory : IMemoryStreamFactory
    {
        public MemoryStream New()
        {
            return new MemoryStream();
        }

        public MemoryStream New(byte[] byteArray)
        {
            return new MemoryStream(byteArray);
        }
    }
}
