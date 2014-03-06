using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.Serialization;
using Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords;
using Infragistics.Documents.Excel.Serialization.BIFF8.OBJRecords;
using Infragistics.Documents.Excel.Serialization.Excel2007;




using Infragistics.Shared;
using System.Drawing;


namespace Infragistics.Documents.Excel
{
	
	// MD 7/20/2007 - BR25039
	// Created class to store comment for a cell
	// MD 9/2/08 - Cell Comments
	//internal class WorksheetCellCommentShape : WorksheetShape
	/// <summary>
	/// Represents a comment for a cell.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// Comments cannot be added to a worksheet's or a shape group's shapes collection. Instead, they must be set directly on the cell 
	/// with the cell's <see cref="WorksheetCell.Comment"/> property.
	/// </p>
	/// <p class="body">
	/// It is not required that the anchors of this shape be initialized before setting it as the comment of a cell. If the 
	/// <see cref="WorksheetShape.TopLeftCornerCell"/> and <see cref="WorksheetShape.BottomRightCornerCell"/> anchors are both null when
	/// the comment is applied to a cell, default anchor cells will be used based on the cell.
	/// </p>
	/// </remarks>



	public

		// MD 11/8/11 - TFS85193
		// This inheritance no longer works. Since shapes have formatted strings with paragraphs, the type for the Text property needed
		// to be changed to FormattedText. But comments do not store their text in paragraph. It is still stored like
		// a FormattedString is structured. So it now derives directly from Shape and defines its own Text property of type FormattedString.
		//class WorksheetCellComment : WorksheetShapeWithText
		class WorksheetCellComment : WorksheetShape, IFormattedStringOwner,
		// MD 1/18/12 - 12.1 - Cell Format Updates
		IWorkbookFontDefaultsResolver
	{
		#region Constants

		// MD 11/8/11 - TFS85193
		// Copied this from WorksheetShapeWithText
		internal const int MaxExcelTextLength = 32767;

		#endregion // Constants

		#region Member Variables

		private string author;
		private WorksheetCell cell;
		private ushort noteOptionFlags;

		// MD 9/2/08 - Cell Comments
		// Moved to the base WorksheetShape because these really apply to all shapes
		//private FormattedString text;
		//private ushort txoOptionFlags;
		//private ushort rotation;

		// MD 11/8/11 - TFS85193
		// Since WorksheetCellComment now derived directly from WorksheetShape, it needs to store its text.
		private FormattedString text;

		#endregion Member Variables

		#region Constructor

		// MD 9/2/08 - Cell Comments
		//internal WorksheetCellCommentShape() { }
		/// <summary>
		/// Creates a new instance of the <see cref="WorksheetCellComment"/> class.
		/// </summary>
		public WorksheetCellComment() 
		{
			// MD 7/18/11 - Shape support
			// Moved this to Initialize()
			//this.PositioningMode = ShapePositioningMode.DontMoveOrSizeWithCells;
			//this.Visible = false;
			this.Initialize();
		}

		// MD 5/4/09 - TFS17197
		// Added a copy constructor so that comments could be created from UnknownShapes.
		internal WorksheetCellComment( WorksheetShapeWithText shapeWithText )
			: base( shapeWithText )
		{
			
			// MD 7/18/11 - Shape support
			// Moved this to Initialize()
			//this.PositioningMode = ShapePositioningMode.DontMoveOrSizeWithCells;
			//this.Visible = false;
			this.Initialize();

			// MD 11/8/11 - TFS85193
			// We need the timing to be right when loading from the 2003 formats. That means that when we use this copy constructor
			// the text should not have been loaded yet. This is to ensure the shape type is right so we know to load a FormattedString
			// for the comment rather than FormattedText for any other shape with text.
			Debug.Assert(shapeWithText.Text == null, "The FormattedText should not have been loaded yet.");
		}

		// MD 7/18/11 - Shape support
		private void Initialize()
		{
			// MD 8/23/11 - TFS84306
			// Initialize the Fill property to the default comment fill.
			// MD 1/18/12 - 12.1 - Cell Format Updates
			//this.Fill = ShapeFill.FromColor(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xE1));
			this.Fill = ShapeFill.FromColor(Utilities.SystemColorsInternal.InfoColor);

			this.PositioningMode = ShapePositioningMode.DontMoveOrSizeWithCells;
			this.Visible = false;
			this.TxoOptionFlags = 530;
		}

		#endregion Constructor

		#region Base Class Overrides

		#region CanBeAddedToShapesCollection

		internal override bool CanBeAddedToShapesCollection
		{
			get { return false; }
		}

		#endregion CanBeAddedToShapesCollection

		// MD 8/23/11 - TFS84306
		// The WorksheetCellComment cannot have it's Outline changed.
		#region CanHaveOutline

		internal override bool CanHaveOutline
		{
			get { return false; }
		}

		#endregion  // CanHaveOutline

		// MD 9/2/08 - Cell Comments
		// This is no longer needed
		//#region HasTxoData
		//
		//internal override bool HasTxoData
		//{
		//    get { return true; }
		//}
		//
		//#endregion HasTxoData

		// MD 11/8/11 - TFS85193
		#region InitSerializationCache

		internal override void InitSerializationCache(WorkbookSerializationManager serializationManager)
		{
			base.InitSerializationCache(serializationManager);

			if (this.text != null)
			{
				// MD 1/18/12 - 12.1 - Cell Format Updates
				//this.text.Element.InitSerializationCache(serializationManager);
				this.text.Element.InitSerializationCache(serializationManager, this);
			}
		}

		#endregion  // InitSerializationCache

		#region IsTopMost

		internal override bool IsTopMost
		{
			get { return true; }
		}

		#endregion IsTopMost

		// MD 11/8/11 - TFS85193
		#region OnRemovedFromCollection

		internal override void OnRemovedFromCollection()
		{
			base.OnRemovedFromCollection();

			if (this.text != null)
				this.text.SetWorksheet(null);
		}

		#endregion // OnRemovedFromCollection

		// MD 11/8/11 - TFS85193
		#region OnSitedOnWorksheet

		internal override void OnSitedOnWorksheet(Worksheet worksheet)
		{
			base.OnSitedOnWorksheet(worksheet);

			if (this.text != null)
				this.text.SetWorksheet(worksheet);
		}

		#endregion // OnSitedOnWorksheet

		// MD 9/2/08 - Cell Comments
		#region PopuplateDrawingProperties

		internal override void PopuplateDrawingProperties( WorkbookSerializationManager manager )
		{
			if ( this.DrawingProperties1 == null )
				this.DrawingProperties1 = new List<PropertyTableBase.PropertyValue>();

			if ( this.DrawingProperties1.Count == 0 )
			{
				this.DrawingProperties1.Add( new PropertyTableBase.PropertyValue( PropertyType.TextDirection, (uint)655368, false, false ) );
				this.DrawingProperties1.Add( new PropertyTableBase.PropertyValue( PropertyType.GeometryTypeOfConnectionSites, (uint)0, false, false ) );
				// MD 8/23/11 - TFS84306
				// This is set dynamically now by the instance on the Fill property of the shape.
				//this.DrawingProperties1.Add( new PropertyTableBase.PropertyValue( PropertyType.FillStyleColor, (uint)14811135, false, false ) );
				this.DrawingProperties1.Add( new PropertyTableBase.PropertyValue( PropertyType.FillStyleBackColor, (uint)14811135, false, false ) );
				this.DrawingProperties1.Add( new PropertyTableBase.PropertyValue( PropertyType.FillStyleColorModification, (uint)268435700, false, false ) );
				// MD 8/23/11 - TFS84306
				// This is set dynamically now by the instance on the Fill property of the shape.
				//this.DrawingProperties1.Add( new PropertyTableBase.PropertyValue( PropertyType.FillStyleNoFillHitTest, (uint)1048592, false, false ) );
				this.DrawingProperties1.Add( new PropertyTableBase.PropertyValue( PropertyType.LineStyleColorModification, (uint)268435700, false, false ) );
				this.DrawingProperties1.Add( new PropertyTableBase.PropertyValue( PropertyType.ShadowColor, (uint)0, false, false ) );
				this.DrawingProperties1.Add( new PropertyTableBase.PropertyValue( PropertyType.ShadowColorModification, (uint)268435700, false, false ) );
				this.DrawingProperties1.Add( new PropertyTableBase.PropertyValue( PropertyType.ShadowObscured, (uint)196611, false, false ) );
			}

			base.PopuplateDrawingProperties( manager );
		} 

		#endregion PopuplateDrawingProperties

		#region PopulateObjRecords

		internal override void PopulateObjRecords()
		{
			// MD 10/30/11 - TFS90733
			// Instead of manually populating the ObjRecords collection, we will now create one Obj instance, which will 
			// manage its stored records.
			//if ( this.ObjRecords != null )
			//    return;
			//
			//this.ObjRecords = new List<OBJRecordBase>();
			//
			//this.ObjRecords.Add( new CommonObjectData( ObjectType.Comment ) );
			//this.ObjRecords.Add( new Note( 22 ) );
			//this.ObjRecords.Add( new End() );
			if ( this.Obj == null )
				this.Obj = new Obj(this);
		} 

		#endregion PopulateObjRecords

		// MD 10/10/11 - TFS90805
		#region Removed

		//#region Type

		//internal override ShapeType Type
		//{
		//    get { return ShapeType.TextBox; }
		//}

		//#endregion Type

		// MD 10/10/11 - TFS90805
		#region Type2003

		internal override ShapeType? Type2003
		{
			get { return ShapeType.TextBox; }
		}

		#endregion  // Type2003

		// MD 10/10/11 - TFS90805
		#region Type2007

		internal override ST_ShapeType? Type2007
		{
			get { return null; }
		}

		#endregion  // Type2007

		#endregion  // Removed

		#region VerifyPositioningMode

		internal override void VerifyPositioningMode( ShapePositioningMode value )
		{
			// MD 2/6/08 - BR30292
			// It seems our assumption that only DontMoveOrSizeWithCells was incorrect here. Until otherwise seen
			// we should assume DontMoveOrSizeWithCells and MoveAndSizeWithCells are the only valid comment 
			// ShapePositioningMode values.
			//if ( value != ShapePositioningMode.DontMoveOrSizeWithCells )
			if ( value != ShapePositioningMode.DontMoveOrSizeWithCells &&
				value != ShapePositioningMode.MoveAndSizeWithCells )
			{
				throw new ArgumentException( SR.GetString( "LE_ArgumentException_InvalidCommentPositioningMode" ), "value" );
			}
		}

		#endregion VerifyPositioningMode

		// MD 9/2/08 - Cell Comments
		#region Worksheet

		/// <summary>
		/// Gets the worksheet on which the shape resides.
		/// </summary>
		/// <value>The worksheet on which the shape resides.</value>
		public override Worksheet Worksheet
		{
			get 
			{
				if ( this.cell == null )
					return null;

				return this.cell.Worksheet; 
			}
		} 

		#endregion Worksheet

		#endregion Base Class Overrides

		#region Interfaces

		// MD 11/8/11 - TFS85193
		#region IFormattedStringOwner Members

		void IFormattedStringOwner.OnUnformattedStringChanged(FormattedString sender)
		{
			Debug.Assert(sender == this.text, "Incorrect sender!");

			// Ensure that the text's length does not exceed the maximum allowed by Excel
			if (this.text != null)
			{
				string unformattedString = this.text.UnformattedString;

				if (String.IsNullOrEmpty(unformattedString) == false && unformattedString.Length > WorksheetCellComment.MaxExcelTextLength)
					throw new ArgumentException(String.Format(SR.GetString("LE_ArgumentException_TextLengthGreaterThanMax"), WorksheetCellComment.MaxExcelTextLength));
			}

			this.VerifyTextFormattingRuns();

			if (this.HasExcel2007ShapeSerializationManager)
				this.Excel2007ShapeSerializationManager.OnFormattedStringChanged();
		}

		#endregion

		// MD 1/18/12 - 12.1 - Cell Format Updates
		#region IWorkbookFontDefaultsResolver Members

		void IWorkbookFontDefaultsResolver.ResolveDefaults(WorkbookFontData font)
		{
			if (font.ColorInfo == null)
				font.ColorInfo = new WorkbookColorInfo(Utilities.SystemColorsInternal.InfoTextColor);

			UltimateFontDefaultsResolver.Instance.ResolveDefaults(font);
		}

		#endregion

		#endregion // Interfaces

		#region Methods

		// MD 11/8/11 - TFS85193
		#region VerifyTextFormattingRuns

		internal void VerifyTextFormattingRuns()
		{
			if (this.text == null)
				return;

			StringElement element = this.text.Element;
			if (String.IsNullOrEmpty(element.UnformattedString))
				return;

			// MD 1/31/12 - TFS100573
			//if (element.HasFormatting)
			FormattedStringElement formattedStringElement = element as FormattedStringElement;
			if (formattedStringElement != null && formattedStringElement.HasFormatting)
				return;

			if (this.Worksheet == null)
				return;

			this.text.BeforeSet();
			
			// MD 1/31/12 - TFS100573
			//element.FormattingRuns.Add(new FormattedStringRun(this.text.Element, 0));
			if (formattedStringElement == null)
				formattedStringElement = this.text.ConvertToFormattedStringElement();

			formattedStringElement.FormattingRuns.Add(new FormattedStringRun(formattedStringElement, 0));

			this.text.AfterSet();
		}
		
		#endregion //VerifyTextFormattingRuns

		#endregion Methods

		#region Properties

		#region Author

		/// <summary>
		/// Gets or sets the author of the comment.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// This information is not displayed in the Microsoft Excel UI and is for informational purposes only.
		/// </p>
		/// </remarks>
		/// <value>The author of the comment.</value>
		public string Author
		{
			get { return this.author; }
			set 
			{
				if ( value == null )
					value = string.Empty;

				this.author = value; 
			}
		}

		#endregion Author

		#region Cell

		/// <summary>
		/// Gets the cell to which the comment is applied.
		/// </summary>
		/// <value>The cell to which the comment is applied.</value>
		/// <seealso cref="WorksheetCell.Comment"/>
		public WorksheetCell Cell
		{
			get { return this.cell; }
			internal set 
            {
                if (this.cell == value)
                    return;

                this.cell = value;

                // MBS 9/8/08 - Cell Comments                
                this.VerifyTextFormattingRuns();
            }
		}

		#endregion Cell

		#region NoteOptionFlags

		// MD 9/2/08 - Cell Comments
		// This should not be public
		//public ushort NoteOptionFlags
		internal ushort NoteOptionFlags
		{
			get { return this.noteOptionFlags; }
			set { this.noteOptionFlags = value; }
		}

		#endregion NoteOptionFlags

		// MD 11/8/11 - TFS85193
		// Since WorksheetCellComment now derived directly from WorksheetShape, it needs to store its text.
		#region Text

		/// <summary>
		/// Gets or sets the formatted text of the comment.
		/// </summary>
		/// <remarks>
		/// <p class="body">
		/// Getting the value of this property will always return a non-null value. If null is set on the property, it will subsequently 
		/// return an empty formatted string.
		/// </p>
		/// </remarks>
		/// <value>The formatted text of the comment.</value>
		public FormattedString Text
		{
			get
			{
				if (this.text == null)
				{
					this.text = new FormattedString(string.Empty);
					this.text.Owner = this;
				}

				return this.text;
			}
			set
			{
				if (this.text == value)
				{
					Debug.Assert(this.text.Owner == this, "The text is not owned by the shape.");
					return;
				}

				if (value != null)
					value.VerifyNewOwner(this);

				if (this.text != null)
					this.text.Owner = null;

				this.text = value;

				if (this.text != null)
					this.text.Owner = this;

				((IFormattedStringOwner)this).OnUnformattedStringChanged(this.text);
			}
		}

		internal bool HasText
		{
			get
			{
				return
					this.text != null &&
					String.IsNullOrEmpty(this.text.UnformattedString) == false;
			}
		}

		#endregion Text 

		// MD 9/2/08 - Cell Comments
		#region Moved to base

		
#region Infragistics Source Cleanup (Region)



























#endregion // Infragistics Source Cleanup (Region)



		#endregion Moved to base

		#endregion Properties
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