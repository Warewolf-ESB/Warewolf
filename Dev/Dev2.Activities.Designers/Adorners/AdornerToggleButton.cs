
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Dev2.Activities.Adorners
{
    /// <summary>
    /// Used by the adorner presenter to get a unified style for the toggle buttons added to the OptionsAdorner
    /// </summary>
    /// <author>Jurie.smit</author>
    /// <date>2013/07/23</date>
    public class AdornerToggleButton : ToggleButton
    {
        /// <summary>
        /// When overridden in a derived class, is invoked whenever application code or
        /// internal processes call <see cref="M:System.Windows.FrameworkElement.ApplyTemplate" />.
        /// </summary>
        /// <author>Jurie.smit</author>
        /// <date>2013/07/23</date>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            DefaultStyleKey = ToolBar.ToggleButtonStyleKey;
        }
    }
}
