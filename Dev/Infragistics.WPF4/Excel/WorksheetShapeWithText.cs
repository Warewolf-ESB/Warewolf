using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using Infragistics.Documents.Excel.Serialization;
using Infragistics.Documents.Excel.Serialization.BIFF8.EscherRecords;
using System.ComponentModel;




using Infragistics.Shared;
using System.Drawing;


namespace Infragistics.Documents.Excel
{
	/// <summary>
	/// Abstract base class for all shapes that can display text.
	/// </summary>
	/// <remarks>
	/// <p class="body">
	/// Primitive shapes, such as polygons, and cell comments can display regular text or text with mixed formatting and are derived from this class.
	/// </p>
	/// </remarks>
	/// <seealso cref="WorksheetCellComment"/>
	/// <seealso cref="UnknownShape"/>



	public

		 abstract class WorksheetShapeWithText : WorksheetShape,
		// MD 11/8/11 - TFS85193
		// WorksheetShapeWithText no longer owns a FormattedString. It now owns FormattedText
		//IFormattedStringOwner,
		IFormattedTextOwner,
		IWorkbookFontDefaultsResolver		// MD 8/23/11 - TFS84306
	{
		#region Constants

		// MD 8/7/12 - TFS115692
		private const int DefaultHorizontalMargin = 144;
		private const int DefaultVerticalMargin = 72;

		// MD 7/18/11 - Shape support
		// Moved to the base class
		//private const uint HiddenBitOnExtendedProperties = 0x02;

		internal const int MaxExcelTextLength = 32767;

		#endregion Constants

		#region Member Variables

		// MD 8/7/12 - TFS115692
		private int bottomMargin = WorksheetShapeWithText.DefaultVerticalMargin;
		private int leftMargin = WorksheetShapeWithText.DefaultHorizontalMargin;
		private int rightMargin = WorksheetShapeWithText.DefaultHorizontalMargin;

		// MD 11/8/11 - TFS85193
		// The shapes with text must now store their text as FormattedText, because each paragraph of a shape's text 
		// can have certain properties which apply at the paragraph level, such as horizontal alignment.
		//private FormattedString text; 
		private FormattedText text;

		// MD 8/7/12 - TFS115692
		private int topMargin = WorksheetShapeWithText.DefaultVerticalMargin;

		#endregion Member Variables

		#region Constructor

		// MD 9/14/11 - TFS86093
		//internal WorksheetShapeWithText() { }
		internal WorksheetShapeWithText()
			: this(true) { }

		internal WorksheetShapeWithText(bool initializeDefaults)
			: base(initializeDefaults) { }

		// MD 11/8/11 - TFS85193
		// This is no longer needed now that WorksheetCellComment doesn't derive from WorksheetShapeWithText anymore.
		//// MD 5/4/09 - TFS17197
		//// Added a copy constructor 
		//internal WorksheetShapeWithText( WorksheetShapeWithText shapeWithText )
		//    : base( shapeWithText )
		//{
		//    if ( shapeWithText.text != null )
		//        this.Text = shapeWithText.text.Clone();
		//}

		#endregion Constructor

		#region Interfaces

		#region IFormattedStringOwner Members

		// MD 11/10/11 - TFS85193
		void IFormattedTextOwner.OnFormattingChanged(FormattedText sender)
		{
			Debug.Assert(sender == this.text, "Incorrect sender!");
			if (this.HasExcel2007ShapeSerializationManager)
				this.Excel2007ShapeSerializationManager.OnFormattedStringChanged();
		}

		// MD 4/12/11 - TFS67084
		//void IFormattedStringOwner.OnUnformattedStringChanged()
		// MD 11/8/11 - TFS85193
		//void IFormattedStringOwner.OnUnformattedStringChanged(FormattedString sender)
		void IFormattedTextOwner.OnUnformattedStringChanged(FormattedText sender)
		{
			Debug.Assert(sender == this.text, "Incorrect sender!");

			// Ensure that the text's length does not exceed the maximum allowed by Excel
			if ( this.text != null )
			{
				// MD 11/8/11 - TFS85193
		        //string unformattedString = this.text.UnformattedString;
				string unformattedString = this.text.ToString();

				if ( String.IsNullOrEmpty( unformattedString ) == false && unformattedString.Length > WorksheetShapeWithText.MaxExcelTextLength )
					throw new ArgumentException( String.Format( SR.GetString( "LE_ArgumentException_TextLengthGreaterThanMax" ), WorksheetShapeWithText.MaxExcelTextLength ) );
			}

			this.VerifyTextFormattingRuns();

			//  BF 9/9/08
			if ( this.HasExcel2007ShapeSerializationManager )
			{
				// MD 11/10/11 - TFS85193
				//this.Excel2007ShapeSerializationManager.OnUnformattedStringChanged();
				this.Excel2007ShapeSerializationManager.OnFormattedStringChanged();
			}
		}
		#endregion

		// MD 8/23/11 - TFS84306
		#region IWorkbookFontDefaultsResolver Members

		void IWorkbookFontDefaultsResolver.ResolveDefaults(WorkbookFontData font)
		{
			// MD 11/8/11 - TFS85193
			// Comments do not derive from WorksheetShapeWithText anymore, so we don't need this check.
			//// Comments do not have white as the default color.
			//if (this is WorksheetCellComment)
			//    return;

			// The default text color for shapes is white, not black.
			// MD 1/17/12 - 12.1 - Cell Format Updates
			//            if (Utilities.ColorIsEmpty(font.Color))
			//            {
			//                font.Color =
			//#if SILVERLIGHT
			//                    Colors.White;
			//#else
			//                    Color.White;
			//#endif
			//            }
			if (font.ColorInfo == null)
				font.ColorInfo = new WorkbookColorInfo(WorkbookThemeColorType.Light1);

			UltimateFontDefaultsResolver.Instance.ResolveDefaults(font);
		}

		#endregion

		#endregion Interfaces

		#region Base Class Overrides

		#region InitSerializationCache

		// MD 8/23/11 - TFS84306
		// This is needed now because strings on shapes are no longer placed in the shared string table.
		//// MD 11/3/10 - TFS49093
		//// This is no longer needed because the shared formatted strings will be iterated over and their fonts will be placed 
		//// in the manager when saving.
		////#region InitSerializationCache
		////
		////internal override void InitSerializationCache( WorkbookSerializationManager serializationManager )
		////{
		////    base.InitSerializationCache( serializationManager );
		////
		////    this.Text.InitSerializationCache( serializationManager );
		////}
		////
		////#endregion InitSerializationCache
		internal override void InitSerializationCache(WorkbookSerializationManager serializationManager)
		{
			base.InitSerializationCache(serializationManager);

			if (this.text != null)
			{
				// MD 11/8/11 - TFS85193
			    //this.text.Element.InitSerializationCache(serializationManager, this);
				this.text.InitSerializationCache(serializationManager, this);
			}
		}

		#endregion  // InitSerializationCache

		#region OnAddingToCollection

		internal override void OnAddingToCollection( WorksheetShapeCollection collection )
		{
			base.OnAddingToCollection( collection );

			this.VerifyTextFormattingRuns();
		} 

		#endregion OnAddingToCollection

		// MD 7/18/11 - Shape support
		// This logic is needed for all shapes, not just those with text, so it was moved to the base implementation.
		#region Removed

		//#region OnDrawingProperties1Changed

		//internal override void OnDrawingProperties1Changed()
		//{
		//    base.OnDrawingProperties1Changed();

		//    // MD 7/18/11
		//    // Found while adding Shape support. We should be checking for null here.
		//    if (this.DrawingProperties1 == null)
		//        return;

		//    foreach ( PropertyTableBase.PropertyValue property in this.DrawingProperties1 )
		//    {
		//        switch ( property.PropertyType )
		//        {
		//            case PropertyType.ExtendedProperties:
		//                {
		//                    if ( ( property.Value is uint ) == false )
		//                    {
		//                        Utilities.DebugFail( "This property should have had a uint value." );
		//                        break;
		//                    }

		//                    uint value = (uint)property.Value;
		//                    this.Visible = ( value & WorksheetShapeWithText.HiddenBitOnExtendedProperties ) == 0;

		//                    break;
		//                }
		//        }
		//    }
		//}

		//#endregion OnDrawingProperties1Changed

		#endregion  // Removed

		// MD 11/3/10 - TFS49093
		// Now that strings are in the shard string table, we need to unroot strings when this shape is removed.
		#region OnRemovedFromCollection

		internal override void OnRemovedFromCollection()
		{
			base.OnRemovedFromCollection();

			if (this.text != null)
				this.text.SetWorksheet(null);
		} 

		#endregion // OnRemovedFromCollection

		// MD 11/3/10 - TFS49093
		// The text of the shape needs to know when it is added to a worksheet.
		#region OnSitedOnWorksheet

		internal override void OnSitedOnWorksheet(Worksheet worksheet)
		{
			base.OnSitedOnWorksheet(worksheet);

			if (this.text != null)
				this.text.SetWorksheet(worksheet);
		} 

		#endregion // OnSitedOnWorksheet

		// MD 7/18/11 - Shape support
		// This logic is needed for all shapes, not just those with text, so it was moved to the base implementation.
		#region Removed

		//        #region PopuplateDrawingProperties

		//#if DEBUG
		//        /// <summary>
		//        /// Override in derived classed to populate the shape's drawing properties when the workbook
		//        /// is about to be saved.
		//        /// </summary>
		//        /// <param name="manager">The serialization manager which will save the workbook.</param>  
		//#endif
		//        internal override void PopuplateDrawingProperties( WorkbookSerializationManager manager )
		//        {
		//            base.PopuplateDrawingProperties( manager );

		//            if ( this.DrawingProperties1 == null )
		//                this.DrawingProperties1 = new List<PropertyTableBase.PropertyValue>();

		//            uint extendedPropertiesDefaultValue = 0x00020000;
		//            if ( this.Visible == false )
		//                extendedPropertiesDefaultValue |= WorksheetShapeWithText.HiddenBitOnExtendedProperties;

		//            bool hasExtendedProperties = false;
		//            if ( this.DrawingProperties1.Count > 0 )
		//            {
		//                foreach ( PropertyTableBase.PropertyValue property in this.DrawingProperties1 )
		//                {
		//                    if ( property.PropertyType != PropertyType.ExtendedProperties )
		//                        continue;

		//                    hasExtendedProperties = true;

		//                    if ( ( property.Value is uint ) == false )
		//                    {
		//                        Utilities.DebugFail( "This property should have had a uint value." );
		//                        continue;
		//                    }

		//                    uint value = (uint)property.Value;

		//                    if ( this.Visible )
		//                        value &= ~WorksheetShapeWithText.HiddenBitOnExtendedProperties;
		//                    else
		//                        value |= WorksheetShapeWithText.HiddenBitOnExtendedProperties;

		//                    property.Value = value;
		//                }
		//            }

		//            if ( hasExtendedProperties == false )
		//                this.DrawingProperties1.Add( new PropertyTableBase.PropertyValue( PropertyType.ExtendedProperties, extendedPropertiesDefaultValue, false, false ) );
		//        } 

		//        #endregion PopuplateDrawingProperties

		#endregion  // Removed

		// MD 8/7/12 - TFS115692
		#region PopulateKnownProperties






		internal override void PopulateKnownProperties()
		{
			base.PopulateKnownProperties();

			if (this.LeftMargin != WorksheetShapeWithText.DefaultHorizontalMargin)
				this.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(PropertyType.TextLeft, (uint)Utilities.TwipsToEMU(this.LeftMargin)));

			if (this.TopMargin != WorksheetShapeWithText.DefaultVerticalMargin)
				this.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(PropertyType.TextTop, (uint)Utilities.TwipsToEMU(this.TopMargin)));

			if (this.RightMargin != WorksheetShapeWithText.DefaultHorizontalMargin)
				this.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(PropertyType.TextRight, (uint)Utilities.TwipsToEMU(this.RightMargin)));

			if (this.BottomMargin != WorksheetShapeWithText.DefaultVerticalMargin)
				this.DrawingProperties1.Add(new PropertyTableBase.PropertyValue(PropertyType.TextBottom, (uint)Utilities.TwipsToEMU(this.BottomMargin)));
		}

		#endregion // PopulateKnownProperties

		#endregion Base Class Overrides

		#region Methods

		#region VerifyTextFormattingRuns

		internal void VerifyTextFormattingRuns()
		{
			// MD 11/8/11 - TFS85193
			// A different verification needs to be done for the FormattedText.
			#region Old Code

			//if ( this.text == null )
			//    return;

			//// MD 11/3/10 - TFS49093
			//// The formatted string data is now stored on the FormattedStringElement.
			////if ( String.IsNullOrEmpty( this.text.UnformattedString ) )
			//// MD 4/12/11 - TFS67084
			//// Removed the FormattedStringProxy class. The FormattedString holds the element directly now.
			////FormattedStringProxy proxy = this.text.Proxy;
			////FormattedStringElement element = proxy.Element;
			//FormattedStringElement element = this.text.Element;

			//if (String.IsNullOrEmpty(element.UnformattedString))
			//    return;

			//// MD 11/3/10 - TFS49093
			//// The formatted string data is now stored on the FormattedStringElement.
			////if ( this.text.HasFormatting )
			//if (element.HasFormatting)
			//    return;

			//if ( this.Worksheet == null )
			//    return;

			//// If there is any text at all, the formatted string must have a formatting run in the first character position, because there is no
			//// cell associated to get the default font.
			//// MD 11/3/10 - TFS49093
			//// The formatted string data is now stored on the FormattedStringElement. Also, call Before/AfterSet so we don't modify shared strings 
			//// used by other shapes or cells.
			////this.text.FormattingRuns.Add( new FormattedStringRun( this.text, 0, this.Worksheet.Workbook ) );
			//// MD 4/12/11 - TFS67084
			//// Removed the FormattedStringProxy class.
			////proxy.BeforeSet();
			//this.text.BeforeSet();

			//element.FormattingRuns.Add(new FormattedStringRun(this.text, 0, this.Worksheet.Workbook));

			//// MD 4/12/11 - TFS67084
			//// Removed the FormattedStringProxy class.
			////proxy.AfterSet();
			//this.text.AfterSet();

			#endregion // Old Code
			if (this.text == null)
				return;

			if (this.text.Paragraphs.Count > 0)
				return;

			if (this.Worksheet == null)
				return;

			this.text.Paragraphs.Add(string.Empty);
		}
		#endregion //VerifyTextFormattingRuns 

		#endregion Methods

		#region Properties

		#region Public Properties

		#region HasText

		internal bool HasText
		{
			get
			{
				// MD 11/8/11 - TFS85193
				//return
				//    this.text != null &&
				//    String.IsNullOrEmpty( this.text.UnformattedString ) == false;
				return
					this.text != null &&
					String.IsNullOrEmpty(this.text.ToString()) == false;
			}
		}

		#endregion HasText

		#region Text

		/// <summary>
		/// Gets or sets the formatted text of the shape.
		/// </summary>
		/// <value>The formatted text of the shape.</value>
		// MD 11/8/11 - TFS85193
		// The shapes with text must now store their text as FormattedText, because each paragraph of a shape's text 
		// can have certain properties which apply at the paragraph level, such as horizontal alignment.
		//public FormattedString Text
		public FormattedText Text
		{
			get
			{
				// MD 11/8/11 - TFS85193
				// We should no longer lazily load the text. The developer must set it explicitly.
				//if ( this.text == null )
				//{
				//    this.text = new FormattedString( string.Empty );
				//    this.text.Owner = this;
				//}

				return this.text;
			}
			set
			{
				if (this.text == value)
				{
					Debug.Assert(this.text.Owner == this, "The text is not owned by the shape.");
					return;
				}

				// MD 4/12/11
				// Found while fixing TFS67084
				// We should be doing a null check here.
				//value.VerifyNewOwner( this );
				if (value != null)
					value.VerifyNewOwner(this);

				if (this.text != null)
					this.text.Owner = null;

				this.text = value;

				if (this.text != null)
					this.text.Owner = this;

				// MD 4/12/11 - TFS67084
				//( (IFormattedStringOwner)this ).OnUnformattedStringChanged();
				// MD 11/8/11 - TFS85193
				//((IFormattedStringOwner)this).OnUnformattedStringChanged(this.text);
				((IFormattedTextOwner)this).OnUnformattedStringChanged(this.text);
			}
		}

		#endregion Text 

		#endregion // Public Properties
	
		#region Internal Properties

		// MD 8/7/12 - TFS115692
		#region LeftMargin

		internal int LeftMargin
		{
			get { return this.leftMargin; }
			set { this.leftMargin = value; }
		}

		#endregion // LeftMargin

		// MD 8/7/12 - TFS115692
		#region TopMargin

		internal int TopMargin
		{
			get { return this.topMargin; }
			set { this.topMargin = value; }
		}

		#endregion // TopMargin

		// MD 8/7/12 - TFS115692
		#region RightMargin

		internal int RightMargin
		{
			get { return this.rightMargin; }
			set { this.rightMargin = value; }
		}

		#endregion // RightMargin

		// MD 8/7/12 - TFS115692
		#region BottomMargin

		internal int BottomMargin
		{
			get { return this.bottomMargin; }
			set { this.bottomMargin = value; }
		}

		#endregion // BottomMargin

		#endregion // Internal Properties

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