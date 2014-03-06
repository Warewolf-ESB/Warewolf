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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Infragistics.Persistence
{
	/// <summary>
	/// An object used to group multiple <see cref="DependencyObject"/>s so that they maybe Saved and Loaded at the same time.
	/// </summary>
	public class PersistenceGroup
	{
		#region Members
		
		List<DependencyObject> _registeredObjects;
		ReadOnlyCollection<DependencyObject> _objects;
		PersistenceEvents _events;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Creates a new instances of the <see cref="PersistenceGroup"/> class.
		/// </summary>
		public PersistenceGroup()
		{
			this._registeredObjects = new List<DependencyObject>();
			this._objects = new ReadOnlyCollection<DependencyObject>(this._registeredObjects);
		}

		#endregion // Constructor

		#region Properties

		#region RegisteredObjects
		/// <summary>
		/// Gets a ReadOnly collection of <see cref="DependencyObject"/>s that this <see cref="PersistenceGroup"/> will save or load.
		/// </summary>
		public ReadOnlyCollection<DependencyObject> RegisteredObjects
		{
			get
			{
				return this._objects;
			}
		}
		#endregion // RegisteredObjects

		#region Events

		/// <summary>
		/// Gets/Sets the events that will be raised while saving and loading this <see cref="PersistenceGroup"/>
		/// </summary>
		public PersistenceEvents Events
		{
			get
			{
				if (this._events == null)
				{
					this._events = new PersistenceEvents();
				}

				return this._events;
			}
			set
			{
				this._events = value;
			}

		}

		#endregion // Events 

		#endregion // Properties

		#region Methods

		#region Internal

		internal void RegisterObject(DependencyObject obj)
		{
			this._registeredObjects.Add(obj);
		}

		internal void UnregisterObject(DependencyObject obj)
		{
			this._registeredObjects.Remove(obj);
		}


		#endregion // Internal

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