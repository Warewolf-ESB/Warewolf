using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Infragistics.Controls.Editors
{
	/// <summary>
	/// A visual representation of a <see cref="ComboHeaderCell"/>
	/// </summary>

	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_MultiColumnCombo, Version = FeatureInfo.Version_11_2)]

	public class ComboHeaderCellControl : ComboCellControlBase
	{
		#region Member Variables

		DataTemplate _currentTemplate;
		FrameworkElement _contentElement;

		#endregion //Member Variables

		#region Constructor


		static ComboHeaderCellControl()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboHeaderCellControl), new FrameworkPropertyMetadata(typeof(ComboHeaderCellControl)));
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="ComboHeaderCellControl"/> class.
        /// </summary>
		public ComboHeaderCellControl()
        {



        }

		#endregion //Constructor

		#region Overrides

		#region AttachContent

		/// <summary>
		/// Invoked when content is attached to the Control.
		/// </summary>
		protected override void AttachContent()
		{
			this.EnsureContent();
		}
		#endregion // AttachContent

		#region EnsureContent

		/// <summary>
		/// This will get called every time the control is measured, and allows the control to adjust it's content if necessary.
		/// </summary>
		internal protected override void EnsureContent()
		{
			ComboColumn column = this.Cell.Column;
			if (column != null)
			{
				bool notSet = true;

				string key = null;
				if (column.Key != null)
				{
					string[] keys = column.Key.Split('.');
					key = keys[keys.Length - 1];
				}

				if (column.HeaderTemplate != null)
				{
					if (this._currentTemplate != column.HeaderTemplate)
					{
						this._currentTemplate = column.HeaderTemplate;
						this._contentElement = column.HeaderTemplate.LoadContent() as FrameworkElement;
					}

					this.Content = this._contentElement;
					this.DataContext = key;

					notSet = false;
				}

				if (notSet || this.Content == null)
				{
					if (string.IsNullOrEmpty(column.HeaderText) && column.DataField != null && column.DataField.DisplayName == null)
						this.Content = key;
					else
						this.Content = column.HeaderText;
				}
			}
		}

		#endregion // EnsureContent

		#region MeasureOverride

		/// <summary>
		/// Allows a Cell to ensure it was propely measured. 
		/// </summary>
		/// <param name="availableSize"></param>
		/// <returns></returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			return base.MeasureOverride(availableSize);
		}

		#endregion // MeasureOverride

		#endregion //Overrides
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