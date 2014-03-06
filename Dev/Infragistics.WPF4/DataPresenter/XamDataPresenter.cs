#region using ...

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Threading;
using System.Diagnostics;
//using System.Windows.Events;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;
using System.Data;
using System.Xml;
using System.Windows.Markup;
//using Infragistics.Windows.Input;
using Infragistics.Shared;
using Infragistics.Windows.DataPresenter;
using Infragistics.Windows.Themes;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Resizing;
using Infragistics.Windows.Selection;
using Infragistics.Windows.Commands;
using Infragistics.Windows.Internal;
using Infragistics.Windows.Editors;
using Infragistics.Windows.Controls;
using Infragistics.Windows.DataPresenter.Events;
using Infragistics.Windows.Licensing;
using System.Windows.Automation.Peers;
using Infragistics.Windows.Automation.Peers;
using System.Text;

#endregion //using ...	
    
namespace Infragistics.Windows.DataPresenter
{
	/// <summary>
    /// A <see cref="DataPresenterBase"/> derived control that can display data using different 'views', depending on the setting of its <see cref="View"/> property.  
	/// </summary>
	/// <remarks>
    /// <p class="body">The control can use either of the two views shipped with NetAdvantage for WPF 7.1 - <see cref="GridView"/> and <see cref="CarouselView"/> as well as any <see cref="ViewBase"/> derived view.
	/// You can extend the <see cref="ViewBase"/> class to provide additional custom views.  Refer to the <a href="xamDataPresenter_Creating_Custom_Views.html">Creating Custom Views for XamDataPresenter</a> topic 
	/// in the Developer's Guide for an explanation of how to create your own custom views.</p>
	/// <p class="body">Unlike <see cref="XamDataCarousel"/> and <see cref="XamDataGrid"/>, this control supports dynamically changing the view at both design time and run time.
    /// To change the View used by the control at designtime or runtime, simply set the control's <see cref="XamDataPresenter.View"/> property to an instance of a <see cref="GridView"/>, <see cref="CarouselView"/> 
    /// or a custom <see cref="ViewBase"/> derived object</p>
    /// <p class="body">Using the XamDataPresenter control with either a <see cref="GridView"/> or a <see cref="CarouselView"/> (but not switching between them)
    /// is functionally equivalent to using a <see cref="XamDataGrid"/> or <see cref="XamDataCarousel"/> control.  If you need the flexibility to switch between these Views (or any future Views 
    /// that Infragistics may release) or if you want to use a custom view that you have created, then the <see cref="XamDataPresenter"/> is the control to use.  If however, your application only needs a 
    /// Grid or Carousel view, you may want to choose a <see cref="XamDataGrid"/> or <see cref="XamDataCarousel"/> control since it is a bit more convenient to use - you can access the View's 
    /// ViewSettings property directly off the control without having to drill into the View itself.</p>
	/// <p class="body">Refer to the <a href="xamData_TheoryOfOperation.html">Theory of Operation</a> topic in the Developer's Guide for an explanation of how this control operates.</p>
    /// <p class="body">Refer to the <a href="WPF_Working_with_xamDataGrid_xamDataCarousel_and_xamDataPresenter_Styling_Points.html">Working with xamDataGrid, xamDataCarousel and xamDataPresenter Styling Points</a> topic in the Developer's Guide for an explanation of how to style the control.</p>
	/// <p class="body">Refer to the <a href="xamCarousel_Terms_Architecture.html">Carousel Architecture Overview</a> topic in the Developer's Guide for an explanation of how Carousel presentation works.</p>
	/// </remarks>
    /// <seealso cref="ViewBase"/>
    /// <seealso cref="GridView"/>
    /// <seealso cref="GridViewSettings"/>
    /// <seealso cref="CarouselView"/>
    /// <seealso cref="CarouselViewSettings"/>
    /// <seealso cref="View"/>
    /// <seealso cref="XamDataCarousel"/>
    /// <seealso cref="XamDataGrid"/>
	[ContentProperty("DataItems")]
	[TemplatePart(Name = "PART_ContentSiteGrid", Type = typeof(Grid))]
	
	//[ToolboxItem(true)]
	
	//[System.Drawing.ToolboxBitmap(typeof(XamDataPresenter), AssemblyVersion.ToolBoxBitmapFolder + "XamDataPresenter.bmp")]
	//[Description("A DataPresenterBase derived control that can display data using different layouts or 'views', depending on the setting of its View property.")]
	// AS 4/15/10 The base class has this set to false so DataPresenterBase doesn't show up but we need to set it back to true for the derived controls that should show up.
	[DesignTimeVisible(true)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class XamDataPresenter : DataPresenterBase
	{
		#region Member Variables

		private UltraLicense							_license = null;

		#endregion //Member Variables	
    
		#region Constructor

		static XamDataPresenter()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(XamDataPresenter), new FrameworkPropertyMetadata(typeof(XamDataPresenter)));

			//			KeyboardNavigation.DirectionalNavigationProperty.OverrideMetadata(typeof(DataPresenterBase), new FrameworkPropertyMetadata(KeyboardNavigationMode.Cycle));


			// Load our commands.  (Since our commands are defined as statics and static initializers are not called
			// until the class is accessed, call the convenience method 'LoadCommands' which will access the class
			// and force the loading of our static commands and register command bindings)
			DataPresenterCommands.LoadCommands();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="XamDataPresenter"/> class
		/// </summary>
		public XamDataPresenter()
		{
			// verify and cache the license
			//
			// Wrapped in a try/catch for a FileNotFoundException.
			// When the assembly is loaded dynamically, VS seems 
			// to be trying to reload a copy of Shared even though 
			// one is in memory. This generates a FileNotFoundException
			// when the dll is not in the gac and not in the AppBase
			// for the AppDomain.
			//
			// AS 11/7/07 BR21903
			// Always do the license checks.
			//
			//if (DesignerProperties.GetIsInDesignMode(this))
			{
				try
				{
					// We need to pass our type into the method since we do not want to pass in 
					// the derived type.
					this._license = LicenseManager.Validate(typeof(XamDataPresenter), this) as UltraLicense;
				}
				catch (System.IO.FileNotFoundException) { }
			}
		}

		#endregion //Constructor

		#region Base class overrides

			#region OnInitialized

		/// <summary>
		/// Called after the element has been initialized.
		/// </summary>
		protected override void OnInitialized(EventArgs e)
		{
            // JJD 1/12/07 - BR29627
            // Moved calling the base implemenation until after the View is initialized 
			//base.OnInitialized(e);

            // JJD 1/2/07 
            // Check for design mode. If false then sync up the View DP property via SetValue.
            // Otherwise, just calling the get below will cause us to initialize CurrentViewInternal
            //
			//this.SetValue(XamDataPresenter.ViewProperty, this.View);
            ViewBase view = this.View;

            if (DesignerProperties.GetIsInDesignMode(this) == false)
			    this.SetValue(XamDataPresenter.ViewProperty, view);

            // JJD 1/12/07 - BR29627
            // Moved calling the base implemenation until after the View is initialized 
            base.OnInitialized(e);
            
		}

			#endregion //OnInitialized	

			#region OnPropertyChanged

		/// <summary>
		/// Called when a dependency property has changed
		/// </summary>
		/// <param name="e">The arguments identifying the property and containing the new and old values.</param>
		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			DependencyProperty property = e.Property;

			if (property == XamDataPresenter.ViewProperty)
			{
				if (e.NewValue is ViewBase  &&  e.NewValue != this.CurrentViewInternal)
					this.CurrentViewInternal = e.NewValue as ViewBase;

				// MD 5/26/10 - ChildRecordsDisplayOrder feature
				// When the view changes, the resolved ChildRecordsDisplayOrder value may change on the FieldLayouts, so we should
				// clear the cached value on each one.
				foreach (FieldLayout fi in this.FieldLayouts)
					fi.ResetCachedChildRecordsDisplayOrderResolvedValue();
			}
		}

			#endregion //OnPropertyChanged	

		#endregion //Base class overrides

		#region Properties

			#region Public properties

				#region View

		/// <summary>
        /// Identifies the <see cref="View"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ViewProperty = DependencyProperty.Register("View",
			typeof(ViewBase), typeof(XamDataPresenter), new FrameworkPropertyMetadata((ViewBase)null, null, new CoerceValueCallback(OnCoerceView)));

		private static object OnCoerceView(DependencyObject d, object value)
		{
            if (value == null)
            {
                // JJD 1/2/07 
                // Check for design mode first
                // Only create a GridView if we are in run mode 
                if (DesignerProperties.GetIsInDesignMode(d) == false)
                    return new GridView();
            }

			return value;
		}

        /// <summary>
        /// Returns/sets the <see cref="ViewBase"/> derived View that the <see cref="XamDataPresenter"/> uses to display data.  If this property is not set 
        /// a <see cref="GridView"/> instance is provided by default.
        /// </summary>
        /// <remarks>
        /// <p class="body">This property can be set to either of the two views shipped with NetAdvantage for WPF 7.1 - <see cref="GridView"/> and <see cref="CarouselView"/> as well as any <see cref="ViewBase"/> derived view.
		/// You can extend the <see cref="ViewBase"/> class to provide additional custom views.  Refer to the <a href="xamDataPresenter_Creating_Custom_Views.html">Creating Custom Views for XamDataPresenter</a> topic in the 
		/// Developer's Guide for an explanation of how to create your own custom views.</p>
        /// <p class="body">To change the View used by the control at designtime or runtime, simply set this property to an instance of a <see cref="GridView"/>, <see cref="CarouselView"/> 
        /// or a custom <see cref="ViewBase"/> derived object</p>
        /// </remarks>
        /// <seealso cref="ViewBase"/>
        /// <seealso cref="GridView"/>
        /// <seealso cref="GridViewSettings"/>
        /// <seealso cref="CarouselView"/>
        /// <seealso cref="CarouselViewSettings"/>
        /// <seealso cref="View"/>
        /// <seealso cref="XamDataCarousel"/>
        /// <seealso cref="XamDataGrid"/>
        //[Description("Returns/sets the View to be used to present data in the control.")]
        //[Category("Appearance")]
		[Bindable(true)]
		[DefaultValue(null)]
		public ViewBase View
        {
            get
            {
                ViewBase view = this.GetValue(XamDataPresenter.ViewProperty) as ViewBase;

                if (view == null)
                {
                    if (this.CurrentViewInternal == null)
                        this.CurrentViewInternal = new GridView();

                    // JJD 1/2/07 
                    // Check for design mode first
                    // If we are in design mode return null 
                    if (DesignerProperties.GetIsInDesignMode(this))
                        return null;
                }

				return this.CurrentViewInternal;
			}
            set
            {
                this.SetValue(XamDataPresenter.ViewProperty, value);
            }
        }

                #endregion //View

			#endregion //Public properties

		#endregion //Properties
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