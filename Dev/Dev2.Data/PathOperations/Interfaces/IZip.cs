/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
    /// Purpose : To provide properties related to Zip operations as per DotNetZip : http://dotnetzip.codeplex.com/
    /// </summary>
    public interface IZip {

        /// <summary>
        /// The compression ratio for ziping
        /// </summary>
        string CompressionRatio { get; set; }

        /// <summary>
        /// The password for the created archive
        /// </summary>
        string ArchivePassword { get; set; }

        /// <summary>
        /// The name of the archive
        /// </summary>
        string ArchiveName { get; set; }

    }
}
