
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;

namespace Dev2.DataList.Contract
{
    public interface IRecordsetTO {
        int CurrentIndex { get; set; }
        void InsertBodyAtIndex(int IndexNumber, string PayLoad);
        void InsertFieldAtIndex(int IndexNumber, string FieldName, string PayLoad);
        string GetRecordAtIndex(int IndexNumber);
        string GetRecordAtCurrentIndex();
        void InsertFieldAtCurrentIndex(string FieldName, string PayLoad);
        void InsertBodyAtCurrentIndex(string PayLoad);
        bool IsEmpty { get; }
        string RecordsetAsString { get; }
        int RecordCount { get; }
        string RecordsetName { get; }
        bool InitailEmpty { get; set; }        
        void InsertWholeRecordset(string PayLoad);
        string FetchFieldAtIndex(int IndexNumber, string FieldName);

        IList<IRecordSetDefinition> Def { get; }
    }
}
