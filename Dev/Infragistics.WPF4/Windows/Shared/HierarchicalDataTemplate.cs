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
using System.Windows.Markup;
using System.Windows.Data;
using System.Collections;

namespace Infragistics.Controls
{
    /// <summary>
    /// Describes a multi-level visual structure of a data object.
    /// </summary>
    [ContentProperty("Template")]
    public class HierarchicalDataTemplate : DependencyObject
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="HierarchicalDataTemplate"/> class.
        /// </summary>
        public HierarchicalDataTemplate()
        {
        }

        #endregion // Constructor

        #region Properties

        #region Public

        #region HierarchicalItemTemplate

        /// <summary>
        /// Identifies the <see cref="HierarchicalItemTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HierarchicalItemTemplateProperty = DependencyProperty.Register("HierarchicalItemTemplate", typeof(HierarchicalDataTemplate), typeof(HierarchicalDataTemplate), null);

        /// <summary>
        /// Gets or sets the <see cref="HierarchicalDataTemplate"/> that describes the visual structure for an object's children.
        /// </summary>
        public HierarchicalDataTemplate HierarchicalItemTemplate
        {
            get { return (HierarchicalDataTemplate)this.GetValue(HierarchicalItemTemplateProperty); }
            set { this.SetValue(HierarchicalItemTemplateProperty, value); }
        }

        #endregion // HierarchicalItemTemplate 

        #region ItemTemplate
        /// <summary>
        /// Identifies the <see cref="ItemTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(HierarchicalDataTemplate), null);

        /// <summary>
        /// Gets or sets the <see cref="DataTemplate"/> that will be used to describe each child item of an object.
        /// </summary>
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)this.GetValue(ItemTemplateProperty); }
            set { this.SetValue(ItemTemplateProperty, value); }
        }

        #endregion // ItemTemplate 

        #region Template

        /// <summary>
        /// Identifies the <see cref="Template"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TemplateProperty = DependencyProperty.Register("Template", typeof(DataTemplate), typeof(HierarchicalDataTemplate), null);
        
        /// <summary>
        /// Gets or sets the DataTemplate that will be used to describe how the owning object will be be laid out.
        /// </summary>
        public DataTemplate Template
        {
            get { return (DataTemplate)this.GetValue(TemplateProperty); }
            set { this.SetValue(TemplateProperty, value); }
        }

        #endregion // Template 

        #region ItemsSource

        /// <summary>
        /// Gets or sets the binding used for obtaining the items source.
        /// </summary>
        public Binding ItemsSource
        {
            get;
            set;
        } 

        #endregion // ItemsSource

        #region ItemContainerStyle

        /// <summary>
        /// Identifies the <see cref="ItemContainerStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemContainerStyleProperty = DependencyProperty.Register("ItemContainerStyle", typeof(Style), typeof(HierarchicalDataTemplate), null);                         

        /// <summary>
        /// Gets or sets the style that is used when rendering the children of the owning object.
        /// </summary>
        public Style ItemContainerStyle
        {
            get { return (Style)this.GetValue(ItemContainerStyleProperty); }
            set { this.SetValue(ItemContainerStyleProperty, value); }
        }

        #endregion // ItemContainerStyle 

        #region DefaultItemsContainer

        /// <summary>
        /// Identifies the <see cref="DefaultItemsContainer"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DefaultItemsContainerProperty = DependencyProperty.Register("DefaultItemsContainer", typeof(DataTemplate), typeof(HierarchicalDataTemplate), null);

        /// <summary>
        /// Gets or sets a DataTemplate whose content will be used as the default container for all generated children items of the owning object.
        /// </summary>
		/// <remarks>
		/// If the root content of the DataTemplate is not of a specific type defined by the owning control, the property will be ignored and the default container will be used instead.
		/// </remarks>
        public DataTemplate DefaultItemsContainer
        {
            get { return (DataTemplate)this.GetValue(DefaultItemsContainerProperty); }
            set { this.SetValue(DefaultItemsContainerProperty, value); }
        }

        #endregion // DefaultItemsContainer 
          
        #endregion // Public

        #endregion // Properties
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