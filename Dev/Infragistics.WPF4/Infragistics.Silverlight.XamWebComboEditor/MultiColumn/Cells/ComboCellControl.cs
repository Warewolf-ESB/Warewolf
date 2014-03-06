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
using Infragistics.Controls.Editors.Primitives;
using System.Windows.Data;

namespace Infragistics.Controls.Editors
{
    /// <summary>
    /// A visual representation of a <see cref="ComboCell"/>
    /// </summary>

	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_MultiColumnCombo, Version = FeatureInfo.Version_11_2)]

	public class ComboCellControl : ComboCellControlBase
    {
        #region Members

        ComboColumnContentProviderBase _content;
        IValueConverter _currentConveter;

        #endregion // Members

		#region Constructor


		static ComboCellControl()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboCellControl), new FrameworkPropertyMetadata(typeof(ComboCellControl)));
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="ComboCellControl"/> class.
        /// </summary>
		public ComboCellControl()
        {



		}

		#endregion //Constructor

		#region Overrides

		#region OnMouseLeftButtonDown
		/// <summary>
		/// Called before the MouseLeftButtonDown event is raised.
		/// </summary>
		/// <param name="e"></param>
		protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			base.OnMouseLeftButtonDown(e);
			
			e.Handled = true;
			this.Cell.Row.ComboEditor.OnComboEditorItemClicked((ComboRow)this.Cell.Row);
		}
		#endregion // OnMouseLeftButtonDown

		#endregion //Overrides

		#region Properties

		#region Public

		#region ContentProvider
		/// <summary>
        /// Resolves the <see cref="ComboColumnContentProviderBase"/> for this <see cref="ComboCellControl"/>.
        /// </summary>
        public override ComboColumnContentProviderBase ContentProvider
        {
            get { return this._content; }
        }

        #endregion // ContentProvider

        #endregion // Public

        #endregion // Properties

        #region Methods

        #region Protected

        #region AttachContent

        /// <summary>
        /// Invoked when content is attached to the Control.
        /// </summary>
        protected override void AttachContent()
        {
            if (this._content == null)
            {
                this._content = this.Cell.Column.GenerateContentProvider();

                if (this._content != null)
                {
                    this._currentConveter = this.Cell.Column.ValueConverter;
                    this.Content = this._content.ResolveDisplayElement(this.Cell, this._content.ResolveBinding(this.Cell));
                }
            }

            if (this._content != null)
            {
                if (this._currentConveter != this.Cell.Column.ValueConverter)
                {
                    this._content.ResolveDisplayElement(this.Cell, this._content.ResolveBinding(this.Cell));
                }
            }
        }
        #endregion // AttachContent

        #region EnsureContent

        /// <summary>
        /// This will get called every time the control is measured, and allows the control to adjust it's content if necessary.
        /// </summary>
        internal protected override void EnsureContent()
        {
            if (this._content != null)
            {
                this._content.AdjustDisplayElement(this.Cell);
            }
        }

        #endregion // EnsureContent

        #endregion // Protected

		#region Internal

		#region EnsureVisualStates

		/// <summary>
		/// Change the VisualState based on the IsSelected and IsFocused properties.
		/// </summary>
		protected internal override void EnsureVisualStates()
		{
			if (this.Cell.Row != null && this.Cell.Row.ComboEditor != null)
			{
				if (!this.Cell.Row.IsEnabled)
				{
					this.GoToState(false, (this.Content is Control) ? "Normal" : "Disabled");
				}
				else if (this.IsMouseOver || this.Cell.Row.IsMouseOver)
				{
					this.GoToState(true, "MouseOver");
				}
				else
				{
					this.GoToState(true, "Normal");
				}

				if (this.Cell.Row.IsSelected)
				{
					this.GoToState(true, "Selected");
				}
				else
				{
					this.GoToState(true, "Unselected");
				}

				if (this.Cell.Row.IsFocused)
				{
					this.GoToState(true, "Focused");
				}
				else
				{
					this.GoToState(true, "Unfocused");
				}

				if (this.Cell.Row.ComboEditor.CheckBoxVisibility.Equals(Visibility.Visible))
				{
					this.GoToState(false, "CheckBox");
				}
				else
				{
					this.GoToState(false, "Standard");
				}
			}
		}

		#endregion //EnsureVisualStates

		#endregion //Internal

		#endregion // Methods
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