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
using System.Collections;
using System.ComponentModel;
using System.Collections.Specialized;

namespace Infragistics.Controls.Grids.Primitives
{
	/// <summary>
	/// An object that wraps an underlying collection of type <see cref="ICollectionBase"/> that contains a collection of <see cref="RowBase"/> objects.
	/// </summary>
	public class RowBaseCollection : INotifyPropertyChanged
	{
		#region Members

		ICollectionBase _actualCollection;

		#endregion // Members

		#region Constructor

		/// <summary>
		/// Creates a new instance of the <see cref="RowBaseCollection"/> class. 
		/// </summary>
		/// <param propertyName="actualCollection">The collection that the <see cref="RowBaseCollection"/> will wrap.</param>
		public RowBaseCollection(ICollectionBase actualCollection)
		{
			this._actualCollection = actualCollection;
			this._actualCollection.CollectionChanged += new NotifyCollectionChangedEventHandler(ActualCollection_CollectionChanged);
		}

		#endregion // Constructor

		#region Properties

		/// <summary>
		/// The underlying collection that the <see cref="RowBaseCollection"/> is wrapping.
		/// </summary>
		public ICollectionBase ActualCollection
		{
			get { return this._actualCollection; }
		}

		/// <summary>
		/// Gets the total amount of <see cref="RowBase"/> objects.
		/// </summary>
		public int Count
		{
			get
			{
				return this._actualCollection.Count;
			}
		}

		/// <summary>
		/// Gets the <see cref="RowBase"/> at the specified index.
		/// </summary>
		/// <param propertyName="index"></param>
		/// <returns></returns>
		public RowBase this[int index]
		{
			get
			{
				return this._actualCollection[index] as RowBase;
			}
		}

		#endregion // Properties

		#region Methods

		/// <summary>
		/// Gets the index of the specified <see cref="RowBase"/> from the underlying collection.
		/// </summary>
		/// <param propertyName="item"></param>
		/// <returns></returns>
		public int IndexOf(RowBase item)
		{
			return this._actualCollection.IndexOf(item);
		}

		/// <summary>
		/// Removes all <see cref="RowBase"/> objects from the underlying collection.
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]		
		public virtual void Clear()
		{
			this._actualCollection.Clear();
		}

		/// <summary>
		/// Gets the IEnumerator from the underlying collection.
		/// </summary>
		/// <returns></returns>
		public IEnumerator GetEnumerator()
		{
			return this._actualCollection.GetEnumerator();
		}

		#endregion // Methods

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Fired when a property changes on the <see cref="DependencyObjectNotifier"/>.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Invoked when a property changes on the <see cref="DependencyObjectNotifier"/> object.
		/// </summary>
		/// <param name="name">The name of the property that has changed.</param>
		protected virtual void OnPropertyChanged(string name)
		{
			if (this.PropertyChanged != null)
				this.PropertyChanged(this, new PropertyChangedEventArgs(name));
		}

		#endregion

		#region EventHandlers

		void ActualCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			this.OnPropertyChanged("Count");
		}

		#endregion // EventHandlers
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