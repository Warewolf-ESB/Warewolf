/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

namespace System.Windows.Controls
{
    public class PopulatingEventArgs : RoutedEventArgs
    {
        public string Parameter { get; private set; }
        
        public bool Cancel { get; set; }
        
        public PopulatingEventArgs(string parameter, RoutedEvent routedEvent)
            : base(routedEvent)
        {
            Parameter = parameter;
        }
    }
}
