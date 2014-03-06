using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Windows;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Collections;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace Infragistics.Windows.DataPresenter
{
	internal class CustomizationsManager
	{
		#region Nested Data Structures

		#region FieldDescription Class

		private class FieldDescription
		{
			#region Vars

			private CustomizationsManager _manager;
			internal string _name;
			internal string _typeName;
			internal bool _isUnbound;
			internal string _bindingPath;

			[ThreadStatic()]
			private static Parser_ListOfItems g_parser_ListOfItems;

			#endregion // Vars

			#region Constructors

			private FieldDescription( )
			{
			}

			internal FieldDescription( CustomizationsManager manager, string name, string typeName, bool isUnbound, string bindingPath )
			{
				GridUtilities.ValidateNotNull( manager );

				_manager = manager;
				_name = name;
				_typeName = typeName;
				_isUnbound = isUnbound;
				_bindingPath = bindingPath;
			}

			internal FieldDescription( CustomizationsManager manager, Field field )
			{
				_manager = manager;
				_name = field.Name;

				Type type = field.DataType;
				if ( null != type )
					type = Utilities.GetUnderlyingType( field.DataType );

				_typeName = type.Name;

				UnboundField ubField = field as UnboundField;
				_isUnbound = null != ubField;

                // JJD 2/4/09 - TFS12751
                // Check to make sure the binding path is not null before calling ToString()
				//if ( _isUnbound )
				// SSP 6/30/10 TFS23826
				// 
				
				
				if ( null != ubField )
				{
					PropertyPath propertyPath = ubField.BindingPath;
					if ( null != propertyPath )
						_bindingPath = GetBindingPathString(propertyPath);
					else
					{
						// JJD 7/07/10 - TFS26644,TFS35483 
						// Added support for the new Binding property on UnboundField
						// If binding is set call the GetBindingString helper to get
						// a string for matching purposes
						BindingBase binding = ubField.Binding;

						if (binding != null)
							_bindingPath = GetBindingString(binding);

					}
				}
			}   

			#endregion // Constructors

			#region GetBindingPathString

			// SSP 6/30/10 TFS23826
			// 
			private static string GetBindingPathString( PropertyPath bindingPath )
			{
				return (string)Utilities.ConvertDataValue( bindingPath, typeof( string ), CultureInfo.InvariantCulture, null );
			}

			#endregion // GetBindingPathString

			// JJD 7/07/10 - TFS26644,TFS35483 
			#region GetBindingString

			private static string GetBindingString(BindingBase bindingBase)
			{
				Binding binding = bindingBase as Binding;

				if (binding != null)
				{
					if (binding.Path != null)
						return  "{" + GetBindingPathString(binding.Path) + "}";
					else
						if (binding.XPath != null)
							return "{XPath=" + binding.XPath + "}";

					return null;
				}

				int childCount = 0;

				MultiBinding multi = bindingBase as MultiBinding;

				if (multi != null)
				{
					StringBuilder sb = new StringBuilder();
					sb.Append("{MultiBinding=");
					foreach (BindingBase child in multi.Bindings)
					{
						string childStr = GetBindingString(child);

						if (childStr != null)
						{
							childCount++;
							sb.Append(childStr);
						}
					}

					if (childCount == 0)
						return null;

					sb.Append("}");

					return sb.ToString();
				}

				PriorityBinding priority = bindingBase as PriorityBinding;

				if (priority != null)
				{
					StringBuilder sb = new StringBuilder();
					sb.Append("{PriorityBinding=");
					foreach (BindingBase child in priority.Bindings)
					{
						string childStr = GetBindingString(child);

						if (childStr != null)
						{
							childCount++;
							sb.Append(childStr);
						}
					}

					if (childCount == 0)
						return null;

					sb.Append("}");

					return sb.ToString();
				}

				return null;
			}

			#endregion //GetBindingString	
    
			#region Parser

			private static Parser_ListOfItems Parser
			{
				get
				{
					if ( null == g_parser_ListOfItems )
						g_parser_ListOfItems = new Parser_ListOfItems( '"', '"', '\\', ';' );

					return g_parser_ListOfItems;
				}
			}

			#endregion // Parser

			#region ToString

			public override string ToString( )
			{
				StringBuilder sb = new StringBuilder( );
				this.ToString( sb );

				return sb.ToString( );
			}

			public void ToString( StringBuilder sb )
			{
				List<string> items = new List<string>( );

				items.Add( _name );

				if ( _isUnbound || "String" != _typeName )
					items.Add( _typeName );

				if ( _isUnbound )
				{
					items.Add( "unbound" );

                    // JJD 2/4/09 - TFS12751
                    // Check to make sure the binding path is not null before adding it to the array
                    if ( _bindingPath != null )
                        items.Add(_bindingPath);
				}

				sb.Append( Parser.ConstructList( items.ToArray( ), false ) );
			}

			#endregion // ToString

			#region Parse

			public static FieldDescription Parse( CustomizationsManager manager, string val )
			{
				FieldDescription ret = new FieldDescription( );
				ret.ParseHelper( manager, val );

				return ret;
			}

			private void ParseHelper( CustomizationsManager manager, string val )
			{
				_manager = manager;

				string[] parts = Parser.ParseList( val, false );
				if ( null != parts && parts.Length >= 1 )
				{
					_name = parts[0];

					if ( parts.Length >= 2 )
						_typeName = parts[1];
					else
						// We don't serialize type name if it's string.
						_typeName = typeof( string ).Name;

					if ( parts.Length >= 3 )
						_isUnbound = "unbound" == parts[2];

					if ( parts.Length >= 4 )
						_bindingPath = parts[3];
				}
			}

			#endregion // Parse
		}

		#endregion // FieldDescription Class

		#region FieldLayoutDescription Class

		private class FieldLayoutDescription
		{
			#region Nested Structurs

			// SSP 8/25/11 TFS83385
			// 
			[Flags]
			internal enum FieldMatchCriteria
			{
				Name = 0x1,
				TypeName = 0x2,
				BindingPath = 0x4,
				IsUnbound = 0x8,
				All = 0xF
			}

			#region FieldDescriptionEqualityComparer Class

			internal class FieldDescriptionEqualityComparer : IEqualityComparer
			{
				#region Vars

				private CustomizationsManager _manager;

				// SSP 6/30/10 TFS23826
				// 
				private FieldLayoutDescription _fieldLayoutDescription;
				private double _formatVersion;

				// SSP 8/25/11 TFS83385
				// 
				private FieldMatchCriteria _criteria;

				#endregion // Vars

				#region Constructors

				internal FieldDescriptionEqualityComparer(
					// SSP 6/30/10 TFS23826
					// Added fieldLayoutDescription parameter.
					// 
					FieldLayoutDescription fieldLayoutDescription, 
					CustomizationsManager manager,
					// SSP 8/25/11 TFS83385
					// 
					FieldMatchCriteria criteria
					)
				{
					GridUtilities.ValidateNotNull( manager );
					_manager = manager;

					// SSP 6/30/10 TFS23826
					// 
					_fieldLayoutDescription = fieldLayoutDescription;

					// SSP 8/25/11 TFS83385
					// 
					_criteria = criteria;

					// If we have a field layout description then get it's format version otherwise
					// get the customization manager's format version. This is used by a field layout
					// node that we retain and save out if the associated field layout never gets
					// loaded. If the previous loaded customizations file was in the old format then
					// the node would have that older format and the reset of the saved file would
					// have the new format.
					// 
					_formatVersion = null != _fieldLayoutDescription
						? _fieldLayoutDescription._nodeFormatVersion
						: _manager._formatVersion;
				}

				#endregion // Constructors

				#region Equals

				public bool Equals( FieldDescription x, FieldDescription y )
				{
					if ( null == x || null == y )
						return x == y;

					// SSP 6/30/10 TFS23826
					// 
					//if ( _manager._formatVersion >= 1.1 )
					if ( _formatVersion >= 1.1 )
					{
						// SSP 8/25/11 TFS83385
						// 
						// ----------------------------------------------------------------------------------------------
						return ( 0 == ( FieldMatchCriteria.Name & _criteria ) || x._name == y._name )
							&& ( 0 == ( FieldMatchCriteria.TypeName & _criteria ) || x._typeName == y._typeName )
							// SSP 6/30/10 TFS23826
							// Before 1.6, we were always doing PropertyPath.ToString which was resulting
							// in the type name, essentially the same string. Therefore there's no point
							// in comparing binding path values for older formats.
							// 
							// SSP 1/16/12 TFS98572
							// 
							//&& ( _formatVersion < 1.6 || ( 0 == ( FieldMatchCriteria.BindingPath & _criteria ) || x._bindingPath == y._bindingPath ) )
							&& ( _formatVersion < 1.6 || ( 0 == ( FieldMatchCriteria.BindingPath & _criteria ) || GridUtilities.AreEqual( x._bindingPath, y._bindingPath, true ) ) )
							&& ( 0 == ( FieldMatchCriteria.IsUnbound & _criteria ) || x._isUnbound == y._isUnbound );
						//return x._name == y._name
						//    && x._typeName == y._typeName
						//    // SSP 6/30/10 TFS23826
						//    // Before 1.6, we were always doing PropertyPath.ToString which was resulting
						//    // in the type name, essentially the same string. Therefore there's no point
						//    // in comparing binding path values for older formats.
						//    // 
						//    && ( _formatVersion < 1.6 || x._bindingPath == y._bindingPath )
						//    && x._isUnbound == y._isUnbound;
						// ----------------------------------------------------------------------------------------------
					}
					else
					{
						return x._name == y._name;
					}
				}

				#endregion // Equals

				#region GetHashCode

				public int GetHashCode( FieldDescription obj )
				{
					// SSP 6/30/10 TFS23826
					// 
					//if ( _manager._formatVersion >= 1.1 )
					if ( _formatVersion >= 1.1 )
					{
						// SSP 8/25/11 TFS83385
						// 
						// ----------------------------------------------------------------------------------------------
						return ( 0 == ( FieldMatchCriteria.Name & _criteria ) ? 0 : GetStringHashHelper( obj._name ) )
							^ ( 0 == ( FieldMatchCriteria.TypeName & _criteria ) ? 0 : GetStringHashHelper( obj._typeName ) )
							// SSP 6/30/10 TFS23826
							// Before 1.6, we were always doing PropertyPath.ToString which was resulting
							// in the type name, essentially the same string. Therefore there's no point
							// in comparing binding path values for older formats.
							// 
							//^ GetStringHashHelper( obj._bindingPath )
							^ ( 0 == ( FieldMatchCriteria.BindingPath & _criteria ) ? 0 : ( _formatVersion >= 1.6 ? GetStringHashHelper( obj._bindingPath ) : 0 ) )
							^ ( 0 == ( FieldMatchCriteria.IsUnbound & _criteria ) ? 0 : obj._isUnbound.GetHashCode( ) );
						//return GetStringHashHelper( obj._name )
						//    ^ GetStringHashHelper( obj._typeName )
						//    // SSP 6/30/10 TFS23826
						//    // Before 1.6, we were always doing PropertyPath.ToString which was resulting
						//    // in the type name, essentially the same string. Therefore there's no point
						//    // in comparing binding path values for older formats.
						//    // 
						//    //^ GetStringHashHelper( obj._bindingPath )
						//    ^ ( _formatVersion >= 1.6 ? GetStringHashHelper( obj._bindingPath ) : 0 )
						//    ^ obj._isUnbound.GetHashCode( );
						// ----------------------------------------------------------------------------------------------
					}
					else
					{
						return GetStringHashHelper( obj._name );
					}
				}

				#endregion // GetHashCode

				#region GetStringHashHelper

				private static int GetStringHashHelper( string s )
				{
					return null != s ? s.GetHashCode( ) : 0;
				}

				#endregion // GetStringHashHelper

				#region IEqualityComparer Members

				bool IEqualityComparer.Equals( object x, object y )
				{
					return this.Equals( (FieldDescription)x, (FieldDescription)y );
				}

				int IEqualityComparer.GetHashCode( object obj )
				{
					return this.GetHashCode( (FieldDescription)obj );
				}

				#endregion
			}

			#endregion // FieldDescriptionEqualityComparer Class

			#endregion // Nested Structurs

			#region Vars

			private CustomizationsManager _manager;
			private string _key;

			// SSP 8/25/11 TFS83385
			// 
			//private HashSet _fieldList;
			private FieldDescription[] _fieldList;

			// SSP 11/13/09 TFS23002
			// 
			internal double _nodeFormatVersion;

			private FieldDescriptionEqualityComparer _fieldDescriptionComparer;
			private Dictionary<Field, FieldDescription> _cachedFsds;

			// SSP 8/25/11 TFS83385
			// 
			internal readonly FieldDescriptionEqualityComparer[] _criteriaComparers;
			private static readonly FieldMatchCriteria[] g_criterias = new FieldMatchCriteria[]
			{
				FieldMatchCriteria.All,
				FieldMatchCriteria.All ^ FieldMatchCriteria.TypeName
			};

			#endregion // Vars

			#region Constructors

			internal FieldLayoutDescription( CustomizationsManager manager, string key, FieldDescription[] fieldList,
					// SSP 11/13/09 TFS23002
					// 
					double nodeFormatVersion
				)
			{
				GridUtilities.ValidateNotNull( manager );

				// SSP 11/13/09 TFS23002
				// 
				_nodeFormatVersion = nodeFormatVersion;
				
				_manager = manager;
				_key = key;

				_fieldDescriptionComparer = new FieldDescriptionEqualityComparer( this, manager, FieldMatchCriteria.All );
				
				// SSP 4/28/09 TFS17155
				// Check for fieldList being null, which can happen when loading older customizations file.
				// 
				// SSP 8/25/11 TFS83385
				// Changed _fieldList from HashSet to array.
				// 
				// --------------------------------------------------------------------------------
				_fieldList = fieldList;

				_criteriaComparers = new FieldDescriptionEqualityComparer[g_criterias.Length];
				for ( int i = 0; i < g_criterias.Length; i++ )
					_criteriaComparers[i] = new FieldDescriptionEqualityComparer( this, _manager, g_criterias[i] );

				
#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

				// --------------------------------------------------------------------------------
			}

			#endregion // Constructors

			#region DoesFieldMatch

			// SSP 6/30/10 TFS23826
			// 
			// SSP 8/25/11 TFS83385
			// Added comparer parameter.
			// 
			//public bool DoesFieldMatch( Field testField, FieldDescription testFsd )
			public bool DoesFieldMatch( Field testField, FieldDescription testFsd, FieldDescriptionEqualityComparer comparer )
			{
				if ( null == _cachedFsds )
					_cachedFsds = new Dictionary<Field, FieldDescription>( );

				FieldDescription fsd;
				if ( !_cachedFsds.TryGetValue( testField, out fsd ) )
					_cachedFsds[testField] = fsd = new FieldDescription( _manager, testField );

				// SSP 8/25/11 TFS83385
				// Added comparer parameter.
				// 
				//return _fieldDescriptionComparer.Equals( fsd, testFsd );
				return comparer.Equals( fsd, testFsd );
			}

			#endregion // DoesFieldMatch

			#region GetHashCode

			public override int GetHashCode( )
			{
				return null != _key ? _key.GetHashCode( ) : 0;
			}

			#endregion // GetHashCode

			// SSP 8/25/11 TFS83385
			// 
			private static Dictionary<string, FieldDescription> ToNameMap( IList<FieldDescription> x )
			{
				Dictionary<string, FieldDescription> map = null;

				int count = x.Count;
				if ( count > 0 )
				{
					map = new Dictionary<string, FieldDescription>( );
					for ( int i = 0; i < count; i++ )
					{
						FieldDescription ii = x[i];
						string name = ii._name;
						if ( string.IsNullOrEmpty( name ) )
						{
							map = null;
							break;
						}

						map[name] = ii;
					}
				}

				return null != map && map.Count == count ? map : null;
			}

			// SSP 8/25/11 TFS83385
			// 
			private static bool AreEqual( IList<FieldDescription> x, IList<FieldDescription> y, FieldDescriptionEqualityComparer comparer )
			{
				bool isSubset, areEqual;
				IsSubsetOfHelper( x, y, comparer, out isSubset, out areEqual );

				return areEqual;
			}

			// SSP 8/25/11 TFS83385
			// 
			private static bool IsSubsetOf( IList<FieldDescription> x, IList<FieldDescription> y, FieldDescriptionEqualityComparer comparer )
			{
				bool isSubset, areEqual;
				IsSubsetOfHelper( x, y, comparer, out isSubset, out areEqual );

				return isSubset;
			}

			// SSP 8/25/11 TFS83385
			// 
			private static void IsSubsetOfHelper( IList<FieldDescription> x, IList<FieldDescription> y, FieldDescriptionEqualityComparer comparer,
				out bool isSubset, out bool areEqual )
			{
				isSubset = areEqual = false;

				int xCount = null != x ? x.Count : 0;
				int yCount = null != y ? y.Count : 0;

				if ( xCount > yCount )
					return;

				// Compare using name lookups if the names are unique and non-empty strings.
				// 
				Dictionary<string, FieldDescription> xxNames = ToNameMap( x );
				Dictionary<string, FieldDescription> yyNames = ToNameMap( y );
				if ( null != xxNames && null != yyNames )
				{
					for ( int i = 0; i < xCount; i++ )
					{
						FieldDescription xx = x[i];

						FieldDescription yy;
						yyNames.TryGetValue( xx._name, out yy );
						if ( null == yy || !comparer.Equals( xx, yy ) )
							return;
					}
				}
				else
				{
					// Otherwise do a linear search.
					// 
					for ( int i = 0; i < xCount; i++ )
					{
						FieldDescription xx = x[i];
						if ( IndexOf( y, x[i], comparer ) < 0 )
							return;
					}
				}

				isSubset = true;
				areEqual = xCount == yCount;
			}

			// SSP 8/25/11 TFS83385
			// 
			private static int IndexOf( IList<FieldDescription> list, FieldDescription item, FieldDescriptionEqualityComparer comparer )
			{
				for ( int i = 0, count = list.Count; i < count; i++ )
				{
					if ( comparer.Equals( list[i], item ) )
						return i;
				}

				return -1;
			}

			#region Equals

			public override bool Equals( object obj )
			{
				FieldLayoutDescription x = obj as FieldLayoutDescription;
				if ( null != x )
				{
					if ( _manager._formatVersion >= 1.1 )
					{
						return _key == x._key
							// SSP 4/28/09 TFS17155
							// Check for fieldList being null, which can happen when loading older customizations file.
							// 
							//&& HashSet.AreEqual( _fieldList, x._fieldList );
							// SSP 8/25/11 TFS83385
							// 
							//&& null != _fieldList && null != x._fieldList && HashSet.AreEqual( _fieldList, x._fieldList );
							&& AreEqual( _fieldList, x._fieldList, _fieldDescriptionComparer );
					}
					else
					{
						return _key == x._key;
					}
				}

				return false;
			}

			#endregion // Equals

			#region DoesFieldLayoutMatch

            // JJD 5/15/09 - NA 2009 vol 2 - Cross band grouping
            // Added ignoreKeyCompare parameter so we can do a 2nd pass on the fieldlayouts
            // to compare only the fields collections. This is necessary because we are now
            // initializing the FieldLayout's Key property differently than we used to.
            // E.g. with DataViews the key used to be the DataView. Now its a string
            // with the table name.
			//public bool DoesFieldLayoutMatch( FieldLayout fl )
			public bool DoesFieldLayoutMatch( FieldLayout fl, bool ignoreKeyCompare )
			{
                // JJD 5/15/09 - NA 2009 vol 2 - Cross band grouping
                // Check ignoreKeyCompare parameter - see note above
				//if ( _key == _manager.GetFieldLayoutKey( fl ) )
				// SSP 11/13/09 TFS23002
				// 
                //if (ignoreKeyCompare == true || _key == _manager.GetFieldLayoutKey(fl))
				if ( ignoreKeyCompare == true || _key == _manager.GetFieldLayoutKey( fl, _nodeFormatVersion ) )
				{
					// SSP 11/13/09 TFS23002
					// 
					//if ( _manager._formatVersion >= 1.1 )
					if ( _nodeFormatVersion >= 1.1 )
					{
						FieldCollection fields = fl.Fields;

						// SSP 8/25/11 TFS83385
						// 
						//HashSet fieldDescriptions = new HashSet( fields.Count, 0.6f, new FieldDescriptionEqualityComparer( this, _manager ) );
						List<FieldDescription> fieldDescriptions = new List<FieldDescription>( );

						foreach ( Field field in fields )
							fieldDescriptions.Add( new FieldDescription( _manager, field ) );

						// When a saved layout is loaded, we are matching by the field layout key and
						// the fields of the field layout (because field layouts could have key collisions).
						// For field matching, match if all the fields in the saved field layout are in
						// existence in the field layout instance. However also allow for addition of a field
						// to the field layout.
						// 
						// SSP 4/28/09 TFS17155
						// Check for fieldList being null, which can happen when loading older customizations file.
						// 
						//return _fieldList.IsSubsetOf( fieldDescriptions );
						// SSP 8/25/11 TFS83385
						// 
						// ------------------------------------------------------------------------------------
						//return null != _fieldList && _fieldList.IsSubsetOf( fieldDescriptions );
						FieldMatchCriteria[] criterias = new FieldMatchCriteria[]
						{
							FieldMatchCriteria.All,
							FieldMatchCriteria.All ^ FieldMatchCriteria.TypeName
						};

						foreach ( FieldDescriptionEqualityComparer comparer in _criteriaComparers )
						{
							if ( null != _fieldList && IsSubsetOf( _fieldList, fieldDescriptions, comparer ) )
								return true;
						}

						return false;
						// ------------------------------------------------------------------------------------
					}
					else
						// For older versions, all we used for comparison was field layout key.
						return true;
				}

				return false;
			}

			#endregion // DoesFieldLayoutMatch
		}

		#endregion // FieldLayoutDescription Class

		#endregion // Nested Data Structures

		#region Member Variables

		private DataPresenterBase									_dataPresenter;
		private XmlDocument											_currentLoadedDocument;
		
		
		
		
		private Dictionary<FieldLayoutDescription, FieldLayoutCustomizationInfo> _fieldLayoutCustomizationInfos;

		private bool												_allLoadedCustomizationsApplied = false;

		
		
		
		// SSP 2/3/09 TFS11860 TFS13101
		// Went from 1.1 to 1.2 to save field visibility.
		// 
        // JJD 5/15/09 - NA 2009 vol 2 - Cross band grouping
		// Went from 1.2 to 1.3 to support ParentFieldLayoutKey and ParentFieldName properties.
        //
		//private const double LATEST_VERSION = 1.1;
		//private const double LATEST_VERSION = 1.2;
		// SSP 9/3/09 TFS18172
		// Added support for saving and loading summaries.
		// 
		//private const double LATEST_VERSION = 1.3;
		// SSP 11/13/09 TFS23002
		// Added support for saving customization info for unloaded field-layouts.
		// 
		//private const double LATEST_VERSION = 1.4;
		// SSP 6/30/10 TFS23826
		// 
		//private const double LATEST_VERSION = 1.5;
		private const double LATEST_VERSION = 1.6;
		private double _formatVersion = LATEST_VERSION;

		#endregion //Member Variables

		#region Constructor

		internal CustomizationsManager(DataPresenterBase dataPresenter)
		{
			Debug.Assert(dataPresenter != null, "DataPresenter null in CustomizationsManager constructor!");

			this._dataPresenter = dataPresenter;
		}
 
		#endregion //Constructor

		#region Constants

		// Tags
		private const string RootTag			= "xamDataPresenter";
		private const string FieldLayoutsTag	= "fieldLayouts";
		private const string FieldLayoutTag		= "fieldLayout";
		private const string SortedFieldsTag	= "sortedFields";
		private const string SortedFieldTag		= "sortedField";
		private const string FieldsTag			= "fields";
		private const string FieldTag			= "field";

        // JJD 5/7/09 - NA 2009 vol 2 - Cross band grouping - added
        private const string ParentFieldLayoutTag
                                                = "parentFieldLayout";

		// JM NA 10.1 CardView
		private const string CardViewSettingsTag= "CardViewSettings";

		// Attribs
		private const string VersionAttrib		= "version";
		private const string KeyAttrib			= "key";
		private const string DirectionAttrib	= "direction";
		private const string FieldNameAttrib	= "fieldName";
		private const string IsGroupByAttrib	= "isGroupBy";
		private const string ExtentAttrib		= "extent";
		private const string NameAttrib			= "name";
		private const string LabelWidthAttrib	= "labelWidth";
		private const string LabelHeightAttrib	= "labelHeight";
		private const string CellWidthAttrib	= "cellWidth";
		private const string CellHeightAttrib	= "cellHeight";
		private const string RowAttrib			= "row";
		private const string ColumnAttrib		= "column";
		private const string RowSpanAttrib		= "rowSpan";
		private const string ColumnSpanAttrib	= "columnSpan";
        
        // JJD 5/7/09 - NA 2009 vol 2 - Cross band grouping - added
        private const string ParentFieldNameAttrib = "parentFieldName";

		// SSP 2/3/09 TFS11860 TFS13101
		// 
		private const string IsCollapsedInLayoutAttrib = "isCollapsedInLayout";
		private const string VisibilityAttrib = "Visibility";

		
		
		
		
		private const string FormatVersionAttrib = "formatVersion";
		private const string FieldListAttrib = "fieldList";
		// SSP 6/30/10 TFS23826
		// 
		private const string FieldExtendedInfo = "extendedInfo";

		// SSP 1/14/09 - NAS9.1 Record Filtering
		// 
		private const string RecordFiltersTag = "recordFilters";
		// SSP 1/19/09 - Fixed Fields
		// 
		private const string FixedLocationAttrib = "FixedLocation";

		// SSP 6/23/09 - NAS9.2 Field Chooser
		// This is used by the field chooser to force a field to be visible even if it's
		// a group-by field and CellVisibilityWhenGrouped is set to a value other than Visible.
		// 
		private const string IgnoreFieldVisibilityOverrides = "IgnoreFieldVisibilityOverrides";

		// AS 7/30/09 NA 2009.2 Field Sizing
		private const string LabelWidthAutoAttrib = "labelWidthAuto";
		private const string LabelHeightAutoAttrib = "labelHeightAuto";
		private const string CellWidthAutoAttrib = "cellWidthAuto";
		private const string CellHeightAutoAttrib = "cellHeightAuto";

		// SSP 9/3/09 TFS18172
		// Added support for saving and loading summaries.
		// 
		private const string SummariesTag = "summaries";

		// SSP 11/13/09 TFS23002
		// 
		private const string ExcludeCustomizations = "ExcludeCustomizationsAttrib";

		// JM NA 10.1 CardView
		private const string CardWidthAttrib	= "CardWidth";
		private const string CardHeightAttrib	= "CardHeight";

		#endregion //Constants

		#region Properties

			#region AreCustomizationsLoaded

		internal bool AreCustomizationsLoaded
		{
			get { return this._currentLoadedDocument != null; }
		}

			#endregion //AreCustomizationsLoaded

			#region FieldLayoutCustomizationInfos

		
		
		
		
		private Dictionary<FieldLayoutDescription, FieldLayoutCustomizationInfo> FieldLayoutCustomizationInfos
		{
			get 
			{
				if (this._fieldLayoutCustomizationInfos == null)
					
					
					
					
					this._fieldLayoutCustomizationInfos = new Dictionary<FieldLayoutDescription, FieldLayoutCustomizationInfo>( 3 );

				return this._fieldLayoutCustomizationInfos;
			}
		}

			#endregion //FieldLayoutCustomizationInfos

		#endregion //Properties

		#region Methods

			#region Internal Methods

				#region ApplyCustomizations



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		internal bool ApplyCustomizations()
		{
			// There is nothing to do if customizations have not yet been loaded.
			if (this.AreCustomizationsLoaded == false)
				return false;


			// If we have already applied all loaded customizations, don't bother processing the list - just return true.
			if (this._allLoadedCustomizationsApplied == true)
				return true;


			// Don't allow this routine to be called recursively.
			if (this._dataPresenter.IsApplyingCustomizations)
				throw new InvalidOperationException(DataPresenterBase.GetString("LE_LoadCustomizationInProgress"));


			// Set 'in progress' flag.
			this._dataPresenter.IsApplyingCustomizations = true;


			// Optimization: Keep track of the number of customizations that cannot be applied because the FieldLayout does
			// not exist or a customized Field doesnot exist.
			int totalCustomizationsThatCouldNotBeApplied = 0;

			try
			{
				#region Prepare

				XmlNode rootNode = this._currentLoadedDocument.SelectSingleNode(RootTag);
				if (rootNode == null)
                    throw new InvalidOperationException(DataPresenterBase.GetString("LE_InvalidCustomizationRootElement"));

				Version version = new Version(rootNode.Attributes[VersionAttrib].Value);

				
				
				
				
				
				
				
				
				_formatVersion = GetFormatVersion( rootNode, 1.0 );
				




				

				// Give the DataPresenter a chance to do something before the customizations are applied.
				this._dataPresenter.OnBeforeApplyCustomizations();

				#endregion //Prepare

				#region FieldLayouts

				XmlNode fieldLayoutsNode = rootNode.SelectSingleNode(FieldLayoutsTag);

                // JJD 5/07/09 - NA 2009 vol 2 - Cross band grouping
                // Allocate a dictionary on the stack to keep track of ParentFieldLayout whose 
                // processing needs to be delayed until all FieldLayouts have been processed
                Dictionary<FieldLayout, XmlNode> delayedParentFieldLayoutNodes = new Dictionary<FieldLayout, XmlNode>();

				// Process each FieldLayout node in the customizations XML.
                foreach (XmlNode fieldLayoutNode in fieldLayoutsNode.SelectNodes(FieldLayoutTag))
                {
                    // Find a FieldLayout in the DataPresenter's FieldLayouts collection that matches
                    // the FieldLayout described in the current XML node, and which has been initialized.
                    
                    
                    
                    
                    
                    string fieldListStr = GridUtilities.GetAttribute(fieldLayoutNode, FieldListAttrib);
					// SSP 11/13/09 TFS23002
					// Added support for saving customization info for unloaded field-layouts.
					// 
					double nodeFormatVersion = GetFormatVersion( fieldLayoutNode, _formatVersion );

                    FieldDescription[] fieldDescriptions = null != fieldListStr ? this.ParseFieldList(fieldListStr) : null;
                    FieldLayoutDescription fieldLayoutNodeKey = new FieldLayoutDescription(this,
                        GridUtilities.GetAttribute(fieldLayoutNode, KeyAttrib),
                        fieldDescriptions,
						// SSP 11/13/09 TFS23002
						// 
						nodeFormatVersion
					);

					// SSP 11/13/09 TFS23002
					// Added support for saving customization info for unloaded field-layouts.
					// 
					FieldLayoutCustomizationInfo info = this.GetFieldLayoutCustomizationInfo( fieldLayoutNodeKey, true );
					if ( !info._customizationsApplied && null == info._fieldLayoutNode )
					{
						info._fieldLayoutNode = fieldLayoutNode;
						info._nodeFormatVersion = nodeFormatVersion;
						string excludeFlagsStr = GridUtilities.GetAttribute( fieldLayoutNode, ExcludeCustomizations );
						CustomizationType exlcudeFlags = GridUtilities.ParseFlaggedEnumFromString<CustomizationType>( excludeFlagsStr, ", " );
						info._excludeCustomizations = exlcudeFlags;
					}
                    

                    FieldLayout matchingFieldLayout = GetLoadedFieldLayoutThatMatchesKey(fieldLayoutNodeKey);
                    if (matchingFieldLayout != null && matchingFieldLayout.IsInitialized == true)
                    {

                        // JJD 5/07/09 - NA 2009 vol 2 - Cross band grouping
                        #region Process ParentFieldLayoutKey

                        // we only want to de-serailze the ParentFieldLyoutKey if it isn't
                        // currently set.
                        if (matchingFieldLayout.ParentFieldLayoutKey == null)
                        {
                            // get the ParentFieldLayout node
                            XmlNode parentFieldLayoutNode = fieldLayoutNode.SelectSingleNode(ParentFieldLayoutTag);

                            if (parentFieldLayoutNode != null)
                            {
                                string fieldList = ReadAttribute(parentFieldLayoutNode, FieldListAttrib, null);

                                // if the field list is null then the key was originally a string
                                // so it is safe to de-serialize and set it now
                                if (fieldList == null)
                                {
                                    string key = ReadAttribute(parentFieldLayoutNode, KeyAttrib, null);

                                    Debug.Assert(key != null && key.Length > 0, "We should have serialized the ParentFieldLayoutKey");

                                    if (key != null && key.Length > 0)
                                        matchingFieldLayout.ParentFieldLayoutKey = key;
                                }
                                else
                                {
                                    if (delayedParentFieldLayoutNodes.ContainsKey(matchingFieldLayout))
                                    {
                                        Debug.Fail("The fieldlayout should only be found once in ApplyCustomizations");
                                    }
                                    else
                                    {
                                        delayedParentFieldLayoutNodes.Add(matchingFieldLayout, parentFieldLayoutNode);
                                    }
                                }
                            }
                        }

                        #endregion //Process ParentFieldLayoutKey

                        
                        
                        
                        
                        
                        
                        if (!this.HasCustomizationsBeenAppliedToFieldLayout(fieldLayoutNodeKey))
                        {
                            // JJD 5/07/09 - NA 2009 vol 2 - Cross band grouping
                            #region Process ParentFieldName attribute

                            // de-serialize the parent field name
                            string parentFieldName = ReadAttribute(fieldLayoutNode, ParentFieldNameAttrib, null);
                            if (parentFieldName != null && parentFieldName.Length > 0)
                                matchingFieldLayout.ParentFieldName = parentFieldName;

                            #endregion //Process ParentFieldName attribute	
    
                            #region Process Sorted Fields

                            // Clear the existing SortedFields (if any)
							// SSP 8/25/10 TFS30982 - Performance
							// Instead of always clearing and adding deserialized sorted fields, only do so
							// if they are actually different than the current sorted fields to prevent 
							// unnecessary resorting of records.
							// 
                            //matchingFieldLayout.SortedFields.Clear();
							List<FieldSortDescription> sortedFields = new List<FieldSortDescription>( );


                            // Process each sorted field customization and add a FieldSortDecsription for each
                            // one to the FieldLayout.SortedFields collection.
                            XmlNode sortedFieldsNode = fieldLayoutNode.SelectSingleNode(SortedFieldsTag);
							// SSP 11/13/09 TFS23002
							// Added support for saving customization info for unloaded field-layouts.
							// 
							//if (sortedFieldsNode != null)
							if ( sortedFieldsNode != null && !info.ExcludeCustomization( CustomizationType.GroupingAndSorting ) )
                            {
                                foreach (XmlNode sortedFieldNode in sortedFieldsNode.SelectNodes(SortedFieldTag))
                                {
									// SSP 6/30/10 TFS23826
									// Since we allow for multiple unbound fields to have empty names as well as same names
									// we need to store extended information that will let us better identify such fields
									// in case of name conficts.
									// 
									//string fieldName = ReadAttribute(sortedFieldNode, FieldNameAttrib, matchingFieldLayout.Fields[0].Name);
									//if (DoesFieldExistInFieldLayout(matchingFieldLayout, fieldName) == true)
									string fieldName = ReadAttribute( sortedFieldNode, FieldNameAttrib, null );
									string fieldExtendedInfo = ReadAttribute( sortedFieldNode, FieldExtendedInfo, null );
									Field matchingField = this.GetMatchingField( matchingFieldLayout, fieldLayoutNodeKey, fieldName, fieldExtendedInfo );
									if ( null != matchingField )
                                    {
                                        
                                        
                                        
                                        
                                        
                                        
                                        
                                        
                                        FieldSortDescription fieldSortDescription = new FieldSortDescription();
                                        fieldSortDescription.Direction = ReadEnumAttribute(sortedFieldNode, DirectionAttrib, ListSortDirection.Ascending);
                                        fieldSortDescription.FieldName = fieldName;
                                        fieldSortDescription.IsGroupBy = ReadAttribute(sortedFieldNode, IsGroupByAttrib, false);

										// SSP 6/30/10 TFS23826
										// Since multiple fields can have the same name, we need to set the Field if we have one.
										// 
										if ( null != matchingField )
											fieldSortDescription.Field = matchingField;

										// SSP 8/25/10 TFS30982 - Performance
										// 
                                        //matchingFieldLayout.SortedFields.Add(fieldSortDescription);
										sortedFields.Add( fieldSortDescription );

                                        
                                        
                                    }
                                    else
                                        totalCustomizationsThatCouldNotBeApplied++;
                                }
                            }

							// SSP 8/25/10 TFS30982 - Performance
							// 
							FieldSortDescriptionCollection flSortedFields = matchingFieldLayout.SortedFields;
							if ( ! GridUtilities.IsSameSortCriteria( sortedFields, flSortedFields ) )
							{
								flSortedFields.BeginUpdate( );

								flSortedFields.Clear( );
								flSortedFields.AddRange( sortedFields );

								flSortedFields.EndUpdate( );
							}

                            #endregion //Process Sorted Fields

                            #region Process Field Customizations

                            XmlNode fieldsNode = fieldLayoutNode.SelectSingleNode(FieldsTag);
                            if (fieldsNode != null)
                            {

                                LayoutInfo dragFieldLayoutInfo = null;


                                foreach (XmlNode fieldNode in fieldsNode.SelectNodes(FieldTag))
                                {
                                    string fieldName = ReadAttribute(fieldNode, NameAttrib, matchingFieldLayout.Fields[0].Name);
									// SSP 11/13/09 TFS23002
									// 
									bool excludeFieldPositionInfo = info.ExcludeCustomization( CustomizationType.FieldPosition );

									// SSP 6/30/10 TFS23826
									// 
									//Field field = GridUtilities.GetField(matchingFieldLayout, fieldName, false);
									string fieldExtendedInfo = ReadAttribute( fieldNode, FieldExtendedInfo, null );
									Field field = this.GetMatchingField( matchingFieldLayout, fieldLayoutNodeKey, fieldName, fieldExtendedInfo );

                                    //if (DoesFieldExistInFieldLayout(matchingFieldLayout, fieldName) == true)
                                    if (null != field)
                                    {
                                        // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
                                        // We're storing the fixed location on the ItemLayoutInfo as well.
                                        //
										
										
                                        
										FixedFieldLocation fixedLocation = excludeFieldPositionInfo ? FixedFieldLocation.Scrollable 
											: ReadEnumAttribute<FixedFieldLocation>( fieldNode, FixedLocationAttrib, FixedFieldLocation.Scrollable );

                                        
                                        
                                        
                                        
                                        
                                        
                                        
                                        

                                        
                                        
                                        
										// AS 7/30/09 NA 2009.2 Field Sizing
										//Field.FieldResizeInfo resizeInfo = field.ExplicitResizeInfo;
										//resizeInfo.CellWidth = ReadAttribute(fieldNode, CellWidthAttrib, double.NaN);
										//resizeInfo.CellHeight = ReadAttribute(fieldNode, CellHeightAttrib, double.NaN);
										//resizeInfo.LabelWidth = ReadAttribute(fieldNode, LabelWidthAttrib, double.NaN);
										//resizeInfo.LabelHeight = ReadAttribute(fieldNode, LabelHeightAttrib, double.NaN);
										
										
										
										this.InitializeResizeInfo( fieldNode, field, info );

                                        #region Drag and Drop customizations


                                        // SSP 2/3/09 TFS11860 TFS13101
                                        // We need to save out field's Visibility and drag-drop item layout info's _isCollapsed state.
                                        // 
										if ( _formatVersion >= 1.2 )
										{
											
											
											
											field.Visibility = excludeFieldPositionInfo ? Visibility.Visible 
												: ReadEnumAttribute<Visibility>( fieldNode, VisibilityAttrib, Visibility.Visible );

											// SSP 6/23/09 - NAS9.2 Field Chooser
											// This is used by the field chooser to force a field to be visible even if it's
											// a group-by field and CellVisibilityWhenGrouped is set to a value other than Visible.
											// 
											
											
											
											field.IgnoreFieldVisibilityOverrides = excludeFieldPositionInfo ? false 
												: ReadAttribute( fieldNode, IgnoreFieldVisibilityOverrides, false );
										}

                                        
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

                                        int row = ReadAttribute(fieldNode, RowAttrib, -1);
                                        int column = ReadAttribute(fieldNode, ColumnAttrib, -1);
                                        int rowSpan = ReadAttribute(fieldNode, RowSpanAttrib, 0);
                                        int columnSpan = ReadAttribute(fieldNode, ColumnSpanAttrib, 0);
										
										
                                        
										if ( row >= 0 && column >= 0 && rowSpan > 0 && columnSpan > 0 && ! excludeFieldPositionInfo )
                                        {
                                            ItemLayoutInfo layoutInfo = new ItemLayoutInfo(column, row, columnSpan, rowSpan);

                                            // SSP 2/3/09 TFS11860 TFS13101
                                            // We need to save out field's Visibility and drag-drop item layout info's _isCollapsed state.
                                            // 
                                            if (_formatVersion >= 1.2)
												layoutInfo.IsCollapsed = ReadAttribute( fieldNode, IsCollapsedInLayoutAttrib, false );

                                            // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
                                            layoutInfo._fixedLocation = fixedLocation;
                                            // SSP 2/3/09 TFS11860 TFS13101
                                            // Since we are saving and loading out IsCollapsedInLayoutAttrib above, we 
                                            // don't need to do this anymore.
                                            // 
                                            //layoutInfo._isCollapsed = !field.IsVisibleInCellArea;

                                            if (null == dragFieldLayoutInfo)
                                                dragFieldLayoutInfo = new LayoutInfo(matchingFieldLayout);

                                            dragFieldLayoutInfo[field] = layoutInfo;
                                        }

                                        #endregion // Drag and Drop customizations

                                        // SSP 1/19/09 - Fixed Fields
                                        // Deserialize the fixed location.
                                        // 
                                        // AS 1/19/09 NA 2009 Vol 1 - Fixed Fields
                                        // We need the fixed location for the ItemLayoutInfo as well.
                                        //
                                        //field.FixedLocation = ReadEnumAttribute<FixedFieldLocation>( fieldNode, FixedLocationAttrib, FixedFieldLocation.Scrollable );
                                        field.FixedLocation = fixedLocation;

                                        
#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)

                                    }
                                    else
                                        totalCustomizationsThatCouldNotBeApplied++;
                                }


                                // Replace any existing drag-and-drop info with what was just desirialized, even
                                // if none was deserialized. This is to cover the scenario where customizations
                                // are saved on a new grid with no drag-and-drop info and then the user re-arranges 
                                // fields and those saved customizations are loaded, the re-arranged fields should
                                // revert back to their original positions.
                                // 
								// SSP 6/26/09 - NAS9.2 Field Chooser
								// Use the new GetFieldLayoutInfo and SetFieldLayoutInfo methods instead of
								// directly accessing the member var.
								// 
                                //matchingFieldLayout._dragFieldLayoutInfo = dragFieldLayoutInfo;
								matchingFieldLayout.SetFieldLayoutInfo( dragFieldLayoutInfo, false, false );

                            }

                            #endregion //Process Field Customizations

                            #region Process Record Filters

                            // SSP 1/14/09 - NAS9.1 Record Filtering
                            // 
                            XmlNode recordFiltersNode = fieldLayoutNode.SelectSingleNode(RecordFiltersTag);
                            this.LoadRecordFilters(matchingFieldLayout, recordFiltersNode, info);

                            #endregion // Process Record Filters

							// SSP 9/3/09 TFS18172
							// Added support for saving and loading summaries.
							// 
							if ( _formatVersion >= 1.4 )
								this.LoadSummaries( matchingFieldLayout, fieldLayoutNode.SelectSingleNode( SummariesTag ), info );

                            
                            
                            
                            
                            
                            
                            this.MarkFieldSizeAndPositionAsApplied(fieldLayoutNodeKey);

							// AS 6/4/09 NA 2009.2 Undo/Redo
							_dataPresenter.History.OnCustomizationsChanged(matchingFieldLayout, CustomizationType.All);
                        }
                        else
                            totalCustomizationsThatCouldNotBeApplied++;
                    }
					// SSP 10/20/09 TFS23001
					// Added the else block. If we fail to apply a customization, we must bump the
					// totalCustomizationsThatCouldNotBeApplied.
					// 
					else
						totalCustomizationsThatCouldNotBeApplied++;
                }

                // JJD 5/07/09 - NA 2009 vol 2 - Cross band grouping
                #region Process delayed ParentFieldLayout nodes

                // Process delayed ParentFieldLayout nodes.
                foreach (KeyValuePair<FieldLayout, XmlNode> entry in delayedParentFieldLayoutNodes)
                {
                    XmlNode parentFieldLayoutNode = entry.Value;
                    FieldLayout fl = entry.Key;

                    Debug.Assert(fl.ParentFieldLayoutKey == null, "We should not have gotten here if ParentFieldLayoutKey was not null");

                    string fieldListStr = ReadAttribute(parentFieldLayoutNode, FieldListAttrib, null);
                    string keyStr = ReadAttribute(parentFieldLayoutNode, KeyAttrib, null);

                    Debug.Assert(keyStr != null && keyStr.Length > 0, "We should have serialized the ParentFieldLayout Key in delayedParentFieldLayoutNodes");
                    Debug.Assert(fieldListStr != null, "We should have serialized the FieldList for the ParentFieldLayout in delayedParentFieldLayoutNodes");

					// SSP 11/13/09 TFS23002
					// Added support for saving customization info for unloaded field-layouts.
					// 
					double nodeFormatVersion = GetFormatVersion( parentFieldLayoutNode, _formatVersion );

                    FieldDescription[] fieldDescriptions = null != fieldListStr ? this.ParseFieldList(fieldListStr) : null;
                    FieldLayoutDescription fieldLayoutNodeKey = new FieldLayoutDescription(this,
                        keyStr,
                        fieldDescriptions,
						// SSP 11/13/09 TFS23002
						// 
						nodeFormatVersion
					);


                    FieldLayout matchingFieldLayout = GetLoadedFieldLayoutThatMatchesKey(fieldLayoutNodeKey);
                    if (matchingFieldLayout != null &&
                        matchingFieldLayout.Key != null)
                    {
                        Field parentField = null; 
                        string parentFieldName = fl.ParentFieldName;

                        if (parentFieldName != null && parentFieldName.Length > 0)
                        {
                            int index = matchingFieldLayout.Fields.IndexOf(parentFieldName);

                            if (index >= 0)
                                parentField = matchingFieldLayout.Fields[index];
                        }
                        
                        fl.InitializeParentInfo(matchingFieldLayout, parentField);
                    }
                }

                #endregion //Process delayed ParentFieldLayout nodes	
    
				#endregion //FieldLayouts

				// JM NA 10.1 CardView.  Apply card resizing info.
				#region CardView Card Sizing

				XmlNode cardViewSettingsNode = rootNode.SelectSingleNode(CardViewSettingsTag);
				if (cardViewSettingsNode != null)
				{
					CardView cardView = this._dataPresenter.CurrentViewInternal as CardView;
					if (cardView != null)
					{
						CardViewSettings viewSettings = cardView.ViewSettings;
						if (viewSettings != null)
						{
							double value = ReadAttribute(cardViewSettingsNode, CardWidthAttrib, double.NaN);
							if (false == double.IsNaN(value))
								viewSettings.CardWidth = value;

							value = ReadAttribute(cardViewSettingsNode, CardHeightAttrib, double.NaN);
							if (false == double.IsNaN(value))
								viewSettings.CardHeight = value;
						}
					}
				}

				#endregion //CardView Card Sizing
			}
			finally
			{
				this._dataPresenter.IsApplyingCustomizations = false;
				this._dataPresenter.OnAfterApplyCustomizations();
				this._allLoadedCustomizationsApplied = (totalCustomizationsThatCouldNotBeApplied == 0);
			}


			return true;
		}

				#endregion //ApplyCustomizations

				#region ClearCustomizations

		public void ClearCustomizations( CustomizationType customizations )
		{
			FieldLayoutCollection fieldLayouts = _dataPresenter.FieldLayoutsIfAllocated;
			if ( null != fieldLayouts )
			{
				foreach ( FieldLayout fieldLayout in fieldLayouts )
					this.ClearCustomizations( fieldLayout, customizations );
			}

			// SSP 11/13/09 TFS23002
			// For field layouts that haven't been loaded yet, we need to keep track which customizations
			// have been cleared so we don't apply the cleared customizations to the field layout when
			// it's loaded.
			// 
			// ------------------------------------------------------------------------------------------
			if ( null != _fieldLayoutCustomizationInfos )
			{
				foreach ( KeyValuePair<FieldLayoutDescription, FieldLayoutCustomizationInfo> ii in _fieldLayoutCustomizationInfos )
					ii.Value._excludeCustomizations |= customizations;
			}
			// ------------------------------------------------------------------------------------------

			// JM NA 10.1 CardView
			if (0 != (CustomizationType.CardViewSettings & customizations))
			{
				CardView cardView = this._dataPresenter.CurrentViewInternal as CardView;
				if (cardView != null)
				{
					CardViewSettings viewSettings = cardView.ViewSettings;
					if (viewSettings != null)
					{
						viewSettings.CardWidth	= double.NaN;
						viewSettings.CardHeight = double.NaN;
					}
				}
			}
		}

		public void ClearCustomizations( FieldLayout fieldLayout, CustomizationType customizations )
		{
			fieldLayout.ClearCustomizations( customizations );
		}

				#endregion // ClearCustomizations

				#region LoadCustomizations

		public void LoadCustomizations(String customizations)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				StreamWriter sw = new StreamWriter(ms);
				sw.Write(customizations);
				sw.Flush();
				ms.Position = 0;

				this.LoadCustomizations(ms);
			}
		}

		public void LoadCustomizations(Stream stream)
		{
			using (BufferedStream bufferedStream = new BufferedStream(stream))
			{
				this._currentLoadedDocument = new XmlDocument();
				this._currentLoadedDocument.Load(bufferedStream);
			}

			this._fieldLayoutCustomizationInfos		= null;
			this._allLoadedCustomizationsApplied	= false;

			this.ApplyCustomizations();
		}
				#endregion //LoadCustomizations

				#region SaveCustomizations

		public string SaveCustomizations()
		{
			using (MemoryStream ms = new MemoryStream())
			{
				this.SaveCustomizations(ms);

				ms.Position = 0;
				StreamReader sr = new StreamReader(ms);
				return sr.ReadToEnd();
			}
		}

		public void SaveCustomizations(Stream stream)
		{
			XmlTextWriter writer = new XmlTextWriter(stream, Encoding.UTF8);
			writer.Formatting = Formatting.Indented;

			
			
			_formatVersion = LATEST_VERSION;

			writer.WriteStartDocument();
			writer.WriteStartElement(RootTag); // <xamDataPresenter>
			writer.WriteAttributeString(VersionAttrib, AssemblyVersion.Version);

			
			
			
			
			writer.WriteAttributeString( FormatVersionAttrib, _formatVersion.ToString( ) );

			#region Prepare

			writer.WriteStartElement(FieldLayoutsTag); // <fieldLayouts>

			#endregion //Prepare

			#region FieldLayouts

			foreach (FieldLayout fieldLayout in this._dataPresenter.FieldLayouts)
			{
				// JJD 1/13/12 - TFS77588
				// Bypass any field layout that has not been initialized. If there
				// were any customizations previously saved for an uninitialized fieldlayout 
				// they will be saved when we process the _fieldLayoutCustomizationInfos
				// for layouts not yet loaded below. This prevents saving it out
				// twice.
				if (fieldLayout.IsInitialized == false)
					continue;

				WriteFieldLayoutStartTag(writer, fieldLayout); // <fieldLayout>

                // JJD 5/07/09 - NA 2009 vol 2 - Cross band grouping 
                // Write out the ParentFieldLayout tag
                WriteParentFieldLayoutTag(writer, fieldLayout);

				#region FieldLayout.SortedFields

				if (fieldLayout.SortedFields.Count > 0)
				{
					writer.WriteStartElement(SortedFieldsTag); // <sortedFields>

					foreach (FieldSortDescription fieldSortDescription in fieldLayout.SortedFields)
					{
						writer.WriteStartElement(SortedFieldTag); // <sortedField>

						// SSP 6/30/10 TFS23826
						// Since we allow for multiple unbound fields to have empty names as well as same names
						// we need to store extended information that will let us better identify such fields
						// in case of name conficts.
						// 
						Field sortedField = fieldSortDescription.Field;
						if ( null != sortedField )
							writer.WriteAttributeString( FieldExtendedInfo, new FieldDescription( this, sortedField ).ToString( ) );

						WriteAttribute(writer, DirectionAttrib, fieldSortDescription.Direction);
						writer.WriteAttributeString(FieldNameAttrib, fieldSortDescription.FieldName);
						WriteAttribute(writer, IsGroupByAttrib, fieldSortDescription.IsGroupBy); 

						writer.WriteEndElement(); // <sortedField>
					}

					writer.WriteEndElement(); // <sortedFields>
				}

				#endregion //FieldLayout.SortedFields

				#region FieldLayout.Fields

				if (fieldLayout.Fields.Count > 0)
				{
					writer.WriteStartElement(FieldsTag); // <fields>

					for (int i = 0; i < fieldLayout.Fields.Count; i++)
					{
						Field field = fieldLayout.Fields[i];
						writer.WriteStartElement(FieldTag); // <field>

						writer.WriteAttributeString(NameAttrib, field.Name);

						// SSP 6/30/10 TFS23826
						// Since we allow for multiple unbound fields to have empty names as well as same names
						// we need to store extended information that will let us better identify such fields
						// in case of name conficts.
						// 
						writer.WriteAttributeString( FieldExtendedInfo, new FieldDescription( this, field ).ToString( ) );

						// SSP 1/19/09 - Fixed Fields
						// 
						// AS 1/22/10 Coerce FixedLocation
						// We may have coerced the fixed location to scrollable.
						//
						//FixedFieldLocation fixedLocation = field.FixedLocation;
						FixedFieldLocation fixedLocation = field.ActualFixedLocation;

						if ( FixedFieldLocation.Scrollable != fixedLocation )
							WriteAttribute( writer, FixedLocationAttrib, fixedLocation );

						Field.FieldResizeInfo resizeInfo = field.ExplicitResizeInfoIfAllocated;
						if ( null != resizeInfo )
						{
							// AS 7/30/09 NA 2009.2 Field Sizing
							//WriteAttribute( writer, CellWidthAttrib, resizeInfo.CellWidth, true );
							//WriteAttribute( writer, CellHeightAttrib, resizeInfo.CellHeight, true );
							//WriteAttribute( writer, LabelWidthAttrib, resizeInfo.LabelWidth, true );
							//WriteAttribute( writer, LabelHeightAttrib, resizeInfo.LabelHeight, true );
							SaveResizeInfo(writer, resizeInfo);
						}

						#region Drag and Drop customizations
						// --------------------------------------------------------------------------------

						// SSP 2/3/09 TFS11860 TFS13101
						// We need to save out field's Visibility and drag-drop item layout info's _isCollapsed state.
						// 
						if ( _formatVersion >= 1.2 )
						{
							WriteAttribute( writer, VisibilityAttrib, field.Visibility );

							// SSP 6/23/09 - NAS9.2 Field Chooser
							// This is used by the field chooser to force a field to be visible even if it's
							// a group-by field and CellVisibilityWhenGrouped is set to a value other than Visible.
							// 
							WriteAttribute( writer, IgnoreFieldVisibilityOverrides, field.IgnoreFieldVisibilityOverrides );
						}

						
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

						// SSP 6/26/09 - NAS9.2 Field Chooser
						// Use the new GetFieldLayoutInfo and SetFieldLayoutInfo methods instead of
						// directly accessing the member var.
						// 
						//if ( null != fieldLayout._dragFieldLayoutInfo )
						LayoutInfo fieldLayoutInfo = fieldLayout.GetFieldLayoutInfo( false, false );
						if ( null != fieldLayoutInfo )
						{
							ItemLayoutInfo layoutInfo;
							// SSP 6/26/09 - NAS9.2 Field Chooser
							// 
							//fieldLayout._dragFieldLayoutInfo.TryGetValue( field, out layoutInfo );
							fieldLayoutInfo.TryGetValue( field, out layoutInfo );

							if ( null != layoutInfo )
							{
								WriteAttribute( writer, RowAttrib, layoutInfo.Row );
								WriteAttribute( writer, ColumnAttrib, layoutInfo.Column );
								WriteAttribute( writer, RowSpanAttrib, layoutInfo.RowSpan );
								WriteAttribute( writer, ColumnSpanAttrib, layoutInfo.ColumnSpan );

								// SSP 2/3/09 TFS11860 TFS13101
								// We need to save out field's Visibility and drag-drop item layout info's _isCollapsed state.
								// 
								if ( _formatVersion >= 1.2 )
									WriteAttribute( writer, IsCollapsedInLayoutAttrib, layoutInfo.IsCollapsed );
							}
						}

						// --------------------------------------------------------------------------------
						#endregion // Drag and Drop customizations

						writer.WriteEndElement(); // <field>
					}

					writer.WriteEndElement(); // <fields>
				}

				#endregion //FieldLayout.Fields

				// SSP 1/14/09 - NAS9.1 Record Filtering
				// 
				this.SaveRecordFilters( writer, fieldLayout );
				
				// SSP 9/3/09 TFS18172
				// Added support for saving and loading summaries.
				// 
				if ( _formatVersion >= 1.4 )
					this.SaveSummaries( writer, fieldLayout );

				writer.WriteEndElement(); // <fieldLayout>
			}

			#endregion //FieldLayouts

			#region FieldLayouts not loaded yet

			// SSP 11/13/09 TFS23002
			// Save the info for the field layouts that haven't been loaded yet.
			// 
			if ( null != _fieldLayoutCustomizationInfos && null != _currentLoadedDocument )
			{
				foreach ( KeyValuePair<FieldLayoutDescription, FieldLayoutCustomizationInfo> ii in _fieldLayoutCustomizationInfos )
				{
					FieldLayoutCustomizationInfo info = ii.Value;
					if ( !info._customizationsApplied
						&& null != info._fieldLayoutNode
						&& CustomizationType.All != ( CustomizationType.All & info._excludeCustomizations ) )
					{
						XmlNode node = info._fieldLayoutNode;
						node.Attributes.RemoveNamedItem( ExcludeCustomizations );
						node.Attributes.RemoveNamedItem( FormatVersionAttrib );

						if ( CustomizationType.None != info._excludeCustomizations )
						{
							XmlAttribute attr = _currentLoadedDocument.CreateAttribute( ExcludeCustomizations );
							attr.Value = GridUtilities.ToStringFromFlaggedEnum( info._excludeCustomizations, ", " );
							node.Attributes.Append( attr );
						}

						if ( info._nodeFormatVersion != _formatVersion )
						{
							XmlAttribute attr = _currentLoadedDocument.CreateAttribute( FormatVersionAttrib );
							attr.Value = info._nodeFormatVersion.ToString( );
							node.Attributes.Append( attr );
						}

						node.WriteTo( writer );
					}
				}
			}

			#endregion // FieldLayouts not loaded yet

			#region Cleanup

			writer.WriteEndElement(); // <fieldLayouts>

			#endregion //Cleanup

			// JM NA 10.1 CardView.  Save card resizing info.
			#region CardView Card Sizing

			CardView cardView = this._dataPresenter.CurrentViewInternal as CardView;
			if (cardView != null)
			{
				CardViewSettings viewSettings = cardView.ViewSettings;
				if (viewSettings != null)
				{
					if (false == double.IsNaN(viewSettings.CardWidth) ||
						false == double.IsNaN(viewSettings.CardHeight))
					{
						writer.WriteStartElement(CardViewSettingsTag);

						if (false == double.IsNaN(viewSettings.CardWidth))
							WriteAttribute(writer, CardWidthAttrib, viewSettings.CardWidth);

						if (false == double.IsNaN(viewSettings.CardHeight))
							WriteAttribute(writer, CardHeightAttrib, viewSettings.CardHeight);

						writer.WriteEndElement();
					}
				}
			}

			#endregion //CardView Card Sizing

			writer.WriteEndElement(); // </xamDataPresenter>
			writer.WriteEndDocument();
			writer.Flush();
		}

				#endregion //SaveCustomizations

			#endregion //Internal Methods

			#region Private Methods

				#region ConstructFieldLayoutUniqueKey

		private static string ConstructFieldLayoutUniqueKey(FieldLayout fieldLayout)
		{
			StringBuilder uniqueKey = new StringBuilder();
			uniqueKey.Append("[");
			uniqueKey.Append(fieldLayout.DataPresenter.FieldLayouts.IndexOf(fieldLayout).ToString());
			uniqueKey.Append("]");

			for(int i = 0; i < fieldLayout.Fields.Count; i++)
			{
				Field field = fieldLayout.Fields[i];
				if (string.IsNullOrEmpty(field.Name) == false)
					uniqueKey.Append(field.Name);
			}

			return uniqueKey.ToString();
		}

				#endregion //ParseEnum (string)

				#region ConstructFieldList

		
		
		
		/// <summary>
		/// Creates list of fields.
		/// </summary>
		/// <param name="fl">Field layout.</param>
		/// <returns>Returns a string that contains field keys and field type information.</returns>
		private string ConstructFieldList( FieldLayout fl )
		{
			FieldCollection fields = fl.Fields;

			List<string> items = new List<string>( );

			for ( int i = 0, count = fields.Count; i < count; i++ )
				items.Add( new FieldDescription( this, fields[i] ).ToString( ) );

			return FieldListPraser.ConstructList( items.ToArray( ), true );
		}

		[ThreadStatic()]
		private static Parser_ListOfItems g_parser_ListOfItems;

		private static Parser_ListOfItems FieldListPraser
		{
			get
			{
				if ( null == g_parser_ListOfItems )
					g_parser_ListOfItems = new Parser_ListOfItems( '"', '"', '\\', ',' );

				return g_parser_ListOfItems;
			}
		}

		private FieldDescription[] ParseFieldList( string fieldList )
		{
			if ( null == fieldList )
				return null;

			List<FieldDescription> list = new List<FieldDescription>( );

			string[] parts = FieldListPraser.ParseList( fieldList, false );
			if ( null != parts )
			{
				for ( int i = 0; i < parts.Length; i++ )
				{
					FieldDescription fieldDescription = FieldDescription.Parse( this, parts[i] );
					if ( null != fieldDescription )
						list.Add( fieldDescription );
				}
			}

			return list.ToArray( );
		}

		

				#endregion // ConstructFieldList

				#region DoesFieldExistInFieldLayout

		private static bool DoesFieldExistInFieldLayout(FieldLayout fieldLayout, string fieldName)
		{
			return fieldLayout.Fields.IndexOf(fieldName) > -1;
		}

				#endregion //DoesFieldExistInFieldLayout

				#region GetFieldLayoutCustomizationInfo

		
		
		private FieldLayoutCustomizationInfo GetFieldLayoutCustomizationInfo( FieldLayoutDescription fieldLayoutKey, bool allocateIfNecessary )
		{
			FieldLayoutCustomizationInfo fieldLayoutCustomizationInfo;

			this.FieldLayoutCustomizationInfos.TryGetValue( fieldLayoutKey, out fieldLayoutCustomizationInfo );
			if ( fieldLayoutCustomizationInfo == null )
			{
				fieldLayoutCustomizationInfo = new FieldLayoutCustomizationInfo( fieldLayoutKey );
				this.FieldLayoutCustomizationInfos.Add( fieldLayoutKey, fieldLayoutCustomizationInfo );
			}

			return fieldLayoutCustomizationInfo;
		}

				#endregion // GetFieldLayoutCustomizationInfo

				#region GetFieldLayoutKey

		// SSP 11/13/09 TFS23002
		// Added support for saving customization info for unloaded field-layouts.
		// 
		//private string GetFieldLayoutKey(FieldLayout fieldLayout)
		private string GetFieldLayoutKey( FieldLayout fieldLayout, double formatVersion )
		{
			
			
			
			string retVal = null;
			object key = fieldLayout.Key;

			if ( null != key )
			{
				// SSP 11/13/09 TFS23002
				// Use the new formatVersion parameter.
				// 
				//if ( key is ITypedList && _formatVersion >= 1.1 )
				if ( key is ITypedList && formatVersion >= 1.1 )
					retVal = ( (ITypedList)key ).GetListName( null );
				else if ( key is Type )
					retVal = ( (Type)key ).Name;
				else if ( key.GetType( ) == typeof( XmlElement ) )
					retVal = ( (XmlElement)key ).Name;
				else if ( TypeDescriptor.GetConverter( key ).CanConvertTo( typeof( string ) ) )
					retVal = Convert.ToString( key );
			}

			if ( null == retVal )
			{
				// SSP 11/13/09 TFS23002
				// Use the new formatVersion parameter.
				// 
				//if ( _formatVersion >= 1.1 )
				if ( formatVersion >= 1.1 )
					// Use empty string as the key
					retVal = string.Empty;
				else
					retVal = ConstructFieldLayoutUniqueKey( fieldLayout );
			}

			return retVal;

			
#region Infragistics Source Cleanup (Region)


















#endregion // Infragistics Source Cleanup (Region)

			
		}

				#endregion //GetFieldLayoutKey	

				#region GetFormatVersion

		// SSP 11/13/09 TFS23002
		// 
		internal static double GetFormatVersion( XmlNode node, double defaultVersion )
		{
			string formatVersionStr = GridUtilities.GetAttribute( node, FormatVersionAttrib );
			double retVal;
			if ( string.IsNullOrEmpty( formatVersionStr ) || !double.TryParse( formatVersionStr, out retVal ) )
				retVal = defaultVersion;

			return retVal;
		}

				#endregion // GetFormatVersion
    
				#region GetLoadedFieldLayoutThatMatchesKey

		
		
		
		
		private FieldLayout GetLoadedFieldLayoutThatMatchesKey( FieldLayoutDescription key )
		{
			foreach (FieldLayout fieldLayout in this._dataPresenter.FieldLayouts)
			{
				
				
				
				
				




                // JJD 5/15/09 - NA 2009 vol 2 - Cross band grouping
                // Pass false as the ignoreKeyCompare parameter on the 1st pass so
                // we try to match up on key value as well as fields
				//if ( key.DoesFieldLayoutMatch( fieldLayout ) )
				if ( key.DoesFieldLayoutMatch( fieldLayout, false ) )
					return fieldLayout;
				
			}

            // JJD 5/15/09 - NA 2009 vol 2 - Cross band grouping
            // When de-serializing versions 1.1 and 1.2 we want to
            // do a 2nd pass of the fieldlayouts ignoring the actual key
            // value and matching on the field list only. This is because
            // the Key value changed in version 1.3.
            // E.g. with DataViews the key used to be the DataView. Now its a string
            // with the table name.
            if (this._formatVersion >= 1.1 && this._formatVersion < 1.3)
            {
                foreach (FieldLayout fieldLayout in this._dataPresenter.FieldLayouts)
                {
                    if (key.DoesFieldLayoutMatch(fieldLayout, true))
                        return fieldLayout;
                }
            }

			return null;
		}

				#endregion //GetLoadedFieldLayoutThatMatchesKey	

				#region GetMatchingField

		// SSP 6/30/10 TFS23826
		// 
		private Field GetMatchingField( FieldLayout fieldLayout, FieldLayoutDescription fieldLayoutDescription, string fieldName, string extendedInfo )
		{
			// Non-existence of fieldLayoutDescription means that the layout was saved before we introduced it.
			// Also note that the presence of extendedInfo means the formatVersion >= 1.6 since that's when we
			// started embedding it in the field and sorted field nodes.
			// 
			if ( null != fieldLayoutDescription && !string.IsNullOrEmpty( extendedInfo ) )
			{
				FieldDescription fd = FieldDescription.Parse( this, extendedInfo );
				Debug.Assert( null != fd && fd._name == fieldName );
				if ( null != fd )
				{
					// SSP 8/25/11 TFS83385
					// 
					// ------------------------------------------------------------------------------------
					//foreach ( Field ii in fieldLayout.Fields )
					//{
					//    if ( fieldLayoutDescription.DoesFieldMatch( ii, fd ) )
					//        return ii;
					//}

					foreach ( FieldLayoutDescription.FieldDescriptionEqualityComparer comparer in fieldLayoutDescription._criteriaComparers )
					{
						foreach ( Field ii in fieldLayout.Fields )
						{
							if ( fieldLayoutDescription.DoesFieldMatch( ii, fd, comparer ) )
								return ii;
						}
					}
					// ------------------------------------------------------------------------------------

					return null;
				}
			}

			return GridUtilities.GetField( fieldLayout, fieldName, false );
		}

				#endregion // GetMatchingField

				#region	HasCustomizationsBeenAppliedToFieldLayout

		
		
		
		
		
		
		private bool HasCustomizationsBeenAppliedToFieldLayout( FieldLayoutDescription fieldLayoutKey )
		{
			FieldLayoutCustomizationInfo fieldLayoutCustomizationInfo;

			if ( this.FieldLayoutCustomizationInfos.TryGetValue( fieldLayoutKey, out fieldLayoutCustomizationInfo ) )
				return fieldLayoutCustomizationInfo._customizationsApplied;
			else
				return false;
		}

				#endregion // HasCustomizationsBeenAppliedToFieldLayout

				// AS 7/30/09 NA 2009.2 Field Sizing
				#region InitializeResizeInfo
		
		
		
		private void InitializeResizeInfo( XmlNode fieldNode, Field field, FieldLayoutCustomizationInfo info )
		{
			Field.FieldResizeInfo resizeInfo = field.ExplicitResizeInfo;
			InitializeResizeInfo(fieldNode, resizeInfo, CellWidthAttrib, CellWidthAutoAttrib, false, true, info);
			InitializeResizeInfo(fieldNode, resizeInfo, CellHeightAttrib, CellHeightAutoAttrib, false, false, info);
			InitializeResizeInfo(fieldNode, resizeInfo, LabelWidthAttrib, LabelWidthAutoAttrib, true, true, info);
			InitializeResizeInfo(fieldNode, resizeInfo, LabelHeightAttrib, LabelHeightAutoAttrib, true, false, info);
		}

		
		
		
		private void InitializeResizeInfo( XmlNode fieldNode, Field.FieldResizeInfo resizeInfo, string valueAttrib, string autoAttrib, bool label, bool width, FieldLayoutCustomizationInfo info )
		{
			
			
			bool excludeResizeInfo = info.ExcludeCustomization( CustomizationType.FieldExtent );

			
			
			
			
			double value = excludeResizeInfo ? double.NaN : ReadAttribute( fieldNode, valueAttrib, double.NaN );
			bool isAuto = excludeResizeInfo ? false : ReadAttribute(fieldNode, autoAttrib, false);

			FieldSize newSize;

			ItemSizeType sizeType = ItemSizeType.Explicit;

			// AS 9/30/09 TFS22891
			// If the layout information is being applied before the initial
			// auto size was applied then clear the pending auto size flag 
			// so we don't step on the size being loaded. Note we only want 
			// to do this when the orientation matches the extent being manipulated.
			//
			Field field = resizeInfo.Field;

			bool isHorz = field.Owner != null && field.Owner.IsHorizontal;

			if (isHorz != width)
				field._hasPendingInitialAutoSize = false;

			if (!isAuto)
				sizeType = ItemSizeType.Explicit;
			else
			{
				// the item was autosized previously
				FieldLength len = field.GetWidthOrHeight(width);

				// if the field is an initial auto then we should recreate the snapshot
				// which means we need to reset the pending initial size flag
				// AS 9/30/09 TFS22891
				//if (len.IsInitialAuto)
				if (isHorz != width && len.IsInitialAuto)
					field._hasPendingInitialAutoSize = true;

				// if its any type of auto then store this as an auto mode size
				if (len.IsAnyAuto)
				{
					FieldSize currentSize = field.GetResizeSize(label, width);

					// if the field is currently in automode (initial or autosizing) then 
					// we want to use the max of the saved value and current value so that 
					// the field doesn't reduce in size after simply saving and loading a 
					// layout
					if (currentSize.Type == ItemSizeType.AutoMode)
						value = AutoSizeFieldHelper.Max(value, currentSize.Value);
					
					sizeType = ItemSizeType.AutoMode;
				}
				else
					sizeType = ItemSizeType.ExplicitAutoSize;
			}

			newSize = new FieldSize(value, sizeType);
			resizeInfo.SetSize(label, width, newSize);
		}
				#endregion //InitializeResizeInfo

				#region LoadRecordFilters

		// SSP 1/14/09 - NAS9.1 Record Filtering
		// 
		// SSP 11/13/09 TFS23002
		// Added support for saving customization info for unloaded field-layouts.
		// 
		//private void LoadRecordFilters( FieldLayout fieldLayout, XmlNode filtersNode )
		private void LoadRecordFilters( FieldLayout fieldLayout, XmlNode filtersNode, FieldLayoutCustomizationInfo info )
		{
			DataPresenterObjectSerializer serializer = new DataPresenterObjectSerializer( );
			object v;

			if ( fieldLayout.HasRecordFilters )
				fieldLayout.RecordFilters.Clear( );

			// SSP 11/13/09 TFS23002
			// Added support for saving customization info for unloaded field-layouts.
			// 
			//if ( null != filtersNode )
			if ( null != filtersNode && !info.ExcludeCustomization( CustomizationType.RecordFilters ) )
			{
				foreach ( XmlNode node in filtersNode.ChildNodes )
				{
					if ( serializer.ParseElement( node as XmlElement, typeof( RecordFilter ), out v ) )
						fieldLayout.RecordFilters.Add( (RecordFilter)v );
				}
			}
		}

				#endregion // LoadRecordFilters

				#region LoadSummaries

		// SSP 9/3/09 TFS18172
		// Added support for saving and loading summaries.
		// 
		
		
		
		private void LoadSummaries( FieldLayout fieldLayout, XmlNode summariesNode, FieldLayoutCustomizationInfo info )
		{
			DataPresenterObjectSerializer serializer = new DataPresenterObjectSerializer( );
			object v;

			if ( null != fieldLayout.SummaryDefinitionsIfAllocated )
				fieldLayout.SummaryDefinitions.Clear( );

			
			
			
			if ( null != summariesNode && ! info.ExcludeCustomization( CustomizationType.Summaries ) )
			{
				foreach ( XmlNode node in summariesNode.ChildNodes )
				{
					if ( serializer.ParseElement( node as XmlElement, typeof( SummaryDefinition ), out v ) )
						fieldLayout.SummaryDefinitions.Add( (SummaryDefinition)v );
				}
			}
		}

				#endregion // LoadSummaries

				#region MarkFieldSizeAndPositionAsApplied

		
		
		
		
		
		
		private void MarkFieldSizeAndPositionAsApplied( FieldLayoutDescription fieldLayoutKey )
		{
			
			
			
			FieldLayoutCustomizationInfo fieldLayoutCustomizationInfo = this.GetFieldLayoutCustomizationInfo( fieldLayoutKey, true );

			fieldLayoutCustomizationInfo._customizationsApplied = true;
		}

				#endregion //MarkFieldSizeAndPositionAsApplied

				#region Commented Out Code
				
#region Infragistics Source Cleanup (Region)





































































#endregion // Infragistics Source Cleanup (Region)

				#endregion // Commented Out Code

				#region ParseEnum (string)



#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)

		private static T ParseEnum<T>(string value)
		{
			Debug.Assert(typeof(T).IsEnum);

			return (T)Enum.Parse(typeof(T), value, false);
		}

				#endregion //ParseEnum (string)

				#region ReadAttribute

		private static string ReadAttribute(XmlNode node, string name, string defaultValue)
		{
			XmlAttribute attrib = node.Attributes[name];

			if (attrib != null)
				return attrib.Value;

			return defaultValue;
		}

		private static bool ReadAttribute(XmlNode node, string name, bool defaultValue)
		{
			string attrib = ReadAttribute(node, name, null);

			return attrib != null ? XmlConvert.ToBoolean(attrib) : defaultValue;
		}

		private static int ReadAttribute(XmlNode node, string name, int defaultValue)
		{
			string attrib = ReadAttribute(node, name, null);

			return attrib != null ? XmlConvert.ToInt32(attrib) : defaultValue;
		}

		private static DateTime ReadAttribute(XmlNode node, string name, DateTime defaultValue)
		{
			string attrib = ReadAttribute(node, name, null);

			return attrib != null ? XmlConvert.ToDateTime(attrib, XmlDateTimeSerializationMode.Local) : defaultValue;
		}

		private static double ReadAttribute(XmlNode node, string name, double defaultValue)
		{
			string attrib = ReadAttribute(node, name, null);

			return attrib != null ? XmlConvert.ToDouble(attrib) : defaultValue;
		}

		private static T ReadAttributeWithConverter<T>(XmlNode node, string name, T defaultValue)
		{
			string attrib = ReadAttribute(node, name, null);

			if (attrib == null)
				return defaultValue;

			TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));

			return (T)converter.ConvertFromString(null, CultureInfo.InvariantCulture, attrib);
		}

		private static T ReadEnumAttribute<T>(XmlNode node, string name, T defaultValue)
		{
			Debug.Assert(typeof(T).IsEnum);
			string attrib = ReadAttribute(node, name, null);

			return attrib != null ? (T)Enum.Parse(defaultValue.GetType(), attrib) : defaultValue;
		}

				#endregion //ReadAttribute

				#region SaveRecordFilters

		// SSP 1/14/09 - NAS9.1 Record Filtering
		// 
		private void SaveRecordFilters( XmlWriter writer, FieldLayout fieldLayout )
		{
			if ( fieldLayout.HasRecordFilters )
			{
				RecordFilterCollection filters = fieldLayout.RecordFilters;
				writer.WriteStartElement( RecordFiltersTag ); // <recordFilters>

				DataPresenterObjectSerializer serializer = new DataPresenterObjectSerializer( );

				foreach ( RecordFilter rf in filters )
				{
					if ( rf.HasConditions )
					{
						XmlElement elem;
						if ( serializer.SaveAsElement( rf, typeof( RecordFilter ), out elem ) )
							elem.WriteTo( writer );
					}
				}

				writer.WriteEndElement( ); // </recordFilters>
			}
		}

				#endregion // SaveRecordFilters

				// AS 7/30/09 NA 2009.2 Field Sizing
				#region SaveResizeInfo
		private static void SaveResizeInfo(XmlTextWriter writer, Field.FieldResizeInfo resizeInfo)
		{
			SaveResizeInfo(writer, resizeInfo, false, true, CellWidthAttrib, CellWidthAutoAttrib);
			SaveResizeInfo(writer, resizeInfo, false, false, CellHeightAttrib, CellHeightAutoAttrib);
			SaveResizeInfo(writer, resizeInfo, true, true, LabelWidthAttrib, LabelWidthAutoAttrib);
			SaveResizeInfo(writer, resizeInfo, true, false, LabelHeightAttrib, LabelHeightAutoAttrib);
		}

		private static void SaveResizeInfo(XmlTextWriter writer, Field.FieldResizeInfo resizeInfo, bool label, bool width, string valueAttrib, string autoAttrib)
		{
			FieldSize size = resizeInfo.GetSize(label, width);

			// if it was auto (explicit or more) then keep a flag to that effect
			if (size.IsAuto)
			{
				// otherwise save the actual value (whether it is explicit/explicit auto
				WriteAttribute(writer, autoAttrib, true);
			}

			if (size.Type != ItemSizeType.AutoMode)
			{
				// otherwise save the actual value (whether it is explicit/explicit auto
				WriteAttribute(writer, valueAttrib, size.Value, true);
			}
		}
				#endregion //SaveResizeInfo

				#region SaveSummaries

		// SSP 9/3/09 TFS18172
		// Added support for saving and loading summaries.
		// 
		private void SaveSummaries( XmlWriter writer, FieldLayout fieldLayout )
		{
			SummaryDefinitionCollection summaries = fieldLayout.SummaryDefinitionsIfAllocated;
			if ( null != summaries && summaries.Count > 0 )
			{
				writer.WriteStartElement( SummariesTag ); // <summaries>

				DataPresenterObjectSerializer serializer = new DataPresenterObjectSerializer( );

				foreach ( SummaryDefinition summary in summaries )
				{
					XmlElement elem;
					if ( serializer.SaveAsElement( summary, typeof( SummaryDefinition ), out elem ) )
						elem.WriteTo( writer );
				}

				writer.WriteEndElement( ); // </summaries>
			}
		}

				#endregion // SaveSummaries

				#region WriteAttribute

		private static void WriteAttribute( XmlWriter writer, string name, double value, bool dontWriteIfNaN )
		{
			if ( dontWriteIfNaN && double.IsNaN( value ) )
				return;

			writer.WriteAttributeString( name, XmlConvert.ToString( value ) );
		}

		private static void WriteAttribute(XmlWriter writer, string name, double value)
		{
			writer.WriteAttributeString(name, XmlConvert.ToString(value));
		}

		private static void WriteAttribute(XmlWriter writer, string name, Enum value)
		{
			writer.WriteAttributeString(name, value.ToString());
		}

		private static void WriteAttribute(XmlWriter writer, string name, int value)
		{
			writer.WriteAttributeString(name, XmlConvert.ToString(value));
		}

		private static void WriteAttribute(XmlWriter writer, string name, bool value)
		{
			writer.WriteAttributeString(name, XmlConvert.ToString(value));
		}

		private static void WriteAttribute(XmlWriter writer, string name, Size value)
		{
			if (value.IsEmpty == false)
				WriteAttribute(writer, name, value, TypeDescriptor.GetConverter(typeof(Size)));
		}

		private static void WriteAttribute(XmlWriter writer, string name, Rect value)
		{
			if (value.IsEmpty == false)
				WriteAttribute(writer, name, value, TypeDescriptor.GetConverter(typeof(Rect)));
		}

		private static void WriteAttribute(XmlWriter writer, string name, Point? value)
		{
			if (null != value)
				WriteAttribute(writer, name, value.Value, TypeDescriptor.GetConverter(typeof(Point)));
		}

		private static void WriteAttribute(XmlWriter writer, string name, object value, TypeConverter converter)
		{
			writer.WriteAttributeString(name, converter.ConvertToString(null, CultureInfo.InvariantCulture, value));
		}

		private static void WriteAttribute(XmlTextWriter writer, string name, DateTime dateTime)
		{
			writer.WriteAttributeString(name, XmlConvert.ToString(dateTime, XmlDateTimeSerializationMode.Utc));
		}

				#endregion //WriteAttribute

				#region WriteFieldLayoutStartTag

		private void WriteFieldLayoutStartTag(XmlWriter writer, FieldLayout fieldLayout)
		{
			writer.WriteStartElement(FieldLayoutTag);

            // JJD 5/07/09 - NA 2009 vol 2 - Cross band grouping
            // write out the parent field name
            string parentFieldName = fieldLayout.ParentFieldName;
            if (parentFieldName != null && parentFieldName.Length > 0)
			    writer.WriteAttributeString(ParentFieldNameAttrib, parentFieldName);

			// SSP 11/13/09 TFS23002
			// Added formatVersion parameter to the GetFieldLayoutKey. Pass that along.
			// 
			//writer.WriteAttributeString(KeyAttrib, this.GetFieldLayoutKey(fieldLayout));
			writer.WriteAttributeString( KeyAttrib, this.GetFieldLayoutKey( fieldLayout, _formatVersion ) );

			
			
			
			writer.WriteAttributeString( FieldListAttrib, this.ConstructFieldList( fieldLayout ) );
		}

				#endregion //WriteFieldLayoutStartTag

                // JJD 5/07/09 - NA 2009 vol 2 - Cross band grouping - added
				#region WriteParentFieldLayoutTag

        private void WriteParentFieldLayoutTag(XmlWriter writer, FieldLayout fieldLayout)
		{
            FieldLayout parentFieldLayout = fieldLayout.ParentFieldLayout;

            // if we have a reference to the parentFieldLayout we can proceed
            if (parentFieldLayout == null)
                return;

            // write the start tag
			writer.WriteStartElement(ParentFieldLayoutTag);

            // we need to construct a unique key along with a list of fields
			
			
            
			writer.WriteAttributeString( KeyAttrib, this.GetFieldLayoutKey( parentFieldLayout, _formatVersion ) );

            writer.WriteAttributeString(FieldListAttrib, this.ConstructFieldList(parentFieldLayout));

            // close the element tag
            writer.WriteEndElement(); //</ParentFieldLayout>
		}

				#endregion //WriteFieldLayoutStartTag

			#endregion //Private Methods

		#endregion //Methods

		#region FieldLayoutCustomizationInfo Class

		private class FieldLayoutCustomizationInfo
		{
			
			
			
			
			
			
			//private Dictionary<string, string>	_sortedFieldsWithCustomizationsApplied;
			//private Dictionary<string, string>	_fieldsWithCustomizationsApplied;
			internal bool _customizationsApplied;

			
			
			
			
			private FieldLayoutDescription _key;

			// SSP 11/13/09 TFS23002
			// 
			internal CustomizationType _excludeCustomizations = 0;
			internal XmlNode _fieldLayoutNode;
			internal double _nodeFormatVersion;

			
			
			
			
			internal FieldLayoutCustomizationInfo( FieldLayoutDescription key )
			{
				this._key = key;
			}

			// SSP 11/13/09 TFS23002
			// 
			internal bool ExcludeCustomization( CustomizationType customizationType )
			{
				return customizationType == ( customizationType & _excludeCustomizations );
			}

			#region Commented Out Code

			
			
			
			
			
			
			
#region Infragistics Source Cleanup (Region)



















#endregion // Infragistics Source Cleanup (Region)


			#endregion // Commented Out Code
		}

		#endregion //FieldLayoutCustomizationInfo Class
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