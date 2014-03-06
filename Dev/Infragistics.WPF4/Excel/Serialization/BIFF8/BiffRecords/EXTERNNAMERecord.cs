using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords
{
	internal class EXTERNNAMERecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			byte[] data = new byte[ 0 ];
			int dataIndex = 0;
			ushort optionFlags = manager.CurrentRecordStream.ReadUInt16FromBuffer( ref data, ref dataIndex );

			bool isBuiltIn =			( optionFlags & 0x0001 ) == 0x0001;

			ushort indexToSheetThatContainsName = manager.CurrentRecordStream.ReadUInt16FromBuffer( ref data, ref dataIndex );
			manager.CurrentRecordStream.ReadUInt16FromBuffer( ref data, ref dataIndex ); // not used

			// MD 10/8/07 - BR27172
			// There are other workbook reference types now
			//ExternalWorkbookReference workbook = manager.WorkbookReferences[ manager.WorkbookReferences.Count - 1 ] as ExternalWorkbookReference;
			//Debug.Assert( workbook != null );
			WorkbookReferenceBase workbook = manager.WorkbookReferences[ manager.WorkbookReferences.Count - 1 ];
			Debug.Assert( ( workbook is CurrentWorkbookReference ) == false );

			string externalName = manager.CurrentRecordStream.ReadFormattedStringFromBuffer( LengthType.EightBit, ref data, ref dataIndex ).UnformattedString; 
			Formula formula = Formula.Load( manager.CurrentRecordStream, FormulaType.ExternalNamedReferenceFormula, ref data, ref dataIndex );

			object scope = workbook;

			if ( indexToSheetThatContainsName > 0 )
				scope = workbook.GetWorksheetReference(indexToSheetThatContainsName - 1);

			if ( isBuiltIn )
				externalName = NamedReferenceBase.NameFromBuiltInName( (BuiltInName)externalName[ 0 ] );

			// MD 10/8/07 - BR27172
			// There are other named reference types now
			//ExternalNamedReference namedReference = (ExternalNamedReference)workbook.GetNamedReference( externalName, scope, true );
			NamedReferenceBase namedReference = workbook.GetNamedReference( externalName, scope, true );
			namedReference.FormulaInternal = formula;

			Debug.Assert( isBuiltIn == namedReference.IsBuiltIn );
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			// MD 10/8/07 - BR27172
			// There are other named reference types now
			//ExternalNamedReference namedReference = (ExternalNamedReference)manager.ContextStack[ typeof( ExternalNamedReference ) ];
			NamedReferenceBase namedReference = (NamedReferenceBase)manager.ContextStack[ typeof( NamedReferenceBase ) ];

			if ( namedReference == null )
			{
                Utilities.DebugFail("There is no named reference in the context stack.");
				return;
			}

			string name = namedReference.Name;

			if ( namedReference.IsBuiltIn )
				name = ( (char)namedReference.BuiltInName ).ToString();

			ushort optionFlags = 0;

			if ( namedReference.IsBuiltIn )
				optionFlags |= 0x0020;

			manager.CurrentRecordStream.Write( optionFlags );

			// MD 10/8/07 - BR27172
			// There are other workbook reference types now
			//if ( namedReference.Scope is ExternalWorkbookReference )
			if ( namedReference.Scope is WorkbookReferenceBase )
				manager.CurrentRecordStream.Write( (ushort)0 );
			else
			{
				Debug.Assert(((WorksheetReference)namedReference.Scope).IsMultiSheet == false, "The scope cannot be a worksheet range.");
				manager.CurrentRecordStream.Write( (ushort)( ( (WorksheetReference)namedReference.Scope ).FirstWorksheetIndex + 1 ) );
			}

			manager.CurrentRecordStream.Write( (ushort)0 );
			manager.CurrentRecordStream.Write( name, LengthType.EightBit );
			namedReference.FormulaInternal.Save( manager.CurrentRecordStream, true, true );
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.EXTERNNAME; }
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