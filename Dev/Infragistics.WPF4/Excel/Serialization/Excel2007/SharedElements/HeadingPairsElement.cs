using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;







using Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements.Variants;
using Infragistics.Documents.Excel.Serialization.Excel2007.SharedContentTypes;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.SharedElements

{
    internal class HeadingPairsElement : XmlElementBase
	{
		#region Constants

		public const string LocalName = "HeadingPairs";

		public const string QualifiedName =
			ExtendedPropertiesPart.DefaultNamespace +
			XmlElementBase.NamespaceSeparator +
			HeadingPairsElement.LocalName; 

		#endregion Constants

		#region Base Class Overrides

		#region ElementName

		public override string ElementName
		{
			get { return HeadingPairsElement.QualifiedName; }
		}

		#endregion ElementName

		#region Load

		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
			// MD 9/28/09 - TFS21642
			// Now that we are reading records as we go, the child elements collection will always be empty when we load a record,
			// so this check is no longer correct.
			//Debug.Assert(
			//    element.ChildNodes.Count == 1 && element.ChildNodes[ 0 ].LocalName == VectorElement.LocalName,
			//    "The HeadingPairs element should contain exactly one vector element." );

			ChildDataItem dataItem = new ChildDataItem();
			manager.ContextStack.Push( dataItem );
		}

		#endregion Load

		#region OnAfterLoadChildNodes

		protected override void OnAfterLoadChildElements( Excel2007WorkbookSerializationManager manager, ElementDataCache elementCache )
		{
			ChildDataItem dataItem = (ChildDataItem)manager.ContextStack[ typeof( ChildDataItem ) ];

			if ( dataItem == null )
			{
                
				Utilities.DebugFail("For some reason, the data item was removed from the context stack");
				return;
			}

			Array array = dataItem.Data as Array;
			Debug.Assert( array != null, "The HeadingPairs element never had an Array created by its child element." );

			// RoundTrip - Page 5112
			//
			// Heading pairs indicates the grouping of document parts and the number of parts in each group. These parts are
			// not document parts but conceptual representations of document sections.
		} 

		#endregion OnAfterLoadChildNodes

		#region Save

		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
			Workbook workbook = (Workbook)manager.ContextStack[ typeof( Workbook ) ];

			if ( workbook == null )
			{
				Utilities.DebugFail( "Cannot find the workbook on the context stack." );
				return;
			}

            object[] headingPairs = new object[ 2 ];

            headingPairs[ 0 ] = "Worksheets";
            headingPairs[ 1 ] = workbook.Worksheets.Count;

            manager.ContextStack.Push( new ChildDataItem( headingPairs ) );

            XmlElementBase.AddElement( element, VectorElement.QualifiedName );
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