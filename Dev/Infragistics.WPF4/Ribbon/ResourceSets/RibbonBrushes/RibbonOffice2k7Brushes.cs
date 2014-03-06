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
	#region RibbonOffice2k7Brushes<T> base class

	/// <summary>
	/// Abstract base class used to supply brush resources for a specific Office Ribbon look.
	/// </summary>
	/// <typeparam name="T">A <see cref="ResourceSetLocator"/> derived class that must have a public parameterless constructor.</typeparam>
	public abstract class RibbonOffice2k7Brushes<T> : ResourceSet where T : ResourceSetLocator, new()
	{
		#region Constants

        // JJD 12/20/07
        // Since these resourceSets never get registered then return an empty string
		//static internal readonly string GroupingName = "RibbonBrushes";
		static internal readonly string GroupingName = "";

		#endregion //Constants

		#region Base class overrides

			#region Grouping

		/// <summary>
		/// Returns the grouping that the resources are applied to (read-only)
		/// </summary>
		/// <remarks>
		/// Examples: RibbonBase, Editors, RibbonBase, WPF etc.
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
				return ResourcesInternal;
			}
		}

			#endregion //Resources

		#endregion //Base class overrides

		#region Static Properties

			#region Private Propeties

				#region Location

		private static ResourceSetLocator g_Location;

		private static ResourceSetLocator Location
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

			#region Public Properties

			#endregion //Public Properties

		#endregion //Static Properties
	}

	#endregion //RibbonOffice2k7Brushes<T> base class

	#region RibbonOffice2k7BlackBrushes

	/// <summary>
	/// Class used to supply brush resources for the Black Office 2007 look for a Ribbons
	/// </summary>
	public class RibbonOffice2k7BlackBrushes : RibbonOffice2k7Brushes<RibbonOffice2k7BlackBrushes.Locator>
	{

		#region Instance static property

		private static RibbonOffice2k7BlackBrushes g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static RibbonOffice2k7BlackBrushes Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new RibbonOffice2k7BlackBrushes();

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
			public override string Theme { get { return "Office2k7BlackBrushes"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return RibbonGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\RibbonBrushes\RibbonOffice2k7Brushes_Black.xaml"; } }
		}
	}

	#endregion //RibbonOffice2k7BlackBrushes

	#region RibbonOffice2k7BlueBrushes

	/// <summary>
	/// Class used to supply brush resources for the Blue Office 2007 look for a Ribbons
	/// </summary>
	public class RibbonOffice2k7BlueBrushes : RibbonOffice2k7Brushes<RibbonOffice2k7BlueBrushes.Locator>
	{

		#region Instance static property

		private static RibbonOffice2k7BlueBrushes g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static RibbonOffice2k7BlueBrushes Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new RibbonOffice2k7BlueBrushes();

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
			public override string Theme { get { return "Office2k7BlueBrushes"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return RibbonGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\RibbonBrushes\RibbonOffice2k7Brushes_Blue.xaml"; } }
		}
	}

	#endregion //RibbonOffice2k7BlueBrushes

	#region RibbonOffice2k7SilverBrushes

	/// <summary>
	/// Class used to supply brush resources for the Silver Office 2007 look for a Ribbons
	/// </summary>
	public class RibbonOffice2k7SilverBrushes : RibbonOffice2k7Brushes<RibbonOffice2k7SilverBrushes.Locator>
	{

		#region Instance static property

		private static RibbonOffice2k7SilverBrushes g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static RibbonOffice2k7SilverBrushes Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new RibbonOffice2k7SilverBrushes();

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
			public override string Theme { get { return "Office2k7SilverBrushes"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return RibbonGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\RibbonBrushes\RibbonOffice2k7Brushes_Silver.xaml"; } }
		}
	}

	#endregion //RibbonOffice2k7SilverBrushes

	#region RibbonHighContrastBrushes

	/// <summary>
	/// Class used to supply brush resources for a system using a dark high constrast theme
	/// </summary>
	public class RibbonHighContrastBrushes : RibbonOffice2k7Brushes<RibbonHighContrastBrushes.Locator>
	{

		#region Instance static property

		// AS 10/16/09 TFS23586
		// Added ThreadStatic so we have 1 instance per thread since the contained
		// resources cannot be shared between threads.
		//
		[ThreadStatic()]
		private static RibbonHighContrastBrushes g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static RibbonHighContrastBrushes Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new RibbonHighContrastBrushes();

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
			public override string Theme { get { return "HighContrastBrushes"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return RibbonGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\RibbonBrushes\RibbonHighContrastBrushes.xaml"; } }
		}
	}

	#endregion //RibbonHighContrastBrushes	

	#region RibbonWashBaseDarkBrushes

	/// <summary>
	/// Class used to supply brush resources for a system using a dark high constrast theme
	/// </summary>
	public class RibbonWashBaseDarkBrushes : RibbonOffice2k7Brushes<RibbonWashBaseDarkBrushes.Locator>
	{

		#region Instance static property

		private static RibbonWashBaseDarkBrushes g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static RibbonWashBaseDarkBrushes Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new RibbonWashBaseDarkBrushes();

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
			public override string Theme { get { return "HighContrastBrushes"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return RibbonGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\RibbonBrushes\RibbonWashBaseDarkBrushes.xaml"; } }
		}
	}

	#endregion //RibbonWashBaseDarkBrushes

	#region RibbonWashBaseLightBrushes

	/// <summary>
	/// Class used to supply brush resources for a system using a dark high constrast theme
	/// </summary>
	public class RibbonWashBaseLightBrushes : RibbonOffice2k7Brushes<RibbonWashBaseLightBrushes.Locator>
	{

		#region Instance static property

		private static RibbonWashBaseLightBrushes g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static RibbonWashBaseLightBrushes Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new RibbonWashBaseLightBrushes();

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
			public override string Theme { get { return "HighContrastBrushes"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return RibbonGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\RibbonBrushes\RibbonWashBaseLightBrushes.xaml"; } }
		}
	}

	#endregion //RibbonWashBaseLightBrushes
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