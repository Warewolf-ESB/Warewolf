
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Activities.Presentation;
using System.Activities.Presentation.Model;

namespace Dev2.Tests
{
    public class TestModelItemUtil
    {
        public static ModelItem CreateModelItem(object objectToMakeModelItem)
        {
            EditingContext ec = new EditingContext();
            ModelTreeManager mtm = new ModelTreeManager(ec);

            mtm.Load(objectToMakeModelItem);

            return mtm.Root;
        }
    }
}
