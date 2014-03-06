using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml;




using System.Drawing;
using Infragistics.Documents.Excel.Serialization.Excel2007.XLSX;

namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.Elements

{



	internal class CustomSheetViewsElement : XLSXElementBase
	{
		#region XML Schema fragment <docs>
		// <complexType name="CT_CustomSheetViews">
		// <sequence>
		// <element name="customSheetView" minOccurs="1" maxOccurs="unbounded"
		// type="CT_CustomSheetView"/>
		// </sequence>
		// </complexType>
		#endregion XML Schema fragment <docs>

		#region Constants

		/// <summary>customSheetViews</summary>
		public const string LocalName = "customSheetViews";
		
		/// <summary>http://schemas.openxmlformats.org/spreadsheetml/2006/main/customSheetViews</summary>
		public const string QualifiedName =
			XLSXElementBase.DefaultXmlNamespace +
			XmlElementBase.NamespaceSeparator +
			CustomSheetViewsElement.LocalName;


		#endregion Constants

		#region Base class overrides

			#region Type

		public override XLSXElementType Type
		{
			get { return XLSXElementType.customSheetViews; }
		}

			#endregion Type

			#region Load

		/// <summary>Loads the data for this element from the specified manager.</summary>
		protected override void Load( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, string value, ref bool isReaderOnNextNode )
		{
            // We don't need to do anything here because this element does not have any attributes
		}

			#endregion Load

			#region Save

		/// <summary>Saves the data for this element to the specified manager.</summary>
		protected override void Save( Excel2007WorkbookSerializationManager manager, ExcelXmlElement element, ref string value )
		{
            //  Get a reference to the Worksheet
            Worksheet worksheet = manager.ContextStack[typeof(Worksheet)] as Worksheet;
            if (worksheet == null)
            {
                Utilities.DebugFail("Could not get the worksheet from the context stack");
                return;
            }

            //  Build a consumable list of the CustomView instances that
            //  are associated with the Worksheet we are processing.
            CustomViewCollection customViews = manager.Workbook.CustomViews;
            List<CustomView> list = new List<CustomView>();
            foreach( CustomView customView in customViews )
            {
                CustomViewDisplayOptions displayOptions = customView.GetDisplayOptions(worksheet);
                if ( displayOptions != null )
                    list.Add( customView );
                else
                    Debug.Assert( false, "Could not get a CustomViewDisplayOptions - unexpected." );
            }

            //  Push the consumable list onto the stack
            ListContext<CustomView> context = new ListContext<CustomView>(list);
            manager.ContextStack.Push(context);

            //  Add a CustomSheetViewElement for each member of the CustomViews collection
            for ( int i = 0; i < list.Count; i++ )
            {
                XmlElementBase.AddElement(element, CustomSheetViewElement.QualifiedName);            
            }
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