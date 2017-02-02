/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Diagnostics.CodeAnalysis;

namespace Dev2.PathOperations
{

    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide a TO for Put operations
    /// </summary>
    public class Dev2PutOperationTOFactory
    {
        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public IFileWrite CreateDev2PutOperationTO(bool append, string content, bool overwrite)
        {
            return new Dev2PutOperationTO(append, content, overwrite);
        }
    }
}
