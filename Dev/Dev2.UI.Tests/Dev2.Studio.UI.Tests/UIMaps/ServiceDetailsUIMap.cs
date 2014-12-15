
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


namespace Dev2.Studio.UI.Tests.UIMaps.ServiceDetailsUIMapClasses
{
    using System.Drawing;
    using Microsoft.VisualStudio.TestTools.UITesting.WpfControls;
    
    
    public partial class ServiceDetailsUIMap
    {
        public bool ServiceDetailsWindowExists()
        {
            WpfWindow theWindow = ServiceDetailsWindow();
            Point p = new Point();
            bool doesExist = theWindow.TryGetClickablePoint(out p);
            return doesExist;
        }

    }
}
