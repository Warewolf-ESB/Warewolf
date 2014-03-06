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
using System.ComponentModel;
using System.Collections.Generic;
using Infragistics.Controls.Grids.Primitives;

namespace Infragistics.Controls.Grids
{
    #region TrackIsChecked

    /// <summary>
    /// Base abstract class for items which will appear in the <see cref="DateFilterSelectionControl"/>.
    /// </summary>
    public abstract class TrackIsChecked : INotifyPropertyChanged
    {
        #region Members

        bool? _isChecked;

        #endregion // Members

        #region Properties

        #region IsChecked

        /// <summary>
        /// Gets / sets if the item is considered checked. 
        /// </summary>
        public bool? IsChecked
        {
            get
            {
                return this._isChecked;
            }
            set
            {
                if (this._isChecked != value)
                {
                    this._isChecked = value;

                    if (this.Parent != null)
                        this.Parent.InvalidateIsChecked();

                    this.InvalidateChildren(value);

                    this.OnPropertyChanged("IsChecked");
                }
            }
        }

        #endregion // IsChecked

        #region Parent

        /// <summary>
        /// Gets / sets the <see cref="TrackIsChecked"/> object which is the parent of this object.
        /// </summary>
        public TrackIsChecked Parent { get; set; }

        #endregion // Parent

        #endregion // Properties

        #region Methods

        #region ChangeSilent
        /// <summary>
        /// Changes the value of the IsChecked property without executing the secondary operations on parent and children.
        /// </summary>
        /// <param name="newValue"></param>
        protected internal virtual void ChangeSilent(bool? newValue)
        {
            this._isChecked = newValue;
            this.OnPropertyChanged("IsChecked");
        }
        #endregion // ChangeSilent

        #region InvalidateIsChecked

        /// <summary>
        /// Designed to be called by child objects so that the parent object can reevaluate it's state.
        /// </summary>
        protected internal virtual void InvalidateIsChecked()
        {

        }

        #endregion // InvalidateIsChecked

        #region InvalidateChildren

        /// <summary>
        /// Changes the children to the parent's value.
        /// </summary>
        /// <param name="newValue"></param>
        protected internal virtual void InvalidateChildren(bool? newValue)
        {

        }

        #endregion // InvalidateChildren

        #endregion // Methods

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Invoked when a property changes on the <see cref="TrackIsChecked"/> object.
        /// </summary>
        /// <param name="name">The name of the property that has changed.</param>
        protected virtual void OnPropertyChanged(string name)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        /// Fired when a property changes on the <see cref="TrackIsChecked"/>.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }

    #endregion // TrackIsChecked

    #region TrackIsCheckWithChildren

    /// <summary>
    /// Base abstract class for items which will appear in the <see cref="DateFilterSelectionControl"/>.
    /// </summary>
    public abstract class TrackIsCheckWithChildren<TChildType> : TrackIsChecked where TChildType : TrackIsChecked
    {
        #region Members

        private bool _isInvalidatingChildren = false;

        private List<TChildType> _children = new List<TChildType>();

        #endregion // Members

        #region Properties

        /// <summary>
        /// A List of <see cref="TrackIsChecked"/> objects.
        /// </summary>
        public List<TChildType> Children
        {
            get
            {
                return this._children;
            }
        }

        #endregion // Properties

        #region Methods

        #region InvalidateIsChecked

        /// <summary>
        /// Designed to be called by child objects so that the parent object can reevaluate it's state.
        /// </summary>
        protected internal override void InvalidateIsChecked()
        {
            if (_isInvalidatingChildren)
                return;

            if (this._children.Count == 0)
                return;

            bool trueFound = false;
            bool falseFound = false;
            bool nullFound = false;

            foreach (TChildType child in this._children)
            {
                if (child.IsChecked == true)
                {
                    trueFound = true;
                }
                else if (child.IsChecked == false)
                {
                    falseFound = true;
                }
                else
                {
                    nullFound = true;
                }

                if (trueFound && falseFound)
                {
                    this.ChangeSilent(null);
                    return;
                }
            }

            if (trueFound && !falseFound && !nullFound)
                this.ChangeSilent(true);
            else if (!trueFound && falseFound && !nullFound)
                this.ChangeSilent(false);
            else
                this.ChangeSilent(null);
        }

        #endregion // InvalidateIsChecked

        #region InvalidateChildren

        /// <summary>
        /// Changes the children to the parent's value.
        /// </summary>
        /// <param name="newValue"></param>
        protected internal override void InvalidateChildren(bool? newValue)
        {
            this._isInvalidatingChildren = true;
            foreach (TChildType child in this._children)
            {
                child.IsChecked = newValue;
            }
            this._isInvalidatingChildren = false;
        }

        #endregion // InvalidateChildren

        #endregion // Methods
    }

    #endregion // TrackIsCheckWithChildren

    #region XamGridFilterDate

    /// <summary>
    /// Class which represents dates in the <see cref="DateFilterSelectionControl"/>.
    /// </summary>
    public class XamGridFilterDate : TrackIsCheckWithChildren<XamGridFilterDate>
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="XamGridFilterDate"/> class.
        /// </summary>
        public XamGridFilterDate()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XamGridFilterDate"/> class.
        /// </summary>
        /// <param name="date"></param>
        public XamGridFilterDate(DateTime date)
        {
            this.Date = date;
            this.DateFilterObjectType = DateFilterObjectType.Date;
        }

        #endregion // Constructor

        #region Members
        bool _isExpanded;
        #endregion // Members

        #region Properties

        #region Date

        /// <summary>
        /// Gets / sets the Date that is represented by this instance of <see cref="XamGridFilterDate"/>.
        /// </summary>
        public DateTime Date { get; set; }

        #endregion // Date

        #region DateFilterObjectType
        /// <summary>
        /// Gets / sets the <see cref="DateFilterObjectType"/> which limit what the <see cref="Date"/> will be filtered on.
        /// </summary>
        public DateFilterObjectType DateFilterObjectType { get; set; }
        #endregion // DateFilterObjectType

        #region ContentString
        /// <summary>
        /// Gets the string that represets the <see cref="Date"/> limited by the <see cref="DateFilterObjectType"/>.
        /// </summary>
        /// <remarks>If the <see cref="DateFilterObjectType"/> is set to <see cref="Infragistics.Controls.Grids.DateFilterObjectType" />.All then this will return an empty string. </remarks>
        public string ContentString
        {
            get
            {
                switch (this.DateFilterObjectType)
                {
                    case (DateFilterObjectType.Year):
                        return this.Date.Year.ToString();
                    case (DateFilterObjectType.Month):
                        return this.ContentStringMonth;
                    case (DateFilterObjectType.Date):
                        return this.Date.Day.ToString();
                    case (DateFilterObjectType.Hour):
                        return this.Date.Hour.ToString();
                    case (DateFilterObjectType.Minute):
                        return ":" + this.Date.Minute.ToString("0#");
                    case (DateFilterObjectType.Second):
                        return ":" + this.Date.Second.ToString("0#");
                    case (DateFilterObjectType.None):
                        return SRGrid.GetString("Blank");
                }
                return "";
            }
        }
        #endregion // ContentString

        #region ContentStringMonth
        /// <summary>
        /// Gets the text month of the <see cref="Date"/>.
        /// </summary>
        public string ContentStringMonth
        {
            get
            {
                if (this.DateFilterObjectType == Grids.DateFilterObjectType.None)
                {
                    return "";
                }
                return System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(this.Date.Month);
            }
        }
        #endregion // ContentStringMonth

        #region NullDate
        /// <summary>
        /// Gets if this object represents a null date object.
        /// </summary>
        public bool NullDate
        {
            get
            {
                return this.DateFilterObjectType == Grids.DateFilterObjectType.None;
            }
        }
        #endregion // NullDate

        #region IsExpanded
        
        /// <summary>
        /// Gets / sets if this object should be expanded or not.
        /// </summary>
        public bool IsExpanded
        {
            get { return this._isExpanded; }
            set
            {
                if (this._isExpanded != value)
                {
                    this._isExpanded = value;
                    this.OnPropertyChanged("IsExpanded");
                }
            }
        }
        #endregion // IsExpanded


        #endregion // Properties

        #region Methods

        #region InvalidateIsChecked

        /// <summary>
        /// Designed to be called by child objects so that the parent object can reevaluate it's state.
        /// </summary>
        protected internal override void InvalidateIsChecked()
        {
            base.InvalidateIsChecked();
            if (this.Parent != null)
            {
                this.Parent.InvalidateIsChecked();
            }
        }

        #endregion // InvalidateIsChecked

        #region GetMonthByDate

        /// <summary>
        /// Gets a <see cref="XamGridFilterDate"/> based on the inputted <see cref="DateTime"/> if the month matches.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        protected internal XamGridFilterDate GetMonthByDate(DateTime date)
        {
            int monthValue = date.Month;

            foreach (XamGridFilterDate month in this.Children)
            {
                if (monthValue == month.Date.Month)
                {
                    return month;
                }
            }

            return null;
        }

        #endregion // GetMonthByDate

        #region GetDayByDate

        /// <summary>
        /// Gets a <see cref="XamGridFilterDate"/> based on the inputted <see cref="DateTime"/> if the date matches.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        protected internal XamGridFilterDate GetDayByDate(DateTime date)
        {
            foreach (XamGridFilterDate day in this.Children)
            {
                if (date.Day == day.Date.Day)
                {
                    return day;
                }
            }

            return null;
        }

        #endregion // GetDayByDate

        #region GetHourByDate
        /// <summary>
        /// Gets a <see cref="XamGridFilterDate"/> based on the inputted <see cref="DateTime"/> if the hour matches.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public XamGridFilterDate GetHourByDate(DateTime date)
        {
            foreach (XamGridFilterDate hour in this.Children)
            {
                if (date.Hour == hour.Date.Hour)
                {
                    return hour;
                }
            }

            return null;
        }
        #endregion // GetHourByDate

        #region GetMinuteByDate
        /// <summary>
        /// Gets a <see cref="XamGridFilterDate"/> based on the inputted <see cref="DateTime"/> if the minute matches.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public XamGridFilterDate GetMinuteByDate(DateTime date)
        {
            foreach (XamGridFilterDate minute in this.Children)
            {
                if (date.Minute == minute.Date.Minute)
                {
                    return minute;
                }
            }
            return null;
        }
        #endregion // GetMinuteByDate

        #region GetSecondByDate
        /// <summary>
        /// Gets a <see cref="XamGridFilterDate"/> based on the inputted <see cref="DateTime"/> if the second matches.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public XamGridFilterDate GetSecondByDate(DateTime date)
        {
            foreach (XamGridFilterDate second in this.Children)
            {
                if (date.Second == second.Date.Second)
                {
                    return second;
                }
            }
            return null;
        }
        #endregion // GetSecondByDate

        #endregion // Methods
    }

    #endregion // XamGridFilterDate

    #region XamGridFilterYearCollection

    /// <summary>
    ///  For Internal use only. Used for managing the Unique list of items for the FilterMenu.
    /// </summary>
    public class XamGridFilterYearCollection : FilterItemCollection<XamGridFilterDate>
    {
        /// <summary>
        /// Gets the item from the collection that matches the year of the Date. Null if not found.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public XamGridFilterDate GetItemByYear(DateTime date)
        {
            int year = date.Year;

            foreach (XamGridFilterDate item in this.List)
            {
                if (item.Date.Year == year)
                {
                    return item;
                }
            }
            return null;
        }
    }

    #endregion // XamGridFilterYearCollection
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