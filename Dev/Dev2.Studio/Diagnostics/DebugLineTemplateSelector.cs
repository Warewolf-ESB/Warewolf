/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Windows;
using System.Windows.Controls;


namespace Dev2.Studio.Diagnostics
{
    public class DebugLineTemplateSelector : DataTemplateSelector
    {
        public string Key { get; set; }
        public DataTemplate ItemTemplate { get; set; }
        public DataTemplate GroupTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) => item is DebugLineGroup ? GroupTemplate : ItemTemplate;
    }
}
