using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Reflection;
using System.Collections;
using System.IO;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Text;
using Infragistics.Persistence.Primitives;

namespace Infragistics.Persistence
{
	/// <summary>
	/// A class that can save a <see cref="DependencyObject"/> in its current state and load it back.
	/// </summary>
	public class PersistenceManager
	{
		#region Static

		#region Members

		/// <summary>
		/// The types that should be ignored when saving and loading properties.
		/// </summary>
		private static List<Type> TypesToIgnore = new List<Type>()
		{
			typeof(ControlTemplate), 
			typeof(DataTemplate), 
			typeof (ItemsPanelTemplate), 
			typeof(Style),
            typeof(ResourceDictionary)
		};

        private static Dictionary<Type, TypeConverter> PredefinedTypeConverters = new Dictionary<Type, TypeConverter>();


        private static List<string> NewNamespaces = new List<string>()
        {
            "Infragistics",
            "Infragistics.Controls",
            "Infragistics.Controls.Primitives",
            "Infragistics.Controls.Editors",
            "Infragistics.Controls.Editors.Primitives",
            "Infragistics.Controls.Menus",
            "Infragistics.Controls.Menus.Primitives",
            "Infragistics.Controls.Interactions",
            "Infragistics.Controls.Interactions.Primitives",
            "Infragistics.Controls.Grids",
            "Infragistics.Controls.Grids.Primitives",
            "Infragistics.Controls.Lists",
            "Infragistics.Controls.Lists.Primitives",
        };

        private static Dictionary<string, string> UpgradeAssemblyDictionary = new Dictionary<string, string>();
                       

        /// <summary>
        /// Initializes static members of the <see cref="PersistenceManager"/>
        /// </summary>
        static PersistenceManager()
        {
            PersistenceManager.PredefinedTypeConverters.Add(typeof(TimeSpan), new TimeSpanTypeConverter());
            PersistenceManager.PredefinedTypeConverters.Add(typeof(Duration), new DurationTypeConverter());
            PersistenceManager.PredefinedTypeConverters.Add(typeof(RepeatBehavior), new RepeatBehaviorTypeConverter());

            PersistenceManager.UpgradeAssemblyDictionary.Add("Infragistics.Silverlight", "InfragisticsSL5");
            PersistenceManager.UpgradeAssemblyDictionary.Add("Infragistics.Silverlight.DragDrop", "InfragisticsSL5.DragDrop");
            PersistenceManager.UpgradeAssemblyDictionary.Add("Infragistics.Silverlight.Compression", "InfragisticsSL5.Compression");
            PersistenceManager.UpgradeAssemblyDictionary.Add("Infragistics.Silverlight.Excel", "InfragisticsSL5.Documents.Excel");
            PersistenceManager.UpgradeAssemblyDictionary.Add("Infragistics.Silverlight.Persistence", "InfragisticsSL5.Persistence");
            PersistenceManager.UpgradeAssemblyDictionary.Add("Infragistics.Silverlight.XamWebEditors", "InfragisticsSL5.Controls.Editors");
            PersistenceManager.UpgradeAssemblyDictionary.Add("Infragistics.Silverlight.XamWebMenu", "InfragisticsSL5.Controls.Menus.XamMenu");
            PersistenceManager.UpgradeAssemblyDictionary.Add("Infragistics.Silverlight.XamWebHtmlViewer", "InfragisticsSL5.Controls.Interactions.XamHtmlViewer");
            PersistenceManager.UpgradeAssemblyDictionary.Add("Infragistics.Silverlight.XamWebDialogWindow", "InfragisticsSL5.Controls.Interactions.XamDialogWindow");
            PersistenceManager.UpgradeAssemblyDictionary.Add("Infragistics.Silverlight.XamWebOutlookBar", "InfragisticsSL5.Controls.Menus.XamOutlookBar");
            PersistenceManager.UpgradeAssemblyDictionary.Add("Infragistics.Silverlight.XamWebRibbon", "InfragisticsSL5.Controls.Menus.XamRibbon");
            PersistenceManager.UpgradeAssemblyDictionary.Add("Infragistics.Silverlight.XamWebGrid", "InfragisticsSL5.Controls.Grids.XamGrid");
            PersistenceManager.UpgradeAssemblyDictionary.Add("Infragistics.Silverlight.XamWebTagCloud", "InfragisticsSL5.Controls.Menus.XamTagCloud");
            PersistenceManager.UpgradeAssemblyDictionary.Add("Infragistics.Silverlight.XamWebComboEditor ", "InfragisticsSL5.Controls.Editors.XamComboEditor");
            PersistenceManager.UpgradeAssemblyDictionary.Add("Infragistics.Silverlight.XamWebTree", "InfragisticsSL5.Controls.Menus.XamTree");
            PersistenceManager.UpgradeAssemblyDictionary.Add("Infragistics.Silverlight.VirtualCollection", "InfragisticsSL5.Collections.VirtualCollection");
            PersistenceManager.UpgradeAssemblyDictionary.Add("Infragistics.Silverlight.XamWebSlider", "InfragisticsSL5.Controls.Editors.XamSlider");
            PersistenceManager.UpgradeAssemblyDictionary.Add("Infragistics.Silverlight.XamWebSpellChecker", "InfragisticsSL5.Controls.Interactions.XamSpellChecker");
            PersistenceManager.UpgradeAssemblyDictionary.Add("Infragistics.Silverlight.XamWebTileView", "InfragisticsSL5.Controls.Lists.XamTileView");
            PersistenceManager.UpgradeAssemblyDictionary.Add("Infragistics.Silverlight.Chart", "InfragisticsSL5.Charts.XamWebChart");
        }


		#endregion // Members

		#region Properties

		#region Public

		#region Settings

		/// <summary>
		/// An Attached Property for storing <see cref="PersistenceSettings"/> on a <see cref="DependencyObject"/>.
		/// </summary>
		public static readonly DependencyProperty SettingsProperty = DependencyProperty.RegisterAttached("Settings", typeof(PersistenceSettings), typeof(PersistenceManager), new PropertyMetadata(null));

		/// <summary>
		/// Gets the <see cref="PersistenceSettings"/> on a <see cref="DependencyObject"/>.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns>Null if not set</returns>
		public static PersistenceSettings GetSettings(DependencyObject obj)
		{
			return (PersistenceSettings)obj.GetValue(SettingsProperty);
		}

		/// <summary>
		/// Sets the <see cref="PersistenceSettings"/> on a <see cref="DependencyObject"/>.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="settings"></param>
		public static void SetSettings(DependencyObject obj, PersistenceSettings settings)
		{
			obj.SetValue(SettingsProperty, settings);
		}

		#endregion // Settings

		#region PersistenceGroup

		/// <summary>
		/// An Attached Property for storing <see cref="PersistenceSettings"/> on a <see cref="DependencyObject"/>.
		/// </summary>
		public static readonly DependencyProperty PersistenceGroupProperty = DependencyProperty.RegisterAttached("PersistenceGroup", typeof(PersistenceGroup), typeof(PersistenceManager), new PropertyMetadata(new PropertyChangedCallback(PersistenceGroupChanged)));

		/// <summary>
		/// Gets the <see cref="PersistenceGroup"/> on a <see cref="DependencyObject"/>.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns>Null if not set</returns>
		public static PersistenceGroup GetPersistenceGroup(DependencyObject obj)
		{
			return (PersistenceGroup)obj.GetValue(PersistenceGroupProperty);
		}

		/// <summary>
		/// Sets the <see cref="PersistenceGroup"/> on a <see cref="DependencyObject"/>.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="group"></param>
		public static void SetPersistenceGroup(DependencyObject obj, PersistenceGroup group)
		{
			obj.SetValue(PersistenceGroupProperty, group);
		}
				
		private static void PersistenceGroupChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			if (e.OldValue != null)
			{
				((PersistenceGroup)e.OldValue).UnregisterObject(obj);
			}

			if (e.NewValue != null)
			{
				((PersistenceGroup)e.NewValue).RegisterObject(obj);
			}
		}

		#endregion // PersistenceGroup

		#region Identifier

		/// <summary>
		/// An Attached Property that allows a user to specify an identifier that can be used to identify an object when its being saved or loaded.
		/// </summary>
		public static readonly DependencyProperty IdentifierProperty = DependencyProperty.RegisterAttached("Identifier", typeof(string), typeof(PersistenceManager), new PropertyMetadata(null));

		/// <summary>
		/// Gets the identifier specified on a <see cref="DependencyObject"/>.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public static string GetIdentifier(DependencyObject obj)
		{
			return (string)obj.GetValue(IdentifierProperty);
		}

		/// <summary>
		/// Sets the identifier on on a <see cref="DependencyObject"/>.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="id"></param>
		public static void SetIdentifier(DependencyObject obj, string id)
		{
			obj.SetValue(IdentifierProperty, id);
		}

		#endregion // Identifier

		#endregion // Public

		#endregion // Properties

		#region Methods

        private static Type FixAssemblyQualifiedName(string oldAssemblyQualifiedName)
        {
            Type type = null;

            // First identify the the AssemblyName
            int start = oldAssemblyQualifiedName.IndexOf(", ") + 2;
            int end = oldAssemblyQualifiedName.IndexOf(".v", start);

            string oldAssemblyName = oldAssemblyQualifiedName.Substring(start, end - start);
            string newAssemblyName = "";

            string assemblyName = typeof(PersistenceManager).AssemblyQualifiedName;

            // Also find the revision number: 10.2.2010.2050, the revsion number would be 2050
            start = assemblyName.IndexOf("Version=") + 8;
            end = assemblyName.IndexOf(",", start);
            string[] revisionParts = assemblyName.Substring(start, end - start).Split('.');
            string revision = revisionParts[revisionParts.Length - 1];

            // Find the revision number for the old assembly: 10.2.2010.2050, the revsion number would be 2050
            start = oldAssemblyQualifiedName.IndexOf("Version=") + 8;
            end = oldAssemblyQualifiedName.IndexOf(",", start);
            revisionParts = oldAssemblyQualifiedName.Substring(start, end - start).Split('.');
            string oldRevision = revisionParts[revisionParts.Length - 1];

            oldAssemblyQualifiedName = oldAssemblyQualifiedName.Replace("." + oldRevision, "." + revision);

            // Look in our dictionary of assembly names, and see if we can upgrade it. 
            if (PersistenceManager.UpgradeAssemblyDictionary.TryGetValue(oldAssemblyName, out newAssemblyName))
            {
                string newAssemblyQualifiedName = oldAssemblyQualifiedName.Replace(oldAssemblyName, newAssemblyName);

                // Do another test to see if that fixed the type.
                type = Type.GetType(newAssemblyQualifiedName);

                if (type == null)
                {
                    // break the AssemblyQualifiedName into parts, so that we can identify the class name
                    string[] typeParts = newAssemblyQualifiedName.Split(',');

                    string oldClassNameWithNamespace = typeParts[0];

                    start = oldClassNameWithNamespace.LastIndexOf('[');
                    if (start != -1)
                    {
                        start++;
                        oldClassNameWithNamespace = oldClassNameWithNamespace.Substring(start, oldClassNameWithNamespace.Length - start);
                    }

                    string[] classNameParts = oldClassNameWithNamespace.Split('.');

                    // Remove the Namespace
                    string className = classNameParts[classNameParts.Length - 1];

                    className = className.Replace("XamWeb", "Xam");

                    // Walk through our list of possible namespaces and try resolving the type.
                    foreach (string newNamespace in PersistenceManager.NewNamespaces)
                    {
                        string tempTypeName = newAssemblyQualifiedName.Replace(oldClassNameWithNamespace, newNamespace + "." + className);

                        type = Type.GetType(tempTypeName);

                        if (type != null)
                        {
                            newAssemblyQualifiedName = tempTypeName;
                            break;
                        }
                    }
                }
            }

            return type;
        }

		#region Save

		/// <summary>
		/// Saves all objects in the group into a single MemoryStream.
		/// </summary>
		/// <param name="group">The group that contains the objects to save.</param>
		public static MemoryStream Save(PersistenceGroup group)
		{
			return PersistenceManager.SaveGroup(group, null);
		}

		/// <summary>
		/// Saves the properties of an object into a <see cref="MemoryStream"/>
		/// </summary>
		/// <param name="obj">The object that should be saved.</param>
		public static MemoryStream Save(DependencyObject obj)
		{
			return PersistenceManager.Save(obj, PersistenceManager.GetSettings(obj));
		}

		/// <summary>
		/// Saves the properties of an object into a <see cref="MemoryStream"/>
		/// </summary>
		/// <param name="obj">The object that should be saved.</param>
		/// <param name="settings">The settings that should be followed while saving properties of the object.</param>
		public static MemoryStream Save(DependencyObject obj, PersistenceSettings settings)
		{
			PersistenceGroup pg = new PersistenceGroup();
			pg.RegisterObject(obj);
			return PersistenceManager.SaveGroup(pg, settings);
		}

		private static MemoryStream SaveGroup(PersistenceGroup group, PersistenceSettings groupSettings)
		{
			Dictionary<int, string> typeDataStore = new Dictionary<int, string>();
			Collection<Type> valueTypes = new Collection<Type>();
			Collection<ElementData> elements = new Collection<ElementData>();

			GroupData groupData = new GroupData();

			foreach (DependencyObject obj in group.RegisteredObjects)
			{
				ElementData data = new ElementData();

				PersistenceSettings settings = groupSettings;

				if (settings == null)
					settings = PersistenceManager.GetSettings(obj);

				// If settings weren't specified, use an empty one.
				if (settings == null)
					settings = new PersistenceSettings();

				// Create a new instance of the PersistenceManager
				PersistenceManager cpm = new PersistenceManager(group, settings, obj, typeDataStore, valueTypes);

				// Now save all properties of the object. 
				data.PropertyTree = cpm.BuildPropertyTree();

				// Loop through all the PropertyData that were stored and store them in a format so that they can be persisted. 
				Collection<PropertyDataPair> dataKeys = new Collection<PropertyDataPair>();
				foreach (KeyValuePair<int, PropertyData> key in cpm.PropertyDataStore)
					dataKeys.Add(new PropertyDataPair() { LookupKey = key.Key, Data = key.Value });

				data.PropDataStore = dataKeys;
				data.TypeLookupKey = cpm.StoreType(obj.GetType());

				elements.Add(data);
			}

			groupData.Elements = elements;

			// Loop through all the Types that were stored and store them in a format so that they can be persisted. 
			Collection<TypeDataPair> typeKeys = new Collection<TypeDataPair>();
			foreach (KeyValuePair<int, string> key in typeDataStore)
				typeKeys.Add(new TypeDataPair() { LookupKey = key.Key, TypeName = key.Value });

			groupData.TypeStore = typeKeys;

			// Loop through all ValueTypes and save them in a format that can be persisted.
			Collection<String> types = new Collection<string>();
			foreach (Type t in valueTypes)
			{
				types.Add(t.AssemblyQualifiedName);
			}

			// Saving is a 2 step proccess. 

			// Step 1 - Save using an Array of ValueTypes. 
			XmlSerializer xs = new XmlSerializer(typeof(GroupData), valueTypes.ToArray());
			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);
			xs.Serialize(sw, groupData);

			MemoryStream stream = new MemoryStream();

			// Step2 - Create another object that will save the previous data as xml, and the type array used to create the xml.
			PersistenceObject po = new PersistenceObject() { Types = types, Xml = sb.ToString() };
			xs = new XmlSerializer(typeof(PersistenceObject));
			xs.Serialize(stream, po);

			stream.Position = 0;

			PersistenceSavedEventArgs args = new PersistenceSavedEventArgs();
			group.Events.OnPersistenceSaved(args);

			return stream;
		}

		#endregion // Save

		#region Load

		/// <summary>
		/// Loads all objects of a group from a stream.
		/// </summary>
		/// <param name="group">The group that contains the objects that should be loaded into.</param>
		/// <param name="stream">The stream in which the object information should be loaded from.</param>
		public static void Load(PersistenceGroup group, Stream stream)
		{
			PersistenceManager.LoadGroup(group, stream, null);
		}

		/// <summary>
		/// Loads the properties of an object from a <see cref="Stream"/>
		/// </summary>
		/// <param name="obj">The object that should be filled.</param>
		/// <param name="stream">The stream in which the object information should be loaded from.</param>
		public static void Load(DependencyObject obj, Stream stream)
		{
			PersistenceManager.Load(obj, stream, PersistenceManager.GetSettings(obj));			
		}

		/// <summary>
		/// Loads the properties of an object from a <see cref="Stream"/>
		/// </summary>
		/// <param name="obj">The object that should be filled.</param>
		/// <param name="stream">The stream in which the object information should be loaded from.</param>
		/// <param name="settings">The settings that should be followed while loading properties onto the object.</param>
		public static void Load(DependencyObject obj, Stream stream, PersistenceSettings settings)
		{
			PersistenceGroup pg = new PersistenceGroup();
			pg.RegisterObject(obj);
			PersistenceManager.LoadGroup(pg, stream, settings);
		}

		private static void LoadGroup(PersistenceGroup group, Stream stream, PersistenceSettings settings)
		{
			List<PropertyPersistenceExceptionDetails> exceptions = new List<PropertyPersistenceExceptionDetails>();

			// As with saving, loading is a 2 Step Proccess.
			// Step 1 - Deserialize the xml and list of types.
			XmlSerializer xs = new XmlSerializer(typeof(PersistenceObject));
			PersistenceObject po = xs.Deserialize(stream) as PersistenceObject;
			if (po != null)
			{
				// Get the xml
				StringReader sr = new StringReader(po.Xml);

				// Rehydrate the Type[] of ValueTypes that were saved.
				Type[] types = new Type[po.Types.Count];
				for (int i = 0; i < po.Types.Count; i++)
					types[i] = Type.GetType(po.Types[i]);

                string oldVersion = "";

                // This means that we're probably loading a different version of our controls.
                // i.e. they were persisted in 10.2, but the user has now upgraded to 10.3
                if (types.Contains(null))
                {
                    // First reset the stream and load the xml.
                    stream.Seek(0, SeekOrigin.Begin);
                    StreamReader reader = new StreamReader(stream);
                    string xml = reader.ReadToEnd();

                    // Step 2: find the new version number, under the assumption that its the same as
                    // the Persistence assembly.
                    string assemblyName = group.GetType().Assembly.FullName;
                    int start = assemblyName.IndexOf(".v") + 2;
                    int end = assemblyName.IndexOf(",", start);
                    string version = assemblyName.Substring(start, end - start);                   

                    // Step 3: find an infragistics assembly and resolve its version
                    start = xml.IndexOf("Infragistics");
                    start = xml.IndexOf(".v", start) + 2;
                    end = xml.IndexOf(",", start);
                    oldVersion = xml.Substring(start, end - start);

                    if (oldVersion == "10.1")
                    {
                        Dictionary<string, string> updates = new Dictionary<string, string>();

                        foreach (string oldAssemblyQualifiedName in po.Types)
                        {
                            Type type = Type.GetType(oldAssemblyQualifiedName);

                            // If the type is null, then we weren't able to resolve, so lets try fixing it.
                            if (type == null)
                            {
                                string name = oldAssemblyQualifiedName.Replace(oldVersion, version);
                                name = name.Replace(oldVersion.Replace(".", "") + ".", version.Replace(".", "") + ".");

                                type = PersistenceManager.FixAssemblyQualifiedName(name);

                                if(type != null)
                                {
                                    updates.Add(oldAssemblyQualifiedName, type.AssemblyQualifiedName);
                                }
                            }
                        }

                        foreach (KeyValuePair<string, string> update in updates)
                        {
                            xml = xml.Replace(update.Key, update.Value);
                        }

                    }
                    else
                    {
                        foreach (string oldAssemblyQualifiedName in po.Types)
                        {
                            Type type = Type.GetType(oldAssemblyQualifiedName);
                            if (type == null)
                            {
                                // First identify the the AssemblyName
                                start = oldAssemblyQualifiedName.IndexOf(", ") + 2;
                                end = oldAssemblyQualifiedName.IndexOf(".v", start);

                                string oldAssemblyName = oldAssemblyQualifiedName.Substring(start, end - start);

                                assemblyName = typeof(PersistenceManager).AssemblyQualifiedName;

                                // Also find the revision number: 10.2.2010.2050, the revsion number would be 2050
                                start = assemblyName.IndexOf("Version=") + 8;
                                end = assemblyName.IndexOf(",", start);
                                string[] revisionParts = assemblyName.Substring(start, end - start).Split('.');
                                string revision = revisionParts[revisionParts.Length - 1];

                                // Find the revision number for the old assembly: 10.2.2010.2050, the revsion number would be 2050
                                start = oldAssemblyQualifiedName.IndexOf("Version=") + 8;
                                end = oldAssemblyQualifiedName.IndexOf(",", start);
                                revisionParts = oldAssemblyQualifiedName.Substring(start, end - start).Split('.');
                                string oldRevision = revisionParts[revisionParts.Length - 1];

                                xml = xml.Replace("." + oldRevision, "." + revision); // ("10.2", "10.3")                                
                                break;
                            }
                        }
                    }

                    // Update the xml, by replacing the old version with the new version.
                    //xml = xml.Replace(oldVersion, version); // ("10.2", "10.3")
                    




                    xml = xml.Replace("v" + oldVersion, "v" + version);
                    xml = xml.Replace("=" + oldVersion, "=" + version);
                    xml = xml.Replace(oldVersion.Replace(".", "") + ".", version.Replace(".", "") + "."); // ("102.", "103.")
                    
                    xml = xml.Replace("InfragisticsSL4", "InfragisticsSL5");

                    // Deserialize again, and hopefully this will fix it.
                    po = xs.Deserialize(new StringReader(xml)) as PersistenceObject;

                    sr = new StringReader(po.Xml);
                    types = new Type[po.Types.Count];

                    for (int i = 0; i < po.Types.Count; i++)
                    {
                        types[i] = Type.GetType(po.Types[i]);
                    }
                }

				// Step 2 - Now deserialzie the acutall ElementData. 
				xs = new XmlSerializer(typeof(GroupData), types);
				GroupData groupData = xs.Deserialize(sr) as GroupData;

				// Rehydrate the Type lookup table.
				Dictionary<int, string> typeStore = new Dictionary<int, string>();

                if (oldVersion == "10.1")
                {
                    foreach (TypeDataPair key in groupData.TypeStore)
                    {
                        Type type = Type.GetType(key.TypeName);

                        if (type != null)
                        {
                            typeStore.Add(key.LookupKey, key.TypeName);
                        }
                        else
                        {

                            type = PersistenceManager.FixAssemblyQualifiedName(key.TypeName);
                            
                            if (type != null)
                            {
                                typeStore.Add(key.LookupKey, type.AssemblyQualifiedName);
                            }
                        }
                    }
                }
                else
                {
                    foreach (TypeDataPair key in groupData.TypeStore)
                        typeStore.Add(key.LookupKey, key.TypeName);
                }

				if (groupData != null)
				{
					int elementCount = groupData.Elements.Count;
					if (elementCount == group.RegisteredObjects.Count)
					{
						for (int i = 0; i < elementCount; i++)
						{
							ElementData data = groupData.Elements[i];
							DependencyObject obj = group.RegisteredObjects[i];

							if (settings == null)
								settings = PersistenceManager.GetSettings(obj);

							// If settings weren't specified, use an empty one.
							if (settings == null)
								settings = new PersistenceSettings();

                            INeedInitializationForPersistence iNeedInit = obj as INeedInitializationForPersistence;
                            if (iNeedInit != null)
                            {
                                iNeedInit.InitObject(null);
                            }

							// Create a new PersistenceManager and hydrate the specified object.
							PersistenceManager pm = new PersistenceManager(group, settings, obj, typeStore, new Collection<Type>());

							if (pm.ResolveType(data.TypeLookupKey) == obj.GetType())
							{								
								// Rehydrate the PropertyData lookup table.
								Dictionary<int, PropertyData> propDataStore = new Dictionary<int, PropertyData>();
								foreach (PropertyDataPair key in data.PropDataStore)
									propDataStore.Add(key.LookupKey, key.Data);


								pm.PropertyDataStore = propDataStore;
								pm.UnloadPropertyTree(data.PropertyTree);
								
								exceptions.AddRange(pm.Exceptions);
							}
							else
							{
								throw new InvalidPersistenceGroupException();
							}
						}
					}
					else
					{
						throw new InvalidPersistenceGroupException();
					}
				}
			}

			PersistenceLoadedEventArgs args = new PersistenceLoadedEventArgs(new ReadOnlyCollection<PropertyPersistenceExceptionDetails>(exceptions));
			group.Events.OnPersistenceLoaded(args);
		}

		#endregion // Load

		#endregion // Methods

		#endregion // Static

		#region Members

		Dictionary<int, string> _typeStore;
		Dictionary<object, int> _objectStore;
		Dictionary<int, object> _reverseObjectStore;
		Dictionary<int, PropertyData> _propDataStore;
		PersistenceSettings _settings;
		DependencyObject _rootObject;
		List<PropertyPersistenceExceptionDetails> _exceptions;
		Collection<Type> _valueTypes;
		PersistenceGroup _group;

		#endregion // Members

		#region Constructor

		private PersistenceManager(PersistenceGroup group, PersistenceSettings settings, DependencyObject rootObject, Dictionary<int, string> typeDataStore, Collection<Type> valueTypes)
		{
			this._group = group;
			this._typeStore = typeDataStore;
			this._objectStore = new Dictionary<object, int>();
			this._reverseObjectStore = new Dictionary<int, object>();
			this._propDataStore = new Dictionary<int, PropertyData>();

			this._settings = settings;
			this._rootObject = rootObject;

			this._valueTypes = valueTypes;

			this._exceptions = new List<PropertyPersistenceExceptionDetails>();
		}

		#endregion // Constructor

		#region Methods

		#region Public

		/// <summary>
		/// Walks through the root object in which this <see cref="PersistenceManager"/> represents, and build a recursive list of <see cref="PropertyDataInfo"/> that can be used later to rehydrate the object.
		/// </summary>
		/// <returns></returns>
		public Collection<PropertyDataInfo> BuildPropertyTree()
		{
			this.StoreProperty(this._rootObject, null);

			Collection<PropertyDataInfo> propertyTree = this.BuildPropertyTree("", this._rootObject, true);

			PersistenceSavedEventArgs args = new PersistenceSavedEventArgs();
			this._settings.Events.OnPersistenceSaved(args);			

			return propertyTree;
		}

		/// <summary>
		/// Walks through a tree of <see cref="PropertyDataInfo"/> objects, and attempts to rehydrate the root object in which this <see cref="PersistenceManager"/> represents, with those values.
		/// </summary>
		/// <param name="tree"></param>
		public void UnloadPropertyTree(Collection<PropertyDataInfo> tree)
		{
			this._reverseObjectStore.Add(0, this._rootObject);
			this._propDataStore.Add(0, new PropertyData() { Properties = tree });
			this.UnloadPropertyTree(this._rootObject, tree, true);

			PersistenceLoadedEventArgs args = new PersistenceLoadedEventArgs(new ReadOnlyCollection<PropertyPersistenceExceptionDetails>(this._exceptions));
			this._settings.Events.OnPersistenceLoaded(args);
		}

		#endregion // Public

		#region Private

		#region BuildPropertyTree
		private Collection<PropertyDataInfo> BuildPropertyTree(string propertyPath, object obj, bool validateAgainstIgnore)
		{
			Collection<PropertyDataInfo> tree = new Collection<PropertyDataInfo>();
			if (obj != null)
			{
                IProvideCustomObjectPersistence ipcop = obj as IProvideCustomObjectPersistence;
                if (ipcop != null)
                {
                    object val = ipcop.SaveObject();

                    if (val != null)
                    {
                        tree.Add(this.CreateAndStorePropertyData(val, "", "PersistenceCustomObject", val.GetType(), null, null, null, this.BuildPropertyTree("", val, false)));
                    }
                    if (this._settings == null || this._settings.SavePersistenceOptions != PersistenceOption.OnlySpecified)
                        return tree;
                }

                bool objIsControl = obj is FrameworkElement;
                IProvidePropertyPersistenceSettings settings = obj as IProvidePropertyPersistenceSettings;

                Type objType = obj.GetType();

                if (settings != null && settings.PriorityProperties != null)
                {
                    foreach (string propertyName in settings.PriorityProperties)
                    {
                        PropertyInfo pi = objType.GetProperty(propertyName);
                        if (pi != null)
                        {
                            object propVal = pi.GetValue(obj, null);

                            PropertyDataInfo data = this.SaveProperty(propertyPath, propVal, pi, validateAgainstIgnore);

                            if (data != null)
                                tree.Add(data);
                        }
                    }
                }

                List<string> propsToIgnore = null;

                if (settings != null)
                    propsToIgnore = settings.PropertiesToIgnore;


				PropertyInfo[] properties = objType.GetProperties();

				foreach (PropertyInfo pi in properties)
				{
					// JJD 07/25/12 - TFS117739
					// Added logic below to always ignore the DataContext property. This prevents
					// a possible stack overflow on a subsequent load
					//if (objIsControl && pi.Name == "Name")
					//    continue;
					switch (pi.Name)
					{
						case "Name":
							if (objIsControl)
								continue;
							break;

						case "DataContext":
							continue;
					}

                    if (PersistenceManager.TypesToIgnore.Contains(objType))
                        continue;

                    if (propsToIgnore != null && propsToIgnore.Contains(pi.Name))
                    {
                        if (this._settings != null)
                        {
                            string path = propertyPath;

                            if (path.Length > 0)
                                path += ".";

                            path += pi.Name;

                            // Has anyone specified a setting that matches this property?
                            PropertyPersistenceInfoBase ppi = this._settings.PropertySettings.GetPropertyPersistenceInfo(pi, path);
                            
                            if (ppi == null)
                                continue;  
                        }
                        else
                        {
                            continue;
                        }
                    }

                    object propVal = null;

                    bool skipProperty = false;  

                    try
                    {
                        propVal = pi.GetValue(obj, null);
                    }
                    catch (Exception)
                    {
                        skipProperty = true;
                        // For whatever reason, an exception was thrown in the getter of a property. 
                        // This specifically happens in SL4 TextBox.InputScope
                    }

                    if (!skipProperty)
                    {
                        PropertyDataInfo data = this.SaveProperty(propertyPath, propVal, pi, validateAgainstIgnore);

                        if (data != null)
                            tree.Add(data);
                    }
				}
			}
			return tree;
		}
		#endregion // BuildPropertyTree

		#region SaveProperty

        int _currentPropertyDataKey;
		private PropertyDataInfo SaveProperty(string propertyPath, object value, PropertyInfo pi, bool validateAgainstIgnore)
		{
            this._currentPropertyDataKey = -1;

			// Determine the PropertyName and its PropertyPath
			string propertyName = "";
			if (pi != null)
			{
				propertyName = pi.Name;

				if (propertyPath.Length > 0)
					propertyPath += ".";

				propertyPath += propertyName;
			}

            // Skip static properties
            if (pi != null && ((pi.CanRead && pi.GetGetMethod() != null && pi.GetGetMethod().IsStatic) || (pi.CanWrite && pi.GetSetMethod() != null && pi.GetSetMethod().IsStatic)))
            {
                return null;
            }

			// Look at the property
			// If its readonly, then see if it has any properties that are writeable
			// Otherwise, don't bother continuing as we can't save any information on it. 
			if (pi != null && pi.GetSetMethod() == null)
			{
				List<PropertyInfo> properties = (from property in pi.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
						where property.CanWrite
						select property).ToList<PropertyInfo>();

				if (value == null || (properties.Count == 0  && !(value is IEnumerable))|| value is FrameworkElement)
					return null;
			}

			if (value == null && pi == null)
				return null;

			Type valueType = (value != null) ? value.GetType() : pi.PropertyType;

            // Have we saved off this value before?
            // If so, no need to continue
            if (value != null && this._objectStore.ContainsKey(value))
            {
                if(valueType.IsValueType)
                    this._currentPropertyDataKey = this._objectStore[value];
                else
                    return new PropertyDataInfo() { PropertyName = propertyName, PropertyPath = propertyPath, DataKey = this._objectStore[value] };
            }

			PersistenceOption option = this._settings.SavePersistenceOptions;

			// Has anyone specified a setting that matches this property?
			PropertyPersistenceInfoBase ppi = this._settings.PropertySettings.GetPropertyPersistenceInfo(pi, propertyPath);

			// If they specified a setting for this property, lets honor it. 
			if (ppi != null)
			{
				string saveVal = ppi.SaveValueToString(pi, propertyPath, value);
				if (string.IsNullOrEmpty(saveVal))
					saveVal = ppi.SaveValue;

				if (!string.IsNullOrEmpty(saveVal))
				{
					return this.CreateAndStorePropertyData(value, propertyName, propertyPath, valueType, null, ppi.SaveValue, null, null);
				}

				if (ppi.TypeConverter != null && ppi.TypeConverter.CanConvertFrom(typeof(string)) && ppi.TypeConverter.CanConvertTo(typeof(string)))
				{
					return this.CreateAndStorePropertyData(value, propertyName, propertyPath, valueType, null, ppi.TypeConverter.ConvertToString(value), ppi.TypeConverter.GetType().AssemblyQualifiedName, null);
				}

				if (!string.IsNullOrEmpty(ppi.Identifier))
				{
					return this.CreateAndStorePropertyData(value, propertyName, propertyPath, valueType, ppi.Identifier, null, null, null);
				}
			}
			else 
			{
				// If we're only saving specified properties, then we know we can just exit it out now, b/c we didn't find one.
				if (option == PersistenceOption.OnlySpecified && validateAgainstIgnore)
				{
                    if (value != null)
                    {
                        ppi = this._settings.IgnorePropertySettings.GetPropertyPersistenceInfo(pi, propertyPath);

					    // If we found a propertySetting, then we need to ignore it, so return null.
                        if (ppi != null)
                        {
                            return null;
                        }

                        IEnumerable collectionValue = value as IEnumerable;

                        if (collectionValue != null && value.GetType() != typeof(string))
                        {
                            Collection<PropertyDataInfo> collectionProps = new Collection<PropertyDataInfo>();

                            foreach (object item in collectionValue)
                            {
                                PropertyDataInfo key = this.SaveProperty(propertyPath + "[]", item, null, true);

                                if (key == null)
                                {
                                    // We may have stored off the value. However, if we did and didn't use it, we need to dispose of it.
                                    if (value != null && this._objectStore.ContainsKey(value))
                                    {
                                        int storeKey = this._objectStore[value];
                                        if (!this._propDataStore.ContainsKey(storeKey))
                                            this._objectStore.Remove(value);
                                    }

                                    return null;
                                }
                                else
                                    collectionProps.Add(key);
                            }

                            if (collectionProps.Count > 0)
                            {
                                Collection<string> lookupKeys = null;
                                IProvidePersistenceLookupKeys collectionObj = collectionValue as IProvidePersistenceLookupKeys;
                                if (collectionObj != null)
                                    lookupKeys = collectionObj.GetLookupKeys();

                                return this.CreateAndStorePropertyData(value, propertyName, propertyPath, valueType, null, false, null, null, collectionProps, lookupKeys);
                            }

                            
                        }
                        else if (valueType.IsValueType || valueType.IsEnum)
			            {
                            
			            }
				        // If we're an primitive type, enum, or string, just store off the to string of the object.
				        else if (valueType.IsEnum || value is string || valueType.IsPrimitive || value is Color)
				        {
        					
				        }
				        // Check to see if it has a public empty ctor, if so, then lets build a property propertyTree for it, so we can recreate it.
                        else if (!PersistenceManager.TypesToIgnore.Contains(valueType) && valueType.GetConstructor(new Type[] { }) != null)
                        {
                            // Avoid Infinite Recursion. 
                            // Store the property off, so that we know we're already handling this object.
                            this.StoreProperty(value, null);

                            Collection<PropertyDataInfo> info = this.BuildPropertyTree(propertyPath, value, true);
                            if (info.Count > 0)
                            {
                                return this.CreateAndStorePropertyData(value, propertyName, propertyPath, valueType, null, null, null, info);
                            }
                        }   
                    }

					return null;
				}
				else
				{
					// Has anyone specified a setting that matches this property that should be ignored?
					ppi = this._settings.IgnorePropertySettings.GetPropertyPersistenceInfo(pi, propertyPath);

					// If we found a propertySetting, then we need to ignore it, so return null.
					if (ppi != null)
					{
                        // We may have stored off the value. However, if we did and didn't use it, we need to dispose of it.
                        if (value != null && this._objectStore.ContainsKey(value))
                        {
                            int storeKey = this._objectStore[value];
                            if (!this._propDataStore.ContainsKey(storeKey))
                                this._objectStore.Remove(value);
                        }

						return null;
					}
				}
			}
									
			// Fire the SaveProperty Event.
			SavePropertyPersistenceEventArgs args = new SavePropertyPersistenceEventArgs(this._rootObject, pi, propertyPath, value);
			this._settings.Events.OnSavePropertyPersistence(args);
			this._group.Events.OnSavePropertyPersistence(args);

			// The user doesn't want this property saved. 
			if (args.Cancel)
			{
                // We may have stored off the value. However, if we did and didn't use it, we need to dispose of it.
                if (value != null && this._objectStore.ContainsKey(value))
                {
                    int storeKey = this._objectStore[value];
                    if (value != null && !this._propDataStore.ContainsKey(storeKey))
                        this._objectStore.Remove(value);
                }

                return null;
			}

			// Honor any setting set in the event.
			if (!string.IsNullOrEmpty(args.SaveValue))
			{
				return this.CreateAndStorePropertyData(value, propertyName, propertyPath, valueType, null, args.SaveValue, null, null);
			}

			if (!string.IsNullOrEmpty(args.Identifier))
			{
				return this.CreateAndStorePropertyData(value, propertyName, propertyPath, valueType, args.Identifier, null, null, null);
			}

			value = args.Value;

            if (value != null)
            {
                // Avoid Infinite Recursion. 
                // Store the property off, so that we know we're already handling this object.
                this.StoreProperty(value, null);

                DependencyObject dObj = value as DependencyObject;
                if (dObj != null)
                {
                    string id = PersistenceManager.GetIdentifier(dObj);
                    if (!string.IsNullOrEmpty(id))
                    {
                        return this.CreateAndStorePropertyData(value, propertyName, propertyPath, valueType, id, null, null, null);
                    }
                }

                // Check and see if the types are supported. 
                if (PersistenceManager.TypesToIgnore.Contains(valueType) || (valueType.IsNotPublic && !typeof(Type).IsAssignableFrom(valueType)))
                {
                    int key = this._objectStore[value];
                    if (!this._propDataStore.ContainsKey(key))
                        this._objectStore.Remove(value);

                    return null;
                }

                IProvideCustomPersistence pcs = value as IProvideCustomPersistence;
                if (pcs != null)
                {
                    return this.CreateAndStorePropertyData(value, propertyName, propertyPath, valueType, null, pcs.Save(), null, null);
                }

                bool canConvertTo = false, canConvertFrom = false;
                string val = null, converterTypeName = null;


                if (PersistenceManager.PredefinedTypeConverters.ContainsKey(valueType))
                {
                    TypeConverter converter = PersistenceManager.PredefinedTypeConverters[valueType];
                    val = converter.ConvertToString(value);
                    canConvertFrom = canConvertTo = true;
                    converterTypeName = converter.GetType().FullName;
                }
                else
                {
                    // Check and see if there are any TypeConverters attached to the object. 
                    object[] typeConverters = (pi != null) ? pi.GetCustomAttributes(typeof(TypeConverterAttribute), true) : null;
                    if (typeConverters == null || typeConverters.Length == 0)
                        typeConverters = valueType.GetCustomAttributes(typeof(TypeConverterAttribute), true);

                    // If there area type converters, determine if we can ConvertTo string, and ConvertFrom string
                    foreach (TypeConverterAttribute attr in typeConverters)
                    {
                        TypeConverter converter = Type.GetType(attr.ConverterTypeName).GetConstructor(new Type[] { }).Invoke(null) as TypeConverter;

                        Type t = converter.GetType();
                        if (converter.CanConvertTo(typeof(string)))
                        {
                            canConvertTo = true;
                            val = converter.ConvertToString(value);
                        }

                        if (converter.CanConvertFrom(typeof(string)))
                        {
                            converterTypeName = attr.ConverterTypeName;
                            canConvertFrom = true;
                        }


                        try
                        {
                            // in WPF converter.CanConvertTo(typeof(string)) will always return true
                            // even if the converter doesn't really support it, b/c it will just call .ToString() on it
                            if (canConvertFrom)
                                converter.ConvertFromString(val);
                        }
                        catch (Exception)
                        {
                            canConvertTo = false;
                        }

                    }
                }

                IEnumerable collectionValue = value as IEnumerable;

                // If se can convert to and from strings, lets store off the converter information. 
                if (canConvertFrom && canConvertTo)
                {
                    return this.CreateAndStorePropertyData(value, propertyName, propertyPath, valueType, null, val, converterTypeName, null);
                }
                // Handle IEnumerable properties
                else if (collectionValue != null && value.GetType() != typeof(string))
                {
                    Collection<PropertyDataInfo> collectionProps = new Collection<PropertyDataInfo>();

                    foreach (object item in collectionValue)
                    {
                        PropertyDataInfo key = this.SaveProperty(propertyPath + "[]", item, null, false);

                        if (key == null)
                        {
                            // We may have stored off the value. However, if we did and didn't use it, we need to dispose of it.
                            if (value != null && this._objectStore.ContainsKey(value))
                            {
                                int storeKey = this._objectStore[value];
                                if (!this._propDataStore.ContainsKey(storeKey))
                                    this._objectStore.Remove(value);
                            }

                            return null;
                        }
                        else
                            collectionProps.Add(key);
                    }

                    Collection<string> lookupKeys = null;
                    IProvidePersistenceLookupKeys collectionObj = collectionValue as IProvidePersistenceLookupKeys;
                    if (collectionObj != null)
                        lookupKeys = collectionObj.GetLookupKeys();

                    return this.CreateAndStorePropertyData(value, propertyName, propertyPath, valueType, null, false, null, null, collectionProps, lookupKeys);

                }
                else if (valueType.IsValueType || valueType.IsEnum)
                {
                    bool save = true;

                    // We could be dealing with a Struct, such as Size or Rect, which have Empty Members. 
                    // The problem with saving Empty objects, is that they don't have legal values
                    // For example, Size sets the Height and Width to Infinity, which you can't recreate.
                    PropertyInfo emptyPi = valueType.GetProperty("IsEmpty");
                    if (emptyPi != null)
                    {
                        object emptyVal = emptyPi.GetValue(value, null);
                        if (emptyVal != null)
                            save = !(bool)emptyVal;
                    }

                    if (save)
                        return this.CreateAndStorePropertyData(value, propertyName, propertyPath, valueType, null, true, null, null, null);
                }
                // If we're an primitive type, enum, or string, just store off the to string of the object.
                else if (value is string || valueType.IsPrimitive || value is Color)
                {
                    return this.CreateAndStorePropertyData(value, propertyName, propertyPath, valueType, null, value.ToString(), null, null);
                }
                // Check to see if it has a public empty ctor, if so, then lets build a property propertyTree for it, so we can recreate it.
                else if (valueType.GetConstructor(new Type[] { }) != null)
                {
                    string strValue = "";
                    Collection<PropertyDataInfo> props = this.BuildPropertyTree(propertyPath, value, false);
                    if (props == null || props.Count == 0)
                        strValue = value.ToString();

                    return this.CreateAndStorePropertyData(value, propertyName, propertyPath, valueType, null, strValue, null, props);
                }
                // If the object is a Type object, store off the Type information.
                else if (value is Type)
                {
                    return this.CreateAndStorePropertyData(value, propertyName, propertyPath, typeof(Type), null, ((Type)value).AssemblyQualifiedName, null, null);
                }
            }
            else if(pi != null)
            {
                return this.CreateAndStorePropertyData(null, propertyName, propertyPath, pi.PropertyType, null, null, null, null);
            }

			// We may have stored off the value. However, if we did and didn't use it, we need to dispose of it.
			if (value != null && this._objectStore.ContainsKey(value))
			{
				int key = this._objectStore[value];
				if(!this._propDataStore.ContainsKey(key))
					this._objectStore.Remove(value);
			}

			return null;
		}
		#endregion // SaveProperty

		#region CreateAndStorePropertyData

		private PropertyDataInfo CreateAndStorePropertyData(object value, string propertyName, string propertyPath, Type t, string id, string stringValue, string converterTypeName, Collection<PropertyDataInfo> props)
		{
			return this.CreateAndStorePropertyData(value, propertyName, propertyPath, t, id, false, stringValue, converterTypeName, props);
		}

		private PropertyDataInfo CreateAndStorePropertyData(object value, string propertyName, string propertyPath, Type t, string id, bool storeValue, string stringValue, string converterTypeName, Collection<PropertyDataInfo> props)
		{
            return this.CreateAndStorePropertyData(value, propertyName, propertyPath, t, id, storeValue, stringValue, converterTypeName, props, null);
		}

        private PropertyDataInfo CreateAndStorePropertyData(object value, string propertyName, string propertyPath, Type t, string id, bool storeValue, string stringValue, string converterTypeName, Collection<PropertyDataInfo> props, Collection<string> lookupKeys)
        {
            PropertyData data = new PropertyData()
            {
                AssemblyTypeKey = this.StoreType(t),
                Identifier = id,
                StringValue = stringValue,
                ConverterTypeName = converterTypeName,
                Properties = props,
                LookUpKeys = lookupKeys
            };

            if (storeValue)
            {
                data.Value = value;
                if (!this.ValueTypes.Contains(t))
                    this.ValueTypes.Add(t);
            }

            return new PropertyDataInfo() { PropertyName = propertyName, PropertyPath = propertyPath, DataKey = this.StoreProperty(value, data) };
        }

		#endregion // CreateAndStorePropertyData

		#region StoreProperty

		private int StoreProperty(object data, PropertyData propData)
		{
            if (data == null)
            {
                data = propData;
            }

			int key = this._objectStore.Count;
			if (this._objectStore.ContainsKey(data))
			{
				key = this._objectStore[data];
			}
			else
			{
				this._objectStore.Add(data, key);
			}

			if (propData != null)
			{
				if (!this._propDataStore.ContainsKey(key))
					this._propDataStore.Add(key, propData);
			}

			return key;
		}
		#endregion // StoreProperty

		#region StoreType

		internal int StoreType(Type t)
		{
			string val = t.AssemblyQualifiedName;
			int key = val.GetHashCode();
			if (!this._typeStore.ContainsKey(key))
				this._typeStore.Add(key, val);
			return key;
		}
		#endregion // StoreType

		#region UnloadPropertyTree
		private void UnloadPropertyTree(object owner, Collection<PropertyDataInfo> propertyTree, bool validateAgainstIgnore)
		{
            IProvidePropertyPersistenceSettings settings = owner as IProvidePropertyPersistenceSettings;

            IProvideCustomObjectPersistence ipcop = owner as IProvideCustomObjectPersistence;

			foreach (PropertyDataInfo dataInfo in propertyTree)
			{
                if (ipcop != null && dataInfo.PropertyPath == "PersistenceCustomObject")
                    ipcop.LoadObject(this.LoadProperty(owner, dataInfo, false));
                else
				    this.LoadProperty(owner, dataInfo, validateAgainstIgnore);
			}

            if (settings != null)
                settings.FinishedLoadingPersistence();
		
		}
		#endregion // UnloadPropertyTree

		#region SetProperty
		private void SetProperty(PropertyInfo pi, object owner, object propertyValue, Collection<PropertyDataInfo> props)
		{
			if (pi != null)
			{
				if (pi.GetSetMethod() != null)
				{
					// If the current value equals the new value, don't bother setting it. 
					object currentVal = pi.GetValue(owner, null);

                    if (currentVal != propertyValue)
					{
						if (currentVal != null)
						{
							if (!currentVal.Equals(propertyValue))
								pi.SetValue(owner, propertyValue, null);
						}
						else
							pi.SetValue(owner, propertyValue, null);
					}
                    else if ((propertyValue is Style))
                    {
                        pi.SetValue(owner, null, null);
                        pi.SetValue(owner, propertyValue, null);
                    }
				}
				else
				{
					object val = pi.GetValue(owner, null);
					if (val != null)
					{
						this.UnloadPropertyTree(val, props, false);
					}
				}
			}
		}
		#endregion // SetProperty

		#region ResolvePropertyData

		private PropertyData ResolvePropertyData(int key)
		{
            if (this._propDataStore.ContainsKey(key))
            {
                return this._propDataStore[key];
            }
            return null;
		}
		#endregion // ResolvePropertyData

		#region LoadProperty

        List<int> _allowableKeys = new List<int>();
		private object LoadProperty(object owner, PropertyDataInfo dataInfo, bool validateAgainstIgnore)
		{
            bool childrenShouldValidateAgainsIgnore = false;

			bool objectAlreadyLoaded = false;
			bool isNewValueAlreadyProvided = false;

			int key = dataInfo.DataKey;

			object obj = null;
			
			// Have we already loaded this object?
			if (this._reverseObjectStore.ContainsKey(key))
			{
				obj = this._reverseObjectStore[key];
				objectAlreadyLoaded = true;
			}

			PropertyInfo pi = null;

			if (owner != null)
				pi = owner.GetType().GetProperty(dataInfo.PropertyName);

			PropertyData data = this.ResolvePropertyData(dataInfo.DataKey);

            if (pi != null)
            {
                // Skip static properties
                if ((pi.CanRead && pi.GetGetMethod() != null && pi.GetGetMethod().IsStatic) || (pi.CanWrite && pi.GetSetMethod() != null && pi.GetSetMethod().IsStatic))
                {
                    return null;
                }

                if (data != null)
                {
                    // Ok, so there is no information saved.... at all, which means the value must be null. so update it and exit early.
                    if (data.Identifier == null && data.LookUpKeys != null && data.LookUpKeys.Count == 0 && data.Properties.Count == 0 && data.StringValue == null && data.Value == null)
                    {
                        Type enumerableType = pi.PropertyType.GetInterface("System.Collections.IEnumerable", true);

                        // Empty collections can fall into here, and we don't want to dismiss them. 
                        if (enumerableType == null)
                        {
                            this.SetProperty(pi, owner, null, data.Properties);
                            return null;
                        }
                    }
                }
                else
                {
                    return null;
                }
            }

            PersistenceOption option = this._settings.LoadPersistenceOptions;

            // Has anyone specified a setting that matches this property?
            PropertyPersistenceInfoBase ppi = this._settings.PropertySettings.GetPropertyPersistenceInfo(pi, dataInfo.PropertyPath);

            // If they specified a setting for this property, lets honor it. 
            if (ppi != null)
            {
                if (!string.IsNullOrEmpty(data.StringValue))
                {
                    obj = ppi.LoadObjectFromString(pi, dataInfo.PropertyPath, data.StringValue);
                }

                if (obj == null)
                {
                    if (ppi.TypeConverter != null && ppi.TypeConverter.CanConvertFrom(typeof(string)))
                    {
                        obj = ppi.TypeConverter.ConvertFromString(data.StringValue);

                        // A new value was provided, so mark it as such, so that it's not overriden. 
                        isNewValueAlreadyProvided = true;
                    }
                }
            }
            else
            {
                // If we're only saving specified properties, then we know we can just exit it out now, b/c we didn't find one.
                if (option == PersistenceOption.OnlySpecified && validateAgainstIgnore)
                {
                    if (!this._allowableKeys.Contains(dataInfo.DataKey))
                    {
                        List<int> allowableKeys = this.CheckForPropertiesThatMatch(data);

                        if (allowableKeys != null && allowableKeys.Count > 0)
                        {
                            this._allowableKeys.AddRange(allowableKeys);
                            this._allowableKeys.Add(dataInfo.DataKey);
                        }
                    }

                    childrenShouldValidateAgainsIgnore = true;

                    if (!this._allowableKeys.Contains(dataInfo.DataKey))
                        return null;
                }
                else
                {
                    // Has anyone specified a setting that matches this property that should be ignored?
                    ppi = this._settings.IgnorePropertySettings.GetPropertyPersistenceInfo(pi, dataInfo.PropertyPath);

                    // If we found a propertySetting, then we need to ignore it, so return null.
                    if (ppi != null)
                    {
                        return null;
                    }
                }
            }

            // Raise LoadProperty Event.
            LoadPropertyPersistenceEventArgs args = new LoadPropertyPersistenceEventArgs(this._rootObject, pi, dataInfo.PropertyPath, data.StringValue, data.Identifier, owner, data.Value);
            this._settings.Events.OnLoadPropertyPersistence(args);
            this._group.Events.OnLoadPropertyPersistence(args);

            if (args.Handled)
                return null;
            else if (args.LoadedValue != null)
            {
                obj = args.LoadedValue;

                // A new value was provided, so mark it as such, so that it's not overriden. 
                isNewValueAlreadyProvided = true;
            }

            if (obj == null)
            {
                bool invalidCast = false;

                if (data.Value != null)
                {
                    obj = data.Value;
                }
                // If an identifier is specified, lets try loading it.
                else if (!string.IsNullOrEmpty(data.Identifier))
                {
                    if (Application.Current != null)
                    {
                        obj = Application.Current.Resources[data.Identifier];
                    }
                } // If a a value is specified, lets try converting it.
                else if (!string.IsNullOrEmpty(data.StringValue))
                {
                    // If we have a converter, lets load the coverter and use it to convert the value.
                    if (!string.IsNullOrEmpty(data.ConverterTypeName))
                    {
                        Type t = Type.GetType(data.ConverterTypeName);
                        if (t == null)
                            t = PersistenceManager.FixAssemblyQualifiedName(data.ConverterTypeName);

                        TypeConverter converter = t.GetConstructor(new Type[] { }).Invoke(null) as TypeConverter;

                        if (converter.CanConvertFrom(typeof(string)))
                        {
                            obj = converter.ConvertFrom(null, CultureInfo.CurrentCulture, data.StringValue);

                            // A new value was provided, so mark it as such, so that it's not overriden. 
                            isNewValueAlreadyProvided = true;
                        }
                    }
                    else
                    {
                        // Last resort is to try and parse or cast the value into our data type.
                        try
                        {
                            Type t = this.ResolveType(data.AssemblyTypeKey);

                            // Color
                            if (t == typeof(Type))
                            {
                                obj = Type.GetType(data.StringValue);
                            }// Who knows, lets jutst try casting it to its requested type.
                            else
                            {
                                obj = System.Convert.ChangeType(data.StringValue, t, System.Globalization.CultureInfo.CurrentCulture);
                            }
                        }
                        catch (InvalidCastException)
                        {
                            invalidCast = true;
                        }
                    }
                }

                // If we were able to load the object successfully.
                if (!invalidCast && obj != null)
                {
                    // Store if off, just in case it hasn't already been stored.
                    if (!this._reverseObjectStore.ContainsKey(key))
                        this._reverseObjectStore.Add(key, obj);

                    IProvideCustomPersistence ipcp = obj as IProvideCustomPersistence;
                    if (ipcp != null)
                        ipcp.Load(owner, data.StringValue);
                    else
                        this.UnloadPropertyTree(obj, data.Properties, childrenShouldValidateAgainsIgnore);
                }
                else
                {
                    object currentValue = null;

                    // First, lets see if there is already an instance of this object that exists on the object. 
                    // If so, then don't bother creating a new one, we can just use that instance, which is better
                    // b/c that way we don't loose any properties that were already set on it. 
                    if (owner != null && !string.IsNullOrEmpty(dataInfo.PropertyName))
                    {
                        PropertyInfo propInfo = owner.GetType().GetProperty(dataInfo.PropertyName);
                        if (propInfo != null)
                            currentValue = propInfo.GetValue(owner, null);
                    }

                    bool needToRecreate = true;

                    IProvidePersistenceLookupKeys lookupKeyObj = currentValue as IProvidePersistenceLookupKeys;
                    if (lookupKeyObj != null && lookupKeyObj.CanRehydrate(data.LookUpKeys))
                    {
                        needToRecreate = false;
                        obj = currentValue;

                        IEnumerable objEnumerable = (IEnumerable)obj;
                        int currentIndex = 0; 
                        foreach (object enumItem in objEnumerable)
                        {
                            // The colleciton must have changed while looping through.
                            // So we can no longer rehydrate existing objects.
                            if (currentIndex >= data.Properties.Count)
                            {
                                needToRecreate = true;
                                break;
                            }

                            PropertyDataInfo pdi = data.Properties[currentIndex];

                            if (!this._reverseObjectStore.ContainsKey(pdi.DataKey))
                            {
                                PropertyData propData = this.ResolvePropertyData(pdi.DataKey);
                                this._reverseObjectStore.Add(pdi.DataKey, enumItem);

                                IProvideCustomPersistence ipcp = enumItem as IProvideCustomPersistence;
                                if (ipcp != null)
                                {
                                    ipcp.Load(owner, propData.StringValue);
                                }
                                else
                                    this.UnloadPropertyTree(enumItem, propData.Properties, childrenShouldValidateAgainsIgnore);
                            }

                            currentIndex++;
                        }
                        
                        if(!needToRecreate)
                            return obj;
                    }
                    
                    if(needToRecreate)
                    {
                        // Otherwise lets resolve the type and try to create a new instance of the object. 
                        Type objType = this.ResolveType(data.AssemblyTypeKey);

                        // Apparently certain types, if they already exist, don't like you modifying properties at runtime. 
                        // A brush is one of those properties, as a SolidColorBrush doesn't like you setting it's Color property
                        // if it's already used, it throws an UnauthorizedException message. 
                        if (currentValue != null && !(currentValue is Brush) && currentValue.GetType() == objType)
                        {
                            obj = currentValue;
                        }
                        else if(objType != typeof(object) && objType != null)
                        {
                            if (!PersistenceManager.TypesToIgnore.Contains(objType))
                            {
                                ConstructorInfo ctor = objType.GetConstructor(new Type[] { });
                                if (ctor != null)
                                {
                                    obj = ctor.Invoke(new object[] { });

                                    INeedInitializationForPersistence initObj = obj as INeedInitializationForPersistence;
                                    if (initObj != null)
                                        initObj.InitObject(this._rootObject);
                                }
                            }
                        }


                        // If we were able to resolve, lets load it up with its properties.
                        if (obj != null)
                        {
                            // Store if off, just in case it hasn't already been stored.
                            if (!this._reverseObjectStore.ContainsKey(key))
                                this._reverseObjectStore.Add(key, obj);

                            IProvideCustomPersistence ipcp = obj as IProvideCustomPersistence;
                            if (ipcp != null)
                                ipcp.Load(owner, data.StringValue);
                            else
                            {
                                this.UnloadPropertyTree(obj, data.Properties, childrenShouldValidateAgainsIgnore);
                            }
                        }
                    }
                }
            }

			// Now its time to take the object we've found and loaded, and try to set the property in which it belongs to.
			if (pi != null)
			{
				try
				{
					// If the property is a readonly IList, then it must be a collection property.
					if (pi.GetSetMethod() == null && pi.PropertyType.GetInterface("IList", true) != null)
					{
						// if we haven't already loaded it, lets load it now.
						if (!objectAlreadyLoaded)
						{
                            object listObj = pi.GetValue(owner, null);
                            IProvideCustomPersistence ipcp = listObj as IProvideCustomPersistence;
                             if (ipcp != null)
                                 ipcp.Load(owner, data.StringValue);
                             else
                             {
                                 IList list = listObj as IList;

                                 if (list != null && !list.IsReadOnly)
                                 {
                                     // Store if off, just in case it hasn't already been stored.
                                     if (!this._reverseObjectStore.ContainsKey(key))
                                         this._reverseObjectStore.Add(key, list);

                                     if (!isNewValueAlreadyProvided)
                                     {
                                         // Clear it
                                         if (!list.IsReadOnly)
                                         {
                                             list.Clear();

                                             // Populate it.
                                             foreach (PropertyDataInfo itemDataInfo in data.Properties)
                                             {
                                                 object item = this.LoadProperty(null, itemDataInfo, childrenShouldValidateAgainsIgnore);
                                                 list.Add(item);
                                             }
                                         }
                                     }
                                     else
                                     {
                                         IList loadedList = obj as IList;
                                         if (loadedList != null)
                                         {
                                             foreach (object item in loadedList)
                                             {
                                                 list.Add(item);
                                             }
                                         }
                                     }
                                 }
                             }
						}
					} // Otherwise, if we have a value, lets load it.
                    else if (obj != null)
                    {
                        // Store if off, just in case it hasn't already been stored.
                        if (!this._reverseObjectStore.ContainsKey(key))
                            this._reverseObjectStore.Add(key, obj);

                        IList list = obj as IList;

                        // Lets first check to see if its an Ilist that we can populate.
                        if (list != null && !objectAlreadyLoaded)
                        {
                            if (!isNewValueAlreadyProvided)
                            {
                                // Clear it
                                list.Clear();

                                // Populate it
                                foreach (PropertyDataInfo itemDataInfo in data.Properties)
                                {
                                    object item = this.LoadProperty(null, itemDataInfo, childrenShouldValidateAgainsIgnore);
                                    list.Add(item);
                                }
                            }

                            // Set the property.
                            this.SetProperty(pi, owner, list, data.Properties);
                        }
                        else
                        {
                            // Otherwise, just set the property.
                            this.SetProperty(pi, owner, obj, data.Properties);
                        }
                    }
                    else
                    {
                        this.SetProperty(pi, owner, obj, data.Properties);
                    }
				}
				catch (Exception ex)
				{
					// For some reason we weren't able to store this property. 
					// Lets log it, and provide this information in the Loaded event at the end.
					this._exceptions.Add(new PropertyPersistenceExceptionDetails(ex, pi, dataInfo.PropertyPath));
				}
			}


			return obj;
		}
		#endregion // LoadProperty

		#region ResolveType

		internal Type ResolveType(int key)
		{
			return Type.GetType(this._typeStore[key]);
		}

		#endregion // ResolveType

        #region CheckForPropertiesThatMatch
        private List<int> CheckForPropertiesThatMatch(PropertyData data)
        {
            List<int> allowableKeys = null;

            Type currentType = this.ResolveType(data.AssemblyTypeKey);
            if (data.Properties != null && data.Properties.Count > 0)
            {
                Type enumerableType = currentType.GetInterface("System.Collections.IEnumerable", true);
                if (enumerableType != null)
                {
                    foreach (PropertyDataInfo pdi in data.Properties)
                    {
                        PropertyPersistenceInfoBase ppi = this._settings.PropertySettings.GetPropertyPersistenceInfo(null, pdi.PropertyPath);

                        if (ppi != null)
                        {
                            if (allowableKeys == null)
                                allowableKeys = new List<int>();

                            allowableKeys.Add(pdi.DataKey);
                        }
                        else
                        {
                            PropertyData childData = this.ResolvePropertyData(pdi.DataKey);
                            if (childData != null)
                            {
                                List<int> test = this.CheckForPropertiesThatMatch(childData);

                                if (test != null)
                                {
                                    if (allowableKeys == null)
                                        allowableKeys = new List<int>();

                                    allowableKeys.AddRange(test);
                                    allowableKeys.Add(pdi.DataKey);
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (PropertyDataInfo pdi in data.Properties)
                    {
                        PropertyInfo pi = currentType.GetProperty(pdi.PropertyName);
                        if (pi != null)
                        {
                            PropertyPersistenceInfoBase ppi = this._settings.PropertySettings.GetPropertyPersistenceInfo(pi, pdi.PropertyPath);
                            if (ppi != null)
                            {
                                if (allowableKeys == null)
                                    allowableKeys = new List<int>();

                                allowableKeys.Add(pdi.DataKey);
                            }
                            else
                            {
                                PropertyData childData = this.ResolvePropertyData(pdi.DataKey);
                                if (childData != null)
                                {
                                    List<int> test = this.CheckForPropertiesThatMatch(childData);

                                    if (test != null)
                                    {
                                        if (allowableKeys == null)
                                            allowableKeys = new List<int>();

                                        allowableKeys.AddRange(test);
                                        allowableKeys.Add(pdi.DataKey);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return allowableKeys;
        }

        #endregion // CheckForPropertiesThatMatch

        #endregion // Private

        #endregion // Methods

        #region Properties

        /// <summary>
		/// Gets/sets a dictionary of all the root properties that belong to the object. 
		/// </summary>
		public Dictionary<int, PropertyData> PropertyDataStore
		{
			get { return this._propDataStore; }
			set { this._propDataStore = value; }
		}

		/// <summary>
		/// Gets alist of Value types that have been stored by their values. 
		/// </summary>
		public Collection<Type> ValueTypes
		{
			get 
			{
				return this._valueTypes;
			}
		}

		internal List<PropertyPersistenceExceptionDetails> Exceptions
		{
			get { return this._exceptions; }
		}

		#endregion // Properties

	}
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