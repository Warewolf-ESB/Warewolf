using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Collections;
using System.Collections.Generic;
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A column that uses a <see cref="HyperlinkButton"/> as the content for it's <see cref="CellBase"/>s
	/// </summary>
	public class HyperlinkColumn : Column
    {
        #region Members

        Binding _contentBinding;
        Style _hyperlinkButtonStyle;

        #endregion // Members

        #region Properties

        #region Public

        #region Content

        /// <summary>
		/// Identifies the <see cref="Content"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(string), typeof(HyperlinkColumn), new PropertyMetadata(new PropertyChangedCallback(ContentChanged)));

		/// <summary>
		/// Gets/Sets the content that should be displayed in every cell of the <see cref="HyperlinkColumn"/>
		/// </summary>
		public string Content
		{
			get { return (string)this.GetValue(ContentProperty); }
			set { this.SetValue(ContentProperty, value); }
		}

		private static void ContentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			HyperlinkColumn col = (HyperlinkColumn)obj;
			col.OnPropertyChanged("Content");
		}

		#endregion // Content 

		#region ContentBinding

		/// <summary>
		/// Gets/Sets a <see cref="Binding"/>that should be used for the conent of the <see cref="HyperlinkColumn"/>. Note: the source of the binding will be the underlying data of the specific row being displayed.
		/// </summary>
		public Binding ContentBinding
		{
            get { return this._contentBinding; }
			set 
            {
                if (this._contentBinding != value)
                {
                    this._contentBinding = value;
                    this.OnPropertyChanged("ContentBinding");
                }
            }
		}

		#endregion // ContentBinding 

		#region TargetName

		/// <summary>
		/// Identifies the <see cref="TargetName"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty TargetNameProperty = DependencyProperty.Register("TargetName", typeof(string), typeof(HyperlinkColumn), new PropertyMetadata(new PropertyChangedCallback(TargetNameChanged)));

		/// <summary>
		/// Gets/Sets the propertyName of a target window or frame to navigate to within the WebPage.
		/// </summary>
		public string TargetName
		{
			get { return (string)this.GetValue(TargetNameProperty); }
			set { this.SetValue(TargetNameProperty, value); }
		}

		private static void TargetNameChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			HyperlinkColumn col = (HyperlinkColumn)obj;
			col.OnPropertyChanged("TargetName");
		}

		#endregion // TargetName 			

		#region HyperlinkButtonStyle

        /// <summary>
        /// Gets/sets the Style that will be applied to every HyperlinkButton in this <see cref="HyperlinkColumn"/>.
        /// </summary>
        /// <value>
        /// The hyperlink button style.
        /// </value>
		public Style HyperlinkButtonStyle
        {
            get
            {
                return this._hyperlinkButtonStyle;
            }
            set
            {
                if (this._hyperlinkButtonStyle != value)
                {
                    this._hyperlinkButtonStyle = value;
                    this.OnPropertyChanged("HyperlinkButtonStyle");
                }
            }
        }

		#endregion // HyperlinkButtonStyle 
				
		#endregion // Public

		#endregion // Properties

		#region Overrides

		#region DataType
		/// <summary>
		/// The DataType that the column's data is derived from.
		/// </summary>
		public override Type DataType
		{
			get
			{
				return base.DataType;
			}
            protected internal set
            {
                base.DataType = value;
                if (base.DataType == typeof(Uri))
                {
                    if (this.SortComparer == null)
                        this.SortComparer = new UriComparer();
                }
            }
		}
		#endregion // DataType

		#region GenerateContentProvider
		/// <summary>
		/// Generates a new <see cref="HyperlinkColumnContentProvider"/> that will be used to generate conent for <see cref="Cell"/> objects for this <see cref="Column"/>.
		/// </summary>
		/// <returns></returns>
		protected internal override ColumnContentProviderBase GenerateContentProvider()
		{
			return new HyperlinkColumnContentProvider();
		}
		#endregion // GenerateContentProvider

		#region FillAvailableSummaries

		/// <summary>
		/// Fills the <see cref="SummaryOperandCollection"/> with the operands that the column expects as summary values.
		/// </summary>
		/// <param name="availableSummaries"></param>
		protected internal override void FillAvailableSummaries(SummaryOperandCollection availableSummaries)
		{
			availableSummaries.Add(new CountSummaryOperand());
		}
		#endregion // FillAvailableSummaries

		#endregion // Overrides
	}

	/// <summary>
	/// A IComparer implementation for URI datatypes
	/// </summary>
	internal class UriComparer : IComparer<Uri>
	{
		#region IComparer<Uri> Members
		/// <summary>
		///     Compares two objects and returns a value indicating whether one is less than,
		///     equal to, or greater than the other.
		/// </summary>
		/// <param propertyName="x"> The first object to compare.</param>
		/// <param propertyName="y">The second object to compare.</param>
		/// <returns/>
		public int Compare(Uri x, Uri y)
		{
			if (x == null && y == null) 
				return 0;

			if (x == null) 
				return -1;

			if (y == null) 
				return 1;

			return string.Compare(x.OriginalString, y.OriginalString, StringComparison.CurrentCulture);
		}

		#endregion
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