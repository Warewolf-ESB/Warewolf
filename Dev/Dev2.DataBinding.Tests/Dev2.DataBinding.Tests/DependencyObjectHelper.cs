using System;
using System.Collections.Generic;
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

            foreach(DependencyProperty dp in dpList)
            {
                var b = BindingOperations.GetBinding(element as DependencyObject, dp);
                if(b != null && b.Path.Path != "(0)")
                {
                    bindings.Add(b);
                }
            }

            return bindings;
        }

        public static List<DependencyProperty> GetDependencyProperties(Object element)
        {
            List<DependencyProperty> properties = new List<DependencyProperty>();
            MarkupObject markupObject = MarkupWriter.GetMarkupObjectFor(element);
            if(markupObject != null)
            {
                foreach(MarkupProperty mp in markupObject.Properties)
                {
                    if(mp.DependencyProperty != null)
                    {
                        properties.Add(mp.DependencyProperty);
                    }
                }
            }

            return properties;
        }

        public static List<DependencyProperty> GetAttachedProperties(Object element)
        {
            List<DependencyProperty> attachedProperties = new List<DependencyProperty>();
            MarkupObject markupObject = MarkupWriter.GetMarkupObjectFor(element);
            if(markupObject != null)
            {
                foreach(MarkupProperty mp in markupObject.Properties)
                {
                    if(mp.IsAttached)
                    {
                        attachedProperties.Add(mp.DependencyProperty);
                    }
                }
            }

            return attachedProperties;
        }

        public static bool ContainsBinding(this List<Binding> bindings, string bindingPath)
        {
            return bindings.Exists(binding => binding.Path.Path == bindingPath);
        }

        public static Binding GetBinding(this List<Binding> bindings, string bindingPath)
        {
            return bindings.Find(binding => binding.Path.Path == bindingPath);
        }
    }
}