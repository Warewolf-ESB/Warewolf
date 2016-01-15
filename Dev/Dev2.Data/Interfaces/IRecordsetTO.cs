
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
// ReSharper disable CheckNamespace

namespace Dev2.DataList.Contract
{
    public interface IRecordsetTO {
        int CurrentIndex { get; set; }
        void InsertBodyAtIndex(int indexNumber, string payLoad);
        void InsertFieldAtIndex(int indexNumber, string fieldName, string payLoad);
        string GetRecordAtIndex(int indexNumber);
        string GetRecordAtCurrentIndex();
        void InsertFieldAtCurrentIndex(string fieldName, string payLoad);
        void InsertBodyAtCurrentIndex(string payLoad);
        bool IsEmpty { get; }
        string RecordsetAsString { get; }
        int RecordCount { get; }
        string RecordsetName { get; }
        bool InitailEmpty { get; set; }        
        void InsertWholeRecordset(string payLoad);
        string FetchFieldAtIndex(int indexNumber, string fieldName);

        IList<IRecordSetDefinition> Def { get; }
    }
}
