
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.PathOperations.Interfaces;

namespace Dev2.PathOperations
{

    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide a generic path abstraction for IO operations
    /// </summary>
    public interface IActivityIOPath : IPathAuth, IPathCertVerify
    {

        /// <summary>
        /// The path type FTP, FTPS, FileSystem
        /// </summary>
        enActivityIOPathType PathType { get; }

        /// <summary>
        /// The string version of the path
        /// </summary>
        string Path { get; set; }

        /// <summary>
        /// Convert this object into XML
        /// </summary>
        /// <returns></returns>
        string ToXML();
    }
}
