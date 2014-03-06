using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;




using System.Drawing;


namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements
{
    // MD 4/24/09
	// Found while fixing TFS16204 
	// Created a base class for all the border side elements
	internal abstract class BorderSideElementBase : XLSXElementBase
	{
		#region Constants

		public const string StyleAttributeName = "style"; 

		#endregion Constants

		#region Base Class Overrides

		#region Load

		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
			BorderInfo borderInfo = (BorderInfo)manager.ContextStack[ typeof( BorderInfo ) ];
			if ( borderInfo == null )
			{
				Utilities.DebugFail( "BorderInfo object not found on stack." );
				return;
			}

			BorderStyleInfo borderStyleInfo = this.GetBorderStyleInfo( borderInfo );

			foreach ( ExcelXmlAttribute attribute in element.Attributes )
			{
				string attributeName = XLSXElementBase.GetQualifiedAttributeName( attribute );

				switch ( attributeName )
				{
					case BorderSideElementBase.StyleAttributeName:
						borderStyleInfo.BorderStyle = (CellBorderLineStyle)XLSXElementBase.GetAttributeValue( attribute, DataType.ST_BorderStyle, CellBorderLineStyle.None );
						break;

					default:
						Utilities.DebugFail( "Unknown attribute type in the " + this.Type.ToString() + " element: " + attributeName );
						break;
				}
			}

			manager.ContextStack.Push( borderStyleInfo.ColorInfo );
		}

		#endregion Load

		#region Save

		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
			BorderInfo borderInfo = (BorderInfo)manager.ContextStack[ typeof( BorderInfo ) ];
			if ( borderInfo == null )
			{
				Utilities.DebugFail( "BorderInfo object not found on stack." );
				return;
			}

			BorderStyleInfo borderStyleInfo = this.GetBorderStyleInfo( borderInfo );

			// MD 4/4/12 - TFS107655
			// Rewrote this code to be a bit cleaner. Also, we should be writing out the color element even if the automatic color is
 			// being written out.
			#region Old Code

			//if ( borderStyleInfo.BorderStyle != CellBorderLineStyle.None &&
			//    borderStyleInfo.BorderStyle != CellBorderLineStyle.Default )
			//    XLSXElementBase.AddAttribute( element, LeftElement.StyleAttributeName, XLSXElementBase.GetXmlString( borderStyleInfo.BorderStyle, DataType.ST_BorderStyle ) );
			//
			//// MD 4/24/09
			//// Found while fixing TFS16204 
			//// If the color evaltaues to black, but it is a system color, it must be written out.
			////if (!borderStyleInfo.ColorInfo.IsDefault &&
			////    borderStyleInfo.ColorInfo.ResolveColor(manager).ToArgb() != System.Drawing.Color.Black.ToArgb())
			////{
			////    manager.ContextStack.Push(borderStyleInfo.ColorInfo);
			////    XLSXElementBase.AddElement(element, ColorElement.QualifiedName);
			////}
			//// MD 2/7/12 - 12.1 - Table Support
			//// The color is automatic by default, so don't write those out.
			////if ( borderStyleInfo.ColorInfo.IsDefault == false )
			//if (borderStyleInfo.ColorInfo.IsDefault == false && borderStyleInfo.ColorInfo.Auto != ExcelDefaultableBoolean.True)
			//{
			//    // MD 1/16/12 - 12.1 - Cell Format Updates
			//    //Color resovledColor = borderStyleInfo.ColorInfo.ResolveColor( manager );
			//    //
			//    //if ( Utilities.ColorIsSystem(resovledColor) ||
			//    //    Utilities.ColorToArgb(resovledColor) != Utilities.ColorToArgb(Utilities.ColorsInternal.Black))
			//    //{
			//    //    manager.ContextStack.Push( borderStyleInfo.ColorInfo );
			//    //    XLSXElementBase.AddElement( element, ColorElement.QualifiedName );
			//    //}
			//    manager.ContextStack.Push(borderStyleInfo.ColorInfo);
			//    XLSXElementBase.AddElement(element, ColorElement.QualifiedName);
			//}

			#endregion // Old Code
			if (borderStyleInfo.BorderStyle != CellBorderLineStyle.None &&
				borderStyleInfo.BorderStyle != CellBorderLineStyle.Default)
			{
				XLSXElementBase.AddAttribute(element, LeftElement.StyleAttributeName, XLSXElementBase.GetXmlString(borderStyleInfo.BorderStyle, DataType.ST_BorderStyle));
			}

			if (borderStyleInfo.ColorInfo.IsDefault == false)
			{
				manager.ContextStack.Push(borderStyleInfo.ColorInfo);
				XLSXElementBase.AddElement(element, ColorElement.QualifiedName);
			}
		}

		#endregion Save

		#region OnAfterLoadChildElements

		protected override void OnAfterLoadChildElements( Excel2007WorkbookSerializationManager manager, ElementDataCache elementCache )
		{
			BorderInfo borderInfo = (BorderInfo)manager.ContextStack[ typeof( BorderInfo ) ];
			if ( borderInfo == null )
			{
				Utilities.DebugFail( "BorderInfo object not found on stack." );
				return;
			}

			ColorInfo colorInfo = (ColorInfo)manager.ContextStack[ typeof( ColorInfo ) ];

			if ( colorInfo == null )
			{
				Utilities.DebugFail( "For some reason, the ColorInfo was removed from the context stack" );
				return;
			}

			// MD 4/4/12 - TFS107655
			// The color is actually null be default. An automatic color will be written out, not omitted.
			//// MD 2/7/12 - 12.1 - Table Support
			//// The color is automatic by default.
			//if (colorInfo.IsDefault)
			//    colorInfo.Auto = ExcelDefaultableBoolean.True;

			BorderStyleInfo borderStyleInfo = this.GetBorderStyleInfo( borderInfo );
			borderStyleInfo.ColorInfo = colorInfo;
		}

		#endregion OnAfterLoadChildElements 

		#endregion Base Class Overrides

		protected abstract BorderStyleInfo GetBorderStyleInfo( BorderInfo borderInfo );
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