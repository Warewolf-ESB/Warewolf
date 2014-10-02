
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup.Primitives;

namespace Dev2.DataBinding.Tests
{
    public static class DependencyObjectHelper
    {
        public static List<Binding> GetBindingObjects(Object element)
        {
            List<Binding> bindings = new List<Binding>();
            List<DependencyProperty> dpList = new List<DependencyProperty>();
            dpList.AddRange(GetDependencyProperties(element));
            dpList.AddRange(GetAttachedProperties(element));

            // ReSharper disable LoopCanBeConvertedToQuery
            foreach(DependencyProperty dp in dpList)
            // ReSharper restore LoopCanBeConvertedToQuery
            {
                var tmp = element as DependencyObject;

                if(tmp != null)
                {
                    var b = BindingOperations.GetBinding(tmp, dp);
                    if(b != null && b.Path.Path != "(0)")
                    {
                        bindings.Add(b);
                    }
                }
            }

            return bindings;
        }

        /// <summary>
        /// Gets the dependency properties.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        public static List<DependencyProperty> GetDependencyProperties(Object element)
        {
            MarkupObject markupObject = MarkupWriter.GetMarkupObjectFor(element);

            return (from mp in markupObject.Properties
                    where mp.DependencyProperty != null
                    select mp.DependencyProperty).ToList();
        }

        /// <summary>
        /// Gets the attached properties.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        public static List<DependencyProperty> GetAttachedProperties(Object element)
        {
            MarkupObject markupObject = MarkupWriter.GetMarkupObjectFor(element);

            return (from mp in markupObject.Properties
                    where mp.IsAttached
                    select mp.DependencyProperty).ToList();
        }

        /// <summary>
        /// Determines whether the specified bindings contains binding.
        /// </summary>
        /// <param name="bindings">The bindings.</param>
        /// <param name="bindingPath">The binding path.</param>
        /// <returns></returns>
        public static bool ContainsBinding(this List<Binding> bindings, string bindingPath)
        {
            return bindings.Exists(binding => binding.Path.Path == bindingPath);
        }

        /// <summary>
        /// Gets the binding.
        /// </summary>
        /// <param name="bindings">The bindings.</param>
        /// <param name="bindingPath">The binding path.</param>
        /// <returns></returns>
        public static Binding GetBinding(this List<Binding> bindings, string bindingPath)
        {
            return bindings.Find(binding => binding.Path.Path == bindingPath);
        }
    }
}
