using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Infragistics.AutomationPeers;
using System.Collections.Generic;
using System.Collections;

namespace Infragistics.Controls.Menus
{
    /// <summary>
    /// <see cref="XamTagCloud"/> is a control that allows a list of objects 
    /// to be displayed in a cloud-like fashion where weight can be assigned to the object based on their frequency 
    /// of occurrence. Objects are scaled according to their weight so that words with greater weight 
    /// will normally be displayed at a larger size; objects with 
    /// less weight will be displayed at a smaller size with.  A <see cref="XamTagCloudItem"/> can contain any content so it's not
    /// simply limited to displaying text tags.
    /// </summary>

    
    

    public class XamTagCloud : ItemsControl, IProvidePropertyPersistenceSettings
    {
        #region Static

        private static Panel GetPanel(DependencyObject element)
        {

            ItemsPresenter presenter = XamTagCloud.ResolveItemsPresenter(element);
            if (presenter != null)
            {
                if (VisualTreeHelper.GetChildrenCount(presenter) > 0)
                    return VisualTreeHelper.GetChild(presenter, 0) as Panel;
            }

            return null;
        }


        private static ItemsPresenter ResolveItemsPresenter(DependencyObject obj)
        {
            int count = VisualTreeHelper.GetChildrenCount(obj);
            for (int i = 0; i < count; i++)
            {
                DependencyObject elem = VisualTreeHelper.GetChild(obj, i);
                ItemsPresenter presenter = elem as ItemsPresenter;
                if (presenter != null)
                    return presenter;
                else
                {
                    presenter = XamTagCloud.ResolveItemsPresenter(elem);
                    if (presenter != null)
                        return presenter;
                }
            }
            return null;
        }
        #endregion //Static

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="XamTagCloud"/> class.
        /// </summary>
        public XamTagCloud()
        {

            base.DefaultStyleKey = typeof(XamTagCloud);


            Infragistics.Windows.Utilities.ValidateLicense(typeof(XamTagCloud), this);


#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

        }


        #endregion //Constructor

        #region Events
        /// <summary>
        /// Occurs when a XamTagCloudItem is clicked.
        /// </summary>
        public event EventHandler<XamTagCloudItemEventArgs> XamTagCloudItemClicked;
        /// <summary>
        /// Fires the <see cref="XamTagCloudItemClicked"/> event
        /// </summary>
        /// <param name="item">XamTagCloudItem</param>
        protected internal virtual void OnXamTagCloudItemClick(XamTagCloudItem item)
        {
            if (this.XamTagCloudItemClicked != null)
                this.XamTagCloudItemClicked(this, new XamTagCloudItemEventArgs() { XamTagCloudItem = item });
        }


        /// <summary>
        /// Occurs when a XamTagCloud cannot display all of its items.
        /// </summary>
        public event EventHandler<XamTagCloudClippedEventArgs> XamTagCloudClipped;
        /// <summary>
        /// Fires the <see cref="XamTagCloudClipped"/> event
        /// </summary>
        protected internal virtual void OnXamTagCloudItemClip(bool cloudClipped)
        {
            if (this.XamTagCloudClipped != null)
                this.XamTagCloudClipped(this, new XamTagCloudClippedEventArgs() { CloudClipped = cloudClipped });
        }
        #endregion //Events

        #region Event Handlers

        void XamTagCloud_XamTagCloudClipped(object sender, XamTagCloudClippedEventArgs e)
        {
            this.OnXamTagCloudItemClip(e.CloudClipped);
        }

        void ScaleBreaks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.InvalidateItemsPanel();
        }

        #endregion //Event Handlers

        #region Members
        private XamTagCloudPanel _panel;
        ObservableCollection<ScaleBreak> _scaleBreaks;
        #endregion //Members

        #region Overrides

        #region IsItemItsOwnContainer
        /// <summary>
        /// Determines if the specified item is (or is eligible to be) its own container.
        /// </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>
        /// True if the item is (or is eligible to be) its own container; otherwise, False.
        /// </returns>
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return (item is XamTagCloudItem);
        }
        #endregion //IsItemItsOwnContainer

        #region GetContainerForItem
        /// <summary>
        /// Creates or identifies the element that is used to display the given item.
        /// </summary>
        /// <returns>
        /// The element that is used to display the given item.
        /// </returns>
        protected override DependencyObject GetContainerForItemOverride()
        {
            XamTagCloudItem item = new XamTagCloudItem();
            if (item.Style != this.ItemsContainerStyle)
            {
                if (this.ItemsContainerStyle == null)
                {
                    item.ClearValue(Control.StyleProperty);
                }
                else
                {
                    item.Style = this.ItemsContainerStyle;
                }
            }
            item.TargetName = this.TargetName;
            return item;
        }
        #endregion //GetContainerForItem

        #region OnCreateAutomationPeer
        /// <summary>
        /// When implemented in a derived class, returns class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> implementations for the automation infrastructure.
        /// </summary>
        /// <returns>
        /// The class-specific <see cref="T:System.Windows.Automation.Peers.AutomationPeer"/> subclass to return.
        /// </returns>
        protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
        {
            return new XamTagCloudAutomationPeer(this);
        }
        #endregion //OnCreateAutomationPeer

        #region PrepareContainerForItem
        /// <summary>
        /// Overrides the framework invocation of preparing a given <see cref="XamTagCloudItem"/>.
        /// </summary>
        /// <param name="element">The <see cref="XamTagCloudItem"/> to prepare.</param>
        /// <param name="item">The content of the <see cref="XamTagCloudItem"/>.</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            //If the item is not a XamTagCloudItem we need to wrap it up in one.
            XamTagCloudItem xwtci = item as XamTagCloudItem;
            if (xwtci == null)
            {
                XamTagCloudItem tagItem = (XamTagCloudItem)element;
                if (!string.IsNullOrEmpty(this.DisplayMemberPath))
                {
                    Binding b = new Binding(this.DisplayMemberPath);
                    b.Source = item;
                    b.Mode = BindingMode.OneWay;
                    tagItem.SetBinding(XamTagCloudItem.ContentProperty, b);
                }

                if (!string.IsNullOrEmpty(this.WeightMemberPath))
                {
                    Binding b = new Binding(this.WeightMemberPath);
                    b.Source = item;
                    b.Mode = BindingMode.OneWay;
                    tagItem.SetBinding(XamTagCloudItem.WeightProperty, b);
                }

                if (!string.IsNullOrEmpty(this.NavigateUriMemberPath))
                {
                    Binding b = new Binding(this.NavigateUriMemberPath);
                    b.Source = item;
                    b.Mode = BindingMode.OneWay;
                    tagItem.SetBinding(XamTagCloudItem.NavigateUriProperty, b);
                }

                DependencyObject obj = item as DependencyObject;
                if (obj != null)
                {
                    tagItem.Weight = XamTagCloudPanel.GetWeight(obj);
                    tagItem.NavigateUri = XamTagCloudPanel.GetNavigateUri(obj);
                }


                FrameworkElement elem = item as FrameworkElement;
                if (elem != null)
                {
                    tagItem.VerticalAlignment = elem.VerticalAlignment;
                }

                tagItem.Owner = this;

            }
            //Otherwise we can set any properties that need setting on the item directly.
            else
            {
                xwtci.Owner = this;
                if (xwtci.Style != this.ItemsContainerStyle)
                {
                    if (this.ItemsContainerStyle == null)
                        xwtci.ClearValue(XamTagCloud.StyleProperty);
                    else
                        xwtci.Style = this.ItemsContainerStyle;
                }
            }

            //Next create bindings for any properties that need to exist on the panel that are exposed through the cloud.
            if (this._panel == null)
            {
                XamTagCloudPanel p = XamTagCloud.GetPanel(this) as XamTagCloudPanel;

                if (p != null)
                {
                    Binding panelBinding;
                    this._panel = p;

                    panelBinding = new Binding("HorizontalContentAlignment");
                    panelBinding.Source = this;
                    panelBinding.Mode = BindingMode.TwoWay;
                    this._panel.SetBinding(XamTagCloudPanel.HorizontalContentAlignmentProperty, panelBinding);

                    panelBinding = new Binding("VerticalContentAlignment");
                    panelBinding.Source = this;
                    panelBinding.Mode = BindingMode.TwoWay;
                    this._panel.SetBinding(XamTagCloudPanel.VerticalContentAlignmentProperty, panelBinding);

                    panelBinding = new Binding("MaxScale");
                    panelBinding.Source = this;
                    panelBinding.Mode = BindingMode.TwoWay;
                    this._panel.SetBinding(XamTagCloudPanel.MaxScaleProperty, panelBinding);

                    panelBinding = new Binding("MinScale");
                    panelBinding.Source = this;
                    panelBinding.Mode = BindingMode.TwoWay;
                    this._panel.SetBinding(XamTagCloudPanel.MinScaleProperty, panelBinding);

                    panelBinding = new Binding("ItemSpacing");
                    panelBinding.Source = this;
                    panelBinding.Mode = BindingMode.TwoWay;
                    this._panel.SetBinding(XamTagCloudPanel.ItemSpacingProperty, panelBinding);

                    panelBinding = new Binding("UseSmoothScaling");
                    panelBinding.Source = this;
                    panelBinding.Mode = BindingMode.TwoWay;
                    this._panel.SetBinding(XamTagCloudPanel.UseSmoothScalingProperty, panelBinding);

                    //Setup scale breaks, this probably needs to be done better.
                    this._panel.SetScaleBreaks(this.ScaleBreaks);

                    //Setup cloud clipped event.
                    this._panel.XamTagCloudClipped -= XamTagCloud_XamTagCloudClipped;
                    this._panel.XamTagCloudClipped += new EventHandler<XamTagCloudClippedEventArgs>(XamTagCloud_XamTagCloudClipped);
                }

            }
            base.PrepareContainerForItemOverride(element, item);

        }

        #endregion //PrepareContainerForItem
        #endregion //Overrides

        #region Properties

        #region Public

        #region MaxScale

        /// <summary>
        /// Identifies the <see cref="MaxScale"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty MaxScaleProperty = DependencyProperty.Register("MaxScale", typeof(Double), typeof(XamTagCloud), new PropertyMetadata(1.0, new PropertyChangedCallback(MaxScaleChanged)));
        /// <summary>
        /// Gets and sets the <see cref="MaxScale"/> property.
        /// </summary>
        public Double MaxScale
        {
            get { return (Double)this.GetValue(MaxScaleProperty); }
            set
            {

                this.SetValue(MaxScaleProperty, value);

            }
        }

        private static void MaxScaleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // MaxScale

        #region MinScale

        /// <summary>
        /// Identifies the <see cref="MinScale"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty MinScaleProperty = DependencyProperty.Register("MinScale", typeof(Double), typeof(XamTagCloud), new PropertyMetadata(0.5, new PropertyChangedCallback(MinScaleChanged)));
        /// <summary>
        /// Gets and sets the <see cref="MinScale"/> property.
        /// </summary>
        public Double MinScale
        {
            get { return (Double)this.GetValue(MinScaleProperty); }
            set
            {

                this.SetValue(MinScaleProperty, value);

            }
        }

        private static void MinScaleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // MinScale

        #region ItemSpacing

        /// <summary>
        /// Identifies the <see cref="ItemSpacing"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ItemSpacingProperty = DependencyProperty.Register("ItemSpacing", typeof(Thickness), typeof(XamTagCloud), new PropertyMetadata(new PropertyChangedCallback(ItemSpacingChanged)));
        /// <summary>
        /// Gets and sets the <see cref="ItemSpacing"/> property.
        /// </summary>
        public Thickness ItemSpacing
        {
            get { return (Thickness)this.GetValue(ItemSpacingProperty); }
            set { this.SetValue(ItemSpacingProperty, value); }
        }

        private static void ItemSpacingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // ItemSpacing

        #region ItemsContainerStyle

        /// <summary>
        /// Identifies the <see cref="ItemsContainerStyle"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty ItemsContainerStyleProperty = DependencyProperty.Register("ItemsContainerStyle", typeof(Style), typeof(XamTagCloud), new PropertyMetadata(new PropertyChangedCallback(ItemsContainerStyleChanged)));
        /// <summary>
        /// Gets and sets the <see cref="ItemsContainerStyle"/> property.
        /// </summary>
        public Style ItemsContainerStyle
        {
            get { return (Style)this.GetValue(ItemsContainerStyleProperty); }
            set { this.SetValue(ItemsContainerStyleProperty, value); }
        }

        private static void ItemsContainerStyleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamTagCloud xwtc = obj as XamTagCloud;
            if (xwtc != null)
            {
                foreach (Object xwtci in xwtc.Items)
                {
                    DependencyObject dpo = xwtc.ItemContainerGenerator.ContainerFromItem(xwtci);

                    dpo.SetValue(XamTagCloudItem.StyleProperty, e.NewValue);

                }
            }
        }

        #endregion // ItemsContainerStyle

        #region WeightMemberPath

        /// <summary>
        /// Identifies the <see cref="WeightMemberPath"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty WeightMemberPathProperty = DependencyProperty.Register("WeightMemberPath", typeof(string), typeof(XamTagCloud), new PropertyMetadata(new PropertyChangedCallback(WeightMemberPathChanged)));
        /// <summary>
        /// Gets and sets the <see cref="WeightMemberPath"/>.
        /// </summary>
        public string WeightMemberPath
        {
            get { return (string)this.GetValue(WeightMemberPathProperty); }
            set { this.SetValue(WeightMemberPathProperty, value); }
        }

        private static void WeightMemberPathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamTagCloud xwtc = obj as XamTagCloud;
            if (xwtc != null && xwtc.ItemsSource != null)
            {
                DependencyObject tagCloudItem = null;
                object item = null;
                IEnumerator enumerator = xwtc.ItemsSource.GetEnumerator();

                for (int i = 0; i < xwtc.Items.Count; i++)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }

                    tagCloudItem = (DependencyObject)xwtc.ItemContainerGenerator.ContainerFromIndex(i);
                    item = enumerator.Current;

                    if (tagCloudItem != null)
                    {
                        xwtc.PrepareContainerForItemOverride(tagCloudItem, item);
                    }
                }
            }
        }

        #endregion // WeightMemberPath

        #region TargetName

        /// <summary>
        /// Identifies the <see cref="TargetName"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty TargetNameProperty = DependencyProperty.Register("TargetName", typeof(string), typeof(XamTagCloud), new PropertyMetadata(new PropertyChangedCallback(TargetNameChanged)));
        /// <summary>
        /// Gets and sets the <see cref="TargetName"/> property.
        /// </summary>
        public string TargetName
        {
            get { return (string)this.GetValue(TargetNameProperty); }
            set { this.SetValue(TargetNameProperty, value); }
        }

        private static void TargetNameChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // TargetName

        #region UseSmoothScaling

        /// <summary>
        /// Identifies the <see cref="UseSmoothScaling"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty UseSmoothScalingProperty = DependencyProperty.Register("UseSmoothScaling", typeof(bool), typeof(XamTagCloud), new PropertyMetadata(new PropertyChangedCallback(UseSmoothScalingChanged)));
        /// <summary>
        /// Gets or sets the <see cref="UseSmoothScaling"/> property.
        /// </summary>
        public bool UseSmoothScaling
        {
            get { return (bool)this.GetValue(UseSmoothScalingProperty); }
            set { this.SetValue(UseSmoothScalingProperty, value); }
        }

        private static void UseSmoothScalingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion // UseSmoothScaling

        #region NavigateUriMemberPath

        /// <summary>
        /// Identifies the <see cref="NavigateUriMemberPath"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty NavigateUriMemberPathProperty = DependencyProperty.Register("NavigateUriMemberPath", typeof(string), typeof(XamTagCloud), new PropertyMetadata(new PropertyChangedCallback(NavigateUriMemberPathChanged)));
        /// <summary>
        /// Gets and sets the <see cref="NavigateUriMemberPath"/> property.
        /// </summary>
        public string NavigateUriMemberPath
        {
            get { return (string)this.GetValue(NavigateUriMemberPathProperty); }
            set { this.SetValue(NavigateUriMemberPathProperty, value); }
        }

        private static void NavigateUriMemberPathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamTagCloud xwtc = obj as XamTagCloud;
            if (xwtc != null && xwtc.ItemsSource != null)
            {
                DependencyObject tagCloudItem = null;
                object item = null;
                IEnumerator enumerator = xwtc.ItemsSource.GetEnumerator();

                for (int i = 0; i < xwtc.Items.Count; i++)
                {
                    if (!enumerator.MoveNext())
                    {
                        break;
                    }

                    tagCloudItem = (DependencyObject)xwtc.ItemContainerGenerator.ContainerFromIndex(i);
                    item = enumerator.Current;

                    if (tagCloudItem != null)
                    {
                        xwtc.PrepareContainerForItemOverride(tagCloudItem, item);
                    }
                }
            }
        }

        #endregion // NavigateUriMemberPath

        #region ScaleBreaks

        /// <summary>
        /// A collection of <see cref="ScaleBreak"/>'s which are used to define explicit scale values for a given range of scale values.
        /// </summary>
        public ObservableCollection<ScaleBreak> ScaleBreaks
        {
            get
            {
                if (this._scaleBreaks == null)
                {
                    this._scaleBreaks = new ObservableCollection<ScaleBreak>();
                    this._scaleBreaks.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(this.ScaleBreaks_CollectionChanged);
                }

                return this._scaleBreaks;
            }
        }

        #endregion // ScaleBreaks

        #endregion //Public

        #region Protected

        #endregion //Protected

        #endregion //Properties

        #region Methods

        #region Internal

        internal void InvalidateItemsPanel()
        {
            if (this._panel != null)
                this._panel.InvalidateMeasure();
        }

        #endregion // Internal

        #region Public

        #region OnApplyTemplate

        /// <summary>
        /// Builds the visual tree for the <see cref="XamWebTagCloud"/> when a new template is applied. 
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (this._panel != null)
            {
                this._panel.XamTagCloudClipped -= XamTagCloud_XamTagCloudClipped;
                this._panel = null;
            }
            base.OnApplyTemplate();
        }

        #endregion // OnApplyTemplate

        #endregion // Public

        #endregion // Methods

        #region IProvidePropertyPersistenceSettings Members

        #region PropertiesToIgnore

        /// <summary>
        /// Gets a List of properties that shouldn't be saved when the PersistenceManager goes to save them.
        /// </summary>
        protected virtual List<string> PropertiesToIgnore
        {
            get
            {
                List<string> list = new List<string>();
                if (this.ItemsSource != null)
                {
                    list.AddRange(new string[]{
                    "ItemsSource",
                    "Items"
                    });
                }
                return list;
            }
        }

        List<string> IProvidePropertyPersistenceSettings.PropertiesToIgnore
        {
            get
            {
                return this.PropertiesToIgnore;
            }
        }

        #endregion // PropertiesToIgnore

        #region PriorityProperties

        /// <summary>
        /// Gets a List of properties that should be applied, before even trying to look at any other property on the object.
        /// </summary>
        protected virtual List<string> PriorityProperties
        {
            get
            {
                return null;
            }
        }
        List<string> IProvidePropertyPersistenceSettings.PriorityProperties
        {
            get { return this.PriorityProperties; }
        }


        #endregion // PriorityProperties

        #region FinishedLoadingPersistence
        /// <summary>
        /// Allows an object to perform an operation, after it's been loaded.
        /// </summary>
        protected virtual void FinishedLoadingPersistence()
        {

        }

        void IProvidePropertyPersistenceSettings.FinishedLoadingPersistence()
        {
            this.FinishedLoadingPersistence();
        }
        #endregion // FinishedLoadingPersistence

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