using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using Infragistics.Documents.Excel.Serialization.Excel2007.XLSX;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords
{
	internal class PropertyTable3 : PropertyTableBase
	{
		public PropertyTable3( WorksheetShape shape )
			: base( shape.DrawingProperties3 ) { }

		public PropertyTable3( List<PropertyTableBase.PropertyValue> properties )
			: base( properties ) { }

		public PropertyTable3( byte version, ushort instance, uint recordLength )
			: base( version, instance, recordLength ) { }

		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			base.Load( manager );

			WorksheetShape shape = (WorksheetShape)manager.ContextStack[ typeof( WorksheetShape ) ];

			if ( shape != null )
			{
				shape.DrawingProperties3 = this.Properties;

				// MD 11/10/11 - TFS85193
				this.ParseLoadedPropertyValues(manager, shape);
			}
			else 
			{
				manager.Workbook.DrawingProperties3 = this.Properties;
			}
		}

		// MD 11/10/11 - TFS85193
		private void ParseLoadedPropertyValues(BIFF8WorkbookSerializationManager manager, WorksheetShape shape)
		{
			byte[] office2007Data = null;

			for (int i = 0; i < this.Properties.Count; i++)
			{
				PropertyValue propertyValue = this.Properties[i];

				switch (propertyValue.PropertyType)
				{
					case PropertyType.Office2007Data:
						office2007Data = (byte[])propertyValue.Value;
						break;
				}
			}

			if (office2007Data != null && manager.PackageFactory != null)
			{
				
				//using (MemoryStream ms = new MemoryStream(office2007Data))
				//using (IPackage package = manager.PackageFactory.Open(ms, FileMode.Open))
				//using (XLSXWorkbookSerializationManager xlsxManager = new XLSXWorkbookSerializationManager(package, manager, shape))
				//{
				//    xlsxManager.Load();
				//}
			}
		}

		public override EscherRecordType Type
		{
			get { return EscherRecordType.PropertyTable3; }
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