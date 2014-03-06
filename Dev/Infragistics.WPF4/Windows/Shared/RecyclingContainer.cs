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
using System.ComponentModel;

namespace Infragistics
{
	/// <summary>
	/// A base class for objects that want to support recycling.
	/// </summary>
	/// <typeparam name="T">The type of <see cref="FrameworkElement"/> that this object represents.</typeparam>
    public abstract class RecyclingContainer<T> : ISupportRecycling, INotifyPropertyChanged where T : FrameworkElement
    {
        #region Static

        private static Type _recyclingElementType = typeof(T);

        #endregion // Static

        #region Properties

        #region Protected

        #region RecyclingElementType

        /// <summary>
        /// Gets the <see cref="Type"/> of the <see cref="FrameworkElement"/> that is being recycled.
        /// </summary>
        protected virtual Type RecyclingElementType
        {
            get
            {
                return RecyclingContainer<T>._recyclingElementType;
            }
        }
        #endregion // RecyclingElementType

        #region RecyclingIdentifier

        /// <summary>
        /// If a <see cref="RecyclingElementType"/> isn't specified, this property can be used to offer another way of identifying 
        /// a reyclable element.
        /// </summary>
        protected virtual string RecyclingIdentifier
        {
            get
            {
                return null;
            }
        }
        #endregion // RecyclingElementType

        #region IsDirty

        /// <summary>
        /// Gets/sets a value that determines if the <see cref="FrameworkElement"/> attached has been modified
        /// in such a way that it should just be thrown away when the object is done with it. 
        /// </summary>
        protected internal virtual bool IsDirty
        {
            get;
            set;
        }

        #endregion // IsDirty

        #region AttachedElement

        /// <summary>
        /// Gets/sets the actual <see cref="FrameworkElement"/> that is attached to the object. If no object is attached
        /// then null is returned. 
        /// </summary>
        protected FrameworkElement AttachedElement
        {
            get;
            set;
        }
        #endregion // AttachedElement

        #endregion // Protected

        #endregion // Properties

        #region Methods

        #region Abstract

        #region CreateInstanceOfRecyclingElement

        /// <summary>
        /// Creates a new instance of the <see cref="FrameworkElement"/> that represents the object.
        /// </summary>
        /// <returns></returns>
        protected abstract T CreateInstanceOfRecyclingElement();

        #endregion // CreateInstanceOfRecyclingElement

        #endregion // Abstract

        #region Protected

        #region OnElementAttached
        /// <summary>
        /// Invoked when a <see cref="FrameworkElement"/>  is being attached to the object.
        /// </summary>
        /// <param name="element"></param>
        protected virtual void OnElementAttached(T element)
        {

        }
        #endregion // OnElementAttached

        #region OnElementReleased
        /// <summary>
        /// Invoked when a <see cref="FrameworkElement"/> is no longer attached to the object. 
        /// </summary>
        /// <param name="element"></param>
        protected virtual void OnElementReleased(T element)
        {

        }
        #endregion // OnElementReleased

        #region OnElementReleasing

        /// <summary>
        /// Invoked when a <see cref="FrameworkElement"/> is being released from an object.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>False, if the element shouldn't be released.</returns>
        protected virtual bool OnElementReleasing(T element)
        {
            return true;
        }

        #endregion // OnElementReleasing

        #endregion // Protected

        #endregion // Methods

        #region ISupportRecycling Members

        Type ISupportRecycling.RecyclingElementType
        {
            get { return this.RecyclingElementType; }
        }

        string ISupportRecycling.RecyclingIdentifier
        {
            get { return this.RecyclingIdentifier; }
        }

        FrameworkElement ISupportRecycling.CreateInstanceOfRecyclingElement()
        {
            return this.CreateInstanceOfRecyclingElement();
        }

        void ISupportRecycling.OnElementAttached(FrameworkElement elem)
        {
            this.OnElementAttached(elem as T);
        }

        bool ISupportRecycling.OnElementReleasing(FrameworkElement elem)
        {
            return this.OnElementReleasing(elem as T);
        }

        void ISupportRecycling.OnElementReleased(FrameworkElement elem)
        {
            this.OnElementReleased(elem as T);
        }

        FrameworkElement ISupportRecycling.AttachedElement
        {
            get { return this.AttachedElement; }
            set { this.AttachedElement = value; }
        }

        bool ISupportRecycling.IsDirty
        {
            get { return this.IsDirty; }
            set { this.IsDirty = value; }
        }

        #endregion

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Fired when a property changes on the RecyclingContainer.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invoked when a property changes on the RecyclingContainer object.
        /// </summary>
        /// <param name="name">The name of the property that has changed.</param>
        protected virtual void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        #endregion

    }


    /// <summary>
    /// A base class for objects that want to support recycling which supports objects that will be <see cref="DependencyObject"/>.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="FrameworkElement"/> that this object represents.</typeparam>
    public abstract class DependencyObjectRecyclingContainer<T> : DependencyObjectNotifier, ISupportRecycling where T : FrameworkElement
    {
        #region Static

        private static Type _recyclingElementType = typeof(T);

        #endregion // Static

        #region Properties

        #region Protected

        #region RecyclingElementType

        /// <summary>
        /// Gets the <see cref="Type"/> of the <see cref="FrameworkElement"/> that is being recycled.
        /// </summary>
        protected virtual Type RecyclingElementType
        {
            get
            {
                return DependencyObjectRecyclingContainer<T>._recyclingElementType;
            }
        }
        #endregion // RecyclingElementType

        #region RecyclingIdentifier

        /// <summary>
        /// If a <see cref="RecyclingElementType"/> isn't specified, this property can be used to offer another way of identifying 
        /// a reyclable element.
        /// </summary>
        protected virtual string RecyclingIdentifier
        {
            get
            {
                return null;
            }
        }
        #endregion // RecyclingElementType

        #region IsDirty

        /// <summary>
        /// Gets/sets a value that determines if the <see cref="FrameworkElement"/> attached has been modified
        /// in such a way that it should just be thrown away when the object is done with it. 
        /// </summary>
        protected virtual bool IsDirty
        {
            get;
            set;
        }

        #endregion // IsDirty

        #region AttachedElement

        /// <summary>
        /// Gets/sets the actual <see cref="FrameworkElement"/> that is attached to the object. If no object is attached
        /// then null is returned. 
        /// </summary>
        protected FrameworkElement AttachedElement
        {
            get;
            set;
        }
        #endregion // AttachedElement

        #endregion // Protected

        #endregion // Properties

        #region Methods

        #region Abstract

        #region CreateInstanceOfRecyclingElement

        /// <summary>
        /// Creates a new instance of the <see cref="FrameworkElement"/> that represents the object.
        /// </summary>
        /// <returns></returns>
        protected abstract T CreateInstanceOfRecyclingElement();

        #endregion // CreateInstanceOfRecyclingElement

        #endregion // Abstract

        #region Protected

        #region OnElementAttached
        /// <summary>
        /// Invoked when a <see cref="FrameworkElement"/>  is being attached to the object.
        /// </summary>
        /// <param name="element"></param>
        protected virtual void OnElementAttached(T element)
        {

        }
        #endregion // OnElementAttached

        #region OnElementReleased
        /// <summary>
        /// Invoked when a <see cref="FrameworkElement"/> is no longer attached to the object. 
        /// </summary>
        /// <param name="element"></param>
        protected virtual void OnElementReleased(T element)
        {

        }
        #endregion // OnElementReleased

        #region OnElementReleasing

        /// <summary>
        /// Invoked when a <see cref="FrameworkElement"/> is being released from an object.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>False, if the element shouldn't be released.</returns>
        protected virtual bool OnElementReleasing(T element)
        {
            return true;
        }

        #endregion // OnElementReleasing

        #endregion // Protected

        #endregion // Methods

        #region ISupportRecycling Members

        Type ISupportRecycling.RecyclingElementType
        {
            get { return this.RecyclingElementType; }
        }

        string ISupportRecycling.RecyclingIdentifier
        {
            get { return this.RecyclingIdentifier; }
        }

        FrameworkElement ISupportRecycling.CreateInstanceOfRecyclingElement()
        {
            return this.CreateInstanceOfRecyclingElement();
        }

        void ISupportRecycling.OnElementAttached(FrameworkElement elem)
        {
            this.OnElementAttached(elem as T);
        }

        bool ISupportRecycling.OnElementReleasing(FrameworkElement elem)
        {
            return this.OnElementReleasing(elem as T);
        }

        void ISupportRecycling.OnElementReleased(FrameworkElement elem)
        {
            this.OnElementReleased(elem as T);
        }

        FrameworkElement ISupportRecycling.AttachedElement
        {
            get { return this.AttachedElement; }
            set { this.AttachedElement = value; }
        }

        bool ISupportRecycling.IsDirty
        {
            get { return this.IsDirty; }
            set { this.IsDirty = value; }
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