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
	#region EditorsResourceSet<T> base class

	/// <summary>
	/// Abstract base class used to supply style resources for a specific look for editor elements that are shared.
	/// </summary>
	/// <typeparam name="T">A <see cref="ResourceSetLocator"/> derived class that must have a public parameterless constructor.</typeparam>
	public abstract class EditorsResourceSet<T> : ResourceSet where T : ResourceSetLocator, new()
	{
		#region Constants

		// AS 11/6/07 ThemeGroupingName
		//static internal readonly string GroupingName = "Editors";
		internal const string GroupingName = "Editors";

		#endregion //Constants

		#region Base class overrides

			#region Grouping

		/// <summary>
		/// Returns the grouping that the resources are applied to (read-only)
		/// </summary>
		/// <remarks>
		/// Examples: DataPresenter, Editors, Editors, WPF etc.
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

			#endregion //Private Properties


			#region CaretElement

		private static Style g_CaretElement;

		/// <summary>
		/// The style for a <see cref="Infragistics.Windows.Editors.CaretElement"/> 
		/// </summary>
		public static Style CaretElement
		{
			get
			{
				if (g_CaretElement == null)
					g_CaretElement = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Editors.CaretElement)) as Style;

				return g_CaretElement;
			}
		}

			#endregion //CaretElement

			#region DisplayCharactersList

		private static Style g_DisplayCharactersList;

		/// <summary>
		/// The style for a <see cref="Infragistics.Windows.Editors.DisplayCharactersList"/> 
		/// </summary>
		public static Style DisplayCharactersList
		{
			get
			{
				if (g_DisplayCharactersList == null)
					g_DisplayCharactersList = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Editors.DisplayCharactersList)) as Style;

				return g_DisplayCharactersList;
			}
		}

			#endregion //DisplayCharactersList

			#region DisplayCharacterPresenter

		private static Style g_DisplayCharacterPresenter;

		/// <summary>
		/// The style for a <see cref="Infragistics.Windows.Editors.DisplayCharacterPresenter"/> 
		/// </summary>
		public static Style DisplayCharacterPresenter
		{
			get
			{
				if (g_DisplayCharacterPresenter == null)
					g_DisplayCharacterPresenter = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Editors.DisplayCharacterPresenter)) as Style;

				return g_DisplayCharacterPresenter;
			}
		}

			#endregion //DisplayCharacterPresenter

			#region SectionsList

		private static Style g_SectionsList;

		/// <summary>
		/// The style for a <see cref="Infragistics.Windows.Editors.SectionsList"/> 
		/// </summary>
		public static Style SectionsList
		{
			get
			{
				if (g_SectionsList == null)
					g_SectionsList = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Editors.SectionsList)) as Style;

				return g_SectionsList;
			}
		}

			#endregion //SectionsList

			#region SectionPresenter

		private static Style g_SectionPresenter;

		/// <summary>
		/// The style for a <see cref="Infragistics.Windows.Editors.SectionPresenter"/> 
		/// </summary>
		public static Style SectionPresenter
		{
			get
			{
				if (g_SectionPresenter == null)
					g_SectionPresenter = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Editors.SectionPresenter)) as Style;

				return g_SectionPresenter;
			}
		}

			#endregion //SectionPresenter

			#region XamCurrencyEditor

		private static Style g_XamCurrencyEditor;

		/// <summary>
		/// The style for a <see cref="Infragistics.Windows.Editors.XamCurrencyEditor"/> 
		/// </summary>
		public static Style XamCurrencyEditor
		{
			get
			{
				if (g_XamCurrencyEditor == null)
					g_XamCurrencyEditor = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Editors.XamCurrencyEditor)) as Style;

				return g_XamCurrencyEditor;
			}
		}

			#endregion //XamCurrencyEditor

			#region XamDateTimeEditor

		private static Style g_XamDateTimeEditor;

		/// <summary>
		/// The style for a <see cref="Infragistics.Windows.Editors.XamDateTimeEditor"/> 
		/// </summary>
		public static Style XamDateTimeEditor
		{
			get
			{
				if (g_XamDateTimeEditor == null)
					g_XamDateTimeEditor = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Editors.XamDateTimeEditor)) as Style;

				return g_XamDateTimeEditor;
			}
		}

			#endregion //XamDateTimeEditor

			#region XamMaskedEditor

		private static Style g_MaskedEditor;

		/// <summary>
		/// The style for a <see cref="Infragistics.Windows.Editors.XamMaskedEditor"/> 
		/// </summary>
		public static Style XamMaskedEditor
		{
		    get
		    {
		        if (g_MaskedEditor == null)
		            g_MaskedEditor = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Editors.XamMaskedEditor)) as Style;

		        return g_MaskedEditor;
		    }
		}

		    #endregion //XamMaskedEditor

			#region XamNumericEditor

		private static Style g_XamNumericEditor;

		/// <summary>
		/// The style for a <see cref="Infragistics.Windows.Editors.XamNumericEditor"/> 
		/// </summary>
		public static Style XamNumericEditor
		{
			get
			{
				if (g_XamNumericEditor == null)
					g_XamNumericEditor = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Editors.XamNumericEditor)) as Style;

				return g_XamNumericEditor;
			}
		}

			#endregion //XamNumericEditor

			#region XamComboEditor

		private static Style g_XamComboEditor;

		/// <summary>
        /// The style for a <see cref="Infragistics.Windows.Editors.XamComboEditor"/> 
		/// </summary>
		public static Style XamComboEditor
		{
			get
			{
				if ( g_XamComboEditor == null )
					g_XamComboEditor = ResourceSet.GetSealedResource(ResourcesInternal, typeof( Infragistics.Windows.Editors.XamComboEditor )) as Style;

				return g_XamComboEditor;
			}
		}

			#endregion //XamComboEditor

            #region XamComboEditorComboBox

        private static Style g_XamComboEditorComboBox;

        /// <summary>
        /// The style for a XamComboEditor.ComboEditorComboBox in a <see cref="Infragistics.Windows.Editors.XamComboEditor"/> 
        /// </summary>
        public static Style XamComboEditorComboBox
        {
            get
            {
                if (g_XamComboEditorComboBox == null)
                    g_XamComboEditorComboBox = ResourceSet.GetSealedResource(ResourcesInternal, Infragistics.Windows.Editors.XamComboEditor.ComboEditorComboBoxStyleKey) as Style;

                return g_XamComboEditorComboBox;
            }
        }

            #endregion //XamComboEditorComboBox

            #region XamComboEditorDropDownButton

        private static Style g_XamComboEditorDropDownButton;

        /// <summary>
        /// The style for a XamComboEditor.DropDownButtonStyle in a <see cref="Infragistics.Windows.Editors.XamComboEditor"/> 
        /// </summary>
        public static Style XamComboEditorDropDownButton
        {
            get
            {
                if (g_XamComboEditorDropDownButton == null)
                    g_XamComboEditorDropDownButton = ResourceSet.GetSealedResource(ResourcesInternal, "ComboEditor_DropDownButtonStyle") as Style;

                return g_XamComboEditorDropDownButton;
            }
        }

            #endregion //XamComboEditorToolDropDownButton

            // AS 10/10/08 XamMonthCalendar
            #region CalendarItem

            private static Style g_CalendarItem;

            /// <summary>
            /// The style for a <see cref="Infragistics.Windows.Editors.CalendarItem"/> 
            /// </summary>
            public static Style CalendarItem
            {
                get
                {
                    if (g_CalendarItem == null)
                        g_CalendarItem = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Editors.CalendarItem)) as Style;

                    return g_CalendarItem;
                }
            }

            #endregion //CalendarItem

            #region CalendarItemGroup

            private static Style g_CalendarItemGroup;

            /// <summary>
            /// The style for a <see cref="Infragistics.Windows.Editors.CalendarItemGroup"/> 
            /// </summary>
            public static Style CalendarItemGroup
            {
                get
                {
                    if (g_CalendarItemGroup == null)
                        g_CalendarItemGroup = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Editors.CalendarItemGroup)) as Style;

                    return g_CalendarItemGroup;
                }
            }

            #endregion //CalendarItemGroup

            #region CalendarDayOfWeek

            private static Style g_CalendarDayOfWeek;

            /// <summary>
            /// The style for a <see cref="Infragistics.Windows.Editors.CalendarDayOfWeek"/> 
            /// </summary>
            public static Style CalendarDayOfWeek
            {
                get
                {
                    if (g_CalendarDayOfWeek == null)
                        g_CalendarDayOfWeek = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Editors.CalendarDayOfWeek)) as Style;

                    return g_CalendarDayOfWeek;
                }
            }

            #endregion //CalendarDayOfWeek

            #region CalendarItemArea

            private static Style g_CalendarItemArea;

            /// <summary>
            /// The style for a <see cref="Infragistics.Windows.Editors.CalendarItemArea"/> 
            /// </summary>
            public static Style CalendarItemArea
            {
                get
                {
                    if (g_CalendarItemArea == null)
                        g_CalendarItemArea = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Editors.CalendarItemArea)) as Style;

                    return g_CalendarItemArea;
                }
            }

            #endregion //CalendarItemArea

            #region CalendarItemGroupTitle

            private static Style g_CalendarItemGroupTitle;

            /// <summary>
            /// The style for a <see cref="Infragistics.Windows.Editors.CalendarItemGroupTitle"/> 
            /// </summary>
            public static Style CalendarItemGroupTitle
            {
                get
                {
                    if (g_CalendarItemGroupTitle == null)
                        g_CalendarItemGroupTitle = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Editors.CalendarItemGroupTitle)) as Style;

                    return g_CalendarItemGroupTitle;
                }
            }

            #endregion //CalendarItemGroupTitle

            #region CalendarDay

            private static Style g_CalendarDay;

            /// <summary>
            /// The style for a <see cref="Infragistics.Windows.Editors.CalendarDay"/> 
            /// </summary>
            public static Style CalendarDay
            {
                get
                {
                    if (g_CalendarDay == null)
                        g_CalendarDay = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Editors.CalendarDay)) as Style;

                    return g_CalendarDay;
                }
            }

            #endregion //CalendarDay

            #region XamMonthCalendar

            private static Style g_XamMonthCalendar;

            /// <summary>
            /// The style for a <see cref="Infragistics.Windows.Editors.XamMonthCalendar"/> 
            /// </summary>
            public static Style XamMonthCalendar
            {
                get
                {
                    if (g_XamMonthCalendar == null)
                        g_XamMonthCalendar = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Editors.XamMonthCalendar)) as Style;

                    return g_XamMonthCalendar;
                }
            }

            #endregion //XamMonthCalendar

            #region CalendarWeekNumber

            private static Style g_CalendarWeekNumber;

            /// <summary>
            /// The style for a <see cref="Infragistics.Windows.Editors.CalendarWeekNumber"/> 
            /// </summary>
            public static Style CalendarWeekNumber
            {
                get
                {
                    if (g_CalendarWeekNumber == null)
                        g_CalendarWeekNumber = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Editors.CalendarWeekNumber)) as Style;

                    return g_CalendarWeekNumber;
                }
            }

            #endregion //CalendarWeekNumber


			#region XamCheckEditor

		private static Style g_XamCheckEditor;

		/// <summary>
		/// The style for a <see cref="Infragistics.Windows.Editors.XamCheckEditor"/> 
		/// </summary>
		public static Style XamCheckEditor
		{
			get
			{
				if (g_XamCheckEditor == null)
					g_XamCheckEditor = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Editors.XamCheckEditor)) as Style;

				return g_XamCheckEditor;
			}
		}

			#endregion //XamCheckEditor

			#region XamTextEditor

		private static Style g_XamTextEditor;

		/// <summary>
		/// The style for a <see cref="Infragistics.Windows.Editors.XamTextEditor"/> 
		/// </summary>
		public static Style XamTextEditor
		{
			get
			{
				if (g_XamTextEditor == null)
					g_XamTextEditor = ResourceSet.GetSealedResource(ResourcesInternal, typeof(Infragistics.Windows.Editors.XamTextEditor)) as Style;

				return g_XamTextEditor;
			}
		}

			#endregion //XamTextEditor

		#endregion //Static Properties
	}

	#endregion //EditorsResourceSet<T> base class

	#region EditorsGeneric

	/// <summary>
	/// Class used to supply style resources for the Generic look for a editor elements that are shared
	/// </summary>
	public class EditorsGeneric : EditorsResourceSet<EditorsGeneric.Locator>
	{

		#region Instance static property

		private static EditorsGeneric g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsGeneric Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new EditorsGeneric();

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
			public override string Grouping { get { return EditorsGeneric.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Editors\EditorsGeneric_Express.xaml;ResourceSets\Editors\EditorsGeneric.xaml"; } }

		}
	}

	#endregion //EditorsGeneric

	#region EditorsOnyx

	/// <summary>
	/// Class used to supply style resources for the Onyx look for a editor elements that are shared
	/// </summary>
	public class EditorsOnyx : EditorsResourceSet<EditorsOnyx.Locator>
	{

		#region Instance static property

		private static EditorsOnyx g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsOnyx Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new EditorsOnyx();

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
			public override string Theme { get { return ThemeManager.ThemeNameOnyx; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return EditorsOnyx.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Editors\EditorsOnyx_Express.xaml;ResourceSets\Editors\EditorsOnyx.xaml"; } }

		}
	}

	#endregion //EditorsOnyx

	#region EditorsAero

	/// <summary>
	/// Class used to supply style resources for the Aero look for a editor elements that are shared
	/// </summary>
	public class EditorsAero : EditorsResourceSet<EditorsAero.Locator>
	{

		#region Instance static property

		private static EditorsAero g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsAero Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new EditorsAero();

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
			public override string Grouping { get { return EditorsAero.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Editors\EditorsAero_Express.xaml;ResourceSets\Editors\EditorsAero.xaml"; } }

		}
	}

	#endregion //EditorsAero

	#region EditorsRoyale

	/// <summary>
	/// Class used to supply style resources for the Royale look for a editor elements that are shared
	/// </summary>
	public class EditorsRoyale : EditorsResourceSet<EditorsRoyale.Locator>
	{

		#region Instance static property

		private static EditorsRoyale g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsRoyale Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new EditorsRoyale();

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
			public override string Grouping { get { return EditorsRoyale.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Editors\EditorsRoyale_Express.xaml;ResourceSets\Editors\EditorsRoyale.xaml"; } }

		}
	}

	#endregion //EditorsRoyale

	#region EditorsRoyaleStrong

	/// <summary>
	/// Class used to supply style resources for the RoyaleStrong look for a editor elements that are shared
	/// </summary>
	public class EditorsRoyaleStrong : EditorsResourceSet<EditorsRoyaleStrong.Locator>
	{

		#region Instance static property

		private static EditorsRoyaleStrong g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsRoyaleStrong Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new EditorsRoyaleStrong();

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
			public override string Theme { get { return ThemeManager.ThemeNameRoyaleStrong; } }
			/// <summary>The name of the grouping</summary>
			public override string Grouping { get { return EditorsRoyaleStrong.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Editors\EditorsRoyaleStrong_Express.xaml;ResourceSets\Editors\EditorsRoyaleStrong.xaml"; } }

		}
	}

	#endregion //EditorsRoyaleStrong

	#region EditorsLunaNormal

	/// <summary>
	/// Class used to supply style resources for the LunaNormal look for a editor elements that are shared
	/// </summary>
	public class EditorsLunaNormal : EditorsResourceSet<EditorsLunaNormal.Locator>
	{

		#region Instance static property

		private static EditorsLunaNormal g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsLunaNormal Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new EditorsLunaNormal();

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
			public override string Grouping { get { return EditorsLunaNormal.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Editors\EditorsLunaNormal_Express.xaml;ResourceSets\Editors\EditorsLunaNormal.xaml"; } }

		}
	}

	#endregion //EditorsLunaNormal

	#region EditorsLunaOlive

	/// <summary>
	/// Class used to supply style resources for the LunaOlive look for a editor elements that are shared
	/// </summary>
	public class EditorsLunaOlive : EditorsResourceSet<EditorsLunaOlive.Locator>
	{

		#region Instance static property

		private static EditorsLunaOlive g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsLunaOlive Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new EditorsLunaOlive();

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
			public override string Grouping { get { return EditorsLunaOlive.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Editors\EditorsLunaOlive_Express.xaml;ResourceSets\Editors\EditorsLunaOlive.xaml"; } }

		}
	}

	#endregion //EditorsLunaOlive

	#region EditorsLunaSilver

	/// <summary>
	/// Class used to supply style resources for the LunaSilver look for a editor elements that are shared
	/// </summary>
	public class EditorsLunaSilver : EditorsResourceSet<EditorsLunaSilver.Locator>
	{

		#region Instance static property

		private static EditorsLunaSilver g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsLunaSilver Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new EditorsLunaSilver();

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
			public override string Grouping { get { return EditorsLunaSilver.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Editors\EditorsLunaSilver_Express.xaml;ResourceSets\Editors\EditorsLunaSilver.xaml"; } }

		}
	}

	#endregion //EditorsLunaSilver

	#region EditorsOffice2k7Black

	/// <summary>
	/// Class used to supply style resources for the Office2k7Black look for a editor elements that are shared
	/// </summary>
	public class EditorsOffice2k7Black : EditorsResourceSet<EditorsOffice2k7Black.Locator>
	{

		#region Instance static property

		private static EditorsOffice2k7Black g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsOffice2k7Black Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new EditorsOffice2k7Black();

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
			public override string Grouping { get { return EditorsOffice2k7Black.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Editors\EditorsOffice2k7Black_Express.xaml;ResourceSets\Editors\EditorsOffice2k7Black.xaml"; } }

		}
	}

	#endregion //EditorsOffice2k7Black

	#region EditorsOffice2k7Blue

	/// <summary>
	/// Class used to supply style resources for the Office2k7Blue look for a editor elements that are shared
	/// </summary>
	public class EditorsOffice2k7Blue : EditorsResourceSet<EditorsOffice2k7Blue.Locator>
	{

		#region Instance static property

		private static EditorsOffice2k7Blue g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsOffice2k7Blue Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new EditorsOffice2k7Blue();

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
			public override string Grouping { get { return EditorsOffice2k7Blue.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Editors\EditorsOffice2k7Blue_Express.xaml;ResourceSets\Editors\EditorsOffice2k7Blue.xaml"; } }

		}
	}

	#endregion //EditorsOffice2k7Blue

	#region EditorsOffice2k7Silver

	/// <summary>
	/// Class used to supply style resources for the Office2k7Silver look for a editor elements that are shared
	/// </summary>
	public class EditorsOffice2k7Silver : EditorsResourceSet<EditorsOffice2k7Silver.Locator>
	{

		#region Instance static property

		private static EditorsOffice2k7Silver g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsOffice2k7Silver Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new EditorsOffice2k7Silver();

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
			public override string Grouping { get { return EditorsOffice2k7Silver.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Editors\EditorsOffice2k7Silver_Express.xaml;ResourceSets\Editors\EditorsOffice2k7Silver.xaml"; } }

		}
	}

	#endregion //EditorsOffice2k7Silver

	// JJD 8/30/10 - Added Office 2010 Blue theme
	#region EditorsOffice2010Blue

	/// <summary>
	/// Class used to supply style resources for the Office2010Blue look for a editor elements that are shared
	/// </summary>
	public class EditorsOffice2010Blue : EditorsResourceSet<EditorsOffice2010Blue.Locator>
	{

		#region Instance static property

		private static EditorsOffice2010Blue g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsOffice2010Blue Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new EditorsOffice2010Blue();

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
			public override string Grouping { get { return EditorsOffice2010Blue.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Editors\EditorsOffice2010Blue_Express.xaml;ResourceSets\Editors\EditorsOffice2010Blue.xaml"; } }

		}
	}

	#endregion //EditorsOffice2010Blue

	#region EditorsWashBaseDark

	/// <summary>
	/// Class used to supply style resources for the WashBaseDark look for a editor elements that are shared
	/// </summary>
	public class EditorsWashBaseDark : EditorsResourceSet<EditorsWashBaseDark.Locator>
	{

		#region Instance static property

		private static EditorsWashBaseDark g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsWashBaseDark Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new EditorsWashBaseDark();

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
			public override string Grouping { get { return EditorsWashBaseDark.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Editors\EditorsWashBaseDark_Express.xaml;ResourceSets\Editors\EditorsWashBaseDark.xaml"; } }

		}
	}

	#endregion //EditorsWashBaseDark

	#region EditorsWashBaseLight

	/// <summary>
	/// Class used to supply style resources for the WashBaseLight look for a editor elements that are shared
	/// </summary>
	public class EditorsWashBaseLight : EditorsResourceSet<EditorsWashBaseLight.Locator>
	{

		#region Instance static property

		private static EditorsWashBaseLight g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsWashBaseLight Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new EditorsWashBaseLight();

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
			public override string Grouping { get { return EditorsWashBaseLight.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Editors\EditorsWashBaseLight_Express.xaml;ResourceSets\Editors\EditorsWashBaseLight.xaml"; } }

		}
	}

	#endregion //EditorsRoyale


    #region EditorsPrintBasic

    /// <summary>
    /// Class used to supply style resources for the Print look for a editor elements that are shared
    /// </summary>
    public class EditorsPrintBasic : EditorsResourceSet<EditorsPrintBasic.Locator>
    {

        #region Instance static property

        private static EditorsPrintBasic g_Instance;

        /// <summary>
        /// Returns a static instance of this type (read-only)
        /// </summary>
        public static EditorsPrintBasic Instance
        {
            get
            {
                if (g_Instance == null)
                    g_Instance = new EditorsPrintBasic();

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
            public override string Theme { get { return ThemeManager.ThemeNamePrintBasic; } }
            /// <summary>The name of the grouping</summary>
            public override string Grouping { get { return EditorsPrintBasic.GroupingName; } }
            /// <summary>The path to the embedded resources within the assembly</summary>
            public override string ResourcePath { get { return @"ResourceSets\Editors\EditorsPrintBasic_Express.xaml;ResourceSets\Editors\EditorsPrintBasic.xaml"; } }
        }
    }

    #endregion //EditorsPrintBasic


	// JJD 10/29/10 - Added IGTheme
	#region EditorsIGTheme

	/// <summary>
	/// Class used to supply style resources for the IGTheme look for a editor elements that are shared
	/// </summary>
	public class EditorsIGTheme : EditorsResourceSet<EditorsIGTheme.Locator>
	{

		#region Instance static property

		private static EditorsIGTheme g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsIGTheme Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new EditorsIGTheme();

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
			public override string Grouping { get { return EditorsIGTheme.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Editors\EditorsIGTheme_Express.xaml;ResourceSets\Editors\EditorsIGTheme.xaml"; } }

		}
	}

	#endregion //EditorsIGTheme

	// JJD 02/16/12 - Added Metro
	#region EditorsMetro

	/// <summary>
	/// Class used to supply style resources for the Metro look for a editor elements that are shared
	/// </summary>
	public class EditorsMetro : EditorsResourceSet<EditorsMetro.Locator>
	{

		#region Instance static property

		private static EditorsMetro g_Instance;

		/// <summary>
		/// Returns a static instance of this type (read-only)
		/// </summary>
		public static EditorsMetro Instance
		{
			get
			{
				if (g_Instance == null)
					g_Instance = new EditorsMetro();

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
			public override string Grouping { get { return EditorsMetro.GroupingName; } }
			/// <summary>The path to the embedded resources within the assembly</summary>



			public override string ResourcePath { get { return @"ResourceSets\Editors\EditorsMetro_Express.xaml;ResourceSets\Editors\EditorsMetro.xaml"; } }

		}
	}

	#endregion //EditorsMetro
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