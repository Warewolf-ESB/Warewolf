using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Reflection;
using System.ComponentModel;

namespace Infragistics.Windows.Themes
{
    #region OutlookBarResourceSet<T> base class

    /// <summary>
    /// Abstract base class used to supply style resources for a specific look for XamOutlookBar.
    /// </summary>
    /// <typeparam name="T">A <see cref="ResourceSetLocator"/> derived class that must have a public parameterless constructor.</typeparam>
    public abstract class OutlookBarResourceSet<T> : ResourceSet where T : ResourceSetLocator, new()
    {
        #region Constants

        internal const string GroupingName = "OutlookBar";

        #endregion //Constants

        #region Base class overrides

        #region Grouping

        /// <summary>
        /// Returns the grouping that the resources are applied to (read-only)
        /// </summary>
        /// <remarks>
        /// Examples: OutlookBar, Editors, Primitives, etc.
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

                // JJD 7/25/07 - ResourceWasher support
                // Call VerifyResources after the initial load so that we can delay the hydrating
                // of the resources by a ResourceWasher until this theme is actually used
                this.VerifyResources();

                return rd;
            }
        }

        #endregion //Resources

        #endregion //Base class overrides

        #region Static Properties

        #region Private Propeties

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

        #region Public Properties

        #region GroupAreaSplitter

        private static Style g_GroupAreaSplitter;

        /// <summary>
        /// The style for a <see cref="Infragistics.Windows.OutlookBar.GroupAreaSplitter"/> 
        /// </summary>
        public static Style GroupAreaSplitter
        {
            get
            {
                if (g_GroupAreaSplitter == null)
                    g_GroupAreaSplitter = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.OutlookBar.GroupAreaSplitter)) as Style;

                return g_GroupAreaSplitter;
            }
        }

        #endregion //GroupAreaSplitter

        #region NavigationPaneOptionsControl

        private static Style g_NavigationPaneOptionsControl;

        /// <summary>
        /// The style for a <see cref="Infragistics.Windows.OutlookBar.NavigationPaneOptionsControl"/> 
        /// </summary>
        public static Style NavigationPaneOptionsControl
        {
            get
            {
                if (g_NavigationPaneOptionsControl == null)
                    g_NavigationPaneOptionsControl = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.OutlookBar.NavigationPaneOptionsControl)) as Style;

                return g_NavigationPaneOptionsControl;
            }
        }

        #endregion //NavigationPaneOptionsControl

        #region VerticalSplitterPreview

        private static Style g_VerticalSplitterPreview;

        /// <summary>
        /// The style for a <see cref="Infragistics.Windows.OutlookBar.VerticalSplitterPreview"/> 
        /// </summary>
        public static Style VerticalSplitterPreview
        {
            get
            {
                if (g_VerticalSplitterPreview == null)
                    g_VerticalSplitterPreview = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.OutlookBar.VerticalSplitterPreview)) as Style;

                return g_VerticalSplitterPreview;
            }
        }

        #endregion //VerticalSplitterPreview

        #region XamOutlookBar

        private static Style g_XamOutlookBar;

        /// <summary>
        /// The style for a <see cref="Infragistics.Windows.OutlookBar.XamOutlookBar"/> 
        /// </summary>
        public static Style XamOutlookBar
        {
            get
            {
                if (g_XamOutlookBar == null)
                    g_XamOutlookBar = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.OutlookBar.XamOutlookBar)) as Style;

                return g_XamOutlookBar;
            }
        }

        #endregion //XamOutlookBar

        #region SelectedGroupHeader

        private static Style g_SelectedGroupHeader;

        /// <summary>
        /// The style for a <see cref="Infragistics.Windows.OutlookBar.SelectedGroupHeader"/> 
        /// </summary>
        public static Style SelectedGroupHeader
        {
            get
            {
                if (g_SelectedGroupHeader == null)
                    g_SelectedGroupHeader = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.OutlookBar.SelectedGroupHeader)) as Style;

                return g_SelectedGroupHeader;
            }
        }

        #endregion //SelectedGroupHeader

        #region SelectedGroupContent

        private static Style g_SelectedGroupContent;

        /// <summary>
        /// The style for a <see cref="Infragistics.Windows.OutlookBar.SelectedGroupContent"/> 
        /// </summary>
        public static Style SelectedGroupContent
        {
            get
            {
                if (g_SelectedGroupContent == null)
                    g_SelectedGroupContent = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.OutlookBar.SelectedGroupContent)) as Style;

                return g_SelectedGroupContent;
            }
        }

        #endregion //SelectedGroupContent

        #region OutlookBarGroup

        private static Style g_OutlookBarGroup;

        /// <summary>
        /// The style for a <see cref="Infragistics.Windows.OutlookBar.OutlookBarGroup"/> 
        /// </summary>
        public static Style OutlookBarGroup
        {
            get
            {
                if (g_OutlookBarGroup == null)
                    g_OutlookBarGroup = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.OutlookBar.OutlookBarGroup)) as Style;

                return g_OutlookBarGroup;
            }
        }

        #endregion //OutlookBarGroup

        #region GroupOverflowArea

        private static Style g_GroupOverflowArea;

        /// <summary>
        /// The style for a <see cref="Infragistics.Windows.OutlookBar.GroupOverflowArea"/> 
        /// </summary>
        public static Style GroupOverflowArea
        {
            get
            {
                if (g_GroupOverflowArea == null)
                    g_GroupOverflowArea = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.OutlookBar.GroupOverflowArea)) as Style;

                return g_GroupOverflowArea;
            }
        }

        #endregion //GroupOverflowArea

        #endregion //Public Properties

        #endregion //Static Properties
    }

    #endregion //OutlookBarResourceSet<T> base class

    #region OutlookBarGeneric

    /// <summary>
    /// Class used to supply style resources for the Generic look for a OutlookBar
    /// </summary>
    public class OutlookBarGeneric : OutlookBarResourceSet<OutlookBarGeneric.Locator>
    {

        #region Instance static property

        private static OutlookBarGeneric g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static OutlookBarGeneric Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new OutlookBarGeneric();

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
            public override string Grouping { get { return OutlookBarGeneric.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\OutlookBar\OutlookBarOffice2k7Blue.xaml"; } }
        }
    }

    #endregion //OutlookBarGeneric

    #region OutlookBarOffice2k7Black

    /// <summary>
    /// Class used to supply style resources for the Generic look for a OutlookBar
    /// </summary>
    public class OutlookBarOffice2k7Black : OutlookBarResourceSet<OutlookBarOffice2k7Black.Locator>
    {

	    #region Instance static property

	    private static OutlookBarOffice2k7Black g_Instance;

	    /// <summary>
	    /// Returns a static instance of this type (read-only)
	    /// </summary>
	    public static OutlookBarOffice2k7Black Instance
	    {
		    get
		    {
			    if (g_Instance == null)
				    g_Instance = new OutlookBarOffice2k7Black();

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
		    public override string Theme { get { return ThemeManager.ThemeNameOffice2k7Black; } }
		    /// <summary>The name of the grouping</summary>
		    public override string Grouping { get { return OutlookBarOffice2k7Black.GroupingName; } }
		    /// <summary>The path to the embedded resources within the assembly</summary>
		    public override string ResourcePath { get { return @"ResourceSets\OutlookBar\OutlookBarOffice2k7Black.xaml"; } }
	    }
    }

    #endregion //OutlookBarOffice2k7Black

    #region OutlookBarOffice2k7Blue

    /// <summary>
    /// Class used to supply style resources for the Generic look for a OutlookBar
    /// </summary>
    public class OutlookBarOffice2k7Blue : OutlookBarResourceSet<OutlookBarOffice2k7Blue.Locator>
    {

	    #region Instance static property

	    private static OutlookBarOffice2k7Blue g_Instance;

	    /// <summary>
	    /// Returns a static instance of this type (read-only)
	    /// </summary>
	    public static OutlookBarOffice2k7Blue Instance
	    {
		    get
		    {
			    if (g_Instance == null)
				    g_Instance = new OutlookBarOffice2k7Blue();

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
		    public override string Theme { get { return ThemeManager.ThemeNameOffice2k7Blue; } }
		    /// <summary>The name of the grouping</summary>
		    public override string Grouping { get { return OutlookBarOffice2k7Blue.GroupingName; } }
		    /// <summary>The path to the embedded resources within the assembly</summary>
		    public override string ResourcePath { get { return @"ResourceSets\OutlookBar\OutlookBarOffice2k7Blue.xaml"; } }
	    }
    }

    #endregion //OutlookBarOffice2k7Blue

    #region OutlookBarOffice2k7Silver

    /// <summary>
    /// Class used to supply style resources for the Generic look for a OutlookBar
    /// </summary>
    public class OutlookBarOffice2k7Silver : OutlookBarResourceSet<OutlookBarOffice2k7Silver.Locator>
    {

	    #region Instance static property

	    private static OutlookBarOffice2k7Silver g_Instance;

	    /// <summary>
	    /// Returns a static instance of this type (read-only)
	    /// </summary>
	    public static OutlookBarOffice2k7Silver Instance
	    {
		    get
		    {
			    if (g_Instance == null)
				    g_Instance = new OutlookBarOffice2k7Silver();

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
		    public override string Theme { get { return ThemeManager.ThemeNameOffice2k7Silver; } }
		    /// <summary>The name of the grouping</summary>
		    public override string Grouping { get { return OutlookBarOffice2k7Silver.GroupingName; } }
		    /// <summary>The path to the embedded resources within the assembly</summary>
		    public override string ResourcePath { get { return @"ResourceSets\OutlookBar\OutlookBarOffice2k7Silver.xaml"; } }
	    }
    }

    #endregion //OutlookBarOffice2k7Silver

	// JJD 8/30/10 - Added Office 2010 Blue theme
	#region OutlookBarOffice2010Blue

    /// <summary>
    /// Class used to supply style resources for the Generic look for a OutlookBar
    /// </summary>
    public class OutlookBarOffice2010Blue : OutlookBarResourceSet<OutlookBarOffice2010Blue.Locator>
    {

	    #region Instance static property

	    private static OutlookBarOffice2010Blue g_Instance;

	    /// <summary>
	    /// Returns a static instance of this type (read-only)
	    /// </summary>
	    public static OutlookBarOffice2010Blue Instance
	    {
		    get
		    {
			    if (g_Instance == null)
				    g_Instance = new OutlookBarOffice2010Blue();

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
		    public override string Theme { get { return ThemeManager.ThemeNameOffice2010Blue; } }
		    /// <summary>The name of the grouping</summary>
		    public override string Grouping { get { return OutlookBarOffice2010Blue.GroupingName; } }
		    /// <summary>The path to the embedded resources within the assembly</summary>
		    public override string ResourcePath { get { return @"ResourceSets\OutlookBar\OutlookBarOffice2010Blue.xaml"; } }
	    }
    }

    #endregion //OutlookBarOffice2010Blue

    #region OutlookBarAero

    /// <summary>
    /// Class used to supply style resources for the Generic look for a OutlookBar
    /// </summary>
    public class OutlookBarAero : OutlookBarResourceSet<OutlookBarAero.Locator>
    {

	    #region Instance static property

	    private static OutlookBarAero g_Instance;

	    /// <summary>
	    /// Returns a static instance of this type (read-only)
	    /// </summary>
	    public static OutlookBarAero Instance
	    {
		    get
		    {
			    if (g_Instance == null)
				    g_Instance = new OutlookBarAero();

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
		    public override string Theme { get { return ThemeManager.ThemeNameAero; } }
		    /// <summary>The name of the grouping</summary>
		    public override string Grouping { get { return OutlookBarAero.GroupingName; } }
		    /// <summary>The path to the embedded resources within the assembly</summary>
		    public override string ResourcePath { get { return @"ResourceSets\OutlookBar\OutlookBarAero.xaml"; } }
	    }
    }

    #endregion //OutlookBarAero

    #region OutlookBarLunaNormal

    /// <summary>
    /// Class used to supply style resources for the Generic look for a OutlookBar
    /// </summary>
    public class OutlookBarLunaNormal : OutlookBarResourceSet<OutlookBarLunaNormal.Locator>
    {

	    #region Instance static property

	    private static OutlookBarLunaNormal g_Instance;

	    /// <summary>
	    /// Returns a static instance of this type (read-only)
	    /// </summary>
	    public static OutlookBarLunaNormal Instance
	    {
		    get
		    {
			    if (g_Instance == null)
				    g_Instance = new OutlookBarLunaNormal();

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
		    public override string Theme { get { return ThemeManager.ThemeNameLunaNormal; } }
		    /// <summary>The name of the grouping</summary>
		    public override string Grouping { get { return OutlookBarLunaNormal.GroupingName; } }
		    /// <summary>The path to the embedded resources within the assembly</summary>
		    public override string ResourcePath { get { return @"ResourceSets\OutlookBar\OutlookBarLunaNormal.xaml"; } }
	    }
    }

    #endregion //OutlookBarLunaNormal

    #region OutlookBarLunaOlive

    /// <summary>
    /// Class used to supply style resources for the Generic look for a OutlookBar
    /// </summary>
    public class OutlookBarLunaOlive : OutlookBarResourceSet<OutlookBarLunaOlive.Locator>
    {

	    #region Instance static property

	    private static OutlookBarLunaOlive g_Instance;

	    /// <summary>
	    /// Returns a static instance of this type (read-only)
	    /// </summary>
	    public static OutlookBarLunaOlive Instance
	    {
		    get
		    {
			    if (g_Instance == null)
				    g_Instance = new OutlookBarLunaOlive();

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
		    public override string Theme { get { return ThemeManager.ThemeNameLunaOlive; } }
		    /// <summary>The name of the grouping</summary>
		    public override string Grouping { get { return OutlookBarLunaOlive.GroupingName; } }
		    /// <summary>The path to the embedded resources within the assembly</summary>
		    public override string ResourcePath { get { return @"ResourceSets\OutlookBar\OutlookBarLunaOlive.xaml"; } }
	    }
    }

    #endregion //OutlookBarLunaOlive

    #region OutlookBarLunaSilver

    /// <summary>
    /// Class used to supply style resources for the Generic look for a OutlookBar
    /// </summary>
    public class OutlookBarLunaSilver : OutlookBarResourceSet<OutlookBarLunaSilver.Locator>
    {

	    #region Instance static property

	    private static OutlookBarLunaSilver g_Instance;

	    /// <summary>
	    /// Returns a static instance of this type (read-only)
	    /// </summary>
	    public static OutlookBarLunaSilver Instance
	    {
		    get
		    {
			    if (g_Instance == null)
				    g_Instance = new OutlookBarLunaSilver();

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
		    public override string Theme { get { return ThemeManager.ThemeNameLunaSilver; } }
		    /// <summary>The name of the grouping</summary>
		    public override string Grouping { get { return OutlookBarLunaSilver.GroupingName; } }
		    /// <summary>The path to the embedded resources within the assembly</summary>
		    public override string ResourcePath { get { return @"ResourceSets\OutlookBar\OutlookBarLunaSilver.xaml"; } }
	    }
    }

    #endregion //OutlookBarLunaSilver

    #region OutlookBarRoyale

    /// <summary>
    /// Class used to supply style resources for the Generic look for a OutlookBar
    /// </summary>
    public class OutlookBarRoyale : OutlookBarResourceSet<OutlookBarRoyale.Locator>
    {

	    #region Instance static property

	    private static OutlookBarRoyale g_Instance;

	    /// <summary>
	    /// Returns a static instance of this type (read-only)
	    /// </summary>
	    public static OutlookBarRoyale Instance
	    {
		    get
		    {
			    if (g_Instance == null)
				    g_Instance = new OutlookBarRoyale();

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
		    public override string Theme { get { return ThemeManager.ThemeNameRoyale; } }
		    /// <summary>The name of the grouping</summary>
		    public override string Grouping { get { return OutlookBarRoyale.GroupingName; } }
		    /// <summary>The path to the embedded resources within the assembly</summary>
		    public override string ResourcePath { get { return @"ResourceSets\OutlookBar\OutlookBarRoyale.xaml"; } }
	    }
    }

    #endregion //OutlookBarRoyale

    #region OutlookBarWashBaseDark

    /// <summary>
    /// Class used to supply style resources for the Generic look for a OutlookBar
    /// </summary>
    public class OutlookBarWashBaseDark : OutlookBarResourceSet<OutlookBarWashBaseDark.Locator>
    {

	    #region Instance static property

	    private static OutlookBarWashBaseDark g_Instance;

	    /// <summary>
	    /// Returns a static instance of this type (read-only)
	    /// </summary>
	    public static OutlookBarWashBaseDark Instance
	    {
		    get
		    {
			    if (g_Instance == null)
				    g_Instance = new OutlookBarWashBaseDark();

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
		    public override string Theme { get { return ThemeManager.ThemeNameWashBaseDark; } }
		    /// <summary>The name of the grouping</summary>
		    public override string Grouping { get { return OutlookBarWashBaseDark.GroupingName; } }
		    /// <summary>The path to the embedded resources within the assembly</summary>
		    public override string ResourcePath { get { return @"ResourceSets\OutlookBar\OutlookBarWashBaseDark.xaml"; } }
	    }
    }

    #endregion //OutlookBarWashBaseDark

    #region OutlookBarWashBaseLight

    /// <summary>
    /// Class used to supply style resources for the Generic look for a OutlookBar
    /// </summary>
    public class OutlookBarWashBaseLight : OutlookBarResourceSet<OutlookBarWashBaseLight.Locator>
    {

	    #region Instance static property

	    private static OutlookBarWashBaseLight g_Instance;

	    /// <summary>
	    /// Returns a static instance of this type (read-only)
	    /// </summary>
	    public static OutlookBarWashBaseLight Instance
	    {
		    get
		    {
			    if (g_Instance == null)
				    g_Instance = new OutlookBarWashBaseLight();

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
		    public override string Theme { get { return ThemeManager.ThemeNameWashBaseLight; } }
		    /// <summary>The name of the grouping</summary>
		    public override string Grouping { get { return OutlookBarWashBaseLight.GroupingName; } }
		    /// <summary>The path to the embedded resources within the assembly</summary>
		    public override string ResourcePath { get { return @"ResourceSets\OutlookBar\OutlookBarWashBaseLight.xaml"; } }
	    }
    }

    #endregion //OutlookBarWashBaseLight

	// JJD 10/29/10 - NA 2011 Volumn 1 - IGTheme
	#region OutlookBarIGTheme

    /// <summary>
    /// Class used to supply style resources for the IGTheme look for a OutlookBar
    /// </summary>
    public class OutlookBarIGTheme : OutlookBarResourceSet<OutlookBarIGTheme.Locator>
    {

	    #region Instance static property

	    private static OutlookBarIGTheme g_Instance;

	    /// <summary>
	    /// Returns a static instance of this type (read-only)
	    /// </summary>
	    public static OutlookBarIGTheme Instance
	    {
		    get
		    {
			    if (g_Instance == null)
				    g_Instance = new OutlookBarIGTheme();

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
		    public override string Theme { get { return ThemeManager.ThemeNameIGTheme; } }
		    /// <summary>The name of the grouping</summary>
		    public override string Grouping { get { return OutlookBarIGTheme.GroupingName; } }
		    /// <summary>The path to the embedded resources within the assembly</summary>
		    public override string ResourcePath { get { return @"ResourceSets\OutlookBar\OutlookBarIGTheme.xaml"; } }
	    }
    }

    #endregion //OutlookBarIGTheme

	// JJD 02/16/12 - NA 2012 Volumn 1 - Metro theme
	#region OutlookBarMetro

    /// <summary>
    /// Class used to supply style resources for the Metro look for a OutlookBar
    /// </summary>
    public class OutlookBarMetro : OutlookBarResourceSet<OutlookBarMetro.Locator>
    {

	    #region Instance static property

	    private static OutlookBarMetro g_Instance;

	    /// <summary>
	    /// Returns a static instance of this type (read-only)
	    /// </summary>
	    public static OutlookBarMetro Instance
	    {
		    get
		    {
			    if (g_Instance == null)
				    g_Instance = new OutlookBarMetro();

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
		    public override string Theme { get { return ThemeManager.ThemeNameMetro; } }
		    /// <summary>The name of the grouping</summary>
		    public override string Grouping { get { return OutlookBarMetro.GroupingName; } }
		    /// <summary>The path to the embedded resources within the assembly</summary>
		    public override string ResourcePath { get { return @"ResourceSets\OutlookBar\OutlookBarMetro.xaml"; } }
	    }
    }

    #endregion //OutlookBarMetro

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