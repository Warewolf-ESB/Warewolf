using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Infragistics.Controls.Menus;
using System.Collections.Generic;

namespace Infragistics.Controls.Menus.Primitives
{
    /// <summary>
    /// Represents a control that contains multiple items and have a header.
    /// </summary>
    [StyleTypedProperty(Property = "ItemContainerStyle", StyleTargetType = typeof(ContentPresenter))]
    public partial class XamHeaderedItemsControl : ItemsControl, IProvidePropertyPersistenceSettings
    {

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the HeaderedItemsControl class.
        /// </summary>
        public XamHeaderedItemsControl()
        {
            
            DefaultStyleKey = typeof(XamHeaderedItemsControl);
            HeaderedItemContainerGenerator = new HeaderedItemContainerGenerator(this);
        }
        #endregion 

        #region Properties

            #region Public Properties

                #region public HierarchicalDataTemplate HierarchicalItemTemplate
        /// <summary>
        /// Identifies the <see cref="HierarchicalItemTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HierarchicalItemTemplateProperty =
            DependencyProperty.Register("HierarchicalItemTemplate", typeof(HierarchicalDataTemplate), typeof(XamHeaderedItemsControl),
             new PropertyMetadata(new PropertyChangedCallback(OnHierarchicalItemTemplateChanged)));

        /// <summary>
        /// Gets or sets the <see cref="HierarchicalDataTemplate"/> for this <see cref="XamHeaderedItemsControl"/> that describes the visual tree of this item, and what property its children are.
        /// </summary>
        public HierarchicalDataTemplate HierarchicalItemTemplate
        {
            get { return (HierarchicalDataTemplate)this.GetValue(HierarchicalItemTemplateProperty); }
            set { this.SetValue(HierarchicalItemTemplateProperty, value); }
        }

        private static void OnHierarchicalItemTemplateChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            XamHeaderedItemsControl control = obj as XamHeaderedItemsControl;
            HierarchicalDataTemplate template = e.NewValue as HierarchicalDataTemplate;
            if (template != null && template.Template != null)
            {
                control.SetValue(XamHeaderedItemsControl.ItemTemplateProperty, template.Template);
            }
        }

        #endregion // HierarchicalItemTemplate 

                #region public DataTemplate DefaultItemsContainer

        /// <summary>
        /// Identifies the <see cref="DefaultItemsContainer"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DefaultItemsContainerProperty =
            DependencyProperty.Register("DefaultItemsContainer", typeof(DataTemplate), typeof(XamHeaderedItemsControl), null);

        /// <summary>
        /// Gets or sets a DataTemplate that will be used as a container when the items are being generated. 
        /// </summary>
        /// <remarks>
        /// <p class="body">
        /// This property takes effect only to the items that are <b>not</b> native containers of the control. For example if
        /// you add TextBlock as an item for <see cref="XamMenu"/>, the control will wrap the item 
        /// with <see cref="XamMenuItem"/> container. For generating of this menu item container, the 
        /// control will use DataTemplete provided by this property. </p>
        /// <p class="note">The root element of the template provided by <b>DefaultItemsContainer</b> should be the native
        /// container of the control. For the <see cref="XamMenu"/> this is <see cref="XamMenuItem"/>.
        /// If the root content of the DataTemplate is not a XamMenuItem, the property will be ignored and 
        /// an Empty XamMenuItem will be used instead</p>
        /// <p class="note"> This DataTemplate will be used for each XamMenuItem 
        /// in the <see cref="XamMenu"/> unless the <see cref="DefaultItemsContainer"/> is set
        /// explicitly on the XamMenuItem itself.</p>
        /// </remarks>
        public DataTemplate DefaultItemsContainer
        {
            get { return (DataTemplate)this.GetValue(DefaultItemsContainerProperty); }
            set { this.SetValue(DefaultItemsContainerProperty, value); }
        }

        #endregion // DefaultItemsContainer

                #region public object Header
        /// <summary>
        /// Gets or sets an object that labels the HeaderedItemsControl.  The
        /// default value is null.  The header can be a string, UIElement, or
        /// any other content.
        /// </summary>
        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        /// <summary>
        /// Identifies the Header dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                "Header",
                typeof(object),
                typeof(XamHeaderedItemsControl),
                new PropertyMetadata(OnHeaderPropertyChanged));

        /// <summary>
        /// HeaderProperty property changed handler.
        /// </summary>
        /// <param name="d">
        /// HeaderedItemsControl that changed its Header.
        /// </param>
        /// <param name="e">Event arguments.</param>
        private static void OnHeaderPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamHeaderedItemsControl source = d as XamHeaderedItemsControl;
            source.OnHeaderChanged(e.OldValue, e.NewValue);
        }
        #endregion public object Header

                #region public DataTemplate HeaderTemplate
        /// <summary>
        /// Gets or sets a data template used to display a control's header. The
        /// default is null.
        /// </summary>
        public DataTemplate HeaderTemplate
        {
            get { return GetValue(HeaderTemplateProperty) as DataTemplate; }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the HeaderTemplate dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register(
                "HeaderTemplate",
                typeof(DataTemplate),
                typeof(XamHeaderedItemsControl),
                new PropertyMetadata(OnHeaderTemplatePropertyChanged));

        /// <summary>
        /// HeaderTemplateProperty property changed handler.
        /// </summary>
        /// <param name="d">
        /// HeaderedItemsControl that changed its HeaderTemplate.
        /// </param>
        /// <param name="e">Event arguments.</param>
        private static void OnHeaderTemplatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            XamHeaderedItemsControl source = d as XamHeaderedItemsControl;
            DataTemplate oldHeaderTemplate = e.OldValue as DataTemplate;
            DataTemplate newHeaderTemplate = e.NewValue as DataTemplate;
            source.OnHeaderTemplateChanged(oldHeaderTemplate, newHeaderTemplate);
        }
        #endregion public DataTemplate HeaderTemplate

            #endregion

            #region Internal Properties
        /// <summary>
        /// Gets or sets a value indicating whether the Header property has been
        /// set to the item of an ItemsControl.
        /// </summary>
        internal bool HeaderIsItem { get; set; }

        /// <summary>
        /// Gets the HeaderedItemContainerGenerator that is associated with this
        /// control.
        /// </summary>
        internal IHeaderedItemContainerGenerator HeaderedItemContainerGenerator { get; private set; }
        #endregion



#region Infragistics Source Cleanup (Region)


































#endregion // Infragistics Source Cleanup (Region)


        #endregion

        #region Base class override

        #region public override void OnApplyTemplate()

        /// <summary>
        /// Apply a control template to the control.
        /// </summary>
        public override void OnApplyTemplate()
        {
            HeaderedItemContainerGenerator.OnApplyTemplate();
            base.OnApplyTemplate();
        }
        #endregion

            #region protected override void PrepareContainerForItemOverride(DependencyObject element, object item)

        /// <summary>
        /// Prepares the specified container to display the specified item.
        /// </summary>
        /// <param name="element">
        /// Container element used to display the specified item.
        /// </param>
        /// <param name="item">Specified item to display.</param>
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            //For some reason specific to WPF if we call Base AFTER the Generator it will cause the element to lose it's DataTemplate...
            //But it will work just fine in Silverlight.
            base.PrepareContainerForItemOverride(element, item);
            HeaderedItemContainerGenerator.PrepareContainerForItemOverride(element, item, ItemContainerStyle);
        }
        #endregion

            #region protected override void ClearContainerForItemOverride(DependencyObject element, object item)

        /// <summary>
        /// Undoes the effects of PrepareContainerForItemOverride.
        /// </summary>
        /// <param name="element">The container element.</param>
        /// <param name="item">The contained item.</param>
        protected override void ClearContainerForItemOverride(DependencyObject element, object item)
        {
            HeaderedItemContainerGenerator.ClearContainerForItemOverride(element, item);
            base.ClearContainerForItemOverride(element, item);
        }
        #endregion


        #region protected override void OnItemContainerStyleChanged(Style oldItemContainerStyle, Style newItemContainerStyle)

        /// <summary>
        /// Allows the XamHeaderedItemsControl to respond to changes in the ItemContainerStyle
        /// </summary>
        /// <param name="oldItemContainerStyle">The previously associated style</param>
        /// <param name="newItemContainerStyle">The newly set style</param>
        protected override void OnItemContainerStyleChanged(Style oldItemContainerStyle, Style newItemContainerStyle)
        {
            this.HeaderedItemContainerGenerator.UpdateItemContainerStyle(newItemContainerStyle);
        }

        #endregion


            

        

        #endregion

        #region Methods

            #region protected virtual void OnHeaderChanged(object oldHeader, object newHeader)

        /// <summary>
        /// Called when the Header property of a HeaderedItemsControl changes.
        /// </summary>
        /// <param name="oldHeader">
        /// The old value of the Header property.
        /// </param>
        /// <param name="newHeader">
        /// The new value of the Header property.
        /// </param>
        protected virtual void OnHeaderChanged(object oldHeader, object newHeader)
        {
        }
        #endregion

            #region protected virtual void OnHeaderTemplateChanged(DataTemplate oldHeaderTemplate, DataTemplate newHeaderTemplate)
        /// <summary>
        /// Called when the HeaderTemplate property changes.
        /// </summary>
        /// <param name="oldHeaderTemplate">
        /// The old value of the HeaderTemplate property.
        /// </param>
        /// <param name="newHeaderTemplate">
        /// The new value of the HeaderTemplate property.
        /// </param>
        protected virtual void OnHeaderTemplateChanged(DataTemplate oldHeaderTemplate, DataTemplate newHeaderTemplate)
        {
        }
        #endregion
        #endregion

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