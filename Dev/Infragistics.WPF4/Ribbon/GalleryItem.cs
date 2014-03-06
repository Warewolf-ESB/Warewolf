using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media;
using Infragistics.Windows.Helpers;
using System.Windows.Input;
using Infragistics.Shared;

namespace Infragistics.Windows.Ribbon
{
	/// <summary>
	/// Represents an item in a <see cref="GalleryTool"/> that displays an image and/or text.  GalleryItems can appear in the <see cref="GalleryTool"/> 
	/// preview area within a RibbonGroup and also in the GalleryTool dropdown.
	/// </summary>
	/// <remarks>
	/// <p class="body">While GalleryItems can appear in both the <see cref="GalleryTool"/> preview area and <see cref="GalleryTool"/> dropdown, GalleryItems
	/// only appear inside assigned <see cref="GalleryItemGroup"/>s in the <see cref="GalleryTool"/> dropdown since the <see cref="GalleryTool"/> preview area 
	/// never displays <see cref="GalleryItemGroup"/>s.  By default, the <see cref="GalleryTool"/> contains no <see cref="GalleryItemGroup"/>s and therefore 
	/// GalleryItems by default do not appear within a <see cref="GalleryItemGroup"/> in the dropdown area.  To display a GalleryItem in a 
	/// <see cref="GalleryItemGroup"/> in the dropdown, you must add the GalleryItem's <see cref="Key"/> value to the <see cref="GalleryItemGroup.ItemKeys"/>
	/// collection for each <see cref="GalleryItemGroup"/> in which you would like the GalleryItem to appear.</p>
	/// <p class="note"><b>Note: </b>You may assign GalleryItems to more than one <see cref="GalleryItemGroup"/>.  In this case they will appear in multiple
	/// <see cref="GalleryItemGroup"/>s in the <see cref="GalleryTool"/> dropdown.  However, they will only appear once in the <see cref="GalleryTool"/> preview area.</p>
	/// </remarks>
	/// <seealso cref="GalleryTool"/>
	public class GalleryItem : DependencyObjectNotifier
	{
		#region Member Variables

		private string													_key = null;

		// JM 10-23-08 TFS7560
		private GalleryTool												_galleryTool;

		#endregion //Member Variables

		#region Constructor

		/// <summary>
		/// Initializes an instance of the <see cref="GalleryItem"/> class.
		/// </summary>
		public GalleryItem()
		{
		}

		static GalleryItem()
		{
			KeyboardNavigation.IsTabStopProperty.OverrideMetadata(typeof(GalleryItem), new FrameworkPropertyMetadata(KnownBoxes.TrueBox));
		}

		#endregion //Constructor

        #region Base class overrides

            #region OnPropertyChanged

        /// <summary>
        /// Called when a property has been changed.
        /// </summary>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

			this.RaisePropertyChangedEvent(e.Property.Name);
        }

            #endregion //OnPropertyChanged

        #endregion //Base class overrides

		#region Properties

			#region Public Properties

				#region ColumnSpan

		/// <summary>
		/// Identifies the <see cref="ColumnSpan"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ColumnSpanProperty = DependencyProperty.Register("ColumnSpan",
			typeof(int), typeof(GalleryItem), new FrameworkPropertyMetadata(1));

		/// <summary>
		/// Returns/sets number of logical columns the <see cref="GalleryItem"/> should span when it is displayed in the <see cref="GalleryTool"/> dropdown.
		/// </summary>
		/// <exception cref="ArgumentOutOfRangeException">If the property is set to a value that is less than 1.</exception>
		/// <seealso cref="GalleryItem"/>
		/// <seealso cref="ColumnSpanProperty"/>
		/// <seealso cref="GalleryTool.MaxDropDownColumns"/>
		/// <seealso cref="GalleryTool.MinDropDownColumns"/>
		/// <seealso cref="GalleryTool.MaxPreviewColumns"/>
		/// <seealso cref="GalleryTool.MinPreviewColumns"/>
		//[Description("Returns/sets number of logical columns the GalleryItem should span when it is displayed in the GalleryTool dropdown.")]
		//[Category("Ribbon Properties")]
		[DefaultValue(1)]
		public int ColumnSpan
		{
			get
			{
				return (int)this.GetValue(GalleryItem.ColumnSpanProperty);
			}
			set
			{
				this.SetValue(GalleryItem.ColumnSpanProperty, value);
			}
		}

				#endregion //ColumnSpan

				#region Image

		/// <summary>
		/// Identifies the <see cref="Image"/> dependency property
		/// </summary>
		public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image",
			typeof(ImageSource), typeof(GalleryItem), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets the image to display for the item.
		/// </summary>
		/// <seealso cref="ImageProperty"/>
		/// <seealso cref="Text"/>
		/// <seealso cref="GalleryItemSettings.SelectionDisplayMode"/>
		//[Description("Returns/sets the image to display for the item.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public ImageSource Image
		{
			get
			{
				return (ImageSource)this.GetValue(GalleryItem.ImageProperty);
			}
			set
			{
				this.SetValue(GalleryItem.ImageProperty, value);
			}
		}

				#endregion //Image

				#region IsSelected

		/// <summary>
		/// Identifies the <see cref="IsSelected"/> dependency property
		/// </summary>
		public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected",
			typeof(bool), typeof(GalleryItem), new FrameworkPropertyMetadata(KnownBoxes.FalseBox, new PropertyChangedCallback(OnIsSelectedChanged)));

		private static void OnIsSelectedChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			GalleryItem galleryItem = target as GalleryItem;
			if (galleryItem != null)
			{
                // AS 3/10/09 - EventArgs not used
                //galleryItem.RaiseIsSelectedChangedInternal(null);
				galleryItem.RaiseIsSelectedChangedInternal();

                // AS 3/10/09 TFS15169
                // We were assuming that the gallery was selected. We should only call 
                // SetSelectedItem if the item is being selected. If its being unselected 
                // and it is the SelectedItem then we should clear the SelectedItem.
                //
                //// JM 10-23-08 TFS7560
                //if (galleryItem.GalleryToolInternal != null)
                //    galleryItem.GalleryToolInternal.SetSelectedItem(galleryItem, null, null);
                GalleryTool gallery = galleryItem.GalleryToolInternal;

                if (null != gallery)
                {
                    if (false.Equals(e.NewValue))
                    {
                        if (gallery.SelectedItem == galleryItem)
                            gallery.SelectedItem = null;
                    }
                    else
                    {
                        gallery.SetSelectedItem(galleryItem, null, null);
                    }
                }
			}
		}

		/// <summary>
		/// Returns/sets whether this <see cref="GalleryItem"/> is the selected item in the <see cref="GalleryTool"/>.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>If the <see cref="GalleryItem"/> is selected and it has been assigned to multiple <see cref="GalleryItemGroup"/>s, 
		/// it will appear selected in each of those <see cref="GalleryItemGroup"/>s.</p>
		/// </remarks>
		/// <seealso cref="IsSelectedProperty"/>
		/// <seealso cref="GalleryTool.ItemBehavior"/>
		/// <seealso cref="GalleryTool.ItemSelectedEvent"/>
		/// <seealso cref="GalleryItemGroup"/>
		/// <seealso cref="GalleryItemGroup.ItemKeys"/>
		//[Description("Returns/sets whether this GalleryItem is the selected item in the GalleryTool.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public bool IsSelected
		{
			get
			{
				return (bool)this.GetValue(GalleryItem.IsSelectedProperty);
			}
			set
			{
				this.SetValue(GalleryItem.IsSelectedProperty, value);
			}
		}

				#endregion //IsSelected

				#region Key

		/// <summary>
		/// Returns/sets the string key associated with this GalleryItem.
		/// </summary>
		/// <remarks>
		/// <p class="body">To assign the <see cref="GalleryItem"/> to one or more <see cref="GalleryItemGroup"/>s, add the <see cref="GalleryItem"/>'s key
		/// to the groups' <see cref="GalleryItemGroup.ItemKeys"/> collection.</p>
		/// <p class="note"><b>Note: </b><see cref="GalleryItem"/> keys must be unique within the <see cref="GalleryTool.Items"/> collection of the 
		/// <see cref="GalleryTool"/>.</p>
		/// </remarks>
		/// <exception cref="ArgumentOutOfRangeException">If the property is set to null or string.Empty.</exception>
		/// <exception cref="InvalidOperationException">If an attempt is made to add a <see cref="GalleryItem"/> whose <see cref="GalleryItem.Key"/> property value conflicts
		/// with the value of the <see cref="GalleryItem.Key"/> property of an existing <see cref="GalleryItem"/> in the collection</exception>
		/// <seealso cref="GalleryItem"/>
		/// <seealso cref="GalleryTool"/>
		/// <seealso cref="GalleryItemGroup"/>
		/// <seealso cref="GalleryItemGroup.ItemKeys"/>
		//[Description("Returns/sets the string key associated with this GalleryItem.")]
		//[Category("Ribbon Properties")]
		[DefaultValue(null)]
		public string Key
		{
			get { return this._key; }
			set
			{
				if (value != this._key)
				{
					if (value == null || value == string.Empty)
						throw new ArgumentException(XamRibbon.GetString("LE_InvalidGalleryItemKey"));

					this._key = value;
				}
			}
		}

				#endregion //Key	

				#region Settings

		/// <summary>
		/// Identifies the <see cref="Settings"/> dependency property
		/// </summary>
		public static readonly DependencyProperty SettingsProperty = DependencyProperty.Register("Settings",
			typeof(GalleryItemSettings), typeof(GalleryItem), new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnSettingsChanged)));

		private static void OnSettingsChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
		{
			GalleryItem galleryItem = target as GalleryItem;
			if (galleryItem != null)
			{
				if (e.OldValue != null)
					((GalleryItemSettings)e.OldValue).PropertyChanged -= new PropertyChangedEventHandler(galleryItem.OnSettingsPropertyChanged);

				if (e.NewValue != null)
					((GalleryItemSettings)e.NewValue).PropertyChanged += new PropertyChangedEventHandler(galleryItem.OnSettingsPropertyChanged);
			}
		}

		/// <summary>
		/// Returns/sets the settings that will be used for the <see cref="GalleryItem"/>.
		/// </summary>
		/// <remarks>
		/// <p class="body">The various property values in the <see cref="GalleryItemSettings"/> specified at the <see cref="GalleryTool"/> level 
		/// (via the <see cref="GalleryTool.ItemSettings"/> property) serve as the ultimate defaults for all <see cref="GalleryItem"/>s.  These values 
		/// can be overridden at two lower levels:
		/// <ul>
		/// <li>at the <see cref="GalleryItemGroup"/> level via the <see cref="GalleryItemGroup.ItemSettings"/> property.  The values specified there
		/// will override corresponding values set at the <see cref="GalleryTool"/> level, but could be further overridden at the <see cref="GalleryItem"/>
		/// level (see next bullet)</li>
		/// <li>at the <see cref="GalleryItem"/> level via the <see cref="GalleryItem.Settings"/> property.  The values specified here will override corresponding 
		/// values set at the <see cref="GalleryTool"/> and <see cref="GalleryItemGroup"/> levels</li>
		/// </ul>
		/// </p>
		/// </remarks>
		/// <seealso cref="SettingsProperty"/>
		/// <seealso cref="GalleryItem"/>
		/// <seealso cref="GalleryItemGroup"/>
		//[Description("Returns/sets the settings that will be used for the GalleryItem.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public GalleryItemSettings Settings
		{
			get
			{
				return (GalleryItemSettings)this.GetValue(GalleryItem.SettingsProperty);
			}
			set
			{
				this.SetValue(GalleryItem.SettingsProperty, value);
			}
		}

				#endregion //Settings

				#region Text

		/// <summary>
		/// Identifies the <see cref="Text"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text",
			typeof(string), typeof(GalleryItem), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets the text that is displayed for the <see cref="GalleryItem"/> when it is displayed in the <see cref="GalleryTool"/> dropdown.
		/// </summary>
		/// <remarks>
		/// <p class="note"><b>Note: </b>The string specified here is never displayed when the <see cref="GalleryItem"/> is being shown in the 
		/// <see cref="GalleryTool"/> preview area.  It is however, conditionally shown when the <see cref="GalleryItem"/> is in the 
		/// <see cref="GalleryTool"/> dropdown based on the setting of the <see cref="GalleryItemSettings.TextDisplayMode"/> property in effect for the 
		/// <see cref="GalleryItem"/> (see the <see cref="Settings"/> property for a description of how <see cref="GalleryItemSettings"/> are applied 
		/// and overridden).</p>
		/// </remarks>
		/// <seealso cref="TextProperty"/>
		/// <seealso cref="Image"/>
		/// <seealso cref="GalleryItemSettings.SelectionDisplayMode"/>
		/// <seealso cref="GalleryItemSettings.TextDisplayMode"/>
		//[Description("Returns/sets the text that is displayed for the GalleryItem when it is displayed in the GalleryTool dropdown.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public string Text
		{
			get
			{
				return (string)this.GetValue(GalleryItem.TextProperty);
			}
			set
			{
				this.SetValue(GalleryItem.TextProperty, value);
			}
		}

				#endregion //Text

				#region Tag

		/// <summary>
		/// Identifies the <see cref="Tag"/> dependency property
		/// </summary>
		public static readonly DependencyProperty TagProperty = FrameworkElement.TagProperty.AddOwner(typeof(GalleryItem), new FrameworkPropertyMetadata(null));

		/// <summary>
		/// Returns/sets optional user data that is stored with the GalleryItem.
		/// </summary>
		/// <seealso cref="TagProperty"/>
		//[Description("Returns/sets optional user data that is stored with the GalleryItem.")]
		//[Category("Ribbon Properties")]
		[Bindable(true)]
		public object Tag
		{
			get
			{
				return (object)this.GetValue(GalleryItem.TagProperty);
			}
			set
			{
				this.SetValue(GalleryItem.TagProperty, value);
			}
		}

				#endregion //Tag

			#endregion //Public Properties

			#region Internal Properties

				// JM 10-23-08 TFS7560
				#region GalleryTool

		internal GalleryTool GalleryToolInternal
		{
			get { return this._galleryTool; }
			set 
			{
				if (value != this._galleryTool)
					this._galleryTool = value;
			}
		}


				#endregion //GalleryTool	

				#region SettingsVersion

		private static readonly DependencyPropertyKey SettingsVersionPropertyKey =
			DependencyProperty.RegisterReadOnly("SettingsVersion",
			typeof(int), typeof(GalleryItem), new FrameworkPropertyMetadata(0));

		internal static readonly DependencyProperty SettingsVersionProperty =
			SettingsVersionPropertyKey.DependencyProperty;

				#endregion //SettingsVersion

			#endregion //Internal Properties

		#endregion //Properties

		#region Methods

			#region Private Methods

				#region OnSettingsPropertyChanged

		private void OnSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			int currentVersion	= (int)this.GetValue(GalleryItem.SettingsVersionProperty);
			int newVersion		= ++currentVersion;
			this.SetValue(GalleryItem.SettingsVersionPropertyKey, newVersion);
		}

				#endregion //OnSettingsProeprtyChanged	

				#region RaiseIsSelectedChangedInternal

        // AS 3/10/09 - EventArgs not used
		//private void RaiseIsSelectedChangedInternal(EventArgs e)
		private void RaiseIsSelectedChangedInternal()
		{
			if (this.IsSelectedChangedInternal != null)
				this.IsSelectedChangedInternal(this, EventArgs.Empty);
		}

				#endregion //RaiseIsSelectedChangedInternal	
    
			#endregion //Private methods

		#endregion //Methods

		#region Events

		internal event EventHandler IsSelectedChangedInternal;

		#endregion //Events
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