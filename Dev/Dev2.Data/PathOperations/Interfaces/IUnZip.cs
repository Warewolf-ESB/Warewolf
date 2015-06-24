
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/



namespace Dev2.PathOperations {
    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To provide properties related to UnZip operations as per DotNetZip : http://dotnetzip.codeplex.com/
    /// </summary>
    public interface IUnZip {

        /// <summary>
        /// The password for UnZiping
        /// </summary>
        string ArchivePassword { get; set; }

        /// <summary>
        /// Indicates if the destination must be overwritten or not
        /// </summary>
        bool Overwrite { get; set; }
    }
}
