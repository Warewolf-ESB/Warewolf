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
using System.Windows.Media.Animation;

namespace Infragistics.Controls.Layouts
{
    internal static class TileUtilities
    {

        #region AreClose

        internal static bool AreClose(double value1, double value2)
        {
            if (value1 == value2)
                return true;

            return Math.Abs(value1 - value2) < .0000000001;
        }

        #endregion //AreClose	
    
        #region BindPathProperty

        internal static bool BindPathProperty(DependencyObject pathSource, object item, DependencyObject target, DependencyProperty dpPath, DependencyProperty dpTarget)
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

                BindingOperations.SetBinding(target, dpTarget, binding);

                return true;
            }

            return false;
        }

        #endregion //BindPathProperty	

		#region CloneAnimation


#region Infragistics Source Cleanup (Region)




















































































#endregion // Infragistics Source Cleanup (Region)

		#endregion //CloneAnimation	
 
		#region CreateBinding

		internal static Binding CreateBinding(DependencyProperty dp, BindingMode mode, object source, Type owningType, string dpPropName)
		{
			Binding binding = PresentationUtilities.CreateBinding(
					new BindingPart
					{

						PathParameter = dp



					}
				);

			binding.Mode = mode;
			binding.Source = source;

			return binding;
		}

		#endregion //CreateBinding	

		#region DoesElementContainPoint



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal static bool DoesElementContainPoint(FrameworkElement elem, Point p)
		{
			return p.X >= 0 && p.Y >= 0 && p.X < elem.ActualWidth && p.Y < elem.ActualHeight;
		}

		#endregion // DoesElementContainPoint

		#region EnsureInElementBounds



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		internal static Point EnsureInElementBounds(Point point, FrameworkElement elem)
		{
			point.X = TileUtilities.EnsureInRange(point.X, 0, elem.ActualWidth);
			point.Y = TileUtilities.EnsureInRange(point.Y, 0, elem.ActualHeight);

			return point;
		}

		#endregion // EnsureInElementBounds

		#region EnsureInRange

		internal static double EnsureInRange(double val, double min, double max)
		{
			return Math.Min(Math.Max(val, min), max);
		}

		#endregion // EnsureInRange

		#region GetIsExpandedWhenMinimizedHelper

		internal static bool GetIsExpandedWhenMinimizedHelper(XamTileManager tm, XamTile tile, object item, bool? isExplicitSetting)
		{
			bool defaultValue = false;

			if (tm != null)
			{
				// If we are in the middle of changing the tile's state then return the explicit value if it has been set
				if (isExplicitSetting.HasValue
					&& tm._settingStateOfTile != null)
				{
					if ( tile == null || tile == tm._settingStateOfTile)
						return isExplicitSetting.Value;
				}

				defaultValue = tm.IsExpandedWhenMinimizedDefault;

				if (tm.MaximizedModeSettingsSafe.MinimizedTileExpansionMode == MinimizedTileExpansionMode.AllowOne)
				{
					XamTile currentMinimizedTile = tm.CurrentMinimizedExpandedTile;

					if (tile == null &&
						item != null)
						tile = tm.TileFromItem(item);

					if (currentMinimizedTile == null ||
						currentMinimizedTile != tile)
						return false;
				}
			}


			if (isExplicitSetting.HasValue)
				return isExplicitSetting.Value;

			return defaultValue;
		}

		#endregion //GetIsExpandedWhenMinimizedHelper	
    
		#region GetString

		internal static string GetString(string name)
		{
#pragma warning disable 436
			return SR.GetString(name);
#pragma warning restore 436
		}

		internal static string GetString(string name, params object[] args)
		{
#pragma warning disable 436
			return SR.GetString(name, args);
#pragma warning restore 436
		}

		#endregion //GetString	
		
		#region IsContainedBy

		internal static bool IsContainedBy(Rect containerRect, Rect rectToTest, bool checkWidth)
		{
			if (checkWidth)
			{
				return (containerRect.Left <= rectToTest.Left &&
						containerRect.Right >= rectToTest.Right);
			}
			else
			{
				return (containerRect.Top <= rectToTest.Top &&
						containerRect.Bottom >= rectToTest.Bottom);
			}
		}

		#endregion //IsContainedBy	
		
		#region IntersectsWith

		internal static bool IntersectsWith(Rect r1, Rect r2)
		{

			return r1.IntersectsWith(r2);






		}

		#endregion //IntersectsWith

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
		
		// JJD 02/27/12 - Added
		#region RoundToIntegerValue

		internal static double RoundToIntegerValue(double value)
		{
			return CoreUtilities.RoundToIntegralValue(value);
		}

		#endregion //RoundToIntegerValue

		#region SystemDoubleClickTime

		/// <summary>
		/// Returns the maximum number of milliseconds allowed between mouse button downs to generate a double click message.
		/// </summary>
		public static int SystemDoubleClickTime
		{
			get 
			{ 

				return Infragistics.Windows.Helpers.NativeWindowMethods.DoubleClickTime; 



			}
		}

		#endregion //SystemDoubleClickTime

		#region SystemDoubleClickSize

		/// <summary>
		/// Returns the maximum distance the cursor is allowed to travel between mouse button downs to generate a double click message.
		/// </summary>
		public static Size SystemDoubleClickSize
		{
			get 
			{ 

				return Infragistics.Windows.Helpers.NativeWindowMethods.DoubleClickSize; 



			}
		}

		#endregion //SystemDoubleClickSize

		#region SystemDragSize

		/// <summary>
		/// Returns the maximum distance the cursor is allowed to travel while the mouse button is down before initiating a drag operation.
		/// </summary>
		public static Size SystemDragSize
		{
			get
			{

				return Infragistics.Windows.Helpers.NativeWindowMethods.DragSize;



			}
		}

		#endregion //SystemDrageSize	

        #region MethodDelegate

        // Optimization - only have 1 parameter less void delegate class defined.
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