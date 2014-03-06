using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Input;
using System.Windows.Markup;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Diagnostics;
using Infragistics.Windows;
using Infragistics.Windows.Themes.Internal;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using Infragistics.Shared;

namespace Infragistics.Windows.Themes
{
	/// <summary>
	/// Static class used for registering resources based on theme and grouping
	/// </summary>
	/// <remarks>
	/// <para>Themes are identified by name (e.g. Onyx, Aero, Royale etc.)</para>
	/// <para>Groupings are also identified by name (e.g. Editors, Primitives, DataPresenter etc.).</para>
	/// </remarks>
	static public class ThemeManager
	{
		#region Private Members

		private static string g_currentTheme = ThemeNameGeneric;
		private static SortedList<string, ThemeInfo> g_themes = new SortedList<string, ThemeInfo>();
		private static ThemeInfo g_currentThemeInfo = new ThemeInfo(ThemeManager.ThemeCurrentLiteral);
		private static bool g_currentThemeInfoDirty = true;

		private static object g_EventLock = new object();
		private static EventHandler g_currentThemeChangedDelegate;
		private static EventHandler g_registrationInfoChangedDelegate;
		private static bool g_ignoreRegistrationInfoChanged;

		// version number for tracking when an assembly is loaded
		private static int g_loadedAssemblyVersion;

		// version number for tracking when the last loaded assembly was processed
		private static int g_lastProcessedAssemblyVersion;

		// list of assemblies that haven't been processed
		private static List<Assembly> g_assemblyList = new List<Assembly>();

		// object used to synchronize changes to the assembly list
		private static readonly object g_AssemblyLoadSyncObject = new object();

		// While testing the use of the assembly under intranet level rights we found 
		// that hooking the events of the AppDomain requires ControlAppDomain rights 
		// so we may not be able to hook the AssemblyLoaded event. To get around this
		// without putting the burden on every derived control, we added a static 
		// method named VerifyThemeInformationRegistered and called it from our
		// base constructor for ResourceSet. In the 
		// event that we were not able to hook the load event, we will keep a list of the
		// assemblies that have been processed and when invoked with one that was not
		// encountered, we add that to the assembly list that we were already maintaining.
		//
		private static readonly bool g_hookedAssemblyLoad;
		private static readonly Hashtable g_reviewedAssemblies;

		// AS 11/6/07 ThemeGroupingName
		private static List<string> g_loadedGroupingNames = new List<string>();
		private static List<Type> g_processedResourceSetTypes = new List<Type>();

		// AS 5/7/08
		// The original problem this was meant to address was a scenario where a customer put
		// one of our ResourceSets<T> into our control's Resources property (or within its 
		// MergedDictionaries) and then set the Theme property to a theme that used the same
		// ResourceDictionary somewhere within its mergeddictionaries change. When this happened
		// the WPF framework threw an exception because the RD maintains a list of the FE's
		// that reference it. We got around this by shallow cloning the registered dictionary
		// but this has its own downsides. For one, it means we force the hydration of the 
		// deferred resources. I.e. when the GetResourceSet is called, we enumerate the 
		// dictionary at which point the RD will ensure that any lazily hydrated items
		// are hydrated. For another, this doesn't help with other situations where the WPF
		// framework doesnt expect to see the same Style reference used for the local and
		// theme style values - in which case it doesn't bother invalidating the dynamic
		// resource references. In any case, we no longer need to clone the dictionaries
		// we hand back from GetResourceSet since we will be returning RDs that we create
		// using a registered ResourceSetLocator.
		//
		//// AS 12/3/07
		//private static Dictionary<ResourceDictionary, ResourceDictionary> g_clonedResources = new Dictionary<ResourceDictionary, ResourceDictionary>();

		// AS 5/9/08
		// Instead of defining the Theme property as a separate property on each Xam control
		// and since we don't necessarily have a common base class, the Theme property will now
		// be an attached property. This allows the ThemeManager to get a notification when the 
		// property is changed and centrally handle loading the appropriate resources into the 
		// element's Resource property. Also, the controls can just addowner this property so
		// we don't have to duplicate logic like the coercetheme override. In addition, we're
		// going to define the ThemeChanged event here. Note, we will not be firing the event
		// though because the elements need to do some processing of the property before the 
		// event can be raised (e.g. invalidatemeasure/updatelayout) so the controls' will 
		// need to raise this event. Since the theme manager will process the Theme property
		// change, it will need to know what groupings to use to obtain the resources to 
		// put into the element. I was going to handle this via another attached property
		// but it was decided that this really wouldn't/shouldn't be changed at a per instance
		// level so instead we're adding a RegisterGroupings method that classes that use
		// the Theme property must use to register the groupings to apply to the element.
		// Since the Theme property is being managed here we can also handle hooking the 
		// Loaded/Unloaded event and remove the resources from the element while it is unloaded.
		// We need to do this to prevent the element from potentially being rooted. When a 
		// ResourceDictionary is added to the Resources collection of an element, the RD 
		// and all of its contained MergedDictionaries are updated to contain a reference
		// to the "Owning" Framework(Content)Element. So if the RD is added to 2 elements
		// (because their Theme property is set to the same value), if one of the elements
		// is unloaded, it will remain in memory because the RD that it contains within 
		// its Resources collection has a reference back to it and that RD is referenced
		// by the other element that is still loaded. Previously this was more of a problem
		// because the RD was the Static instance that we maintained a reference too. We 
		// no longer do that but there still is the possibility of the elements being rooted
		// as long as one of the referencing elements is still alive.
		//
		private static readonly object g_RegisteredGroupingsSyncObject = new object();
		private static Dictionary<Type, string[]> g_registeredGroupings = new Dictionary<Type, string[]>();

		#endregion //Private Members

		#region Static Constructor

		/// <summary>
		/// Static constructor
		/// </summary>
		static ThemeManager()
		{
			System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

			// Store the currently loaded assemblies so we can process them when requested.
			//
			foreach (System.Reflection.Assembly assembly in assemblies)
				ThemeManager.g_assemblyList.Add(assembly);

			// increment the counter
			ThemeManager.g_loadedAssemblyVersion++;

			// AS 5/25/06
			try
			{
				System.Security.Permissions.SecurityPermission perm = new System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityPermissionFlag.ControlAppDomain);

				try
				{
					perm.Assert();
				}
				catch (InvalidOperationException)
				{
				}

				HookAssemblyLoad();

				ThemeManager.g_hookedAssemblyLoad = true;
			}
			catch (System.Security.SecurityException)
			{
				ThemeManager.g_hookedAssemblyLoad = false;
				ThemeManager.g_reviewedAssemblies = new Hashtable();

				// keep track of the reviewed assemblies
				for (int i = 0; i < assemblies.Length; i++)
					ThemeManager.g_reviewedAssemblies.Add(assemblies[i], null);
			}
		}

		#endregion //Static Constructor

		#region Constants

		/// <summary>
		/// The literal used to represent all resource sets for a theme (read-only)
		/// </summary>
		public const string AllGroupingsLiteral = "*";
		/// <summary>
		/// The literal used to represent a null theme value in a property grid (read-only)
		/// </summary>
		public const string ThemeDefaultLiteral = "[default]";
		/// <summary>
		/// The literal used to represent the current theme (read-only)
		/// </summary>
		public const string ThemeCurrentLiteral = "[current]";
		/// <summary>
		/// The name of the Generic theme (read-only)
		/// </summary>
		public const string ThemeNameGeneric = "Generic";
		/// <summary>
		/// The name of the Aero theme (read-only)
		/// </summary>
		public const string ThemeNameAero = "Aero";
		/// <summary>
		/// The name of the Onyx theme (read-only)
		/// </summary>
		public const string ThemeNameOnyx = "Onyx";
		/// <summary>
		/// The name of the Royale theme (read-only)
		/// </summary>
		public const string ThemeNameRoyale = "Royale";
		/// <summary>
		/// The name of the Royale theme (read-only)
		/// </summary>
		public const string ThemeNameRoyaleStrong = "RoyaleStrong";
		/// <summary>
		/// The name of the Hulk theme (read-only)
		/// </summary>
		public const string ThemeNameForestGreen = "ForestGreen";
		/// <summary>
		/// The name of the Leaf theme (read-only)
		/// </summary>
		public const string ThemeNameLeaf = "Leaf";
		/// <summary>
		/// The name of the Lipstick theme (read-only)
		/// </summary>
		public const string ThemeNameLipstick = "Lipstick";
		
		/// <summary>
		/// The name of the LunaNormal theme (read-only)
		/// </summary>
		public const string ThemeNameLunaNormal = "LunaNormal";

		/// <summary>
		/// The name of the LunaOlive theme (read-only)
		/// </summary>
		public const string ThemeNameLunaOlive = "LunaOlive";

		/// <summary>
		/// The name of the LunaSilver theme (read-only)
		/// </summary>
		public const string ThemeNameLunaSilver = "LunaSilver";

		// JJD 2/16/12 - Added Metro theme
		/// <summary>
		/// The name of the Metro theme (read-only)
		/// </summary>
		public const string ThemeNameMetro = "Metro";

		// JJD 8/30/10 - Added Office 2010 Blue theme
		/// <summary>
		/// The name of the Office 2010 blue theme (read-only)
		/// </summary>
		public const string ThemeNameOffice2010Blue = "Office2010Blue";

		/// <summary>
		/// The name of the Office 2007 blue theme (read-only)
		/// </summary>
		public const string ThemeNameOffice2k7Blue = "Office2k7Blue";

		/// <summary>
		/// The name of the Office 2007 black theme (read-only)
		/// </summary>
		public const string ThemeNameOffice2k7Black = "Office2k7Black";

		/// <summary>
		/// The name of the Office 2007 silver theme (read-only)
		/// </summary>
		public const string ThemeNameOffice2k7Silver = "Office2k7Silver";

        // JJD 11/28/07 - added HighContrast theme name
		/// <summary>
        /// The name of the HighContrastLight theme (read-only)
		/// </summary>
        public const string ThemeNameHighContrast = "HighContrast";

        // TH 11/29/07 - added ThemeBrushesLight theme name
	   /// <summary>
	   /// The name of the ThemeBrushesLight theme (read-only)
	   /// </summary>
		public const string ThemeNameWashBaseLight = "WashBaseLight";

		// TH 11/29/07 - added ThemeBrushesDark theme name
	   /// <summary>
		/// The name of the ThemeBrushesDark theme (read-only)
	   /// </summary>
		public const string ThemeNameWashBaseDark = "WashBaseDark";


        /// <summary>
        /// The name of the Print theme (read-only)
        /// </summary>
        public const string ThemeNamePrintBasic = "Print Basic";


		// JJD 10/29/10 - Added IGTheme
		/// <summary>
		/// The name of the IG theme (read-only)
		/// </summary>
		public const string ThemeNameIGTheme = "IGTheme";

        #endregion //Constants

		#region Events

			#region CurrentThemeChanged event

		/// <summary>
		/// Occurs when the <see cref="CurrentTheme"/> property has been changed.
		/// </summary>
		public static event EventHandler CurrentThemeChanged
		{
			add
			{
				lock (g_EventLock)
				{
					if (g_currentThemeChangedDelegate == null)
						g_currentThemeChangedDelegate = value;
					else
						g_currentThemeChangedDelegate = (EventHandler)System.Delegate.Combine(g_currentThemeChangedDelegate, value);
				}
			}
			remove
			{
				lock (g_EventLock)
				{
					if (g_currentThemeChangedDelegate != null)
						g_currentThemeChangedDelegate = (EventHandler)System.Delegate.Remove(g_currentThemeChangedDelegate, value);
				}
			}
		}

			#endregion //CurrentThemeChanged event

			#region RegistrationInfoChanged event

		/// <summary>
		/// Occurs when the <see cref="RegistrationInfoChanged"/> property has been changed.
		/// </summary>
		public static event EventHandler RegistrationInfoChanged
		{
			add
			{
				lock (g_EventLock)
				{
					if (g_registrationInfoChangedDelegate == null)
						g_registrationInfoChangedDelegate = value;
					else
						g_registrationInfoChangedDelegate = (EventHandler)System.Delegate.Combine(g_registrationInfoChangedDelegate, value);
				}
			}
			remove
			{
				lock (g_EventLock)
				{
					if (g_registrationInfoChangedDelegate != null)
						g_registrationInfoChangedDelegate = (EventHandler)System.Delegate.Remove(g_registrationInfoChangedDelegate, value);
				}
			}
		}

			#endregion //RegistrationInfoChanged event

			#region ThemeChangedEvent

		// AS 5/9/08
		// While we're not going to fire the event, we do want to define it here to allow the same RoutedEvent
		// instance be able to be used to catch the event for all elements as well as to prevent duplication of
		// code.
		//
		/// <summary>
		/// Routed event for raising a notification when the <see cref="ThemeManager.ThemeProperty"/> has been changed.
		/// </summary>
		/// <remarks>
		/// <p class="body">An element must raise this event when its Theme property has been changed. This is normally 
		/// done within the PropertyChangedCallback added by overriding the metadata for the ThemeProperty.</p>
		/// </remarks>
		public static readonly RoutedEvent ThemeChangedEvent = EventManager.RegisterRoutedEvent(
			"ThemeChanged",
			RoutingStrategy.Bubble,
			typeof(RoutedPropertyChangedEventHandler<string>),
			typeof(ThemeManager)); 

			#endregion //ThemeChangedEvent

		#endregion //Events

		#region Properties

			#region Public Properties

				#region CurrentTheme

		/// <summary>
		/// Gets/sets the current theme
		/// </summary>
		/// <seealso cref="CurrentThemeChanged"/>
		public static string CurrentTheme
		{
			get
			{
				return g_currentTheme;
			}
			set
			{
				if (g_currentTheme != value)
				{
					if ( value == ThemeManager.ThemeDefaultLiteral )
						value = null;

					if (value != null)
					{
						ThemeInfo themeInfo = GetThemeInfo(value, false);

						if (themeInfo == null)
							throw new ArgumentException( SR.GetString( "LE_ArgumentException_6", value ) );
					}

					g_currentTheme = value;

					g_currentThemeInfoDirty = true;

					VerifyCurrentThemeInfo();

					if (g_currentThemeChangedDelegate != null)
						g_currentThemeChangedDelegate(null, EventArgs.Empty);
				}
			}
		}

				#endregion //CurrentTheme

				#region Theme

		// AS 5/9/08
		// See the comment at the top of this class for full details but basically we're going to define
		// the Theme property here so we don't have to duplicate it on all classes that have a Theme 
		// property but also to centralize the handling of this property change to within this class.
		//
		/// <summary>
		/// Identifies the Theme attached dependency property
		/// </summary>
		/// <seealso cref="GetTheme"/>
		/// <seealso cref="SetTheme"/>
		public static readonly DependencyProperty ThemeProperty = DependencyProperty.RegisterAttached("Theme",
			typeof(string), typeof(ThemeManager), 
			new FrameworkPropertyMetadata(null, 
				new PropertyChangedCallback(OnThemeChanged), 
				new CoerceValueCallback(CoerceTheme)));

		private static object CoerceTheme(DependencyObject target, object value)
		{
			if (value is string)
			{
				if (string.Compare((string)value, ThemeManager.ThemeDefaultLiteral, true) == 0)
					return null;

				if (((string)value).Length == 0)
					return null;
			}

			return value;
		}

		private static void OnThemeChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			if (target is FrameworkElement)
			{
				Type elementType = target.GetType();
				string[] groupings = GetGroupingsForType(elementType);

				if (groupings == null)
                    throw new InvalidOperationException(SR.GetString("LE_ThemeManagerRegisterGroupings", new object[] { elementType.FullName }));

				ThemeManager.OnThemeChanged(target as FrameworkElement, (string)e.NewValue, groupings);
			}
		}

		/// <summary>
		/// Gets the value of the 'Theme' attached property
		/// </summary>
		/// <remarks>
		/// <para class="body">If left set to null then the default 'Generic' theme will be used. 
		/// This property can be set to the name of any registered theme (see <see cref="Infragistics.Windows.Themes.ThemeManager.Register(string, string, ResourceDictionary)"/> and <see cref="Infragistics.Windows.Themes.ThemeManager.GetThemes()"/> methods).</para>
		/// <para></para>
		/// <para class="note"><b>Note: </b> The following themes are pre-registered by this assembly but additional themes can be registered as well.
		/// <ul>
		/// <li>"Aero" - a theme that is compatible with Vista's 'Aero' theme.</li>
		/// <li>"Generic" - the default theme.</li>
		/// <li>"LunaNormal" - a theme that is compatible with XP's 'blue' theme.</li>
		/// <li>"LunaOlive" - a theme that is compatible with XP's 'olive' theme.</li>
		/// <li>"LunaSilver" - a theme that is compatible with XP's 'silver' theme.</li>
		/// <li>"Office2k7Black" - a theme that is compatible with MS Office 2007's 'Black' theme.</li>
		/// <li>"Office2k7Blue" - a theme that is compatible with MS Office 2007's 'Blue' theme.</li>
		/// <li>"Office2k7Silver" - a theme that is compatible with MS Office 2007's 'Silver' theme.</li>
		/// <li>"Onyx" - a theme that features black and orange highlights.</li>
		/// <li>"Royale" - a theme that features subtle blue highlights.</li>
		/// <li>"RoyaleStrong" - a theme that features strong blue highlights.</li>
		/// </ul>
		/// </para>
		/// </remarks>
		/// <seealso cref="Infragistics.Windows.Themes.ThemeManager"/>
		/// <seealso cref="ThemeProperty"/>
		/// <seealso cref="SetTheme"/>
		public static string GetTheme(DependencyObject d)
		{
			return (string)d.GetValue(ThemeManager.ThemeProperty);
		}

		/// <summary>
		/// Sets the value of the 'Theme' attached property
		/// </summary>
		/// <seealso cref="ThemeProperty"/>
		/// <seealso cref="GetTheme"/>
		public static void SetTheme(DependencyObject d, string value)
		{
			d.SetValue(ThemeManager.ThemeProperty, value);
		}

				#endregion //Theme

			#endregion //Public Properties

			#region Private Properties

			#endregion //Private Properties

		#endregion //Properties

		#region Public Methods

			#region GetGroupingsForType

		// AS 5/9/08
		// Since we're going to handle the Theme property change here, we'll need to know what 
		// groupings should be used so we know what resourcesets to put into the element's
		// Resources collection when its Theme property is set. This method is used to get 
		// the groupings for a type or one of its base types.
		//
		/// <summary>
		/// Returns the registered groupings for the specified type
		/// </summary>
		/// <param name="type">The type whose groupings are to be returned.</param>
		/// <returns>An array of the registered groupings for the type or its base type if it has been registered; null if no groupings have been registered for the element.</returns>
		public static string[] GetGroupingsForType(Type type)
		{
			string[] groupings = GetGroupingsForTypeImpl(type);

			// make a copy since we don't want anyone changing the array
			if (null != groupings)
				groupings = (string[])groupings.Clone();

			return groupings;
		}

		/// <summary>
		/// Returns the registered groupings for the specified type
		/// </summary>
		/// <param name="type">The type whose groupings are to be returned.</param>
		/// <returns>An array of the registered groupings for the type or its base type if it has been registered; null if no groupings have been registered for the element.</returns>
		private static string[] GetGroupingsForTypeImpl(Type type)
		{
			string[] groupings = null;

			// NOTE: the private version does not copy the array but for the public one we
			// need to copy it so it can't be manipulated.
			lock (g_RegisteredGroupingsSyncObject)
			{
				while (null != type)
				{
					if (g_registeredGroupings.TryGetValue(type, out groupings))
						return groupings;

					type = type.BaseType;
				}
			}

			return groupings;
		} 
			#endregion //GetGroupingsForType

			#region GetGroupingsForTheme

		/// <summary>
		/// Returns a list of all registered groupings for a specific theme
		/// </summary>
		/// <param name="theme">The specified theme.</param>
		public static string[] GetGroupingsForTheme(string theme)
		{
			ThemeInfo themeInfo = ThemeManager.GetThemeInfo(theme, false);

			if (themeInfo == null)
				return new string[0];

			Monitor.Enter(typeof(ThemeManager));

			try
			{
				
#region Infragistics Source Cleanup (Region)





#endregion // Infragistics Source Cleanup (Region)

				return themeInfo.GetGroupingNames();
			}
			finally
			{
				Monitor.Exit(typeof(ThemeManager));
			}
		}

			#endregion //GetGroupingsForTheme

			#region GetResourceSet

		/// <summary>
		/// Registers a ResourceDictionary containing resources for a specific theme and grouping
		/// </summary>
		/// <param name="theme">The name of the theme (e.g. Onyx, Aero, Royale etc.)</param>
		/// <param name="grouping">The name of the grouping (e.g. Editors, Primitives, DataPresenter etc.)</param>
		/// <returns>The Resourcedictioanry containing the associated styles or null if not registered.</returns>
		/// <seealso cref="Register(string, string, ResourceDictionary)"/>
		public static ResourceDictionary GetResourceSet(string theme, string grouping)
		{
			ThemeInfo themeInfo = ThemeManager.GetThemeInfo(theme, true);

			if (themeInfo == null)
				return null;

			if (grouping == null)
				throw new ArgumentNullException("grouping");

			if (grouping.Length == 0)
				throw new ArgumentException( SR.GetString( "LE_ArgumentException_7" ) );

			Monitor.Enter(typeof(ThemeManager));

			try
			{
				// call verify if this is the current theme
				if (themeInfo == g_currentThemeInfo)
					VerifyCurrentThemeInfo();

				// convert the name to lowercase to use as a key since we want the keys
				// to be case insensitive
				string groupingKey = grouping.ToLower();

				
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

				
#region Infragistics Source Cleanup (Region)













#endregion // Infragistics Source Cleanup (Region)

				return themeInfo.GetResourceSet(groupingKey, true);
			}
			finally
			{
				Monitor.Exit(typeof(ThemeManager));
			}
		}

			#endregion //GetResourceSet

			#region GetThemes

		/// <summary>
		/// Returns a list of all registered themes
		/// </summary>
		public static string[] GetThemes()
		{
			return GetThemes(false);
		}

		/// <summary>
		/// Returns a list of all registered themes
		/// </summary>
		/// <param name="includeDefault">A boolean indicating whether to include the default theme</param>
		public static string[] GetThemes(bool includeDefault)
		{
			return GetThemes(includeDefault, AllGroupingsLiteral);
		}

		// AS 5/12/08
		// Added an overload that could be used from samples, etc. to get just the themes that apply
		// to a particular grouping. This is esp important for chart where we had a sample that listed
		// the chart themes. Previously it used its enum to get that list but now that its a string
		// property, we need to get it from the theme manager but we don't want to include themes
		// that will not affect it.
		//
		/// <summary>
		/// Returns a list of all registered themes
		/// </summary>
		/// <param name="includeDefault">A boolean indicating whether to include the default theme</param>
		/// <param name="grouping">The name of the grouping which must be registered for a theme in order to be returned.</param>
		public static string[] GetThemes(bool includeDefault, string grouping)
		{
			ProcessLoadedAssemblies();

			Monitor.Enter(typeof(ThemeManager));

			try
			{
				int count = g_themes.Count;

				
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

				
#region Infragistics Source Cleanup (Region)














#endregion // Infragistics Source Cleanup (Region)

				List<string> themeNames = new List<string>();
				IList<ThemeInfo> themes = g_themes.Values;

				if (includeDefault)
					themeNames.Add(ThemeManager.ThemeDefaultLiteral);

				if (null != grouping)
					grouping = grouping.ToLower();

				for (int i = 0; i < count; i++)
				{
					ThemeInfo theme = themes[i];

					if (theme.HasGrouping(grouping))
						themeNames.Add(theme.Name);
				}

				return themeNames.ToArray();
			}
			finally
			{
				Monitor.Exit(typeof(ThemeManager));
			}
		}

			#endregion //GetThemes

			#region IsThemeValid

		/// <summary>
		/// Determines if a theme is valid.
		/// </summary>
		/// <param name="theme">The name of the theme (e.g. Onyx, Aero, Royale etc.)</param>
		/// <returns>True if the theme has been registered or is equal to '[default]' or '[current]'.</returns>
		/// <seealso cref="Register(string, string, ResourceDictionary)"/>
		public static bool IsThemeValid(string theme)
		{
			if (theme == null || theme.Length == 0)
				return false;

			ThemeInfo themeInfo = ThemeManager.GetThemeInfo(theme, true);

			return themeInfo != null;
		}

			#endregion //IsThemeValid

			#region OnThemeChanged

		/// <summary>
		/// Helper method called by controls that expose a 'Theme' property to merge the appropriate resources for the specified theme.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> the registered theme resource sets ar injected into the MergedDictionaries collection of the control's Resources.
		/// </para></remarks>
		/// <param name="themedControl">The control that exposes the 'Theme' property.</param>
		/// <param name="theme">The new value of the 'Theme' property.</param>
		/// <param name="groupings">An array of the appropriate theme groupings to include.</param>
		/// <exception cref="ArgumentNullException">If themedControl or groupings is null.</exception>
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public static void OnThemeChanged(FrameworkElement themedControl, string theme, string[] groupings)
		{
			
#region Infragistics Source Cleanup (Region)

































































#endregion // Infragistics Source Cleanup (Region)

			ControlThemeInfo.ProcessThemeChanged(themedControl, theme, groupings);
		}

			#endregion //OnThemeChanged	
    
			#region Register

		#region Old Version
		
#region Infragistics Source Cleanup (Region)




























































































































#endregion // Infragistics Source Cleanup (Region)

		#endregion //Old Version

		/// <summary>
		/// Registers a ResourceDictionary containing resources for a specific theme and grouping
		/// </summary>
		/// <param name="theme">The name of the theme (e.g. Onyx, Aero, Royale etc.)</param>
		/// <param name="grouping">The name of the grouping (e.g. Editors, Primitives, DataPresenter etc.)</param>
		/// <param name="resources">The ResourceDictioanry containing the associated styles.</param>
		/// <seealso cref="GetResourceSet"/>
		public static void Register(string theme, string grouping, ResourceDictionary resources)
		{
			Utilities.ThrowIfNull(resources, "resources");

			Register(theme, grouping, (object)resources);
		}

		/// <summary>
		/// Registers a <see cref="ResourceSetLocator"/> containing resources for a specific theme and grouping
		/// </summary>
		/// <param name="locator">The <see cref="ResourceSetLocator"/> containing the information about the theme, grouping and location of the resources.</param>
		/// <seealso cref="GetResourceSet"/>
		public static void Register(ResourceSetLocator locator)
		{
			if (locator.Assembly == null)
                throw new ArgumentException(SR.GetString("LE_ThemeManagerLocatorAssembly"));

			Utilities.ThrowIfNull(locator, "locator");

			Register(locator.Theme, locator.Grouping, (object)locator);
		}

		private static void Register(string theme, string grouping, object objectToRegister)
		{
			Utilities.ThrowIfNull(theme, "theme");
			Utilities.ThrowIfNull(grouping, "grouping");
			Utilities.ThrowIfNull(objectToRegister, "objectToRegister");

			if (theme.Length == 0)
				throw new ArgumentException("theme");

			if (grouping.Length == 0)
				throw new ArgumentException("grouping");

			if (objectToRegister is ResourceDictionary == false && objectToRegister is ResourceSetLocator == false)
				throw new ArgumentException("objectToRegister");

			// AS 5/7/08
			// The following used to be within the monitor but there is no need to enter
			// the monitor if the information is invalid.
			//
			// convert the name to lowercase to use as a key since we want the keys
			// to be case insensitive
			string themeKey = theme.ToLower();

			if (themeKey == ThemeDefaultLiteral ||
				 themeKey == ThemeCurrentLiteral)
				throw new ArgumentException( SR.GetString( "LE_ArgumentException_8" ) );

			// convert the name to lowercase to use as a key since we want the keys
			// to be case insensitive
			string groupingKey = grouping.ToLower();

			if (groupingKey == AllGroupingsLiteral)
				throw new ArgumentException( SR.GetString( "LE_ArgumentException_9" ) );

			bool changesRegistered = false;
			ThemeInfo themeInfo = null;

			Monitor.Enter(typeof(ThemeManager));

			try
			{
				bool themeFound = g_themes.TryGetValue(themeKey, out themeInfo);

				// if the theme wasn't registered previously then add it now
				if (!themeFound)
				{
					themeInfo = new ThemeInfo(theme);
					g_themes.Add(themeKey, themeInfo);
					changesRegistered = true;
				}

				if (objectToRegister is ResourceSetLocator)
					changesRegistered = themeInfo.Register(groupingKey, (ResourceSetLocator)objectToRegister);
				else
					changesRegistered = themeInfo.Register(groupingKey, (ResourceDictionary)objectToRegister);
			}
			finally
			{
				Monitor.Exit(typeof(ThemeManager));

				if (changesRegistered)
				{
					// AS 5/7/08
					// The themeInfo handles this itself.
					//
					//themeInfo.DirtyAllGroupings();

					// AS 10/18/10 TFS57528
					//if (themeInfo.Name == g_currentThemeInfo.Name)
					if (themeInfo.Name == g_currentTheme)
						g_currentThemeInfoDirty = true;

					if (!g_ignoreRegistrationInfoChanged)
					{
						themeInfo.VerifyAllGroupings();
						
						VerifyCurrentThemeInfo();

						// raise the event to let listeners know that changes have been made
						if (g_registrationInfoChangedDelegate != null)
							g_registrationInfoChangedDelegate(null, EventArgs.Empty);
					}
				}
			}
		}
			#endregion //Register

			#region RegisterGroupings

		// AS 5/9/08
		// Since we're going to handle the Theme property change here, we'll need to know what 
		// groupings should be used so we know what resourcesets to put into the element's
		// Resources collection when its Theme property is set. This method should be called
		// once in the static ctor of classes that define/use the Theme property.
		//
		/// <summary>
		/// Registers the name of the Theme groupings that should be applied to a type when its Theme property is changed.
		/// </summary>
		/// <param name="type">The type for which to register the groupings</param>
		/// <param name="groupings">An array of 1 or more strings that represent the groupings which should be added to the element's resources when its Theme property is changed.</param>
		public static void RegisterGroupings(Type type, string[] groupings)
		{
			Utilities.ThrowIfNull(type, "type");
			Utilities.ThrowIfNull(groupings, "groupings");

			lock (g_RegisteredGroupingsSyncObject)
			{
				if (g_registeredGroupings.ContainsKey(type))
                    throw new InvalidOperationException(SR.GetString("LE_ThemeManagerGroupingsAlreadyRegistered", new object[] { type.FullName }));

				string[] groupingsCopy = (string[])groupings.Clone();

				// verify the groupings
				foreach (string grouping in groupingsCopy)
				{
					if (string.IsNullOrEmpty(grouping))
                        throw new ArgumentException(SR.GetString("LE_ThemeManagerGroupingsCannotBeNull"), "groupings");
                }

				// store the groupings
				g_registeredGroupings[type] = groupings;
			}
		} 
			#endregion //RegisterGroupings

			#region VerifyThemeInformationRegistered

		/// <summary>
		/// This member supports the Infragistics infrastructure and is not meant to be invoked externally.
		/// </summary>
		/// <param name="assembly">Type whose assembly style information should be evaluated.</param>
		public static void VerifyThemeInformationRegistered(Assembly assembly)
		{
			ProcessLoadedAssemblies();

			if (null != assembly &&
				ThemeManager.g_hookedAssemblyLoad == false &&
				ThemeManager.g_ignoreRegistrationInfoChanged == false)
			{
				if (ThemeManager.g_reviewedAssemblies.ContainsKey(assembly) == false)
				{
					ThemeManager.g_reviewedAssemblies.Add(assembly, null);
					ThemeManager.OnAssemblyLoaded(assembly);
				}
			}
		}
		
			#endregion //VerifyThemeInformationRegistered

		#endregion //Public Methods

		#region Private Methods

			#region Clone


#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		internal static ResourceDictionary Clone(ResourceDictionary source)
		{
			ResourceSet set = source as ResourceSet;

			if (null != set)
				set.VerifyResources();

			ResourceWasher washer = source as ResourceWasher;

			if (null != washer)
				washer.VerifyThemeAccess();

			ResourceDictionary clone = new ResourceDictionary();

			foreach (DictionaryEntry entry in source)
				clone.Add(entry.Key, entry.Value);

			// loop over the source's merged dictionaries cloning each one
			foreach (ResourceDictionary mergedDictionary in source.MergedDictionaries)
			{
				ResourceDictionary mergedClone = Clone(mergedDictionary);

				clone.MergedDictionaries.Add(mergedClone);
			}

			return clone;
		}
			#endregion //Clone

			#region CreateAssemblyInfo


#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		private static AssemblyResourceSetLoader CreateAssemblyInfo(Type assemblyResourceSetLoaderType)
		{
			Debug.Assert(null != assemblyResourceSetLoaderType &&
				typeof(AssemblyResourceSetLoader).IsAssignableFrom(assemblyResourceSetLoaderType) &&
				assemblyResourceSetLoaderType.IsAbstract == false, "The type must be a creatable 'AssemblyResourceSetLoader' type!");

			try
			{
				return Activator.CreateInstance(assemblyResourceSetLoaderType) as AssemblyResourceSetLoader;
			}
			catch (Exception ex)
			{
				Debug.Fail("Error while trying to create an AssemblyResourceSetLoader!", ex.Message);
				return null;
			}
		}

			#endregion //CreateAssemblyInfo

			#region CreateAssemblyResourceSetLoaders

		// AS 11/6/07 ThemeGroupingName
		//private static AssemblyResourceSetLoader[] CreateAssemblyResourceSetLoaders(Type[] assemblyStyleResourceSetLoaderTypes)
		private static void CreateAssemblyResourceSetLoaders(List<AssemblyResourceSetLoader> list, List<Type> assemblyStyleResourceSetLoaderTypes)
		{
			// AS 11/6/07 ThemeGroupingName
			//List<AssemblyResourceSetLoader> list = new List<AssemblyResourceSetLoader>();

			if (null != assemblyStyleResourceSetLoaderTypes)
			{
				// iterate the list
				foreach (Type assemblyResourceSetLoaderType in assemblyStyleResourceSetLoaderTypes)
				{
					// create the associated metadata definition class
					AssemblyResourceSetLoader assemblyInfo = CreateAssemblyInfo(assemblyResourceSetLoaderType);

					if (null != assemblyInfo)
						list.Add(assemblyInfo);
				}
			}

			// AS 11/6/07 ThemeGroupingName
			//return list.ToArray();
		}
			#endregion //CreateAssemblyResourceSetLoaders

			#region GetAssemblyResourceSetLoaderTypes

		private static void GetAssemblyResourceSetLoaderTypes(List<Type> list, System.Reflection.Assembly assembly)
		{
			try
			{
				// AssemblyBuilder derived assemblies throw an exception
				// when you try to get the exported types so skip them.
				//
				if (assembly.GetType() == typeof(System.Reflection.Emit.AssemblyBuilder))
					return;

				// if this assembly is loaded as reflection only then bypass it since we can't instaitae the 
				// types anyway
				if (assembly.ReflectionOnly)
					return;

				object[] attribs = assembly.GetCustomAttributes(typeof(AssemblyResourceSetLoaderAttribute), false);

				if (attribs != null && attribs.Length > 0)
					list.Add(((AssemblyResourceSetLoaderAttribute)attribs[0]).AssemblyResourceSetLoaderType);

			}
			catch (Exception ex)
			{
				Debug.WriteLine(new String('*', 40));
				Debug.WriteLine(ex.Message, "GetAssemblyResourceSetLoader Error");
				Debug.WriteLine(new String('*', 40));
			}
		}
			#endregion //GetAssemblyResourceSetLoaderTypes

			// AS 11/6/07 ThemeGroupingName
			#region GetAssemblyThemeGroupingName

		private static void GetAssemblyThemeGroupingName(List<string> list, System.Reflection.Assembly assembly)
		{
			try
			{
				// AssemblyBuilder derived assemblies throw an exception
				// when you try to get the exported types so skip them.
				//
				if (assembly.GetType() == typeof(System.Reflection.Emit.AssemblyBuilder))
					return;

				// if this assembly is loaded as reflection only then bypass it since we can't instaitae the 
				// types anyway
				if (assembly.ReflectionOnly)
					return;

				object[] attribs = assembly.GetCustomAttributes(typeof(AssemblyThemeGroupingNameAttribute), false);

				if (attribs != null && attribs.Length > 0)
					list.Add(((AssemblyThemeGroupingNameAttribute)attribs[0]).Name);

			}
			catch (Exception ex)
			{
				Debug.WriteLine(new String('*', 40));
				Debug.WriteLine(ex.Message, "GetAssemblyThemeGroupingName Error");
				Debug.WriteLine(new String('*', 40));
			}
		}
			#endregion //GetAssemblyThemeGroupingName

			#region GetLoadedAssemblyResourceSetLoaderTypes
		
#region Infragistics Source Cleanup (Region)















#endregion // Infragistics Source Cleanup (Region)

			#endregion //GetLoadedAssemblyResourceSetLoaderTypess

			#region GetThemeInfo

		private static ThemeInfo GetThemeInfo(string theme, bool allowSpecialLiterals)
		{
			if (theme == null)
				throw new ArgumentNullException("theme");

			if (theme.Length == 0)
				throw new ArgumentException("theme");

			ProcessLoadedAssemblies();

			Monitor.Enter(typeof(ThemeManager));

			try
			{
				// convert the name to lowercase to use as a key since we want the keys
				// to be case insensitive
				string themeKey = theme.ToLower();

				if (allowSpecialLiterals == true)
				{
					switch (themeKey)
					{
						case ThemeManager.ThemeCurrentLiteral:
							return g_currentThemeInfo;

						case ThemeManager.ThemeDefaultLiteral:
							themeKey = ThemeManager.ThemeNameGeneric;
							themeKey = theme.ToLower();
							break;

					}
				}

				ThemeInfo themeInfo;

				if (false == g_themes.TryGetValue(themeKey, out themeInfo))
					return null;

				return themeInfo;
			}
			finally
			{
				Monitor.Exit(typeof(ThemeManager));
			}
		}

			#endregion //GetThemeInfo

			#region HookAssemblyLoad

		private static void HookAssemblyLoad()
		{
			// we need to know when an assembly is loaded so we can search
			// for new node definitions
			AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(OnAssemblyLoaded);
		}

			#endregion //HookAssemblyLoad

			#region OnAssemblyLoaded

		private static void OnAssemblyLoaded(object sender, AssemblyLoadEventArgs e)
		{
			OnAssemblyLoaded(e.LoadedAssembly);
		}

		private static void OnAssemblyLoaded(System.Reflection.Assembly loadedAssembly)
		{
			try
			{
				System.Threading.Monitor.Enter(g_AssemblyLoadSyncObject);

				ThemeManager.g_assemblyList.Add(loadedAssembly);

				// increment the counter
				ThemeManager.g_loadedAssemblyVersion++;
			}
			finally
			{
				System.Threading.Monitor.Exit(g_AssemblyLoadSyncObject);
			}
		}

			#endregion //OnAssemblyLoaded

			#region ProcessLoadedAssemblies

		private static void ProcessLoadedAssemblies()
		{
			if (ThemeManager.g_lastProcessedAssemblyVersion != ThemeManager.g_loadedAssemblyVersion)
				ProcessLoadedAssembliesImpl();
		}

		private static void ProcessLoadedAssembliesImpl()
		{
			#region Get Assemblies
			System.Reflection.Assembly[] assemblies;

			try
			{
				System.Threading.Monitor.Enter(g_AssemblyLoadSyncObject);

				assemblies = new System.Reflection.Assembly[ThemeManager.g_assemblyList.Count];
				ThemeManager.g_assemblyList.CopyTo(assemblies);
				ThemeManager.g_assemblyList.Clear();

				// store the version number that we are processing
				ThemeManager.g_lastProcessedAssemblyVersion = ThemeManager.g_loadedAssemblyVersion;
			}
			finally
			{
				System.Threading.Monitor.Exit(g_AssemblyLoadSyncObject);
			}
			#endregion //Get Assemblies

			#region Get new loaded GroupingNames
			// AS 11/6/07 ThemeGroupingName
			// Find out what grouping names have been loaded.
			//
			List<string> listNewGroupingNames = new List<string>();

			for (int i = 0; i < assemblies.Length; i++)
				GetAssemblyThemeGroupingName(listNewGroupingNames, assemblies[i]);

			#endregion //Get new loaded GroupingNames

			#region Get AssemblyResourceSetLoader types list
			List<Type> listLoaderTypes = new List<Type>();

			for (int i = 0; i < assemblies.Length; i++)
			{
				// build a list of the types in the newly loaded assembly
				// that contain creatable app style metadata classes
				GetAssemblyResourceSetLoaderTypes(listLoaderTypes, assemblies[i]);
			} 
			#endregion //Get AssemblyResourceSetLoader types list

			#region Update Global Processed Lists

			List<string> listAllGroupingNames = new List<string>();
			List<Type> listPreviouslyLoadedTypes = new List<Type>();

			Monitor.Enter(typeof(ThemeManager));

			try
			{
				// store the types already processed. we may need them if we have 
				// new grouping names loaded
				listPreviouslyLoadedTypes.AddRange(g_processedResourceSetTypes);

				// update the global list of processed types
				if (listLoaderTypes.Count > 0)
				{
					foreach (Type resourceSetType in listLoaderTypes)
						g_processedResourceSetTypes.Add(resourceSetType);
				}

				// update the global list of loaded grouping names
				if (listNewGroupingNames.Count > 0)
				{
					foreach (string newGroupName in listNewGroupingNames)
						g_loadedGroupingNames.Add(newGroupName);
				}

				// keep a local list of all the registered grouping names for use with the 
				// new loader types
				listAllGroupingNames.AddRange(g_loadedGroupingNames);
			}
			finally
			{
				Monitor.Exit(typeof(ThemeManager));
			}

			#endregion //Update Global Processed Lists

			#region Refactored
			
#region Infragistics Source Cleanup (Region)















































#endregion // Infragistics Source Cleanup (Region)


			#endregion //Refactored

			#region Register ResourceSets
			// set a flag so we dont raise an event for every resourceset that
			// gets registered
			g_ignoreRegistrationInfoChanged = true;

			try
			{
				// if there are new grouping names then iterate the list and call register as needed
				// for the newly loaded groupings
				if (listNewGroupingNames.Count > 0)
				{
					// get the resource loaders for the previously loaded types
					List<AssemblyResourceSetLoader> resourceSetLoaders = new List<AssemblyResourceSetLoader>();
					CreateAssemblyResourceSetLoaders(resourceSetLoaders, listPreviouslyLoadedTypes);
					
					// register the new groupings for the old registered loaders
					foreach (AssemblyResourceSetLoader loader in resourceSetLoaders)
						loader.RegisterResourceSets(listNewGroupingNames);
				}

				// if we have new assembly resource loader attribs then gather the types
				if (listLoaderTypes.Count > 0)
				{
					List<AssemblyResourceSetLoader> resourceSetLoaders = new List<AssemblyResourceSetLoader>();
					CreateAssemblyResourceSetLoaders(resourceSetLoaders, listLoaderTypes);

					// register the new groupings
					foreach (AssemblyResourceSetLoader loader in resourceSetLoaders)
						loader.RegisterResourceSets(listAllGroupingNames);
				}
			}
			finally
			{
				// reset the flag
				g_ignoreRegistrationInfoChanged = false;
			} 
			#endregion //Register ResourceSets

			#region Verification/Notifications
			if (listNewGroupingNames.Count > 0 || listLoaderTypes.Count > 0)
			{
				#region VerifyAllGroupings
				// verify each theme's AllGroupings 
				if (g_themes != null)
				{
					Monitor.Enter(typeof(ThemeManager));

					try
					{
						foreach (ThemeInfo themeInfo in g_themes.Values)
							themeInfo.VerifyAllGroupings();
					}
					finally
					{
						Monitor.Exit(typeof(ThemeManager));
					}
				} 
				#endregion //VerifyAllGroupings

				VerifyCurrentThemeInfo();

				// raise the event to let listeners know that changes have been made
				if (g_registrationInfoChangedDelegate != null)
					g_registrationInfoChangedDelegate(null, EventArgs.Empty);
			} 
			#endregion //Verification/Notifications
		}

			#endregion //ProcessLoadedAssemblies

			#region VerifyCurrentThemeInfo

		private static void VerifyCurrentThemeInfo()
		{
			if (g_currentThemeInfoDirty == false ||
				g_ignoreRegistrationInfoChanged == true)
				return;

			g_currentThemeInfoDirty = false;

			Monitor.Enter(typeof(ThemeManager));

			try
			{
				string currentTheme;

				if (g_currentTheme != null)
					currentTheme = g_currentTheme.ToLower();
				else
					currentTheme = null;

				
#region Infragistics Source Cleanup (Region)


































































#endregion // Infragistics Source Cleanup (Region)


				ThemeInfo themeInfo = null;

				if (currentTheme != null)
					themeInfo = GetThemeInfo(currentTheme, false);

				ThemeInfo currentThemeInfo = g_currentThemeInfo;

				List<string> groupings = new List<string>();

				// create an entry for each grouping the current theme has now
				groupings.AddRange(currentThemeInfo.GetGroupingNames());

				if (null != themeInfo)
				{
					themeInfo.TransferRegisteredObjects(currentThemeInfo);

					// remove all the groupings from the current theme since they were
					// updated in the current theme info
					foreach (string actualThemeGrouping in themeInfo.GetGroupingNames())
						groupings.Remove(actualThemeGrouping);
				}

				// lastly replace any groupings that weren't replaced with empty dictionaries
				foreach (string grouping in groupings)
					currentThemeInfo.Register(grouping, new ResourceDictionary());

				currentThemeInfo.VerifyAllGroupings();
			}
			finally
			{
				Monitor.Exit(typeof(ThemeManager));
			}
		}

			#endregion //VerifyCurrentThemeInfo	

			#region VerifyResourcesWashed

		// JJD 7/23/07 - ResourceWasher support
		// Verify that the resources have been washed
		static private void VerifyResourcesWashed(ResourceDictionary rd)
		{
			if (rd == null)
				return;

			int count = rd.MergedDictionaries.Count;

			for (int i = 0; i < count; i++)
			{
				ResourceWasher washer = rd.MergedDictionaries[i] as ResourceWasher;

				if (washer != null)
					washer.VerifyThemeAccess();

			}
		}

			#endregion //VerifyResourcesWashed	
        
		#endregion //Private Methods

		#region private class ThemeInfo - old

		
#region Infragistics Source Cleanup (Region)









































































































































#endregion // Infragistics Source Cleanup (Region)


		#endregion //private class ThemeInfo - old

		#region private class ThemeInfo

		// AS 5/7/08
		private class ThemeInfo
		{
			#region Private Members

			private string _name;
			private WeakReference _allGroupings;
			private bool _areAllGroupingsDirty;
			private Dictionary<string, RegistrationInfo> _registrationInfo;

			// AS 5/9/08
			// see the comment in PrepareProvidedDictionary for details on this
			[ThreadStatic()]
			private static FrameworkElement _sharedOwner;

			[ThreadStatic()]
			private static ResourceDictionary _sharedEmptyDictionary;

			#endregion //Private Members

			#region Constructor
			internal ThemeInfo(string name)
			{
				this._name = name;

				this._registrationInfo = new Dictionary<string, RegistrationInfo>();
			} 
			#endregion //Constructor

			#region Properties

			#region Name
			public string Name
			{
				get { return this._name; }
			}
			#endregion //Name 

			#endregion //Properties

			#region Methods

			#region CreateAllGroupings
			private static ResourceDictionary CreateAllGroupings(ThemeInfo info)
			{
				ResourceDictionary rd = CreateProvidedDictionary();

				foreach (KeyValuePair<string, RegistrationInfo> dictionaryItem in info._registrationInfo)
				{
					// get the dictionary if one was explicitly registered
					ResourceDictionary rdChild = dictionaryItem.Value.GetProvidedDictionary(true);

					rd.MergedDictionaries.Add(rdChild);
				}

				return rd;
			}
			#endregion //CreateAllGroupings

			#region CreateProvidedDictionary
			private static ResourceDictionary CreateProvidedDictionary()
			{
				ResourceDictionary rd = new ResourceDictionary();

				PrepareProvidedDictionary(rd);

				return rd;
			}
			#endregion //CreateProvidedDictionary

			#region GetGroupingNames
			internal string[] GetGroupingNames()
			{
				string[] names = new string[this._registrationInfo.Count];
				this._registrationInfo.Keys.CopyTo(names, 0);
				Array.Sort(names, StringComparer.Ordinal);
				return names;
			} 
			#endregion //GetGroupingNames

			#region GetInfo
			private RegistrationInfo GetInfo(string grouping)
			{
				RegistrationInfo info;
				if (false == this._registrationInfo.TryGetValue(grouping, out info))
				{
					info = new RegistrationInfo(grouping);
					this._registrationInfo.Add(grouping, info);
				}

				return info;
			}
			#endregion //GetInfo

			#region GetResourceSet
			public ResourceDictionary GetResourceSet(string grouping, bool createIfNull)
			{
				if (grouping == ThemeManager.AllGroupingsLiteral)
				{
					return this.VerifyAllGroupings(createIfNull);
				}
				else
				{
					RegistrationInfo info;
					if (false == this._registrationInfo.TryGetValue(grouping, out info))
					{
						//Debug.Fail("Unexpected - the grouping has not been registered!");
						return null;
					}

					return info.GetProvidedDictionary(createIfNull);
				}
			}
			#endregion //GetResourceSet

			#region HasGrouping
			internal bool HasGrouping(string grouping)
			{
				if (string.IsNullOrEmpty(grouping) || grouping == AllGroupingsLiteral)
					return this._registrationInfo.Count > 0;
				else
					return this._registrationInfo.ContainsKey(grouping);
			}
			#endregion //HasGrouping

			#region PrepareProvidedDictionary
			private static void PrepareProvidedDictionary(ResourceDictionary rd)
			{
				if (_sharedOwner == null)
					_sharedOwner = new FrameworkElement();

				if (_sharedEmptyDictionary == null)
					_sharedEmptyDictionary = new ResourceDictionary();

				// there is a bug/behavior in wpf's rd such that the first element
				// to which the rd is added becomes the inheritance context
				// of the rd. even when the rd is removed from the element's resources
				// that continues to reference the rd so if that rd is kept around
				// then the first element to which it was added will be kept around.
				// to get around this i'm going to create a shared thread static
				// framework element that will serve as the inheritance context. to
				// prevent that element from rooting the new rd, we have to then remove
				// the rd from that shared element. for that i'm using a shared empty
				// resource dictionary
				_sharedOwner.Resources = rd;
				_sharedOwner.Resources = _sharedEmptyDictionary;
			} 
			#endregion // PrepareProvidedDictionary

			#region Register
			public bool Register(string grouping, ResourceSetLocator locator)
			{
				RegistrationInfo info = GetInfo(grouping);
				ResourceDictionary oldDictionary;
				if (info.Register(locator, out oldDictionary))
				{
					this._areAllGroupingsDirty = true;
					return true;
				}

				return false;
			}

			public bool Register(string grouping, ResourceDictionary dictionary)
			{
				RegistrationInfo info = GetInfo(grouping);
				ResourceDictionary oldDictionary;
				if (info.Register(dictionary, out oldDictionary))
				{
					this._areAllGroupingsDirty = true;
					return true;
				}

				return false;
			}
			#endregion //Register

			#region TransferRegisteredObjects
			internal void TransferRegisteredObjects(ThemeInfo destThemeInfo)
			{
				foreach (KeyValuePair<string, RegistrationInfo> item in this._registrationInfo)
				{
					if (item.Value.RegisteredLocator != null)
						destThemeInfo.Register(item.Key, item.Value.RegisteredLocator);
					else
						destThemeInfo.Register(item.Key, item.Value.RegisteredDictionary);
				}
			}
			#endregion //TransferRegisteredObjects

			#region VerifyAllGroupings
			internal void VerifyAllGroupings()
			{
				this.VerifyAllGroupings(false);
			}

			private ResourceDictionary VerifyAllGroupings(bool createIfNull)
			{
				ResourceDictionary rd = null;

				if (this._areAllGroupingsDirty && null != this._allGroupings)
				{
					rd = this._allGroupings != null ? Utilities.GetWeakReferenceTargetSafe(this._allGroupings) as ResourceDictionary : null;

					if (null != rd)
					{
						this._areAllGroupingsDirty = false;

						Debug.Assert(rd.MergedDictionaries.Count == 1);

						// create new wrappers
						if (rd.MergedDictionaries.Count == 1)
							rd.MergedDictionaries[0] = CreateAllGroupings(this);

						// its fixed up so we can return here
						return rd;
					}
					else // release the weak reference if the dictionary is not being used
						this._allGroupings = null;
				}

				if (createIfNull)
				{
					this._areAllGroupingsDirty = false;

					rd = new ResourceDictionary();
					rd.MergedDictionaries.Add(CreateAllGroupings(this));
					this._allGroupings = new WeakReference(rd);
				}

				return rd;
			}
			#endregion //VerifyAllGroupings

			#endregion //Methods

			#region RegistrationInfo class
			private class RegistrationInfo
			{
				#region Member Variables

				private WeakReference _providedDictionary;
				private ResourceDictionary _registeredDictionary;
				private ResourceSetLocator _registeredLocator;
				private string _grouping;

				#endregion //Member Variables

				#region Constructor
				internal RegistrationInfo(string grouping)
				{
					this._grouping = grouping;
				}
				#endregion //Constructor

				#region Properties

				#region Grouping
				internal string Grouping
				{
					get { return this._grouping; }
				}
				#endregion //Grouping

				#region RegisteredLocator
				internal ResourceSetLocator RegisteredLocator
				{
					get { return this._registeredLocator; }
				}
				#endregion //RegisteredLocator

				#region RegisteredDictionary
				internal ResourceDictionary RegisteredDictionary
				{
					get { return this._registeredDictionary; }
				}
				#endregion //RegisteredDictionary

				#endregion //Properties

				#region Methods

				#region GetProvidedDictionary
				internal ResourceDictionary GetProvidedDictionary(bool createIfNull)
				{
					ResourceDictionary rd = null;

					if (this._providedDictionary != null)
					{
						rd = Utilities.GetWeakReferenceTargetSafe(this._providedDictionary) as ResourceDictionary;

						if (null != rd)
							return rd;
						else
							this._providedDictionary = null;
					}

					if (createIfNull == false)
						return null;

					rd = new ResourceDictionary();

					if (this._registeredDictionary != null)
						rd.MergedDictionaries.Add(this._registeredDictionary);
					else if (this._registeredLocator != null)
						rd.MergedDictionaries.Add(new ResourceSetLoader(this._registeredLocator));
					else
						Debug.Fail("We don't have a registered dictionary or locator!");

					// AS 5/9/08
					// See the comment in this method for details on why this is needed.
					//
					ThemeInfo.PrepareProvidedDictionary(rd);

					this._providedDictionary = new WeakReference(rd);
					return rd;
				}
				#endregion //GetProvidedDictionary

				#region Register
				internal bool Register(ResourceSetLocator value, out ResourceDictionary oldDictionary)
				{
					oldDictionary = null;

					if (this._registeredLocator == value)
						return false;

					Debug.Assert(null != value);

					// store the locator
					this._registeredLocator = value;

					// clear the registered dictionary - the last one in wins
					this._registeredDictionary = null;

					// verify the provided dictionary
					ResourceDictionary rdProvided = this.GetProvidedDictionary(false);

					if (null != rdProvided)
					{
						// if someone locked the dictionary we provided then remove our 
						// reference to it. the next one who asks will get one with the
						// updated info
						if (rdProvided.IsReadOnly)
							this._providedDictionary = null;
						else
						{
							Debug.Assert(rdProvided.MergedDictionaries.Count == 1);

							if (rdProvided.MergedDictionaries.Count == 1)
							{
								oldDictionary = rdProvided.MergedDictionaries[0];
								rdProvided.MergedDictionaries[0] = new ResourceSetLoader(this._registeredLocator);
							}
						}
					}

					return true;
				}
				#endregion //Register

				#region Register
				internal bool Register(ResourceDictionary value, out ResourceDictionary oldDictionary)
				{
					oldDictionary = null;

					if (value == this._registeredDictionary)
						return false;

					Debug.Assert(null != value);

					// clear the registered locator
					this._registeredLocator = null;

					this._registeredDictionary = value;

					// verify the provided dictionary
					ResourceDictionary rdProvided = this.GetProvidedDictionary(false);

					if (null != rdProvided)
					{
						// if someone locked the dictionary we provided then remove our 
						// reference to it. the next one who asks will get one with the
						// updated info
						if (rdProvided.IsReadOnly)
							this._providedDictionary = null;
						else
						{
							Debug.Assert(rdProvided.MergedDictionaries.Count == 1);

							if (rdProvided.MergedDictionaries.Count == 1)
							{
								oldDictionary = rdProvided.MergedDictionaries[0];
								rdProvided.MergedDictionaries[0] = this._registeredDictionary;
							}
						}
					}

					return true;
				} 
				#endregion //Register

				#endregion //Methods
			} 
			#endregion //RegistrationInfo class
		}

		#endregion //private class ThemeInfo

		#region ControlThemeInfo private class

		private class ControlThemeInfo : DependencyObject
		{
			#region Old version
			
#region Infragistics Source Cleanup (Region)




































#endregion // Infragistics Source Cleanup (Region)

			#endregion //Old version

			#region Member Variables

			private string _currentTheme;
			private ResourceDictionary _mergedDictionary;
			private FrameworkElement _element;
			private string[] _currentGroupings;

			// AS 6/11/12 TFS108663
			// This is flawed. We could have applied the theme and then have it changed while the control is unloaded.
			//
			//private bool _hasThemeApplied;
			private string _appliedTheme;
			private string[] _appliedGroupings;

            // AS 10/9/08 TFS7905
            private bool _isApplyingTheme;

			#endregion //Member Variables

			#region Constructor
			private ControlThemeInfo(FrameworkElement element)
			{
				this._element = element;

				// AS 6/4/08
				// If the element is initializing them we want to wait until the
				// Initialized event because the Resources property may be set 
				// after the Theme property has been set.
				//
				if (false == element.IsInitialized)
					element.Initialized += new EventHandler(OnElementInitialized);
				else if (element.IsLoaded)
					element.AddHandler(FrameworkElement.UnloadedEvent, new RoutedEventHandler(OnElementUnloaded), true);
				else
					element.AddHandler(FrameworkElement.LoadedEvent, new RoutedEventHandler(OnElementLoaded), true);
			}
			#endregion //Constructor

			#region Properties

			#region ControlThemeInfo

			private static readonly DependencyProperty ControlThemeInfoProperty = DependencyProperty.Register("ControlThemeInfo",
				typeof(ControlThemeInfo), typeof(ControlThemeInfo), new FrameworkPropertyMetadata(null));

			#endregion //ControlThemeInfo

			// AS 6/11/12 TFS108663
			#region IsApplyThemeNeeded
			private bool IsApplyThemeNeeded
			{
				get
				{
					return _currentTheme != _appliedTheme || _currentGroupings != _appliedGroupings;
				}
			}
			#endregion //IsApplyThemeNeeded

			#endregion //Properties

			#region Methods

			#region ApplyTheme
			private void ApplyTheme(string theme, string[] groupings, bool updateMembers)
			{
                // AS 10/9/08 TFS7905
                // Moved the original impl into a helper method so we can track whether
                // we are in the middle of a theme application when we get another one.
                //
                if (this._isApplyingTheme)
                    throw new InvalidOperationException(SR.GetString("LE_CannotChangeThemeWhileApplyingTheme", theme, this._currentTheme));

                this._isApplyingTheme = true;
                try
                {
                    this.ApplyThemeImpl(theme, groupings, updateMembers);
                }
                finally
                {
                    this._isApplyingTheme = false;
                }
            }

            // AS 10/9/08 TFS7905
            // Moved impl from the ApplyTheme method.
            //
            private void ApplyThemeImpl(string theme, string[] groupings, bool updateMembers)
			{
				if (updateMembers)
				{
					this._currentTheme = theme;
					this._currentGroupings = groupings;
				}

				Debug.Assert(null == theme || theme == this._currentTheme);

				// AS 6/11/12 TFS108663
				//this._hasThemeApplied = false == string.IsNullOrEmpty(theme);
				_appliedTheme = theme;
				_appliedGroupings = groupings;

				ResourceDictionary resourceSets = new ResourceDictionary();

				if (false == string.IsNullOrEmpty(theme))
				{
					for (int i = 0; i < groupings.Length; i++)
					{
						string grouping = groupings[i];

						Debug.Assert(grouping != null);

						if (grouping != null)
						{
							ResourceDictionary rd = ThemeManager.GetResourceSet(theme, grouping);

							if (null != rd)
								resourceSets.MergedDictionaries.Add(rd);
						}
					}
				}

				Collection<ResourceDictionary> mergedDictionaries = this._element.Resources.MergedDictionaries;

				// if we previously applied a theme...
				if (this._mergedDictionary != null)
				{
					// there is a bug in the current implmentation of ResourceDictionary
					// that prevents a simple remove. Therefore we have to create a dummy
					// instance to replace the old one.
					
					for (int i = 0; i < mergedDictionaries.Count; i++)
					{
						if (mergedDictionaries[i] == this._mergedDictionary)
						{
							this._mergedDictionary = resourceSets;
							mergedDictionaries[i] = this._mergedDictionary;
							break;
						}
					}
				}

				// if the new resourceset collection wasn't replaced above then
				// add it now
				if (this._mergedDictionary != resourceSets)
				{
					this._mergedDictionary = resourceSets;

					// JM 02-09-09 TFS12230 - Insert our Theme's resources at the beginning of the merged dictionaries list so when the framework walks over the
					//						  contents of the merged dictionaries in reverse order our resource don't blow away any resources that the user
					//						  may have placed there.
					//mergedDictionaries.Add(this._mergedDictionary);
					mergedDictionaries.Insert(0, this._mergedDictionary);
				}
			}
			#endregion //ApplyTheme

            // AS 9/30/08 TFS6215
            #region HasLoadedAncestor
            
#region Infragistics Source Cleanup (Region)





















#endregion // Infragistics Source Cleanup (Region)

            #endregion //HasLoadedAncestor

            // AS 9/30/08 TFS6215
            #region OnDelayedUnload
            private void OnDelayedUnload(object param)
            {
                // JJD 12/17/08 TFS10903 
                // Moved HasLoadedAncestor to Utilities class and made public
                //if (HasLoadedAncestor(this._element))
                if (Utilities.HasLoadedAncestor(this._element))
                    return;

                this._element.RemoveHandler(FrameworkElement.UnloadedEvent, new RoutedEventHandler(OnElementUnloaded));
                this._element.AddHandler(FrameworkElement.LoadedEvent, new RoutedEventHandler(OnElementLoaded), true);

                // remove the theme dictionary
                ApplyTheme(null, null, false);
            }
            #endregion //OnDelayedUnload

            #region OnElementInitialized
			private void OnElementInitialized(object sender, EventArgs e)
			{
				this._element.Initialized -= new EventHandler(OnElementInitialized);

				if (this._element.IsLoaded)
					this._element.AddHandler(FrameworkElement.UnloadedEvent, new RoutedEventHandler(OnElementUnloaded), true);
				else
					this._element.AddHandler(FrameworkElement.LoadedEvent, new RoutedEventHandler(OnElementLoaded), true);

				// reapply the theme dictionary if we still have one and haven't applied
				// it ahead of the load
				// AS 6/11/12 TFS108663
				//if (false == this._hasThemeApplied && false == string.IsNullOrEmpty(this._currentTheme))
				if (this.IsApplyThemeNeeded)
					ApplyTheme(this._currentTheme, this._currentGroupings, false);
			}
			#endregion //OnElementInitialized

			#region OnElementLoaded
			void OnElementLoaded(object sender, RoutedEventArgs e)
			{
				this._element.RemoveHandler(FrameworkElement.LoadedEvent, new RoutedEventHandler(OnElementLoaded));
				this._element.AddHandler(FrameworkElement.UnloadedEvent, new RoutedEventHandler(OnElementUnloaded), true);

				// reapply the theme dictionary if we still have one and haven't applied
				// it ahead of the load
				// AS 6/11/12 TFS108663
				//if (false == this._hasThemeApplied && false == string.IsNullOrEmpty(this._currentTheme))
				if (this.IsApplyThemeNeeded)
					ApplyTheme(this._currentTheme, this._currentGroupings, false);
			}
			#endregion //OnElementLoaded

			#region OnElementUnloaded
			void OnElementUnloaded(object sender, RoutedEventArgs e)
			{
                
#region Infragistics Source Cleanup (Region)








#endregion // Infragistics Source Cleanup (Region)

                this._element.Dispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new SendOrPostCallback(OnDelayedUnload), null);
			}
			#endregion //OnElementUnloaded

			#region ProcessThemeChanged
			internal static void ProcessThemeChanged(FrameworkElement themedControl, string theme, string[] groupings)
			{
				Utilities.ThrowIfNull(themedControl, "themedControl");
				Utilities.ThrowIfNull(groupings, "groupings");

				ControlThemeInfo themeInfo = themedControl.GetValue(ControlThemeInfoProperty) as ControlThemeInfo;

				if (themeInfo == null)
				{
					themeInfo = new ControlThemeInfo(themedControl);
					themedControl.SetValue(ControlThemeInfoProperty, themeInfo);
				}

				// if the element is loaded or if it is in the process of being
				// initialized then apply the theme right away. otherwise wait
				// for it to be loaded since the theme could root the element. i'm 
				// allowing an uninitialized element to be processed right away
				// for effeciency. the loaded event is raised after the element
				// has been measured/arranged so if the theme property is being
				// set from the xaml then we don't want to cause it to invalidate
				// the arrange/measure again by waiting for the loaded event
				// AS 6/4/08
				// If the element is initializing them we want to wait until the
				// Initialized event because the Resources property may be set 
				// after the Theme property has been set.
				//
				//if (themedControl.IsLoaded || false == themedControl.IsInitialized)
				// JJD 6/10/11 - TFS74350
				// Also apply the theme here if the control is in a report since
				// that process is done synchronously with no good trigger point to
				// use to update the resources with the theme dictionaries
				//if (themedControl.IsLoaded)
				if (themedControl.IsLoaded || Reporting.ReportSection.GetIsInReport(themedControl))
				{
					themeInfo.ApplyTheme(theme, groupings, true);
				}
				// AS 6/11/12 TFS108663
				// If we have a theme pending then we're hooked into loading/initialized and will apply it 
				// then but if we don't have one pending and the control is already initialized and it has 
				// loaded ancestors then we should apply the theme now.
				//
				else if (!themeInfo.IsApplyThemeNeeded && themedControl.IsInitialized && Utilities.HasLoadedAncestor(themedControl))
				{
					themeInfo.ApplyTheme(theme, groupings, true);
				}
				else
				{
					// just store the info and apply when loaded
					themeInfo._currentTheme = theme;
					themeInfo._currentGroupings = groupings;
				}
			}

			#endregion //ProcessThemeChanged 

			#endregion //Methods
		}

		#endregion //ControlThemeInfo private class
	}

	#region ResourceHolder

	internal class ResourceHolder : ResourceDictionary
	{




		internal ResourceHolder()
		{



		}



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


	}

	#endregion //ResourceHolder
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