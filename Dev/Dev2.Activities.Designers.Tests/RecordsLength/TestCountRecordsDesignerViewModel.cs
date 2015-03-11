
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.RecordsLength;

namespace Dev2.Activities.Designers.Tests.RecordsLength
{
    public class TestRecordsLengthDesignerViewModel : RecordsLengthDesignerViewModel
    {
        public TestRecordsLengthDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
        }

        public TestRecordsLengthDesignerViewModel(ModelItem modelItem, string recordSetName)
            : base(modelItem)
        {
            RecordsetName = recordSetName;
        }

        public string CountNumber { get { return GetProperty<string>(); } set { SetProperty(value); } }
        public string RecordsetName { get { return GetProperty<string>(); } set { SetProperty(value); } }
    }
}
