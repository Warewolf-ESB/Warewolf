using System.ComponentModel;
using System.Windows;

namespace Infragistics.Controls.Menus
{
    /// <summary>
    /// A class which defines the editing settings which will be applied a single <see cref="NodeLayout"/> of a <see cref="XamDataTree"/>.
    /// </summary>
    public class TreeEditingSettingsOverride : SettingsBaseOverride
    {
        #region Overrides

        #region SettingsObject

        /// <summary>
        /// Gets the <see cref="SettingsBase"/> that is the counterpart to this <see cref="SettingsBaseOverride"/>
        /// </summary>
        protected override SettingsBase SettingsObject
        {
            get
            {
                SettingsBase settings = null;
                if (this.NodeLayout != null && this.NodeLayout.Tree != null)
                    settings = this.NodeLayout.Tree.EditingSettings;
                return settings;
            }
        }

        #endregion // SettingsObject

        #endregion // Overrides

        #region Properties

        #region AllowEditing

        /// <summary>
        /// Identifies the <see cref="AllowEditing"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AllowEditingProperty = DependencyProperty.Register("AllowEditing", typeof(bool?), typeof(TreeEditingSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(AllowEditingChanged)));

        /// <summary>
        /// Gets/Sets if Editing is enabled, and if so, what mode it should be in for a particular <see cref="NodeLayout"/>
        /// </summary>
        [TypeConverter(typeof(NullableBoolConverter))]
        public bool? AllowEditing
        {
            get { return (bool?)this.GetValue(AllowEditingProperty); }
            set { this.SetValue(AllowEditingProperty, value); }
        }

        private static void AllowEditingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            TreeEditingSettingsOverride settings = (TreeEditingSettingsOverride)obj;
            settings.OnPropertyChanged("AllowEditing");
        }

        #endregion // AllowEditing

        #region AllowEditingResolved

        /// <summary>
        /// Resolves the <see cref="TreeEditingSettingsOverride.AllowEditing"/> property for a particular <see cref="NodeLayout"/>
        /// </summary>
        public bool AllowEditingResolved
        {
            get
            {
                if (this.AllowEditing == null && this.SettingsObject != null)
                    return ((TreeEditingSettings)this.SettingsObject).AllowEditing;
                else
                    return (bool)this.AllowEditing;
            }
        }

        #endregion //AllowEditingResolved

        #region IsF2EditingEnabled

        /// <summary>
        /// Identifies the <see cref="IsF2EditingEnabled"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsF2EditingEnabledProperty = DependencyProperty.Register("IsF2EditingEnabled", typeof(bool?), typeof(TreeEditingSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(IsF2EditingEnabledChanged)));

        /// <summary>
        /// Gets/Sets if pressing the F2 key will cause the ActiveCell to enter edit mode, on a particular <see cref="NodeLayout"/>.
        /// </summary>
        [TypeConverter(typeof(NullableBoolConverter))]
        public bool? IsF2EditingEnabled
        {
            get { return (bool?)this.GetValue(IsF2EditingEnabledProperty); }
            set { this.SetValue(IsF2EditingEnabledProperty, value); }
        }

        private static void IsF2EditingEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            TreeEditingSettingsOverride settings = (TreeEditingSettingsOverride)obj;
            settings.OnPropertyChanged("IsF2EditingEnabled");
        }

        #endregion // IsF2EditingEnabled

        #region IsF2EditingEnabledResolved

        /// <summary>
        /// Resolves the <see cref="TreeEditingSettingsOverride.IsF2EditingEnabled"/> property for a particular <see cref="NodeLayout"/>
        /// </summary>
        public bool IsF2EditingEnabledResolved
        {
            get
            {
                if (this.IsF2EditingEnabled == null && this.SettingsObject != null)
                    return ((TreeEditingSettings)this.SettingsObject).IsF2EditingEnabled;
                else
                    return (bool)this.IsF2EditingEnabled;
            }
        }

        #endregion //IsF2EditingEnabledResolved

        #region IsEnterKeyEditingEnabled

        /// <summary>
        /// Identifies the <see cref="IsEnterKeyEditingEnabled"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsEnterKeyEditingEnabledProperty = DependencyProperty.Register("IsEnterKeyEditingEnabled", typeof(bool?), typeof(TreeEditingSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(IsEnterKeyEditingEnabledChanged)));

        /// <summary>
        /// Gets/Sets if pressing the Enter ke will cause the ActiveCell to enter edit mode on a particular <see cref="NodeLayout"/>.
        /// </summary>
        [TypeConverter(typeof(NullableBoolConverter))]
        public bool? IsEnterKeyEditingEnabled
        {
            get { return (bool?)this.GetValue(IsEnterKeyEditingEnabledProperty); }
            set { this.SetValue(IsEnterKeyEditingEnabledProperty, value); }
        }

        private static void IsEnterKeyEditingEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            TreeEditingSettingsOverride settings = (TreeEditingSettingsOverride)obj;
            settings.OnPropertyChanged("IsEnterKeyEditingEnabled");
        }

        #endregion // IsEnterKeyEditingEnabled

        #region IsEnterKeyEditingEnabledResolved

        /// <summary>
        /// Resolves the <see cref="TreeEditingSettingsOverride.IsEnterKeyEditingEnabled"/> property for a particular <see cref="NodeLayout"/>
        /// </summary>
        public bool IsEnterKeyEditingEnabledResolved
        {
            get
            {
                if (this.IsEnterKeyEditingEnabled == null && this.SettingsObject != null)
                    return ((TreeEditingSettings)this.SettingsObject).IsEnterKeyEditingEnabled;
                else
                    return (bool)this.IsEnterKeyEditingEnabled;
            }
        }

        #endregion //IsEnterKeyEditingEnabledResolved

        #region IsMouseActionEditingEnabled

        /// <summary>
        /// Identifies the <see cref="IsMouseActionEditingEnabled"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsMouseActionEditingEnabledProperty = DependencyProperty.Register("IsMouseActionEditingEnabled", typeof(TreeMouseEditingAction?), typeof(TreeEditingSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(IsMouseActionEditingEnabledChanged)));

        /// <summary>
        /// Gets/Sets the <see cref="TreeMouseEditingAction"/> that can cause a <see cref="XamDataTreeNode"/> to enter edit mode, on a particular <see cref="NodeLayout"/>.
        /// </summary>
        [TypeConverter(typeof(NullableEnumTypeConverter<TreeMouseEditingAction>))]
        public TreeMouseEditingAction? IsMouseActionEditingEnabled
        {
            get { return (TreeMouseEditingAction?)this.GetValue(IsMouseActionEditingEnabledProperty); }
            set { this.SetValue(IsMouseActionEditingEnabledProperty, value); }
        }

        private static void IsMouseActionEditingEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            TreeEditingSettingsOverride settings = (TreeEditingSettingsOverride)obj;
            settings.OnPropertyChanged("IsMouseActionEditingEnabled");
        }

        #endregion // IsMouseActionEditingEnabled

        #region IsMouseActionEditingEnabledResolved

        /// <summary>
        /// Resolves the <see cref="TreeEditingSettingsOverride.IsMouseActionEditingEnabled"/> property for a particular <see cref="NodeLayout"/>
        /// </summary>
        public TreeMouseEditingAction IsMouseActionEditingEnabledResolved
        {
            get
            {
                if (this.IsMouseActionEditingEnabled == null && this.SettingsObject != null)
                    return ((TreeEditingSettings)this.SettingsObject).IsMouseActionEditingEnabled;
                else
                    return (TreeMouseEditingAction)this.IsMouseActionEditingEnabled;
            }
        }

        #endregion //IsMouseActionEditingEnabledResolved

        #region IsOnNodeActiveEditingEnabled

        /// <summary>
        /// Identifies the <see cref="IsOnNodeActiveEditingEnabled"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty IsOnNodeActiveEditingEnabledProperty = DependencyProperty.Register("IsOnCellActiveEditingEnabled", typeof(bool?), typeof(TreeEditingSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(IsOnNodeActiveEditingEnabledChanged)));


        /// <summary>
        /// Gets/Sets if a <see cref="XamDataTreeNode"/> will enter edit mode when it becomes active, on a particular <see cref="NodeLayout"/>.
        /// </summary>
        [TypeConverter(typeof(NullableBoolConverter))]
        public bool? IsOnNodeActiveEditingEnabled
        {
            get { return (bool?)this.GetValue(IsOnNodeActiveEditingEnabledProperty); }
            set { this.SetValue(IsOnNodeActiveEditingEnabledProperty, value); }
        }

        private static void IsOnNodeActiveEditingEnabledChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            TreeEditingSettingsOverride settings = (TreeEditingSettingsOverride)obj;
            settings.OnPropertyChanged("IsOnNodeActiveEditingEnabled");
        }

        #endregion // IsOnNodeActiveEditingEnabled

        #region IsOnCellActiveEditingEnabledResolved

        /// <summary>
        /// Resolves the <see cref="TreeEditingSettingsOverride.IsOnNodeActiveEditingEnabled"/> property for a particular <see cref="NodeLayout"/>
        /// </summary>
        public bool IsOnNodeActiveEditingEnabledResolved
        {
            get
            {
                if (this.IsOnNodeActiveEditingEnabled == null && this.SettingsObject != null)
                    return ((TreeEditingSettings)this.SettingsObject).IsOnNodeActiveEditingEnabled;
                else
                    return (bool)this.IsOnNodeActiveEditingEnabled;
            }
        }

        #endregion //IsOnCellActiveEditingEnabledResolved

        #region AllowDeletion

        /// <summary>
        /// Identifies the <see cref="AllowDeletion"/> dependency property. 
        /// </summary>
        public static readonly DependencyProperty AllowDeletionProperty = DependencyProperty.Register("AllowDeletion", typeof(bool?), typeof(TreeEditingSettingsOverride), new PropertyMetadata(null, new PropertyChangedCallback(AllowDeletionChanged)));

        /// <summary>
        /// Gets / sets if deleting records is allowed on this <see cref="NodeLayout"/> of the <see cref="XamDataTree"/>.
        /// </summary>
        public bool? AllowDeletion
        {
            get { return (bool?)this.GetValue(AllowDeletionProperty); }
            set { this.SetValue(AllowDeletionProperty, value); }
        }

        private static void AllowDeletionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            TreeEditingSettingsOverride ctrl = (TreeEditingSettingsOverride)obj;
            ctrl.OnPropertyChanged("AllowDeletion");
        }

        #endregion // AllowDeletion


        #region AllowDeletionResolved

        /// <summary>
        /// Resolves the <see cref="TreeEditingSettingsOverride.AllowDeletion"/> property for a particular <see cref="NodeLayout"/>.
        /// </summary>
        public bool AllowDeletionResolved
        {
            get
            {
                if (this.AllowDeletion == null && this.SettingsObject != null)
                    return ((TreeEditingSettings)this.SettingsObject).AllowDeletion;
                else
                    return (bool)this.AllowDeletion;
            }
        }
        #endregion // AllowDeletionResolved

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