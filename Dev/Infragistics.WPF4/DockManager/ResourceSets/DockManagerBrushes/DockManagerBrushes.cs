using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Themes;
using System.Windows;
using System.Reflection;
using System.ComponentModel;
using Infragistics.Windows.DockManager;

namespace Infragistics.Windows.Themes
{
	#region DockManagerBrushes<T>
	/// <summary>
	/// Abstract base class used to supply brush resources for the <see cref="XamDockManager"/>.
	/// </summary>
	/// <typeparam name="T">A <see cref="ResourceSetLocator"/> derived class that must have a public parameterless constructor.</typeparam>
	public abstract class DockManagerBrushes<T> : ResourceSet
		where T : ResourceSetLocator, new()
	{
		#region Member Variables

		private static readonly ResourceSetLocator g_Location;
		private static readonly ResourceDictionary g_ResourcesInternal;

		#endregion //Member Variables

		#region Constructor
		static DockManagerBrushes()
		{
			g_Location = new T();
			g_ResourcesInternal = Utilities.CreateResourceSetDictionary(g_Location.Assembly, g_Location.ResourcePath);
		}

		/// <summary>
		/// Initializes a new <see cref="DockManagerBrushes&lt;T&gt;"/>
		/// </summary>
		protected DockManagerBrushes()
		{
		}
		#endregion //Constructor

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

		#region Private Properties

		#region Location

		private static ResourceSetLocator Location
		{
			get { return g_Location; }
		}

		#endregion //Location

		#region ResourcesInternal

		private static ResourceDictionary ResourcesInternal
		{
			get { return g_ResourcesInternal; }
		}

		#endregion //ResourcesInternal

		#endregion //Private Properties

		#region Public Properties

		#endregion //Public Properties

		#endregion //Static Properties
	} 
	#endregion //DockManagerBrushes<T>

	#region DockManagerGenericBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamDockManager"/>
	/// </summary>
	public class DockManagerGenericBrushes : DockManagerBrushes<DockManagerGenericBrushes.Locator>
	{
		#region Member Variables

		private static DockManagerGenericBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static DockManagerGenericBrushes()
		{
			g_Instance = new DockManagerGenericBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="DockManagerGenericBrushes"/>
		/// </summary>
		public DockManagerGenericBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DockManagerGenericBrushes Instance
		{
			get { return g_Instance; }
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
			public override string Theme { get { return "Generic"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DockManagerGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DockManagerBrushes\DockManagerGeneric_Brushes.xaml"; } }
		}
	}

	#endregion //DockManagerGenericBrushes

	#region DockManagerLunaNormalBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamDockManager"/>
	/// </summary>
	public class DockManagerLunaNormalBrushes : DockManagerBrushes<DockManagerLunaNormalBrushes.Locator>
	{
		#region Member Variables

		private static DockManagerLunaNormalBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static DockManagerLunaNormalBrushes()
		{
			g_Instance = new DockManagerLunaNormalBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="DockManagerLunaNormalBrushes"/>
		/// </summary>
		public DockManagerLunaNormalBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DockManagerLunaNormalBrushes Instance
		{
			get { return g_Instance; }
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
			public override string Theme { get { return "LunaNormal"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DockManagerGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DockManagerBrushes\DockManagerLunaNormal_Brushes.xaml"; } }
		}
	}

	#endregion //DockManagerLunaNormalBrushes

	#region DockManagerLunaOliveBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamDockManager"/>
	/// </summary>
	public class DockManagerLunaOliveBrushes : DockManagerBrushes<DockManagerLunaOliveBrushes.Locator>
	{
		#region Member Variables

		private static DockManagerLunaOliveBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static DockManagerLunaOliveBrushes()
		{
			g_Instance = new DockManagerLunaOliveBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="DockManagerLunaOliveBrushes"/>
		/// </summary>
		public DockManagerLunaOliveBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DockManagerLunaOliveBrushes Instance
		{
			get { return g_Instance; }
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
			public override string Theme { get { return "LunaOlive"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DockManagerGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DockManagerBrushes\DockManagerLunaOlive_Brushes.xaml"; } }
		}
	}

	#endregion //DockManagerLunaOliveBrushes

	#region DockManagerLunaSilverBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamDockManager"/>
	/// </summary>
	public class DockManagerLunaSilverBrushes : DockManagerBrushes<DockManagerLunaSilverBrushes.Locator>
	{
		#region Member Variables

		private static DockManagerLunaSilverBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static DockManagerLunaSilverBrushes()
		{
			g_Instance = new DockManagerLunaSilverBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="DockManagerLunaSilverBrushes"/>
		/// </summary>
		public DockManagerLunaSilverBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DockManagerLunaSilverBrushes Instance
		{
			get { return g_Instance; }
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
			public override string Theme { get { return "LunaSilver"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DockManagerGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DockManagerBrushes\DockManagerLunaSilver_Brushes.xaml"; } }
		}
	}

	#endregion //DockManagerLunaSilverBrushes

	#region DockManagerAeroBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamDockManager"/>
	/// </summary>
	public class DockManagerAeroBrushes : DockManagerBrushes<DockManagerAeroBrushes.Locator>
	{
		#region Member Variables

		private static DockManagerAeroBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static DockManagerAeroBrushes()
		{
			g_Instance = new DockManagerAeroBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="DockManagerAeroBrushes"/>
		/// </summary>
		public DockManagerAeroBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DockManagerAeroBrushes Instance
		{
			get { return g_Instance; }
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
			public override string Theme { get { return "Aero"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DockManagerGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DockManagerBrushes\DockManagerAero_Brushes.xaml"; } }
		}
	}

	#endregion //DockManagerAeroBrushes

	#region DockManagerOffice2k7BlackBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamDockManager"/>
	/// </summary>
	public class DockManagerOffice2k7BlackBrushes : DockManagerBrushes<DockManagerOffice2k7BlackBrushes.Locator>
	{
		#region Member Variables

		private static DockManagerOffice2k7BlackBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static DockManagerOffice2k7BlackBrushes()
		{
			g_Instance = new DockManagerOffice2k7BlackBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="DockManagerOffice2k7BlackBrushes"/>
		/// </summary>
		public DockManagerOffice2k7BlackBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DockManagerOffice2k7BlackBrushes Instance
		{
			get { return g_Instance; }
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
			public override string Theme { get { return "Office2k7Black"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DockManagerGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DockManagerBrushes\DockManagerOffice2k7Black_Brushes.xaml"; } }
		}
	}

	#endregion //DockManagerOffice2k7BlackBrushes

	#region DockManagerOffice2k7BlueBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamDockManager"/>
	/// </summary>
	public class DockManagerOffice2k7BlueBrushes : DockManagerBrushes<DockManagerOffice2k7BlueBrushes.Locator>
	{
		#region Member Variables

		private static DockManagerOffice2k7BlueBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static DockManagerOffice2k7BlueBrushes()
		{
			g_Instance = new DockManagerOffice2k7BlueBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="DockManagerOffice2k7BlueBrushes"/>
		/// </summary>
		public DockManagerOffice2k7BlueBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DockManagerOffice2k7BlueBrushes Instance
		{
			get { return g_Instance; }
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
			public override string Theme { get { return "Office2k7Blue"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DockManagerGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DockManagerBrushes\DockManagerOffice2k7Blue_Brushes.xaml"; } }
		}
	}

	#endregion //DockManagerOffice2k7BlueBrushes

	#region DockManagerOffice2k7SilverBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamDockManager"/>
	/// </summary>
	public class DockManagerOffice2k7SilverBrushes : DockManagerBrushes<DockManagerOffice2k7SilverBrushes.Locator>
	{
		#region Member Variables

		private static DockManagerOffice2k7SilverBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static DockManagerOffice2k7SilverBrushes()
		{
			g_Instance = new DockManagerOffice2k7SilverBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="DockManagerOffice2k7SilverBrushes"/>
		/// </summary>
		public DockManagerOffice2k7SilverBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DockManagerOffice2k7SilverBrushes Instance
		{
			get { return g_Instance; }
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
			public override string Theme { get { return "Office2k7Silver"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DockManagerGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DockManagerBrushes\DockManagerOffice2k7Silver_Brushes.xaml"; } }
		}
	}

	#endregion //DockManagerOffice2k7SilverBrushes

	#region DockManagerRoyaleBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamDockManager"/>
	/// </summary>
	public class DockManagerRoyaleBrushes : DockManagerBrushes<DockManagerRoyaleBrushes.Locator>
	{
		#region Member Variables

		private static DockManagerRoyaleBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static DockManagerRoyaleBrushes()
		{
			g_Instance = new DockManagerRoyaleBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="DockManagerRoyaleBrushes"/>
		/// </summary>
		public DockManagerRoyaleBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DockManagerRoyaleBrushes Instance
		{
			get { return g_Instance; }
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
			public override string Theme { get { return "Royale"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DockManagerGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DockManagerBrushes\DockManagerRoyale_Brushes.xaml"; } }
		}
	}

	#endregion DockManagerRoyaleBrushes

	#region DockManagerWashBaseDarkBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamDockManager"/>
	/// </summary>
	public class DockManagerWashBaseDarkBrushes : DockManagerBrushes<DockManagerWashBaseDarkBrushes.Locator>
	{
		#region Member Variables

		private static DockManagerWashBaseDarkBrushes g_Instance;

		#endregion Member Variables

		#region Constructor
		static DockManagerWashBaseDarkBrushes()
		{
			g_Instance = new DockManagerWashBaseDarkBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="DockManagerWashBaseDarkBrushes"/>
		/// </summary>
		public DockManagerWashBaseDarkBrushes()
		{
		}
		#endregion Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DockManagerWashBaseDarkBrushes Instance
		{
			get { return g_Instance; }
		}

		#endregion Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return "WashBaseDark"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DockManagerGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DockManagerBrushes\DockManagerWashBaseDark_Brushes.xaml"; } }
		}
	}

	#endregion DockManagerWashBaseDarkBrushes

	#region DockManagerWashBaseLightBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for a <see cref="XamDockManager"/>
	/// </summary>
	public class DockManagerWashBaseLightBrushes : DockManagerBrushes<DockManagerWashBaseLightBrushes.Locator>
	{
		#region Member Variables

		private static DockManagerWashBaseLightBrushes g_Instance;

		#endregion Member Variables

		#region Constructor
		static DockManagerWashBaseLightBrushes()
		{
			g_Instance = new DockManagerWashBaseLightBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="DockManagerWashBaseLightBrushes"/>
		/// </summary>
		public DockManagerWashBaseLightBrushes()
		{
		}
		#endregion Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static DockManagerWashBaseLightBrushes Instance
		{
			get { return g_Instance; }
		}

		#endregion Instance static property

		/// <summary>
		/// Identifies the location of a specific theme grouping
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public class Locator : ResourceSetLocator
		{
			/// <summary>The assembly containg the resource set</summary>
			public override Assembly Assembly { get { return this.GetType().Assembly; } }
			/// <summary>The name of the theme</summary>
			public override string Theme { get { return "WashBaseDark"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return DockManagerGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\DockManagerBrushes\DockManagerWashBaseLight_Brushes.xaml"; } }
		}
	}

	#endregion DockManagerWashBaseLightBrushes

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