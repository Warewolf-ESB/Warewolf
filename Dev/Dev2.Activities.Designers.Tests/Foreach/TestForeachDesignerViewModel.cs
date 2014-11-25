
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Activities.Presentation.Model;
using Dev2.Activities.Designers2.Foreach;
using Dev2.Data.Enums;

namespace Dev2.Activities.Designers.Tests.Foreach
{
    public class TestForeachDesignerViewModel : ForeachDesignerViewModel
    {
        public TestForeachDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
        }
        public enForEachType ForEachType { set { SetProperty(value); } get { return GetProperty<enForEachType>(); } }
    }
}
