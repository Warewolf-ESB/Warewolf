using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Markup;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Infragistics.Windows;

namespace Infragistics.Windows.Themes
{
	#region SystemResourceSet<T> base class

	/// <summary>
	/// Abstract base class used to supply style resources for a specific look for primitive elements that are shared.
	/// </summary>
	/// <typeparam name="T">A <see cref="ResourceSetLocator"/> derived class that must have a public parameterless constructor.</typeparam>
	public abstract class SystemResourceSet<T> : ResourceSet where T : ResourceSetLocator, new()
	{
		#region Constants

		// AS 11/6/07 ThemeGroupingName
		//static internal readonly string GroupingName = "System";
		internal const string GroupingName = "System";

		#endregion //Constants

		#region Base class overrides

		#region Grouping

		/// <summary>
		/// Returns the grouping that the resources are applied to (read-only)
		/// </summary>
		/// <remarks>
		/// Examples: DataPresenter, Editors, System, WPF etc.
		/// </remarks>
		public override string Grouping
		{
			get { return Location.Grouping; }
		}

		#endregion //Grouping

		#region Theme

		/// <summary>
		/// Returns the name of the look (read-only)
		/// </summary>
		public override string Theme
		{
			get
			{
				return Location.Theme;
			}
		}

		#endregion //Theme

		#region Resources

		/// <summary>
		/// Returns the ResourceDictionary containing the associated styles (read-only)
		/// </summary>
		public override ResourceDictionary Resources
		{
			get
			{
				//return ResourcesInternal;
				ResourceDictionary rd = ResourcesInternal;

				// JJD 7/23/07 - ResourceWasher support
				// Call VerifyResources after the initial load so that we can delay the hydrating
				// of the resources by a ResourceWasher until this theme is actually used
				this.VerifyResources();

				return rd;
			}
		}

		#endregion //Resources

		#endregion //Base class overrides

		#region Static Properties

		#region Private Properties

		#region Location

		private static ResourceSetLocator g_Location;

		// AS 5/7/08
		/// <summary>
		/// Returns the <see cref="ResourceSetLocator"/> that describes the theme information for the resource set.
		/// </summary>
		//private static ResourceSetLocator Location
		public static ResourceSetLocator Location
		{
			get
			{
				if (g_Location == null)
					g_Location = new T();

				return g_Location;
			}
		}

		#endregion //Location

		#region ResourcesInternal

		private static ResourceDictionary g_ResourcesInternal;

		private static ResourceDictionary ResourcesInternal
		{
			get
			{
				if (g_ResourcesInternal == null)
				{
					g_ResourcesInternal = Utilities.CreateResourceSetDictionary(Location.Assembly, Location.ResourcePath);
				}

				return g_ResourcesInternal;
			}
		}

		#endregion //ResourcesInternal

		#endregion //Private Propeties

		#endregion //Static Properties
	}

	#endregion //SystemResourceSet<T> base class	

	#region SystemGeneric

	/// <summary>
	/// Class used to supply style resources for the Generic look for primitive elements that are shared
	/// </summary>
	public class SystemGeneric : SystemResourceSet<SystemGeneric.Locator>
	{

		#region Instance static property

		private static SystemGeneric g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static SystemGeneric Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new SystemGeneric();

				return g_Instance;
			}
		}

		#endregion //Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return ThemeManager.ThemeNameGeneric; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return SystemGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\System\SystemGeneric_Express.xaml;ResourceSets\System\SystemGeneric.xaml"; } }

		}
	}

	#endregion //SystemGeneric	
    	
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