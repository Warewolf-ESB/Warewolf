using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Xml;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Security.Permissions;
using System.Security;

namespace Infragistics.Windows.Tiles
{
    internal static class TileUtilities
    {
        [ThreadStatic]
        private static bool? _hasUnmanagedCode;
        [ThreadStatic]
        private static bool? _popupAllowsTransparency;

        #region AreClose

        internal static bool AreClose(double value1, double value2)
        {
            if (value1 == value2)
                return true;

            return Math.Abs(value1 - value2) < .0000000001;
        }

        #endregion //AreClose	
    
        #region BindPathProperty

        internal static bool BindPathProperty(DependencyObject pathSource, object item, DependencyObject target, DependencyProperty dpPath, DependencyProperty dpTarget, string stringFormat)
        {
            string strValue = pathSource.GetValue(dpPath) as string;

            if (strValue != null && strValue.Length > 0)
            {
                Binding binding = new Binding();

                binding.Source = item;

                if (TileUtilities.IsXmlNodeOptimized(item))
                {
                    binding.XPath = strValue;
                }
                else
                {
                    binding.Path = new PropertyPath(strValue, new object[0]);
                }

                if (stringFormat != null && XamTilesControl.s_BindingStringFormatInfo != null)
                    XamTilesControl.s_BindingStringFormatInfo.SetValue(binding, stringFormat, null);

                BindingOperations.SetBinding(target, dpTarget, binding);

                return true;
            }

            return false;
        }

        #endregion //BindPathProperty	

        #region CheckUnmanagedCodePermission
        internal static bool CheckUnmanagedCodePermission()
        {
            try
            {
                SecurityPermission perm = new SecurityPermission(SecurityPermissionFlag.UnmanagedCode);
                perm.Demand();
                return true;
            }
            catch (SecurityException)
            {
                return false;
            }
        }
        #endregion //CheckUnmanagedCodePermission
        #region HasUnmanagedCodeRights
        internal static bool HasUnmanagedCodeRights
        {
            get
            {
                if (_hasUnmanagedCode == null)
                    _hasUnmanagedCode = CheckUnmanagedCodePermission();

                return _hasUnmanagedCode.Value;
            }
        }
        #endregion //HasUnmanagedCodeRights
        
        #region IsPopupInChildWindow
        internal static bool IsPopupInChildWindow
        {
            get
            {
                if (_popupAllowsTransparency == null)
                {
                    System.Windows.Controls.Primitives.Popup p = new System.Windows.Controls.Primitives.Popup();
                    p.AllowsTransparency = true;
                    p.CoerceValue(System.Windows.Controls.Primitives.Popup.AllowsTransparencyProperty);
                    _popupAllowsTransparency = p.AllowsTransparency;
                }

                return !_popupAllowsTransparency.Value;
            }
        }
        #endregion //IsPopupInChildWindow

        #region IsXmlNodeOptimized

        internal static bool IsXmlNodeOptimized(object item)
        {
            if (item != null)
            {
                // first check to see if the type is in the System.Xml namespace.
                // If not we can return false. This will prevent us from calling 
                // IsXmlModeInternal below which would otherwise possibly force the
                // unnecessary loading of the xml assembly
                if (s_isXmlAssemblyLoaded == false &&
                    !item.GetType().FullName.StartsWith("System.Xml", StringComparison.Ordinal))
                    return false;

                return IsXmlNodeInternal(item);
            }

            return false;
        }

        private static bool s_isXmlAssemblyLoaded = false;

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool IsXmlNodeInternal(object item)
        {
            s_isXmlAssemblyLoaded = true;

            return item is XmlNode;
        }

        #endregion //IsXmlNodeOptimized	
		
        #region ThrowIfXXX
		/// <summary>
		/// Helper method to throw an exception if the specified enum is not valid for the enum type.
		/// </summary>
		/// <param name="parameter">The enum to evaluate. Note, this should not be a flagged enum.</param>
		/// <param name="parameterName">The name of the parameter. This is used in the exception if the parameter is not a valid enum member.</param>
		internal static void ThrowIfInvalidEnum(Enum parameter, string parameterName)
		{
			Type enumType = parameter.GetType();
			Debug.Assert(enumType.GetCustomAttributes(typeof(FlagsAttribute), false).Length == 0, "This should not be used with flagged enums");

			if (false == Enum.IsDefined(enumType, parameter))
			{
				throw new InvalidEnumArgumentException(parameterName, Convert.ToInt32(parameter), enumType);
			}
		}

		/// <summary>
		/// Helper method to throw an exception if the specified parameter is null.
		/// </summary>
		/// <param name="parameter">The parameter to evaluate</param>
		/// <param name="parameterName">The name of the parameter. This is used in the exception if the parameter is null</param>
		internal static void ThrowIfNull(object parameter, string parameterName)
		{
			if (null == parameter)
				throw new ArgumentNullException(parameterName);
		}
		/// <summary>
		/// Helper method to throw an exception if the specified parameter is null.
		/// </summary>
		/// <param name="parameter">The parameter to evaluate</param>
		/// <param name="parameterName">The name of the parameter. This is used in the exception if the parameter is null</param>
		internal static void ThrowIfNullOrEmpty(string parameter, string parameterName)
		{
			if (string.IsNullOrEmpty(parameter))
				throw new ArgumentNullException(parameterName);
		}
		#endregion //ThrowIfXXX

        #region MethodDelegate

        // Optimization - only have 1 parameterless void delegate class defined.
        //
        internal delegate void MethodDelegate();

        #endregion //MethodDelegate

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