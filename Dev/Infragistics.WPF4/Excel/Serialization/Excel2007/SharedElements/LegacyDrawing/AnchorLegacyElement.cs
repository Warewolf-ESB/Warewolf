using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;






using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.LegacyDrawing

{
    internal class AnchorLegacyElement : XmlElementBase
	{
		#region Constants






		public const string LocalName = "Anchor";






		public const string QualifiedName =
			LegacyDrawingsPart.ExcelNamespace +
			XmlElementBase.NamespaceSeparator +
			AnchorLegacyElement.LocalName;

		#endregion Constants

		#region Base class overrides

		#region ElementName

		public override string ElementName
		{
			get { return AnchorLegacyElement.QualifiedName; }
		}

		#endregion ElementName

		#region Load



#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
			LegacyShapeData shapeData = (LegacyShapeData)manager.ContextStack[ typeof( LegacyShapeData ) ];

			if ( shapeData == null )
			{
				Utilities.DebugFail( "There was no shape data on the context stack." );
				return;
			}

			value = value.Trim();
			string[] anchorValues = value.Split( ',' );

			if ( anchorValues.Length != 8 )
			{
				Utilities.DebugFail( "Incorrect anchor: " + value );
				return;
			}

			int[] anchorData = new int[ 8 ];

			for ( int i = 0; i < 8; i++ )
			{
				string anchorValue = anchorValues[ i ];
				anchorValue = anchorValue.Trim();

				int anchor;
				if ( Int32.TryParse( anchorValue, out anchor ) == false )
				{
					Utilities.DebugFail( "Incorrect anchor: " + value );
					return;
				}

				shapeData.AnchorData[ i ] = anchor;
			}
		}

		#endregion Load

		#region Save



#region Infragistics Source Cleanup (Region)



#endregion // Infragistics Source Cleanup (Region)

		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
			LegacyShapeData shapeData = (LegacyShapeData)manager.ContextStack[ typeof( LegacyShapeData ) ];

			if ( shapeData == null )
			{
				Utilities.DebugFail( "Could not find the shape data on the context stack." );
				return;
			}

			for ( int i = 0; i < 8; i++ )
				value += shapeData.AnchorData[ i ] + ", ";

			value = value.Substring( 0, value.Length - 2 );
		}

		#endregion Save

		#endregion Base class overrides
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