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


namespace Infragistics.Windows.Themes
{
    #region ChartResourceSet<T> base class

    /// <summary>
    /// Abstract base class used to supply style resources for a specific look for Chart.
    /// </summary>
    /// <typeparam name="T">A <see cref="ResourceSetLocator"/> derived class that must have a public parameterless constructor.</typeparam>
    internal abstract class ChartResourceSet<T> : ResourceSet where T : ResourceSetLocator, new()
    {
        #region Constants

		// AS 11/6/07 ThemeGroupingName
		//static internal readonly string GroupingName = "Chart";
		internal const string GroupingName = "Chart";

        #endregion //Constants

        #region Base class overrides

        #region Grouping

        /// <summary>
        /// Returns the grouping that the resources are applied to (read-only)
        /// </summary>
        /// <remarks>
        /// Examples: ChartBase, Editors, ChartBase, WPF etc.
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
                //this.VerifyResources();

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

        #endregion //Public Properties

        #endregion //Static Properties
    }

    #endregion //ChartResourceSet<T> base class

    #region Theme1

    /// <summary>
    /// Class used to supply style resources
    /// </summary>
    internal class Theme1 : ChartResourceSet<Theme1.Locator>
    {

        #region Instance static property

        private static Theme1 g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static Theme1 Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new Theme1();

                return g_Instance;
            }
        }

        #endregion //Instance static property

        /// <summary>
        /// Identifies the location of a specific theme grouping
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class Locator : ResourceSetLocator
        {
            /// <summary>The assembly containg the resource set</summary>
            public override Assembly Assembly { get { return this.GetType().Assembly; } }
            /// <summary>The name of the theme</summary>
            public override string Theme { get { return "Aquarius"; } }
            /// <summary>The name of the grouping</summary>
            public override string Grouping { get { return Theme1.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Chart\XamChartAquarius.xaml"; } }
        }
    }

    #endregion // Theme1

    #region Theme2

    /// <summary>
    /// Class used to supply style resources
    /// </summary>
    internal class Theme2 : ChartResourceSet<Theme2.Locator>
    {

        #region Instance static property

        private static Theme2 g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static Theme2 Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new Theme2();

                return g_Instance;
            }
        }

        #endregion //Instance static property

        /// <summary>
        /// Identifies the location of a specific theme grouping
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class Locator : ResourceSetLocator
        {
            /// <summary>The assembly containg the resource set</summary>
            public override Assembly Assembly { get { return this.GetType().Assembly; } }
            /// <summary>The name of the theme</summary>
            public override string Theme { get { return "Default"; } }
            /// <summary>The name of the grouping</summary>
            public override string Grouping { get { return Theme2.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Chart\XamChartDefault.xaml"; } }
        }
    }

    #endregion // Theme2

    #region Theme4

    /// <summary>
    /// Class used to supply style resources
    /// </summary>
    internal class Theme4 : ChartResourceSet<Theme4.Locator>
    {

        #region Instance static property

        private static Theme4 g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static Theme4 Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new Theme4();

                return g_Instance;
            }
        }

        #endregion //Instance static property

        /// <summary>
        /// Identifies the location of a specific theme grouping
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class Locator : ResourceSetLocator
        {
            /// <summary>The assembly containg the resource set</summary>
            public override Assembly Assembly { get { return this.GetType().Assembly; } }
            /// <summary>The name of the theme</summary>
            public override string Theme { get { return "LucidDream"; } }
            /// <summary>The name of the grouping</summary>
            public override string Grouping { get { return Theme4.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Chart\XamChartLucidDream.xaml"; } }
        }
    }

    #endregion // Theme4

    #region Theme5

    /// <summary>
    /// Class used to supply style resources
    /// </summary>
    internal class Theme5 : ChartResourceSet<Theme5.Locator>
    {

        #region Instance static property

        private static Theme5 g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static Theme5 Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new Theme5();

                return g_Instance;
            }
        }

        #endregion //Instance static property

        /// <summary>
        /// Identifies the location of a specific theme grouping
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class Locator : ResourceSetLocator
        {
            /// <summary>The assembly containg the resource set</summary>
            public override Assembly Assembly { get { return this.GetType().Assembly; } }
            /// <summary>The name of the theme</summary>
            public override string Theme { get { return "Luminol"; } }
            /// <summary>The name of the grouping</summary>
            public override string Grouping { get { return Theme5.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Chart\XamChartLuminol.xaml"; } }
        }
    }

    #endregion // Theme5

    #region Theme6

    /// <summary>
    /// Class used to supply style resources
    /// </summary>
    internal class Theme6 : ChartResourceSet<Theme6.Locator>
    {

        #region Instance static property

        private static Theme6 g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static Theme6 Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new Theme6();

                return g_Instance;
            }
        }

        #endregion //Instance static property

        /// <summary>
        /// Identifies the location of a specific theme grouping
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class Locator : ResourceSetLocator
        {
            /// <summary>The assembly containg the resource set</summary>
            public override Assembly Assembly { get { return this.GetType().Assembly; } }
            /// <summary>The name of the theme</summary>
            public override string Theme { get { return "Nautilus"; } }
            /// <summary>The name of the grouping</summary>
            public override string Grouping { get { return Theme6.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Chart\XamChartNautilus.xaml"; } }
        }
    }

    #endregion // Theme6

    #region Theme7

    /// <summary>
    /// Class used to supply style resources
    /// </summary>
    internal class Theme7 : ChartResourceSet<Theme7.Locator>
    {

        #region Instance static property

        private static Theme7 g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static Theme7 Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new Theme7();

                return g_Instance;
            }
        }

        #endregion //Instance static property

        /// <summary>
        /// Identifies the location of a specific theme grouping
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class Locator : ResourceSetLocator
        {
            /// <summary>The assembly containg the resource set</summary>
            public override Assembly Assembly { get { return this.GetType().Assembly; } }
            /// <summary>The name of the theme</summary>
            public override string Theme { get { return "Neon"; } }
            /// <summary>The name of the grouping</summary>
            public override string Grouping { get { return Theme7.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Chart\XamChartNeon.xaml"; } }
        }
    }

    #endregion // Theme7

    #region Theme8

    /// <summary>
    /// Class used to supply style resources
    /// </summary>
    internal class Theme8 : ChartResourceSet<Theme8.Locator>
    {

        #region Instance static property

        private static Theme8 g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static Theme8 Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new Theme8();

                return g_Instance;
            }
        }

        #endregion //Instance static property

        /// <summary>
        /// Identifies the location of a specific theme grouping
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class Locator : ResourceSetLocator
        {
            /// <summary>The assembly containg the resource set</summary>
            public override Assembly Assembly { get { return this.GetType().Assembly; } }
            /// <summary>The name of the theme</summary>
            public override string Theme { get { return "Peach"; } }
            /// <summary>The name of the grouping</summary>
            public override string Grouping { get { return Theme8.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Chart\XamChartPeach.xaml"; } }
        }
    }

    #endregion // Theme8

    #region Theme9

    /// <summary>
    /// Class used to supply style resources
    /// </summary>
    internal class Theme9 : ChartResourceSet<Theme9.Locator>
    {

        #region Instance static property

        private static Theme9 g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static Theme9 Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new Theme9();

                return g_Instance;
            }
        }

        #endregion //Instance static property

        /// <summary>
        /// Identifies the location of a specific theme grouping
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class Locator : ResourceSetLocator
        {
            /// <summary>The assembly containg the resource set</summary>
            public override Assembly Assembly { get { return this.GetType().Assembly; } }
            /// <summary>The name of the theme</summary>
            public override string Theme { get { return "Pumpkin"; } }
            /// <summary>The name of the grouping</summary>
            public override string Grouping { get { return Theme9.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Chart\XamChartPumpkin.xaml"; } }
        }
    }

    #endregion // Theme9

    #region Theme10

    /// <summary>
    /// Class used to supply style resources
    /// </summary>
    internal class Theme10 : ChartResourceSet<Theme10.Locator>
    {

        #region Instance static property

        private static Theme10 g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static Theme10 Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new Theme10();

                return g_Instance;
            }
        }

        #endregion //Instance static property

        /// <summary>
        /// Identifies the location of a specific theme grouping
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class Locator : ResourceSetLocator
        {
            /// <summary>The assembly containg the resource set</summary>
            public override Assembly Assembly { get { return this.GetType().Assembly; } }
            /// <summary>The name of the theme</summary>
            public override string Theme { get { return "RedPlanet"; } }
            /// <summary>The name of the grouping</summary>
            public override string Grouping { get { return Theme10.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Chart\XamChartRedPlanet.xaml"; } }
        }
    }

    #endregion // Theme10

    #region Theme11

    /// <summary>
    /// Class used to supply style resources
    /// </summary>
    internal class Theme11 : ChartResourceSet<Theme11.Locator>
    {

        #region Instance static property

        private static Theme11 g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static Theme11 Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new Theme11();

                return g_Instance;
            }
        }

        #endregion //Instance static property

        /// <summary>
        /// Identifies the location of a specific theme grouping
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class Locator : ResourceSetLocator
        {
            /// <summary>The assembly containg the resource set</summary>
            public override Assembly Assembly { get { return this.GetType().Assembly; } }
            /// <summary>The name of the theme</summary>
            public override string Theme { get { return "RoyaleVelvet"; } }
            /// <summary>The name of the grouping</summary>
            public override string Grouping { get { return Theme11.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Chart\XamChartRoyaleVelvet.xaml"; } }
        }
    }

    #endregion // Theme11
    #region Theme12

    /// <summary>
    /// Class used to supply style resources
    /// </summary>
    internal class Theme12 : ChartResourceSet<Theme12.Locator>
    {

        #region Instance static property

        private static Theme12 g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static Theme12 Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new Theme12();

                return g_Instance;
            }
        }

        #endregion //Instance static property

        /// <summary>
        /// Identifies the location of a specific theme grouping
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class Locator : ResourceSetLocator
        {
            /// <summary>The assembly containg the resource set</summary>
            public override Assembly Assembly { get { return this.GetType().Assembly; } }
            /// <summary>The name of the theme</summary>
            public override string Theme { get { return "ThemePark"; } }
            /// <summary>The name of the grouping</summary>
            public override string Grouping { get { return Theme12.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Chart\XamChartThemePark.xaml"; } }
        }
    }

    #endregion // Theme12

    #region IGTheme

    /// <summary>
    /// Class used to supply style resources
    /// </summary>
    internal class IGTheme : ChartResourceSet<IGTheme.Locator>
    {

        #region Instance static property

        private static IGTheme g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static IGTheme Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new IGTheme();

                return g_Instance;
            }
        }

        #endregion //Instance static property

        /// <summary>
        /// Identifies the location of a specific theme grouping
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal class Locator : ResourceSetLocator
        {
            /// <summary>The assembly containg the resource set</summary>
            public override Assembly Assembly { get { return this.GetType().Assembly; } }
            /// <summary>The name of the theme</summary>
            public override string Theme { get { return ThemeManager.ThemeNameIGTheme; } }
            /// <summary>The name of the grouping</summary>
            public override string Grouping { get { return IGTheme.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Chart\XamChartIGTheme.xaml"; } }
        }
    }

    #endregion // IGTheme
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