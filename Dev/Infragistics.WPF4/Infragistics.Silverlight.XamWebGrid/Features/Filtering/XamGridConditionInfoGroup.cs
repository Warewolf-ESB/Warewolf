using System.Collections.Generic;
using System.ComponentModel;
namespace Infragistics.Controls.Grids
{
    /// <summary>
    /// A class that is used by the <see cref="Infragistics.Controls.Grids.Primitives.CompoundFilterDialogControl"/> to show the data on that form.
    /// </summary>
    /// <remarks>Not intended for general use.</remarks>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class XamGridConditionInfoGroup : INotifyPropertyChanged
    {
        #region Members

        private XamGridConditionInfoGroup _parentGroup;
        private List<XamGridConditionInfo> _infoObjects;
        List<XamGridConditionInfoGroup> _childGroups = new List<XamGridConditionInfoGroup>();
        private LogicalOperator _operator = LogicalOperator.And;
        #endregion // Members

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="XamGridConditionInfoGroup"/> class.
        /// </summary>
        public XamGridConditionInfoGroup(XamGridConditionInfoGroup parentGroup)
        {
            this.ParentGroup = parentGroup;
        }
        #endregion // Constructor

        #region Properties

        #region ParentGroup
        /// <summary>
        /// The <see cref="XamGridConditionInfoGroup"/> which this instance belongs to.
        /// </summary>
        /// <remarks
        /// The top level instance will return null for this property.
        /// </remarks>
        public XamGridConditionInfoGroup ParentGroup
        {
            get
            {
                return this._parentGroup;
            }
            set
            {
                if (this._parentGroup != value)
                {
                    if (this._parentGroup != null)
                    {
                        this._parentGroup.ChildGroups.Remove(this);
                    }

                    this._parentGroup = value;

                    if (this._parentGroup != null)
                    {
                        this._parentGroup.ChildGroups.Add(this);
                    }

                    this.OnPropertyChanged("ParentGroup");
                    this.OnPropertyChanged("Level");
                    this.OnPropertyChanged("LevelName");
                }
            }
        }
        #endregion // ParentGroup

        #region Level

        /// <summary>
        /// The depth of this <see cref="XamGridConditionInfoGroup"/>.
        /// </summary>
        /// <remarks>
        /// This property walks up the <see cref="XamGridConditionInfoGroup.ParentGroup"/> tree to figure out how many parents you have.
        /// </remarks>
        public int Level
        {
            get
            {
                int level = 0;
                XamGridConditionInfoGroup parent = this._parentGroup;
                while (parent != null)
                {
                    level++;
                    parent = parent._parentGroup;
                }
                return level;
            }
        }

        #endregion // Level

        #region Operator
        /// <summary>
        /// The <see cref="LogicalOperator"/> that binds together the <see cref="XamGridConditionInfo"/>s.
        /// </summary>
        public LogicalOperator Operator
        {
            get
            {
                return this._operator;
            }
            set
            {
                if (this._operator != value)
                {
                    this._operator = value;
                    this.OnPropertyChanged("Operator");
                }
            }
        }
        #endregion // Operator

        #region InfoObjects

        /// <summary>
        /// The list of <see cref="XamGridConditionInfo"/> objects that are related by the <see cref="XamGridConditionInfoGroup.Operator"/>
        /// </summary>
        protected internal List<XamGridConditionInfo> InfoObjects
        {
            get
            {
                if (_infoObjects == null)
                {
                    _infoObjects = new List<XamGridConditionInfo>();
                }
                return this._infoObjects;
            }
        }

        #endregion // InfoObjects

        #region ChildGroups
        /// <summary>
        /// A List of all the groups that have registered this <see cref="XamGridConditionInfoGroup"/> as it's parent.
        /// </summary>
        protected internal List<XamGridConditionInfoGroup> ChildGroups
        {
            get
            {
                return this._childGroups;
            }
        }
        #endregion // ChildGroups

        #region LevelName
        /// <summary>
        /// Gets the string representation of the Level.  This is a generated field for sorting purposes.
        /// </summary>
        public string LevelName
        {
            get
            {
                string levelName = "";
                XamGridConditionInfoGroup current = this;

                if (this._parentGroup == null)
                {
                    levelName = "0" + levelName;
                }
                else
                {
                    levelName = this.ParentGroup.LevelName + this.ParentGroup.ChildGroups.IndexOf(this);
                }

                return levelName;
            }
        }
        #endregion // LevelName

        #endregion // Properties

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Fired when a property changes on the <see cref="XamGridConditionInfoGroup"/>.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invoked when a property changes on the <see cref="XamGridConditionInfoGroup"/> object.
        /// </summary>
        /// <param propertyName="propertyName">The propertyName of the property that has changed.</param>
        protected virtual void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
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