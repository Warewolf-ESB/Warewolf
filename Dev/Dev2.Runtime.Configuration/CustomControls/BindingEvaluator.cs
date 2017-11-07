/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993] for details.
// All other rights reserved.

using System.Windows.Data;

namespace System.Windows.Controls
{
    internal class BindingEvaluator<T> : FrameworkElement
    {
        private Binding _binding;

        #region public T Value

        public T Value
        {
            get { return (T)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }
        
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(T),
                typeof(BindingEvaluator<T>),
                new PropertyMetadata(default(T)));

        #endregion public string Value
        
        public Binding ValueBinding
        {
            get { return _binding; }
            set
            {
                _binding = value;
                SetBinding(ValueProperty, _binding);
            }
        }
        
        public BindingEvaluator(Binding binding)
        {
            SetBinding(ValueProperty, binding);
        }
        
        public void ClearDataContext()
        {
            DataContext = null;
        }
        
        public T GetDynamicValue(object o)
        {
            DataContext = o;
            return Value;
        }
    }
}
