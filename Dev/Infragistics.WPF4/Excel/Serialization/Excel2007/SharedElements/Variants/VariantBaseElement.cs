using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;




namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Variants

{
	
	internal abstract class VariantBaseElement : XmlElementBase
	{
		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
			Debug.Assert(
				manager.ContextStack.Current is CollectionContext,
				"We don't know how to handle a variant element which is not nested directly under a vector element." );

			CollectionContext collectionContext = (CollectionContext)manager.ContextStack[ typeof( CollectionContext ) ];

			if ( collectionContext == null )
				return;

			object convertedValue = XmlElementBase.GetValue( value, (DataType)this.Type, "" );

			collectionContext.AddItem( convertedValue );
		}

		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
			CollectionContext collectionContext = (CollectionContext)manager.ContextStack[ typeof( CollectionContext ) ];

			if ( collectionContext == null )
			{
				Utilities.DebugFail( "Could not find the collection context on the stack." );
				return;
			}

			value = XmlElementBase.GetXmlString( collectionContext.ConsumeCurrentItem(), (DataType)this.Type );
		}

		protected abstract ST_VectorBaseType Type { get; }

		internal static string GetVariantElementLocalName( ST_VectorBaseType type )
		{
			
			switch ( type )
			{
				case ST_VectorBaseType.variant:		return VariantElement.LocalName;
				case ST_VectorBaseType.i1:			return I1Element.LocalName;
				case ST_VectorBaseType.i2:			return I2Element.LocalName;
				case ST_VectorBaseType.i4:			return I4Element.LocalName;
				case ST_VectorBaseType.i8:			return I8Element.LocalName;
				case ST_VectorBaseType.ui1:			return Ui1Element.LocalName;
				case ST_VectorBaseType.ui2:			return Ui2Element.LocalName;
				case ST_VectorBaseType.ui4:			return Ui4Element.LocalName;
				case ST_VectorBaseType.ui8:			return Ui8Element.LocalName;
				case ST_VectorBaseType.r4:			return R4Element.LocalName;
				case ST_VectorBaseType.r8:			return R8Element.LocalName;
				case ST_VectorBaseType.lpstr:		return LpstrElement.LocalName;
				//case ST_VectorBaseType.lpwstr:		return LpwstrElement.LocalName;
				//case ST_VectorBaseType.bstr:		return BstrElement.LocalName;
				case ST_VectorBaseType.date:		return DateElement.LocalName;
				//case ST_VectorBaseType.filetime:	return FiletimeElement.LocalName;
				case ST_VectorBaseType._bool:		return BoolElement.LocalName;
				case ST_VectorBaseType.cy:			return CyElement.LocalName;
				//case ST_VectorBaseType.error:		return ErrorElement.LocalName;
				//case ST_VectorBaseType.clsid:		return ClsidElement.LocalName;
				//case ST_VectorBaseType.cf:			return CfElement.LocalName;

				default:
					Utilities.DebugFail( "Unknown variant type: " + type );
					goto case ST_VectorBaseType.variant;
			}
		}

		internal static string GetVariantElementQualifiedName( ST_VectorBaseType type )
		{
			
			switch ( type )
			{
				case ST_VectorBaseType.variant:		return VariantElement.QualifiedName;
				case ST_VectorBaseType.i1:			return I1Element.QualifiedName;
				case ST_VectorBaseType.i2:			return I2Element.QualifiedName;
				case ST_VectorBaseType.i4:			return I4Element.QualifiedName;
				case ST_VectorBaseType.i8:			return I8Element.QualifiedName;
				case ST_VectorBaseType.ui1:			return Ui1Element.QualifiedName;
				case ST_VectorBaseType.ui2:			return Ui2Element.QualifiedName;
				case ST_VectorBaseType.ui4:			return Ui4Element.QualifiedName;
				case ST_VectorBaseType.ui8:			return Ui8Element.QualifiedName;
				case ST_VectorBaseType.r4:			return R4Element.QualifiedName;
				case ST_VectorBaseType.r8:			return R8Element.QualifiedName;
				case ST_VectorBaseType.lpstr:		return LpstrElement.QualifiedName;
				//case ST_VectorBaseType.lpwstr:		return LpwstrElement.QualifiedName;
				//case ST_VectorBaseType.bstr:		return BstrElement.QualifiedName;
				case ST_VectorBaseType.date:		return DateElement.QualifiedName;
				//case ST_VectorBaseType.filetime:	return FiletimeElement.QualifiedName;
				case ST_VectorBaseType._bool:		return BoolElement.QualifiedName;
				case ST_VectorBaseType.cy:			return CyElement.QualifiedName;
				//case ST_VectorBaseType.error:		return ErrorElement.QualifiedName;
				//case ST_VectorBaseType.clsid:		return ClsidElement.QualifiedName;
				//case ST_VectorBaseType.cf:			return CfElement.QualifiedName;

				default:
					Utilities.DebugFail( "Unknown variant type: " + type );
					goto case ST_VectorBaseType.variant;
			}
		}

		internal static ST_VectorBaseType GetVariantType( Type type )
		{
			if ( type == typeof( bool ) )
				return ST_VectorBaseType._bool;

			if ( type == typeof( decimal ) )
				return ST_VectorBaseType.cy;

			if ( type == typeof( DateTime ) )
				return ST_VectorBaseType.date; 

			if ( type == typeof( sbyte ) )
				return ST_VectorBaseType.i1;

			if ( type == typeof( short ) )
				return ST_VectorBaseType.i2;

			if ( type == typeof( int ) )
				return ST_VectorBaseType.i4;

			if ( type == typeof( long ) )
				return ST_VectorBaseType.i8;

			if ( type == typeof( float ) )
				return ST_VectorBaseType.r4;

			if ( type == typeof( double ) )
				return ST_VectorBaseType.r8;

			if ( type == typeof( byte ) )
				return ST_VectorBaseType.ui1;

			if ( type == typeof( ushort ) )
				return ST_VectorBaseType.ui2;

			if ( type == typeof( uint ) )
				return ST_VectorBaseType.ui4;

			if ( type == typeof( ulong ) )
				return ST_VectorBaseType.ui8;

			if ( type == typeof( string ) )
				return ST_VectorBaseType.lpstr; 

			if ( type == typeof( object ) )
				return ST_VectorBaseType.variant;

			Utilities.DebugFail( "Unknown vector base type" );
			return ST_VectorBaseType.variant;
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