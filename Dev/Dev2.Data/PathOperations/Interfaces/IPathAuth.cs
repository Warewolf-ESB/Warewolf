
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/



namespace Dev2.Data.PathOperations.Interfaces {
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide a common abstraction as it related to file system ( FTP, FTPS, Disk ) operations
    /// </summary>
    public interface IPathAuth {

        /// <summary>
        /// Username for auth
        /// </summary>
        string Username { get; set; }

        /// <summary>
        /// Password for auth
        /// </summary>
        string Password { get; set; }
    }
}
