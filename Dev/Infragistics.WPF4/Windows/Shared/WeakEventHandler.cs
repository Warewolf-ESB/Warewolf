using System;
using System.Collections.Specialized;

namespace Infragistics
{
    #region WeakEventHandler

    /// <summary>
    /// Helper class for weak event handling.
    /// </summary>
    /// <typeparam name="TInstance"></typeparam>
    /// <typeparam name="TEventSource"></typeparam>
    /// <typeparam name="TEventArgs"></typeparam>
    /// <example>
    /// <code>
    /// <![CDATA[
    /// using System;
    /// using System.Collections;
    /// using System.Collections.Specialized;
    /// using System.ComponentModel;
    /// using Infragistics;
    /// 
    /// namespace TestApp
    /// {
    ///     public class TestClass
    ///     {
    ///         private IEnumerable _itemsSource;
    ///         private WeakCollectionChangedHandler<TestClass> _weakCollectionChanged;
    ///         private WeakEventHandler<TestClass, ICollectionView, EventArgs> _weakCollectionViewCurrentChanged;
    /// 
    ///         public IEnumerable ItemsSource
    ///         {
    ///             get { return _itemsSource; }
    ///             set { this.SetItemsSource(value); }
    ///         }
    /// 
    ///         private void SetItemsSource(IEnumerable value)
    ///         {
    ///             if (this._itemsSource == value)
    ///             {
    ///                 return;
    ///             }
    /// 
    ///             if (this._weakCollectionChanged != null)
    ///             {
    ///                 this._weakCollectionChanged.Detach();
    ///                 this._weakCollectionChanged = null;
    ///             }
    /// 
    ///             if (this._weakCollectionViewCurrentChanged != null)
    ///             {
    ///                 this._weakCollectionViewCurrentChanged.Detach();
    ///                 this._weakCollectionViewCurrentChanged = null;
    ///             }
    /// 
    ///             this._itemsSource = value;
    /// 
    ///             INotifyCollectionChanged notifyCollectionChanged = value as INotifyCollectionChanged;
    ///             if (notifyCollectionChanged != null)
    ///             {
    ///                 this._weakCollectionChanged =
    ///                     new WeakCollectionChangedHandler<TestClass>
    ///                         (
    ///                             this,
    ///                             notifyCollectionChanged,
    ///                             (instance, s, e) => instance.ItemsSource_CollectionChanged(s, e)
    ///                         );
    /// 
    ///                 notifyCollectionChanged.CollectionChanged += this._weakCollectionChanged.OnEvent;
    ///             }
    /// 
    ///             ICollectionView collectionView = value as ICollectionView;
    ///             if (collectionView != null)
    ///             {
    ///                 this._weakCollectionViewCurrentChanged =
    ///                     new WeakEventHandler<TestClass, ICollectionView, EventArgs>
    ///                         (
    ///                             this,
    ///                             collectionView,
    ///                             (instance, s, e) => instance.ItemsSource_CurrentChanged(s, e),
    ///                             (weakHandler, eventSource) => eventSource.CurrentChanged -= weakHandler.OnEvent
    ///                         );
    /// 
    ///                 collectionView.CurrentChanged += this._weakCollectionViewCurrentChanged.OnEvent;
    ///             }
    ///         }
    /// 
    ///         private void ItemsSource_CurrentChanged(object sender, EventArgs e)
    ///         {
    ///             // ...
    ///         }
    /// 
    ///         private void ItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    ///         {
    ///             // ...
    ///         }
    ///     }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    internal class WeakEventHandler<TInstance, TEventSource, TEventArgs>
        where TInstance : class
    {
        #region Members

        private readonly WeakReference _weakInstance;
        private readonly WeakReference _weakEventSource;

        #endregion // Members

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="WeakEventHandler{TInstance,TEventSource,TEventArgs}"/> class.
        /// </summary>
        /// <param name="instance">The short living object that wants to recieve events from the long living <paramref name="eventSource"/> object.</param>
        /// <param name="eventSource">The long living object that raises the event.</param>
        /// <param name="onEventAction">The delegate that will be invoked when the event is raised.</param>
        /// <param name="onDetachAction">The delegate that will be invoked when the event should be detached</param>
        /// <remarks>
        /// The delegates <paramref name="onEventAction"/> and <paramref name="onDetachAction"/> must not refer to instance methods.
        /// </remarks>
        public WeakEventHandler(TInstance instance, TEventSource eventSource, Action<TInstance, object, TEventArgs> onEventAction, Action<WeakEventHandler<TInstance, TEventSource, TEventArgs>, TEventSource> onDetachAction)
        {


#region Infragistics Source Cleanup (Region)
















#endregion // Infragistics Source Cleanup (Region)

            this._weakInstance = new WeakReference(instance);
            this._weakEventSource = new WeakReference(eventSource);
            this.OnEventAction = onEventAction;
            this.OnDetachAction = onDetachAction;
        }

        #endregion // Constructor

        #region Properties

        #region OnEventAction

        /// <summary>
        /// Gets or sets the delegate that will be invoked when the event is raised.
        /// </summary>
        /// <remarks>
        /// The delegate must not refer to an instance method.
        /// </remarks>
        private Action<TInstance, object, TEventArgs> OnEventAction { get; set; }

        #endregion // OnEventAction

        #region OnDetachAction

        /// <summary>
        /// Gets or sets the delegate that will be invoked when the event should be detached.
        /// </summary>
        /// <remarks>
        /// The delegate must not refer to an instance method.
        /// </remarks>
        private Action<WeakEventHandler<TInstance, TEventSource, TEventArgs>, TEventSource> OnDetachAction { get; set; }

        #endregion // OnDetachAction

        #endregion // Properties

        #region Methods

        #region OnEvent

        /// <summary>
        /// Handler for the event raised by the long living object.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="eventArgs">The <see cref="TEventArgs"/> instance containing the event data.</param>
        public void OnEvent(object source, TEventArgs eventArgs)
        {
            TInstance target = CoreUtilities.GetWeakReferenceTargetSafe(this._weakInstance) as TInstance;

            if (target != null)
            {
                if (this.OnEventAction != null)
                {
                    this.OnEventAction(target, source, eventArgs);
                }
            }
            else
            {
                this.Detach();
            }
        }

        #endregion // OnEvent

        #region Detach

        /// <summary>
        /// Invokes <see cref="OnDetachAction"/> that handles the detaching of <see cref="OnEvent"/> from the event.
        /// </summary>
        public void Detach()
        {
            TEventSource eventSource = (TEventSource)CoreUtilities.GetWeakReferenceTargetSafe(this._weakEventSource);

            if (this.OnDetachAction != null && this._weakEventSource.IsAlive)
            {
                this.OnDetachAction(this, eventSource);
            }

            this.OnDetachAction = null;
        }

        #endregion // Detach

        #endregion // Methods
    }

    #endregion // WeakEventHandler

    #region WeakCollectionChangedHandler

    /// <summary>
    /// Helper class for weak handling of <c>System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged</c>.
    /// </summary>
    /// <typeparam name="TInstance">The type of the instance that will recieve the events.</typeparam>
    /// <remarks>
    /// See <see cref="WeakEventHandler{TInstance,TEventSource,TEventArgs}"/> for a sample.
    /// </remarks>
    internal class WeakCollectionChangedHandler<TInstance> : WeakEventHandler<TInstance, INotifyCollectionChanged, NotifyCollectionChangedEventArgs>
        where TInstance : class 
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="WeakCollectionChangedHandler{TInstance}"/> class.
        /// </summary>
        /// <param name="instance">The short living object that wants to recieve events from the long living <paramref name="eventSource"/> object.</param>
        /// <param name="eventSource">The long living object that raises the event.</param>
        /// <param name="onEventAction">The delegate that will be invoked when the event is raised.</param>
        public WeakCollectionChangedHandler(TInstance instance, INotifyCollectionChanged eventSource, Action<TInstance, object, NotifyCollectionChangedEventArgs> onEventAction)
            : base(instance, eventSource, onEventAction, (weakHandler, evSource) => evSource.CollectionChanged -= weakHandler.OnEvent)
        {
        }

        #endregion // Constructor
    }

    #endregion // WeakCollectionChangedHandler
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