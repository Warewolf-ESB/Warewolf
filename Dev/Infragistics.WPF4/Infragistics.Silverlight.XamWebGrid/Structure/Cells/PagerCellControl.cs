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
using System.ComponentModel;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// Visual object for the <see cref="PagerCell"/> object.
	/// </summary>
	[TemplatePart(Name = "PagerItemControl", Type = typeof(PagerControlBase))]
	[TemplateVisualState(GroupName = "Orientation", Name = "Top")]
	[TemplateVisualState(GroupName = "Orientation", Name = "Bottom")]
	public class PagerCellControl : CellControlBase, ICommandTarget
	{
		#region Members
		PagerControlBase _pagerControl;
		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="PagerCellControl"/> class.
		/// </summary>
		public PagerCellControl()
		{
			base.DefaultStyleKey = typeof(PagerCellControl);
		}

		#endregion // Constructor

		#region Overrides

		#region AttachContent
		/// <summary>
		/// Builds the visual tree for the <see cref="PagerCellControl"/> when a new template is applied. 
		/// </summary>
		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();
			this._pagerControl = base.GetTemplateChild("PagerItemControl") as PagerControlBase;
			PopulatePagerControlValues();            
		}

		/// <summary>
		/// Invoked when content is attached to the Control.
		/// </summary>
		protected override void AttachContent()
		{
			this.EnsureContent();
		}
		#endregion // AttachContent

		#region PopulatePagerControlValues
		/// <summary>
		/// Sets values on the PagerControl from the Manager
		/// </summary>
		protected internal void PopulatePagerControlValues()
		{
			if (this._pagerControl != null && this.Cell != null)
			{
				RowsManager manager = (RowsManager)this.Cell.Row.Manager;
				this._pagerControl.SetCurrentPageIndexSilent(manager.CurrentPageIndex);
				this._pagerControl.SetRowsManagerMaximumPagesSilent(manager.PageCount);
			}
		}
        #endregion // PopulatePagerControlValues

        #region OnAttached

        /// <summary>
        /// Called when the <see cref="CellBase"/> is attached to the <see cref="CellControlBase"/>.
        /// </summary>
        /// <param propertyName="cell">The <see cref="CellBase"/> that is being attached to the <see cref="CellControlBase"/></param>        
        protected internal override void OnAttached(CellBase cell)
        {
            base.OnAttached(cell);
            RowsManager manager = this.Cell.Row.Manager as RowsManager;
            if (manager != null)
            {
                IPagedCollectionView ipcv = manager.ItemsSource as IPagedCollectionView;
                if (ipcv != null)
                {
                    Binding b = new Binding("CanChangePage");
                    b.Source = ipcv;
                    this.SetBinding(Control.IsEnabledProperty, b);
                }
            }
        }

        #endregion // OnAttached

        #region OnReleased

        /// <summary>
        /// Called when the <see cref="CellBase"/> releases the <see cref="CellControlBase"/>.
        /// </summary>
        protected internal override void OnReleased(CellBase cell)
        {
            base.OnReleased(cell);
            this.ClearValue(Control.IsEnabledProperty);
        }

        #endregion // OnReleased

        #region EnsureContent
        /// <summary>
		/// This will get called every time the control is measured, and allows the control to adjust it's content if necessary.
		/// </summary>
		protected internal override void EnsureContent()
		{
			base.EnsureContent();
			PopulatePagerControlValues();
			if (this._pagerControl != null)
			{
				this._pagerControl.EnsureContent();
			}
			RowsManager rowsManager = (RowsManager)this.Cell.Row.Manager;
			if (-1 != rowsManager.RegisteredBottomRows.IndexOf(this.Cell.Row))
			{
				VisualStateManager.GoToState(this, "Bottom", true);
			}
			else if (-1 != rowsManager.RegisteredTopRows.IndexOf(this.Cell.Row))
			{
				VisualStateManager.GoToState(this, "Top", true);
			}
		}
		#endregion // EnsureContent

		#endregion // Overrides

		#region ICommandTarget Members

		/// <summary>
		/// Returns if the object will support a given command type.
		/// </summary>
		/// <param propertyName="command">The command to be validated.</param>
		/// <returns>True if the object recognizes the command as actionable against it.</returns>
		bool ICommandTarget.SupportsCommand(ICommand command)
		{
			return this.SupportsCommand(command);
		}

		/// <summary>
		/// Returns the object that defines the parameters necessary to execute the command.
		/// </summary>
		/// <param propertyName="source">The CommandSource object which defines the command to be executed.</param>
		/// <returns>The object necessary for the command to complete.</returns>
		object ICommandTarget.GetParameter(CommandSource source)
		{
			return this.GetParameter(source);
		}

		#endregion

		#region Methods
		#region Protected
		#region SupportsCommand
		/// <summary>
		/// Determines if the <see cref="PagerCellControl"/> supports the inputted <see cref="ICommand"/> object.
		/// </summary>
		/// <param propertyName="command">The <see cref="ICommand"/> command that may be run against the <see cref="PagerCellControl"/>.</param>
		/// <returns>True if the command can be executed.</returns>
		protected virtual bool SupportsCommand(ICommand command)
		{
			return command is PagingBaseCommand;
		}
		#endregion // SupportsCommand

		#region GetParameter
		/// <summary>
		/// Returns the object that defines the parameters necessary to execute the command.
		/// </summary>
		/// <param propertyName="source">The CommandSource object which defines the command to be executed.</param>
		/// <returns>The object necessary for the command to complete.</returns>
		protected virtual object GetParameter(CommandSource source)
		{
			if (source.Command is GoToPageCommand )
			{
				if (source.Parameter != null)
					return new object[] { Cell.Row.Manager, source.Parameter };

				PagingEventArgs pagingEventArgs = source.OriginEventArgs as PagingEventArgs;
				if (pagingEventArgs != null)
				{
					return new object[] { Cell.Row.Manager, pagingEventArgs.NextPage };
				}
			}
			
			return Cell.Row.Manager;
		}
		#endregion // GetParameter
		#endregion // Protected
		#endregion // Methods

		#region Properties

		#region Protected
		#region PagerControl
		/// <summary>
		/// Gets the underlying PagerControlBase object
		/// </summary>
		protected internal PagerControlBase PagerControl
		{
			get
			{
				return this._pagerControl;
			}
		}
		#endregion  // PagerControl
		#endregion  // Protected
		#endregion  // Properties
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