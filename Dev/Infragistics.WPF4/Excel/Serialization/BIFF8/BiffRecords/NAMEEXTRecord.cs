using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal class NAMEEXTRecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			BIFF8RecordType repeatedBiffType = (BIFF8RecordType)manager.CurrentRecordStream.ReadUInt16();
			Debug.Assert( repeatedBiffType == this.Type );

			manager.CurrentRecordStream.ReadUInt16(); // Cell reference flags
			manager.CurrentRecordStream.ReadUInt64(); // Not used

			ushort referenceNameLength = manager.CurrentRecordStream.ReadUInt16();
			ushort commentLength = manager.CurrentRecordStream.ReadUInt16();

			string referenceName = manager.CurrentRecordStream.ReadFormattedString( referenceNameLength ).UnformattedString;
			string comment = manager.CurrentRecordStream.ReadFormattedString( commentLength ).UnformattedString;

			IList<NamedReferenceBase> namedReferences = manager.Workbook.CurrentWorkbookReference.NamedReferences;
			NamedReference namedReference = (NamedReference)namedReferences[namedReferences.Count - 1];
			Debug.Assert( namedReference.Name == referenceName );
			namedReference.Comment = comment;
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			NamedReferenceBase namedReference = manager.ContextStack.Get<NamedReferenceBase>();

			if ( namedReference == null )
			{
                Utilities.DebugFail("There is no named reference in the context stack.");
				return;
			}

			manager.CurrentRecordStream.Write( (ushort)this.Type );
			manager.CurrentRecordStream.Write( (ushort)0 );
			manager.CurrentRecordStream.Write( (ulong)0 );
			manager.CurrentRecordStream.Write( (ushort)namedReference.Name.Length );
			manager.CurrentRecordStream.Write( (ushort)namedReference.Comment.Length );
			manager.CurrentRecordStream.Write( namedReference.Name );
			manager.CurrentRecordStream.Write( namedReference.Comment );
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.NAMEEXT; }
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