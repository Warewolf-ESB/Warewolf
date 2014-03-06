using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Infragistics.Controls.Editors.Primitives;
using System.Windows;

namespace Infragistics.Controls.Editors
{
	/// <summary>
	/// The base control class for all <see cref="ComboCellBase"/> controls.
	/// </summary>

	[InfragisticsFeature(FeatureName = FeatureInfo.FeatureName_MultiColumnCombo, Version = FeatureInfo.Version_11_2)]

	
	public abstract class ComboCellControlBase : ContentControl
	{
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ComboCellControlBase"/> class.
		/// </summary>
		protected ComboCellControlBase()
		{
			this.Loaded += new RoutedEventHandler(ComboCellControlBase_Loaded);
		}

		#endregion //Constructor

		#region Properties

		#region Public

		#region ContentProvider
		/// <summary>
		/// Resolves the <see cref="ComboColumnContentProviderBase"/> for this <see cref="ComboCellControlBase"/>.
		/// </summary>
		public virtual ComboColumnContentProviderBase ContentProvider
		{
			get { return null; }
		}

		#endregion // ContentProvider

		#region Cell

		/// <summary>
		/// The <see cref="ComboCellBase"/> that owns the <see cref="ComboCellControlBase"/>.
		/// </summary>
		public ComboCellBase Cell
		{
			get;
			protected set;
		}

		#endregion // Cell

		#endregion //Public 

		#region Internal

		#region IsCellLoaded

		internal bool IsCellLoaded
		{
			get;
			set;
		}

		#endregion //IsCellLoaded

		#region IsMeasureRaised
		internal bool IsMeasureRaised
		{
			get;
			set;
		}
		#endregion //IsMeasureRaised

		#endregion // Internal

		#region Protected 



#region Infragistics Source Cleanup (Region)









#endregion // Infragistics Source Cleanup (Region)


		#endregion //Protected

		#endregion //Properties

		#region Methods

		#region Private

		#endregion //Private

		#region Protected

		#region AttachContent

		/// <summary>
		/// Invoked when content is attached to the Control.
		/// </summary>
		protected virtual void AttachContent()
		{
		}

		#endregion // AttachContent

		#region EnsureContent

		/// <summary>
		/// This will get called every time the control is measured, and allows the control to adjust it's content if necessary.
		/// </summary>
		internal protected virtual void EnsureContent()
		{
		}

		#endregion // EnsureContent

		#region EnsureVisualStates

		/// <summary>
		/// Change the VisualState of the cell control.
		/// </summary>
		protected internal virtual void EnsureVisualStates()
		{
		}

		#endregion //EnsureVisualStates

		#region GoToState

		/// <summary>
		/// Sets a new VisualState. 
		/// </summary>
		/// <param name="useTransitions">Specifies if transition is used.</param>
		/// <param name="stateName">The name of the new VisualState.</param>
		protected internal void GoToState(bool useTransitions, string stateName)
		{
			VisualStateManager.GoToState(this, stateName, useTransitions);
		}

		#endregion //GoToState

		#region OnAttached

		/// <summary>
		/// Called when the <see cref="ComboCellBase"/> is attached to the <see cref="ComboCellControlBase"/>.
		/// </summary>
		/// <param propertyName="cell">The <see cref="ComboCellBase"/> that is being attached to the <see cref="ComboCellControlBase"/></param>
		protected internal virtual void OnAttached(ComboCellBase cell)
		{
			this.ClearValue(ComboCellControlBase.DataContextProperty);

			this.Cell = cell;
			this.AttachContent();

			this.EnsureVisualStates();
		}

		#endregion // OnAttached

		#region OnReleased

		/// <summary>
		/// Called when the <see cref="ComboCellBase"/> releases the <see cref="ComboCellControlBase"/>.
		/// </summary>
		protected internal virtual void OnReleased(ComboCellBase cell)
		{
			if (cell != null)
			{
				cell.EnsureCurrentState();
			}

			// Setting this to null will break our inheritance chain to the DataContext of the CellsPanel
			// Which is good, b/c we aren't attached to a cell anymore, so we don't want to get notified when the DataContext changes
			// while vertical scrolling.
			this.DataContext = null;

		}
		#endregion // OnReleased

		#region OnLoaded

		/// <summary>
		/// Raised when the <see cref="ComboCellControlBase"/> is Loaded. 
		/// </summary>
		protected virtual void OnLoaded()
		{
			this.IsCellLoaded = true;
		}

		#endregion // OnLoaded

		#endregion //Protected

		#endregion //Methods

		#region EventHandlers

		#region CellControlBase_Loaded

		private void ComboCellControlBase_Loaded(object sender, RoutedEventArgs e)
		{
			
			//if (this.Cell != null)
			//    this.GoToState(this.Cell.NormalState, false);

			this.OnLoaded();
		}

		#endregion // CellControlBase_Loaded

		#endregion //Event Handlers

		#region Overrides

		#region MeasureOverride

		/// <summary>
		/// Allows a Cell to ensure it was propely measured. 
		/// </summary>
		/// <param name="availableSize"></param>
		/// <returns></returns>
		protected override Size MeasureOverride(Size availableSize)
		{
			this.IsMeasureRaised = true;

			return base.MeasureOverride(availableSize);
		}

		#endregion // MeasureOverride



#region Infragistics Source Cleanup (Region)































#endregion // Infragistics Source Cleanup (Region)

		#region OnPropertyChanged

		/// <summary>
		/// Called when a property value changes.
		/// </summary>
		/// <param name="e">Information about the property value that changed.</param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

            if (e.Property == UIElement.IsMouseOverProperty)
            {
                this.Cell.Row.IsMouseOver = this.IsMouseOver;
				if (this.Cell.Row.Control != null)
				{
                if (this.IsMouseOver)
                    this.Cell.Row.Control.InternalCellMouseEnter();
                else
                    this.Cell.Row.Control.InternalCellMouseLeave();
				}
            }
		}

        #endregion //OnPropertyChanged


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