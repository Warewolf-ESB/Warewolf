
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
using Dev2.Activities.Designers2.Random;
using Dev2.Common.Interfaces.Enums;

namespace Dev2.Activities.Designers.Tests.Random
{
    public class TestRandomDesignerViewModel : RandomDesignerViewModel
    {
        public TestRandomDesignerViewModel(ModelItem modelItem)
            : base(modelItem)
        {
        }
        public enRandomType RandomType { set { SetProperty(value); } get { return GetProperty<enRandomType>(); } }
    }
}
