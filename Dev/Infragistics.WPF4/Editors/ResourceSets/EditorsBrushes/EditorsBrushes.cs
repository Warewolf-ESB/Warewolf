using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.Windows.Themes;
using System.Windows;
using System.Reflection;
using System.ComponentModel;
using Infragistics.Windows.Editors;

namespace Infragistics.Windows.Themes
{
	#region EditorBrushes<T>
	/// <summary>
	/// Abstract base class used to supply brush resources for <see cref="ValueEditor"/> derived classes.
	/// </summary>
	/// <typeparam name="T">A <see cref="ResourceSetLocator"/> derived class that must have a public parameterless constructor.</typeparam>
	public abstract class EditorBrushes<T> : ResourceSet
		where T : ResourceSetLocator, new()
	{
		#region Member Variables

		private static readonly ResourceSetLocator g_Location;
		private static readonly ResourceDictionary g_ResourcesInternal;

		#endregion //Member Variables

		#region Constructor
		static EditorBrushes()
		{
			g_Location = new T();
			g_ResourcesInternal = Utilities.CreateResourceSetDictionary(g_Location.Assembly, g_Location.ResourcePath);
		}

		/// <summary>
		/// Initializes a new <see cref="EditorBrushes&lt;T&gt;"/>
		/// </summary>
		protected EditorBrushes()
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
	#endregion //EditorBrushes<T>

	#region EditorsGenericBrushes

	/// <summary>
    /// Class used to supply brush resources for the default look for <see cref="ValueEditor"/> derived classes
	/// </summary>
	public class EditorsGenericBrushes : EditorBrushes<EditorsGenericBrushes.Locator>
	{
		#region Member Variables

		private static EditorsGenericBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static EditorsGenericBrushes()
		{
			g_Instance = new EditorsGenericBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="EditorsGenericBrushes"/>
		/// </summary>
		public EditorsGenericBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsGenericBrushes Instance
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
			public override string Grouping { get { return EditorsGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\EditorsBrushes\EditorsGeneric_Brushes.xaml"; } }
		}
	}

	#endregion //EditorsGenericBrushes

	#region EditorsLunaNormalBrushes

	/// <summary>
    /// Class used to supply brush resources for the default look for <see cref="ValueEditor"/> derived classes
	/// </summary>
	public class EditorsLunaNormalBrushes : EditorBrushes<EditorsLunaNormalBrushes.Locator>
	{
		#region Member Variables

		private static EditorsLunaNormalBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static EditorsLunaNormalBrushes()
		{
			g_Instance = new EditorsLunaNormalBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="EditorsLunaNormalBrushes"/>
		/// </summary>
		public EditorsLunaNormalBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsLunaNormalBrushes Instance
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
			public override string Grouping { get { return EditorsGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\EditorsBrushes\EditorsLunaNormal_Brushes.xaml"; } }
		}
	}

	#endregion //EditorsLunaNormalBrushes

	#region EditorsLunaOliveBrushes

	/// <summary>
    /// Class used to supply brush resources for the default look for <see cref="ValueEditor"/> derived classes
	/// </summary>
	public class EditorsLunaOliveBrushes : EditorBrushes<EditorsLunaOliveBrushes.Locator>
	{
		#region Member Variables

		private static EditorsLunaOliveBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static EditorsLunaOliveBrushes()
		{
			g_Instance = new EditorsLunaOliveBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="EditorsLunaOliveBrushes"/>
		/// </summary>
		public EditorsLunaOliveBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsLunaOliveBrushes Instance
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
			public override string Grouping { get { return EditorsGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\EditorsBrushes\EditorsLunaOlive_Brushes.xaml"; } }
		}
	}

	#endregion //EditorsLunaOliveBrushes

	#region EditorsLunaSilverBrushes

	/// <summary>
    /// Class used to supply brush resources for the default look for <see cref="ValueEditor"/> derived classes
	/// </summary>
	public class EditorsLunaSilverBrushes : EditorBrushes<EditorsLunaSilverBrushes.Locator>
	{
		#region Member Variables

		private static EditorsLunaSilverBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static EditorsLunaSilverBrushes()
		{
			g_Instance = new EditorsLunaSilverBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="EditorsLunaSilverBrushes"/>
		/// </summary>
		public EditorsLunaSilverBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsLunaSilverBrushes Instance
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
			public override string Grouping { get { return EditorsGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\EditorsBrushes\EditorsLunaSilver_Brushes.xaml"; } }
		}
	}

	#endregion //EditorsLunaSilverBrushes

	#region EditorsAeroBrushes

	/// <summary>
    /// Class used to supply brush resources for the default look for <see cref="ValueEditor"/> derived classes
	/// </summary>
	public class EditorsAeroBrushes : EditorBrushes<EditorsAeroBrushes.Locator>
	{
		#region Member Variables

		private static EditorsAeroBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static EditorsAeroBrushes()
		{
			g_Instance = new EditorsAeroBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="EditorsAeroBrushes"/>
		/// </summary>
		public EditorsAeroBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsAeroBrushes Instance
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
			public override string Grouping { get { return EditorsAero.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\EditorsBrushes\EditorsAero_Brushes.xaml"; } }
		}
	}

	#endregion //EditorsAeroBrushes

	#region EditorsOffice2k7BlackBrushes

	/// <summary>
    /// Class used to supply brush resources for the default look for <see cref="ValueEditor"/> derived classes
	/// </summary>
	public class EditorsOffice2k7BlackBrushes : EditorBrushes<EditorsOffice2k7BlackBrushes.Locator>
	{
		#region Member Variables

		private static EditorsOffice2k7BlackBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static EditorsOffice2k7BlackBrushes()
		{
			g_Instance = new EditorsOffice2k7BlackBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="EditorsOffice2k7BlackBrushes"/>
		/// </summary>
		public EditorsOffice2k7BlackBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsOffice2k7BlackBrushes Instance
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
			public override string Grouping { get { return EditorsOffice2k7Black.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\EditorsBrushes\EditorsOffice2k7Black_Brushes.xaml"; } }
		}
	}

	#endregion //EditorsOffice2k7BlackBrushes

	#region EditorsOffice2k7BlueBrushes

	/// <summary>
    /// Class used to supply brush resources for the default look for <see cref="ValueEditor"/> derived classes
	/// </summary>
	public class EditorsOffice2k7BlueBrushes : EditorBrushes<EditorsOffice2k7BlueBrushes.Locator>
	{
		#region Member Variables

		private static EditorsOffice2k7BlueBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static EditorsOffice2k7BlueBrushes()
		{
			g_Instance = new EditorsOffice2k7BlueBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="EditorsOffice2k7BlueBrushes"/>
		/// </summary>
		public EditorsOffice2k7BlueBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsOffice2k7BlueBrushes Instance
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
			public override string Grouping { get { return EditorsOffice2k7Blue.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\EditorsBrushes\EditorsOffice2k7Blue_Brushes.xaml"; } }
		}
	}

	#endregion //EditorsOffice2k7BlueBrushes

	#region EditorsOffice2k7SilverBrushes

	/// <summary>
    /// Class used to supply brush resources for the default look for <see cref="ValueEditor"/> derived classes
	/// </summary>
	public class EditorsOffice2k7SilverBrushes : EditorBrushes<EditorsOffice2k7SilverBrushes.Locator>
	{
		#region Member Variables

		private static EditorsOffice2k7SilverBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static EditorsOffice2k7SilverBrushes()
		{
			g_Instance = new EditorsOffice2k7SilverBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="EditorsOffice2k7SilverBrushes"/>
		/// </summary>
		public EditorsOffice2k7SilverBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsOffice2k7SilverBrushes Instance
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
			public override string Grouping { get { return EditorsOffice2k7Silver.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\EditorsBrushes\EditorsOffice2k7Silver_Brushes.xaml"; } }
		}
	}

	#endregion //EditorsOffice2k7SilverBrushes

	#region EditorsRoyaleBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for <see cref="ValueEditor"/> derived classes
	/// </summary>
	public class EditorsRoyaleBrushes : EditorBrushes<EditorsRoyaleBrushes.Locator>
	{
		#region Member Variables

		private static EditorsRoyaleBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static EditorsRoyaleBrushes()
		{
			g_Instance = new EditorsRoyaleBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="EditorsRoyaleBrushes"/>
		/// </summary>
		public EditorsRoyaleBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsRoyaleBrushes Instance
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
			public override string Grouping { get { return EditorsRoyale.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\EditorsBrushes\EditorsRoyale_Brushes.xaml"; } }
		}
	}

	#endregion EditorsRoyaleBrushes

	#region EditorsRoyaleStrongBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for <see cref="ValueEditor"/> derived classes
	/// </summary>
	public class EditorsRoyaleStrongBrushes : EditorBrushes<EditorsRoyaleStrongBrushes.Locator>
	{
		#region Member Variables

		private static EditorsRoyaleStrongBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static EditorsRoyaleStrongBrushes()
		{
			g_Instance = new EditorsRoyaleStrongBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="EditorsRoyaleStrongBrushes"/>
		/// </summary>
		public EditorsRoyaleStrongBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsRoyaleStrongBrushes Instance
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
			public override string Theme { get { return "RoyaleStrong"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return EditorsRoyaleStrong.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\EditorsBrushes\EditorsRoyaleStrong_Brushes.xaml"; } }
		}
	}

	#endregion EditorsRoyaleStrongBrushes

	#region EditorsOnyxBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for <see cref="ValueEditor"/> derived classes
	/// </summary>
	public class EditorsOnyxBrushes : EditorBrushes<EditorsOnyxBrushes.Locator>
	{
		#region Member Variables

		private static EditorsOnyxBrushes g_Instance;

		#endregion //Member Variables

		#region Constructor
		static EditorsOnyxBrushes()
		{
			g_Instance = new EditorsOnyxBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="EditorsOnyxBrushes"/>
		/// </summary>
		public EditorsOnyxBrushes()
		{
		}
		#endregion //Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsOnyxBrushes Instance
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
			public override string Theme { get { return "Onyx"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return EditorsOnyx.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\EditorsBrushes\EditorsOnyx_Brushes.xaml"; } }
		}
	}

	#endregion EditorsOnyxBrushes

	#region EditorsWashBaseDarkBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for <see cref="ValueEditor"/> derived classes
	/// </summary>
	public class EditorsWashBaseDarkBrushes : EditorBrushes<EditorsWashBaseDarkBrushes.Locator>
	{
		#region Member Variables

		private static EditorsWashBaseDarkBrushes g_Instance;

		#endregion Member Variables

		#region Constructor
		static EditorsWashBaseDarkBrushes()
		{
			g_Instance = new EditorsWashBaseDarkBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="EditorsWashBaseDarkBrushes"/>
		/// </summary>
		public EditorsWashBaseDarkBrushes()
		{
		}
		#endregion Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsWashBaseDarkBrushes Instance
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
			public override string Grouping { get { return EditorsWashBaseDark.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\EditorsBrushes\EditorsWashBaseDark_Brushes.xaml"; } }
		}
	}

	#endregion EditorsWashBaseDarkBrushes

	#region EditorsWashBaseLightBrushes

	/// <summary>
	/// Class used to supply brush resources for the default look for <see cref="ValueEditor"/> derived classes
	/// </summary>
	public class EditorsWashBaseLightBrushes : EditorBrushes<EditorsWashBaseLightBrushes.Locator>
	{
		#region Member Variables

		private static EditorsWashBaseLightBrushes g_Instance;

		#endregion Member Variables

		#region Constructor
		static EditorsWashBaseLightBrushes()
		{
			g_Instance = new EditorsWashBaseLightBrushes();
		}

		/// <summary>
		/// Initializes a new <see cref="EditorsWashBaseLightBrushes"/>
		/// </summary>
		public EditorsWashBaseLightBrushes()
		{
		}
		#endregion Constructor

		#region Instance static property

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsWashBaseLightBrushes Instance
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
			public override string Theme { get { return "WashBaseLight"; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return EditorsWashBaseLight.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>
			public override string ResourcePath { get { return @"ResourceSets\EditorsBrushes\EditorsWashBaseLight_Brushes.xaml"; } }
		}
	}

	#endregion EditorsWashBaseLightBrushes


    #region EditorsPrintBasicBrushes

    /// <summary>
    /// Class used to supply brush resources for <see cref="ValueEditor"/> derived classes when printing
    /// </summary>
    public class EditorsPrintBasicBrushes : EditorBrushes<EditorsPrintBasicBrushes.Locator>
    {
        #region Member Variables

        private static EditorsPrintBasicBrushes g_Instance;

        #endregion //Member Variables

        #region Constructor
        static EditorsPrintBasicBrushes()
        {
            g_Instance = new EditorsPrintBasicBrushes();
        }

        /// <summary>
        /// Initializes a new <see cref="EditorsPrintBasicBrushes"/>
        /// </summary>
        public EditorsPrintBasicBrushes()
        {
        }
        #endregion //Constructor

        #region Instance static property

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static EditorsPrintBasicBrushes Instance
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
            public override string Theme { get { return ThemeManager.ThemeNamePrintBasic; } }
            /// <summary>The name of the grouping</summary>
            public override string Grouping { get { return EditorsPrintBasic.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\EditorsBrushes\EditorsPrintBasic_Brushes.xaml"; } }
        }
    }

    #endregion EditorsPrintBrushes

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