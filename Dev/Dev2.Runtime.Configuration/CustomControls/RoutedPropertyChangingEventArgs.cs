/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

namespace System.Windows.Controls
{
    public class RoutedPropertyChangingEventArgs<T> : RoutedEventArgs
    {
        public DependencyProperty Property { get; private set; }
        
        public T OldValue { get; private set; }
        
        public T NewValue { get; set; }
        
        public bool IsCancelable { get; private set; }
        
        public bool Cancel
        {
            get { return _cancel; }
            set
            {
                if (IsCancelable)
                {
                    _cancel = value;
                }
                else
                {
                    if (value)
                    {
                        throw new InvalidOperationException(Dev2.Runtime.Configuration.Properties.Resources.RoutedPropertyChangingEventArgs_CancelSet_InvalidOperation);
                    }
                }
            }
        }

        private bool _cancel;
        
        public bool InCoercion { get; set; }
        
        public RoutedPropertyChangingEventArgs(
            DependencyProperty property,
            T oldValue,
            T newValue,
            bool isCancelable)
        {
            Property = property;
            OldValue = oldValue;
            NewValue = newValue;
            IsCancelable = isCancelable;
            Cancel = false;
        }

#if !SILVERLIGHT

        public RoutedPropertyChangingEventArgs(DependencyProperty property,
            T oldValue, T newValue, bool isCancelable, RoutedEvent routedEvent)
            : base(routedEvent)
        {
            Property = property;
            OldValue = oldValue;
            NewValue = newValue;
            IsCancelable = isCancelable;
            Cancel = false;
        }

#endif
    }
}
