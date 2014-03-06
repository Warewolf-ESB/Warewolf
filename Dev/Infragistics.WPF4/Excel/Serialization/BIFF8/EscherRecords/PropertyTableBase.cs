using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords
{
	internal abstract class PropertyTableBase : EscherRecordBase
	{
		private List<PropertyValue> properties;

		public PropertyTableBase( List<PropertyTableBase.PropertyValue> properties )
			: this( 0x03, (ushort)properties.Count, 0 ) 
		{
			this.properties = properties;

			uint recordLength = 0;
			foreach ( PropertyTableBase.PropertyValue property in this.properties )
			{
				recordLength += 6;

				if ( property.IsComplex )
					recordLength += (uint)( (byte[])property.Value ).Length;
			}

			this.RecordLength = recordLength;
		}

		public PropertyTableBase( byte version, ushort instance, uint recordLength )
			: base( version, instance, recordLength )
		{
			Debug.Assert( version == 0x03 );
		}

		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			WorksheetShape shape = (WorksheetShape)manager.ContextStack[ typeof( WorksheetShape ) ];

			this.properties = new List<PropertyValue>();
			bool foundBlipId = false;

			for ( int i = 0; i < this.Instance; i++ )
			{
				ushort propertyHeader = manager.CurrentRecordStream.ReadUInt16();
				uint value = manager.CurrentRecordStream.ReadUInt32();

				PropertyType propertyType = (PropertyType)( propertyHeader & 0x3FFF );
				bool isComplex =	( propertyHeader & 0x8000 ) == 0x8000;
				bool isBlipId =		( propertyHeader & 0x4000 ) == 0x4000 && isComplex == false;

				PropertyValue prop = new PropertyValue( propertyType, value, isComplex, isBlipId );

				// MD 10/30/11 - TFS90733
				// This could be in the MSODRAWING(GROUP) stream of an HFPICTURE record, which means there is no shape associated.
				//if ( isBlipId )
				if (isBlipId && shape != null)
				{
					Debug.Assert( foundBlipId == false );
					foundBlipId = true;

					// MD 10/30/11 - TFS90733
					// Other shapes (such as controls) may store images.
					//WorksheetImage image = shape as WorksheetImage;
					IWorksheetImage image = shape as IWorksheetImage;

					Debug.Assert( image != null );
					if ( image != null )
					{
						// The blip ids are 1-based
						image.Image = manager.Images[ (int)value - 1 ].Image;
						Debug.Assert( image.Image != null );
					}
				}

				this.properties.Add( prop );
			}

			this.properties.Sort();

			foreach ( PropertyValue property in this.properties )
			{
				if ( property.IsComplex == false )
					continue;

				// MD 8/26/11 - TFS84024
				// It looks like Excel may have a bug when saving out IMsoArray data types. For some properties, the complex property's 
				// initial value, which is the size of the byte array, is saved out as the IMsoArray's data size, excluding the IMsoArray's
				// header size, which is 6. But other times, the complex property's initial value is correctly the header size plus the 
				// data size. So for IMsoArray types, we should read the header in first, determine the data size, and read in the full 
				// IMsoArray data or the remaining complex value's size, whichever is greater.
				//
				//property.Value = manager.CurrentRecordStream.ReadBytes( (int)(uint)property.Value );
				int complexValueSize = (int)(uint)property.Value;

				if (complexValueSize == 0)
				{
					property.Value = new byte[0];
					continue;
				}

				if (property.IsIMsoArray)
				{
					const int HeaderSize = 6;

					if (complexValueSize < HeaderSize)
					{
						Utilities.DebugFail("There is not enough bytes for the IMsoArray header.");
						property.Value = manager.CurrentRecordStream.ReadBytes(complexValueSize);
					}
					else
					{
						byte[] header = manager.CurrentRecordStream.ReadBytes(HeaderSize);

						ushort nElems = BitConverter.ToUInt16(header, 0);
						ushort nElemsAlloc = BitConverter.ToUInt16(header, 2);
						ushort cbElem = BitConverter.ToUInt16(header, 4);

						int elementSize = cbElem == 0xFFF0 ? 4 : cbElem;

						int remainingDataSize = nElems * elementSize;
						int remainingComplexPropertySize = complexValueSize - HeaderSize;
						int dataToRead = Math.Max(remainingDataSize, remainingComplexPropertySize);
						Debug.Assert(dataToRead == remainingDataSize, "There are extra bytes in the complex property value it seems.");

						byte[] data = manager.CurrentRecordStream.ReadBytes(dataToRead);
						byte[] allData = new byte[header.Length + data.Length];

						Buffer.BlockCopy(header, 0, allData, 0, header.Length);
						Buffer.BlockCopy(data, 0, allData, header.Length, data.Length);

						property.Value = allData;
					}
				}
				else
				{
					property.Value = manager.CurrentRecordStream.ReadBytes(complexValueSize);
				}
			}
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			base.Save( manager );

			foreach ( PropertyValue property in this.properties )
			{
				uint value = 0;
				ushort propertyHeader = 0;

				propertyHeader |= (ushort)property.PropertyType;

				if ( property.IsComplex )
				{
					propertyHeader |= 0xC000;

					byte[] data = property.Value as byte[];

					Debug.Assert( data != null );

					if ( data != null )
						value = (uint)data.Length;
				}
				else
				{
					value = (uint)property.Value;
				}

				if ( property.IsBlipId )
					propertyHeader |= 0x4000;

				manager.CurrentRecordStream.Write( propertyHeader );
				manager.CurrentRecordStream.Write( value );
			}

			foreach ( PropertyValue property in this.properties )
			{
				if ( property.IsComplex == false )
					continue;

				byte[] data = property.Value as byte[];

				Debug.Assert( data != null );
				if ( data != null )
					manager.CurrentRecordStream.Write( data );
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append( "------------------------------" );
			sb.Append( "\n" );

			sb.Append( base.ToString() );
			sb.Append( "\n\n" );

			if ( this.properties != null )
			{
				for ( int i = 0; i < this.properties.Count; i++ )
				{
					sb.AppendFormat( "Property {0}: {1}\n", i, this.properties[ i ].PropertyType );

					if ( this.properties[ i ].PropertyType == PropertyType.BLIPName ||
						this.properties[ i ].PropertyType == PropertyType.GroupShapeName ||
						this.properties[ i ].PropertyType == PropertyType.GroupShapeDescription	 )
					{
						sb.AppendFormat( "\tValue: {0}\n", Encoding.Unicode.GetString( (byte[])this.properties[ i ].Value, 0, ( (byte[])this.properties[ i ].Value ).Length - 2 ) );
					}
					else if ( this.properties[ i ].Value is byte[] )
						sb.AppendFormat( "\tValue: {0}\n", BitConverter.ToString( (byte[])this.properties[ i ].Value ) );
					else
						sb.AppendFormat( "\tValue: {0}\n", this.properties[ i ].Value );
				}
			}

			sb.Append( "------------------------------" );

			return sb.ToString();
		}

		protected List<PropertyValue> Properties
		{
			get { return this.properties; }
		}

		[DebuggerDisplay("{propertyType} = {value}")]
		public class PropertyValue : IComparable<PropertyValue>
		{
			private PropertyType propertyType;
			private bool isComplex;
			private bool isBlipId;
			private object value;

			// MD 8/23/11 - TFS84306
			// Added a simpler constructor because most callers were always passing in false and false for the last two params.
			public PropertyValue(PropertyType propertyType, object value)
				: this(propertyType, value, false, false) { }

			public PropertyValue( PropertyType propertyType, object value, bool isComplex, bool isBlipId )
			{
				this.propertyType = propertyType;
				this.value = value;
				this.isComplex = isComplex;
				this.isBlipId = isBlipId;
			}

			public bool IsBlipId
			{
				get { return this.isBlipId; }
			}

			public bool IsComplex
			{
				get { return this.isComplex; }
			}

			// MD 8/26/11 - TFS84024





			public bool IsIMsoArray
			{
				get
				{
					if (this.isComplex == false)
						return false;

					switch (this.propertyType)
					{
						case PropertyType.ClipVertices:
						case PropertyType.ClipSegmentsInfo:
						case PropertyType.DiagramRelationTable:
						case PropertyType.FillStyleShadeColors:
						case PropertyType.GeometryAdjustHandles:
						case PropertyType.GeometryConnectionSites:
						case PropertyType.GeometryConnectionSitesDirection:
						case PropertyType.GeometryFragments:
						case PropertyType.GeometryGuides:
						case PropertyType.GeometryInscribe:
						case PropertyType.GeometrySegmentInfo:
						case PropertyType.GeometryVertices:
						case PropertyType.GroupShapeWrapPolygonVertices:
						case PropertyType.LineBottomStyleDashStyle:
						case PropertyType.LineColumnStyleDashStyle:
						case PropertyType.LineLeftStyleDashStyle:
						case PropertyType.LineRightStyleDashStyle:
						case PropertyType.LineTopStyleDashStyle:
						case PropertyType.LineStyleDashStyle:
							return true;
					}

					return false;
				}
			}

			public PropertyType PropertyType
			{
				get { return this.propertyType; }
			}

			public object Value
			{
				get { return this.value; }
				set { this.value = value; }
			}

			#region IComparable<PropertyValue> Members

			// MD 1/24/08
			// Made changes to allow for VS2008 style unit test accessors
			//int IComparable<PropertyValue>.CompareTo( PropertyValue other )
			public int CompareTo( PropertyValue other )
			{
				return (ushort)this.propertyType - (ushort)other.propertyType;
			}

			#endregion
		}
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