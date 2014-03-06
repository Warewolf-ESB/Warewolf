using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Automation;
using Infragistics.Windows.Controls;
using System.Windows.Controls;

namespace Infragistics.Windows.Automation.Peers
{
	/// <summary>
	/// Exposes the <see cref="CarouselListBoxItem"/> to UI Automation
	/// </summary>
	public class CarouselListBoxItemAutomationPeer : RecycleableItemAutomationPeer,
		IScrollItemProvider,
		ISelectionItemProvider
	{

		#region Constructor
		/// <summary>
		/// Initializes a new <see cref="CarouselListBoxItemAutomationPeer"/>
		/// </summary>
		/// <param name="item">The item to be represented by the automation peer</param>
		/// <param name="listAutomationPeer">The automation peer associated with the containing <see cref="XamCarouselListBox"/></param>
		public CarouselListBoxItemAutomationPeer(object item, XamCarouselListBoxAutomationPeer listAutomationPeer)
			: base(item, listAutomationPeer)
		{
		} 
		#endregion //Constructor

		#region Base class overrides

		#region GetClassNameCore
		/// <summary>
		/// Returns the name of the <see cref="CarouselListBoxItem"/>
		/// </summary>
		/// <returns>A string that contains 'CarouselListBoxItem'</returns>
		protected override string GetClassNameCore()
		{
			return "CarouselListBoxItem";
		}

				#endregion //GetClassNameCore

		#region GetPattern
		/// <summary>
		/// Returns the control pattern associated with the specified <see cref="PatternInterface"/> for the associated <see cref="XamCarouselListBox"/>.
		/// </summary>
		/// <param name="patternInterface">The pattern being requested</param>
		public override object GetPattern(PatternInterface patternInterface)
		{
			if (patternInterface == PatternInterface.SelectionItem)
			{
				return this;
			}
			else if (patternInterface == PatternInterface.ScrollItem)
			{
				return this;
			}

			return base.GetPattern(patternInterface);
		}
		#endregion //GetPattern

		#endregion //Base class overrides

		#region Properties
		private XamCarouselListBox List
		{
			// JM 08-20-09 NA 9.2 EnhancedGridView
			//get { return (XamCarouselListBox)this.ItemsControlAutomationPeer.Owner; }
			get { return (XamCarouselListBox)this.ListAutomationPeer.Owner; }
		}
		#endregion //Properties

		#region ISelectionItemProvider Members

		void ISelectionItemProvider.AddToSelection()
		{
			this.ThrowIfNotEnabled();


			// JM 08-20-09 NA 9.2 EnhancedGridView
			//ISelectionProvider provider = this.ItemsControlAutomationPeer.GetPattern(PatternInterface.Selection) as ISelectionProvider;
			ISelectionProvider provider = this.ListAutomationPeer.GetPattern(PatternInterface.Selection) as ISelectionProvider;

			if (null != provider &&
				provider.CanSelectMultiple == false &&
				provider.GetSelection() != null)
			{
				throw new InvalidOperationException();
			}

			this.List.SelectedItem = this.Item;
		}

		bool ISelectionItemProvider.IsSelected
		{
			get { return this.Item == this.List.SelectedItem; }
		}

		void ISelectionItemProvider.RemoveFromSelection()
		{
			this.ThrowIfNotEnabled();

			if (((ISelectionItemProvider)this).IsSelected)
				this.List.SelectedItem = null;
		}

		void ISelectionItemProvider.Select()
		{
			ThrowIfNotEnabled();

			this.List.SelectedItem = this.Item;
		}

		IRawElementProviderSimple ISelectionItemProvider.SelectionContainer
		{
			// JM 08-20-09 NA 9.2 EnhancedGridView
			//get { return this.ProviderFromPeer(this.ItemsControlAutomationPeer); }
			get { return this.ProviderFromPeer(this.ListAutomationPeer as AutomationPeer); }
		}

		#endregion

		#region IScrollItemProvider Members

		void IScrollItemProvider.ScrollIntoView()
		{
			// JM 08-20-09 NA 9.2 EnhancedGridView
			//Panel panel = this.ItemsControlAutomationPeer.ItemsControlPanel;
			Panel panel = this.ListAutomationPeer.ItemsControlPanel;

			if (panel is XamCarouselPanel)
			{
				int index = this.List.Items.IndexOf(this.Item);
				((XamCarouselPanel)panel).EnsureItemIsVisible(index);
			}
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