using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Globalization;

namespace Infragistics.Documents.Excel.StructuredStorage.FileTypes
{
	internal class DocumentSummaryInformation : InformationBase
	{
		#region Constants

		private const int FirstUserPropertyType = 2;
		private const string UserSectionId = "d5cdd5052e9c101b939708002b2cf9ae";

		#endregion Constants

		#region Member Variables

		private DocumentSummaryInformationProperties properties;
		//private DocumentSummaryInformationUserDefinedProperties userDefinedProperties;

		// MD 1/30/08 - BR30189
		private Dictionary<UserDefinedPropertyType, object> userDefinedProperties;

		private List<string> userDefinedNames;
		private Dictionary<string, object> userDefinedPropertiesRaw;
        private Encoding userSectionEncoding = Utilities.EncodingDefault;

		#endregion Member Variables

		#region Constructor

		public DocumentSummaryInformation()
		{
			this.userDefinedNames = new List<string>();
			// MD 4/6/12 - TFS101506
			//this.userDefinedPropertiesRaw = new Dictionary<string, object>( StringComparer.CurrentCultureIgnoreCase );
			this.userDefinedPropertiesRaw = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

			// MD 1/30/08 - BR30189
			this.userDefinedProperties = new Dictionary<UserDefinedPropertyType, object>();
		}

		#endregion Constructor

		#region Base Class Overrides

		#region DefaultSectionId

		protected override string DefaultSectionId
		{
			get { return "d5cdd5022e9c101b939708002b2cf9ae"; }
		}

		#endregion DefaultSectionId

		#region PopuplateSectionDefinitions

		protected override void PopuplateSectionDefinitions( List<PropertySectionDefinition> sectionDefinitions )
		{
			// Populate the default section
			base.PopuplateSectionDefinitions( sectionDefinitions );

			// If there is at least one user property, propulate the user property section
			if ( this.userDefinedPropertiesRaw.Count > 0 )
			{
				PropertySectionDefinition definition = new PropertySectionDefinition();
				definition.sectionType = new Guid( DocumentSummaryInformation.UserSectionId );
				sectionDefinitions.Add( definition );
			}
		}

		#endregion PopuplateSectionDefinitions

		#region ReadSectionProperties

		protected override void ReadSectionProperties( BinaryReader reader, PropertySectionDefinition sectionDefintion )
		{
			string sectionId = sectionDefintion.sectionType.ToString( "N" );

			// If the section is the user section, do special processing for it.
			if ( sectionId == DocumentSummaryInformation.UserSectionId )
			{
				// Read the section's code page so we know how to parse string properties to follow
				this.userSectionEncoding = this.ReadSectionCodePage( reader, sectionDefintion );

				// Read the rest of the properties in the section
				for ( int j = 0; j < sectionDefintion.propertyCount; j++ )
				{
					PropertyDefinition definition = sectionDefintion.propertyDefinitions[ j ];

					// Read all properties but the code page
					if ( definition.type != 1 )
						this.ReadUserDefinedProperty( reader, definition );
				}
			}
			else
			{
				// Otherwise, let the base class read the section
				base.ReadSectionProperties( reader, sectionDefintion );
			}
		}

		#endregion ReadSectionProperties

		#region VeridyEncoding

		protected override void VeridyEncoding()
		{
			base.VeridyEncoding();

			List<string> stringValues = new List<string>();

			foreach ( string name in this.userDefinedPropertiesRaw.Keys )
				stringValues.Add( name );

			foreach ( object value in this.userDefinedPropertiesRaw.Values )
			{
				string stringValue = value as string;

				if ( stringValue != null )
					stringValues.Add( stringValue );
			}

			this.userSectionEncoding = InformationBase.GetRequiredEncoding( stringValues );
		}

		#endregion VeridyEncoding

		#region WriteSectionProperties

		protected override void WriteSectionProperties( BinaryWriter writer, PropertySectionDefinition sectionDefintion )
		{
			string sectionId = sectionDefintion.sectionType.ToString( "N" );

			// If the section is the user section, do special processing for it.
			if ( sectionId == DocumentSummaryInformation.UserSectionId )
			{
				// The number of properties in the section is the number of user defined properties plus 3:
				// the code page, the locale, and the user property dictionary
				sectionDefintion.propertyCount = this.userDefinedPropertiesRaw.Count + 3;
				sectionDefintion.propertyDefinitions = new List<PropertyDefinition>();

				// Advance the stream past the property header section, which contains an int for the type and an
				// int for the stream offset of each property in the section (the offsets are from the start of the section)
				writer.BaseStream.Position += sectionDefintion.propertyCount * 8;

				// Write the user property dictionary as the first property 
				PropertyDefinition dictionaryDefinition = new PropertyDefinition();
				dictionaryDefinition.positionOfValue = (int)writer.BaseStream.Position;
				dictionaryDefinition.type = (int)UserDefinedPropertyType.Dictionary;
				this.WriteUserDefinedProperty( writer, dictionaryDefinition );
				sectionDefintion.propertyDefinitions.Add( dictionaryDefinition );

				// Write the code page as the second property
				PropertyDefinition codePageDefinition = new PropertyDefinition();
				codePageDefinition.positionOfValue = (int)writer.BaseStream.Position;
				codePageDefinition.type = InformationBase.CodePagePropertyType;
				VariantUtilities.WriteVariant( writer, this.userSectionEncoding, (short)Utilities.EncodingGetCodePage(this.userSectionEncoding) );
				sectionDefintion.propertyDefinitions.Add( codePageDefinition );

				// Write the locale as the third property
				PropertyDefinition localeDefinition = new PropertyDefinition();
				localeDefinition.positionOfValue = (int)writer.BaseStream.Position;
				localeDefinition.type = (int)UserDefinedPropertyType.Locale;
				this.WriteUserDefinedProperty( writer, localeDefinition );
				sectionDefintion.propertyDefinitions.Add( localeDefinition );

				// MD 1/30/08 - BR30189
				// Write out the 'standard' user defined proeprties
				foreach ( KeyValuePair<UserDefinedPropertyType, object> property in this.UserDefinedProperties )
				{
					PropertyDefinition definition = new PropertyDefinition();
					definition.positionOfValue = (int)writer.BaseStream.Position;
					definition.type = (int)property.Key;
					VariantUtilities.WriteVariant( writer, this.userSectionEncoding, property.Value );
					sectionDefintion.propertyDefinitions.Add( definition );
				}

				// Write out each user property
				for ( int propertyType = 0; propertyType < this.userDefinedNames.Count; propertyType++ )
				{
					PropertyDefinition definition = new PropertyDefinition();
					definition.positionOfValue = (int)writer.BaseStream.Position;
					definition.type = propertyType + DocumentSummaryInformation.FirstUserPropertyType;
					this.WriteUserDefinedProperty( writer, definition );
					sectionDefintion.propertyDefinitions.Add( definition );
				}
			}
			else
			{
				// Otherwise, let the base class write the section
				base.WriteSectionProperties( writer, sectionDefintion );
			}
		}

		#endregion WriteSectionProperties

		#endregion Base Class Overrides

		#region Methods

		#region ReadUserDefinedProperty

		protected void ReadUserDefinedProperty( BinaryReader reader, PropertyDefinition definition )
		{
			// Advance the stream to the position of the property
			reader.BaseStream.Position = definition.positionOfValue;

			switch ( (UserDefinedPropertyType)definition.type )
			{
				case UserDefinedPropertyType.Dictionary:
					{
						// The first value in the dictionary property is the number of user properties
						int numberOfUserProperties = reader.ReadInt32();

						// Reinitilaize both user property collections
						this.userDefinedNames = new List<string>( numberOfUserProperties );

						// MD 4/6/12 - TFS101506
						//this.userDefinedPropertiesRaw = new Dictionary<string, object>( numberOfUserProperties, StringComparer.CurrentCultureIgnoreCase );
						this.userDefinedPropertiesRaw = new Dictionary<string, object>(numberOfUserProperties, StringComparer.InvariantCultureIgnoreCase);

						// Populate the user defined names will all nulls for now
						for ( int i = 0; i < this.userDefinedNames.Capacity; i++ )
							this.userDefinedNames.Add( null );

						// Read each property name in the dictionary
						for ( int i = 0; i < numberOfUserProperties; i++ )
						{
							// The first value in the name definition is the index of the user property
							int propertyIndex = reader.ReadInt32();

							// The next value in the name definition is the actual name, read it and store 
							// it in the names collection
							this.userDefinedNames[ propertyIndex - DocumentSummaryInformation.FirstUserPropertyType ] =
								(string)VariantUtilities.ReadVariantType( reader, VariantType.LPSTR, this.userSectionEncoding );
						}

						break;
					}

				// MD 1/30/08 - BR30189
				// There are a few property types we weren't handling
				case UserDefinedPropertyType.SelfDefStructure:
				case UserDefinedPropertyType.StructureInstance:
					this.UserDefinedProperties.Add( 
						(UserDefinedPropertyType)definition.type, 
						VariantUtilities.ReadVariant( reader, this.userSectionEncoding ) );
					break;

				case UserDefinedPropertyType.Locale:
					{


#region Infragistics Source Cleanup (Region)






#endregion // Infragistics Source Cleanup (Region)

						// MD 9/5/07 - BR26240
						// We should always read this property, even if we don't use it, so the stream is in the right location
						VariantUtilities.ReadVariant( reader, this.userSectionEncoding );

						break;
					}

				default:
					{
						// Reader the value in the property, and store it with the property name which 
						// corresponds to its property index
						this.userDefinedPropertiesRaw.Add(
							this.userDefinedNames[ definition.type - DocumentSummaryInformation.FirstUserPropertyType ],
							VariantUtilities.ReadVariant( reader, this.userSectionEncoding ) );

						break;
					}
			}
		}

		#endregion ReadUserDefinedProperty

		#region WriteUserDefinedProperty

		protected void WriteUserDefinedProperty( BinaryWriter writer, PropertyDefinition definition )
		{
			if ( definition.type == (int)UserDefinedPropertyType.Locale )
			{
				// Write the locale id of the current culture
				VariantUtilities.WriteVariant( writer, this.userSectionEncoding, (uint)Utilities.CurrentCultureInfoLCID, true );
			}
			else if ( definition.type == (int)UserDefinedPropertyType.Dictionary )
			{
				// For the dictionayr properyt type, the first value is the count of property names
				writer.Write( this.userDefinedNames.Count );

				// Write out each property name
				for ( int i = 0; i < this.userDefinedNames.Count; i++ )
				{
					// The first value in each property name is the property index it corresponds to
					writer.Write( i + DocumentSummaryInformation.FirstUserPropertyType );

					// Write the actual property name, without the variant type header
					VariantUtilities.WriteVariantType( writer, VariantType.LPSTR, this.userSectionEncoding, this.userDefinedNames[ i ], false );
				}
			}
			else
			{
				// For all other user defined properties, write out the property value
				object value = this.userDefinedPropertiesRaw[ this.userDefinedNames[ definition.type - DocumentSummaryInformation.FirstUserPropertyType ] ];
				VariantUtilities.WriteVariant( writer, this.userSectionEncoding, value );
			}
		}

		#endregion WriteUserDefinedProperty

		#endregion Methods

		#region Properties

		#region Properties

		public DocumentSummaryInformationProperties Properties
		{
			get
			{
				if ( this.properties == null )
					this.properties = new DocumentSummaryInformationProperties( this );

				return this.properties;
			}
		}

		#endregion Properties

		// MD 1/30/08 - BR30189
		#region UserDefinedProperties

		private Dictionary<UserDefinedPropertyType, object> UserDefinedProperties
		{
			get { return this.userDefinedProperties; }
		}

		#endregion UserDefinedProperties

		//#region UserDefinedProperties

		//public DocumentSummaryInformationUserDefinedProperties UserDefinedProperties
		//{
		//    get
		//    {
		//        if ( this.userDefinedProperties == null )
		//            this.userDefinedProperties = new DocumentSummaryInformationUserDefinedProperties( this );

		//        return this.userDefinedProperties;
		//    }
		//}

		//#endregion UserDefinedProperties

		#endregion Properties


		#region DocumentSummaryInformationProperties class

		public class DocumentSummaryInformationProperties
		{
			private DocumentSummaryInformation documentSummaryInformation;

			#region Constructor

			public DocumentSummaryInformationProperties( DocumentSummaryInformation summaryInformation )
			{
				this.documentSummaryInformation = summaryInformation;
			}

			#endregion Constructor

			#region VerifyPropertyType

			private static bool VerifyPropertyType( DocumentSummaryPropertyType propertyType, Type valueType )
			{
				// If this is the Code Page property, never allow it to be set
				if ( 1 == (int)propertyType )
					return false;

				switch ( propertyType )
				{
					case DocumentSummaryPropertyType.Category:
					case DocumentSummaryPropertyType.PresentationTarget:
					case DocumentSummaryPropertyType.Manager:
					case DocumentSummaryPropertyType.Company:
						return valueType == typeof( string );

					case DocumentSummaryPropertyType.Bytes:
					case DocumentSummaryPropertyType.Lines:
					case DocumentSummaryPropertyType.Paragraphs:
					case DocumentSummaryPropertyType.Slides:
					case DocumentSummaryPropertyType.Notes:
					case DocumentSummaryPropertyType.HiddenSlides:
					case DocumentSummaryPropertyType.MMClips:
						return valueType == typeof( int );

					case DocumentSummaryPropertyType.ScaleCrop:
					case DocumentSummaryPropertyType.LinksUpToData:
						return valueType == typeof( bool );

					case DocumentSummaryPropertyType.HeadingPairs:
						return valueType == typeof( object[] );

					case DocumentSummaryPropertyType.TitlesOfParts:
						return valueType == typeof( string[] );

					default:
						return true;
				}
			}

			// MD 1/30/08 - BR30189
			private static bool VerifyPropertyType( UserDefinedPropertyType propertyType, Type valueType )
			{
				// If this is the Code Page property, never allow it to be set
				if ( 1 == (int)propertyType )
					return false;

				switch ( propertyType )
				{
					case UserDefinedPropertyType.SelfDefStructure:
						return valueType == typeof( string );

					case UserDefinedPropertyType.StructureInstance:
						Utilities.DebugFail( "Not sure what the expected type of the StructureInstance is." );
						return true;

					default:
						return true;
				}
			}

			#endregion VerifyPropertyType

			#region Indexer [ DocumentSummaryPropertyType ]

			public object this[ DocumentSummaryPropertyType type ]
			{
				get 
				{
					int key = (int)type;

					if ( this.documentSummaryInformation.DefaultProperties.ContainsKey( key ) == false )
						return null;

					return this.documentSummaryInformation.DefaultProperties[ key ]; 
				}
				set
				{
					int key = (int)type;

					Array array = value as Array;

					// If the value is nul or DBNull, remove the property from the info class
					if ( value == null || 
						value == DBNull.Value ||
						( array != null && array.Length == 0 ) )
					{
						if ( this.documentSummaryInformation.DefaultProperties.ContainsKey( key ) )
							this.documentSummaryInformation.DefaultProperties.Remove( key );
					}
					else
					{
						if ( VerifyPropertyType( type, value.GetType() ) == false )
						{
							Utilities.DebugFail( "Invaid type of value for the property." );
							return;
						}

						// MD 5/17/07 - BR23013
						// The array could be null
						//if ( array.Rank != 1 )
						if ( array != null && array.Rank != 1 )
						{
							Utilities.DebugFail( "The specified array has too many dimensions." );
							return;
						}

						// Set the value of the property
						if ( this.documentSummaryInformation.DefaultProperties.ContainsKey( key ) )
							this.documentSummaryInformation.DefaultProperties[ key ] = value;
						else
							this.documentSummaryInformation.DefaultProperties.Add( key, value );
					}
				}
			}

			#endregion Indexer [ DocumentSummaryPropertyType ]

			// MD 1/30/08 - BR30189
			#region Indexer [ UserDefinedPropertyType ]

			public object this[ UserDefinedPropertyType type ]
			{
				get
				{
					if ( this.documentSummaryInformation.UserDefinedProperties.ContainsKey( type ) == false )
						return null;

					return this.documentSummaryInformation.UserDefinedProperties[ type ];
				}
				set
				{
					Array array = value as Array;

					// If the value is nul or DBNull, remove the property from the info class
					if ( value == null ||
						value == DBNull.Value ||
						( array != null && array.Length == 0 ) )
					{
						if ( this.documentSummaryInformation.UserDefinedProperties.ContainsKey( type ) )
							this.documentSummaryInformation.UserDefinedProperties.Remove( type );
					}
					else
					{
						if ( VerifyPropertyType( type, value.GetType() ) == false )
						{
							Utilities.DebugFail( "Invaid type of value for the property." );
							return;
						}

						// MD 5/17/07 - BR23013
						// The array could be null
						//if ( array.Rank != 1 )
						if ( array != null && array.Rank != 1 )
						{
							Utilities.DebugFail( "The specified array has too many dimensions." );
							return;
						}

						// Set the value of the property
						this.documentSummaryInformation.UserDefinedProperties[ type ] = value;
					}
				}
			}

			#endregion Indexer [ UserDefinedPropertyType ]
		}

		#endregion DocumentSummaryInformationProperties class

		//#region DocumentSummaryInformationUserDefinedProperties class

		//public class DocumentSummaryInformationUserDefinedProperties
		//{
		//    private DocumentSummaryInformation documentSummaryInformation;

		//    #region Constructor

		//    public DocumentSummaryInformationUserDefinedProperties( DocumentSummaryInformation summaryInformation )
		//    {
		//        this.documentSummaryInformation = summaryInformation;
		//    }

		//    #endregion Constructor

		//    #region VerifyPropertyType

		//    private static bool VerifyPropertyType( Type valueType )
		//    {
		//        return
		//            valueType == typeof( bool ) ||
		//            valueType == typeof( decimal ) ||
		//            valueType == typeof( DateTime ) ||
		//            valueType == typeof( sbyte ) ||
		//            valueType == typeof( short ) ||
		//            valueType == typeof( int ) ||
		//            valueType == typeof( long ) ||
		//            valueType == typeof( float ) ||
		//            valueType == typeof( double ) ||
		//            valueType == typeof( byte ) ||
		//            valueType == typeof( ushort ) ||
		//            valueType == typeof( uint ) ||
		//            valueType == typeof( ulong ) ||
		//            valueType == typeof( string );
		//    }

		//    #endregion VerifyPropertyType

		//    #region Indexer [ string ]

		//    public object this[ string propertyName ]
		//    {
		//        get 
		//        {
		//            if ( this.documentSummaryInformation.userDefinedPropertiesRaw.ContainsKey( propertyName ) == false )
		//                return null;

		//            return this.documentSummaryInformation.userDefinedPropertiesRaw[ propertyName ]; 
		//        }
		//        set
		//        {
		//            if ( propertyName == null )
		//            {
		//                Utilities.DebugFail( "The property name cannot be null." );
		//                return;
		//            }

		//            // If the value is nul or DBNull, remove the property from the info class
		//            if ( value == null || value == DBNull.Value )
		//            {
		//                if ( this.documentSummaryInformation.userDefinedPropertiesRaw.ContainsKey( propertyName ) )
		//                {
		//                    int index = this.documentSummaryInformation.userDefinedNames.FindIndex(
		//                        delegate( string obj )
		//                        {
		//                            return StringComparer.CurrentCultureIgnoreCase.Compare( propertyName, obj ) == 0;
		//                        } );

		//                    Debug.Assert( index >= 0 );
		//                    if ( index >= 0 )
		//                        this.documentSummaryInformation.userDefinedNames.RemoveAt( index );

		//                    this.documentSummaryInformation.userDefinedPropertiesRaw.Remove( propertyName );
		//                }
		//            }
		//            else
		//            {
		//                if ( VerifyPropertyType( value.GetType() ) == false )
		//                {
		//                    Utilities.DebugFail( "Invaid type of value for the property." );
		//                    return;
		//                }

		//                // Set the value of the property
		//                if ( this.documentSummaryInformation.userDefinedPropertiesRaw.ContainsKey( propertyName ) )
		//                {
		//                    this.documentSummaryInformation.userDefinedPropertiesRaw[ propertyName ] = value;
		//                }
		//                else
		//                {
		//                    this.documentSummaryInformation.userDefinedNames.Add( propertyName );
		//                    this.documentSummaryInformation.userDefinedPropertiesRaw.Add( propertyName, value );
		//                }
		//            }
		//        }
		//    }

		//    #endregion Indexer [ string ]
		//}

		//#endregion DocumentSummaryInformationUserDefinedProperties class
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