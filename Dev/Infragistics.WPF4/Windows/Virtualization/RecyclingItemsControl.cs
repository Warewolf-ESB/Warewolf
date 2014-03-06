using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using Infragistics.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Diagnostics;
using Infragistics.Windows.Automation.Peers;
using System.Windows.Automation.Peers;

namespace Infragistics.Windows.Virtualization
{
	#region RecyclingItemsControl abstract base class

	/// <summary>
	/// An abstract base class for ItemsControls that are interested in recycling elements. 
	/// </summary>
	/// <remarks>
	/// <para class="note"><b>Note:</b> Using the <see cref="RecyclingItemsControl.RecyclingItemContainerGenerator"/> instead of ItemControl's ItemContainerGenerator will require that the 
	/// derived class implement its own navigation logic. The <see cref="RecyclingItemsControl.RecyclingItemContainerGenerator"/> will also not support grouping thru the Items collection.</para>
	/// </remarks>
	/// <seealso cref="Infragistics.Windows.Virtualization.RecyclingItemContainerGenerator"/>
	/// <seealso cref="RecyclingItemsControl.RecyclingItemContainerGenerator"/>
	/// <seealso cref="RecyclingItemsPanel"/>
	/// <seealso cref="ItemContainerGenerationMode"/>
	public abstract class RecyclingItemsControl : ItemsControl
	{
		#region Private Members

		private RecyclingItemContainerGenerator _recyclingGenerator;

		// JM BR28932 11-13-07
		private DependencyProperty				_navigationServiceDependencyProperty;

		#endregion //Private Members

		#region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		protected RecyclingItemsControl()
		{
			this._recyclingGenerator = new RecyclingItemContainerGenerator(this);
		}

		#endregion //Constructor

		#region Base class overrides

		// AS 6/8/07 UI Automation
		#region OnItemsChanged
		/// <summary>
		/// Overridden. Invoked when the contents of the items collection has changed.
		/// </summary>
		/// <param name="e">Event arguments indicating the change that occurred.</param>
		protected override void OnItemsChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
		{
			base.OnItemsChanged(e);

			RecyclingItemsControlAutomationPeer peer = UIElementAutomationPeer.FromElement(this) as RecyclingItemsControlAutomationPeer;

			if (null != peer)
				peer.OnItemsChanged(e);
		}
		#endregion //OnItemsChanged

		// AS 6/8/07 UI Automation
		#region OnItemsPanelChanged
		/// <summary>
		/// Overriden. Invoked when the <see cref="ItemsControl.ItemsPanel"/> property changes.
		/// </summary>
		/// <param name="oldItemsPanel">The old panel template</param>
		/// <param name="newItemsPanel">The new panel template</param>
		protected override void OnItemsPanelChanged(ItemsPanelTemplate oldItemsPanel, ItemsPanelTemplate newItemsPanel)
		{
			base.OnItemsPanelChanged(oldItemsPanel, newItemsPanel);

			RecyclingItemsControlAutomationPeer peer = UIElementAutomationPeer.FromElement(this) as RecyclingItemsControlAutomationPeer;

			if (null != peer)
				peer.OnItemsPanelChanged();
		}
		#endregion //OnItemsPanelChanged

		// AS 9/12/07
		// We should let the automation peer know when the items source has changed since
		// it could be caching information - like we do in the datapresenter's peer.
		//
		#region OnItemsSourceChanged
		/// <summary>
		/// Overridden. Invoked when the ItemsSource property changes.
		/// </summary>
		/// <param name="oldValue">Old value of the ItemsSource property</param>
		/// <param name="newValue">New value of the ItemsSource property</param>
		protected override void OnItemsSourceChanged(System.Collections.IEnumerable oldValue, System.Collections.IEnumerable newValue)
		{
			base.OnItemsSourceChanged(oldValue, newValue);

			RecyclingItemsControlAutomationPeer peer = UIElementAutomationPeer.FromElement(this) as RecyclingItemsControlAutomationPeer;

			if (null != peer)
				peer.OnItemsSourceChanged(oldValue, newValue);
		}
		#endregion //OnItemsSourceChanged

			// JM BR28932 11-13-07
			#region OnPropertyChanged

        /// <summary>
        /// Called when a property changes.
        /// </summary>
        /// <param name="e">A DependencyPropertyChangedEventArgs instance that contains information about the property that changed.</param>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			bool propertyIsNavigationServiceProperty = false;
			if (this._navigationServiceDependencyProperty == null)
			{
				if (e.Property.Name == "NavigationService")
				{
					this._navigationServiceDependencyProperty	= e.Property;
					propertyIsNavigationServiceProperty			= true;
				}
			}
			else
				propertyIsNavigationServiceProperty = (e.Property == this._navigationServiceDependencyProperty);

			if (propertyIsNavigationServiceProperty &&
				e.OldValue == null					&&
				e.NewValue != null					&&
				this.ItemContainerGenerationMode == ItemContainerGenerationMode.Recycle)
			{
                // JJD 5/3/10 - TFS31411
                // Moved logic to virtual method
				//this.RecyclingItemContainerGenerator.RemoveAll();
				this.OnNavigationServiceInitialized();
			}

			base.OnPropertyChanged(e);
		}

			#endregion //OnPropertyChanged

		#endregion //Base class overrides

		#region Properties

			#region Public Properties

				#region ItemContainerGenerationMode

		/// <summary>
		/// Identifies the <see cref="ItemContainerGenerationMode"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ItemContainerGenerationModeProperty = DependencyProperty.Register("ItemContainerGenerationMode",
			typeof(ItemContainerGenerationMode), typeof(RecyclingItemsControl), 
			new FrameworkPropertyMetadata(ItemContainerGenerationMode.Recycle));

		/// <summary>
		/// Gets/sets how item containers are generated and cached.
		/// </summary>
		/// <seealso cref="ItemContainerGenerationModeProperty"/>
		//[Description("Gets/sets how item containers are generated and cached.")]
		//[Category("Behavior")]
		[DefaultValue(ItemContainerGenerationMode.Recycle)]
		public ItemContainerGenerationMode ItemContainerGenerationMode
		{
			get
			{
				return (ItemContainerGenerationMode)this.GetValue(RecyclingItemsControl.ItemContainerGenerationModeProperty);
			}
			set
			{
				this.SetValue(RecyclingItemsControl.ItemContainerGenerationModeProperty, value);
			}
		}

				#endregion //ItemContainerGenerationMode

				#region RecyclingItemContainerGenerator

		/// <summary>
		/// An alternative generator used by Panels interested in recycling elements. 
		/// </summary>
		/// <remarks>
		/// <para class="note"><b>Note:</b> Using this generator instead of ItemControl's ItemContainerGenerator will require that the 
		/// derived class implement its own navigation logic and create its own automation peer. This generator will also not support grouping thru the Items collection.</para>
		/// </remarks>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[Bindable(false)]
		[Browsable(false)]
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		public RecyclingItemContainerGenerator RecyclingItemContainerGenerator { get { return this._recyclingGenerator; } }

				#endregion //RecyclingItemContainerGenerator

			#endregion //Public Properties
			
            #region Protected Properties

				// MD 3/16/11 - TFS24163
				#region MaxDeactivatedContainersWithoutIndexes

		/// <summary>
		/// Gets the maximum number of deeactivated containers without indexes which can remain in the control.
		/// </summary>
		protected internal virtual int MaxDeactivatedContainersWithoutIndexes { get { return 0; } } 

				#endregion // MaxDeactivatedContainersWithoutIndexes

                // JJD 05/07/10 - TFS31643 - added
				#region UnderlyingItemsCount

		/// <summary>
		/// Returns the number of items in the underlying source. 
		/// </summary>
		/// <remarks>
        /// <para class="note"><b>Note:</b> derived classes that append additional items (e.g. header records) can override this property to return the number of items not counting the appended ones.</para>
		/// </remarks>
        /// <value>The default implementation returns the count of the Items collection.</value>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal protected virtual int UnderlyingItemsCount { get { return this.Items.Count; } }

				#endregion //UnderlyingItemsCount

			#endregion //Protected Properties


		#endregion //Properties

		#region Methods

			#region Internal Methods

				#region ClearContainerForItemInternal

		internal void ClearContainerForItemInternal(DependencyObject container, object item)
		{
			this.ClearContainerForItemOverride(container, item);
		}

				#endregion //ClearContainerForItemInternal	
		    
				#region GetContainerForItemInternal

		
		
		
		
		
		
		
		
		internal DependencyObject GetContainerForItemInternal( object item )
		{
			return this.GetContainerForItemOverride( item );
		}
		
#region Infragistics Source Cleanup (Region)


#endregion // Infragistics Source Cleanup (Region)


				#endregion //GetContainerForItemInternal	
		    
				#region IsItemItsOwnContainerInternal
	
		internal bool IsItemItsOwnContainerInternal(object item)
		{
			return this.IsItemItsOwnContainerOverride(item);
		}

				#endregion //IsItemItsOwnContainerInternal	
		    
				#region PrepareContainerForItemInternal

		internal void PrepareContainerForItemInternal(DependencyObject container, object item)
		{
			this.PrepareContainerForItemOverride(container, item);
		}

				#endregion //PrepareContainerForItemInternal
		    
				// JJD 8/24/11 - TFS84215 - added
				#region ShouldApplyItemContainerStyleInternal

		internal bool ShouldApplyItemContainerStyleInternal(DependencyObject container, object item)
		{
			return this.ShouldApplyItemContainerStyle(container, item);
		}

				#endregion //ShouldApplyItemContainerStyleInternal

			#endregion //Internal Methods

		#region Protected Methods

		#region DeactivateContainer

		/// <summary>
		/// Called when a container is being deactivated.
		/// </summary>
		/// <param name="container">The container being deactivated.</param>
		/// <param name="item">Its associated item.</param>
		/// <remarks>
		/// <para class="note"><b>Note:</b> the Visibility property of the container will be set to 'Collapsed' before this method is called. 
		/// The original setting for the Visibility property will be restored before a subsequent call to <see cref="ReactivateContainer"/> or <see cref="ReuseContainerForNewItem"/>.</para>
		/// </remarks>
		/// <seealso cref="ReactivateContainer"/>
		/// <seealso cref="ReuseContainerForNewItem"/>
		protected internal virtual void DeactivateContainer(DependencyObject container, object item) { }

				#endregion //DeactivateContainer

				#region GetContainerForItemOverride

		
		
		
		
		
		
		
		
		/// <summary>
		/// Creates the container to wrap an item.
		/// </summary>
		/// <param name="item">Item for which to create the container.</param>
		/// <returns>The newly created container</returns>
		protected virtual System.Windows.DependencyObject GetContainerForItemOverride( object item )
		{
			// Call the other overload so current (and also typical since that's what the items control
			// has) implementations that are overriding parameterless GetContainerForItemOverride continue 
			// to work.
			// 
			return this.GetContainerForItemOverride( );
		}

				#endregion // GetContainerForItemOverride

				#region IsContainerCompatibleWithItem

		/// <summary>
		/// Determines if a container can be reused for a specific item.
		/// </summary>
		/// <param name="container">The container to be reused.</param>
		/// <param name="item">The potential new item.</param>
		/// <returns>True if the container can be reused for the item</returns>
		/// <remarks>
		/// <para class="body">When looking for a suitable container for an item the generator will search its cache and call this method to see 
		/// if one of its cached containers is compatible with the item. If this method returns true then the container is assigned to the item and 
		/// the <see cref="ReuseContainerForNewItem"/> method is called.
		/// </para>
		/// <para class="note"><b>Note:</b> the default implementation always returns true.</para>
		/// </remarks>
		/// <seealso cref="ReuseContainerForNewItem"/>
		protected internal virtual bool IsContainerCompatibleWithItem(DependencyObject container, object item)
		{
			return true;
		}

				#endregion //IsContainerCompatibleWithItem

                // JJD 05/07/10 - TFS31643 - added
				#region IsStillValid

        /// <summary>
        /// Deterimines whether the container and item are still valid
        /// </summary>
        /// <param name="container">The container associated with the item.</param>
        /// <param name="item">Its associated item.</param>
        /// <returns>True if still valid. The default is null.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal protected virtual bool? IsStillValid(DependencyObject container, object item){ return null; }

				#endregion //IsStillValid

                // JJD 5/3/10 - TFS31411 - added
                #region OnNavigationServiceInitialized

        /// <summary>
        /// Called when the NavigationService has been set to a non-null value
        /// </summary>
        protected virtual void OnNavigationServiceInitialized()
        {
            this.RecyclingItemContainerGenerator.RemoveAll();
        }

                #endregion //OnNavigationServiceInitialized	
    
				#region ReactivateContainer

		/// <summary>
		/// Called when a container is being reactivated for the same item.
		/// </summary>
		/// <param name="container">The container being reactivated.</param>
		/// <param name="item">Its associated item.</param>
		/// <remarks>
		/// <para class="note"><b>Note:</b> the original setting for the Visibility property, prior to its deactivation (refer to <see cref="DeactivateContainer"/>), will be restored before a this method is called.</para>
		/// </remarks>
		/// <seealso cref="DeactivateContainer"/>
		/// <seealso cref="ReuseContainerForNewItem"/>
		protected internal virtual void ReactivateContainer(DependencyObject container, object item) { }

				#endregion //ReactivateContainer

				#region ReuseContainerForNewItem

		/// <summary>
		/// Called when a container is being reused, i.e. recycled, or a different item.
		/// </summary>
		/// <param name="container">The container being reused/recycled.</param>
		/// <param name="item">The new item.</param>
		/// <remarks>
		/// <para class="note"><b>Note:</b> if the container had previously been deactivated then the original setting for the Visibility property, prior to its deactivation (refer to <see cref="DeactivateContainer"/>), will be restored before a this method is called.</para>
		/// </remarks>
		/// <seealso cref="DeactivateContainer"/>
		/// <seealso cref="ReactivateContainer"/>
		protected internal virtual void ReuseContainerForNewItem(DependencyObject container, object item)
		{
			FrameworkElement fe = container as FrameworkElement;

			if (fe == null)
				return;

			if (fe is ContentControl)
			{
				((ContentControl)fe).Content = item;
				return;
			}
			if (fe is ContentPresenter)
			{
				((ContentPresenter)fe).Content = item;
				return;
			}
		}

				#endregion //ReuseContainerForNewItem

                // JJD 3/11/10 - TFS28705 - Optimization - added
				#region ShouldDeactivateContainer

		/// <summary>
		/// Called after a Reset notification to determine if the Container should be de-activated instead of cleared.
		/// </summary>
		/// <param name="container">The container to be deactivated or cleared.</param>
		/// <param name="item">Its associated item.</param>
        /// <returns>True to de-activate the cointainer after a reset or false to clear it. The default is false.</returns>
        [EditorBrowsable( EditorBrowsableState.Never )]
        protected internal virtual bool ShouldDeactivateContainer(DependencyObject container, object item) { return false; }

				#endregion //ShouldDeactivateContainer

			#endregion //Protected Methods

		#endregion //Methods
	}

	#endregion //RecyclingItemsControl abstract base class
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