/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;
using System.Windows.Controls;


namespace Dev2.Studio.CustomControls
{
    [TemplatePart(Name = PART_Label, Type = typeof(Label))]
    public class Dev2StatusBar : TextBox
    {
        private const string PART_Label = "StatusBarLabel";

        // Using a DependencyProperty as the backing store for StatusBarLabelText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusBarLabelTextProperty =
            DependencyProperty.Register("StatusBarLabelText", typeof(string), typeof(Dev2StatusBar), new PropertyMetadata(string.Empty));

        // Using a DependencyProperty as the backing store for ProgressBarVisiblity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProgressBarVisiblityProperty =
            DependencyProperty.Register("ProgressBarVisiblity", typeof(Visibility), typeof(Dev2StatusBar), new PropertyMetadata(Visibility.Visible));

        public Dev2StatusBar()
        {
            DefaultStyleKey = typeof(Dev2StatusBar);
        }
    }
}
