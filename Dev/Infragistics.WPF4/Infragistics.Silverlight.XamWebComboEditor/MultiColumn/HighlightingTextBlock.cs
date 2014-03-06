using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Media;




namespace Infragistics.Controls.Editors.Primitives

{

	/// <summary>
	/// Specialized TextBlock that highlights portions of the text that matches a supplied string.  For internal use only.
	/// </summary>
	public class HighlightingTextBlock : Control
	{
		#region Member Variables

		private string				_lastTextToHighlight;

		#endregion //Member Variables

		#region Constructor

		static HighlightingTextBlock()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(HighlightingTextBlock), new FrameworkPropertyMetadata(typeof(HighlightingTextBlock)));
		}


		internal HighlightingTextBlock()
		{



		}

		#endregion //Constructor

		#region Base Class Overrides

		#region MeasureOverride

		/// <summary>
		/// Provides the behavior for the "measure" pass of the <see cref="HighlightingTextBlock"/>.
		/// </summary>
		/// <param name="availableSize"></param>
		/// <returns></returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			this.HighlightText();

			return base.MeasureOverride(availableSize);
		}

		#endregion //MeasureOverride

		#region OnApplyTemplate

		/// <summary>
		/// Builds the visual tree for the ComboEditorBase when a new template is applied. 
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this.TextBlock = this.GetTemplateChild("TextBlock") as TextBlock;
			if (null != this.TextBlock)
			{
				this.TextBlock.TextWrapping	= this.TextBlockTextWrapping;
				this.TextBlock.Style		= this.TextBlockStyle;
				this.HighlightText();
			}
		}

		#endregion //OnApplyTemplate

		#endregion //Base Class Overrides

		#region Properties

		#region Public

		#region Text

		/// <summary>
		/// Identifies the <see cref="Text"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty TextProperty =
			DependencyProperty.Register("Text", typeof(string), typeof(HighlightingTextBlock), new PropertyMetadata(string.Empty, new PropertyChangedCallback(TextChanged)));

		/// <summary>
		/// Gets/sets the text associated with the control.
		/// </summary>
		public string Text
		{
			get { return (string)this.GetValue(TextProperty); }
			set { this.SetValue(TextProperty, value); }
		}

		private static void TextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			HighlightingTextBlock highlightingTextBlock = obj as HighlightingTextBlock;

			if (null != highlightingTextBlock.TextBlock)
				highlightingTextBlock.HighlightText();
		}

		#endregion // Text

		#endregion //Public

		#region Internal


		#region ComboEditor

		internal XamMultiColumnComboEditor ComboEditor
		{
			get; set;
		}

        #endregion //ComboEditor


        #region TextBlock

        internal TextBlock TextBlock
		{
			get;
			private set;
		}

		#endregion //TextBlock

		#region TextBlockTextWrapping

		internal TextWrapping TextBlockTextWrapping
		{
			get;
			set;
		}

		#endregion //TextBlockTextWrapping

		#region TextBlockStyle

		internal Style TextBlockStyle
		{
			get;
			set;
		}

		#endregion //TextBlockStyle

		#endregion //Internal

		#endregion //Properties

		#region Methods

		#region HighlightText

		internal void HighlightText()
		{

			if (this.ComboEditor != null)
				this.HighlightText(this.ComboEditor.CurrentSearchText);
			else

				this.HighlightText(this._lastTextToHighlight);
		}

		internal void HighlightText(string textToHighlight)
		{
			this._lastTextToHighlight = textToHighlight;

			if (null == this.TextBlock)
				return;

			this.TextBlock.Inlines.Clear();

			if (string.IsNullOrEmpty(textToHighlight))
			{
				this.TextBlock.Text = this.Text;
				return;
			}

			if (string.IsNullOrEmpty(this.Text))
				return;

			string fullText = this.Text;

			int indexOfMatch = fullText.ToUpper().IndexOf(textToHighlight.ToUpper());
			if (indexOfMatch < 0)
				this.TextBlock.Text = fullText;
			else
			{
				Run r;

				// Add text before the match if any.
				if (indexOfMatch > 0)
				{
					r		= new Run();
					r.Text	= fullText.Substring(0, indexOfMatch);
					this.TextBlock.Inlines.Add(r);
				}

				// Add the matched text.
				r				= new Run();
				r.Text			= fullText.Substring(indexOfMatch, textToHighlight.Length);
				r.FontWeight	= FontWeights.Bold;

				r.Background	= Brushes.LightGray;

				this.TextBlock.Inlines.Add(r);

				// Add the text after the match if any.
				int i = indexOfMatch + textToHighlight.Length;
				if (i < fullText.Length)
				{
					r		= new Run();
					r.Text	= fullText.Substring(i);
					this.TextBlock.Inlines.Add(r);
				}
			}
		}

		#endregion //HighlightText

		#endregion //Methods
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