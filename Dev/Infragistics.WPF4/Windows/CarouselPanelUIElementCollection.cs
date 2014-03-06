using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Infragistics.Windows.Controls
{
	#region CarouselPanelUIElementCollection Internal Class

	internal class CarouselPanelUIElementCollection : UIElementCollection
	{
		#region Member Variables

		private XamCarouselPanel						_carouselPanel = null;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Creates an instance of the CarouselPanelUIElementCollection class.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>This class is for internal use only.</p>
		/// </remarks>
		public CarouselPanelUIElementCollection(XamCarouselPanel carouselPanel, UIElement visualParent, FrameworkElement logicalParent)
			: base(visualParent, logicalParent)
		{
			this._carouselPanel = carouselPanel;
		}

		#endregion //Constructor

		#region Base Class Overrides

			#region Add

		/// <summary>
		/// Adds an element to the collection.
		/// </summary>
		/// <param name="element">The element to add</param>
		/// <returns></returns>
		public override int Add(UIElement element)
		{
			if (this.CarouselPanel.IsInCarouselListBox)
			{
				CarouselListBoxItem carouselListBoxItem = new CarouselListBoxItem(this.CarouselPanel);
				carouselListBoxItem.Content				= element;

				return base.Add(carouselListBoxItem);
			}

			
			CarouselPanelItem carouselPanelItem = new CarouselPanelItem(this.CarouselPanel);
			carouselPanelItem.Content			= element;

			// JM 11-28-07 BR28764
			//return base.Add(carouselPanelItem);
			int index = base.Add(carouselPanelItem);
			this.RaiseCollectionChanged();
			return index;
		}

			#endregion //Add

			// JM 11-28-07 BR28764
			#region Clear

		public override void Clear()
		{
			base.Clear();
			this.RaiseCollectionChanged();
		}

			#endregion //Clear	
    
			#region GetEnumerator

		/// <summary>
		/// Returns an IEnumerator that can be used to iterate through the collection.
		/// </summary>
		/// <returns></returns>
		public override IEnumerator GetEnumerator()
		{
			return base.GetEnumerator();
		}

			#endregion //GetEnumerator	
    
			// JM 11-28-07 BR28764
			#region Insert

		public override void Insert(int index, UIElement element)
		{
			base.Insert(index, element);
			this.RaiseCollectionChanged();
		}

			#endregion //Insert	
    
			// JM 11-28-07 BR28764
			#region Remove

		public override void Remove(UIElement element)
		{
			base.Remove(element);
			this.RaiseCollectionChanged();
		}

			#endregion //Remove	
    
			// JM 11-28-07 BR28764
			#region RemoveAt

		public override void RemoveAt(int index)
		{
			base.RemoveAt(index);
			this.RaiseCollectionChanged();
		}

			#endregion //RemoveAt	
    
			// JM 11-28-07 BR28764
			#region RemoveRange

		public override void RemoveRange(int index, int count)
		{
			base.RemoveRange(index, count);
			this.RaiseCollectionChanged();
		}

			#endregion //RemoveRange	
    
		#endregion //Base Class Overrides

		#region Properties

			#region Internal Properties

				#region CarouselPanel

		internal XamCarouselPanel CarouselPanel
		{
			get { return this._carouselPanel; }
		}

				#endregion //CarouselPanel	
    
			#endregion //Internal Properties
    
		#endregion //Properties	

		#region Events

			// JM 11-28-07 BR28764
			#region CollectionChanged (internal CLR event)

		internal event EventHandler<EventArgs> CollectionChanged;

			#endregion //CollectionChanged (internal CLR event)

		#endregion //Events

		#region Methods

			// JM 11-28-07 BR28764
			#region RaiseCollectionChanged

		private void RaiseCollectionChanged()
		{
			if (this.CollectionChanged != null)
				CollectionChanged(this, EventArgs.Empty);
		}

			#endregion //RaiseCollectionChanged

		#endregion //Methods
	}

	#endregion //CarouselPanelUIElementCollection Internal Class
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