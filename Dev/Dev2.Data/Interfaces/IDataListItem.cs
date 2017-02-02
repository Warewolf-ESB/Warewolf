/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.DataList.Contract;

namespace Dev2.Data.Interfaces
{
    public interface IDataListItem
    {
        string Field { get; set; }
        string Recordset { get; set; }
        string Index { get; set; }        
        enRecordsetIndexType RecordsetIndexType { get; set; }
        bool CanHaveMutipleRows { get; set; }
        string ParentName { get; set; }
        bool IsObject { get; set; }
        string Value { get; set; }
        string DisplayValue { get; set; }
        string Description { get; set; }
    }
}
