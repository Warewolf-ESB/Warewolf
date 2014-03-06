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
using Infragistics.Controls.Grids.Primitives;
using System.Collections.Generic;

namespace Infragistics.Controls.Grids
{
	/// <summary>
	/// A <see cref="Column"/> that generates a <see cref="DatePicker"/> as the content for a <see cref="Cell"/>.
	/// </summary>
	public class DateColumn : DateColumnBase
    {
        
        #region Constructor
        /// <summary>
		/// Initializes a new instance of the <see cref="DateColumn"/> class.
		/// </summary>
        public DateColumn()
        {
            this.AddNewRowItemTemplateVerticalContentAlignment = this.VerticalContentAlignment = VerticalAlignment.Stretch;
            this.AddNewRowItemTemplateHorizontalContentAlignment = this.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        }
		#endregion // Constructor

		#region GenerateContentProvider
		/// <summary>
		/// Generates a new <see cref="DateColumnContentProvider"/> that will be used to generate conent for <see cref="Cell"/> objects for this <see cref="Column"/>.
		/// </summary>
		/// <returns></returns>
		protected internal override ColumnContentProviderBase GenerateContentProvider()
		{
			return new DateColumnContentProvider();
		}
		#endregion // GenerateContentProvider
        
        #region Properties

        #region SelectedDateFormat

        /// <summary>
        /// Identifies the <see cref="SelectedDateFormat"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty SelectedDateFormatProperty = DependencyProperty.Register("SelectedDateFormat", typeof(DatePickerFormat), typeof(DateColumn), new PropertyMetadata(DatePickerFormat.Short,
            new PropertyChangedCallback(SelectedDateFormatChanged)));
        
        /// <summary>
        /// Gets / sets the <see cref="DatePickerFormat"/> that will be assigned to the <see cref="DatePicker"/> controls of the <see cref="DateColumn"/>.
        /// </summary>
        public DatePickerFormat SelectedDateFormat
        {
            get { return (DatePickerFormat)this.GetValue(SelectedDateFormatProperty); }
            set { this.SetValue(SelectedDateFormatProperty, value); }
        }

        private static void SelectedDateFormatChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            DateColumn ctrl = (DateColumn)obj;
            ctrl.OnPropertyChanged("SelectedDateFormat");
        }

        #endregion // SelectedDateFormat

        #endregion // Properties
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