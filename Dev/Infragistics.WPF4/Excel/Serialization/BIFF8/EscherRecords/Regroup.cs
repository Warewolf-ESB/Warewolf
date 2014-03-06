using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords
{
	// MD 10/30/11 - TFS90733
	internal class Regroup : EscherRecordBase
	{
		private ushort[] fridNews;
		private ushort[] fridOlds;

		public Regroup( byte version, ushort instance, uint recordLength )
			: base( version, instance, recordLength )
		{
			Debug.Assert( version == 0x00 );
		}

		// http://msdn.microsoft.com/en-us/library/dd951577(v=office.12).aspx
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			this.fridNews = new ushort[this.Instance];
			this.fridOlds = new ushort[this.Instance];

			for (int i = 0; i < this.fridNews.Length; i++)
			{
				this.fridNews[i] = manager.CurrentRecordStream.ReadUInt16();
				this.fridOlds[i] = manager.CurrentRecordStream.ReadUInt16();
			}
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			base.Save( manager );

			if (this.fridNews != null)
			{
				for (int i = 0; i < this.fridNews.Length; i++)
				{
					manager.CurrentRecordStream.Write(this.fridNews[i]);
					manager.CurrentRecordStream.Write(this.fridOlds[i]);
				}
			}
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();

			sb.Append( "------------------------------" );
			sb.Append( "\n" );

			sb.Append( base.ToString() );
			sb.Append( "\n\n" );

			if (this.fridNews != null)
			{
				for (int i = 0; i < this.fridNews.Length; i++)
				{
					sb.Append(i);
					sb.Append(": ");
					sb.Append(this.fridNews[i]);
					sb.Append(", ");
					sb.Append(this.fridOlds[i]);
					sb.AppendLine();
				}
			}
			
			sb.Append( "------------------------------" );

			return sb.ToString();
		}

		public override EscherRecordType Type
		{
			get { return EscherRecordType.Regroup; }
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