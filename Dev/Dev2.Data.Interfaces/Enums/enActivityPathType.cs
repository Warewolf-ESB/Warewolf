/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;

namespace Dev2.Data.Interfaces.Enums {

    /// <summary>
    /// PBI : 1172
    /// Status : New
    /// Purpose : To abstract IO endpoint types
    /// </summary>
    [Serializable]

    public enum enActivityIOPathType {

        FileSystem,
        FTP,
        FTPS,
        SFTP,
        FTPES,
        Invalid
    }
}
