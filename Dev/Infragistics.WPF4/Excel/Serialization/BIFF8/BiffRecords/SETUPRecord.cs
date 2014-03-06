using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;




namespace Infragistics.Documents.Excel.Serialization.BIFF8.BiffRecords

{
	internal class SETUPRecord : Biff8RecordBase
	{
		public override void Load( BIFF8WorkbookSerializationManager manager )
		{
			PrintOptions printOptions = (PrintOptions)manager.ContextStack[ typeof( PrintOptions ) ];

			if ( printOptions == null )
			{
                Utilities.DebugFail("There is no print options in the context stack.");
				return;
			}

			PaperSize paperSize = (PaperSize)manager.CurrentRecordStream.ReadUInt16();
			ushort scalingFactor = manager.CurrentRecordStream.ReadUInt16();
			printOptions.StartPageNumber = manager.CurrentRecordStream.ReadInt16();
			printOptions.MaxPagesHorizontally = manager.CurrentRecordStream.ReadUInt16();
			printOptions.MaxPagesVertically = manager.CurrentRecordStream.ReadUInt16();

			ushort optionFlags = manager.CurrentRecordStream.ReadUInt16();

			ushort resolution = manager.CurrentRecordStream.ReadUInt16();
			ushort verticalResolution = manager.CurrentRecordStream.ReadUInt16();
			printOptions.HeaderMargin = manager.CurrentRecordStream.ReadDouble();
			printOptions.FooterMargin = manager.CurrentRecordStream.ReadDouble();
			ushort numberOfCopies = manager.CurrentRecordStream.ReadUInt16();

			printOptions.PageOrder =		   (PageOrder)( ( optionFlags & 0x0001 ) );
			Orientation orientation =				 (Orientation)( ( optionFlags & 0x0002 ) >> 1 );
			// Paper size, scaling factor, paper orientation (portrait/landscape), print resolution and number of copies are not initialised
			bool propsNotValid =							( optionFlags & 0x0004 ) == 0x0004;
			printOptions.PrintInBlackAndWhite =				( optionFlags & 0x0008 ) == 0x0008;
			printOptions.DraftQuality =						( optionFlags & 0x0010 ) == 0x0010;
			bool printNotes =								( optionFlags & 0x0020 ) == 0x0020;
			bool orientationNotValid =						( optionFlags & 0x0040 ) == 0x0040;
			printOptions.PageNumbering =   (PageNumbering)( ( optionFlags & 0x0080 ) >> 7 );
			bool printNotesAtEndOfSheet =					( optionFlags & 0x0200 ) == 0x0200;
			printOptions.PrintErrors =		 (PrintErrors)( ( optionFlags & 0x0C00 ) >> 10 );

			if ( propsNotValid )
			{
				paperSize = PaperSize.Letter;
				scalingFactor = 100;
				orientation = Orientation.Portrait;
				resolution = 600;
				verticalResolution = 600;
				numberOfCopies = 1;
			}
			else if ( orientationNotValid )
			{
				orientation = Orientation.Portrait;
			}

			// MD 7/13/10 - TFS35691
			// If the loaded paper size is unknown, resolve it to the default paper size.
			if (Enum.IsDefined(typeof(PaperSize), paperSize) == false)
			{
				Utilities.DebugFail("Unknown PaperSize loaded: " + paperSize);
				paperSize = PaperSize.Letter;
			}

			printOptions.PaperSize = paperSize;

			// MD 6/30/09 - TFS18936
			// If the scaling factor is incorrect, we should not throw an error as Excel doesn't report errors when loading this value. 
			// Call off to a new method which allows setting the ScalingFactor with an invalid value.
			//printOptions.ScalingFactor = scalingFactor;
			printOptions.SetScalingFactor( scalingFactor, false );

			printOptions.Resolution = resolution;
			printOptions.VerticalResolution = verticalResolution;
			printOptions.NumberOfCopies = numberOfCopies;
			printOptions.Orientation = orientation;

			if ( printNotes )
			{
				if ( printNotesAtEndOfSheet )
					printOptions.PrintNotes = PrintNotes.PrintAtEndOfSheet;
				else
					printOptions.PrintNotes = PrintNotes.PrintAsDisplayed;
			}
			else
			{
				printOptions.PrintNotes = PrintNotes.DontPrint;
			}
		}

		public override void Save( BIFF8WorkbookSerializationManager manager )
		{
			PrintOptions printOptions = (PrintOptions)manager.ContextStack[ typeof( PrintOptions ) ];

			if ( printOptions == null )
			{
                Utilities.DebugFail("There is no print options in the context stack.");
				return;
			}

			manager.CurrentRecordStream.Write( (ushort)printOptions.PaperSize );
			manager.CurrentRecordStream.Write( (ushort)printOptions.ScalingFactor );
			manager.CurrentRecordStream.Write( (ushort)printOptions.StartPageNumber );
			manager.CurrentRecordStream.Write( (ushort)printOptions.MaxPagesHorizontally );
			manager.CurrentRecordStream.Write( (ushort)printOptions.MaxPagesVertically );

			ushort optionFlags = 0;

			optionFlags |= (ushort)printOptions.PageOrder;

            // MBS 7/30/08 - Excel 2007 Format
            // We need to use the resolved property since the 97-2003 format
            // doesn't have a concept of 'default'
            //
			//optionFlags |= (ushort)( (ushort)printOptions.Orientation << 1 );
            optionFlags |= (ushort)((ushort)printOptions.OrientationResolved << 1);

			if ( printOptions.PrintInBlackAndWhite )
				optionFlags |= 0x0008;

			if ( printOptions.DraftQuality )
				optionFlags |= 0x0010;

			if ( printOptions.PrintNotes != PrintNotes.DontPrint )
				optionFlags |= 0x0020;

			optionFlags |= (ushort)( (ushort)printOptions.PageNumbering << 7 );

			if ( printOptions.PrintNotes == PrintNotes.PrintAtEndOfSheet )
				optionFlags |= 0x0200;

			optionFlags |= (ushort)( (ushort)printOptions.PrintErrors << 10 );

			manager.CurrentRecordStream.Write( optionFlags );

			manager.CurrentRecordStream.Write( (ushort)printOptions.Resolution );
			manager.CurrentRecordStream.Write( (ushort)printOptions.VerticalResolution );
			manager.CurrentRecordStream.Write( printOptions.HeaderMargin );
			manager.CurrentRecordStream.Write( printOptions.FooterMargin );
			manager.CurrentRecordStream.Write( (ushort)printOptions.NumberOfCopies );
		}

		public override BIFF8RecordType Type
		{
			get { return BIFF8RecordType.SETUP; }
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