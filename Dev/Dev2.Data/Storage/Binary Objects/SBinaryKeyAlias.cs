
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace Dev2.Data.Storage.Binary_Objects
{
    /// <summary>
    /// Store the key alias on the mmf stack ;)
    /// Each key is 64 bytes long
    /// PBI : 10440
    /// </summary>
    public struct SBinaryKeyAlias
    {
        public byte[] guid; // 32 spaces for guid
        public byte[] colName; // 32 spaces for column name
    }
}
