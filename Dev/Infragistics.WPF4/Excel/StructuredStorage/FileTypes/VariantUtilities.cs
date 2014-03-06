using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;




namespace Infragistics.Documents.Excel.StructuredStorage.FileTypes

{
	internal static class VariantUtilities
	{
		#region GetVariantType

		internal static VariantType GetVariantType( object variant )
		{
			Array array = variant as Array;

			if ( array != null )
			{
				VariantType subType = VariantUtilities.GetVariantType( array.GetValue( 0 ) );

				for ( int i = 1; i < array.Length; i++ )
				{
					VariantType newSubType = VariantUtilities.GetVariantType( array.GetValue( i ) );

					if ( newSubType != subType )
					{
						subType = VariantType.Variant;
						break;
					}
				}

				return VariantType.Vector | subType;
			}
			else if ( variant is bool )
				return VariantType.Bool;
			else if ( variant is decimal )
				return VariantType.Decimal;
			else if ( variant is DateTime )
				return VariantType.FileTime;
			else if ( variant is sbyte )
				return VariantType.I1;
			else if ( variant is short )
				return VariantType.I2;
			else if ( variant is int )
				return VariantType.I4;
			else if ( variant is long )
				return VariantType.I8;
			else if ( variant is float )
				return VariantType.R4;
			else if ( variant is double )
				return VariantType.R8;
			else if ( variant is byte )
				return VariantType.UI1;
			else if ( variant is ushort )
				return VariantType.UI2;
			else if ( variant is uint )
				return VariantType.UI4;
			else if ( variant is ulong )
				return VariantType.UI8;
			else if ( variant is string )
				return VariantType.LPSTR;
			else if ( variant == null )
				return VariantType.Null;

			Utilities.DebugFail( "Unknown object type" );
			return VariantType.Unknown;
		}

		#endregion GetVariantType

		// MD 1/30/08 - BR30189
		// Moved out helper method from ReadVariantType
		#region ReadString

		// MD 6/9/09 - TFS18302
		// Added a parameter to indicate whether the variant type uses single or double bytes for characters.
		//private static string ReadString( BinaryReader reader, Encoding encoding )
		private static string ReadString( BinaryReader reader, Encoding encoding, bool isSingleByte )
		{
			int length = reader.ReadInt32();

			// MD 2/12/09 - TFS13909
			// For some reason, when they encoding is double byte, we need to have an even length string and we need two bytes per character, 
			// so then double the length.
			// MD 6/9/09 - TFS18302
			// This check is not based on the encoding, but rather on the variant type being read in, so based it on the isSingleByte
			// parameter, which is set based on the variant type.
			//if ( encoding.IsSingleByte == false )
			if ( isSingleByte == false )
			{
				length += ( length % 2 );
				length *= 2;
			}

			byte[] data = new byte[ length ];

			int validLength;
			for ( validLength = 0; validLength < length; validLength++ )
			{
				byte next = reader.ReadByte();

				if ( next == 0 )
					break;

				data[ validLength ] = next;

				// MD 2/12/09 - TFS13909
				// Is this is a double byte encoding, read the next byte as well so we don't think this is the end of the string 
				// when we get a zero byte above.
				// MD 6/9/09 - TFS18302
				// This check is not based on the encoding, but rather on the variant type being read in, so based it on the isSingleByte
				// parameter, which is set based on the variant type.
				//if ( encoding.IsSingleByte == false )
				if ( isSingleByte == false )
				{
					validLength++;
					data[ validLength ] = reader.ReadByte();
				}
			}

			Debug.Assert( validLength < length, "There was no trailing zero." );

			for ( int i = validLength + 1; i < length; i++ )
			{
				byte value = reader.ReadByte();
				Debug.Assert( value == 0 );
			}

			return encoding.GetString( data, 0, validLength );
		}

		#endregion ReadString

		#region ReadVariant

		internal static object ReadVariant( BinaryReader reader, Encoding encoding )
		{
			VariantType type = (VariantType)reader.ReadInt32();
			return VariantUtilities.ReadVariantType( reader, type, encoding );
		}

		#endregion ReadVariant

		#region ReadVariantType

		internal static object ReadVariantType( BinaryReader reader, VariantType type, Encoding encoding )
		{
			if ( ( type & VariantType.Vector ) == VariantType.Vector )
			{
				VariantType subType = type & ~VariantType.Vector;

				int elementCount = reader.ReadInt32();
				object[] values = new object[ elementCount ];

				for ( int i = 0; i < elementCount; i++ )
					values[ i ] = VariantUtilities.ReadVariantType( reader, subType, encoding );

				return values;
			}

			
			switch ( type )
			{
				case VariantType.Array:
				case VariantType.BLOBObject:
				case VariantType.Bstr:
				case VariantType.ByRef:
				case VariantType.CARRAY:
				case VariantType.CF:
				case VariantType.CY:
				case VariantType.Date:
				case VariantType.Dispatch:
				case VariantType.Error:
				case VariantType.Hresult:
				case VariantType.PTR:
				case VariantType.Reserved:
				case VariantType.SafeArray:
				case VariantType.Storage:
				case VariantType.Stream:
				case VariantType.StreamedObject:
				case VariantType.Unknown:
				case VariantType.UserDefined:
				case VariantType.Vector:
				case VariantType.VOID:
					Utilities.DebugFail( "Don't know how to parse this variant type: " + type );
					break;

				// MD 10/30/11 - TFS90733
				// Added support for loading BLOB variants.
				case VariantType.BLOB:
					int cbData = reader.ReadInt32();
					Debug.Assert(cbData <= reader.BaseStream.Length - reader.BaseStream.Position, "The cbData field is longer than the amount of space left in the stream.");
					return reader.ReadBytes(cbData);

				case VariantType.Bool:
					return reader.ReadBoolean();

				// MD 2/6/08
				// Found while fixing BR30292
				// We can safely read the CLSID variant type.
				case VariantType.CLSID:
					return new Guid( reader.ReadBytes( 16 ) );

				case VariantType.Decimal:
					return reader.ReadDecimal();

				case VariantType.Empty:
					return null;

				case VariantType.FileTime:
					return DateTime.FromFileTime( reader.ReadInt64() );

				case VariantType.I1:
					return reader.ReadSByte();

				case VariantType.I2:
					return reader.ReadInt16();

				case VariantType.I4:
				case VariantType.INT: 
					return reader.ReadInt32();

				case VariantType.I8:
					return reader.ReadInt64();

				case VariantType.LPSTR:
					// MD 1/30/08 - BR30189
					// Moved to helper method ReadString
					//{
					//    int length = reader.ReadInt32();
					//
					//    byte[] data = new byte[ length ];
					//
					//    int validLength;
					//    for ( validLength = 0; validLength < length; validLength++ )
					//    {
					//        byte next = reader.ReadByte();
					//
					//        if ( next == 0 )
					//            break;
					//
					//        data[ validLength ] = next;
					//    }
					//
					//    Debug.Assert( validLength < length, "There was no trailing zero." );
					//
					//    for ( int i = validLength + 1; i < length; i++ )
					//    {
					//        byte value = reader.ReadByte();
					//        Debug.Assert( value == 0 );
					//    }
					//
					//    return encoding.GetString( data, 0, validLength ); ;
					//}
					// MD 6/9/09 - TFS18302
					// Added a parameter to indicate whether the variant type uses single or double bytes for characters.
					//return VariantUtilities.ReadString( reader, encoding );
					// MD 6/8/10 - TFS33952
					// Even if this is a LPSTR, but a unicode encoding is being used, read in the string as a wide string.
					//return VariantUtilities.ReadString( reader, encoding, true );
					return VariantUtilities.ReadString(reader, encoding, encoding.Equals(Encoding.Unicode) == false);

				// MD 1/30/08 - BR30189
				case VariantType.LPWSTR:
					// MD 6/9/09 - TFS18302
					// Added a parameter to indicate whether the variant type uses single or double bytes for characters.
					//return VariantUtilities.ReadString( reader, Encoding.Unicode );
					return VariantUtilities.ReadString( reader, Encoding.Unicode, false );

				case VariantType.Null:
					return null;

				case VariantType.R4:
					return reader.ReadSingle();

				case VariantType.R8:
					return reader.ReadDouble();

				case VariantType.UI1:
					return reader.ReadByte();

				case VariantType.UI2:
					return reader.ReadUInt16();

				case VariantType.UI4:
				case VariantType.UINT: 
					return reader.ReadUInt32();

				case VariantType.UI8:
					return reader.ReadUInt64();

				case VariantType.Variant:
					return VariantUtilities.ReadVariant( reader, encoding );

				default:
					Utilities.DebugFail( "Unknown property type" );
					break;
			}

			return null;
		}

		#endregion ReadVariantType

		// MD 1/30/08 - BR30189
		// Moved out helper method from WriteVariantType
		#region WriteString

		private static void WriteString( BinaryWriter writer, Encoding encoding, string value, bool alignStringLengthBy4Bytes )
		{
			byte[] data = encoding.GetBytes( value );

			int length = data.Length + 1;

			if ( alignStringLengthBy4Bytes )
				length = Utilities.RoundUpToMultiple( length, 4 );

			writer.Write( length );

			writer.Write( data );
			writer.Write( (byte)0 );
		}

		#endregion WriteString

		#region WriteVariant

		internal static void WriteVariant( BinaryWriter writer, Encoding encoding, object value )
		{
			VariantUtilities.WriteVariant( writer, encoding, value, true );
		}

		internal static void WriteVariant( BinaryWriter writer, Encoding encoding, object value, bool alignBy4Bytes )
		{
			VariantType type = VariantUtilities.GetVariantType( value );

			writer.Write( (int)type );
			VariantUtilities.WriteVariantType( writer, type, encoding, value, alignBy4Bytes );

			if ( alignBy4Bytes && ( type & VariantType.Vector ) == 0 )
			{
				while ( writer.BaseStream.Position % 4 != 0 )
					writer.Write( (byte)0 );
			}
		}

		#endregion WriteVariant

		#region WriteVariantType

		internal static void WriteVariantType( BinaryWriter writer, VariantType type, Encoding encoding, object value, bool alignStringLengthBy4Bytes )
		{
			if ( ( type & VariantType.Vector ) == VariantType.Vector )
			{
				VariantType subType = type & ~VariantType.Vector;

				Array array = (Array)value;

				int elementCount = array.Length;
				writer.Write( elementCount );

				for ( int i = 0; i < elementCount; i++ )
					VariantUtilities.WriteVariantType( writer, subType, encoding, array.GetValue( i ), false );

				return;
			}

			switch ( type )
			{
				case VariantType.Array:
				case VariantType.BLOB:
				case VariantType.BLOBObject:
				case VariantType.Bstr:
				case VariantType.ByRef:
				case VariantType.CARRAY:
				case VariantType.CF:
				case VariantType.CLSID:
				case VariantType.CY:
				case VariantType.Date:
				case VariantType.Dispatch:
				case VariantType.Error:
				case VariantType.Hresult:
				case VariantType.PTR:
				case VariantType.Reserved:
				case VariantType.SafeArray:
				case VariantType.Storage:
				case VariantType.Stream:
				case VariantType.StreamedObject:
				case VariantType.Unknown:
				case VariantType.UserDefined:
				case VariantType.Vector:
				case VariantType.VOID:
					Utilities.DebugFail( "Don't know how to parse this variant type: " + type );
					break;

				case VariantType.Bool:
					writer.Write( (bool)value );
					break;

				case VariantType.Decimal:
					writer.Write( (decimal)value );
					break;


				case VariantType.Empty:
					writer.Write( (int)0 ); 
					break;

				case VariantType.FileTime:
					writer.Write( ( (DateTime)value ).ToFileTime() );
					break;

				case VariantType.I1:
					writer.Write( (sbyte)value );
					break;

				case VariantType.I2:
					writer.Write( (short)value );
					break;

				case VariantType.I4:
				case VariantType.INT: 
					writer.Write( (int)value );
					break;

				case VariantType.I8:
					writer.Write( (long)value );
					break;

				case VariantType.LPSTR:
					// MD 1/30/08 - BR30189
					// Moved to helper method WriteString
					//{
					//    byte[] data = encoding.GetBytes( (string)value );
					//
					//    int length = data.Length + 1;
					//
					//    if ( alignStringLengthBy4Bytes )
					//        length = Utilities.RoundUpToMultiple( length, 4 );
					//
					//    writer.Write( length );
					//
					//    writer.Write( data );
					//    writer.Write( (byte)0 );
					//
					//    break;
					//}
					VariantUtilities.WriteString( writer, encoding, (string)value, alignStringLengthBy4Bytes );
					break;

				// MD 1/30/08 - BR30189
				case VariantType.LPWSTR:
					VariantUtilities.WriteString( writer, Encoding.Unicode, (string)value, alignStringLengthBy4Bytes );
					break;

				case VariantType.Null:
					writer.Write( (int)0 ); 
					break;

				case VariantType.R4:
					writer.Write( (float)value );
					break;

				case VariantType.R8:
					writer.Write( (double)value );
					break;

				case VariantType.UI1:
					writer.Write( (byte)value );
					break;

				case VariantType.UI2:
					writer.Write( (ushort)value );
					break;

				case VariantType.UI4:
				case VariantType.UINT: 
					writer.Write( (uint)value );
					break;

				case VariantType.UI8:
					writer.Write( (ulong)value );
					break;

				case VariantType.Variant:
					VariantUtilities.WriteVariant( writer, encoding, value, alignStringLengthBy4Bytes );
					break;

				default:
					Utilities.DebugFail( "Unknown property type" );
					break;
			}
		}

		#endregion WriteVariantType
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