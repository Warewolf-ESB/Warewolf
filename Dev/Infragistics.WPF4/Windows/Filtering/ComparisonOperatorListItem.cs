using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.ObjectModel;
using System.Globalization;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Infragistics.Shared;
using Infragistics.Windows.Internal;

namespace Infragistics.Windows.Controls
{
    /// <summary>
    /// Represents an entry in the filter operator drop-down list of a ComparisonOperatorSelector.
    /// </summary>
    /// <seealso cref="ComparisonOperatorSelector"/>
    /// <seealso cref="ComparisonOperator"/>
    public class ComparisonOperatorListItem : DependencyObject
    {
        #region Member Vars

        private ComparisonOperator _operator;
        private DynamicResourceString _dynamicString;

		// AS 9/14/09 TFS22121
		private PropertyValueTracker _dynamicStringTracker;

        #endregion // Member Vars

        #region Constructor

        /// <summary>
        /// Initializes a new instance of <see cref="ComparisonOperatorListItem"/>.
        /// </summary>
        /// <param name="value">The specific operator enum value.</param>
        public ComparisonOperatorListItem(ComparisonOperator value)
        {
            this._operator = value;
            this._dynamicString = Infragistics.Windows.Resources.GetDynamicResourceString("ComparisonOperator_" + this._operator.ToString());
			// AS 9/14/09 TFS22121
			// We don't want to hook its event since that will root the instance.
			// 
            //this._dynamicString.PropertyChanged += new PropertyChangedEventHandler(OnDynamicStringPropertyChanged);
			this._dynamicStringTracker = new PropertyValueTracker(_dynamicString, "Value", new PropertyValueTracker.PropertyValueChangedHandler(UpdateDisplayText));
			this.UpdateDisplayText();
		}

        /// <summary>
        /// Initializes a new instance of <see cref="ComparisonOperatorListItem"/>.
        /// </summary>
        public ComparisonOperatorListItem()
            : this(ComparisonOperator.Equals)
        {
        }

        #endregion // Constructor

        #region Base class overrides

        /// <summary>
        /// Returns the description.
        /// </summary>
        public override string ToString()
        {
            return this.Description;
        }

        #endregion //Base class overrides	
    
        #region Properties

        #region Public Properties

            #region Image

        /// <summary>
        /// Identifies the <see cref="Image"/> dependency property
        /// </summary>
        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image",
            typeof(ImageSource), typeof(ComparisonOperatorListItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure));

        /// <summary>
        /// Gets or sets the Image used to represent the operator
        /// </summary>
        /// <seealso cref="ImageProperty"/>
        //[Description("Gets or sets the Image used to represent the operator")]
        //[Category("Appearance")]
        public ImageSource Image
        {
            get
            {
                return (ImageSource)this.GetValue(ComparisonOperatorListItem.ImageProperty);
            }
            set
            {
                this.SetValue(ComparisonOperatorListItem.ImageProperty, value);
            }
        }

            #endregion //Image

            #region Description

        private static readonly DependencyPropertyKey DescriptionPropertyKey =
            DependencyProperty.RegisterReadOnly("Description",
            typeof(string), typeof(ComparisonOperatorListItem), new FrameworkPropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="Description"/> dependency property
        /// </summary>
        public static readonly DependencyProperty DescriptionProperty =
            DescriptionPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets a the description of the operator that can be used for display (read-only).
        /// </summary>
        /// <remarks>
        /// <para cass="note"><b>Note:</b> this value is obtained from resources in this assembly. They can be changed 
        /// programmatically by calling the <see cref="ResourceCustomizer.SetCustomizedString(string,string)"/> method 
        /// of the <see cref="Infragistics.Windows.Resources.Customizer"/> exposed off Infragistics.Windows.Resources. The identifier 
        /// strings for these resources begin with 'ComparisonOperator_' and are appended with the enum string. For example, 
        /// 'ComparisonOperator_Equals', 'ComparisonOperator_NotEquals' etc.</para>
        /// </remarks>
        /// <seealso cref="DescriptionProperty"/>
        //[Description("Gets a the description of the operator that can be used for display (read-only).")]
        //[Category("Appearance")]
        [Bindable(true)]
        public string Description
        {
            get
            {
                 return (string)this.GetValue(ComparisonOperatorListItem.DescriptionProperty);
            }
        }

            #endregion //Description
	
            #region Operator

        /// <summary>
        /// Returns the <see cref="ComparisonOperator"/> this item represents (read-only).
        /// </summary>
        [ReadOnly(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ComparisonOperator Operator
        {
            get
            {
                return this._operator;
            }
        }

            #endregion //Operator	
    
        #endregion // Public Properties

        #endregion // Properties

        #region Methods

            #region Private Methods

                #region OnDynamicStringPropertyChanged

		
#region Infragistics Source Cleanup (Region)




#endregion // Infragistics Source Cleanup (Region)

                #endregion //OnDynamicStringPropertyChanged	
    
                #region UpdateDisplayText

        private void UpdateDisplayText()
        {
            this.SetValue(DescriptionPropertyKey, this._dynamicString.Value);
        }

                #endregion //UpdateDisplayText	
    
            #endregion //Private Methods	
    
        #endregion //Methods
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