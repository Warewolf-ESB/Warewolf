using System;
using System.Net;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Infragistics.AutomationPeers;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// A base class that should be used for <see cref="RowBase"/> objects that can be expandable. 
	/// </summary>
	public abstract class ExpandableRowBase : RowBase
	{
		#region Members

		bool _expanded;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ExpandableRowBase"/> class.
		/// </summary>
		/// <param propertyName="manager">The <see cref="RowsManagerBase"/> that owns the <see cref="ExpandableRowBase"/>.</param>		
		protected internal ExpandableRowBase(RowsManagerBase manager):base	(manager)
        {
        }

        #endregion // Constructor

		#region Properties

		#region Public 

		#region IsExpanded

		/// <summary>
		/// Gets/sets whether the <see cref="ExpandableRowBase"/> is expanded or collapsed.
		/// </summary>
		public bool IsExpanded
		{
			get
			{

				return this._expanded;
			}
			set
			{
				if (this._expanded != value)
				{
				   if (!this.ColumnLayout.Grid.OnRowExpansionChanging(this))
				    {
				     this._expanded = value;

						if (value && this.HasChildren)
							this.Manager.RegisterChildRowsManager(this.ChildRowsManager);
						else
							this.Manager.UnregisterChildRowsManager(this.ChildRowsManager);

				        this.OnPropertyChanged("Expanded");
				        this.ColumnLayout.Grid.OnRowExpansionChanged(this);


				    }
				    this.ColumnLayout.Grid.InvalidateScrollPanel(true);

				}
			}
		}

		#endregion // IsExpanded

		#region HasChildren

		/// <summary>
		/// Gets whether or not <see cref="ExpandableRowBase"/> has any child rows.
		/// </summary>
		public abstract bool HasChildren
		{
			get;
		}

		#endregion // HasChildren

		#region ChildRowsManager

		/// <summary>
		/// The <see cref="RowsManagerBase"/> of the <see cref="ExpandableRowBase"/>'s children.
		/// </summary>
		protected internal abstract RowsManagerBase ChildRowsManager
		{
			get;
			set;
		}

		#endregion // ChildRowsManager

		#endregion // Public

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