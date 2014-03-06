using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;




namespace Infragistics.Documents.Excel.StructuredStorage.FileTypes

{
	internal abstract class InformationBase
	{
		#region Constants

		protected const int CodePagePropertyType = 1;
		private const ushort HeaderValue = 0xFFFE;

		#endregion Constants

		#region Member Variables

		private Dictionary<int, object> defaultProperties;
        private Encoding encoding = Utilities.EncodingDefault;
		private Guid id;

		#endregion Member Variables

		#region Constructor

		public InformationBase()
		{
			this.defaultProperties = new Dictionary<int, object>();
		}

		#endregion Constructor

		#region Methods

		#region GetRequiredEncoding

		protected static Encoding GetRequiredEncoding( List<string> stringValues )
		{
            Encoding encoding = Utilities.EncodingDefault;

			for ( int i = 0; i < stringValues.Count; i++ )
			{
				string currentString = stringValues[ i ];

				if ( currentString != encoding.GetString( encoding.GetBytes( currentString ) ) )
				{
					Debug.Assert( currentString == Encoding.UTF8.GetString( Encoding.UTF8.GetBytes( currentString ) ) );
					return Encoding.UTF8;
				}
			}

			return encoding;
		}

		#endregion GetRequiredEncoding

		#region Load

		public void Load( Stream stream )
		{
			BinaryReader reader = new BinaryReader( stream );

			// Read the stream header
			ushort headerValue = reader.ReadUInt16();

			if ( headerValue != InformationBase.HeaderValue )
			{
				Utilities.DebugFail( "Invalid header value" );
				stream.Position = 0;
				return;
			}

			ushort shouldBeZeros = reader.ReadUInt16();
			Debug.Assert( shouldBeZeros == 0 );

			reader.ReadInt32(); // OS Version
			this.id = new Guid( reader.ReadBytes( 16 ) );

			List<PropertySectionDefinition> sectionDefinitions = new List<PropertySectionDefinition>();

			// Read the section list header
			int numberOfSections = reader.ReadInt32();
			for ( int i = 0; i < numberOfSections; i++ )
			{
				PropertySectionDefinition sectionDefintion = new PropertySectionDefinition();
				sectionDefintion.sectionType = new Guid( reader.ReadBytes( 16 ) );
				sectionDefintion.positionOfSection = reader.ReadInt32();
				sectionDefinitions.Add( sectionDefintion );
			}

			// Read each section in the stream
			for ( int i = 0; i < numberOfSections; i++ )
				this.ReadSection( reader, sectionDefinitions[ i ] );
		}

		#endregion Load

		#region PopuplateSectionDefinitions

		protected virtual void PopuplateSectionDefinitions( List<PropertySectionDefinition> sectionDefinitions )
		{
			// Always add the default section to the section definitions
			PropertySectionDefinition definition = new PropertySectionDefinition();
			definition.sectionType = new Guid( this.DefaultSectionId );
			sectionDefinitions.Add( definition );
		}

		#endregion PopuplateSectionDefinitions

		#region ReadProperty

		protected object ReadProperty( BinaryReader reader, PropertyDefinition definition )
		{
			// Advance the stream to the position of the property
			reader.BaseStream.Position = definition.positionOfValue;

			// Read the variant at the current location in the stream
			return VariantUtilities.ReadVariant( reader, this.encoding );
		}

		#endregion ReadProperty

		#region ReadSection

		protected void ReadSection( BinaryReader reader, PropertySectionDefinition sectionDefintion )
		{
			// Advance the stream to the position of the section
			reader.BaseStream.Position = sectionDefintion.positionOfSection;

			// Read the section length and the number of properties in the section
			sectionDefintion.sectionLength = reader.ReadInt32();
			sectionDefintion.propertyCount = reader.ReadInt32();

			// Initialize the property definitions collection for the section
			sectionDefintion.propertyDefinitions = new List<PropertyDefinition>( sectionDefintion.propertyCount );

			// Read the property list header
			for ( int j = 0; j < sectionDefintion.propertyCount; j++ )
			{
				PropertyDefinition definition = new PropertyDefinition();

				// Read the type of the property which follows later
				definition.type = reader.ReadInt32();
				// Read the offset of the property and store the absolute position of the property
				definition.positionOfValue = sectionDefintion.positionOfSection + reader.ReadInt32();

				sectionDefintion.propertyDefinitions.Add( definition );
			}

			// Read each property in the section
			this.ReadSectionProperties( reader, sectionDefintion );
		}

		#endregion ReadSection

		#region ReadSectionCodePage

		protected Encoding ReadSectionCodePage( BinaryReader reader, PropertySectionDefinition sectionDefintion )
		{
			// Loop through all the properties first to look determine the code page
			for ( int i = 0; i < sectionDefintion.propertyCount; i++ )
			{
				PropertyDefinition definition = sectionDefintion.propertyDefinitions[ i ];

				if ( definition.type == InformationBase.CodePagePropertyType )
                    return Utilities.EncodingGetEncoding((int)(ushort)(short)this.ReadProperty(reader, definition));
			}

            return Utilities.EncodingDefault;
		}

		#endregion ReadSectionCodePage

		#region ReadSectionProperties

		protected virtual void ReadSectionProperties( BinaryReader reader, PropertySectionDefinition sectionDefintion )
		{
			string sectionId = sectionDefintion.sectionType.ToString( "N" );

			if ( sectionId == this.DefaultSectionId )
			{
				// Create or re-create the collection of properties
				this.defaultProperties = new Dictionary<int, object>();

				// Read the section's code page so we know how to parse string properties to follow
				this.encoding = this.ReadSectionCodePage( reader, sectionDefintion );

				// Read the rest of the properties in the section
				for ( int i = 0; i < sectionDefintion.propertyCount; i++ )
				{
					PropertyDefinition definition = sectionDefintion.propertyDefinitions[ i ];

					// MD 3/29/10 - TFS30087
					// If we won't be able to read the property value, we have to skip this property.
					if (reader.BaseStream.Length < (definition.positionOfValue + 4))
					{
						Utilities.DebugFail("Cannot read the current property's variant type. Something is wrong with the information file.");
						continue;
					}

					// Read all properties but the code page
					if ( definition.type != InformationBase.CodePagePropertyType )
						this.defaultProperties.Add( definition.type, this.ReadProperty( reader, definition ) );
				}
			}
			else
			{
				Utilities.DebugFail( "Unknown section type: " + sectionId );
			}
		}

		#endregion ReadSectionProperties

		#region Save

		public void Save( Stream stream )
		{
			// Verify the encoding used to store strings
			this.VeridyEncoding();

			BinaryWriter writer = new BinaryWriter( stream );

			// Write the stream header
			writer.Write( InformationBase.HeaderValue );
			writer.Write( (ushort)0 );

			// MD 6/21/11 - TFS78796
			// When using Windows XP, this causes the workbook to open in protected mode. 
			// Zero doesn't seem to cause any problems, so use that instead.
			//int osVersion =
			//    Environment.OSVersion.Version.Revision |
			//    ( Environment.OSVersion.Version.Minor << 8 ) | 
			//    Environment.OSVersion.Version.Major;
			int osVersion = 0;

			writer.Write( osVersion );
			writer.Write( this.id.ToByteArray() );

			// Determine the sections which need to be written
			List<PropertySectionDefinition> sectionDefinitions = new List<PropertySectionDefinition>();
			this.PopuplateSectionDefinitions( sectionDefinitions );

			// Move the stream past the section list header, it will be 
			// written after each section is written
			stream.Position += 4 + ( 20 * sectionDefinitions.Count );

			// Write out each section
			for ( int i = 0; i < sectionDefinitions.Count; i++ )
			{
				PropertySectionDefinition sectionDefintion = sectionDefinitions[ i ];
				sectionDefintion.positionOfSection = (int)stream.Position;

				this.WriteSection( writer, sectionDefintion );
			}

			// Move the stream back to the section list header
			stream.Position = 0x18;

			// Write the section list header
			writer.Write( sectionDefinitions.Count );
			for ( int i = 0; i < sectionDefinitions.Count; i++ )
			{
				PropertySectionDefinition sectionDefintion = sectionDefinitions[ i ];

				writer.Write( sectionDefintion.sectionType.ToByteArray() );
				writer.Write( sectionDefintion.positionOfSection );
			}

			stream.Position = stream.Length;
		}

		#endregion Save

		#region VeridyEncoding

		protected virtual void VeridyEncoding()
		{
			List<string> stringValues = new List<string>();

			foreach ( object value in this.defaultProperties.Values )
			{
				string stringValue = value as string;

				if ( stringValue != null )
				{
					stringValues.Add( stringValue );
					continue;
				}

				Array array = value as Array;

				if ( array != null )
				{
					for ( int i = 0; i < array.Length; i++ )
					{
						stringValue = array.GetValue( i ) as string;

						if ( stringValue != null )
							stringValues.Add( stringValue );
					}
				}
			}

			this.encoding = InformationBase.GetRequiredEncoding( stringValues );
		}

		#endregion VeridyEncoding

		#region WriteSection

		protected void WriteSection( BinaryWriter writer, PropertySectionDefinition sectionDefintion )
		{
			// Move past the section length and number of properties values, they will be written later
			writer.BaseStream.Position += 8;

			// Write out all section properties
			this.WriteSectionProperties( writer, sectionDefintion );

			// Make sure the section is aligned on a 4 byte boundary
			while ( writer.BaseStream.Position % 4 != 0 )
				writer.Write( (byte)0 );

			// Determine the length of the section
			sectionDefintion.sectionLength = (int)( writer.BaseStream.Position - sectionDefintion.positionOfSection );

			// Move back to the beginning of the section
			writer.BaseStream.Position = sectionDefintion.positionOfSection;

			// Write the section length and number of properties
			writer.Write( sectionDefintion.sectionLength );
			writer.Write( sectionDefintion.propertyDefinitions.Count );

			// Write the property list header
			for ( int i = 0; i < sectionDefintion.propertyDefinitions.Count; i++ )
			{
				PropertyDefinition defintion = sectionDefintion.propertyDefinitions[ i ];

				// Write the property type
				writer.Write( defintion.type );
				// Write out the offset of the property from the section position, not the actual property position
				writer.Write( defintion.positionOfValue - sectionDefintion.positionOfSection );
			}

			// Move the stream to the end of the section
			writer.BaseStream.Position = sectionDefintion.positionOfSection + sectionDefintion.sectionLength;
		}

		#endregion WriteSection

		#region WriteSectionProperties

		protected virtual void WriteSectionProperties( BinaryWriter writer, PropertySectionDefinition sectionDefintion )
		{
			string sectionId = sectionDefintion.sectionType.ToString( "N" );

			// If the section is the default section, write it out
			if ( sectionId == this.DefaultSectionId )
			{
				// The number of properties in the section is the number of properties plus 1: the code page
				sectionDefintion.propertyCount = this.defaultProperties.Count + 1;
				sectionDefintion.propertyDefinitions = new List<PropertyDefinition>();

				// Advance the stream past the property header list, which contains an int for the type and an
				// int for the stream offset of each property in the section (the offsets are from the start of the section)
				writer.BaseStream.Position += sectionDefintion.propertyCount * 8;

				// Write the code page as the first property
				PropertyDefinition codePageDefinition = new PropertyDefinition();
				codePageDefinition.positionOfValue = (int)writer.BaseStream.Position;
				codePageDefinition.type = InformationBase.CodePagePropertyType;
				VariantUtilities.WriteVariant( writer, this.encoding, (short)Utilities.EncodingGetCodePage(this.encoding) );
				sectionDefintion.propertyDefinitions.Add( codePageDefinition );

				// Write out each property
				foreach ( KeyValuePair<int, object> property in this.defaultProperties )
				{
					PropertyDefinition definition = new PropertyDefinition();
					definition.positionOfValue = (int)writer.BaseStream.Position;
					definition.type = property.Key;
					VariantUtilities.WriteVariant( writer, this.encoding, property.Value );
					sectionDefintion.propertyDefinitions.Add( definition );
				}
			}
			else
			{
				// Otherwise, we have an erro
				Utilities.DebugFail( "Unknown section type: " + sectionId );
			}
		}

		#endregion WriteSectionProperties

		#endregion Methods

		#region Properties

		#region DefaultProperties

		protected Dictionary<int, object> DefaultProperties
		{
			get { return this.defaultProperties; }
		}

		#endregion DefaultProperties

		#region DefaultSectionId

		protected abstract string DefaultSectionId { get;}

		#endregion DefaultSectionId

		#endregion Properties


		#region PropertyDefinition class

		protected class PropertyDefinition
		{
			public int type;
			public int positionOfValue;
		}

		#endregion PropertyDefinition class

		#region PropertySectionDefinition class

		protected class PropertySectionDefinition
		{
			public Guid sectionType;
			public int sectionLength;
			public int positionOfSection;
			public int propertyCount;
			public List<PropertyDefinition> propertyDefinitions;
		}

		#endregion PropertySectionDefinition class
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