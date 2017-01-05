/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.RecordsLengthNullHandler;

namespace Dev2.Activities.Designers.Tests.RecordsLengthNullHandler
{
    public class TestRecordsLengthnullHandlerDesignerViewModel : RecordsLengthDesignerViewModel
    {
        public TestRecordsLengthnullHandlerDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public TestRecordsLengthnullHandlerDesignerViewModel(ModelItem modelItem, string recordSetName)
            : base(modelItem)
        {
            RecordsetName = recordSetName;
        }

        public string CountNumber { get { return GetProperty<string>(); } set { SetProperty(value); } }
        public string RecordsetName { get { return GetProperty<string>(); } set { SetProperty(value); } }
    }
}
