using System.Windows;

namespace Infragistics
{
	/// <summary>
	/// A class which describes the type of summary being applied.
	/// </summary>
	public class SummaryDefinition : DependencyObjectNotifier
	{
		#region ColumnKey

		/// <summary>
		/// Identifies the <see cref="ColumnKey"/> dependency property. 
		/// </summary>
		public static readonly DependencyProperty ColumnKeyProperty = DependencyProperty.Register("ColumnKey", typeof(string), typeof(SummaryDefinition), new PropertyMetadata(new PropertyChangedCallback(ColumnKeyChanged)));

		/// <summary>
		/// Gets / sets the key of the column which this filter applies to.
		/// </summary>
		public string ColumnKey
		{
			get { return (string)this.GetValue(ColumnKeyProperty); }
			set { this.SetValue(ColumnKeyProperty, value); }
		}

		private static void ColumnKeyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			SummaryDefinition sd = (SummaryDefinition)obj;
			sd.OnPropertyChanged("ColumnKey");
		}

		#endregion // ColumnKey

		#region SummaryOperand

		/// <summary>
		/// Get / set the <see cref="SummaryOperandBase"/> which designates which summary should be executed.
		/// </summary>
		public SummaryOperandBase SummaryOperand
		{
			get;
			set;
		}
		#endregion // SummaryOperand
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