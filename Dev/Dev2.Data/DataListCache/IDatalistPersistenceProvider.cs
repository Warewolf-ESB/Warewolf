
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.DataList.Contract;
using Dev2.DataList.Contract.Binary_Objects;

namespace Dev2.Data.DataListCache
{
    public interface IDataListPersistenceProvider
    {
        bool WriteDataList(Guid datalistID, IBinaryDataList datalist, ErrorResultTO errors);
        IBinaryDataList ReadDatalist(Guid datalistID, ErrorResultTO errors);
        bool DeleteDataList(Guid id, bool onlyIfNotPersisted);
    }
}
