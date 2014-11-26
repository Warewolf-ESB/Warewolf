
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace Dev2.DataList.Contract
{
    /// <summary>
    /// List of operations a user can perform on system tags
    /// </summary>
    public enum enTranslationDepth
    {
       /* Take the shape */
       Shape,
       /* Take the data from the right, avoid overwriting existing data if present */
       Data, 
       /* Take the data from the right overwriting it all */
       Data_With_Blank_OverWrite
    }
}
