using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;

namespace Infragistics.Windows.Helpers
{
	/// <summary>
	/// Class used to support weak event listening for the IBindingList.ListChanged event
	/// </summary>
	public sealed class BindingListChangedEventManager : WeakEventManager
	{
		private BindingListChangedEventManager(){}

		#region Static Methods

			#region Instance

		private static BindingListChangedEventManager Instance
		{
			get
			{
				Type mType = typeof(BindingListChangedEventManager);

				BindingListChangedEventManager manager = (BindingListChangedEventManager)WeakEventManager.GetCurrentManager(mType);

				if (manager == null)
				{
					manager = new BindingListChangedEventManager();
					WeakEventManager.SetCurrentManager(mType, manager);
				}

				return manager;

			}
		}

			#endregion //Instance	

			#region AddListener

		/// <summary>
		/// Static method used to add a listener to a source for the event.
		/// </summary>
		/// <param name="bindingList">The source of the event.</param>
		/// <param name="listener">An object that implements the IWeakEventListener interface to receive the event.</param>
		public static void AddListener(IBindingList bindingList, IWeakEventListener listener)
		{
			Instance.ProtectedAddListener(bindingList, listener);
		}

			#endregion //AddListener	
    
			#region RemoveListener

		/// <summary>
		/// Static method used to remove a listener to a source for the event.
		/// </summary>
		/// <param name="bindingList">The source of the event.</param>
		/// <param name="listener">An object that implements the IWeakEventListener interface to receive the event.</param>
		public static void RemoveListener(IBindingList bindingList, IWeakEventListener listener)
		{
			Instance.ProtectedRemoveListener(bindingList, listener);
		}

			#endregion //RemoveListener	
        
		#endregion //static Methods	
    
		#region Base class overrides

			#region StartListening

		/// <summary>
		/// Called to wire up the underlying event.
		/// </summary>
		/// <param name="source">The source of the event</param>
		protected override void StartListening(object source)
		{
			IBindingList list = source as IBindingList;

			list.ListChanged += new ListChangedEventHandler(OnListChanged);
		}

			#endregion //StartListening	
    
			#region StopListening

		/// <summary>
		/// Called to un-wire the underlying event.
		/// </summary>
		/// <param name="source">The source of the event</param>
		protected override void StopListening(object source)
		{
			IBindingList list = source as IBindingList;

			list.ListChanged -= new ListChangedEventHandler(OnListChanged);
		}

			#endregion //StopListening	
    
		#endregion //Base class overrides	
    
		#region Event handler

		void OnListChanged(object sender, ListChangedEventArgs e)
		{
			base.DeliverEvent(sender, e);
		}

		#endregion //Event handler	
    
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