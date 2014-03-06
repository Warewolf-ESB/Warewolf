using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;




using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Variants

{
	internal class VectorElement : XmlElementBase
	{
		#region Constants

		public const string BaseTypeAttributeName = "baseType";
		public const string SizeAttributeName = "size";

		public const string LocalName = "vector";

		public const string QualifiedName =
			VariantElement.VariantTypesNamespace +
			XmlElementBase.NamespaceSeparator +
			VectorElement.LocalName;

		#endregion Constants

		#region Base Class Overrides

		#region ElementName

		public override string ElementName
		{
			get { return VectorElement.QualifiedName; }
		}

		#endregion ElementName

		#region Load

		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
			Debug.Assert( element.Attributes.Count == 2, "There are two attribtues in the vector element and they are both required." );

			uint size = 0;
			ST_VectorBaseType baseType = ST_VectorBaseType.variant;

			foreach ( ExcelXmlAttribute attribute in element.Attributes )
			{
				string attributeName = XmlElementBase.GetQualifiedAttributeName( attribute );

				switch ( attributeName )
				{
					case VectorElement.BaseTypeAttributeName:
						baseType = (ST_VectorBaseType)XmlElementBase.GetAttributeValue( attribute, DataType.ST_VectorBaseType, 0 );
						break;

					case VectorElement.SizeAttributeName:
						size = (uint)XmlElementBase.GetAttributeValue( attribute, DataType.UInt32, 0 );
						break;

					default:
						Utilities.DebugFail( "Unknown attribute type in the vector element: " + attributeName );
						break;
				}
			}

			Array array;

			#region Create array

			switch ( baseType )
			{
				case ST_VectorBaseType.variant:
					//case ST_VectorBaseType.clsid:
					//case ST_VectorBaseType.cf:
					array = new object[ size ];
					break;

				case ST_VectorBaseType.i1:
					array = new sbyte[ size ];
					break;

				case ST_VectorBaseType.i2:
					array = new short[ size ];
					break;

				case ST_VectorBaseType.i4:
					//case ST_VectorBaseType.error:
					array = new int[ size ];
					break;

				case ST_VectorBaseType.i8:
					array = new long[ size ];
					break;

				case ST_VectorBaseType.ui1:
					array = new byte[ size ];
					break;

				case ST_VectorBaseType.ui2:
					array = new ushort[ size ];
					break;

				case ST_VectorBaseType.ui4:
					array = new uint[ size ];
					break;

				case ST_VectorBaseType.ui8:
					array = new ulong[ size ];
					break;

				case ST_VectorBaseType.r4:
					array = new float[ size ];
					break;

				case ST_VectorBaseType.r8:
					array = new double[ size ];
					break;

				case ST_VectorBaseType.lpstr:
					//case ST_VectorBaseType.lpwstr:
					//case ST_VectorBaseType.bstr:
					array = new string[ size ];
					break;

				case ST_VectorBaseType.date:
					//case ST_VectorBaseType.filetime:
					array = new DateTime[ size ];
					break;

				case ST_VectorBaseType._bool:
					array = new bool[ size ];
					break;

				case ST_VectorBaseType.cy:
					array = new decimal[ size ];
					break;

				default:
					Utilities.DebugFail( "Unknown variant baseType value: " + baseType );
					array = new object[ size ];
					break;
			}

			#endregion Create array

			ChildDataItem dataItem = (ChildDataItem)manager.ContextStack[ typeof( ChildDataItem ) ];

			// The owning element needs to get a reference to the array created by this element
			if ( dataItem != null )
				dataItem.Data = array;

			manager.ContextStack.Push( new ArrayContext( array ) );
		}

		#endregion Load

		#region Save

		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
            ChildDataItem childData = (ChildDataItem)manager.ContextStack[ typeof( ChildDataItem ) ];

            if ( childData == null )
            {
                Utilities.DebugFail( "There was no child data on the context stack." );
                return;
            }

            Array array = childData.Data as Array;

            if ( array == null )
            {
                Utilities.DebugFail( "The child data item should have been of type Array." );
                return;
            }

            Debug.Assert( array.Rank == 1, "The array is expected to have one dimension" );

            int size = array.GetLength( 0 );
            string sizeValue = XmlElementBase.GetXmlString( size, DataType.Int32 );
            XmlElementBase.AddAttribute( element, VectorElement.SizeAttributeName, sizeValue );

            ST_VectorBaseType baseType = VariantBaseElement.GetVariantType( array.GetType().GetElementType() );
            string baseTypeValue = VariantBaseElement.GetVariantElementLocalName( baseType );
            XmlElementBase.AddAttribute( element, VectorElement.BaseTypeAttributeName, baseTypeValue );

            manager.ContextStack.Push( new ArrayContext( array ) );

            XmlElementBase.AddElements( element, VariantBaseElement.GetVariantElementQualifiedName( baseType ), size );
		}

		#endregion Save

		#endregion Base Class Overrides
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