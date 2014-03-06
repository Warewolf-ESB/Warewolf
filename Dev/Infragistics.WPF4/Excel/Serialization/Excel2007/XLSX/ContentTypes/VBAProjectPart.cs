using System;
using System.Collections.Generic;
using System.Text;




namespace Infragistics.Documents.Excel.Serialization.Excel2007.XLSX.ContentTypes

{
	// MD 10/1/08 - TFS8471
	internal class VBAProjectPart : ContentTypeBase
	{
		public const string BasePartName = "/xl/vbaProject.bin";
		public const string ContentTypeValue = "application/vnd.ms-office.vbaProject";
		public const string RelationshipTypeValue = "http://schemas.microsoft.com/office/2006/relationships/vbaProject";

		public override string ContentType
		{
			get { return VBAProjectPart.ContentTypeValue; }
		}

		public override string RelationshipType
		{
			get { return VBAProjectPart.RelationshipTypeValue; }
		}

		public override object Load( Excel2007WorkbookSerializationManager manager, System.IO.Stream contentTypeStream )
		{
			byte[] data = new byte[ contentTypeStream.Length ];
			contentTypeStream.Read( data, 0, data.Length );
			manager.Workbook.VBAData2007 = data;

			return null;
		}

		public override void Save( Excel2007WorkbookSerializationManager manager, System.IO.Stream contentTypeStream )
		{
			byte[] data = manager.Workbook.VBAData2007;
			contentTypeStream.Write( data, 0, data.Length );
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