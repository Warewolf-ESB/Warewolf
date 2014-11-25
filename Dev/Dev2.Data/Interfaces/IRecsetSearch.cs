
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
    /// Interface for the IRecsetSearch class
    /// </summary>
    public interface IRecsetSearch
    {
        string FieldsToSearch { get; set; }
        string Result { get; set; }
        string SearchCriteria { get; set; }
        string SearchType { get; set; }
        string From { get; set; }
        string To { get; set; }
        string StartIndex { get; set; }
        bool MatchCase { get; set; }
        bool RequireAllFieldsToMatch { get; set; }
    }
}
