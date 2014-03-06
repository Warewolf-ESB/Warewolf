using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Infragistics;
using System.Windows.Input;





namespace Infragistics.Controls.Primitives
{
    /// <summary>
    /// Static class with attached properties to help with cross platform development with WPF and Silverlight.
    /// </summary>
    public class XamlHelper : DependencyObject
    {
        static XamlHelper()
        {
        }

        private XamlHelper()
        {
        }

        #region CanContentScroll

        // SSP 10/17/11 TFS91522
        // Use the ScrollViewer.CanContentScrollProperty property itself instead of registering a new property.
        // This way one can also get the value of the CanContentScrollProperty as well as be able to set it.
        // Also changed the type from bool? to bool.
        // 


		/// <summary>
		/// Identifies the CanContentScroll attached dependency property
		/// </summary>
		public static readonly DependencyProperty CanContentScrollProperty = ScrollViewer.CanContentScrollProperty.AddOwner( typeof( XamlHelper ) );


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)




        //        /// <summary>
        //        /// Identifies the CanContentScroll attached dependency property
        //        /// </summary>
        //        public static readonly DependencyProperty CanContentScrollProperty = DependencyProperty.RegisterAttached("CanContentScroll",
        //            typeof(bool?), typeof(XamlHelper),
        //            DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnCanContentScrollChanged))
        //            );

        //        private static void OnCanContentScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //        {
        //#if !SILVERLIGHT
        //            d.SetValue(ScrollViewer.CanContentScrollProperty, e.NewValue == null ? DependencyProperty.UnsetValue : e.NewValue);
        //#endif
        //        }

        /// <summary>
        /// Used to change the CanContentScroll value of a ScrollViewer in WPF.
        /// </summary>
        /// <param name="d">The object whose value is to be returned</param>
        /// <seealso cref="CanContentScrollProperty"/>
        /// <seealso cref="SetCanContentScroll"/>
        // SSP 10/17/11 TFS91522
        // Related to the change above. Commented out the attribute as the property's type was changed to non-nullable.
        // 
        //#if SILVERLIGHT
        //        [TypeConverter(typeof(XamlHelper.NullableConverter<bool>))]
        //#endif
        public static bool GetCanContentScroll(DependencyObject d)
        {
            return (bool)d.GetValue(XamlHelper.CanContentScrollProperty);
        }

        /// <summary>
        /// Sets the value of the attached CanContentScroll DependencyProperty.
        /// </summary>
        /// <param name="d">The object whose value is to be modified</param>
        /// <param name="value">The new value</param>
        /// <seealso cref="CanContentScrollProperty"/>
        /// <seealso cref="GetCanContentScroll"/>
        public static void SetCanContentScroll(DependencyObject d, bool value)
        {
            d.SetValue(XamlHelper.CanContentScrollProperty, value);
        }

        #endregion //CanContentScroll

        #region Focusable

        // SSP 10/17/11 TFS91522
        // Use the UIElement.FocusableProperty property itself instead of registering a new property.
        // This way one can also get the value of the FocusableProperty as well as be able to set it.
        // Also changed the type from bool? to bool.
        // 


		/// <summary>
		/// Identifies the Focusable attached dependency property
		/// </summary>
		public static readonly DependencyProperty FocusableProperty = UIElement.FocusableProperty.AddOwner( typeof( XamlHelper ) );


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)


        //        /// <summary>
        //        /// Identifies the Focusable attached dependency property
        //        /// </summary>
        //        public static readonly DependencyProperty FocusableProperty = DependencyProperty.RegisterAttached("Focusable",
        //            typeof(bool?), typeof(XamlHelper),
        //            DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnFocusableChanged))
        //            );

        //        private static void OnFocusableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        //        {
        //#if !SILVERLIGHT
        //            d.SetValue(UIElement.FocusableProperty, e.NewValue == null ? DependencyProperty.UnsetValue : e.NewValue);
        //#endif
        //        }

        /// <summary>
        /// Used to change the Focusable value of a UIElement in WPF.
        /// </summary>
        /// <param name="d">The object whose value is to be returned</param>
        /// <seealso cref="FocusableProperty"/>
        /// <seealso cref="SetFocusable"/>
        // SSP 10/17/11 TFS91522
        // Related to the change above. Commented out the attribute as the property's type was changed to non-nullable.
        // 
        //#if WINDOWS_PHONE
        //        [TypeConverter(typeof(NullableBoolConverter))]
        //#elif SILVERLIGHT
        //        [TypeConverter(typeof(XamlHelper.NullableConverter<bool>))]
        //#endif
        public static bool GetFocusable(DependencyObject d)
        {
            return (bool)d.GetValue(XamlHelper.FocusableProperty);
        }

        /// <summary>
        /// Sets the value of the attached Focusable DependencyProperty.
        /// </summary>
        /// <param name="d">The object whose value is to be modified</param>
        /// <param name="value">The new value</param>
        /// <seealso cref="FocusableProperty"/>
        /// <seealso cref="GetFocusable"/>
        public static void SetFocusable(DependencyObject d, bool value)
        {
            d.SetValue(XamlHelper.FocusableProperty, value);
        }

        #endregion //Focusable

        #region StaysOpen

      


        /// <summary>
        /// Identifies the WPF Popup StaysOpen attached dependency property
        /// </summary>
        public static readonly DependencyProperty StaysOpenProperty = System.Windows.Controls.Primitives.Popup.StaysOpenProperty.AddOwner(typeof(XamlHelper));


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)



        /// <summary>
        /// Used to change the StaysOpen value of a Popup in WPF.
        /// </summary>
        /// <param name="d">The object whose value is to be returned</param>
        public static bool GetStaysOpen(DependencyObject d)
        {
            return (bool)d.GetValue(XamlHelper.StaysOpenProperty);
        }

        /// <summary>
        /// Sets the value of the attached StaysOpen DependencyProperty.
        /// </summary>
        /// <param name="d">The object whose value is to be modified</param>
        /// <param name="value">The new value</param>
        public static void SetStaysOpen(DependencyObject d, bool value)
        {
            d.SetValue(XamlHelper.StaysOpenProperty, value);
        }

        #endregion //StaysOpen

        #region AllowsTransparency


        /// <summary>
        /// Identifies the WPF Popup AllowsTransparency attached dependency property
        /// </summary>
        public static readonly DependencyProperty AllowsTransparencyProperty = System.Windows.Controls.Primitives.Popup.AllowsTransparencyProperty.AddOwner(typeof(XamlHelper));


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)



        /// <summary>
        /// Used to change the AllowsTransparency value of a popup in WPF.
        /// </summary>
        /// <param name="d">The object whose value is to be returned</param>
        public static bool GetAllowsTransparency(DependencyObject d)
        {
            return (bool)d.GetValue(XamlHelper.AllowsTransparencyProperty);
        }

        /// <summary>
        /// Sets the value of the attached AllowsTransparency DependencyProperty.
        /// </summary>
        /// <param name="d">The object whose value is to be modified</param>
        /// <param name="value">The new value</param>
        public static void SetAllowsTransparency(DependencyObject d, bool value)
        {
            d.SetValue(XamlHelper.AllowsTransparencyProperty, value);
        }

       #endregion //AllowsTransparency

        #region IsExcludedFromWashProperty


        /// <summary>
        /// Identifies the ResourceWasher IsExcludedFromWash attached dependency property
        /// </summary>
        public static readonly DependencyProperty IsExcludedFromWashProperty = Infragistics.Windows.Themes.ResourceWasher.IsExcludedFromWashProperty.AddOwner(typeof(XamlHelper));


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


        /// <summary>
        /// Sets the value of the attached IsExcludedFromWash property.
        /// </summary>
        /// <param name="element">The object whose value is to be modified.</param>
        /// <param name="value">The new value</param>
        public static void SetIsExcludedFromWash(DependencyObject element, bool value)
        {

            Infragistics.Windows.Themes.ResourceWasher.SetIsExcludedFromWash(element, value);



        }


        /// <summary>
        /// Gets the value of the IsExcludedFromWash attached property.
        /// </summary>
        /// <param name="element">The object whose value is to be returned.</param>
        /// <returns></returns>
        public static Boolean GetIsExcludedFromWash(DependencyObject element)
        {

            return Infragistics.Windows.Themes.ResourceWasher.GetIsExcludedFromWash(element);



        }

        #endregion // IsExcludedFromWashProperty

        #region WashGroupProperty


        /// <summary>
        /// Identifies the ResourceWasher WashGroup attached dependency property
        /// </summary>
        public static readonly DependencyProperty WashGroupProperty = Infragistics.Windows.Themes.ResourceWasher.WashGroupProperty.AddOwner(typeof(XamlHelper));


#region Infragistics Source Cleanup (Region)







#endregion // Infragistics Source Cleanup (Region)


        /// <summary>
        /// Sets the value of the attached WashGroup property.
        /// </summary>
        /// <param name="element">The object whose value is to be modified.</param>
        /// <param name="value">The new value</param>
        public static void SetWashGroup(DependencyObject element, string value)
        {

            Infragistics.Windows.Themes.ResourceWasher.SetWashGroup(element, value);



        }


        /// <summary>
        /// Gets the value of the WashGroup attached property.
        /// </summary>
        /// <param name="element">The object whose value is to be returned.</param>
        /// <returns></returns>
        public static string GetWashGroup(DependencyObject element)
        {

            return Infragistics.Windows.Themes.ResourceWasher.GetWashGroup(element);



        }

        #endregion // WashGroupProperty

        #region SnapsToDevicePixels

        /// <summary>
        /// Identifies the SnapsToDevicePixels attached dependency property
        /// </summary>

		public static readonly DependencyProperty SnapsToDevicePixelsProperty = FrameworkElement.SnapsToDevicePixelsProperty.AddOwner(typeof(XamlHelper));


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)


        /// <summary>
        /// Used to change the SnapsToDevicePixels value of a UIElement in WPF.
        /// </summary>
        /// <param name="d">The object whose value is to be returned</param>
        /// <seealso cref="SnapsToDevicePixelsProperty"/>
        /// <seealso cref="SetSnapsToDevicePixels"/>
        public static bool GetSnapsToDevicePixels(DependencyObject d)
        {
            return (bool)d.GetValue(XamlHelper.SnapsToDevicePixelsProperty);
        }

        /// <summary>
        /// Sets the value of the attached SnapsToDevicePixels DependencyProperty.
        /// </summary>
        /// <param name="d">The object whose value is to be modified</param>
        /// <param name="value">The new value</param>
        /// <seealso cref="SnapsToDevicePixelsProperty"/>
        /// <seealso cref="GetSnapsToDevicePixels"/>
        public static void SetSnapsToDevicePixels(DependencyObject d, bool value)
        {
            d.SetValue(XamlHelper.SnapsToDevicePixelsProperty, value);
        }

        #endregion //SnapsToDevicePixels

		// AS 8/7/12 TFS116600
		#region TabNavigation


		/// <summary>
		/// Identifies the TabNavigation attached dependency property
		/// </summary>
		/// <seealso cref="GetTabNavigation"/>
		/// <seealso cref="SetTabNavigation"/>
		public static readonly DependencyProperty TabNavigationProperty = KeyboardNavigation.TabNavigationProperty.AddOwner( typeof( XamlHelper ) );


#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)


		/// <summary>
		/// Gets the value of the attached TabNavigation DependencyProperty.
		/// </summary>
		/// <param name="d">The object whose value is to be returned</param>
		/// <seealso cref="TabNavigationProperty"/>
		/// <seealso cref="SetTabNavigation"/>
		public static KeyboardNavigationMode GetTabNavigation(DependencyObject d)
		{
			return (KeyboardNavigationMode)d.GetValue( XamlHelper.TabNavigationProperty );
		}

		/// <summary>
		/// Sets the value of the attached TabNavigation DependencyProperty.
		/// </summary>
		/// <param name="d">The object whose value is to be modified</param>
		/// <param name="value">The new value</param>
		/// <seealso cref="TabNavigationProperty"/>
		/// <seealso cref="GetTabNavigation"/>
		public static void SetTabNavigation(DependencyObject d, KeyboardNavigationMode value)
		{
			d.SetValue( XamlHelper.TabNavigationProperty, value );
		}

		#endregion //TabNavigation

        
        #region NullableConverter<T> class
        // this is needed for silverlight or else you get xamlparseexceptions when the property is parsed from string


#region Infragistics Source Cleanup (Region)








































































































#endregion // Infragistics Source Cleanup (Region)

        #endregion //NullableConverter<T>  class

        #region DateTimeTypeConverter


#region Infragistics Source Cleanup (Region)





















































































#endregion // Infragistics Source Cleanup (Region)

        #endregion //DateTimeTypeConverter

        #region Int32TypeConverter


#region Infragistics Source Cleanup (Region)





















































































#endregion // Infragistics Source Cleanup (Region)

        #endregion //Int32TypeConverter

        #region SingleTypeConverter


#region Infragistics Source Cleanup (Region)





















































































#endregion // Infragistics Source Cleanup (Region)

        #endregion //SingleTypeConverter

        #region UpdateBindingOnTextPropertyChanged

        /// <summary>
        /// Identifies the UpdateBindingOnTextPropertyChanged attached dependency property which can be set on a TextBox
        /// to control whether changes to the TextBox's Text property forces an immediate update of the binding Target the same way
        /// that an UpdateSourceTrigger = PropertyChanged works in WPF.
        /// </summary>
        /// <seealso cref="GetUpdateBindingOnTextPropertyChanged"/>
        /// <seealso cref="SetUpdateBindingOnTextPropertyChanged"/>
        public static readonly DependencyProperty UpdateBindingOnTextPropertyChangedProperty = DependencyPropertyUtilities.RegisterAttached("UpdateBindingOnTextPropertyChanged",
            typeof(bool), typeof(XamlHelper),
                DependencyPropertyUtilities.CreateMetadata(false, new PropertyChangedCallback(OnUpdateBindingOnTextPropertyChanged))
            );

        private static void OnUpdateBindingOnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox)
            {
                // Use a helper class instance created on the stack to list for the textChange events so that we do not root anything.
                XamlHelper.TextBoxTextChangedListener x = new XamlHelper.TextBoxTextChangedListener((TextBox)d);
            }
        }

        /// <summary>
        /// Gets the value of the attached UpdateBindingOnTextPropertyChanged DependencyProperty.
        /// </summary>
        /// <param name="d">The object whose value is to be returned</param>
        /// <seealso cref="UpdateBindingOnTextPropertyChangedProperty"/>
        /// <seealso cref="SetUpdateBindingOnTextPropertyChanged"/>
        public static bool GetUpdateBindingOnTextPropertyChanged(DependencyObject d)
        {
            return (bool)d.GetValue(XamlHelper.UpdateBindingOnTextPropertyChangedProperty);
        }

        /// <summary>
        /// Sets the value of the attached UpdateBindingOnTextPropertyChanged DependencyProperty.
        /// </summary>
        /// <param name="d">The object whose value is to be modified</param>
        /// <param name="value">The new value</param>
        /// <seealso cref="UpdateBindingOnTextPropertyChangedProperty"/>
        /// <seealso cref="GetUpdateBindingOnTextPropertyChanged"/>
        public static void SetUpdateBindingOnTextPropertyChanged(DependencyObject d, bool value)
        {
            d.SetValue(XamlHelper.UpdateBindingOnTextPropertyChangedProperty, value);
        }

        #endregion //UpdateBindingOnTextPropertyChanged

        #region Nested private Class TextBoxTextChangedListener
        private class TextBoxTextChangedListener
        {
            internal TextBoxTextChangedListener(TextBox textBox)
            {
                textBox.TextChanged += new TextChangedEventHandler(TextBoxTextChanged);
            }

            static void TextBoxTextChanged(object sender, TextChangedEventArgs e)
            {
                if (sender is DependencyObject)
                {






				BindingExpression bindingExpression = BindingOperations.GetBindingExpression(sender as DependencyObject, TextBox.TextProperty);
				if (bindingExpression != null) 
				{
					if (bindingExpression.ParentBinding.Mode == System.Windows.Data.BindingMode.Default ||
						bindingExpression.ParentBinding.Mode == System.Windows.Data.BindingMode.TwoWay	||
						bindingExpression.ParentBinding.Mode	== System.Windows.Data.BindingMode.OneWayToSource)
						bindingExpression.UpdateSource();
				}

                }
            }
        }
        #endregion //Nested private Class TextBoxTextChangedListener


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