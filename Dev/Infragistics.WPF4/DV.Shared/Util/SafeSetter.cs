using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;

namespace Infragistics
{
    /// <summary>
    /// Allows for setting properties only if the trust level allows.
    /// </summary>
    public static class SafeSetters
    {
        /// <summary>
        /// The safe setters to associate with the element.
        /// </summary>
        public static readonly DependencyProperty SettersProperty =
        DependencyProperty.RegisterAttached(
        "Setters",
        typeof(SafeSetterCollection),
        typeof(SafeSetters),
        new PropertyMetadata(null,
            (o, e) =>
                {
                    OnSettersChanged(
                        o,
                        e.OldValue as SafeSetterCollection,
                        e.NewValue as SafeSetterCollection);
                }));

        /// <summary>
        /// Sets the setters to associate with an element.
        /// </summary>
        /// <param name="target">The target element.</param>
        /// <param name="setters">The setters to associate.</param>
        public static void SetSetters(DependencyObject target, SafeSetterCollection setters)
        {
            target.SetValue(SettersProperty, setters);
        }

        /// <summary>
        /// Gets the setters associated with an element.
        /// </summary>
        /// <param name="target">The target element.</param>
        /// <returns>The setters.</returns>
        public static SafeSetterCollection GetSetters(DependencyObject target)
        {
            return (SafeSetterCollection)target.GetValue(SettersProperty);
        }

        private static void OnSettersChanged(
            DependencyObject o, 
            SafeSetterCollection oldValue, 
            SafeSetterCollection newValue)
        {
            if (newValue != null)
            {
                newValue.Apply(o);
            }
        }
    }

    /// <summary>
    /// A collection of safe setters.
    /// </summary>
    public class SafeSetterCollection
        : ObservableCollection<SafeSetter>
    {
        /// <summary>
        /// SafeSetterCollection constructor.
        /// </summary>
        public SafeSetterCollection()
        {
        }

        /// <summary>
        /// Applies the setters against a target object.
        /// </summary>
        /// <param name="target">The target object.</param>
        public void Apply(DependencyObject target)
        {
            if (target == null)
            {
                return;
            }

            foreach (var setter in this)
            {
                setter.Apply(target);
            }
        }
    }

    /// <summary>
    /// A setter that will safely set its value only if the trust level is appropriate.
    /// </summary>
    [ContentProperty("Value")]
    public class SafeSetter

        : Freezable



    {

        protected override Freezable CreateInstanceCore()
        {
            return new SafeSetter();
        }

        /// <summary>
        /// Static boolean indicating whether or not unrestricted UI permission is granted.
        /// </summary>
        public readonly static bool IsSafe;

        internal static bool TestPartialTrust { get; set; }

        static SafeSetter()
        {



            IsSafe = IsPermissionGranted(new UIPermission(PermissionState.Unrestricted));

        }


        [SecuritySafeCritical]
        private static bool IsPermissionGranted(CodeAccessPermission uIPermission)
        {
            try
            {
                uIPermission.Demand();
                return true;
            }
            catch
            {
                return false;
            }
        }


        /// <summary>
        /// Identifies the Value dependency property.
        /// </summary>
        public readonly static DependencyProperty ValueProperty = 
            DependencyProperty.Register(
            "Value",
            typeof(object),
            typeof(SafeSetter),
            new PropertyMetadata(
                null));

        /// <summary>
        /// Sets or gets the value that should be set on the target.
        /// </summary>
        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Identifies the ValueAsXamlString dependency property.
        /// </summary>
        public readonly static DependencyProperty ValueAsXamlStringProperty =
            DependencyProperty.Register(
            "ValueAsXamlString",
            typeof(string),
            typeof(SafeSetter),
            new PropertyMetadata(null));

        /// <summary>
        /// Sets or gets the Xaml string that will be hydrated and then set on the target.
        /// </summary>
        /// <remarks>
        /// This can be used over the value property if contstruction of the value needs to be deferred
        /// until it is sure to be safe.
        /// </remarks>
        public string ValueAsXamlString
        {
            get { return (string)GetValue(ValueAsXamlStringProperty); }
            set { SetValue(ValueAsXamlStringProperty, value); }
        }

        /// <summary>
        /// Identifies the PropertyName dependency property.
        /// </summary>
        public readonly static DependencyProperty PropertyNameProperty =
            DependencyProperty.Register(
            "PropertyName",
            typeof(string),
            typeof(SafeSetter),
            new PropertyMetadata(
                null));

        /// <summary>
        /// Sets or gets the name of the property on the target that will be set.
        /// </summary>
        public string PropertyName
        {
            get { return (string)GetValue(PropertyNameProperty); }
            set { SetValue(PropertyNameProperty, value); }
        }
      
        internal void Apply(DependencyObject target)
        {
            if (PropertyName == null)
            {
                return;
            }
            if (target == null)
            {
                return;
            }

            if (CanFreeze)
            {
                Freeze();
            }


            if (IsSafe && !TestPartialTrust)
            {
                PropertyInfo pi = target.GetType().GetProperty(PropertyName);
                if (pi == null)
                {
                    return;
                }

                pi.SetValue(target, EffectiveValue(), null);
            }
        }

        private object EffectiveValue()
        {
            if (ValueAsXamlString != null)
            {
                return LoadXamlString(ValueAsXamlString);
            }
            else
            {
                return Value;
            }
        }

        private object LoadXamlString(string value)
        {



            using (var sr = new StringReader(value))
            {
                using (var xr = XmlReader.Create(sr))
                {
                    return XamlReader.Load(xr);
                }
            }

        }
    }
}

#region Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved
/* ---------------------------------------------------------------------*
*                           Infragistics, Inc.                          *
*              Copyright (c) 2001-2012 All Rights reserved               *
*                                                                       *
*                                                                       *
* This file and its contents are protected by United States and         *
* International copyright laws.  Unauthorized reproduction and/or       *
* distribution of all or any portion of the code contained herein       *
* is strictly prohibited and will result in severe civil and criminal   *
* penalties.  Any violations of this copyright will be prosecuted       *
* to the fullest extent possible under law.                             *
*                                                                       *
* THE SOURCE CODE CONTAINED HEREIN AND IN RELATED FILES IS PROVIDED     *
* TO THE REGISTERED DEVELOPER FOR THE PURPOSES OF EDUCATION AND         *
* TROUBLESHOOTING. UNDER NO CIRCUMSTANCES MAY ANY PORTION OF THE SOURCE *
* CODE BE DISTRIBUTED, DISCLOSED OR OTHERWISE MADE AVAILABLE TO ANY     *
* THIRD PARTY WITHOUT THE EXPRESS WRITTEN CONSENT OF INFRAGISTICS, INC. *
*                                                                       *
* UNDER NO CIRCUMSTANCES MAY THE SOURCE CODE BE USED IN WHOLE OR IN     *
* PART, AS THE BASIS FOR CREATING A PRODUCT THAT PROVIDES THE SAME, OR  *
* SUBSTANTIALLY THE SAME, FUNCTIONALITY AS ANY INFRAGISTICS PRODUCT.    *
*                                                                       *
* THE REGISTERED DEVELOPER ACKNOWLEDGES THAT THIS SOURCE CODE           *
* CONTAINS VALUABLE AND PROPRIETARY TRADE SECRETS OF INFRAGISTICS,      *
* INC.  THE REGISTERED DEVELOPER AGREES TO EXPEND EVERY EFFORT TO       *
* INSURE ITS CONFIDENTIALITY.                                           *
*                                                                       *
* THE END USER LICENSE AGREEMENT (EULA) ACCOMPANYING THE PRODUCT        *
* PERMITS THE REGISTERED DEVELOPER TO REDISTRIBUTE THE PRODUCT IN       *
* EXECUTABLE FORM ONLY IN SUPPORT OF APPLICATIONS WRITTEN USING         *
* THE PRODUCT.  IT DOES NOT PROVIDE ANY RIGHTS REGARDING THE            *
* SOURCE CODE CONTAINED HEREIN.                                         *
*                                                                       *
* THIS COPYRIGHT NOTICE MAY NOT BE REMOVED FROM THIS FILE.              *
* --------------------------------------------------------------------- *
*/
#endregion Copyright (c) 2001-2012 Infragistics, Inc. All Rights Reserved