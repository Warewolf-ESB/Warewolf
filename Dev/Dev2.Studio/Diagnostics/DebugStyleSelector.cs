
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;
using System.Windows.Controls;
using Dev2.ViewModels.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Dev2.Studio.Diagnostics
{
    public class DebugStyleSelector : StyleSelector
    {

        public Style DebugStringStyle { get; set; }

        public Style DebugStateStyle { get; set; }

        public override Style SelectStyle(object item,
        DependencyObject container)
        {
            if(item.GetType() == typeof(DebugStringTreeViewItemViewModel))
            {
                return DebugStringStyle;
            }

            if(item.GetType() == typeof(DebugStateTreeViewItemViewModel))
            {
                return DebugStateStyle;
            }
            return null;
        }

    }
}
