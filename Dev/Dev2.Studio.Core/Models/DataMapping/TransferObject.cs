
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
using Dev2.Common.Interfaces.Data;
using Dev2.DataList.Contract;
using Dev2.Studio.Core.Interfaces.DataList;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Core.Models
{
    public class TransferObject
    {

        public IList<IDev2Definition> OutputScalarList { get; set; }
        public IList<IDev2Definition> InputScalarList { get; set; }
        public IRecordSetCollection OutputRecSet { get; set; }
        public IRecordSetCollection InputRecSet { get; set; }
        public List<IDataListItemModel> DataList { get; set; }
        public List<IDataListItemModel> DataObjectNames { get; set; }
        public IList<IInputOutputViewModel> DisplayOutputData { get; set; }
        public IList<IInputOutputViewModel> DisplayInputData { get; set; }
    }
}
