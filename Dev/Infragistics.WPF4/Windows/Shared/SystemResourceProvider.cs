using System;
using System.Net;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Infragistics;
using Infragistics.Controls;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using Infragistics.Controls.Primitives;
using Infragistics.Collections;
using System.Collections;
using System.Text;

namespace Infragistics.Controls.Primitives
{
	#region SystemResourceId enum

	/// <summary>
	/// An enum that identifies system colors and associated brushes for use by <see cref="SystemResourceProvider"/>
	/// </summary>
	/// <seealso cref="SystemResourceProvider"/>

	[InfragisticsFeature(FeatureName = "XamCalendar", Version = "11.1")]

	public enum SystemResourceId
	{
		/// <summary>
		/// A SolidColorBrush created using the SystemColors.ActiveBorderColor color.
		/// </summary>
		ActiveBorderBrush,
		/// <summary>
		/// The SystemColors.ActiveBorderColor color.
		/// </summary>
		ActiveBorderColor,
		/// <summary>
		/// A SolidColorBrush created using the SystemColors.ActiveCaptionColor color.
		/// </summary>
		ActiveCaptionBrush,
		/// <summary>
		/// The SystemColors.ActiveCaptionColor color.
		/// </summary>
		ActiveCaptionColor,
		/// <summary>
		/// A SolidColorBrush created using the SystemColors.ActiveCaptionTextColor color.
		/// </summary>
		ActiveCaptionTextBrush,
		/// <summary>
		/// The SystemColors.ActiveCaptionTextColor color.
		/// </summary>
		ActiveCaptionTextColor,
		/// <summary>
		/// A SolidColorBrush created using the SystemColors.AppWorkspaceColor color.
		/// </summary>
		AppWorkspaceBrush,
		/// <summary>
		/// The SystemColors.AppWorkspaceColor color.
		/// </summary>
		AppWorkspaceColor,
		/// <summary>
		/// A SolidColorBrush created using the SystemColors.ControlColor color.
		/// </summary>
		ControlBrush,
		/// <summary>
		/// The SystemColors.ControlColor color.
		/// </summary>
		ControlColor,
		/// <summary>
		/// A SolidColorBrush created using the SystemColors.ControlDarkColor color.
		/// </summary>
		ControlDarkBrush,
		/// <summary>
		/// The SystemColors.ControlDarkColor color.
		/// </summary>
		ControlDarkColor,
		/// <summary>
		/// A SolidColorBrush created using the SystemColors.ControlDarkDarkColor color.
		/// </summary>
		ControlDarkDarkBrush,
		/// <summary>
		/// The SystemColors.ControlDarkDarkColor color.
		/// </summary>
		ControlDarkDarkColor,
		/// <summary>
		/// A SolidColorBrush created using the SystemColors.ControlLightColor color.
		/// </summary>
		ControlLightBrush,
		/// <summary>
		/// The SystemColors.ControlLightColor color.
		/// </summary>
		ControlLightColor,
		/// <summary>
		/// A SolidColorBrush created using the SystemColors.ControlLightLightColor color.
		/// </summary>
		ControlLightLightBrush,
		/// <summary>
		/// The SystemColors.ControlLightLightColor color.
		/// </summary>
		ControlLightLightColor,
		/// <summary>
		/// A SolidColorBrush created using the SystemColors.ControlTextColor color.
		/// </summary>
		ControlTextBrush,
		/// <summary>
		/// The SystemColors.ControlTextColor color.
		/// </summary>
		ControlTextColor,
		/// <summary>
		/// A SolidColorBrush created using the SystemColors.DesktopColor color.
		/// </summary>
		DesktopBrush,
		/// <summary>
		/// The SystemColors.DesktopColor color.
		/// </summary>
		DesktopColor,
		/// <summary>
		/// A SolidColorBrush created using the SystemColors.GrayTextColor color.
		/// </summary>
		GrayTextBrush,
		/// <summary>
		/// The SystemColors.GrayTextColor color.
		/// </summary>
		GrayTextColor,
		/// <summary>
		/// A SolidColorBrush created using the SystemColors.HighlightColor color.
		/// </summary>
		HighlightBrush,
		/// <summary>
		/// The SystemColors.HighlightColor color.
		/// </summary>
		HighlightColor,
		/// <summary>
		/// A SolidColorBrush created using the SystemColors.HighlightTextColor color.
		/// </summary>
		HighlightTextBrush,
		/// <summary>
		/// The SystemColors.HighlightTextColor color.
		/// </summary>
		HighlightTextColor,
		/// <summary>
		/// A SolidColorBrush created using the SystemColors.InfoColor color.
		/// </summary>
		InfoBrush,
		/// <summary>
		/// The SystemColors.InfoColor color.
		/// </summary>
		InfoColor,
		/// <summary>
		/// A SolidColorBrush created using the SystemColors.InfoTextColor color.
		/// </summary>
		InfoTextBrush,
		/// <summary>
		/// The SystemColors.InfoTextColor color.
		/// </summary>
		InfoTextColor,
		/// <summary>
		/// A SolidColorBrush created using the SystemColors.InactiveBorderColor color.
		/// </summary>
		InactiveBorderBrush,
		/// <summary>
		/// The SystemColors.InactiveBorderColor color.
		/// </summary>
		InactiveBorderColor,
		/// <summary>
		/// A SolidColorBrush created using the SystemColors.InactiveCaptionColor color.
		/// </summary>
		InactiveCaptionBrush,
		/// <summary>
		/// The SystemColors.InactiveCaptionColor color.
		/// </summary>
		InactiveCaptionColor,
		/// <summary>
		/// A SolidColorBrush created using the SystemColors.InactiveCaptionTextColor color.
		/// </summary>
		InactiveCaptionTextBrush,
		/// <summary>
		/// The SystemColors.InactiveCaptionTextColor color.
		/// </summary>
		InactiveCaptionTextColor,
		/// <summary>
		/// A SolidColorBrush created using the SystemColors.MenuColor color.
		/// </summary>
		MenuBrush,
		/// <summary>
		/// The SystemColors.MenuColor color.
		/// </summary>
		MenuColor,
		/// <summary>
		/// A SolidColorBrush created using the SystemColors.MenuTextColor color.
		/// </summary>
		MenuTextBrush,
		/// <summary>
		/// The SystemColors.MenuTextColor color.
		/// </summary>
		MenuTextColor,
		/// <summary>
		/// A SolidColorBrush created using the SystemColors.ScrollBarColor color.
		/// </summary>
		ScrollBarBrush,
		/// <summary>
		/// The SystemColors.ScrollBarColor color.
		/// </summary>
		ScrollBarColor,
		/// <summary>
		/// A SolidColorBrush created using the SystemColors.WindowColor color.
		/// </summary>
		WindowBrush,
		/// <summary>
		/// The SystemColors.WindowColor color.
		/// </summary>
		WindowColor,
		/// <summary>
		/// A SolidColorBrush created using the SystemColors.WindowFrameColor color.
		/// </summary>
		WindowFrameBrush,
		/// <summary>
		/// The SystemColors.WindowFrameColor color.
		/// </summary>
		WindowFrameColor,
		/// <summary>
		/// A SolidColorBrush created using the SystemColors.WindowTextColor color.
		/// </summary>
		WindowTextBrush,
		/// <summary>
		/// The SystemColors.WindowTextColor color.
		/// </summary>
		WindowTextColor
	}

	#endregion //SystemResourceId enum

	#region SystemResourceProvider class

	/// <summary>
	/// Class that exposes an indexer to retrieve system colors and their associated brushes identified by <see cref="SystemResourceId"/>.
	/// </summary>

	[InfragisticsFeature(FeatureName = "XamCalendar", Version = "11.1")]

	public class SystemResourceProvider : ResourceProvider<SystemResourceId>
	{
		#region Private Members

		[ThreadStatic()]
		private static Dictionary<SystemResourceId, object> _Map;

		private static readonly WeakSet<SystemResourceProvider> _SystemProviders = new WeakSet<SystemResourceProvider>();

		#endregion // Private Members	
        
		#region Constructor
		

		/// <summary>
		/// Creates a new instance of <see cref="SystemResourceProvider"/>
		/// </summary>
		public SystemResourceProvider()
		{
			lock (_SystemProviders)
				_SystemProviders.Add(this);
		}

		#endregion //Constructor	
    
		#region Base class overrides

		#region GetResource

		/// <summary>
		/// Returns a value for a specific id
		/// </summary>
		/// <param name="id">The id of the resource</param>
		/// <returns>The resource identified by the id or null.</returns>
		protected override object GetResource(SystemResourceId id)
		{

			object resource = null;
			if (_Map == null)
				_Map = new Dictionary<SystemResourceId, object>();
			else
				_Map.TryGetValue(id, out resource);

			Type type;

			Color systemColor = ColorFromId(id, out type);

			bool exists = resource != null;

			if (exists)
			{
				// ensure that the color from the resource matches the system color
				if (type == typeof(Brush))
				{
					SolidColorBrush br = resource as SolidColorBrush;

					if (br != null || br.Color == systemColor)
						return resource;
				}
				else if (type == typeof(Color))
				{
					if (resource is Color)
					{
						if (systemColor == (Color)resource)
							return resource;
					}
				}
				else
					return resource;

			}


			if ( type == typeof(Brush))
			{
				Brush br = new SolidColorBrush(systemColor);
	
				br.Freeze();
	
				resource = br;
			}
			else if (type == typeof(Color))
			{
				resource = systemColor;
			}

			if (exists)
			{
				_Map[id] = resource;

				// call VerifyCache to clean up any other brushes whose colors have changed
				VerifyHelper(false);

				// Notify all instances of SystemResourceProvider that te system settings have changed
				NotifyAllProviders();
			}
			else
				_Map.Add(id, resource);

			return resource;
		}

		#endregion //GetResource	
    
		#region InvalidateResources

		/// <summary>
		/// Called to invalidate any cached values
		/// </summary>
		public override void InvalidateResources()
		{
			VerifyCachedResources();
		}

		#endregion //InvalidateResources	

		#region OnSystemColorsChanged

		/// <summary>
		/// Called when the system colors have changed
		/// </summary>
		protected override void OnSystemColorsChanged()
		{
			this.BumpResourceVersion();
			this.RaisePropertyChangedEvent("Item[]");
		}

		#endregion //OnSystemColorsChanged
    
		#endregion //Base class overrides	
    
		#region Properties

		#endregion //Properties

		#region Methods

		#region VerifyCachedResources

		/// <summary>
		/// Checks all cached resources to make sure that they are still valid. 
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note</b>: if any resources are invalid then all instances of <see cref="SystemResourceProvider"/> will bump their <see cref="ResourceProvider.ResourceVersion"/> property and raise a property change notification for their indexer ('Item[]').</para>
		/// </remarks>
		/// <returns>True is all cached resources are still valid or false if any one of them was cleared because it wasn't still valid.</returns>
		public static bool VerifyCachedResources()
		{
			return VerifyHelper(true);
		}

		#endregion //VerifyCachedResources

		#region Private Methods

		#region ColorFromId

		private static Color ColorFromId(SystemResourceId id, out Type type)
		{
			type = typeof(Brush);
			switch (id)
			{
				case SystemResourceId.ActiveBorderBrush:
					return SystemColors.ActiveBorderColor;
				
				case SystemResourceId.ActiveBorderColor:
					type = typeof(Color);
					return SystemColors.ActiveBorderColor;

				case SystemResourceId.ActiveCaptionBrush:
					return SystemColors.ActiveCaptionColor;
				
				case SystemResourceId.ActiveCaptionColor:
					type = typeof(Color);
					return SystemColors.ActiveCaptionColor;

				case SystemResourceId.ActiveCaptionTextBrush:
					return SystemColors.ActiveCaptionTextColor;

				case SystemResourceId.ActiveCaptionTextColor:
					type = typeof(Color);
					return SystemColors.ActiveCaptionTextColor;

				case SystemResourceId.AppWorkspaceBrush:
					return SystemColors.AppWorkspaceColor;

				case SystemResourceId.AppWorkspaceColor:
					type = typeof(Color);
					return SystemColors.AppWorkspaceColor;

				case SystemResourceId.ControlBrush:
					return SystemColors.ControlColor;

				case SystemResourceId.ControlColor:
					type = typeof(Color);
					return SystemColors.ControlColor;

				case SystemResourceId.ControlDarkBrush:
					return SystemColors.ControlDarkColor;

				case SystemResourceId.ControlDarkColor:
					type = typeof(Color);
					return SystemColors.ControlDarkColor;

				case SystemResourceId.ControlDarkDarkBrush:
					return SystemColors.ControlDarkDarkColor;

				case SystemResourceId.ControlDarkDarkColor:
					type = typeof(Color);
					return SystemColors.ControlDarkDarkColor;

				case SystemResourceId.ControlLightBrush:
					return SystemColors.ControlLightColor;

				case SystemResourceId.ControlLightColor:
					type = typeof(Color);
					return SystemColors.ControlLightColor;

				case SystemResourceId.ControlLightLightBrush:
					return SystemColors.ControlLightLightColor;

				case SystemResourceId.ControlLightLightColor:
					type = typeof(Color);
					return SystemColors.ControlLightLightColor;

				case SystemResourceId.ControlTextBrush:
					return SystemColors.ControlTextColor;

				case SystemResourceId.ControlTextColor:
					type = typeof(Color);
					return SystemColors.ControlTextColor;

				case SystemResourceId.DesktopBrush:
					return SystemColors.DesktopColor;

				case SystemResourceId.DesktopColor:
					type = typeof(Color);
					return SystemColors.DesktopColor;

				case SystemResourceId.GrayTextBrush:
					return SystemColors.GrayTextColor;

				case SystemResourceId.GrayTextColor:
					type = typeof(Color);
					return SystemColors.GrayTextColor;

				case SystemResourceId.HighlightBrush:
					return SystemColors.HighlightColor;

				case SystemResourceId.HighlightColor:
					type = typeof(Color);
					return SystemColors.HighlightColor;

				case SystemResourceId.HighlightTextBrush:
					return SystemColors.HighlightTextColor;

				case SystemResourceId.HighlightTextColor:
					type = typeof(Color);
					return SystemColors.HighlightTextColor;

				case SystemResourceId.InactiveBorderBrush:
					return SystemColors.InactiveBorderColor;

				case SystemResourceId.InactiveBorderColor:
					type = typeof(Color);
					return SystemColors.InactiveBorderColor;

				case SystemResourceId.InactiveCaptionBrush:
					return SystemColors.InactiveCaptionColor;

				case SystemResourceId.InactiveCaptionColor:
					type = typeof(Color);
					return SystemColors.InactiveCaptionColor;

				case SystemResourceId.InactiveCaptionTextBrush:
					return SystemColors.InactiveCaptionTextColor;

				case SystemResourceId.InactiveCaptionTextColor:
					type = typeof(Color);
					return SystemColors.InactiveCaptionTextColor;

				case SystemResourceId.InfoBrush:
					return SystemColors.InfoColor;

				case SystemResourceId.InfoColor:
					type = typeof(Color);
					return SystemColors.InfoColor;

				case SystemResourceId.InfoTextBrush:
					return SystemColors.InfoTextColor;

				case SystemResourceId.InfoTextColor:
					type = typeof(Color);
					return SystemColors.InfoTextColor;

				case SystemResourceId.MenuBrush:
					return SystemColors.MenuColor;

				case SystemResourceId.MenuColor:
					type = typeof(Color);
					return SystemColors.MenuColor;

				case SystemResourceId.MenuTextBrush:
					return SystemColors.MenuTextColor;

				case SystemResourceId.MenuTextColor:
					type = typeof(Color);
					return SystemColors.MenuTextColor;

				case SystemResourceId.ScrollBarBrush:
					return SystemColors.ScrollBarColor;

				case SystemResourceId.ScrollBarColor:
					type = typeof(Color);
					return SystemColors.ScrollBarColor;

				case SystemResourceId.WindowBrush:
					return SystemColors.WindowColor;

				case SystemResourceId.WindowColor:
					type = typeof(Color);
					return SystemColors.WindowColor;

				case SystemResourceId.WindowFrameBrush:
					return SystemColors.WindowFrameColor;

				case SystemResourceId.WindowFrameColor:
					type = typeof(Color);
					return SystemColors.WindowFrameColor;

				case SystemResourceId.WindowTextBrush:
					return SystemColors.WindowTextColor;

				case SystemResourceId.WindowTextColor:
					type = typeof(Color);
					return SystemColors.WindowTextColor;

				default:
					Debug.Assert(false, "Unknown SystemResourceId");
					return SystemColors.WindowColor;
			}

		}

		#endregion //ColorFromId	

		#region NotifyAllProviders

		private static void NotifyAllProviders()
		{
			// copy our static instances into a stack list for processsing below
			List<SystemResourceProvider> providers = null;
			lock (_SystemProviders)
				providers = new List<SystemResourceProvider>(_SystemProviders);

			foreach (SystemResourceProvider provider in providers)
				provider.OnSystemColorsChangedInternal();
		}

		#endregion //NotifyAllProviders	

		#region VerifyHelper

		private static bool VerifyHelper(bool notify)
		{
			if (_Map == null)
				return true;

			List<SystemResourceId> idsToRemove = null;

			foreach (KeyValuePair<SystemResourceId, object> entry in _Map)
			{
				Type type;
				Color expectedColor = ColorFromId(entry.Key, out type);
				bool remove = false;

				if (type == typeof(Brush))
				{
					SolidColorBrush br = entry.Value as SolidColorBrush;

					if (br != null)
					{
						if (br.Color != expectedColor)
							remove = true;
					}
					else
						remove = true;
				}
				else if (type == typeof(Color))
				{
					if (entry.Value is Color)
					{
						if ((Color)entry.Value != expectedColor)
							remove = true;
					}
					else
						remove = true;
				}

				if ( remove )
				{
					if (idsToRemove == null)
						idsToRemove = new List<SystemResourceId>();

					idsToRemove.Add(entry.Key);
				}
			}

			if (idsToRemove == null)
				return true;

			int count = idsToRemove.Count;

			for (int i = 0; i < count; i++)
				_Map.Remove(idsToRemove[i]);


			if (notify)
				NotifyAllProviders();

			return false;
		}

		#endregion //VerifyHelper	
        
		#endregion //Private Methods	
    
		#endregion //Methods
	}

	#endregion //SystemResourceProvider class
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