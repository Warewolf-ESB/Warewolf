using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Windows.Media;
//using System.Windows.Events;
using System.Windows.Media.Animation;
using System.Windows.Automation;
using System.Windows.Automation.Provider;
using System.Windows.Markup;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Data;
using System.Globalization;
using Infragistics.Windows.Helpers;
using Infragistics.Windows.Automation.Peers.DataPresenter;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.DataPresenter
{
    /// <summary>
	/// A control that contains all the elements that make up the header area in the <see cref="DataPresenterBase"/> controls that display a separate header area such as in <see cref="XamDataGrid"/>. The HeaderPresenter contains the <see cref="HeaderPrefixArea"/> and <see cref="HeaderLabelArea"/> controls.  It is used primarily for styling the outermost portion of the header area.
    /// </summary>
	//[Description("A control that contains all the elements that make up the header area in the 'DataPresenterBase' derived controls that display a separate header area such as in 'XamDataGrid'.  The HeaderPresenter contains the 'HeaderPrefixArea' and 'HeaderLabelArea' controls.  It is used primarily for styling the outermost portion of the header area.")]
	[DesignTimeVisible(false)]	// JM 02-18-10 - DO NOT MOVE TO DESIGN ASSEMBLY!!!
	public class HeaderPresenter : ContentControl
    {
        #region Member Variables

        private int _cachedVersion;
        private bool _versionInitialized;
        private FieldLayout _fieldLayout;
        private StyleSelectorHelper _styleSelectorHelper;
		
		// JJD 2/9/11 - TFS63916 - added
		private PropertyValueTracker _tracker;
		private RecordPresenter _rp;

        #endregion Member Variables

        #region Constructors

		static HeaderPresenter()
		{
			FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(HeaderPresenter), new FrameworkPropertyMetadata(typeof(HeaderPresenter)));

			// AS 11/8/11 TFS88111 - Added Stretch HorizontalContentAlignment
			Control.HorizontalContentAlignmentProperty.OverrideMetadata(typeof(HeaderPresenter), new FrameworkPropertyMetadata(KnownBoxes.HorizontalAlignmentStretchBox));
		}

        /// <summary>
		/// Initializes a new instance of the <see cref="HeaderPresenter"/> class
        /// </summary>
        public HeaderPresenter()
        {
            // initialize the styleSelectorHelper
            this._styleSelectorHelper = new StyleSelectorHelper(this);
        }

        #endregion Constructors

        #region Base class overrids

			#region OnCreateAutomationPeer
		/// <summary>
		/// Returns an automation peer that exposes the <see cref="HeaderPresenter"/> to UI Automation.
		/// </summary>
		/// <returns>A <see cref="Infragistics.Windows.Automation.Peers.DataPresenter.HeaderPresenterAutomationPeer"/></returns>
		protected override System.Windows.Automation.Peers.AutomationPeer OnCreateAutomationPeer()
		{
			return new HeaderPresenterAutomationPeer(this);
		}
			#endregion //OnCreateAutomationPeer

            #region OnPropertyChanged

        /// <summary>
        /// Called when a property is changed
        /// </summary>
        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == ContentProperty)
            {
                this._fieldLayout = this.Content as FieldLayout;
                this._styleSelectorHelper.InvalidateStyle();
            }
            else if (e.Property == InternalVersionProperty)
            {
                int version = this.InternalVersion;

                if (this._cachedVersion != version)
                {
                    this._cachedVersion = version;

                    if (this._versionInitialized == true)
                    {
                        this._styleSelectorHelper.InvalidateStyle();
                    }
                }

                this._versionInitialized = true;
            }
        }

            #endregion //OnPropertyChanged

        #endregion //Base class overrids

        #region Properties

            #region Public Properties

				// JJD 2/9/11 - TFS63916 - added attached inherited property
				#region RecordManager

		internal static readonly DependencyPropertyKey RecordManagerPropertyKey =
			DependencyProperty.RegisterAttachedReadOnly("RecordManager",
			typeof(RecordManager), typeof(HeaderPresenter), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits
				));

		/// <summary>
		/// Identifies the RecordManager" attached readonly dependency property
		/// </summary>
		/// <seealso cref="GetRecordManager"/>
		public static readonly DependencyProperty RecordManagerProperty =
			RecordManagerPropertyKey.DependencyProperty;


		/// <summary>
		/// Gets the value of the 'RecordManager' attached readonly property
		/// </summary>
		/// <seealso cref="RecordManagerProperty"/>
		public static RecordManager GetRecordManager(DependencyObject d)
		{
			return (RecordManager)d.GetValue(HeaderPresenter.RecordManagerProperty);
		}

				#endregion //RecordManager

            #endregion //Public Properties

            #region Internal Properties

                #region InternalVersion

        internal static readonly DependencyProperty InternalVersionProperty = DependencyProperty.Register("InternalVersion",
            typeof(int), typeof(HeaderPresenter), new FrameworkPropertyMetadata(0));
        //            typeof(int), typeof(HeaderPresenter), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsMeasure));

        internal int InternalVersion
        {
            get
            {
                return (int)this.GetValue(HeaderPresenter.InternalVersionProperty);
            }
            set
            {
                this.SetValue(HeaderPresenter.InternalVersionProperty, value);
            }
        }

                #endregion //InternalVersion

				// JJD 2/9/11 - TFS63916 - added
				#region RecordPresenter

		internal RecordPresenter RecordPresenter
		{
			get { return this._rp; }
			set
			{
				if (value != _rp)
				{
					_tracker = null;
					_rp = value;

					if (_rp != null)
						this._tracker = new PropertyValueTracker(_rp, RecordPresenter.RecordManagerForHeaderProperty, this.OnRecordManagerChanged);

					this.OnRecordManagerChanged();
				}
			}
		}

				#endregion //RecordPresenter	
    			
			#endregion //Internal Properties

		#endregion //Properties

		#region Methods

			// JJD 2/9/11 - TFS63916 - added
			#region OnRecordManagerChanged

		private void OnRecordManagerChanged()
		{
			RecordManager rm = _rp != null ? _rp.RecordManagerForHeader : null;

			if (rm != null)
				this.SetValue(RecordManagerPropertyKey, rm);
			else
				this.ClearValue(RecordManagerPropertyKey);
		}

			#endregion //OnRecordManagerChanged	
    
		#endregion //Methods

		#region StyleSelectorHelper private class

		private class StyleSelectorHelper : StyleSelectorHelperBase
		{
			private HeaderPresenter _hp;

			internal StyleSelectorHelper(HeaderPresenter hp) : base(hp)
			{
				this._hp = hp;
			}

			/// <summary>
			/// The style to be used as the source of a binding (read-only)
			/// </summary>
			public override Style Style
			{
				get
				{
					if (this._hp == null)
						return null;

					FieldLayout fl = this._hp._fieldLayout;

					if (fl != null)
					{
						DataPresenterBase dp = fl.DataPresenter;

						if (dp != null)
							return dp.InternalHeaderPresenterStyleSelector.SelectStyle(fl, this._hp);
					}

					return null;
				}
			}
		}

		#endregion //StyleSelectorHelper private class	
    
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