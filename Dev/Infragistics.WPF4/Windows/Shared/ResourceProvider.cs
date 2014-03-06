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


using Infragistics.Windows.Themes;


namespace Infragistics.Controls.Primitives
{
	#region IResourceProviderClient interface

	/// <summary>
	/// Used to notify <see cref="ResourceProvider"/> client elements that the resources may have changed
	/// </summary>

	[InfragisticsFeature(FeatureName = "XamCalendar", Version = "11.1")]

	public interface IResourceProviderClient
	{
		/// <summary>
		/// Called when resources have changed or when the <see cref="ResourceProvider"/> attached property has changed.
		/// </summary>
		void OnResourcesChanged();
	}

	#endregion //IResourceProviderClient interface	
    
	#region ResourceProvider class

	/// <summary>
	/// Abstract base class that exposes an indexer to retrieve values based on a key.
	/// </summary>

	[InfragisticsFeature(FeatureName = "XamCalendar", Version = "11.1")]

	public abstract class ResourceProvider : PropertyChangeNotifier
	{
		#region Private Members


		private static readonly bool _UserPreferenceEventWired;


		[ThreadStatic()]
		private static DependencyObject _DistpatherOwer;

		private bool _isHighContrast;
		private int _resourceVersion;

		private static bool _CachedIsHighContrastDirty;
		private static bool _CachedIsHighContrast;
		private static WeakSet<ResourceProvider> _Providers;

		#endregion //Private Members	
    
		#region Constructor

		static ResourceProvider()
		{
			_CachedIsHighContrastDirty = true;

			_Providers = new WeakSet<ResourceProvider>();

			try
			{
				

				

				InitUserPreferenceChanged();
				
			_UserPreferenceEventWired = true;

			}
			catch {}



		}

		/// <summary>
		/// Instantiates a new instance of <see cref="ResourceProvider"/>
		/// </summary>
		internal ResourceProvider()
		{
			
			if (_DistpatherOwer == null)
			{

				_DistpatherOwer = new DependencyObject();



			}


			if (!_UserPreferenceEventWired)

			{
				VeriftyCachedIsHighContrast();
			}

			lock (_Providers)
				_Providers.Add(this);

		}

		#endregion //Constructor	

		#region Public Properties

		#region Indexers

		/// <summary>
		/// Indexer that takes an object/>
		/// </summary>
		public abstract object this[object key] { get; }

		/// <summary>
		/// Indexer that takes a string 
		/// </summary>
		public virtual object this[string key] 
		{
			get
			{
				return this[(object)key];
			}
		}

		#endregion //Indexers

		#region IsHighContrast

		private void SetIsHighContrastInternal(bool isHighContrast)
		{
			this.IsHighContrast = isHighContrast;
		}

		/// <summary>
		/// Returns  true if the system theme is a high contrast theme (read-only).
		/// </summary>
		[Browsable(false)]
		[ReadOnly(true)]

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

		public bool IsHighContrast
		{
			get
			{
				return this._isHighContrast;
			}
			private set
			{
				if (this._isHighContrast != value)
				{
					this._isHighContrast = value;

					this.InvalidateResources();

					this.RaisePropertyChangedEvent("IsHighContrast");
				}
			}
		}

		#endregion // IsHighContrast

		#region ResourceProvider

		/// <summary>
		/// Identifies the ResourceProvider attached dependency property
		/// </summary>
		/// <seealso cref="GetResourceProvider"/>
		/// <seealso cref="SetResourceProvider"/>
		public static readonly DependencyProperty ResourceProviderProperty = DependencyPropertyUtilities.RegisterAttached("ResourceProvider",
			typeof(ResourceProvider), typeof(ResourceProvider),
			DependencyPropertyUtilities.CreateMetadata(null, new PropertyChangedCallback(OnResourceProviderChanged))
			);

		private static void OnResourceProviderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			IResourceProviderClient item = d as IResourceProviderClient;

			if (item != null)
				item.OnResourcesChanged();
		}

		/// <summary>
		/// Gets the value of the attached ResourceProvider DependencyProperty.
		/// </summary>
		/// <param name="target">The object whose value is to be returned</param>
		/// <seealso cref="ResourceProviderProperty"/>
		/// <seealso cref="SetResourceProvider"/>
		public static ResourceProvider GetResourceProvider(DependencyObject target)
		{
			return (ResourceProvider)target.GetValue(ResourceProvider.ResourceProviderProperty);
		}

		/// <summary>
		/// Sets the value of the attached ResourceProvider DependencyProperty.
		/// </summary>
		/// <param name="target">The object whose value is to be modified</param>
		/// <param name="value">The new value</param>
		/// <seealso cref="ResourceProviderProperty"/>
		/// <seealso cref="GetResourceProvider"/>
		public static void SetResourceProvider(DependencyObject target, ResourceProvider value)
		{
			target.SetValue(ResourceProvider.ResourceProviderProperty, value);
		}

		#endregion //ResourceProvider

		#region ResourceVersion

		/// <summary>
		/// Returns a version number that will get bumped every time <see cref="InvalidateResources"/> is called.
		/// </summary>
		public int ResourceVersion
		{
			get
			{
				return this._resourceVersion;
			}
			private set
			{
				if (this._resourceVersion != value)
				{
					this._resourceVersion = value;

					this.RaisePropertyChangedEvent("ResourceVersion");
				}
			}
		}

		#endregion //ResourceVersion

		#endregion //Public Properties

		#region Private Properties


		#region IsHighContrastStatic

		/// <summary>
		/// Gets whether the system should render controls with high contrast.
		/// </summary>
		/// <remarks>The conditions to determine this are consistant with Office2007.</remarks>
		private static bool IsHighContrastStatic
		{
			get
			{
				if (_CachedIsHighContrastDirty)
				{
					_CachedIsHighContrastDirty = false;
					_CachedIsHighContrast = SystemParameters.HighContrast ||
						(SystemColors.ControlColor == Colors.Black && SystemColors.ControlTextColor == Colors.White) ||
						(SystemColors.ControlColor == Colors.White && SystemColors.ControlTextColor == Colors.Black);
				}

				return _CachedIsHighContrast;
			}
		}

		#endregion //IsHighContrastStatic

		#endregion //Private Properties	
    
		#region Methods

		#region Public Methods

		#region InvalidateResources

		/// <summary>
		/// Called to invalidate any cached values
		/// </summary>
		public virtual void InvalidateResources()
		{
			this.BumpResourceVersion();
		}

		#endregion //InvalidateResources	
    
   		#endregion //Public Methods	
    
		#region Protected Methods

		#region OnSystemColorsChanged

		/// <summary>
		/// Called when the system colors have changed
		/// </summary>
		protected virtual void OnSystemColorsChanged()
		{
			this.InvalidateResources();
		}

		#endregion //OnSystemColorsChanged

		#endregion //Protected Methods	

		#region BumpResourceVersion

		internal void BumpResourceVersion()
		{
			this.ResourceVersion++;
		}

		#endregion //BumpResourceVersion	
    
		#region Internal Methods

		#region OnSystemColorsChangedInternal

		internal void OnSystemColorsChangedInternal()
		{

			if (!_DistpatherOwer.CheckAccess())
			{
				// marshall this call onto the dispatcher's thread
				_DistpatherOwer.Dispatcher.BeginInvoke(new MethodInvoker(this.OnSystemColorsChangedInternal));
				return;
			}

			SetIsHighContrastInternal(IsHighContrastStatic);

			this.OnSystemColorsChanged();

		}

		#endregion //OnSystemColorsChangedInternal

		#endregion //Internal Methods	
        
		#region Private Methods

		#region InitUserPreferenceChanged (private)

		[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
		private static void InitUserPreferenceChanged()
		{
			Microsoft.Win32.SystemEvents.UserPreferenceChanged += new Microsoft.Win32.UserPreferenceChangedEventHandler(OnUserPreferenceChanged);

		}

		#endregion InitUserPreferenceChanged

		#region OnUserPreferenceChanged


		private static void OnUserPreferenceChanged(object sender, Microsoft.Win32.UserPreferenceChangedEventArgs e)
		{
			if (e.Category == Microsoft.Win32.UserPreferenceCategory.Color)
			{
				VeriftyCachedIsHighContrast();
			}
		}



		#endregion //OnUserPreferenceChanged

		private static void VeriftyCachedIsHighContrast()
		{
			// first check if the system clors have changed since we don't get notified in SL
			bool isHighContrastCached = IsHighContrastStatic;

			_CachedIsHighContrastDirty = true;

			if (isHighContrastCached != IsHighContrastStatic)
			{
				// copy our static instances into a stack list for processsing below
				List<ResourceProvider> providers = null;
				lock (_Providers)
					providers = new List<ResourceProvider>(_Providers);

				foreach (ResourceProvider provider in providers)
					provider.OnSystemColorsChangedInternal();
			}
		}

		#endregion //Private Methods	
    
		#endregion //Methods

		#region MethodInvoker

		internal delegate void MethodInvoker();

		#endregion //MethodInvoker
	}

	#endregion //ResourceProvider class	
    
	#region ResourceProvider<T> class

	/// <summary>
	/// Abstract base class that exposes an indexer to retrieve values based on an enum.
	/// </summary>
	/// <typeparam name="T"></typeparam>

	[InfragisticsFeature(FeatureName = "XamCalendar", Version = "11.1")]

	public abstract class ResourceProvider<T> : ResourceProvider where T : struct
	{
		#region Members

		private static string _TypeExceptionMessage;

		internal readonly static int _Count;
		internal readonly static T[] _Ids;

		#endregion //Members	
    
		#region Constructor

		static ResourceProvider()
		{
			if (!typeof(T).IsEnum)
			{
				_TypeExceptionMessage = SR.GetString("Provider_TypeNotEnum", typeof(T));
				return;
			}

			List<T> listOfIds = new List<T>( typeof(T).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
				.Where(f => f.IsLiteral)
				.Select(f => (T)f.GetValue(null)));

			listOfIds.Sort();

			_Count = listOfIds.Count;

			if (_Count < 1)
			{
				_TypeExceptionMessage = SR.GetString("Provider_EnumEmpty", typeof(T));
				return;
			}

			int expectedValue = 0;

			foreach (T id in listOfIds)
			{
				int value = ((IConvertible)(id)).ToInt32(null);

				if (value != expectedValue)
				{
					_TypeExceptionMessage = SR.GetString("Provider_SparseEnumValues", typeof(T));
					return;
				}

				expectedValue++;
			}

			_Ids = listOfIds.ToArray();

		}

		/// <summary>
		/// Instantiates a new instance of <see cref="ResourceProvider"/>
		/// </summary>
		protected ResourceProvider()
		{
			if (_TypeExceptionMessage != null)
				throw new NotSupportedException( _TypeExceptionMessage);
		}

		#endregion //Constructor	
    
		#region Base class overrides

		#region Indexers

		/// <summary>
		/// Indexer takes a string or a key/>
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override object this[object key]
		{
			get
			{
				T id;
					
				if (!TryGetEnumValueFromObject(key, out id) )
					throw new KeyNotFoundException(SR.GetString("Provider_KeyNotFound", typeof(T), key));

				return this.GetResource(id);
			}
		}

		/// <summary>
		/// Indexer that takes a string 
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public override object this[string key]
		{
			get
			{
				return this[(object)key];
			}
		}

		/// <summary>
		/// Indexer that takes an enum
		/// </summary>
		public object this[T key]
		{
			get
			{
				return this[(object)key];
			}
		}

		#endregion //Indexers

		#endregion //Base class overrides

		#region Properties

		#endregion //Properties

		#region Methods

		#region Public Methods

		#region GetResource

		/// <summary>
		/// Returns a value for a specific id
		/// </summary>
		/// <param name="id">The id of the resource</param>
		/// <returns>The resource identified by the id or null.</returns>
		protected abstract object GetResource(T id);

		#endregion //GetResource	
    
		#endregion //Public Methods	
    
		#region Internal Methods

		#region TryGetEnumValueFromObject

		internal static bool TryGetEnumValueFromObject(object key, out T id)
		{
			if (key is T)
				id = (T)key;
			else
			{
				id = _Ids[0];

				string strKey = key as string;




                if (strKey == null || !Enum.TryParse<T>(strKey, out id))

                {
					IConvertible convertible = key as IConvertible;

					if (convertible == null)
						return false;

					int keyInt;
					try { keyInt = convertible.ToInt32(null); }
					catch (Exception)
					{
						return false;
					}

					if (keyInt >= 0 && keyInt < _Count)
						id = _Ids[keyInt];
					else
					{
						return false;
					}
				}
			}

			return true;
		}

		#endregion //TryGetEnumValueFromObject

		#endregion //Internal Methods

		#endregion //Methods
	}

	#endregion //ValueFromEnumProvider class

	#region ResourceProviderWithOverrides<T> class

	/// <summary>
	/// Abstract base class that exposes an indexer to retrieve values based on an enum from a <see cref="ResourceDictionary"/>.
	/// </summary>

	[InfragisticsFeature(FeatureName = "XamCalendar", Version = "11.1")]

	public abstract class ResourceProviderWithOverrides<T> : ResourceProvider<T>

        , ISupportInitialize // WINDOWS_PHONE doesn't support ISupportInitialize, it throw an exception in design time "ISupportInitialize couldn´t be loaded"
		, IResourceWasherTarget // AS 2/23/12 TFS102032

 where T : struct
	{
		private  object[] _values;
		private bool _isLoading;
		private bool _isHighContrastSupported = true;
		private ResourceDictionary _defaultResources;
		private ResourceDictionary _defaultResourcesHighContrast;
		private ResourceDictionary _resourceOverrides;
		private ResourceDictionary _resourceOverridesHighContrast;

		// AS 2/23/12 TFS102032

		private ResourceWasher _resourceWasher;
		private ResourceDictionary _washedDefaultResources;


		private static object NullEntry = new object();

		#region Base class overrides

		#region GetResource

		/// <summary>
		/// Returns a value for a specific id
		/// </summary>
		/// <param name="id">The id of the resource</param>
		/// <returns>The resource identified by the id or null.</returns>
		protected override object GetResource(T id)
		{

			int index = ((IConvertible)(id)).ToInt32(null);

			if (index >= 0 && index < _Ids.Length)
			{
				object resource;

				if (_values != null)
				{
					resource = _values[index];

					if (resource != null)
					{
						// if the value is the special 'null entry' then return null
						if (resource == NullEntry)
							return null;

						return resource;
					}
				}

				if (_values == null)
					_values = new object[_Count];

				ResourceDictionary defaults = null;
				ResourceDictionary overrides = null;

				bool isHighContrast = this.IsHighContrast;

				if (isHighContrast && IsHighContrastSupported)
				{
					defaults = this._defaultResourcesHighContrast;
					overrides = this.ResourceOverridesHighContrast;
				}
				else
					overrides = this.ResourceOverrides;

				if (defaults == null)
				{



					// AS 2/23/12 TFS102032
					// If we previously washed the default resources then use that.
					//
					defaults = _washedDefaultResources ?? this._defaultResources;

					// AS 2/23/12 TFS102032
					// If we have a resource washer and we haven't washed the default resources 
					// yet do so now and cached the washed resource dictionary so we can use it 
					// again for the next resource id.
					//
					if (null != defaults && _resourceWasher != null && _washedDefaultResources == null)
						defaults = _washedDefaultResources = (ResourceDictionary)_resourceWasher.CreateWashedResource(defaults);

				}

				string idName = id.ToString();

				resource = GetResourceHelper(overrides, id, index, idName, false);

				if (resource != null)
					return resource;

				resource = GetResourceHelper(defaults, id, index, idName, true);

				if (resource != null)
					return resource;

				// AS 4/3/12 TFS107893
				// Added if check. In my case it is ok that things are missing but if we have 
				// a defaults (which I don't) then we'll keep doing this.
				//
				if (defaults != null)
					LogDebuggerWarning(SR.GetString("Provider_MissingResource", idName, this));

			}

			return null;
		}

		#endregion //GetResource	
    
		#region InvalidateResources

		/// <summary>
		/// Called to invalidate any cached resources
		/// </summary>
		public sealed override void InvalidateResources()
		{
			_values = null;


			_washedDefaultResources = null;	// AS 2/23/12 TFS102032


			this.OnPropertyChanged("Item[]");

			base.InvalidateResources();
		}

		#endregion //InvalidateResources	
    
		#endregion //Base class overrides	
    
		#region Properties

		#region Public Properties

		#region IsHighContrast

		/// <summary>
		/// Determines whether controls will be rendered in high contrast based on the current system settings (read-only).
		/// </summary>
		/// <remarks>
		/// <para class="body">If the system theme is a high contrast theme and this property is left to its default valu of true then high contrast resources will be returned by the indexers.</para>
		/// </remarks>

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

		public bool IsHighContrastSupported
		{
			get
			{
				return this._isHighContrastSupported;
			}
			set
			{
				if (this._isHighContrastSupported != value)
				{
					this._isHighContrastSupported = value;

					this.InvalidateResources();

					this.RaisePropertyChangedEvent("IsHighContrastSupported");
				}
			}
		}

		#endregion // IsHighContrast

		#region ResourceOverrides

		/// <summary>
		/// Gets/sets an optional <see cref="ResourceDictionary"/> that contains resources keyed by the enum values of T that will be used instead of the the corresponding defalse values.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> In WPF the x:Key used for the resoure can either be the enum value or its exact string equivalent. 
		/// In Silverlight however, since that framework only supports keys that are strings or Types, you can only use the string equivalent as the key.</para>
		/// </remarks>
		public ResourceDictionary ResourceOverrides
		{
			get
			{
				return this._resourceOverrides;
			}
			set
			{
				if (this._resourceOverrides != value)
				{
					this._resourceOverrides = value;

					this.InvalidateResources();

					this.RaisePropertyChangedEvent("ResourceOverrides");
				}
			}
		}

		#endregion //ResourceOverrides

		#region ResourceOverridesHighContrast

		/// <summary>
		/// Gets/sets an optional <see cref="ResourceDictionary"/> that contains resources keyed by the enum values of T that will be used instead of the the corresponding defalse values.
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> In WPF the x:Key used for the resoure can either be the enum value or its exact string equivalent. 
		/// In Silverlight however, since that framework only supports keys that are strings or Types, you can only use the string equivalent as the key.</para>
		/// </remarks>
		public ResourceDictionary ResourceOverridesHighContrast
		{
			get
			{
				return this._resourceOverridesHighContrast;
			}
			set
			{
				if (this._resourceOverridesHighContrast != value)
				{
					this._resourceOverridesHighContrast = value;

					this.InvalidateResources();

					this.RaisePropertyChangedEvent("ResourceOverridesHighContrast");
				}
			}
		}

		#endregion //ResourceOverridesHighContrast
    		
		#endregion //Public Properties

		#region Protected Properties

		#region DefaultResources

		/// <summary>
		/// Gets/sets a <see cref="ResourceDictionary"/> that contains the default resources for all values of T. 
		/// </summary>
		protected ResourceDictionary DefaultResources
		{
			get
			{
				return _defaultResources;
			}
			set
			{
				if (value != _defaultResources)
				{
					_defaultResources = value;
					this.InvalidateResources();
				}
			}
		}

		#endregion //DefaultResources	

		#region DefaultResourcesHighContrast

		/// <summary>
		/// Gets/sets a <see cref="ResourceDictionary"/> that contains the default resources for all values of T. 
		/// </summary>
		protected ResourceDictionary DefaultResourcesHighContrast
		{
			get
			{
				return _defaultResourcesHighContrast;
			}
			set
			{
				if (value != _defaultResourcesHighContrast)
				{
					_defaultResourcesHighContrast = value;
					this.InvalidateResources();
				}
			}
		}

		#endregion //DefaultResourcesHighContrast	

		#region IsLoading

		/// <summary>
		/// Returns true between calls to <see cref="BeginInit"/> and <see cref="EndInit"/>
		/// </summary>
		protected internal bool IsLoading { get { return _isLoading; } }

		#endregion //IsLoading	
    
		#endregion //Protected Properties

		#endregion //Properties

		#region Methods

		#region Public Methods

		#region BeginInit

		/// <summary>
		/// Called when object initialization is beginning
		/// </summary>
		public virtual void BeginInit()
		{
			this._isLoading = true;
		}

		#endregion //BeginInit	
 
		#region EndInit

		/// <summary>
		/// Called when object initialization has ended
		/// </summary>
		public virtual void EndInit()
		{
			this._isLoading = false;

		}

		#endregion //EndInit	
    
		#endregion //Public Methods	

		#region Protected Methods

		#region IsResourceValid

		/// <summary>
		/// Returns true if this is a valid resource
		/// </summary>
		/// <param name="id">The id of the resource</param>
		/// <param name="resource">The resource that was found</param>
		/// <returns>True if the resource is valid, otherwise false</returns>
		/// <remarks>
		/// <para class="body">For example, if the id represents a Brush and a Thickness is found then this method should return false.</para>
		/// </remarks>
		protected abstract bool IsResourceValid(T id, object resource);

		#endregion //IsResourceValid

		#endregion //Protected Methods	
        
		#region Private Methods

		#region GetResourceHelper

		private object GetResourceHelper(ResourceDictionary rd, T id, int index, string idName, bool isDefault)
		{
			if ( rd == null )
				return null;

			object resource = null;

			if (rd.Contains(idName))
				resource = rd[idName];

			else
			if (rd.Contains(id))
				 resource = rd[id];

			else
			if (!isDefault )
				return null;

			if (IsResourceValid(id, resource))
			{
				if (resource == null)
					resource = NullEntry;

				_values[index] = resource;
			}
			else
			{
				//LogDebuggerWarning(string.Format("The resource with a key {0} when verifying {1}'s ResourceDictionary is not valid. it should not be a {2}", id, this, entry.Value != null ? entry.Value.GetType().ToString() : "x:Null"));
				LogDebuggerWarning(SR.GetString("Provider_InvalidResource", id, this, resource != null ? resource.GetType().ToString() : "[null]"));

				return null;
			}

			return resource;
		}

		#endregion //GetResourceHelper

		#region LogDebuggerError

		internal static void LogDebuggerWarning(string warning)
		{
			// we use level 40 as a warning
			if (Debugger.IsAttached && Debugger.IsLogging())
				Debugger.Log(40, "Global", warning + Environment.NewLine);
		}

		#endregion //LogDebuggerError

		#endregion //Private Methods	
    
		#endregion //Methods

		// AS 2/23/12 TFS102032
		#region IResourceWasherTarget

		ResourceWasher IResourceWasherTarget.ResourceWasher
		{
			get
			{
				return _resourceWasher;
			}
			set
			{
				// note i'm not checking for a difference because it's possible
				// that the color/mode of the washer is being changed so we want 
				// to dirty
				_resourceWasher = value;
				this.InvalidateResources();
			}
		}

		#endregion //IResourceWasherTarget
	}

	#endregion //ResourceProviderWithOverrides class
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