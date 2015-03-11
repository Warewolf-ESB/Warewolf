
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
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
    public interface IRecordsetScopingObject
    {
        IRecordsetTO GetRecordset(string recsetName);
        string Finalize(string dataListShape, string currentDataList);
        string PostingDataToDataList(IList<OutputTO> outputs, string currentDataList, string dataListShape);

        string DataList { get; }
    }
}
